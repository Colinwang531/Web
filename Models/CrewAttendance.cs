using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.Models
{
    /// <summary>
    /// 船员考勤
    /// </summary>
    public class CrewAttendance
    {
        /// <summary>
        /// CrewId
        /// </summary>
        public string CrewId { get; set; }

        /// <summary>
        /// ShipId
        /// </summary>
        public string ShipId { get; set; }

        /// <summary>
        /// 船名
        /// </summary>
        public string ShipName { get; set; }
        /// <summary>
        /// 船员名
        /// </summary>
        public string CrewName { get; set; }
        /// <summary>
        /// 考勤时间
        /// </summary>
        public DateTime? Time { get; set; }
        /// <summary>
        /// 行为
        /// </summary>
        public int? Behavior { get; set; }
        /// <summary>
        /// 月考勤率
        /// </summary>
        public double? Rate { get; set; }
        
    }
}
