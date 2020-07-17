using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    public class Algorithm
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
        /// <summary>
        /// 检测阀值1
        /// </summary>
        public double DetectThreshold_1 { get; set; }
        /// <summary>
        /// 检测阀值2
        /// </summary>
        public double DetectThreshold_2 { get; set; }
        /// <summary>
        /// 跟踪阀值
        /// </summary>
        public double TrackThreshold { get; set; }
        /// <summary>
        /// 船ID
        /// </summary>
        public string ShipId { get; set; }
    }
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
        /// 人脸
        /// </summary>
        FACE = 5
    }
}
