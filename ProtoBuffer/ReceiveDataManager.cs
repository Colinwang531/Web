using Newtonsoft.Json;
using Renci.SshNet.Security;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.ProtoBuffer.Models;
using ShipWeb.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer
{
    public class ReceiveDataManager
    {
        private string shipId = "";
        private SendDataMsg manager;
        public ReceiveDataManager() {
            manager = new SendDataMsg();
            using (var context=new MyContext())
            {
                shipId = context.Ship.FirstOrDefault().Id;
            }
        }
        /// <summary>
        /// 组件处理
        /// </summary>
        /// <param name="response"></param>
        public void ComponentData(ShipWeb.ProtoBuffer.Models.Component component) 
        {
            //添加日志
            ProtoBDManager.AddReceiveLog<ShipWeb.ProtoBuffer.Models.Component>("Component", component);
            switch (component.command)
            {
                case Models.Component.Command.SIGNIN_REQ:
                    break;
                case Models.Component.Command.SIGNIN_REP:
                    if (component.componentresponse != null && component.componentresponse.result == 0)
                    {
                        if (ManagerHelp.Cid=="")
                        {
                            ProtoBDManager.ComponentAdd(component.componentresponse.cid, shipId);
                        }
                    }
                    break;
                case Models.Component.Command.SIGNOUT_REQ:
                    break;
                case Models.Component.Command.SIGNOUT_REP:
                    break;
                case Models.Component.Command.QUERY_REQ:
                    break;
                case Models.Component.Command.QUERY_REP:
                    ManagerHelp.Reponse = JsonConvert.SerializeObject(component.componentresponse);
                    if (component.componentresponse != null && component.componentresponse.result == 0)
                    {
                        ProtoBDManager.ComponentAddRange(component.componentresponse.componentinfos, shipId);
                    }
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 算法处理
        /// </summary>
        /// <param name="algorithm"></param>
        public void AlgorithmData(ShipWeb.ProtoBuffer.Models.Algorithm  algorithm)
        { 
            //添加日志
            ProtoBDManager.AddReceiveLog<ShipWeb.ProtoBuffer.Models.Algorithm>("Algorithm", algorithm);
            switch (algorithm.command)
            {
                case Models.Algorithm.Command.CONFIGURE_REQ:
                    var request = algorithm.algorithmrequest;
                    if (request.algorithminfo != null)
                    {
                        ProtoBDManager.AlgorithmSet(request.algorithminfo);
                    }
                    break;
                case Models.Algorithm.Command.CONFIGURE_REP:
                    var response = algorithm.algorithmresponse;
                    //ManagerHelp.Reponse=JsonConvert.SerializeObject(response);
                    if (response.result == 0 && response.configures != null && response.configures.Count > 0)
                    {
                        ManagerHelp.Reponse = "OK";
                    }
                    break;
                case Models.Algorithm.Command.QUERY_REQ:
                    var query = algorithm.algorithmrequest;
                    var algorithms = ProtoBDManager.AlgorithmQuery();
                    manager.SendAlgorithmRN(algorithms);
                    break;
                case Models.Algorithm.Command.QUERY_REP:
                    if (algorithm.algorithmresponse!=null&&algorithm.algorithmresponse.result==0&&algorithm.algorithmresponse.configures!=null)
                    {
                        ManagerHelp.Reponse = JsonConvert.SerializeObject(algorithm.algorithmresponse.configures);
                    }
                    break;
                default:
                    break;
            }       
        }
        /// <summary>
        /// 设备处理
        /// </summary>
        /// <param name="device"></param>
        public void DeviceData(ShipWeb.ProtoBuffer.Models.Device device)
        {
            //添加日志
            ProtoBDManager.AddReceiveLog<ShipWeb.ProtoBuffer.Models.Device>("Device", device);
            switch (device.command)
            {
                case Models.Device.Command.NEW_REQ:
                    if (device.devicerequest!=null)
                    {
                        ProtoBDManager.DeviceAdd(device.devicerequest.deviceinfo);
                    }
                    break;
                case Models.Device.Command.NEW_REP:
                    //ManagerHelp.Reponse = JsonConvert.SerializeObject(device.deviceresponse);
                    if (device.deviceresponse.result == 0 && device.deviceresponse.deviceinfos != null)
                    {
                        ProtoBDManager.DeviceAdd(device.deviceresponse.deviceinfos[0]);
                    }
                    break;
                case Models.Device.Command.DELETE_REQ:
                    if (device.devicerequest!=null&&!string.IsNullOrEmpty(device.devicerequest.did))
                    {
                        ProtoBDManager.DeviceDelete(device.devicerequest.did);
                    }
                    break;
                case Models.Device.Command.DELETE_REP:
                    if (device.deviceresponse.result == 0 )
                    {
                        ProtoBDManager.DeviceDelete(device.deviceresponse.did);
                    }
                    break;
                case Models.Device.Command.MODIFY_REQ:
                    if (device.devicerequest!=null&&!string.IsNullOrEmpty(device.devicerequest.did))
                    {
                        ProtoBDManager.DeviceUpdate(device.devicerequest.did, device.devicerequest.deviceinfo);
                    }
                    break;
                case Models.Device.Command.MODIFY_REP:
                    if (device.deviceresponse.result == 0 && device.deviceresponse.deviceinfos != null)
                    {
                        ProtoBDManager.DeviceUpdate(device.deviceresponse.did, device.deviceresponse.deviceinfos[0]);
                    }
                    break;
                case Models.Device.Command.QUERY_REQ:
                    var info = device.devicerequest == null ? null : device.devicerequest.deviceinfo;
                    string did = device.devicerequest != null ? device.devicerequest.did : "";
                    var devices=ProtoBDManager.DeviceQuery(info, did);
                    manager.SendDeviceRN(did, devices);
                    break;
                case Models.Device.Command.QUERY_REP:
                    if (device.deviceresponse!=null&&device.deviceresponse.result==0&&device.deviceresponse.deviceinfos!=null)
                    {
                        ManagerHelp.Reponse = JsonConvert.SerializeObject(device.deviceresponse.deviceinfos); 
                    }
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 船员处理
        /// </summary>
        /// <param name="crew"></param>
        public void CrewData(ShipWeb.ProtoBuffer.Models.Crew crew) 
        {
            switch (crew.command)
            {
                case Models.Crew.Command.NEW_REQ:
                    if (crew.crewrequest != null)
                    {
                        ProtoBDManager.CrewAdd(crew.crewrequest.crewinfo);
                    }
                    break;
                case Models.Crew.Command.DELETE_REQ:
                    if (crew.crewrequest != null)
                    {
                        ProtoBDManager.CrewDelete(crew.crewrequest.crewinfo.uid);
                    }
                    break;
                case Models.Crew.Command.MODIFY_REQ:
                    if (crew.crewrequest != null)
                    {
                        ProtoBDManager.CrewUpdate(crew.crewrequest.crewinfo);
                    }
                    break;
                case Models.Crew.Command.QUERY_REQ:
                    string uid = crew.crewrequest != null && crew.crewrequest.crewinfo != null ? crew.crewrequest.crewinfo.uid : "";
                    var crews = ProtoBDManager.CrewQuery(uid);
                    manager.SendCrewRN(crews);
                    break;
                case Models.Crew.Command.NEW_REP:
                    if (crew.crewresponse!=null&&crew.crewresponse.result==0)
                    {

                    }
                    break;
                case Models.Crew.Command.DELETE_REP:
                    break;
                case Models.Crew.Command.MODIFY_REP:
                    break;
                case Models.Crew.Command.QUERY_REP:
                    if (crew.crewresponse!=null&&crew.crewresponse.result==0&&crew.crewresponse.crewinfos!=null)
                    {
                        ManagerHelp.Reponse = JsonConvert.SerializeObject(crew.crewresponse.crewinfos);
                    }
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 船舶状态
        /// </summary>
        /// <param name="status"></param>
        public void StatusData(ShipWeb.ProtoBuffer.Models.Status status) 
        {
            //添加日志
            ProtoBDManager.AddReceiveLog<ShipWeb.ProtoBuffer.Models.Status>("Status", status);
            switch (status.command)
            {
                case Status.Command.SET_REQ:
                    if (status.statusrequest != null)
                    {
                        ProtoBDManager.ShipSet(status.statusrequest);
                    }
                    break;
                case Status.Command.SET_REP:
                    if (status.statusresponse!=null)
                    {
                        ProtoBDManager.ShipSet(status.statusresponse.flag);
                    }
                    break;
                case Status.Command.QUERY_REQ:
                    var ship=ProtoBDManager.StatusQuery();
                    manager.SendStatusRN(ship);
                    break;
                case Status.Command.QUERY_REP:
                    if (status.statusresponse!=null)
                    {
                        ManagerHelp.Reponse = JsonConvert.SerializeObject(status.statusresponse);
                    }
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 报警消息
        /// </summary>
        public void AlarmData()
        {
            var alarmInfos=ProtoBDManager.GetAlarmInfo(DateTime.Now, DateTime.Now.AddDays(1));
            foreach (var item in alarmInfos)
            {
                manager.SendAlarm("", item);
            };
           
        }
    }
}
