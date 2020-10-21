using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.Models
{
    public class ShipViewModel
    {
        /// <summary>
        /// 船ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 船名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// true:在线 false:离线
        /// </summary>
        public bool Line { get; set; }
        /// <summary>
        /// 船状态当船处理在线时此值有效 true:航行 false:停港
        /// </summary>
        public bool flag { get; set; }
    }
}
