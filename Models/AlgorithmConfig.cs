using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    public class AlgorithmConfig
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 算法类型
        /// </summary>
        public AlgorithmType Type { get; set; }
        /// <summary>
        /// 摄像机ID
        /// </summary>
        public string Cid { get; set; }
        /// <summary>
        /// GPU(0,1)
        /// </summary>
        public int GPU { get; set; }
        /// <summary>
        /// 人脸相似度（0.1-0.99）
        /// </summary>
        public double Similar { get; set; }
        public string ShipId { get; set; }
    }
    public enum AlgorithmType {
        /// <summary>
        /// 睡觉
        /// </summary>
        ENABLESLEEP = 1,
        /// <summary>
        /// 打架
        /// </summary>
        ENABLEFIGHT = 2,
        /// <summary>
        /// 安全帽
        /// </summary>
        ENABLEHELMET = 3,
        /// <summary>
        /// 打电话
        /// </summary>
        ENABLEPHONE = 4,
        /// <summary>
        /// 考勤入
        /// </summary>
        ENABLEATTENDANCEIN = 5,
        /// <summary>
        /// 考勤出
        /// </summary>
        ENABLEATTENDANCEOUT = 6
    }
}
