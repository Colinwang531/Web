using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    public enum AlgorithmType
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
        /// <summary>
        /// 考勤出
        /// </summary>
        ATTENDANCE_OUT = 6,
        /// <summary>
        /// 缺岗
        /// </summary>
        CAPTURE=7
    }
}
