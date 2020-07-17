using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    public class AlgorithmViewModel
    {

        /// <summary>
        /// 主键ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 算法类型
        /// </summary>
        public int Type { get; set; }
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
        public string Similar { get; set; }
        /// <summary>
        /// 检测阀值1
        /// </summary>
        public string DetectThreshold_1 { get; set; }
        /// <summary>
        /// 检测阀值2
        /// </summary>
        public string DetectThreshold_2 { get; set; }
        /// <summary>
        /// 跟踪阀值
        /// </summary>
        public string TrackThreshold { get; set; }
    }
}
