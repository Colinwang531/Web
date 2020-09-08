using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;
using System.IO;
using ShipWeb.ProtoBuffer.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security;
using System.Runtime.InteropServices;
using System.Threading;
using ShipWeb.DB;
using ShipWeb.Tool;
using Microsoft.Extensions.Logging;
using ShipWeb.Interface;
using ShipWeb.Helpers;
using Org.BouncyCastle.Crypto.Tls;

namespace ShipWeb.ProtoBuffer
{
    public class ProtoManager
    {
        public DealerSocket dealer = null;
        private static object dealer_Lock = new object(); //锁同步
        public static string IP = AppSettingHelper.GetSectionValue("IP");// "tcp://192.168.0.22:61001";//接收从main入口过来的url
        private static ProtoBDManager dbManager = new ProtoBDManager();
        public static string identity = "";//DealerSocket的通讯ID全局唯一     
        //单例控制dealer只有一个。
        public ProtoManager()
        {
            lock (dealer_Lock)
            {
                if (dealer == null)
                {
                    dealer = new DealerSocket(IP);
                    //等待时间10秒
                    dealer.Options.Linger=new TimeSpan(0,0,10);
                    dealer.Options.Identity = Encoding.UTF8.GetBytes(identity);
                }
            }
        }

        #region 组件
        /// <summary>
        /// 发送组件注册请求
        /// </summary>
        /// <returns>组件ID</returns>
        public ComponentResponse ComponentStart(ComponentInfo.Type type, string name = "组件1", string cid = "")
        {
            ComponentResponse retult = new ComponentResponse();
            //组件注册消息整理
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
                            type = type,
                            cname = name
                        },                         
                    }
                }
            };
            if (!string.IsNullOrEmpty(cid)) msg.component.componentrequest.componentinfo.cid = cid;

            //retult = DataMessage(msg,dealer);
            SendMessage(msg,identity,"");
            return retult;
        }
        /// <summary>
        /// 心跳
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="cid"></param>
        public void Heart( ComponentInfo.Type type, string name = "组件1", string cid = "")
        {
            ComponentResponse retult = new ComponentResponse();
            //组件注册消息整理
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
                            type = type,
                            cname = name
                        },
                    }
                }
            };
            if (!string.IsNullOrEmpty(cid)) msg.component.componentrequest.componentinfo.cid = cid;
            SendMessage(msg,identity,"");
            //service.SendMessage(msg, identity);
        }
        /// <summary>
        /// 递归处理消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="dealer"></param>
        /// <returns></returns>
        private ComponentResponse DataMessage(MSG msg, DealerSocket dealer)
        {
            ComponentResponse retult = new ComponentResponse();
            //SendMessage(msg);
            //MSG revMsg = ReceiveMessage(dealer);
            //if (revMsg.type == MSG.Type.COMPONENT)
            //{
            //    Component compMsg = revMsg.component;
            //    if (compMsg.command == Component.Command.SIGNIN_REP)
            //    {
            //        if (compMsg.componentresponse != null)
            //        {
            //            retult = compMsg.componentresponse;
            //        }
            //    }
            //    //陆地端接收到船舶端的组件注册请求
            //    else if (compMsg.command == Component.Command.SIGNIN_REQ)
            //    {
            //        byte[] by = dealer.Options.Identity;
            //        string dbcid = ProtoBDManager.ComponentSet(Encoding.UTF8.GetString(by),compMsg.componentrequest.componentinfo);
            //        MSG msgSend = new MSG()
            //        {
            //            type = MSG.Type.COMPONENT,
            //            timestamp = ProtoBufHelp.TimeSpan(),
            //            sequence = 2,
            //            component = new Component()
            //            {
            //                command = Component.Command.SIGNIN_REP,
            //                componentresponse = new ComponentResponse()
            //                {
            //                    cid = dbcid,
            //                    result = 0
            //                }
            //            }
            //        };
            //      return DataMessage(msgSend, dealer);
            //    }
            //}
            return retult;
        }

        /// <summary>
        /// 发送组件退出请求
        /// </summary>
        /// <param name="cid"></param>
        public void ComponentExit(string cid)
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
            SendMessage(msg,identity,"");
        }
        /// <summary>
        /// 组件查询
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="identity"></param>
        public ComponentResponse ComponentQuery(string nextIdentity="")
        {
            ComponentResponse result = new ComponentResponse();
            //组件注册消息整理
            MSG msg = new MSG()
            {
                type = MSG.Type.COMPONENT,
                timestamp = ProtoBufHelp.TimeSpan(),
                sequence = 1,
                component = new Component()
                {
                    command = Component.Command.QUERY_REP
                }
            };
            SendMessage(msg,identity,nextIdentity);
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg != null && revMsg.type == MSG.Type.COMPONENT)
            {
                Component compMsg = revMsg.component;
                if (compMsg.command == Component.Command.QUERY_REQ)
                {
                    if (compMsg.componentresponse.result == 0)
                    {
                        result = compMsg.componentresponse;
                    }
                }
            }
            return result;
        }
        #endregion

        #region 算法
        /// <summary>
        /// 算法设置
        /// </summary>
        /// <param name="configure">算法配置实体</param>
        /// <returns>0:成功</returns>
        public int AlgorithmSet(AlgorithmInfo protoModel,string algoIdentity)
        {
            int result = 1;
            MSG msg = new MSG()
            {
                type = MSG.Type.ALGORITHM,
                sequence = 3,
                timestamp = ProtoBufHelp.TimeSpan(),
                algorithm = new Algorithm()
                {
                     command=Algorithm.Command.CONFIGURE_REQ,
                    algorithmrequest = new AlgorithmRequest()
                    {
                        algorithminfo = protoModel
                    }
                }
            };
            result=ConfigureSetMessage(msg, dealer,algoIdentity);
            return result;
        }
        private int ConfigureSetMessage(MSG msg, DealerSocket dealer,string algoIdentity)
        {
            int result = 1;
            SendMessage(msg,identity,algoIdentity);
            MSG revMsg = ReceiveMessage(dealer);
            //MSG revMsg = msg;
            if (revMsg.type == MSG.Type.ALGORITHM)
            {
                Algorithm algMsg = revMsg.algorithm;
                if (algMsg.command == Algorithm.Command.CONFIGURE_REP && algMsg.algorithmresponse != null)
                {
                    result = algMsg.algorithmresponse.result;
                }
                else if (algMsg.command == Algorithm.Command.CONFIGURE_REQ && algMsg.algorithmrequest != null)
                {
                    byte[] by = dealer.Options.Identity;
                    string shipId = Encoding.UTF8.GetString(by);
                    result = ProtoBDManager.CameraConfigSet(algMsg.algorithmrequest.algorithminfo);
                    MSG sendMsg = new MSG()
                    {
                        type = MSG.Type.ALGORITHM,
                        sequence = 4,
                        timestamp = ProtoBufHelp.TimeSpan(),
                        algorithm = new Algorithm()
                        {
                            command = Algorithm.Command.CONFIGURE_REP,
                            algorithmresponse = new AlgorithmResponse()
                            {
                                result = result
                            }
                        }
                    };
                    return ConfigureSetMessage(sendMsg, dealer,algoIdentity);
                }
            }
            return result;
        }
        /// <summary>
        /// 算法查询
        /// </summary>
        /// <returns>算法配置实体</returns>
        public List<AlgorithmInfo> AlgorithmQuery(string algoIdentity)
        {
            List<AlgorithmInfo> list = null;
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
            list = ConfigureMessage(msg, dealer,algoIdentity);

            return list;
        }

        private List<AlgorithmInfo> ConfigureMessage(MSG msg, DealerSocket dealer,string algoIdentity)
        {
            List<AlgorithmInfo> list = new List<AlgorithmInfo>();
            SendMessage(msg,identity,algoIdentity);
            MSG revMsg = ReceiveMessage(dealer);
            //MSG revMsg = msg;
            if (revMsg.type == MSG.Type.ALGORITHM)
            {
                Algorithm algMsg = revMsg.algorithm;
                if (algMsg.command == Algorithm.Command.QUERY_REP && algMsg.algorithmresponse != null)
                {
                    if (algMsg.algorithmresponse.result == 0)
                    {
                        list = algMsg.algorithmresponse.configures;
                    }
                }
                else if (algMsg.command == Algorithm.Command.QUERY_REQ)
                {
                    var conList = ProtoBDManager.CameraConfigQuery();
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
                                configures = conList,
                                result = 0
                            }
                        }
                    };
                    return ConfigureMessage(sendMsg, dealer,algoIdentity);
                }
            }
            return list;
        }
        #endregion

        #region 船员
        /// <summary>
        /// 查询船员信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public List<CrewInfo> CrewQuery(string uid = "", string nextIdentity="")
        {
            List<CrewInfo> list = new List<CrewInfo>();
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
            list=EmployeeMessage(msg, dealer, nextIdentity);
            return list;
        }
        private List<CrewInfo> EmployeeMessage(MSG msg, DealerSocket dealer, string nextIdentyty)
        {
            List<CrewInfo> list = new List<CrewInfo>();
            SendMessage(msg,identity,nextIdentyty);
            MSG revMsg = ReceiveMessage(dealer);
            //MSG revMsg = msg;
            if (revMsg.type == MSG.Type.CREW)
            {
                Crew crewMsg = revMsg.crew;
                if (crewMsg.command == Crew.Command.QUERY_REP && crewMsg.crewresponse != null)
                {
                    if (crewMsg.crewresponse.result == 0)
                    {
                        list = crewMsg.crewresponse.crewinfos;
                    }
                }
                else if (crewMsg.command == Crew.Command.QUERY_REQ)
                {
                    string uid = crewMsg.crewrequest.crewinfo==null ? "" : crewMsg.crewrequest.crewinfo.uid;
                    var empList = ProtoBDManager.CrewQuery(uid);
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
                                 crewinfos=empList
                            }
                        }
                    };
                    return EmployeeMessage(sendMsg, dealer,nextIdentyty);
                }
            }
            return list;
        }
        /// <summary>
        /// 添加船员
        /// </summary>
        /// <param name="crewinfo">般员信息</param>
        /// <returns></returns>
        public int CrewAdd(CrewInfo crewinfo,string nextIdentity="")
        {
            int result = 1;
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
                         crewinfo= crewinfo
                    }
                }
            };
            result=CrewAddMessage(msg, dealer,nextIdentity);
            return result;
        }
        private int CrewAddMessage(MSG msg, DealerSocket dealer,string nextIdentity)
        {
            int result = 1;
            SendMessage(msg,identity,nextIdentity);
            MSG revMsg = ReceiveMessage(dealer);
            //MSG revMsg = msg;
            if (revMsg.type == MSG.Type.CREW)
            {
                Crew crewMsg = revMsg.crew;
                if (crewMsg.command == Crew.Command.NEW_REP && crewMsg.crewresponse != null)
                {                    
                  result = crewMsg.crewresponse.result;
                }
                else if (crewMsg.command == Crew.Command.NEW_REQ && crewMsg.crewrequest != null)
                {
                    var res = ProtoBDManager.CrewAdd(crewMsg.crewrequest.crewinfo);
                    MSG sendMsg = new MSG()
                    {
                        type = MSG.Type.CREW,
                        sequence = 4,
                        timestamp = ProtoBufHelp.TimeSpan(),
                        crew = new Crew()
                        {
                            command = Crew.Command.NEW_REP,
                            crewresponse = new CrewResponse()
                            {
                                result = res
                            }
                        }
                    };
                    return CrewAddMessage(sendMsg, dealer,nextIdentity);
                }
            }
            return result;
        }

        /// <summary>
        /// 修改船员
        /// </summary>
        /// <param name="crewInfo">般员信息</param>
        /// <returns>为空表示失败</returns>
        public int CrewUpdate(CrewInfo crewInfo,string nextIdentity="")
        {
            int result = 1;
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
            result = CrewUpdateMessage(msg, dealer,nextIdentity);
            return result;
        }
        private int CrewUpdateMessage(MSG msg, DealerSocket dealer, string nextIdentity)
        {
            int result = 1;
            SendMessage(msg,identity,nextIdentity);
            MSG revMsg = ReceiveMessage(dealer);
            //MSG revMsg = msg;
            if (revMsg.type == MSG.Type.CREW)
            {
                Crew crewMsg = revMsg.crew;
                if (crewMsg.command == Crew.Command.MODIFY_REP && crewMsg.crewresponse != null)
                {
                    result = crewMsg.crewresponse.result;
                }
                else if (crewMsg.command == Crew.Command.MODIFY_REQ && crewMsg.crewrequest != null)    
                {
                    var res = ProtoBDManager.CrewUpdate(crewMsg.crewrequest.crewinfo);
                    MSG sendMsg = new MSG()
                    {
                        type = MSG.Type.CREW,
                        sequence = 4,
                        timestamp = ProtoBufHelp.TimeSpan(),
                        crew = new Crew()
                        {
                            command = Crew.Command.MODIFY_REP,
                            crewresponse = new CrewResponse()
                            {
                                result = res
                            }
                        }
                    };
                    return CrewUpdateMessage(sendMsg, dealer,nextIdentity);
                }
            }
            return result;
        }
        /// <summary>
        /// 删除船员
        /// </summary>
        /// <param name="uid">船员ID</param>
        /// <returns></returns>
        public int CrewDelete(string uid,string nextIdentity="")
        {
            int result = 1;
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
                         crewinfo=new CrewInfo() { 
                             uid=uid
                         }
                    }
                }
            };
            result = CrewDeleteMessage(msg, dealer,nextIdentity);
            return result;
        }
        private int CrewDeleteMessage(MSG msg, DealerSocket dealer,string nextIdentity)
        {
            int result = 1;
            SendMessage(msg,identity,nextIdentity);
            MSG revMsg = ReceiveMessage(dealer);
            //MSG revMsg = msg;
            if (revMsg.type == MSG.Type.CREW)
            {
                Crew crewMsg = revMsg.crew;
                if (crewMsg.command == Crew.Command.DELETE_REP && crewMsg.crewresponse != null)
                {
                    result = crewMsg.crewresponse.result;
                }
                else if (crewMsg.command == Crew.Command.DELETE_REQ && crewMsg.crewrequest != null)
                {
                    var res = ProtoBDManager.CrewDelete(crewMsg.crewrequest.crewinfo.uid);
                    MSG sendMsg = new MSG()
                    {
                        type = MSG.Type.CREW,
                        sequence = 4,
                        timestamp = ProtoBufHelp.TimeSpan(),
                        crew = new Crew()
                        {
                            command = Crew.Command.DELETE_REP,
                            crewresponse = new CrewResponse()
                            {
                                result = res
                            }
                        }
                    };
                    return CrewDeleteMessage(sendMsg, dealer,nextIdentity);
                }
            }
            return result;
        }
        #endregion

        #region 设备
        /// <summary>
        /// 查询设备
        /// </summary>
        /// <param name="did">设备ID</param>
        /// <returns></returns>
        public List<DeviceInfo> DeviceQuery(string devIdentity, string did = "")
        {
            List<DeviceInfo> list = new List<DeviceInfo>();

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
            list = DeviceMessage(msg, dealer,devIdentity);
            return list;
        }
        private List<DeviceInfo> DeviceMessage(MSG msg, DealerSocket dealer,string devIdentity)
        {
            List<DeviceInfo> list = new List<DeviceInfo>();
            SendMessage(msg,identity,devIdentity);
            MSG revMsg = ReceiveMessage(dealer);
            //MSG revMsg = msg;
            if (revMsg.type == MSG.Type.DEVICE)
            {
                Device devMsg = revMsg.device;
                if (devMsg.command == Device.Command.QUERY_REP && devMsg.deviceresponse != null)
                {
                    if (devMsg.deviceresponse.result == 0)
                    {
                        return list = devMsg.deviceresponse.deviceinfos;
                    }
                }
                else if (devMsg.command == Device.Command.QUERY_REQ)
                {
                    string did = devMsg.devicerequest==null ? "" : devMsg.devicerequest.did;
                    var info = devMsg.devicerequest == null ? null : devMsg.devicerequest.deviceinfo;
                    byte[] by = dealer.Options.Identity;
                    list = ProtoBDManager.DeviceQuery(info,did);
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
                                deviceinfos = list,
                                result = 0
                            }
                        }
                    };
                    return DeviceMessage(sendMsg, dealer,devIdentity);
                }
            }
            return list;
        }
        /// <summary>
        /// 添加设备
        /// </summary>
        /// <param name="deviceInfo">设备实体</param>
        /// <returns></returns>
        public DeviceResponse DeveiceAdd(DeviceInfo deviceInfo,string devIdentity)
        {
            DeviceResponse result = new DeviceResponse();
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
            //SendMessage(msg);
            result = DeveiceAddMessage(msg, dealer,devIdentity);
            return result;
        }
        private DeviceResponse DeveiceAddMessage(MSG msg, DealerSocket dealer,string devIdentity)
        {
            DeviceResponse result = new DeviceResponse();
            SendMessage(msg,identity,devIdentity);
            MSG revMsg = ReceiveMessage(dealer);
            //MSG revMsg = msg;
            if (revMsg.type == MSG.Type.DEVICE)
            {
                Device devMsg = revMsg.device;
                if (devMsg.command == Device.Command.NEW_REP && devMsg.deviceresponse != null)
                {
                    if (devMsg.deviceresponse.result == 0)
                    {
                        result = devMsg.deviceresponse;
                    }
                }
                else if (devMsg.command == Device.Command.NEW_REQ && devMsg.devicerequest != null)
                {
                    var res = ProtoBDManager.DeviceAdd(devMsg.devicerequest.deviceinfo);
                    MSG sendMsg = new MSG()
                    {
                        type = MSG.Type.DEVICE,
                        sequence = 6,
                        timestamp = ProtoBufHelp.TimeSpan(),
                        device = new Device()
                        {
                            command = Device.Command.NEW_REP,
                            deviceresponse = new DeviceResponse()
                            {
                                result = res
                            }
                        }
                    };
                    return DeveiceAddMessage(sendMsg, dealer,devIdentity);
                }
            }
            return result;
        }
        /// <summary>
        /// 修改设备
        /// </summary>
        /// <param name="embedded">设备实体</param>
        /// <param name="optype"></param>
        /// <returns></returns>
        public int DeveiceUpdate(DeviceInfo embedded, string did,string devIdentity)
        {
            int result = 1;
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
                        deviceinfo = embedded,
                        did = did
                    }
                }
            };
            result = DeveiceUpdateMessage(msg, dealer,devIdentity);
            return result;
        }
        private int DeveiceUpdateMessage(MSG msg, DealerSocket dealer,string devIdentity)
        {
            int result = 1;
            SendMessage(msg,identity,devIdentity);
            MSG revMsg = ReceiveMessage(dealer);
            //MSG revMsg = msg;
            if (revMsg.type == MSG.Type.DEVICE)
            {
                Device devMsg = revMsg.device;
                if (devMsg.command == Device.Command.MODIFY_REP && devMsg.deviceresponse != null)
                {
                    result = devMsg.deviceresponse.result;
                }
                else if (devMsg.command == Device.Command.MODIFY_REQ && devMsg.devicerequest != null)
                {
                    var res = ProtoBDManager.DeviceUpdate(devMsg.devicerequest.did, devMsg.devicerequest.deviceinfo);
                    MSG sendMsg = new MSG()
                    {
                        type = MSG.Type.DEVICE,
                        sequence = 6,
                        timestamp = ProtoBufHelp.TimeSpan(),
                        device = new Device()
                        {
                            command = Device.Command.MODIFY_REP,
                            deviceresponse = new DeviceResponse()
                            {
                                result = res
                            }
                        }
                    };
                    return DeveiceUpdateMessage(sendMsg, dealer,devIdentity);
                }
            }
            return result;
        }
        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="did"></param>
        /// <returns></returns>
        public int DeveiceDelete(string did,string devIdentity)
        {
            int result = 1;

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
            result = DeveiceDeleteMessage(msg, dealer,devIdentity);
            return result;
        }
        private int DeveiceDeleteMessage(MSG msg, DealerSocket dealer,string devIdentity)
        {
            int result = 1;
            SendMessage(msg,identity,devIdentity);
            MSG revMsg = ReceiveMessage(dealer);
            //MSG revMsg = msg;
            if (revMsg.type == MSG.Type.DEVICE)
            {
                Device devMsg = revMsg.device;
                if (devMsg.command == Device.Command.DELETE_REP && devMsg.deviceresponse != null)
                {
                    result = devMsg.deviceresponse.result;
                }
                else if (devMsg.command == Device.Command.DELETE_REQ && devMsg.devicerequest != null)
                {
                    string did = devMsg.devicerequest.did;
                    var res = ProtoBDManager.DeviceDelete(did);
                    MSG sendMsg = new MSG()
                    {
                        type = MSG.Type.DEVICE,
                        sequence = 6,
                        timestamp = ProtoBufHelp.TimeSpan(),
                        device = new Device()
                        {
                            command = Device.Command.DELETE_REP,
                            deviceresponse = new DeviceResponse()
                            {
                                result = res
                            }
                        }
                    };
                    return DeveiceDeleteMessage(sendMsg, dealer,devIdentity);
                }
            }
            return result;
        }
        #endregion

        #region 状态
        /// <summary>
        /// 状态设置
        /// </summary>
        /// <param name="flag">是否在港口</param>
        /// <returns></returns>
        public StatusResponse StatussSet(StatusRequest request,string nextIdentity="")
        {
            StatusResponse result = new StatusResponse();
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
            //SendMessage(msg);
            result = StatusSetMessage(msg, dealer, nextIdentity);
            return result;
        }
        private StatusResponse StatusSetMessage(MSG msg, DealerSocket dealer ,string nextIdentity) 
        {
            StatusResponse result = new StatusResponse();
            SendMessage(msg,identity,nextIdentity);
            MSG revMsg = ReceiveMessage(dealer);
            //MSG revMsg = msg;
            if (revMsg.type == MSG.Type.STATUS)
            {
                Status staMsg = revMsg.status;
                if (staMsg.command == Status.Command.SET_REP )
                {
                    result = staMsg.statusresponse;
                }
                //收到设置请求
                else if (staMsg.command == Status.Command.SET_REQ && staMsg.statusrequest != null)
                {
                    byte[] by= dealer.Options.Identity;
                    var res=ProtoBDManager.ShipSet(Encoding.UTF8.GetString(by), staMsg.statusrequest);
                    MSG sendMsg = new MSG()
                    {
                        type = MSG.Type.STATUS,
                        sequence = 6,
                        timestamp = ProtoBufHelp.TimeSpan(),
                        status = new Status()
                        {
                            command = Status.Command.SET_REP,
                            statusresponse = new StatusResponse()
                            {
                                result = res
                            }
                        }
                    };
                    return StatusSetMessage(sendMsg, dealer,nextIdentity);
                }
            }
            return result;
        }
        /// <summary>
        /// 查询船信息
        /// </summary>
        /// <param name="type"></param>
        /// <param name="identity"></param>
        /// <returns></returns>
        public StatusResponse StatusQuery()
        {
            StatusResponse result = new StatusResponse();
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
            result=StatusQueryMessage(msg, dealer);
            return result;
        }
        private StatusResponse StatusQueryMessage(MSG msg, DealerSocket dealer) 
        {
            StatusResponse result = new StatusResponse();
            SendMessage(msg,identity,"");
            MSG revMsg = ReceiveMessage(dealer);
            //MSG revMsg = msg;
            if (revMsg.type == MSG.Type.STATUS)
            {
                Status staMsg = revMsg.status;
                if (staMsg.command == Status.Command.QUERY_REP && staMsg.statusresponse != null)
                {
                    result = staMsg.statusresponse;
                }
                else if(staMsg.command == Status.Command.QUERY_REQ)
                {
                    var ship = ProtoBDManager.StatusQuery();
                    MSG sendMsg = new MSG()
                    {
                        type = MSG.Type.STATUS,
                        sequence = 6,
                        timestamp = ProtoBufHelp.TimeSpan(),
                         status=new Status() { 
                             command=Status.Command.QUERY_REP,
                              statusresponse=new StatusResponse()
                              {
                                  result = ship != null ? 0 : 1,
                                  flag = ship != null ? ship.Flag : false,
                                  name = ship != null ? ship.Name : ""
                              }
                         }
                    };
                    return StatusQueryMessage(sendMsg, dealer);
                }
            }
            return result;
        }
        #endregion

        #region 报警消息
        public Alarm AlarmStart()
        {
            Alarm result = new Alarm();
            MSG msg = new MSG()
            {
                type = MSG.Type.ALARM,
                sequence = 1,
                timestamp = ProtoBufHelp.TimeSpan()
            };
            SendMessage(msg,identity,"");
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg.type == MSG.Type.ALARM)
            {
                result = revMsg.alarm;
            }
            return result;
        }
        #endregion

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="identity">自己的通讯ID</param>
        /// <param name="nextIdentity">需要操作的通讯ID</param>
        /// <param name="msg">通讯内容</param>
        private void SendMessage(MSG msg,string identity,string nextIdentity)
        {
            try
            {
                byte[] byt = ProtoBufHelp.Serialize<MSG>(msg);
                NetMQMessage mqmsg = new NetMQMessage();
                mqmsg.AppendEmptyFrame();
                mqmsg.Append("Request");
                mqmsg.Append(identity);               
                mqmsg.Append(nextIdentity); 
                mqmsg.Append(byt);
                //发送注册请求
                bool flag = dealer.TrySendMultipartMessage(mqmsg);
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="dealer"></param>
        /// <returns></returns>
        public MSG ReceiveMessage(DealerSocket dealer)
        {
            bool flag = true;
            StringBuilder sb = new StringBuilder();
            List<byte[]> list = new List<byte[]>();
            while (flag)
            {
                try
                {
                    list = dealer.ReceiveMultipartBytes();
                    flag = false;
                }
                catch (Exception ex)
                {
                    continue;
                }
                //Thread.Sleep(1000);
            }
            List<byte> byteSource = new List<byte>();
            foreach (var item in list)
            {
                byteSource.AddRange(item);
            }
            byte[] mory = byteSource.ToArray();
            MSG revmsg = ProtoBufHelp.DeSerialize<MSG>(mory);
            if (revmsg.type == MSG.Type.ALARM)
            {
                ProtoBDManager.AlarmAdd(revmsg);
            }
            return revmsg;
        }
    }
}
