
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
                var comList = context.Component.FirstOrDefault(c=>c.Type==Models.Component.ComponentType.WEB);
                if (comList!=null)
                {
                    ManagerHelp.Cid = comList.Id;
                }
                //Component();
                BoatInit();
            }
        }
        /// <summary>
        /// 船舶端组件初使化
        /// </summary>
        public static void BoatInit()
        {
            StatusRequest request = new StatusRequest();
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
                    request.type = StatusRequest.Type.SAIL;
                    request.flag = (int)model.type;
                }
                else
                {
                    request.type = StatusRequest.Type.SAIL;
                    request.flag = (int)ship.type;
                }
            }
            SendDataMsg assembly = new SendDataMsg();
            //发送组件注册请求
            assembly.SendComponentSign("WEB", ManagerHelp.Cid);
            //发送查询请求
            assembly.SendComponentQuery();

            //Task.WaitAll(DealerService.taskList.ToArray());
            //DealerService.taskList = new List<Task>();
            //string aa = ManagerHelp.Reponse;
            string result = "";
            bool flag = true;
            new TaskFactory().StartNew(() =>
            {
                while (flag)
                {
                    if (ManagerHelp.Reponse != "")
                    {
                        result = ManagerHelp.Reponse;
                        ManagerHelp.Reponse = "";
                        flag = false;
                    }
                    Thread.Sleep(100);
                }
            }).Wait(2000);
            flag = false;
            //发送船员状态
            assembly.SendStatusSet(request);
            if (result!="")
            {
                using (var cont = new MyContext())
                {
                    //查询组件信息
                    var components = cont.Component.Where(c => c.Type != Models.Component.ComponentType.WEB).ToList();
                    if (components.Count > 0)
                    {
                        //发送设备信息
                        GetDevice(components);
                        if (components.Where(c => c.Type == Models.Component.ComponentType.AI).Any())
                        {
                            //发送算法信息
                            GetAlgorithm(components.FirstOrDefault(c => c.Type == Models.Component.ComponentType.AI).Id);
                        }
                        //发送船员信息
                        GetCrew(components);
                    }
                }

            }
        }
        /// <summary>
        /// 陆地端注册
        /// </summary>
        public static void LandInit() 
        {
            SendDataMsg assembly = new SendDataMsg();
            assembly.SendComponentSign("WEB", ManagerHelp.Cid);
            assembly.SendComponentQuery();
        }
       
        /// <summary>
        /// 设备请求
        /// </summary>
        private static void GetDevice(List<Models.Component> list) 
        {
            //大华通讯ID
            string DHDIdenity = "";
            //海康通讯Id
            string HKDIdentity = "";
            if (list.Where(c => c.Type == Models.Component.ComponentType.DHD).Any())
            {
                DHDIdenity = list.FirstOrDefault(c => c.Type == Models.Component.ComponentType.DHD).Id;
            }
            if (list.Where(c => c.Type == Models.Component.ComponentType.HKD).Any())
            {
                HKDIdentity = list.FirstOrDefault(c => c.Type == Models.Component.ComponentType.HKD).Id;
            }
            SendDataMsg assembly = new SendDataMsg();
            var deviceInfos=ProtoBDManager.DeviceQuery(null);
            foreach (var item in deviceInfos)
            {
                string devIdentity = "";
                if (item.factory ==DeviceInfo.Factory.DAHUA) devIdentity = DHDIdenity;
                else if (item.factory == DeviceInfo.Factory.HIKVISION) devIdentity = HKDIdentity;
                //海康和大华组件尚未启动则不需要发送组件注册消息
                if (devIdentity == "") continue;
                assembly.SendDeveiceAdd(item, devIdentity);

            }
        }

        /// <summary>
        /// 算法请求
        /// </summary>
        private static void GetAlgorithm(string algoIdentity) 
        {
            SendDataMsg assembly = new SendDataMsg();
            var algorithmInfos=ProtoBDManager.AlgorithmQuery();
            foreach (var item in algorithmInfos)
            {
                assembly.SendAlgorithmSet(item, algoIdentity);
            }
        }
        /// <summary>
        /// 船员请求
        /// </summary>
        /// <param name="nextIdentity"></param>
        private static void GetCrew(List<Models.Component> list) 
        {
            SendDataMsg assembly = new SendDataMsg();
            var crewInfos=ProtoBDManager.CrewQuery();
            foreach (var item in crewInfos)
            {
                string nextIdentity = "";
                //查询安全帽的通讯ID
                if (list.Where(c => c.Type == Models.Component.ComponentType.XMQ).Any())
                {
                    nextIdentity = list.FirstOrDefault(c => c.Type == Models.Component.ComponentType.XMQ).Id;
                }
                //海康和大华组件尚未启动则不需要发送组件注册消息
                if (nextIdentity == "") continue;
                assembly.SendCrewAdd(item, nextIdentity);
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
                assembly.SendComponentSign("WEB", ManagerHelp.Cid);
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
                        var component = context.Component.FirstOrDefault(c => c.ShipId == itship.Id && c.Type == Models.Component.ComponentType.WEB);
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
    }
}
