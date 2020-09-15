using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
            /// 自动航行
            /// </summary>
            AUTO = 0,
            /// <summary>
            /// 手动航行
            /// </summary>
            SAIL = 1,
            /// <summary>
            /// 手动停港
            /// </summary>
            PORT = 2
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
        /// 航行类型
        /// </summary>
        public Type type { get; set; }
        /// <summary>
        /// 航状态  true:航行 false:停港
        /// </summary>
        public bool Flag { get; set; }

        /// <summary>
        /// 坐标(经纬度数组)
        /// </summary>
        public string Coordinate { get; set; }

        /// <summary>
        /// 船员数量
        /// </summary>
        public int? CrewNum { get; set; }


    }

}
