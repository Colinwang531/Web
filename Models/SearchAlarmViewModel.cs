using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    public class SearchAlarmViewModel
    {
        /// <summary>
        /// 船ID
        /// </summary>
        public string ShipId { get; set; }
        /// <summary>
        /// 摄像机ID
        /// </summary>
        public string Cid { get; set; }
        /// <summary>
        /// 摄像机名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 报警类型
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 报警开始时间
        /// </summary>
        public string StartTime { get; set; }
        /// <summary>
        /// 报警结束时间
        /// </summary>
        public string EndTime { get; set; }
        /// <summary>
        /// 当前页
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 每页显示条数
        /// </summary>
        public int PageSize { get; set; }
    }
}
