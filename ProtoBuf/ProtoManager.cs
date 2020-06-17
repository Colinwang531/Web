﻿using NetMQ;
using NetMQ.Sockets;
using ProtoBuf.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;
using System.IO;

namespace ProtoBuf
{
    public class ProtoManager
    {
        private static ProtoManager _protoManager = null;
        private static DealerSocket dealer = null;
        private static object dealer_Lock = new object(); //锁同步
        //int aa;
        //单例控制dealer只有一个。
        public  ProtoManager()
        {
            lock (dealer_Lock)
            {
                if (dealer == null)
                {
                    dealer = new DealerSocket("tcp://192.168.0.2:61001");
                    //aa = rm.Next();
                    _protoManager = new ProtoManager();
                }
            }
        }

        #region 组件
        /// <summary>
        /// 发送组件注册请求
        /// </summary>
        /// <returns>组件ID</returns>
        public ComponentResponse ComponentStart(string identity, int type = 2, string name = "组件1")
        {
            ComponentResponse retult = new ComponentResponse();
            dealer.Options.Identity = Encoding.Unicode.GetBytes(identity);

            //组件注册消息整理
            MSG msg = new MSG()
            {
                type = MSG.Type.COMPONENT,
                timestamp = ProtoBufHelp.TimeSpan(),
                sequence = 3,
                component = new Componentinfo()
                {
                    command = Componentinfo.Command.SIGNIN_REQ,
                    componentrequest = new ComponentRequest()
                    {
                        type = ComponentRequest.Type.WEB
                    }
                }
            };
            if (type == 1) msg.component.componentrequest.type = ComponentRequest.Type.XMQ;
            if (type == 3) msg.component.componentrequest.type = ComponentRequest.Type.HKD;
            if (type == 4) msg.component.componentrequest.type = ComponentRequest.Type.DHD;
            if (type == 5) msg.component.componentrequest.type = ComponentRequest.Type.ALM;
            //把成实转成字节流
            SendMessage(msg);
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg.type == MSG.Type.COMPONENT)
            {
                Componentinfo compMsg = revMsg.component;
                if (compMsg.command == Componentinfo.Command.SIGNIN_REP)
                {
                    if (compMsg.componentresponse != null)
                    {

                        retult = compMsg.componentresponse;
                    }
                }
            }
            return retult;
        }


