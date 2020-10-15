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
                var ship = context.Ship.FirstOrDefault();
                if (ship!=null)
                {
                    shipId = context.Ship.FirstOrDefault().Id;
                }
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
            ManagerHelp.ComponentReponse = "";
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
                    ManagerHelp.ComponentReponse = JsonConvert.SerializeObject(component.componentresponse);
                    //船舶端初使化后的查询
                    if (ManagerHelp.isInit)
                    {
                        if (component.componentresponse != null && component.componentresponse.result == 0)
                        {
                            ProtoBDManager.ComponentAddRange(component.componentresponse.componentinfos);
                        }
                        //发送船员状态
                        InitManger.InitStatus();
                        //发送设备信息
                        InitManger.InitDevice();
                        //初使化是不需要此值
                        ManagerHelp.ComponentReponse = "";
                    }
                    else if (ManagerHelp.isLandHert)
                    {
                        if (component.componentresponse != null && component.componentresponse.result == 0)
                        {
                            ProtoBDManager.ComponentUpdateRange(component.componentresponse.componentinfos);
                        }
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
                    int result = ProtoBDManager.AlgorithmSet(request.algorithminfo);
                    manager.SendAlgorithmRN(Models.Algorithm.Command.CONFIGURE_REP, null, result);
                    break;
                case Models.Algorithm.Command.QUERY_REQ:
                    var algorithms = ProtoBDManager.AlgorithmQuery();
                    manager.SendAlgorithmRN(Models.Algorithm.Command.QUERY_REP, algorithms);
                    break;
                case Models.Algorithm.Command.CONFIGURE_REP:
                    ManagerHelp.AlgorithmReponse = algorithm.algorithmresponse.result.ToString();
                    break;
                case Models.Algorithm.Command.QUERY_REP:
                    if (algorithm.algorithmresponse != null && algorithm.algorithmresponse.result == 0 && algorithm.algorithmresponse.configures != null)
                    {
                        ManagerHelp.AlgorithmReponse = JsonConvert.SerializeObject(algorithm.algorithmresponse.configures);
                    }
                    break;
                default:
                    break;
            }      
        }
        /// <summary>
        /// 缺岗信息
        /// </summary>
        /// <param name="captureInfo"></param>
        public void CaptureData(Event evt) 
        {
            switch (evt.command)
            {
                case Event.Command.CAPTURE_JPEG_REQ:
                    break;
                case Event.Command.CAPTURE_JPEG_REP:
                    ProtoBDManager.CaptureAdd(evt.captureinfo);
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
                        string devId=ProtoBDManager.DeviceAdd(device.devicerequest.deviceinfo);
                        manager.SendDeviceRN(Models.Device.Command.NEW_REP, devId,null,devId!=""?0:1);
                    }
                    else
                    {
                        manager.SendDeviceRN(Models.Device.Command.NEW_REP, "", null, 1);
                    }
                    break;
                case Models.Device.Command.DELETE_REQ:
                    if (device.devicerequest != null && !string.IsNullOrEmpty(device.devicerequest.did))
                    {
                        var result=ProtoBDManager.DeviceDelete(device.devicerequest.did);
                        manager.SendDeviceRN(Models.Device.Command.DELETE_REP, device.devicerequest.did, null,result);
                    }
                    else
                    {
                        manager.SendDeviceRN(Models.Device.Command.DELETE_REP, "", null, 1);
                    }
                    break;
                case Models.Device.Command.MODIFY_REQ:
                    if (device.devicerequest != null && !string.IsNullOrEmpty(device.devicerequest.did))
                    {
                        int result=ProtoBDManager.DeviceUpdate(device.devicerequest.did, device.devicerequest.deviceinfo);
                        manager.SendDeviceRN(Models.Device.Command.MODIFY_REP, device.devicerequest.deviceinfo.did, null,result);
                    }
                    else
                    {
                        manager.SendDeviceRN(Models.Device.Command.MODIFY_REP, "", null, 1);
                    }
                    break;
                case Models.Device.Command.QUERY_REQ:
                    var info = device.devicerequest == null ? null : device.devicerequest.deviceinfo;
                    string did = device.devicerequest != null ? device.devicerequest.did : "";
                    var devices = ProtoBDManager.DeviceQuery(info, did);
                    manager.SendDeviceRN(Models.Device.Command.QUERY_REP, did, devices);
                    break;
                case Models.Device.Command.NEW_REP:
                    if (device.deviceresponse.result == 0 && device.deviceresponse.deviceinfos != null)
                    {
                        var dev = device.deviceresponse.deviceinfos[0];
                        ProtoBDManager.AddCameras(dev.camerainfos,dev.did);
                        if (ManagerHelp.isInit)
                        {
                            //发送算法信息
                            InitManger.InitAlgorithm();
                            //发送船员信息
                            InitManger.InitCrew();
                            ManagerHelp.isInit = false;
                        }
                    }
                    if (!ManagerHelp.isInit)
                    {
                        ManagerHelp.DeviceReponse = device.deviceresponse.result.ToString();
                    }
                    break;
                case Models.Device.Command.DELETE_REP:
                    ManagerHelp.DeviceReponse = device.deviceresponse.result.ToString();
                    break;
                case Models.Device.Command.MODIFY_REP:
                    if (device.deviceresponse.result == 0 && device.deviceresponse.deviceinfos != null)
                    {
                        ProtoBDManager.DeviceUpdate(device.deviceresponse.deviceinfos[0].did, device.deviceresponse.deviceinfos[0]);
                    }
                    ManagerHelp.DeviceReponse = device.deviceresponse.result.ToString();
                    break;
                case Models.Device.Command.QUERY_REP:
                    if (device.deviceresponse!=null&&device.deviceresponse.result==0&&device.deviceresponse.deviceinfos!=null)
                    {
                        ManagerHelp.DeviceReponse = JsonConvert.SerializeObject(device.deviceresponse.deviceinfos); 
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
            int result = 1;//响应状态
            switch (crew.command)
            {
                case Models.Crew.Command.NEW_REQ:
                  
                    if (crew.crewrequest != null)
                    {
                        result=ProtoBDManager.CrewAdd(crew.crewrequest.crewinfo);
                    }
                    manager.SendCrewRN(Models.Crew.Command.NEW_REP, null, result);
                    break;
                case Models.Crew.Command.DELETE_REQ:
                    if (crew.crewrequest != null)
                    {
                        int id = Convert.ToInt32(crew.crewrequest.crewinfo.uid);
                         result=ProtoBDManager.CrewDelete(id);
                    }
                    manager.SendCrewRN(Models.Crew.Command.DELETE_REP, null, result);
                    break;
                case Models.Crew.Command.MODIFY_REQ:
                    if (crew.crewrequest != null)
                    {
                         result=ProtoBDManager.CrewUpdate(crew.crewrequest.crewinfo);
                    }
                    manager.SendCrewRN(Models.Crew.Command.DELETE_REP, null, result);
                    break;
                case Models.Crew.Command.QUERY_REQ:
                    int uid = crew.crewrequest != null && crew.crewrequest.crewinfo != null ?Convert.ToInt32(crew.crewrequest.crewinfo.uid) : 0;
                    var crews = ProtoBDManager.CrewQuery(uid);
                    manager.SendCrewRN(Models.Crew.Command.QUERY_REP,crews);
                    break;
                case Models.Crew.Command.NEW_REP:
                    ManagerHelp.CrewReponse = crew.crewresponse.result.ToString();
                    break;
                case Models.Crew.Command.DELETE_REP:
                    ManagerHelp.CrewReponse = crew.crewresponse.result.ToString();
                    break;
                case Models.Crew.Command.MODIFY_REP:
                    ManagerHelp.CrewReponse = crew.crewresponse.result.ToString();
                    break;
                case Models.Crew.Command.QUERY_REP:
                    if (crew.crewresponse!=null&&crew.crewresponse.result==0&&crew.crewresponse.crewinfos!=null)
                    {
                        ManagerHelp.CrewReponse = JsonConvert.SerializeObject(crew.crewresponse.crewinfos);
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
                case Status.Command.SET_REQ://设置船状态
                    int result = 1;
                    if (status.statusrequest != null)
                    {
                        result = ProtoBDManager.ShipSet(status.statusrequest);
                    }
                    manager.SendStatusRN(Status.Command.SET_REP, null, result);
                    break;
                case Status.Command.QUERY_REQ:
                    var ship = ProtoBDManager.StatusQuery();
                    manager.SendStatusRN(Status.Command.QUERY_REP, ship);
                    break;
                case Status.Command.SET_REP://接收设备成功与否

                    ManagerHelp.StatusReponse = status.statusresponse.result.ToString();
                    break;
                case Status.Command.QUERY_REP:
                    if (status.statusresponse!=null)
                    {
                        ManagerHelp.StatusReponse = JsonConvert.SerializeObject(status.statusresponse);
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
                //将船舶的报警信息发给陆地端
                manager.SendAlarm("", item);
            };
           
        }
    }
}
