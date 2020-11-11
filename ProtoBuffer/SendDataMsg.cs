using Microsoft.AspNetCore.SignalR;
using NuGet.Frameworks;
using Org.BouncyCastle.Ocsp;
using Smartweb.Hubs;
using SmartWeb.Interface;
using SmartWeb.Models;
using SmartWeb.ProtoBuffer.Models;
using SmartWeb.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer
{
    public class SendDataMsg
    {
        DealerService dealer = new DealerService();
       



        /// <summary>
        /// 组合返回数据
        /// </summary>
        /// <param name="algorithms"></param>
        public void SendAlgorithmRN(Models.Algorithm.Command command, List<AlgorithmInfo> algorithms = null, int status = 0)
        {
            MSG sendMsg = new MSG()
            {
                type = MSG.Type.ALGORITHM,
                sequence = 4,
                timestamp = ProtoBufHelp.TimeSpan(),
                algorithm = new Models.Algorithm()
                {
                    command = command,
                    algorithmresponse = new AlgorithmResponse()
                    {
                        configures = algorithms,
                        result = status
                    }
                }
            };
            dealer.Send(sendMsg, ManagerHelp.UpToId, "response");
        }
        /// <summary>
        /// 组合返回数据
        /// </summary>
        /// <param name="algorithms"></param>
        public void SendDeviceRN(Models.Device.Command command, string did, List<SmartWeb.Models.Device> devices = null, int status = 0)
        {
            List<DeviceInfo> list = new List<DeviceInfo>();
            if (devices != null) 
            {
                foreach (var item in devices)
                {
                    list.Add(GetDeviceInfo(item));
                }
            }            
            MSG sendMsg = new MSG()
            {
                type = MSG.Type.DEVICE,
                sequence = 6,
                timestamp = ProtoBufHelp.TimeSpan(),
                device = new Models.Device()
                {
                    command = command,
                    deviceresponse = new DeviceResponse()
                    {
                        did = did,
                        deviceinfos = list,
                        result = status
                    }
                }
            };
            dealer.Send(sendMsg, ManagerHelp.UpToId, "response");
        }

        /// <summary>
        /// 组合返回数据
        /// </summary>
        /// <param name="algorithms"></param>
        public void SendCrewRN(Models.Crew.Command command, List<CrewInfo> crews = null, int status = 0)
        {
            MSG sendMsg = new MSG()
            {
                type = MSG.Type.CREW,
                sequence = 8,
                timestamp = ProtoBufHelp.TimeSpan(),
                crew = new Models.Crew()
                {
                    command = command,
                    crewresponse = new CrewResponse()
                    {
                        result = status,
                        crewinfos = crews
                    }
                }
            };
            dealer.Send(sendMsg, ManagerHelp.UpToId, "response");
        }
        /// <summary>
        /// 组合返回数据
        /// </summary>
        /// <param name="status"></param>
        public void SendStatusRN(Status.Command command, SmartWeb.Models.Ship ship, int status = 0)
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.STATUS,
                sequence = 3,
                timestamp = ProtoBufHelp.TimeSpan(),
                status = new Status()
                {
                    command = command,
                    statusresponse = new StatusResponse()
                    {
                        flag = ship == null ? false : ship.Flag,
                        name = ship == null ? "" : ship.Name+"|"+(int)ship.type,
                        result = status
                    }
                }
            };
            dealer.Send(msg, ManagerHelp.UpToId, "response");
        }
        /// <summary>
        /// 查询船员
        /// </summary>
        /// <param name="nextIdentity"></param>
        /// <param name="uid"></param>
        public void SendCrewQuery(string nextIdentity, int uid = 0)
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.CREW,
                sequence = 7,
                timestamp = ProtoBufHelp.TimeSpan(),
                crew = new Models.Crew()
                {
                    command = Models.Crew.Command.QUERY_REQ,
                    crewrequest = new CrewRequest()
                    {
                        crewinfo = new CrewInfo()
                        {
                            uid = uid.ToString()
                        }
                    }
                }
            };
            dealer.Send(msg, nextIdentity);
        }
        /// <summary>
        /// 添加船员
        /// </summary>
        /// <param name="nextIdentity"></param>
        /// <param name="crewinfo"></param>
        public void SendCrewAdd(CrewInfo crewinfo, string nextIdentity = "")
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.CREW,
                sequence = 3,
                timestamp = ProtoBufHelp.TimeSpan(),
                crew = new Models.Crew()
                {
                    command = Models.Crew.Command.NEW_REQ,
                    crewrequest = new CrewRequest()
                    {
                        crewinfo = crewinfo
                    }
                }
            };
            dealer.Send(msg, nextIdentity);
        }
        /// <summary>
        /// 修改船员
        /// </summary>
        /// <param name="nextIdentity"></param>
        /// <param name="crewInfo"></param>
        public void SendCrewUpdate(CrewInfo crewInfo, string nextIdentity = "")
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.CREW,
                sequence = 3,
                timestamp = ProtoBufHelp.TimeSpan(),
                crew = new Models.Crew()
                {
                    command = Models.Crew.Command.MODIFY_REQ,
                    crewrequest = new CrewRequest()
                    {
                        crewinfo = crewInfo
                    }
                }
            };
            dealer.Send(msg, nextIdentity);
        }
        /// <summary>
        /// 删除船员
        /// </summary>
        /// <param name="nextIdentity"></param>
        /// <param name="uid"></param>
        public void SendCrewDelete(int uid, string nextIdentity = "")
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.CREW,
                sequence = 3,
                timestamp = ProtoBufHelp.TimeSpan(),
                crew = new Models.Crew()
                {
                    command = Models.Crew.Command.DELETE_REQ,
                    crewrequest = new CrewRequest()
                    {
                        crewinfo = new CrewInfo()
                        {
                            uid = uid.ToString()
                        }
                    }
                }
            };
            dealer.Send(msg, nextIdentity);
        }
        /// <summary>
        /// 组件注册请求
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cid"></param>
        public void SendComponentSign(string name = "WEB", string cid = null)
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.COMPONENT,
                timestamp = ProtoBufHelp.TimeSpan(),
                sequence = 1,
                component = new Models.Component()
                {
                    command = Models.Component.Command.SIGNIN_REQ,
                    componentrequest = new ComponentRequest()
                    {
                        componentinfo = new ComponentInfo()
                        {
                            type = ComponentInfo.Type.WEB,
                            cname = name,
                            componentid = ManagerHelp.ComponentId,
                            commid = cid
                        },
                    }
                }
            };
            dealer.Send(msg, "");
        }
        /// <summary>
        /// 组件查询
        /// </summary>
        public void SendComponentQuery(string nextIdentity = "")
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.COMPONENT,
                timestamp = ProtoBufHelp.TimeSpan(),
                sequence = 1,
                component = new Models.Component()
                {
                    command = Models.Component.Command.QUERY_REQ
                }
            };
            dealer.Send(msg, nextIdentity);
        }
        /// <summary>
        /// 发送组件退出请求
        /// </summary>
        /// <param name="cid"></param>
        public void SendComponentExit(string cid)
        {
            //组件注册消息整理
            MSG msg = new MSG()
            {
                type = MSG.Type.COMPONENT,
                timestamp = ProtoBufHelp.TimeSpan(),
                sequence = 1,
                component = new Models.Component()
                {
                    command = Models.Component.Command.SIGNOUT_REQ,
                    componentrequest = new ComponentRequest()
                    {
                        componentinfo = new ComponentInfo()
                        {
                            componentid = cid
                        }
                    }
                }
            };
            //把成实转成字节流
            byte[] byt = ProtoBufHelp.Serialize<MSG>(msg);
            dealer.Send(msg);
        }
        /// <summary>
        /// 发送算法设置请求
        /// </summary>
        /// <param name="protoModel"></param>
        public void SendAlgorithmSet(AlgorithmInfo protoModel, string nextIdentity)
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.ALGORITHM,
                sequence = 3,
                timestamp = ProtoBufHelp.TimeSpan(),
                algorithm = new Models.Algorithm()
                {
                    command = Models.Algorithm.Command.CONFIGURE_REQ,
                    algorithmrequest = new AlgorithmRequest()
                    {
                        algorithminfo = protoModel
                    }
                }
            };
            dealer.Send(msg, nextIdentity);
        }
        /// <summary>
        /// 算法查询
        /// </summary>
        /// <param name="algoIdentity"></param>
        public void SendAlgorithmQuery(string algoIdentity)
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.ALGORITHM,
                sequence = 5,
                timestamp = ProtoBufHelp.TimeSpan(),
                algorithm = new Models.Algorithm()
                {
                    command = Models.Algorithm.Command.QUERY_REQ
                }
            };
            dealer.Send(msg, algoIdentity);
        }
        /// <summary>
        /// 缺岗请求
        /// </summary>
        /// <param name="captureInfo"></param>
        /// <param name="identity"></param>
        public void SendCapture(CaptureInfo captureInfo, Event.Command command, string identity,string head="request")
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.EVENT,
                sequence = 11,
                timestamp = ProtoBufHelp.TimeSpan(),
                evt = new Event()
                {
                    command = command,
                    captureinfo = captureInfo
                }
            };
            dealer.Send(msg, identity,head);
        }
        /// <summary>
        /// 发送设备增加请求
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <param name="did"></param>
        public void SendDeveiceAdd(SmartWeb.Models.Device model, string nextIdentity)
        {
            DeviceInfo deviceInfo = GetDeviceInfo(model);
            MSG msg = new MSG()
            {
                type = MSG.Type.DEVICE,
                sequence = 5,
                timestamp = ProtoBufHelp.TimeSpan(),
                device = new Models.Device()
                {
                    command = Models.Device.Command.NEW_REQ,
                    devicerequest = new DeviceRequest()
                    {
                        deviceinfo = deviceInfo
                    }
                }
            };
            dealer.Send(msg, nextIdentity);
        }

        private static DeviceInfo GetDeviceInfo(SmartWeb.Models.Device model)
        {
            var device = new DeviceInfo()
            {
                did = model.Id,
                factory = (DeviceInfo.Factory)model.factory,
                ip = model.IP,
                name = model.Name,
                nickname = model.Nickname,
                password = model.Password,
                enable = model.Enable,
                port = model.Port,
                type = (DeviceInfo.Type)model.type
            };
            if (model.CameraModelList != null && model.CameraModelList.Count > 0)
            {
                device.camerainfos = new List<CameraInfo>();
                foreach (var item in model.CameraModelList)
                {
                    device.camerainfos.Add(new CameraInfo()
                    {
                        cid = item.Id,
                        enable = item.Enable,
                        nickname = item.NickName,
                        index = item.Index,
                        ip = item.IP
                    });
                }
            }
            return device;
        }

        /// <summary>
        /// 发送设备修改请求
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <param name="did"></param>
        public void SendDeveiceUpdate(SmartWeb.Models.Device model, string nextIdentity, string did = "")
        {
            DeviceInfo deviceInfo = GetDeviceInfo(model);
            MSG msg = new MSG()
            {
                type = MSG.Type.DEVICE,
                sequence = 5,
                timestamp = ProtoBufHelp.TimeSpan(),
                device = new Models.Device()
                {
                    command = Models.Device.Command.MODIFY_REQ,
                    devicerequest = new DeviceRequest()
                    {
                        deviceinfo = deviceInfo,
                        did = did
                    }
                }
            };
            dealer.Send(msg, nextIdentity);
        }
        /// <summary>
        /// 发送设备删除请求
        /// </summary>
        /// <param name="did"></param>
        public void SendDeveiceDelete(string nextIdentity, string did)
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.DEVICE,
                sequence = 5,
                timestamp = ProtoBufHelp.TimeSpan(),
                device = new Models.Device()
                {
                    command = Models.Device.Command.DELETE_REQ,
                    devicerequest = new DeviceRequest()
                    {
                        deviceinfo=new DeviceInfo() { 
                             did=did,
                             enable=true
                        }
                    }
                }
            };
            dealer.Send(msg, nextIdentity);
        }
        /// <summary>
        /// 设备查询
        /// </summary>
        public void SendDeveiceQuery(string devIdentity, string did = "")
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.DEVICE,
                sequence = 5,
                timestamp = ProtoBufHelp.TimeSpan(),
                device = new Models.Device()
                {
                    command = Models.Device.Command.QUERY_REQ
                }
            };
            if (did != "")
            {
                msg.device.devicerequest = new DeviceRequest()
                {
                    did = did
                };
            }
            dealer.Send(msg, devIdentity);
        }
        /// <summary>
        /// 发送船舶状态修改请求
        /// </summary>
        /// <param name="ship">船舶信息</param>
        /// <param name="type">StatusRequest.Type.SAIL</param>
        /// <param name="request"></param>
        public void SendStatusSet(Ship ship, StatusRequest.Type type, string nextIdentity = "")
        {
            if (ship == null) return;
            StatusRequest request = new StatusRequest();
            request.type = type;
            if (type == StatusRequest.Type.NAME) {
                request.text = ship.Name;
            }
            else if (type==StatusRequest.Type.SAIL)
            {
                request.flag =ship.Flag?0:1;
            }
            MSG msg = new MSG()
            {
                type = MSG.Type.STATUS,
                sequence = 3,
                timestamp = ProtoBufHelp.TimeSpan(),
                status = new Status()
                {
                    command = Status.Command.SET_REQ,
                    statusrequest = request
                }
            };
            dealer.Send(msg, nextIdentity);
        }
        /// <summary>
        /// 查询船员状态信息
        /// </summary>
        /// <param name="nextIdentity"></param>
        public void SendStatusQuery(string nextIdentity = "")
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.STATUS,
                sequence = 3,
                timestamp = ProtoBufHelp.TimeSpan(),
                status = new Status()
                {
                    command = Status.Command.QUERY_REQ
                }
            };
            dealer.Send(msg, nextIdentity);
        }
        /// <summary>
        /// 获取报警消息
        /// </summary>
        /// <param name="head"></param>
        public void SendAlarm(string head = "upload", AlarmInfo info = null,string toId="")
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.ALARM,
                sequence = 11,
                timestamp = ProtoBufHelp.TimeSpan(),
                alarm = new Models.Alarm()
                {
                    command = Models.Alarm.Command.NOTIFY,
                    alarminfo=info
                }
            };
            dealer.Send(msg, toId, head);
        }
    }
}
