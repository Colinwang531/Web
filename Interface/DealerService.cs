using NetMQ;
using NetMQ.Sockets;
using ShipWeb.ProtoBuffer;
using ShipWeb.ProtoBuffer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipWeb.Interface
{
    public class DealerService : IDealerService
    {
        public const string IP = "tcp://192.168.0.22:61001";//接收从main入口过来的url
        public void SendMessage(MSG msg,string identity)
        {
            using (var dealer = new DealerSocket(IP))
            {
                dealer.Options.Identity =Encoding.UTF8.GetBytes(identity);
                byte[] byt = ProtoBufHelp.Serialize<MSG>(msg);
                NetMQMessage mqmsg = new NetMQMessage();
                mqmsg.AppendEmptyFrame();
                mqmsg.Append(byt);
                //发送注册请求
                bool flag = dealer.TrySendMultipartMessage(mqmsg);
            }
        }
        public MSG ReviceMessage()
        {
            using (var dealer = new DealerSocket(IP))
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
}
