using NetMQ;
using NetMQ.Sockets;
using SmartWeb.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer
{
    public class PlayerService
    {
        private static PublisherSocket publisher = null;
        private static object pub_Lock = new object(); //锁同步
        //public static List<Task> taskList = new List<Task>();
        public PlayerService()
        {
            lock (pub_Lock)
            {
                if (publisher == null)
                {
                    publisher = new PublisherSocket();
                    publisher.Bind(ManagerHelp.PlayerIP);
                }
            }
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        public void Send(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                publisher.SendMoreFrame("A").SendFrame(data);
            }
        }
    }
}
