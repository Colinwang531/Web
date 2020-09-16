﻿using ShipWeb.Interface;
using ShipWeb.ProtoBuffer.Models;
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
        public void SendAlgorithmRN(List<AlgorithmInfo> algorithms)
        {
            MSG sendMsg = new MSG()
            {
                type = MSG.Type.ALGORITHM,
                sequence = 4,
                timestamp = ProtoBufHelp.TimeSpan(),
                algorithm = new Algorithm()
                {
                    command = Algorithm.Command.QUERY_REP,
                    algorithmresponse = new AlgorithmResponse()
                    {
                        configures = algorithms,
                        result = 0
                    }
                }
            };
            dealer.Send(sendMsg);
        }
        /// <summary>
        /// 组合返回数据
        /// </summary>
        /// <param name="algorithms"></param>
        public void SendDeviceRN(string did, List<DeviceInfo> devices)
        {
            MSG sendMsg = new MSG()
            {
                type = MSG.Type.DEVICE,
                sequence = 6,
                timestamp = ProtoBufHelp.TimeSpan(),
                device = new Device()
                {
                    command = Device.Command.QUERY_REP,
                    deviceresponse = new DeviceResponse()
                    {
                        did = did,
                        deviceinfos = devices,
                        result = 0
                    }
                }
            };
            dealer.Send(sendMsg);
        }
        /// <summary>
        /// 组合返回数据
        /// </summary>
        /// <param name="algorithms"></param>
        public void SendCrewRN(List<CrewInfo> crews)
        {
            MSG sendMsg = new MSG()
            {
                type = MSG.Type.CREW,
                sequence = 8,
                timestamp = ProtoBufHelp.TimeSpan(),
                crew = new Crew()
                {
                    command = Crew.Command.QUERY_REP,
                    crewresponse = new CrewResponse()
                    {
                        result = 0,
                        crewinfos = crews
                    }
                }
            };
            dealer.Send(sendMsg);
        }
        /// <summary>
        /// 组合返回数据
        /// </summary>
        /// <param name="status"></param>
        public void SendStatusRN(ShipWeb.Models.Ship ship) 
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.STATUS,
                sequence = 3,
                timestamp = ProtoBufHelp.TimeSpan(),
                status = new Status()
                {
                    command = Status.Command.QUERY_REP,
                    statusresponse = new StatusResponse() {
                        flag = ship.Flag,
                        name = ship.Name,
                        result = 0
                    }
                }
            };
            dealer.Send(msg);
        }
        /// <summary>
        /// 查询船员
        /// </summary>
        /// <param name="nextIdentity"></param>
        /// <param name="uid"></param>
        public void SendCrewQuery(string nextIdentity,string uid="")
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.CREW,
                sequence = 7,
                timestamp = ProtoBufHelp.TimeSpan(),
                crew = new Crew()
                {
                    command = Crew.Command.QUERY_REQ,
                    crewrequest = new CrewRequest()
                    {
                        crewinfo = new CrewInfo()
                        {
                            uid = uid
                        }
                    }
                }
            };
            dealer.Send(msg,nextIdentity);
        }
        /// <summary>
        /// 添加船员
        /// </summary>
        /// <param name="nextIdentity"></param>
        /// <param name="crewinfo"></param>
        public void SendCrewAdd(CrewInfo crewinfo, string nextIdentity="") 
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.CREW,
                sequence = 3,
                timestamp = ProtoBufHelp.TimeSpan(),
                crew = new Crew()
                {
                    command = Crew.Command.NEW_REQ,
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
        public void SendCrewUpdate(CrewInfo crewInfo,string nextIdentity="") 
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.CREW,
                sequence = 3,
                timestamp = ProtoBufHelp.TimeSpan(),
                crew = new Crew()
                {
                    command = Crew.Command.MODIFY_REQ,
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
        public void SendCrewDelete(string uid,string nextIdentity="")
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.CREW,
                sequence = 3,
                timestamp = ProtoBufHelp.TimeSpan(),
                crew = new Crew()
                {
                    command = Crew.Command.DELETE_REQ,
                    crewrequest = new CrewRequest()
                    {
                        crewinfo = new CrewInfo()
                        {
                            uid = uid
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
        public void SendComponentSign(string name="WEB",string cid="")
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.COMPONENT,
                timestamp = ProtoBufHelp.TimeSpan(),
                sequence = 1,
                component = new Component()
                {
                    command = Component.Command.SIGNIN_REQ,
                    componentrequest = new ComponentRequest()
                    {
                        componentinfo = new ComponentInfo()
                        {
                            type = ComponentInfo.Type.WEB,
                            cname = name
                        },
                    }
                }
            };
            if (!string.IsNullOrEmpty(cid)) msg.component.componentrequest.componentinfo.cid = cid;
            dealer.Send(msg);
        }
        /// <summary>
        /// 组件查询
        /// </summary>
        public void SendComponentQuery(string nextIdentity="")
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.COMPONENT,
                timestamp = ProtoBufHelp.TimeSpan(),
                sequence = 1,
                component = new Component()
                {
                    command = Component.Command.QUERY_REQ
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
                component = new Component()
                {
                    command = Component.Command.SIGNOUT_REQ,
                    componentrequest = new ComponentRequest()
                    {
                        componentinfo = new ComponentInfo()
                        {
                            cid = cid
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
        public void SendAlgorithmSet(AlgorithmInfo protoModel,string nextIdentity) 
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.ALGORITHM,
                sequence = 3,
                timestamp = ProtoBufHelp.TimeSpan(),
                algorithm = new Algorithm()
                {
                    command = Algorithm.Command.CONFIGURE_REQ,
                    algorithmrequest = new AlgorithmRequest()
                    {
                        algorithminfo = protoModel
                    }
                }
            };
            dealer.Send(msg,nextIdentity);
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
                sequence = 3,
                timestamp = ProtoBufHelp.TimeSpan(),
                algorithm = new Algorithm()
                {
                    command = Algorithm.Command.QUERY_REQ
                }
            };
            dealer.Send(msg, algoIdentity);
        }
        /// <summary>
        /// 发送设备增加请求
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <param name="did"></param>
        public void SendDeveiceAdd(DeviceInfo deviceInfo,string nextIdentity) 
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.DEVICE,
                sequence = 5,
                timestamp = ProtoBufHelp.TimeSpan(),
                device = new Device()
                {
                    command = Device.Command.NEW_REQ,
                    devicerequest = new DeviceRequest()
                    {
                        deviceinfo = deviceInfo
                    }
                }
            };
            dealer.Send(msg,nextIdentity);
        }
        /// <summary>
        /// 发送设备修改请求
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <param name="did"></param>
        public void SendDeveiceUpdate(DeviceInfo deviceInfo, string nextIdentity, string did = "")
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.DEVICE,
                sequence = 5,
                timestamp = ProtoBufHelp.TimeSpan(),
                device = new Device()
                {
                    command = Device.Command.MODIFY_REQ,
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
        public void SendDeveiceDelete(string nextIdentity,string did) {
            MSG msg = new MSG()
            {
                type = MSG.Type.DEVICE,
                sequence = 5,
                timestamp = ProtoBufHelp.TimeSpan(),
                device = new Device()
                {
                    command = Device.Command.DELETE_REQ,
                    devicerequest = new DeviceRequest()
                    {
                        did = did
                    }
                }
            };
            dealer.Send(msg,nextIdentity);
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
                device = new Device()
                {
                    command = Device.Command.QUERY_REQ
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
        public void SendStatusSet(StatusRequest request,string nextIdentity="") 
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
            dealer.Send(msg,nextIdentity);
        }
        /// <summary>
        /// 查询船员状态信息
        /// </summary>
        /// <param name="nextIdentity"></param>
        public void SendStatusQuery(string nextIdentity="")
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
        public void SendAlarm(string nextIdentity="", AlarmInfo info=null)
        {
            MSG msg = new MSG()
            {
                type = MSG.Type.STATUS,
                sequence = 3,
                timestamp = ProtoBufHelp.TimeSpan(),
                alarm=new Alarm() { 
                   command=Alarm.Command.NOTIFY
                }
            };
            if (info != null) msg.alarm.alarminfo = info;
            dealer.Send(msg, nextIdentity);
        }
    }
}