        /// <summary>
        /// 发送组件退出请求
        /// </summary>
        /// <param name="cid"></param>
        public void ComponentExit(string cid,string identity)
        {          
            dealer.Options.Identity = Encoding.Unicode.GetBytes(identity);
           
            //组件注册消息整理
            MSG msg = new MSG()
            {
                type = MSG.Type.COMPONENT,
                timestamp = ProtoBufHelp.TimeSpan(),
                sequence = 3,
                component = new Componentinfo()
                {
                    command = Componentinfo.Command.SIGNOUT_REQ,
                    componentrequest = new ComponentRequest()
                    {
                        cid = cid,
                        type = ComponentRequest.Type.WEB
                    }
                }
            };
            //把成实转成字节流
            byte[] byt = ProtoBufHelp.Serialize<MSG>(msg);
            SendMessage(msg);
        }
        /// <summary>
        /// 组件查询
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="identity"></param>
        public List<Componentinfo> ComponentQuery(string cid, string identity)
        {
            List<Componentinfo> list = new List<Componentinfo>();
            dealer.Options.Identity = Encoding.Unicode.GetBytes(identity);
            //组件注册消息整理
            MSG msg = new MSG()
            {
                type = MSG.Type.COMPONENT,
                timestamp = ProtoBufHelp.TimeSpan(),
                sequence = 3,
                component = new Componentinfo()
                {
                    command = Componentinfo.Command.QUERY_REP,
                    componentrequest = new ComponentRequest()
                    {
                        cid = cid
                    }
                }
            };
            SendMessage(msg);
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg != null && revMsg.type == MSG.Type.COMPONENT)
            {
                Componentinfo compMsg = revMsg.component;
                if (compMsg.command == Componentinfo.Command.QUERY_REQ)
                {
                    if (compMsg.componentresponse.result==0)
                    {
                        list = compMsg.componentresponse.componentinfos;
                    }
                }
            }
            return list;
        }
        #endregion

        #region 算法
        /// <summary>
        /// 算法设置
        /// </summary>
        /// <param name="configure">算法配置实体</param>
        /// <returns>0:成功</returns>
        public int AlgorithmSet(string identity, List<Configure> configureList)
        {
            int  result =55;
            dealer.Options.Identity = Encoding.Unicode.GetBytes(identity);

            MSG msg = new MSG()
            {
                type = MSG.Type.ALGORITHM,
                sequence = 2,
                timestamp = ProtoBufHelp.TimeSpan(),
                algorithm = new Algorithm()
                {
                    algorithmrequest = new AlgorithmRequest()
                    {
                        configure = configureList
                    }
                }
            };
            SendMessage(msg);
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg.type == MSG.Type.ALGORITHM)
            {
                Algorithm algMsg = revMsg.algorithm;
                if (algMsg.command == Algorithm.Command.QUERY_REP && algMsg.algorithmresponse != null)
                {
                    result = algMsg.algorithmresponse.result;
                }
                       
            }
            return result;
        }

        /// <summary>
        /// 算法查询
        /// </summary>
        /// <returns>算法配置实体</returns>
        public List<Configure> AlgorithmQuery(string identity)
        {
            List<Configure> list = null;
            dealer.Options.Identity = Encoding.Unicode.GetBytes(identity);           
            MSG msg = new MSG()
            {
                type = MSG.Type.ALGORITHM,
                sequence = 2,
                timestamp = ProtoBufHelp.TimeSpan(),
                algorithm = new Algorithm()
                {
                    command = Algorithm.Command.QUERY_REQ
                }
            };
            SendMessage(msg);
            MSG revMsg = ReceiveMessage(dealer);
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
        public List<Employee> CrewQuery(string Identity, string uid = "")
        {
            List<Employee> list = new List<Employee>();
            dealer.Options.Identity = Encoding.Unicode.GetBytes(Identity);
           
            MSG msg = new MSG()
            {
                type = MSG.Type.CREW,
                sequence = 4,
                timestamp = ProtoBufHelp.TimeSpan(),
                crew = new Crew()
                {
                    command = Crew.Command.QUERY_REQ
                }
            };
            if (uid != "")
            {
                msg.crew.crewrequest = new CrewRequest()
                {
                    uid = uid
                };
            }
            SendMessage(msg);
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg.type == MSG.Type.CREW)
            {
                Crew crewMsg = revMsg.crew;
                if (crewMsg.command == Crew.Command.QUERY_REP && crewMsg.crewresponse != null)
                {
                    if (crewMsg.crewresponse.result == 0)
                    {
                        list = crewMsg.crewresponse.employees;
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 添加船员
        /// </summary>
        /// <param name="employee">般员信息</param>
        /// <returns></returns>
        public string CrewAdd(Employee employee,string identity)
        {
            string result = "";
            dealer.Options.Identity = Encoding.Unicode.GetBytes(identity);           
            MSG msg = new MSG()
            {
                type = MSG.Type.CREW,
                sequence =4,
                timestamp = ProtoBufHelp.TimeSpan(),
                crew = new Crew()
                {
                    command =  Crew.Command.NEW_REQ,
                    crewrequest = new CrewRequest()
                    {
                        employee = employee
                    }
                }
            };
            SendMessage(msg);
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg.type == MSG.Type.CREW)
            {
                Crew crewMsg = revMsg.crew;
                if (crewMsg.command == Crew.Command.NEW_REP && crewMsg.crewresponse != null)
                {
                    if (crewMsg.crewresponse.result == 0)
                    {
                        result = crewMsg.crewresponse.uid;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 修改船员
        /// </summary>
        /// <param name="employee">般员信息</param>
        /// <returns>为空表示失败</returns>
        public int CrewUpdate(Employee employee, string uid,string identity)
        {
            int result = 55;
            dealer.Options.Identity = Encoding.Unicode.GetBytes(identity);
           
            MSG msg = new MSG()
            {
                type = MSG.Type.CREW,
                sequence =4,
                timestamp = ProtoBufHelp.TimeSpan(),
                crew = new Crew()
                {
                    command = Crew.Command.MODIFY_REQ,
                    crewrequest = new CrewRequest()
                    {
                        employee = employee,
                        uid = uid
                    }
                }
            };
            SendMessage(msg);
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg.type == MSG.Type.CREW)
            {
                Crew crewMsg = revMsg.crew;
                if (crewMsg.command == Crew.Command.MODIFY_REP && crewMsg.crewresponse != null)
                {
                    result = crewMsg.crewresponse.result;
                }
            }
            return result;
        }
        /// <summary>
        /// 删除船员
        /// </summary>
        /// <param name="uid">船员ID</param>
        /// <returns></returns>
        public int CrewDelete(string uid,string identity)
        {
            int result = 55;
            dealer.Options.Identity = Encoding.Unicode.GetBytes(identity);
           
            MSG msg = new MSG()
            {
                type = MSG.Type.CREW,
                sequence = 4,
                timestamp = ProtoBufHelp.TimeSpan(),
                crew = new Crew()
                {
                    command = Crew.Command.DELETE_REQ,
                    crewrequest = new CrewRequest()
                    {
                        uid = uid
                    }
                }
            };
            SendMessage(msg);
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg.type == MSG.Type.CREW)
            {
                Crew crewMsg = revMsg.crew;
                if (crewMsg.command == Crew.Command.DELETE_REP && crewMsg.crewresponse != null)
                {
                    result = crewMsg.crewresponse.result;
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
        public List<Embedded> DeviceQuery(string identity, string did = "")
        {
            List<Embedded> list = new List<Embedded>();
            dealer.Options.Identity = Encoding.Unicode.GetBytes(identity);
           
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
            SendMessage(msg);
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg.type == MSG.Type.DEVICE)
            {
                Device devMsg = revMsg.device;
                if (devMsg.command == Device.Command.QUERY_REP && devMsg.deviceresponse != null)
                {
                    if (devMsg.deviceresponse.result == 0)
                    {
                        list = devMsg.deviceresponse.embedded;
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// 添加设备
        /// </summary>
        /// <param name="embedded">设备实体</param>
        /// <returns></returns>
        public DeviceResponse DeveiceAdd(Embedded embedded,string identity)
        {
            DeviceResponse result = new DeviceResponse();
            dealer.Options.Identity = Encoding.Unicode.GetBytes(identity);
           
            MSG msg = new MSG()
            {
                type = MSG.Type.DEVICE,
                sequence = 5,
                timestamp = ProtoBufHelp.TimeSpan(),
                device = new Device()
                {
                    command =  Device.Command.NEW_REQ,
                    devicerequest = new DeviceRequest()
                    {
                        embedded = embedded
                    }
                }
            };
            SendMessage(msg);
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg.type == MSG.Type.DEVICE)
            {
                Device devMsg = revMsg.device; 
                if (devMsg.command == Device.Command.NEW_REP && devMsg.deviceresponse != null)
                {
                    if (devMsg.deviceresponse.result == 0)
                    {
                        result = devMsg.deviceresponse;
                    }
                } }
            return result;
        }
        /// <summary>
        /// 修改设备
        /// </summary>
        /// <param name="embedded">设备实体</param>
        /// <param name="optype"></param>
        /// <returns></returns>
        public int DeveiceUpdate(Embedded embedded, string did,string identity)
        {
            int result =55;
            dealer.Options.Identity = Encoding.Unicode.GetBytes(identity);
           
            MSG msg = new MSG()
            {
                type = MSG.Type.DEVICE,
                sequence = 5,
                timestamp = ProtoBufHelp.TimeSpan(),
                device = new Device()
                {
                    command =Device.Command.MODIFY_REQ,
                    devicerequest = new DeviceRequest()
                    {
                        embedded = embedded,
                         did=did
                    }
                }
            };
            SendMessage(msg);
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg.type == MSG.Type.DEVICE)
            {
                Device devMsg = revMsg.device;
                if (devMsg.command == Device.Command.MODIFY_REP && devMsg.deviceresponse != null)
                {
                    result = devMsg.deviceresponse.result;
                }
            }
            return result;
        }

        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="did"></param>
        /// <returns></returns>
        public int DeveiceDelete(string did,string identity)
        {
            int result = 55;
            dealer.Options.Identity = Encoding.Unicode.GetBytes(identity);
           
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
            SendMessage(msg);
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg.type == MSG.Type.DEVICE)
            {
                Device devMsg = revMsg.device;
                if (devMsg.command == Device.Command.DELETE_REQ && devMsg.deviceresponse != null)
                {
                    result = devMsg.deviceresponse.result;
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
        public int StatesSet(StatusRequest request, string identity)
        {
            int result = 55;
            dealer.Options.Identity = Encoding.Unicode.GetBytes(identity);
           
            MSG msg = new MSG()
            {
                type = MSG.Type.STATUS,
                sequence = 6,
                timestamp = ProtoBufHelp.TimeSpan(),
                 status=new Status()
                 {
                     command = Status.Command.SET_REQ,
                     statusrequest = request
                 }
            };
            SendMessage(msg);
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg.type == MSG.Type.STATUS)
            {
                Status staMsg = revMsg.status;
                if (staMsg.command == Status.Command.SET_REQ && staMsg.statusresponse != null)
                {
                    result = staMsg.statusresponse.result;
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

        public StatusResponse StateQuery(StatusRequest.Type type, string identity)
        {
            StatusResponse result = new StatusResponse();
            dealer.Options.Identity = Encoding.Unicode.GetBytes(identity);

            MSG msg = new MSG()
            {
                type = MSG.Type.STATUS,
                sequence = 6,
                timestamp = ProtoBufHelp.TimeSpan(),
                status = new Status()
                {
                    command = Status.Command.QUERY_REP,
                    statusrequest = new StatusRequest()
                    { 
                         type= type
                    }
                }
            };
            SendMessage(msg);
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg.type == MSG.Type.STATUS)
            {
                Status staMsg = revMsg.status;
                if (staMsg.command == Status.Command.QUERY_REP && staMsg.statusresponse != null)
                {
                    result = staMsg.statusresponse;
                }
            }
            return result;
        }
        #endregion

        #region 用户
        /// <summary>
        /// 增加用户
        /// </summary>
        /// <param name="person">用户信息</param>
        /// <returns></returns>
        public UserResponse UserAdd(Person person,string identity)
        {
            UserResponse result = new UserResponse();
            MSG msg = UserAddOrUpdate(identity, person);
            SendMessage(msg);
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg.type == MSG.Type.USER)
            {
                User useMsg = revMsg.user;
                if (useMsg.command == User.Command.NEW_REP && useMsg.userresponse != null)
                {
                    if (useMsg.userresponse.result == 0)
                    {
                        result = useMsg.userresponse;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <param name="person"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public int UserUpdate(Person person,string uid,string identity)
        {
            int result =55;
            MSG msg = UserAddOrUpdate(identity, person,uid);
            SendMessage(msg);
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg.type == MSG.Type.USER)
            {
                User useMsg = revMsg.user;
                if (useMsg.command == User.Command.MODIFY_REP && useMsg.userresponse != null)
                {
                    result = useMsg.userresponse.result;
                }
            }
            return result;
        }

        /// <summary>
        /// 删除用户信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public int UserDelete(string uid,string identity)
        {
            int result = 55;
            dealer.Options.Identity = Encoding.Unicode.GetBytes(identity);           
            MSG msg = new MSG()
            {
                type = MSG.Type.USER,
                sequence = 7,
                timestamp = ProtoBufHelp.TimeSpan(),
                user = new User()
                {
                    command = User.Command.DELETE_REQ,
                    userrequest = new UserRequest()
                    {
                         uid=uid
                    }
                }
            };
            SendMessage(msg);
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg.type == MSG.Type.USER)
            {
                User useMsg = revMsg.user;
                if (useMsg.command == User.Command.DELETE_REP && useMsg.userresponse != null)
                {
                    result = useMsg.userresponse.result;
                }
            }
            return result;
        }
        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="uid">用户ID</param>
        /// <returns></returns>
        public List<Person> UserQuery(string uid = "")
        {
            List<Person> list = new List<Person>();
            Random rm = new Random();
            int index = rm.Next();
            dealer.Options.Identity = Encoding.Unicode.GetBytes(index.ToString());
           
            MSG msg = new MSG()
            {
                type = MSG.Type.USER,
                sequence = 7,
                timestamp = ProtoBufHelp.TimeSpan(),
                user = new User()
                {
                    command = User.Command.QUERY_REQ,                   
                }
            };
            if (!string.IsNullOrEmpty(uid))
            {
                msg.user.userrequest.uid = uid;
            }
            SendMessage(msg);
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg.type == MSG.Type.USER)
            {
                User useMsg = revMsg.user;
                if (useMsg.command == User.Command.QUERY_REP && useMsg.userresponse != null)
                {
                    if (useMsg.userresponse.result == 0)
                    {
                        list = useMsg.userresponse.persons;
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// 用户登陆
        /// </summary>
        /// <param name="uid">用户ID</param>
        /// <param name="person">用户详情</param>
        /// <returns></returns>
        public int UserLoggin(string uid,Person person,string identity)
        {
            int result = 55;
            dealer.Options.Identity = Encoding.Unicode.GetBytes(identity);
           
            MSG msg = new MSG()
            {
                type = MSG.Type.USER,
                sequence = 7,
                timestamp = ProtoBufHelp.TimeSpan(),
                user = new User()
                {
                    command = User.Command.LOGIN_REQ,
                }
            };
            SendMessage(msg);
            MSG revMsg = ReceiveMessage(dealer);
            if (revMsg.type == MSG.Type.USER)
            {
                User useMsg = revMsg.user;
                if (useMsg.command == User.Command.LOGIN_REP && useMsg.userresponse != null)
                {
                    result = useMsg.userresponse.result;
                }
            }
            return result;
        }
        private MSG UserAddOrUpdate(string identity, Person person,string uid="")
        {
            dealer.Options.Identity = Encoding.Unicode.GetBytes(identity);
           
            MSG msg = new MSG()
            {
                type = MSG.Type.USER,
                sequence = 7,
                timestamp = ProtoBufHelp.TimeSpan(),
                user = new User()
                {
                    command = string.IsNullOrEmpty(uid) ? User.Command.NEW_REQ : User.Command.MODIFY_REQ,
                    userrequest = new UserRequest()
                    {
                        person = person
                    }
                }
            };
            //执行修改时需要填入用户ID
            if (!string.IsNullOrEmpty(uid))
            {
                msg.user.userrequest.uid = uid;
            }
            return msg;
        }
        #endregion

        #region 报警消息
        public Alarm AlarmStart(string identity)
        {
            Alarm result = new Alarm();
            dealer.Options.Identity = Encoding.Unicode.GetBytes(identity);
           
            MSG msg = new MSG()
            {
                type = MSG.Type.ALARM,
                sequence = 1,
                timestamp =ProtoBufHelp.TimeSpan()                
            };
            SendMessage(msg);
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
        /// <param name="index">序号</param>
        /// <param name="msg"></param>
        private void SendMessage(MSG msg)
        {
            byte[] byt = ProtoBufHelp.Serialize<MSG>(msg);
            NetMQMessage mqmsg = new NetMQMessage();
            mqmsg.AppendEmptyFrame();
            mqmsg.Append(byt);
            //发送注册请求
            dealer.SendMultipartMessage(mqmsg);
        }

        
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="dealer"></param>
        /// <returns></returns>
        private MSG ReceiveMessage(DealerSocket dealer)
        {
            bool flag = true;
            StringBuilder sb = new StringBuilder();
            List<byte[]> list = new List<byte[]>();
            while (flag)
            {
                list = dealer.ReceiveMultipartBytes();
                flag = false;
            }
            List<byte> byteSource = new List<byte>();
            foreach (var item in list)
            {
                byteSource.AddRange(item);
            }
            byte[] mory = byteSource.ToArray();
            MSG revmsg = ProtoBufHelp.DeSerialize<MSG>(mory);
            return revmsg;
        }

    }
}
