using NetMQ;
using SmartWeb.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer
{
    public class PublisherService
    {
        private static NetMQ.Sockets.PublisherSocket publisher = null;
        private static object pub_Lock = new object(); //锁同步
        //public static List<Task> taskList = new List<Task>();
        public PublisherService()
        {
            lock (pub_Lock)
            {
                if (publisher == null)
                {
                    publisher = new NetMQ.Sockets.PublisherSocket();
                    publisher.Bind(ManagerHelp.PublisherIP);
                }
            }
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">Behavior考勤类型,SignInTime 考勤时间,EmployeeName 员工姓名,PhotosBuffer 考勤图片</param>
        public void Send(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                byte[] by = Encoding.UTF8.GetBytes(data);
                publisher.SendFrame(by);
            }
        }
    }
}
