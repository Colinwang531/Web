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
        public enum AlarmType
        {
            /// <summary>
            /// 安全帽
            /// </summary>
            HELMET = 1,
            /// <summary>
            /// 打电话
            /// </summary>
            PHONE = 2,
            /// <summary>
            /// 睡觉
            /// </summary>
            SLEEP = 3,
            /// <summary>
            /// 打架
            /// </summary>
            FIGHT = 4,
            /// <summary>
            /// 考勤入
            /// </summary>
            ATTENDANCE_IN = 5,

            /// <SUMMARY>
            /// 考勤出
            /// </SUMMARY>
            ATTENDANCE_OUT = 6
        }
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 报警类型
        /// </summary>
        public AlarmType Type { get; set; }
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
        /// 用户ID
        /// </summary>
        public string Uid { get; set; }
        /// <summary>
        /// 船表主键id
        /// </summary>
        public string ShipId { get; set; }
        public List<AlarmPosition> alarmPositions { get; set; }
    }
}
