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
        public enum ShipType
        {
            SAIL = 1,
            AUTO = 2,
            NAME = 3
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
        public ShipType Type { get; set; }
        /// <summary>
        /// 是否在港口，true表示航行，false表示停港
        /// </summary>
        public bool Flag { get; set; }
    }
    
}
