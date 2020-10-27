using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Renci.SshNet.Security;
using Smartweb.Hubs;
using SmartWeb.DB;
using SmartWeb.Models;
using SmartWeb.ProtoBuffer.Models;
using SmartWeb.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer
{
    public class ReceiveDataManager
    {
        private SendDataMsg manager=null;
        private readonly IHubContext<AlarmVoiceHub> hubContext;
        public ReceiveDataManager(IHubContext<AlarmVoiceHub> _hubContext) {
            this.hubContext = _hubContext;
            manager = new SendDataMsg(hubContext);
        }
        /// <summary>
        /// 组件处理
        /// </summary>
        /// <param name="response"></param>
        public void ComponentData(SmartWeb.ProtoBuffer.Models.Component component)
        {
            //添加日志
            ProtoBDManager.AddReceiveLog<SmartWeb.ProtoBuffer.Models.Component>("Component/" + Enum.GetName(typeof(Models.Component.Command), Convert.ToInt32(component.command)), component);
            ManagerHelp.ComponentReponse = "";
            switch (component.command)
            {
                case Models.Component.Command.SIGNIN_REQ:
                    break;
                case Models.Component.Command.SIGNIN_REP:
                    if (component.componentresponse != null && component.componentresponse.result == 0)
                    {
                        if (ManagerHelp.Cid == "")
                        {
                            ProtoBDManager.ComponentAdd(component.componentresponse.cid);
                        }
                    }
                    if (component.componentresponse.result != 0) ManagerHelp.ComponentReponse = component.componentresponse.result.ToString();
                    //心跳是否有响应
                    if (ManagerHelp.SendCount>0)ManagerHelp.SendCount--;
                    break;
                case Models.Component.Command.SIGNOUT_REQ:
                    break;
                case Models.Component.Command.SIGNOUT_REP:
                    break;
                case Models.Component.Command.QUERY_REQ:
                    break;
                case Models.Component.Command.QUERY_REP:
                    List<ComponentInfo> infos = new List<ComponentInfo>();
                    if (component.componentresponse != null && component.componentresponse.result == 0)
                    {
                        ProtoBDManager.ComponentUpdateRange(component.componentresponse.componentinfos);
                        infos = component.componentresponse.componentinfos;
                    }
                    //陆地端查询组件时返回已在线船舶（XMQ陆地端的船舶）
                    if (!infos.Where(c=>c.type==ComponentInfo.Type.XMQ).Any())
                    {
                        ManagerHelp.ComponentReponse = JsonConvert.SerializeObject(component.componentresponse);
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
        public void AlgorithmData(SmartWeb.ProtoBuffer.Models.Algorithm  algorithm)
        { 
            //添加日志
            ProtoBDManager.AddReceiveLog<SmartWeb.ProtoBuffer.Models.Algorithm>("Algorithm/"+Enum.GetName(typeof(Models.Component.Command), Convert.ToInt32(algorithm.command)), algorithm);
            switch (algorithm.command)
            {
                case Models.Algorithm.Command.CONFIGURE_REQ:
                    #region 陆地端发送算法设置请求
                    var request = algorithm.algorithmrequest;
                    if (request == null)
                    {
                        manager.SendAlgorithmRN(Models.Algorithm.Command.CONFIGURE_REP, null, 1);
                        break; 
                    }
                    string devcid = request.algorithminfo.cid;
                    var cid = devcid.Split(":")[1];
                    request.algorithminfo.cid = cid;
                    int result = ProtoBDManager.AlgorithmSet(request.algorithminfo);
                    if (result==0)
                    {
                        if (request.algorithminfo.type != AlgorithmInfo.Type.CAPTURE) 
                        {
                            string name = Enum.GetName(typeof(AlgorithmType), request.algorithminfo.type);
                            string identity = ManagerHelp.GetShipToId(ComponentType.AI, name);
                            if (identity != "")
                            {
                                request.algorithminfo.cid = devcid;
                                //向组件发送算法请求
                                manager.SendAlgorithmSet(request.algorithminfo, identity);
                                ManagerHelp.UpSend.Add("Algorithm");
                            }
                            else
                            {
                                manager.SendAlgorithmRN(Models.Algorithm.Command.CONFIGURE_REP, null, 1);
                            }
                        }
                        else
                        {
                            //向陆地端响应算法请求
                            manager.SendAlgorithmRN(Models.Algorithm.Command.CONFIGURE_REP, null, result);
                        }
                    }
                    else
                    { //向陆地端响应算法请求
                        manager.SendAlgorithmRN(Models.Algorithm.Command.CONFIGURE_REP, null, result);
                    }
                    break;
                    #endregion
                case Models.Algorithm.Command.QUERY_REQ:
                    #region 陆地端发送算法查询请求
                    //查询船舶端数据库获取所有算法数据
                    var algorithms = ProtoBDManager.AlgorithmQuery();
                    //返回给陆地端
                    manager.SendAlgorithmRN(Models.Algorithm.Command.QUERY_REP, algorithms);
                    break;
                    #endregion
                case Models.Algorithm.Command.CONFIGURE_REP:
                    if (ManagerHelp.UpSend.Where(c => c == "Algorithm").Any())
                    {
                        //向陆地端响应请求
                        manager.SendAlgorithmRN(Models.Algorithm.Command.CONFIGURE_REP, null, algorithm.algorithmresponse.result);
                        ManagerHelp.UpSend.Remove("Algorithm");
                    }
                    else
                    {
                        ManagerHelp.AlgorithmResult = algorithm.algorithmresponse.result.ToString();
                    }
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
        public void DeviceData(SmartWeb.ProtoBuffer.Models.Device device)
        {
            //添加日志
            ProtoBDManager.AddReceiveLog<SmartWeb.ProtoBuffer.Models.Device>("Device/"+ Enum.GetName(typeof(Models.Component.Command), Convert.ToInt32(device.command)), device);
            switch (device.command)
            {
                //上游陆地端的请求
                case Models.Device.Command.NEW_REQ:
                    #region 陆地端增加设备请求
                    if (device.devicerequest!=null)
                    {
                        var model=ProtoBDManager.DeviceAdd(device.devicerequest.deviceinfo);
                        if (model!=null)
                        {
                            //获取设置的组件ID
                            string identity = ManagerHelp.GetShipToId(ManagerHelp.GetComponentType((int)model.factory));
                            if (identity != "") 
                            {
                                manager.SendDeveiceAdd(model, identity);
                                ManagerHelp.UpSend.Add(model.Id + "Add");
                            }
                            else
                            {
                                manager.SendDeviceRN(Models.Device.Command.MODIFY_REP, "", null, 1);
                            }
                        }
                    }
                    else
                    {
                        manager.SendDeviceRN(Models.Device.Command.NEW_REP, "", null, 1);
                    }
                    break;
                #endregion
                case Models.Device.Command.DELETE_REQ:
                    #region 陆地端删除设备请求
                    if (device.devicerequest != null && !string.IsNullOrEmpty(device.devicerequest.did))
                    {
                        //删除船舶端数据
                        var result=ProtoBDManager.DeviceDelete(device.devicerequest.did);
                        manager.SendDeviceRN(Models.Device.Command.DELETE_REP, device.devicerequest.did, null,result);
                    }
                    else
                    {
                        manager.SendDeviceRN(Models.Device.Command.DELETE_REP, "", null, 1);
                    }
                    break;
                    #endregion
                case Models.Device.Command.MODIFY_REQ:
                    #region 陆地端编辑设备请求
                    if (device.devicerequest != null && !string.IsNullOrEmpty(device.devicerequest.did))
                    {
                        //修改船舶端数据库信息
                        var model=ProtoBDManager.DeviceUpdate(device.devicerequest.did, device.devicerequest.deviceinfo);
                        if (model!=null)
                        {
                            //获取设置的组件ID
                            string identity =ManagerHelp.GetShipToId(ManagerHelp.GetComponentType((int)model.factory));
                            if (identity!="")
                            {
                                manager.SendDeveiceUpdate(model, identity);
                                ManagerHelp.UpSend.Add(model.Id + "Edit");
                            }
                            else
                            {
                                manager.SendDeviceRN(Models.Device.Command.MODIFY_REP, "", null, 1);
                            }                           
                        }
                        else
                        {
                            manager.SendDeviceRN(Models.Device.Command.MODIFY_REP, "", null, 1);
                        }
                    }
                    else
                    {
                        manager.SendDeviceRN(Models.Device.Command.MODIFY_REP, "", null, 1);
                    }
                    break;
                    #endregion
                case Models.Device.Command.QUERY_REQ:
                    #region 陆地端查询设备请求
                    var info = device.devicerequest == null ? null : device.devicerequest.deviceinfo;
                    string did = device.devicerequest != null ? device.devicerequest.did : "";
                    var devices = ProtoBDManager.DeviceQuery(info, did);
                    manager.SendDeviceRN(Models.Device.Command.QUERY_REP, did, devices);
                    break;
                    #endregion
                case Models.Device.Command.NEW_REP:
                    #region XMQ响应设备增加
                    did = "";
                    if (device.deviceresponse.result == 0 && device.deviceresponse.deviceinfos != null)
                    {
                        var dev = device.deviceresponse.deviceinfos[0];
                        ProtoBDManager.AddCameras(dev.camerainfos, dev.did);
                        did = dev.did;
                    }
                    if (ManagerHelp.UpSend.Where(c => c == did+"Add").Any())
                    {
                        //向陆地端响应请求
                        manager.SendDeviceRN(Models.Device.Command.NEW_REP, did);
                        ManagerHelp.UpSend.Remove(did + "Add");
                    }
                    else
                    {
                        ManagerHelp.DeviceResult = device.deviceresponse.result.ToString();
                    }
                    break;
                   #endregion
                case Models.Device.Command.DELETE_REP:
                    ManagerHelp.DeviceResult = device.deviceresponse.result.ToString();
                    break;
                case Models.Device.Command.MODIFY_REP:
                    #region XMQ响应设备修改
                    did = "";
                    if (device.deviceresponse.result == 0 && device.deviceresponse.deviceinfos != null)
                    {
                        did = device.deviceresponse.deviceinfos[0].did;
                    }
                    if (ManagerHelp.UpSend.Where(c => c == did + "Edit").Any())
                    {
                        //向陆地端响应请求
                        manager.SendDeviceRN(Models.Device.Command.MODIFY_REP, did);
                        ManagerHelp.UpSend.Remove(did + "Edit");
                    }
                    else
                    {
                        ProtoBDManager.DeviceUpdate(did, device.deviceresponse.deviceinfos[0]);
                        ManagerHelp.DeviceResult = device.deviceresponse.result.ToString();
                    }
                    break;
                   #endregion
                case Models.Device.Command.QUERY_REP:
                    if (device.deviceresponse != null && device.deviceresponse.result == 0 && device.deviceresponse.deviceinfos != null)
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
        public void CrewData(SmartWeb.ProtoBuffer.Models.Crew crew)
        {
            int result = 1;//响应状态
            switch (crew.command)
            {
                case Models.Crew.Command.NEW_REQ:
                    #region 陆地端添加船员请求
                    if (crew.crewrequest == null) manager.SendCrewRN(Models.Crew.Command.NEW_REP, null, 1);
                    else
                    {
                        var crewInfo = crew.crewrequest.crewinfo;
                        result = ProtoBDManager.CrewAdd(ref crewInfo);
                        if (result == 0)
                        {
                            string identity = ManagerHelp.GetShipToId(ComponentType.AI, ManagerHelp.FaceName);
                            if (identity != "")
                            {
                                //向组件发送船员请求
                                manager.SendCrewAdd(crewInfo, identity);
                                ManagerHelp.UpSend.Add("CrewAdd");
                            }
                            else
                            {
                                //向陆地端响应算法请求
                                manager.SendCrewRN(Models.Crew.Command.NEW_REP, null, result);
                            }
                        }
                        else
                        {
                            //向陆地端响应算法请求
                            manager.SendCrewRN(Models.Crew.Command.NEW_REP, null, result);
                        }
                    }
                    break;
                    #endregion
                case Models.Crew.Command.DELETE_REQ:
                    #region 陆地端删除船员请求
                    if (crew.crewrequest == null) manager.SendCrewRN(Models.Crew.Command.DELETE_REP, null, 1);
                    else
                    {
                        result = ProtoBDManager.CrewDelete(crew.crewrequest.crewinfo.uid);
                        if (result == 0)
                        {
                            string identity = ManagerHelp.GetShipToId(ComponentType.AI, ManagerHelp.FaceName);
                            //向组件发送船员请求
                            manager.SendCrewDelete(Convert.ToInt32(crew.crewrequest.crewinfo.uid), identity);
                            ManagerHelp.UpSend.Add("CrewDel");
                        }
                        else
                        {
                            manager.SendCrewRN(Models.Crew.Command.DELETE_REP, null, result);
                        }
                    }
                    break;
                   #endregion
                case Models.Crew.Command.MODIFY_REQ:
                    #region 陆地端修改船员请求
                    if (crew.crewrequest == null) manager.SendCrewRN(Models.Crew.Command.MODIFY_REP, null, 1);
                    else
                    {
                        result = ProtoBDManager.CrewUpdate(crew.crewrequest.crewinfo);
                        if (result == 0)
                        {
                            string identity = ManagerHelp.GetShipToId(ComponentType.AI, ManagerHelp.FaceName);
                            if (identity != "")
                            {
                                //向组件发送船员请求
                                manager.SendCrewUpdate(crew.crewrequest.crewinfo, identity);
                                ManagerHelp.UpSend.Add("CrewEdit");
                            }
                            else {
                                manager.SendCrewRN(Models.Crew.Command.MODIFY_REP, null, result);
                            }
                        }
                        else
                        {
                            manager.SendCrewRN(Models.Crew.Command.MODIFY_REP, null, result);
                        }
                    }
                    break;
                   #endregion
                case Models.Crew.Command.QUERY_REQ:
                    #region 陆地端查询船员请求
                    int uid = crew.crewrequest != null && crew.crewrequest.crewinfo != null ? Convert.ToInt32(crew.crewrequest.crewinfo.uid) : 0;
                    var crews = ProtoBDManager.CrewQuery(uid);
                    manager.SendCrewRN(Models.Crew.Command.QUERY_REP, crews);
                    break;
                   #endregion
                case Models.Crew.Command.NEW_REP:
                    if (ManagerHelp.UpSend.Where(c => c == "CrewAdd").Any())
                    {
                        //向陆地端响应请求
                        manager.SendCrewRN(Models.Crew.Command.NEW_REP, null, crew.crewresponse.result);
                        ManagerHelp.UpSend.Remove("CrewAdd");
                    }
                    else
                    {
                        ManagerHelp.CrewResult = crew.crewresponse.result.ToString();
                    }
                    break;
                case Models.Crew.Command.DELETE_REP:
                    if (ManagerHelp.UpSend.Where(c => c == "CrewDel").Any())
                    {
                        //向陆地端响应请求
                        manager.SendCrewRN(Models.Crew.Command.DELETE_REP, null, crew.crewresponse.result);
                        ManagerHelp.UpSend.Remove("CrewDel");
                    }
                    else
                    {
                        ManagerHelp.CrewResult = crew.crewresponse.result.ToString();
                    }
                    break;
                case Models.Crew.Command.MODIFY_REP:
                    if (ManagerHelp.UpSend.Where(c => c == "CrewEdit").Any())
                    {
                        //向陆地端响应请求
                        manager.SendCrewRN(Models.Crew.Command.MODIFY_REP, null, crew.crewresponse.result);
                        ManagerHelp.UpSend.Remove("CrewEdit");
                    }
                    else
                    {
                        ManagerHelp.CrewResult = crew.crewresponse.result.ToString();
                    }
                    break;
                case Models.Crew.Command.QUERY_REP:
                    if (crew.crewresponse != null && crew.crewresponse.result == 0 && crew.crewresponse.crewinfos != null)
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
        public void StatusData(SmartWeb.ProtoBuffer.Models.Status status) 
        {
            //添加日志
            ProtoBDManager.AddReceiveLog<SmartWeb.ProtoBuffer.Models.Status>("Status", status);
            switch (status.command)
            {
                case Status.Command.SET_REQ://设置船状态 
                    #region 陆地端修改船状态
                    int result = ProtoBDManager.ShipSet(status.statusrequest);
                    if (result == 0)
                    {
                        var components=ProtoBDManager.GetComponentByAI();
                        foreach (var item in components)
                        {
                            //向XMQ组件里的所有算法发送船信息
                            manager.SendStatusSet(status.statusrequest, item.Id);
                        }                       
                    }
                    manager.SendStatusRN(Status.Command.SET_REP, null, result);
                    break;
                   #endregion
                case Status.Command.QUERY_REQ:
                    #region 陆地端查询船信息
                    var ship = ProtoBDManager.StatusQuery();
                    manager.SendStatusRN(Status.Command.QUERY_REP, ship);
                    break;
                    #endregion
                case Status.Command.SET_REP://接收设备成功与否
                    ManagerHelp.StatusResult = status.statusresponse.result.ToString();
                    break;
                case Status.Command.QUERY_REP:
                    if (status.statusresponse != null)
                    {
                        ManagerHelp.StatusReponse = JsonConvert.SerializeObject(status.statusresponse);
                    }
                    break;
                default:
                    break;
            }
        }

    }
}
