using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    public class AttendanceViewModel
    {
        /// <summary>
        /// 船员名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 打卡时间
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// 考勤入/考勤出
        /// </summary>
        public string Behavior { get; set; }
        /// <summary>
        /// 考勤图片
        /// </summary>
        public byte[] Picture { get; set; }
    }
}
