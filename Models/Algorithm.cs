using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.Models
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
        public float Similar { get; set; }
        /// <summary>
        /// 检测阀值1
        /// </summary>
        public float DectectFirst { get; set; }
        /// <summary>
        /// 检测阀值2
        /// </summary>
        public float DectectSecond { get; set; }
        /// <summary>
        /// 跟踪阀值
        /// </summary>
        public float Track { get; set; }
        /// <summary>
        /// 船ID
        /// </summary>
        public string ShipId { get; set; }

    }
}
