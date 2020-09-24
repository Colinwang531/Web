using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    public class ComponentToken
    {
        /// <summary>
        /// 组件ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 通讯ID
        /// </summary>
        public string CommId { get; set; }
        /// <summary>
        /// 组件名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 组件类型
        /// </summary>
        public ComponentType Type { get; set; }
    }
}
