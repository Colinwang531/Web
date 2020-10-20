using NuGet.Frameworks;
using ShipWeb.Interface;
using ShipWeb.ProtoBuffer.Models;
using ShipWeb.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer
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
        public void SendDeviceRN (Models.Device.Command command, string did, List<ShipWeb.Models.Device> devices = null, int status = 0)
        {
            List<DeviceInfo> list = new List<DeviceInfo>() ;
            foreach (var item in devices)
            {
                list.Add(GetDeviceInfo(item));
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
            dealer.Send(sendMsg, "upstream", "response");
        }
        /// <summary>
        /// 组合返回数据
        /// </summary>
        /// <param name="status"></param>
        public void SendStatusRN(Status.Command command, ShipWeb.Models.Ship ship, int status = 0)
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
                        name = ship == null ? "" : ship.Name,
                        result = status
                    }
                }
            };
            dealer.Send(msg, "upstream", "response");
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
                            componentid = cid
                        },
                    }
                }
            };
            dealer.Send(msg,ManagerHelp.ComponentId);
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
        public void SendCapture(CaptureInfo captureInfo, string identity)
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.EVENT,
                sequence = 11,
                timestamp = ProtoBufHelp.TimeSpan(),
                evt = new Event()
                {
                    command = Event.Command.CAPTURE_JPEG_REQ,
                    captureinfo = captureInfo
                }
            };
            dealer.Send(msg, identity);
        }
        /// <summary>
        /// 发送设备增加请求
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <param name="did"></param>
        public void SendDeveiceAdd(ShipWeb.Models.Device model, string nextIdentity)
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

        private static DeviceInfo GetDeviceInfo(ShipWeb.Models.Device model)
        {
            var device= new DeviceInfo()
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
            if (device.camerainfos!=null&&device.camerainfos.Count>0)
            {
                device.camerainfos = new List<CameraInfo>();
                foreach (var item in device.camerainfos)
                {
                    device.camerainfos.Add(new CameraInfo()
                    {
                        cid = item.cid,
                        enable = item.enable,
                        nickname = item.nickname,
                        index = item.index,
                        ip = item.ip
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
        public void SendDeveiceUpdate(ShipWeb.Models.Device model, string nextIdentity, string did = "")
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
                        did = did
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
        /// <param name="request"></param>
        public void SendStatusSet(StatusRequest request, string nextIdentity = "")
        {
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
        /// <param name="nextIdentity"></param>
        public void SendAlarm(string nextIdentity = "", AlarmInfo info = null)
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.STATUS,
                sequence = 3,
                timestamp = ProtoBufHelp.TimeSpan(),
                alarm = new Models.Alarm()
                {
                    command = Models.Alarm.Command.NOTIFY
                }
            };
            if (info != null) msg.alarm.alarminfo = info;
            dealer.Send(msg, nextIdentity);
        }
    }
}
