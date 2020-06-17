using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer.Models
{
    [ProtoContract]
    public class CrewRequest
    {
        [ProtoMember (1)]
        public Employee employee { get; set; }
        [ProtoMember(2)]
        public string uid { get; set; }
    }
}
