
using Microsoft.AspNetCore.Http;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartWeb.DB;
using SmartWeb.Models;
using SmartWeb.Tool;
using System.Text;
using System.IO;
using SmartWeb.ProtoBuffer;
using SmartWeb.ProtoBuffer.Models;
using NuGet.Frameworks;
using System.Security;
using NetMQ.Sockets;
using SmartWeb.Helpers;
using System.Threading;
using NetMQ;
using System.Diagnostics;
using SmartWeb.Interface;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.SignalR;
using Smartweb.Hubs;

namespace SmartWeb.ProtoBuffer.Init
{
    public class InitManger
    {
        private SendDataMsg assembly = new SendDataMsg();
        /// <summary>
        /// 初使化
        /// </summary>
        public void Init()
        {
            using (var context = new MyContext())
            {
                //船舶端组件注册
                var component = context.Component.FirstOrDefault(c => c.Type == ComponentType.WEB);
                if (component != null)
                {
                    ManagerHelp.ComponentId = component.Id;
                    component.Cid = "";
                    context.Update(component);
                    context.SaveChanges();
                }
                else
                {
                    ManagerHelp.ComponentId = Guid.NewGuid().ToString();
                }
                //获取数据库默认值
                LoadDBValue();
                //组件注册
                InitData();
                //船舶端需要发送缺岗通知
                if (ManagerHelp.IsShipPort)
                {
                    LoadNotice();
                    //向IPad推送消息
                    PublisherService service = new PublisherService();
                    PlayerService player = new PlayerService();
                }
                //定时获取组件信息
                 QueryComponent();
            }
          
        }
        /// <summary>
        /// 获取数据库默认值
        /// </summary>
        /// <param name="context"></param>
        private static void LoadDBValue()
        {
            try
            {
                using (var context = new MyContext())
                {
                    var sysdic = context.SysDictionary.ToList();
                    if (sysdic.Count() > 0)
                    {
                        if (sysdic.Where(c => c.key == "ExportCompany").Any())
                        {
                            ManagerHelp.ExportCompany = sysdic.FirstOrDefault(c => c.key == "ExportCompany").value;
                        }
                        if (sysdic.Where(c => c.key == "DepartureTime").Any())
                        {
                            ManagerHelp.DepartureTime = Convert.ToInt32(sysdic.FirstOrDefault(c => c.key == "DepartureTime").value);
                        }
                    }
                    var algo = context.Algorithm.Where(c => c.Type == AlgorithmType.CAPTURE).ToList();
                    if (algo.Count > 0) ManagerHelp.LiveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    if (ManagerHelp.IsShipPort)
                    {
                        var ship = context.Ship.FirstOrDefault();
                        if (ship == null)
                        {
                            ship = new Ship()
                            {
                                Coordinate = "",
                                CrewNum = 0,
                                Flag = false,
                                Id = Guid.NewGuid().ToString(),
                                Name = "Boat",
                                type = Ship.Type.AUTO
                            };
                            context.Ship.Add(ship);
                            context.SaveChanges();
                        }
                        ManagerHelp.ShipId = ship.Id;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 组件初使化
        /// 发送组件注册
        /// 发送组件查询
        /// 收到开启组件后发送船状态设置
        /// 发送设备信息
        /// 收到设备成功消息后发送算法配置
        /// 发送船员信息
        /// </summary>
        public void InitData()
        {
            //发送组件注册请求
            assembly.SendComponentSign("WEB", "");
            Task.Factory.StartNew(state =>
            {
                while (ManagerHelp.Cid == "")
                {
                    Thread.Sleep(1000);
                }
                //发送查询请求
                assembly.SendComponentQuery(ManagerHelp.Cid);
                Task.Factory.StartNew(state =>
                {
                    while (ManagerHelp.ComponentReponse == "")
                    {
                        Thread.Sleep(1000);
                    }
                    InitStatus();
                    InitDevice();
                    ManagerHelp.ComponentReponse = "";
                    if (ManagerHelp.isInit)
                    {
                        Task.Factory.StartNew(t =>
                        {
                            while (ManagerHelp.DeviceResult == "")
                            {
                                Thread.Sleep(1000);
                            }
                            //发送算法信息
                            InitAlgorithm();
                            //发送船员信息
                            InitCrew();
                            ManagerHelp.isInit = false;
                            ManagerHelp.DeviceResult = "";
                        }, TaskCreationOptions.LongRunning);
                    }
                }, TaskCreationOptions.LongRunning);
            }, TaskCreationOptions.LongRunning);
            ManagerHelp.isInit = true;
            ManagerHelp.atWorks = new List<AtWork>();
            ManagerHelp.UpSend = new List<string>();
        }
        /// <summary>
        /// 初使化船状态
        /// </summary>
        public void InitStatus()
        {
            using (var con = new MyContext())
            {
                var components = con.Component.Where(c => c.Type == ComponentType.AI&&c.Line==0).ToList();
                if (components.Count > 0)
                {
                    var ship = con.Ship.FirstOrDefault();
                    if (ship != null)
                    {
                        foreach (var item in components)
                        {
                            assembly.SendStatusSet(ship, StatusRequest.Type.SAIL, item.Cid);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 初使化设备
        /// </summary>
        public bool InitDevice()
        {
            List<SmartWeb.Models.Component> list = new List<SmartWeb.Models.Component>();
            using (var con = new MyContext())
            {
                list = con.Component.Where(c => c.Type != ComponentType.WEB&&c.Line==0).ToList();
            }
            if (list.Count > 0)
            {
                var deviceInfos = ProtoBDManager.DeviceQuery(null);
                foreach (var item in deviceInfos)
                {
                    var type = ManagerHelp.GetComponentType((int)item.factory);
                    string toId = "";
                    if (list.Where(c => c.Type == type).Any())
                    {
                        toId = list.FirstOrDefault(c => c.Type == type).Cid;
                    }
                    //海康和大华组件尚未启动则不需要发送组件注册消息
                    if (toId == "") continue;
                    assembly.SendDeveiceAdd(item, toId);
                }
                return true;
            }
            else
            {
                ManagerHelp.isInit = false;
                return false;
            }
        }

        /// <summary>
        /// 算法请求
        /// </summary>
        public void InitAlgorithm(string typeName="")
        {
            try
            {
                using (var con = new MyContext())
                {
                    var components = con.Component.Where(c => c.Type == ComponentType.AI && c.Line == 0).ToList();
                    if (components.Count > 0)
                    {
                        var algorithmInfos = ProtoBDManager.AlgorithmQuery(false, typeName);
                        foreach (var item in algorithmInfos)
                        {
                            //配置的是缺岗类型则传入设备的通讯ID
                            if (item.type != AlgorithmInfo.Type.CAPTURE)
                            {
                                string name = Enum.GetName(typeof(AlgorithmType), Convert.ToInt32(item.type));
                                if (name == "ATTENDANCE_IN" || name == "ATTENDANCE_OUT")
                                {
                                    name = ManagerHelp.FaceName.ToUpper();
                                }
                                if (components.Where(c => c.Name.ToUpper() == name).Any())
                                {
                                    string identity = components.FirstOrDefault(c => c.Name.ToUpper() == name).Cid;
                                    assembly.SendAlgorithmSet(item, identity);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// 船员请求
        /// </summary>
        /// <param name="nextIdentity"></param>
        public void InitCrew()
        {
            using (var con = new MyContext())
            {
                var component = con.Component.FirstOrDefault(c => c.Type != ComponentType.AI && c.Name == ManagerHelp.FaceName&&c.Line==0);
                if (component != null)
                {

                    var crewInfos = ProtoBDManager.CrewQuery();
                    foreach (var item in crewInfos)
                    {
                        assembly.SendCrewAdd(item, component.Cid);
                    }
                }
            }
        }
        /// <summary>
        /// 心跳查询
        /// </summary>
        public void HeartBeat()
        {
            //if (!string.IsNullOrEmpty(ManagerHelp.Cid))
            //{
                //船舶端发送注册请求
                assembly.SendComponentSign("WEB", ManagerHelp.Cid);
                ManagerHelp.SendCount++;
                //连续3次发送心跳后未得到响应将全部组件下线
                if (ManagerHelp.SendCount == 3)
                {
                    using (var context = new MyContext())
                    {
                        var compontent = context.Component.ToList();
                        foreach (var item in compontent)
                        {
                            if (item.Type == ComponentType.WEB)
                            {
                                item.Cid = "";
                            }
                            else
                            {
                                item.Line = 1;
                            }
                        }
                        context.UpdateRange(compontent);
                        context.SaveChanges();
                    }
                    ManagerHelp.SendCount = 0;
                    //重新注册
                    ManagerHelp.Cid = "";
                    assembly.SendComponentSign("WEB", "");
            }
            //}
        }
        /// <summary>
        /// 组件退出
        /// </summary>
        public void Exit()
        {
            assembly.SendComponentExit(ManagerHelp.Cid);
        }
        /// <summary>
        /// 发送缺岗通知
        /// </summary>
        public void LoadNotice()
        {
            Task.Factory.StartNew(state =>
            {
                //获取间隔时间
                int departureTime = ManagerHelp.DepartureTime;              
                while (true)
                {
                    try
                    {
                        using (var context = new MyContext())
                        {
                            //获取船的航行状态
                            var ship = context.Ship.FirstOrDefault();
                            if (ship == null) continue;
                            DateTime dt = DateTime.Now;
                            bool flag = false;
                            if (ManagerHelp.LiveTime != "")
                            {
                                if ((dt - Convert.ToDateTime(ManagerHelp.LiveTime)).Minutes >= departureTime)
                                {
                                    flag = true;
                                }
                            }
                            //Console.WriteLine("考勤人数" + ManagerHelp.atWorks.Count + " 当前时间:" + dt + " 离岗时间：" + ManagerHelp.LiveTime);
                            //ManagerHelp.atWorks 考勤人数的集合
                            if (ship.Flag && ManagerHelp.atWorks.Count <= 0 && flag)
                            {
                                var algo = context.Algorithm.Where(c => c.Type == AlgorithmType.CAPTURE).ToList();
                                if (algo.Count == 0) continue;
                                var camares = context.Camera.Where(c => algo.Select(a => a.Cid).Contains(c.Id)).ToList();
                                foreach (var item in camares)
                                {
                                    var device = context.Device.FirstOrDefault(c => c.Id == item.DeviceId);
                                    if (device == null) continue;
                                    var component = context.Component.FirstOrDefault(c => c.Type == ManagerHelp.GetComponentType((int)device.factory));
                                    if (component == null) continue;
                                    CaptureInfo captureInfo = new CaptureInfo()
                                    {
                                        cid = item.Id+"\0",//C++ 字符串是以\0结尾的
                                        did = item.DeviceId + "\0",
                                        idx = item.Index
                                    };
                                    assembly.SendCapture(captureInfo,Event.Command.CAPTURE_JPEG_REQ,component.Cid);
                                    Console.WriteLine("发送缺岗请求成功，时间"+dt.ToString("yyyy-MM-dd HH:mm:ss"));
                                    ManagerHelp.LiveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ProtoBDManager.AddReceiveLog("ExceptionCapture","", ex.Message);
                    }
                    Thread.Sleep(1000);//单位毫秒
                }
            }, TaskCreationOptions.LongRunning);
        }
        /// <summary>
        /// 定时更新组件信息
        /// </summary>
        public void QueryComponent()
        {
            Task.Factory.StartNew(state =>
            {
                while (true)
                {
                    if (ManagerHelp.Cid != null) assembly.SendComponentQuery(ManagerHelp.Cid);
                    //每30秒查询组件
                    Thread.Sleep(30 * 1000);
                }
            }, TaskCreationOptions.LongRunning);
        }


    }
}
