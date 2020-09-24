using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    /// <summary>
    /// 组件表用于存放组件的注册信息
    /// </summary>
    public class Component
    {
        /// <summary>
        /// 主键id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 通讯ID
        /// </summary>
        public string CommId { get; set; }
        /// <summary>
        /// 组件类型 1：XMQ，2：WEB ，3：HKD，4：DHD，5：ALM 6:AI 7:MED
        /// </summary>
        public ComponentType Type { get; set; }
        /// <summary>s
        /// 组件名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 在线/离线 (0:在线 1：离线)
        /// 当它为船舶端数据时一直是在线状态
        /// 当它为陆地端数据时才可能出现离线状态
        /// </summary>
        public int Line { get; set; }
        /// <summary>
        /// 船表主键id
        /// </summary>
        public string ShipId { get; set; }
    }
}
