using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    /// <summary>
    /// 摄像机算法配置表
    /// </summary>
    public class CameraConfig
    {
       // public string NickName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 摄像机uuid标识
        /// </summary>
        public string Cid { get; set; }
        /// <summary>
        /// gpu序号
        /// </summary>
        public int GPU { get; set; }
        /// <summary>
        /// 是否启用睡觉标识
        /// </summary>
        public bool EnableSleep { get; set; }
        /// <summary>
        /// 是否启用打架算法
        /// </summary>
        public bool EnableFight { get; set; }
        /// <summary>
        /// 是否启用安全帽
        /// </summary>
        public bool EnableHelmet { get; set; }
        /// <summary>
        /// 是否启用打电话
        /// </summary>
        public bool EnablePhone { get; set; }
        /// <summary>
        /// 是否请求考勤入
        /// </summary>
        public bool EnableAttendanceIn { get; set; }
        /// <summary>
        /// 是否请求考勤出
        /// </summary>
        public bool EnableAttendanceOut { get; set; }
        /// <summary>
        /// 人脸相似度，取值范围(0.1-1.0)
        /// </summary>
        public double Similar { get; set; }
        /// <summary>
        /// 船表主键id
        /// </summary>
        public string ShipId { get; set; }
    }
}
