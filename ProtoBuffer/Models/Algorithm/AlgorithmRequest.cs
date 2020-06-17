using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer.Models
{
    [ProtoContract]
    public class AlgorithmRequest
    {
        /// <summary>
        /// 配置
        /// </summary>
        [ProtoMember (1)]
        public List<Configure> configure { get; set; }
    }
}
