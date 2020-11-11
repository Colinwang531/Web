using Microsoft.AspNetCore.SignalR;
using NetMQ;
using NetMQ.Sockets;
using Smartweb.Hubs;
using SmartWeb.Helpers;
using SmartWeb.ProtoBuffer;
using SmartWeb.ProtoBuffer.Models;
using SmartWeb.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer
{
    public class DealerService
    {
        private static DealerSocket dealer = null;
        private static object dealer_Lock = new object(); //锁同步
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

        public void Send(MSG msg, string toId = "", string head = "request")
        {
            try
            {
                if (!dealer.IsDisposed)
                {
                    byte[] byt = ProtoBufHelp.Serialize<MSG>(msg);
                    NetMQMessage mqmsg = new NetMQMessage(6);
                    mqmsg.AppendEmptyFrame();
                    mqmsg.Append("worker");
                    mqmsg.Append(head);
                    mqmsg.Append(ManagerHelp.ComponentId);//当前组件ID
                    mqmsg.Append(toId);//下一级组件ID或上一级的组件ID
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
                try
                {
                    var mQFrames = dealer.ReceiveMultipartMessage(6);
                    byte[] mory = mQFrames.Last.ToByteArray();
                    MSG revmsg = ProtoBufHelp.DeSerialize<MSG>(mory);
                    //消息来源
                    var fromId = mQFrames[3].ConvertToString();
                    var toId = mQFrames[4].ConvertToString();
                    if (revmsg == null) continue;
                    if (revmsg.type == MSG.Type.ALGORITHM || revmsg.type == MSG.Type.DEVICE || revmsg.type == MSG.Type.STATUS || revmsg.type == MSG.Type.CREW)
                    {
                        var component = ProtoBDManager.GetComponentById(fromId);
                        if (component == null)
                        {
                            //记录从陆地端传过来的ID，船舶端发送时做为上级ID传值
                            ManagerHelp.UpToId = fromId;
                        }
                    }
                    if (revmsg.type == MSG.Type.COMPONENT)
                    {
                        manager.ComponentData(revmsg.component);
                    }
                    if (revmsg.type == MSG.Type.ALARM)
                    {
                        if (revmsg.alarm.alarminfo != null)
                        {
                            string xmq = "";
                            //陆地端收到船舶推送的报警数据带有upload标识
                            //船舶端收到陆地端的响应带有request标识
                            if (mQFrames[2].ConvertToString() == "upload"|| mQFrames[2].ConvertToString() == "request")
                            {
                                xmq = fromId;                               
                            }
                            manager.AlarmData(xmq, revmsg.alarm.alarminfo);
                            //var ss = new ProtoBDManager();
                            //ss.AlarmAdd(revmsg.alarm.alarminfo, xmq);
                        }
                    }
                    else if (revmsg.type == MSG.Type.EVENT)
                    {
                        if (revmsg.evt != null)
                        {
                            string xmqId = "";
                            if (mQFrames[2].ConvertToString() == "upload")
                            {
                                xmqId = fromId;
                            }
                            manager.EventData(revmsg.evt, xmqId);
                        }
                    }
                    Task.Factory.StartNew(st =>
                    {
                        try
                        {
                            if (revmsg.type == MSG.Type.ALGORITHM)
                            {
                                manager.AlgorithmData(revmsg.algorithm, fromId);
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
                        }
                        catch (Exception ex)
                        {
                        //异常日志入库
                        ProtoBDManager.AddReceiveLog<MSG>("Exception", revmsg, ex.Message);
                        }
                    }, TaskCreationOptions.LongRunning);
                }
                catch (Exception ex)
                {
                    //异常日志入库
                    ProtoBDManager.AddReceiveLog("Exception", "数据处理异常", ex.Message);
                    continue;
                }
                Thread.Sleep(100);
            }
        }
    }
}
