using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    /// <summary>
    /// 报警
    /// </summary>
    public class Alarm
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 摄像机ID
        /// </summary>
        public string Cid { get; set; }
        /// <summary>
        /// 报警时间
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 报警实时图片 以JEPG格式封装
        /// </summary>
        public byte[] Picture { get; set; }

        /// <summary>
        /// 船表主键id
        /// </summary>
        public string ShipId { get; set; }
        public AlarmInfo alarmInfo { get; set; }
    }
}
