using System;
using System.Collections.Generic;
using System.Linq;
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
        /// 组件id标识,用CPU@进程id标识
        /// </summary>
        public string Cid { get; set; }
        /// <summary>
        /// 组件类型 1：XMQ，2：WEB ，3：HKD，4：DHD，5：ALM 
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 组件名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 船表主键id
        /// </summary>
        public string ShipId { get; set; }
    }
}
