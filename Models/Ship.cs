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
        /// <summary>
        /// 主键id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 船名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 航行类型 1：SAIL 航行
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 是否在港口，true表示航行，false表示停港
        /// </summary>
        public bool Flag { get; set; }
    }
}
