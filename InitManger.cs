
using Microsoft.AspNetCore.Http;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.Tool;
using System.Text;
using System.IO;
using ShipWeb.ProtoBuffer;
using ShipWeb.ProtoBuffer.Models;
using NuGet.Frameworks;
using System.Security;
using NetMQ.Sockets;
using ShipWeb.Helpers;
using System.Threading;
using NetMQ;
using System.Diagnostics;
using ShipWeb.Interface;
using System.Reflection;

namespace ShipWeb
{
    public class InitManger
    {

        /// <summary>
        /// 初使化
        /// </summary>
        public static void Init()
        {
            using (var context = new MyContext())
            {
                //船舶端组件注册
                var comList = context.Component.FirstOrDefault(c => c.Type == ComponentType.WEB);
                if (comList != null)
                {
                    ManagerHelp.Cid = comList.Id;
                }
                var sysdic = context.SysDictionary.ToList();
                if (sysdic.Count() > 0)
                {
                    if (sysdic.Where(c => c.key == "NetMqID").Any())
                    {
                        ManagerHelp.IP = sysdic.FirstOrDefault(c => c.key == "NetMqID").value;
                    }
                    if (sysdic.Where(c => c.key == "ExportCompany").Any())
                    {
                        ManagerHelp.ExportCompany = sysdic.FirstOrDefault(c => c.key == "ExportCompany").value;
                    }
                    if (sysdic.Where(c => c.key == "DepartureTime").Any())
                    {
                        ManagerHelp.DepartureTime = sysdic.FirstOrDefault(c => c.key == "DepartureTime").value;
                    }
                    if (sysdic.Where(c => c.key == "PublisherIP").Any())
                    {
                        ManagerHelp.PublisherIP = sysdic.FirstOrDefault(c => c.key == "PublisherIP").value;
                    }
                }
                //BoatInit();
                //LandInit();

                //if (true)
                //{
                //    MusicPlay.PlaySleepMusic();
                //}
                //else if (true)
                //{

                //}


                // Test();
            }
        }
        /// <summary>
        /// 船舶端组件初使化
        /// 发送组件注册
        /// 发送组件查询
        /// 收到开启组件后发送船状态设置
        /// 发送设备信息
        /// 收到设备成功消息后发送算法配置
        /// 发送船员信息
        /// </summary>
        public static void BoatInit()
        {
            using (var context = new MyContext())
            {
                var ship = context.Ship.FirstOrDefault();
                if (ship == null)
                {
                    Models.Ship model = new Ship()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Flag = false,
                        Name = "船1",
                        type = Ship.Type.PORT
                    };
                    context.Ship.Add(model);
                    context.SaveChanges();
                }
            }
            SendDataMsg assembly = new SendDataMsg();
            //发送组件注册请求
            assembly.SendComponentSign("WEB", ManagerHelp.Cid);
            //发送查询请求
            assembly.SendComponentQuery();
            ManagerHelp.atWorks = new List<AtWork>();
            Task.Factory.StartNew(state =>
            {
                while (ManagerHelp.ComponentReponse == "")
                {
                    Thread.Sleep(100);
                }
                InitStatus();
                InitDevice();
                ManagerHelp.ComponentReponse = "";
                if (ManagerHelp.isInit)
                {
                    Task.Factory.StartNew(t =>
                    {
                        while (ManagerHelp.DeviceReponse == "")
                        {
                            Thread.Sleep(100);
                        }
                        //发送算法信息
                        InitManger.InitAlgorithm();
                        //发送船员信息
                        InitManger.InitCrew();
                        ManagerHelp.isInit = false;
                        ManagerHelp.DeviceReponse = "";
                        LoadNotice();
                    }, TaskCreationOptions.LongRunning);
                }
            }, TaskCreationOptions.LongRunning);
            ManagerHelp.isInit = true;
        }
        /// <summary>
        /// 陆地端注册
        /// </summary>
        public static void LandInit()
        {
            SendDataMsg assembly = new SendDataMsg();
            assembly.SendComponentSign("WEB", ManagerHelp.Cid);
            assembly.SendComponentQuery();
            ManagerHelp.isInit = false;
            ManagerHelp.isLandHert = true;
        }
        /// <summary>
        /// 初使化船状态
        /// </summary>
        public static void InitStatus()
        {
            using (var con = new MyContext())
            {
                var components = con.Component.Where(c => c.Type == ComponentType.AI).ToList();
                SendDataMsg assembly = new SendDataMsg();
                var ship = con.Ship.FirstOrDefault();
                StatusRequest request = new StatusRequest()
                {
                    flag = (int)ship.type,
                    type = StatusRequest.Type.SAIL
                };
                foreach (var item in components)
                {
                    assembly.SendStatusSet(request, item.Id);
                }

            }
        }
        /// <summary>
        /// 初使化设备
        /// </summary>
        public static void InitDevice()
        {
            List<Models.Component> list = new List<Models.Component>();
            using (var con = new MyContext())
            {
                list = con.Component.Where(c => c.Type != ComponentType.WEB).ToList();
            }
            if (list.Count > 0)
            {
                //大华通讯ID
                string DHDIdenity = "";
                //海康通讯Id
                string HKDIdentity = "";
                if (list.Where(c => c.Type == ComponentType.DHD).Any())
                {
                    DHDIdenity = list.FirstOrDefault(c => c.Type == ComponentType.DHD).Id;
                }
                if (list.Where(c => c.Type == ComponentType.HKD).Any())
                {
                    HKDIdentity = list.FirstOrDefault(c => c.Type == ComponentType.HKD).Id;
                }
                if (HKDIdentity == "" && DHDIdenity == "")
                {
                    return;
                }
                SendDataMsg assembly = new SendDataMsg();
                var deviceInfos = ProtoBDManager.DeviceQuery(null);
                foreach (var item in deviceInfos)
                {
                    string devIdentity = "";
                    if (item.factory == DeviceInfo.Factory.DAHUA) devIdentity = DHDIdenity;
                    else if (item.factory == DeviceInfo.Factory.HIKVISION) devIdentity = HKDIdentity;
                    //海康和大华组件尚未启动则不需要发送组件注册消息
                    if (devIdentity == "") continue;
                    assembly.SendDeveiceAdd(item, devIdentity);
                }
            }
            else
            {
                ManagerHelp.isInit = false;
            }

        }

