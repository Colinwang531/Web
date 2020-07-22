using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class AttendancePicture
    {
        public string Id { get; set; }
        /// <summary>
        /// 船员签到图片
        /// </summary>
        public byte[] Picture { get; set; }
        /// <summary>
        /// 船员签到表主键id
        /// </summary>
        public string AttendanceId { get; set; }
        /// <summary>
        /// 船表主键id
        /// </summary>
        public string ShipId { get; set; }
    }
}
