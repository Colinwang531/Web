using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    /// <summary>
    /// 报警消息点位表(既触发报警的图形位)
    /// </summary>
    public class AlarmPosition
    {
        /// <summary>
        /// 
        /// </summary>
      public string Id { get; set; }
        /// <summary>
        /// x座标
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// y座标
        /// </summary>
        public int Y { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        public int W { get; set; }
        /// <summary>
        /// 高度
        /// </summary>
        public int H { get; set; }
        /// <summary>
        /// 报警信息ID
        /// </summary>
        public string AlarmInfoId { get; set; }
        /// <summary>
        /// 船表主键id
        /// </summary>
        public string ShipId { get; set; }
    }
}