        /// <summary>
        /// 算法请求
        /// </summary>
        public static void InitAlgorithm()
        {
            using (var con = new MyContext())
            {
                var components = con.Component.Where(c => c.Type == ComponentType.AI).ToList();
                if (components.Count > 0)
                {
                    SendDataMsg assembly = new SendDataMsg();
                    var algorithmInfos = ProtoBDManager.AlgorithmQuery();
                    foreach (var item in algorithmInfos)
                    {
                        //配置的是缺岗类型则传入设备的通讯ID
                        if (item.type == AlgorithmInfo.Type.CAPTURE)
                        {
                            var camera = con.Camera.FirstOrDefault(c => c.Id == item.cid);
                            if (camera == null) continue;
                            var device = con.Device.FirstOrDefault(c => c.Id == camera.DeviceId);
                            if (device == null) continue;
                            var comtype = ComponentType.HKD;
                            if (device.factory == Models.Device.Factory.DAHUA) comtype = ComponentType.DHD;
                            var compontent = con.Component.FirstOrDefault(c => c.Type == comtype);
                            if (compontent == null) continue;
                            assembly.SendAlgorithmSet(item, compontent.Id);
                        }
                        else
                        {
                            string name = Enum.GetName(typeof(AlgorithmType), Convert.ToInt32(item.type));
                            if (name == "ATTENDANCE_IN" || name == "ATTENDANCE_OUT")
                            {
                                name = ManagerHelp.FaceName.ToUpper();
                            }
                            if (components.Where(c => c.Name.ToUpper() == name).Any())
                            {
                                string identity = components.FirstOrDefault(c => c.Name.ToUpper() == name).Id;
                                assembly.SendAlgorithmSet(item, identity);
                            }

                        }

                    }
                }
            }
        }
        /// <summary>
        /// 船员请求
        /// </summary>
        /// <param name="nextIdentity"></param>
        public static void InitCrew()
        {
            using (var con = new MyContext())
            {
                var component = con.Component.FirstOrDefault(c => c.Type != ComponentType.AI && c.Name == ManagerHelp.FaceName);
                if (component != null)
                {
                    SendDataMsg assembly = new SendDataMsg();
                    var crewInfos = ProtoBDManager.CrewQuery();
                    foreach (var item in crewInfos)
                    {
                        assembly.SendCrewAdd(item, component.Id);
                    }
                }
            }
        }
        /// <summary>
        /// 心跳查询
        /// </summary>
        public static void HeartBeat()
        {
            if (!string.IsNullOrEmpty(ManagerHelp.Cid))
            {
                SendDataMsg assembly = new SendDataMsg();
                if (!ManagerHelp.isLandHert)
                {
                    //船舶端发送注册请求
                    assembly.SendComponentSign("WEB", ManagerHelp.Cid);
                }
                else
                {
                    //陆地端定时更新组件信息
                    assembly.SendComponentQuery();
                }
            }
        }
        /// <summary>
        /// 组件退出
        /// </summary>
        public static void Exit()
        {
            SendDataMsg assembly = new SendDataMsg();
            assembly.SendComponentExit(ManagerHelp.Cid);
        }

