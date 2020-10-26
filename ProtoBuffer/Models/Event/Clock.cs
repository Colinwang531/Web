using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer.Models
{
    [ProtoContract]
    public class Clock
    {
        /// <summary>
        /// 母钟时间
        /// </summary>
        [ProtoMember(1)]
        public string time { get; set; }
    }
}
