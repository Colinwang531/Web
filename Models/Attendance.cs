using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.Models
{
    /// <summary>
    /// 船员签到表，签入 签出
    /// </summary>
    public class Attendance
    {
        public Attendance() {
            this.CreateTime = DateTime.Now;
            this.CameraName = "";
            this.CrewJob = "";
            this.CameraName = "";
            this.IsSyncSucces = false;
            this.ShipAttendanceId = "";
        }
        public string Id { get; set; }
        /// <summary>
        /// 船员表主键id
        /// </summary>
        public int CrewId { get; set; }
        /// <summary>
        /// 船员名称
        /// </summary>
        public string CrewName { get; set; }
        /// <summary>
        /// 船员工作内容
        /// </summary>
        public string CrewJob { get; set; }
        /// <summary>
        /// 摄像机主键id
        /// </summary>
        public string CameraId { get; set; }
        /// <summary>
        /// 摄像机名称
        /// </summary>
        public string CameraName { get; set; }
        /// <summary>s
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
        /// <summary>
        /// 入库时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 是否同步完成
        /// </summary>
        public bool IsSyncSucces { get; set; }
        /// <summary>
        /// 陆地端时此值有效 存的是船舶端报警表的主键ID
        /// </summary>
        public string ShipAttendanceId { get; set; }
        public List<AttendancePicture> attendancePictures { get; set; }
    }
}
