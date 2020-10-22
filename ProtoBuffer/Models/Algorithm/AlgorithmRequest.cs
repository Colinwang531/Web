using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer.Models
{
    [ProtoContract]
    public class AlgorithmRequest
    {
        /// <summary>
        /// 配置
        /// </summary>
        [ProtoMember (1)]
        public AlgorithmInfo algorithminfo { get; set; }
    }
}
