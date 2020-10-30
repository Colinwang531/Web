using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.Models
{
    public class SearchAttendance
    { /// <summary>
      /// 船员名称
      /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 打卡时间开始
        /// </summary>
        public string StartTime { get; set; }
        /// <summary>
        /// 打卡结束时间
        /// </summary>
        public string EndTime { get; set; }
        /// <summary>
        /// 考勤入/考勤出
        /// </summary>
        public string Behavior { get; set; }
        /// <summary>
        /// 工作内容
        /// </summary>
        public string Job { get; set; }
    }
}
