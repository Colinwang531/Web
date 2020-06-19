using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Tool
{
    public class ManagerHelp
    {
        private static ManagerHelp _manager = null;
        public static string ShipId = "";
        public static string Cid = "5555";
        /// <summary>
        /// 是否有设置权限
        /// </summary>
        public static bool IsSet = true;
        public static Dictionary<int, string> components;
        private static object ShipId_Lock = new object(); //锁同步
        private static object Cid_Lock = new object(); //锁同步
        //int aa;
        //单例控制dealer只有一个。
        public ManagerHelp()
        {
            lock (ShipId_Lock)
            {
                if (ShipId == null)
                {
                    _manager = new ManagerHelp();
                }
            }
            lock (Cid_Lock)
            {
                if (Cid == null)
                {
                    _manager = new ManagerHelp();
                }
            }
        }
    }
}
