using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace SmartWeb.Models
{
    public class ComponentViewModel
    {
        /// <summary>
        /// 组件ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 组件类型 
        /// XMQ = 1;
        /// WEB = 2;
        /// HKD = 3;
        /// DHD = 4;
        /// ALM = 5;
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// 组件名称
        /// </summary>
        public string name { get; set; }
    }
}
