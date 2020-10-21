using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.Models
{
    public class Fleet
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 船队名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 船队管理人
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 船队管理人电话
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 管理的船ID(多个船员用逗号隔开)
        /// </summary>
        public string ShipIds { get; set; }
    }
}
