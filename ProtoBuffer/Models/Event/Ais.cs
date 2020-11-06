using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer.Models
{
    /// <summary>
    /// 航海定位系统
    /// </summary>
    [ProtoContract]
    public class Ais
    {
        /// <summary>
        /// 航海定位状态0:航行 1—15是停港
        /// </summary>
        [ProtoMember(1)]
        public int status { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        [ProtoMember(2)]
        public string longitude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        [ProtoMember(3)]
        public string latitude { get; set; }
    }
}