        /// <summary>
        /// 获取报警信息
        /// </summary>
        public static void Alarm()
        {
            #region 获取报警信息并入库
            try
            {
                SendDataMsg assembly = new SendDataMsg();
                using (var context = new MyContext())
                {
                    var ship = context.Ship.ToList();
                    foreach (var itship in ship)
                    {
                        string shipId = itship.Id;
                        string identity = Guid.NewGuid().ToString();
                        var component = context.Component.FirstOrDefault(c => c.ShipId == itship.Id && c.Type == ComponentType.WEB);
                        ProtoBuffer.Models.Alarm protoalarm = new ProtoBuffer.Models.Alarm();
                        assembly.SendAlarm(component.Id);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            #endregion
        }

        /// <summary>
        /// 发送缺岗通知
        /// </summary>
        public static void LoadNotice()
        {
            #region 测试数据
            //using (var context=new MyContext())
            //{
            //    var device = context.Device.FirstOrDefault();
            //    if (device != null)
            //    {
            //        var component = context.Component.FirstOrDefault(c => c.Type == (device.factory == Models.Device.Factory.DAHUA ? ComponentType.DHD : ComponentType.HKD));
            //        if (component != null)
            //        {
            //            SendDataMsg assembly = new SendDataMsg();
            //            CaptureInfo captureInfo = new CaptureInfo()
            //            {
            //                cid = "5bcedc31-7788-4a18-b049-e4129f27c370",
            //                did = "05a091c5-333e-43ad-8c2b-b18bc1662d1a",
            //                idx = 35
            //            };
            //            assembly.SendCapture(captureInfo, component.Id);
            //        }
            //    }
            //}
            #endregion

            Task.Factory.StartNew(state =>
            {
                SendDataMsg assembly = new SendDataMsg();
                //获取间隔时间
                int departureTime = 1;
                try
                {
                    Convert.ToInt32(ManagerHelp.DepartureTime);
                }
                catch (Exception)
                {
                }
                while (true)
                {
                    using (var context = new MyContext())
                    {
                        //获取船的航行状态
                        var ship = context.Ship.FirstOrDefault();
                        if (ship == null) continue;
                        DateTime dt = DateTime.Now;
                        //ManagerHelp.atWorks 考勤人数的集合
                        if (ship.Flag && ManagerHelp.atWorks != null && ManagerHelp.atWorks.Count <= 0)
                        {
                            var algo = context.Algorithm.Where(c => c.Type == AlgorithmType.CAPTURE);
                            if (algo.Count() > 0)
                            {
                                var camares = context.Camera.Where(c => algo.Select(a => a.Cid).Contains(c.Id));
                                foreach (var item in camares)
                                {
                                    var device = context.Device.FirstOrDefault(c => c.Id == item.DeviceId);
                                    if (device == null) continue;
                                    var component = context.Component.FirstOrDefault(c => c.Type ==ManagerHelp.GetComponentType((int)device.factory));
                                    if (component == null) continue;
                                    CaptureInfo captureInfo = new CaptureInfo()
                                    {
                                        cid = item.Id,
                                        did = item.DeviceId,
                                        idx = item.Index
                                    };
                                    assembly.SendCapture(captureInfo, component.Id);
                                }
                            }
                        }
                    }
                    Thread.Sleep(departureTime * 1000 * 60);//单位毫秒
                }
            }, TaskCreationOptions.LongRunning);
        }

        public static void Test()
        {
            using (var context = new MyContext())
            {
                PublisherService publisher = new PublisherService();
                var list = context.AttendancePicture.Take(5).ToList();
                while (true)
                {
                    foreach (var item in list)
                    {
                        //考勤类型
                        int Behavior = 1;
                        //考勤时间
                        string SignInTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                        //考勤人员
                        string EmployeeName = "张三";
                        //考勤图片
                        string PhotosBuffer = Convert.ToBase64String(item.Picture);
                        string data = Behavior + "," + SignInTime + "," + EmployeeName + "," + PhotosBuffer;
                        publisher.Send(data);
                    }
                    Thread.Sleep(10000);
                }
            }

        }
    }
}
