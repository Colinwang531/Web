using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    /// <summary>
    /// 船员签到表，签入 签出
    /// </summary>
    public class Attendance
    {
        public string Id { get; set; }
        /// <summary>
        /// 船员表主键id
        /// </summary>
        public string EmployeeId { get; set; }
        /// <summary>
        /// 摄像机主键id
        /// </summary>
        public string CameraId { get; set; }
        /// <summary>
        /// 行为时间，签入或签出的时间
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 行为，0：签入，1：签出
        /// </summary>
        public int Behavior { get; set; }
        /// <summary>
        /// 船表主键id
        /// </summary>
        public string ShipId { get; set; }
    }
}
