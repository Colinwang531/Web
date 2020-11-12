using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.Models
{
    /// <summary>
    /// 报警
    /// </summary>
    public class Alarm
    {
        public Alarm() {
            CreatDate = DateTime.Now;
            this.IsSyncSucces = false;
            this.ShipAlarmId = "";
        }
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
            ATTENDANCE_OUT = 6,
            /// <summary>
            /// 缺岗
            /// </summary>
            CAPTURE=7
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
        /// 摄像名称
        /// </summary>
        public string Cname { get; set; }
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
        public int Uid { get; set; }
        /// <summary>
        /// 船表主键id
        /// </summary>
        public string ShipId { get; set; }
        /// <summary>
        /// 入库时间
        /// </summary>
        public DateTime CreatDate { get; set; }
        /// <summary>
        /// 是否同步完成
        /// </summary>
        public bool IsSyncSucces { get; set; }
        /// <summary>
        /// 陆地端此值有效（存的是船舶端报警表的主键ID）
        /// </summary>
        public string ShipAlarmId { get; set; }
        public List<AlarmPosition> alarmPositions { get; set; }
    }
}
