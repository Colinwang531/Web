using NetMQ;
using NetMQ.Sockets;
using ShipWeb.Helpers;
using ShipWeb.ProtoBuffer;
using ShipWeb.ProtoBuffer.Models;
using ShipWeb.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer
{
    public class DealerService
    {
        private static DealerSocket dealer = null;
        private static object dealer_Lock = new object(); //锁同步

        //public static List<Task> taskList = new List<Task>();
        public DealerService()
        {
            lock (dealer_Lock)
            {
                if (dealer == null)
                {
                    dealer = new DealerSocket();
                    //dealer.Connect("tcp://192.168.0.21:5556")
                    dealer.Connect(ManagerHelp.IP);
                    //等待时间10秒
                    //dealer.Options.Linger=new TimeSpan(0,0,10);
                    string commID = Guid.NewGuid().ToString();
                    dealer.Options.Identity = Encoding.UTF8.GetBytes(commID);
                    Task.Factory.StartNew(state =>
                    {
                        Receive();
                    }, TaskCreationOptions.LongRunning);
                }
            }
        }

        public void Send(MSG msg, string toIdentity = "", string head = "request")
        {
            try
            {
                if (!dealer.IsDisposed)
                {
                    byte[] byt = ProtoBufHelp.Serialize<MSG>(msg);
                    NetMQMessage mqmsg = new NetMQMessage(5);
                    mqmsg.AppendEmptyFrame();
                    mqmsg.Append("worker");
                    mqmsg.Append(head);
                    ////船舶向陆地端发送
                    //if ((head == "response") && !ManagerHelp.UpFromId.Equals(ManagerHelp.ComponentId))
                    //{
                    //    mqmsg.Append(ManagerHelp.ComponentId);//当前组件ID 
                    //    mqmsg.Append(ManagerHelp.UpFromId);//上一级组件ID
                    //}
                    //else //船舶向组件请求
                    //{
                        mqmsg.Append(ManagerHelp.ComponentId);//当前组件ID
                        //to
                        mqmsg.Append(toIdentity);//下一级组件ID
                    //}
                    mqmsg.Append(byt);
                    //发送注册请求
                    dealer.SendMultipartMessage(mqmsg);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void Receive()
        {
            ReceiveDataManager manager = new ReceiveDataManager();
            while (true)
            {
                var mQFrames = dealer.ReceiveMultipartMessage(6);
                var temp1 = mQFrames[3].ConvertToString();
                var temp2 = mQFrames[4].ConvertToString();
                //对陆地端方向的接受
                if (temp2.Equals(ManagerHelp.ComponentId))
                {
                    //陆地端传下来的FromId
                    ManagerHelp.UpFromId = temp2;
                    //陆地端过来的ToId
                    ManagerHelp.UpToId = temp1;
                }
                byte[] mory = mQFrames.Last.ToByteArray();
                MSG revmsg = ProtoBufHelp.DeSerialize<MSG>(mory);
                try
                {
                    if (revmsg.type == MSG.Type.ALARM)
                    {
                        if (revmsg.alarm.alarminfo != null)
                        {
                            string xmq = "";
                            if (mQFrames[2].ToString() == "upload")
                            {
                                xmq = temp1;
                            }
                            ProtoBDManager.AlarmAdd(revmsg.alarm.alarminfo, xmq);
                        }
                    }
                    else if (revmsg.type == MSG.Type.ALGORITHM)
                    {
                        manager.AlgorithmData(revmsg.algorithm);
                    }
                    else if (revmsg.type == MSG.Type.CREW)
                    {
                        manager.CrewData(revmsg.crew);
                    }
                    else if (revmsg.type == MSG.Type.DEVICE)
                    {
                        manager.DeviceData(revmsg.device);
                    }
                    else if (revmsg.type == MSG.Type.STATUS)
                    {
                        manager.StatusData(revmsg.status);
                    }
                    else if (revmsg.type == MSG.Type.COMPONENT)
                    {
                        manager.ComponentData(revmsg.component);
                    }
                    else if (revmsg.type == MSG.Type.EVENT)
                    {
                        manager.CaptureData(revmsg.evt);
                    }
                }
                catch (Exception ex)
                {
                    //异常日志入库
                    ProtoBDManager.AddReceiveLog<MSG>("Exception", revmsg, ex.Message);
                    continue;
                }
                Thread.Sleep(100);
            }
        }
    }
}
