using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.Models
{
    /// <summary>
    /// 月报警量统计
    /// </summary>
    public class MonthAlarm
    {
        public DateTime Time { get; set; }
        public int Count { get; set; }
    }
}
