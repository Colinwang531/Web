using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer.Models
{
    [ProtoContract]
    public class StatusResponse
    {
        [ProtoMember(1)]
        public int result { get; set; }
        [ProtoMember(2)]
        public bool flag { get; set; }
        [ProtoMember(3)]
        public string name { get; set; }
    }
}
