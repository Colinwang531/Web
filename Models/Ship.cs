using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    /// <summary>
    /// 船表
    /// </summary>
    public class Ship
    {
        public enum Type
        {
            /// <summary>
            /// 自动
            /// </summary>
            AUTO = 1,
            /// <summary>
            /// 航行
            /// </summary>
            SAIL = 2,
            /// <summary>
            /// 停港
            /// </summary>
            PORT = 3
        }
        /// <summary>
        /// 主键id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 船名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 航行类型 1：SAIL 航行 2:自动 3：名称
        /// </summary>
        public Type type { get; set; }
        /// <summary>
        /// 是否在港口，true表示航行，false表示停港
        /// </summary>
        public bool Flag { get; set; }
    }
    
}
