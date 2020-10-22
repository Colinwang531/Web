using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer.Models
{
    [ProtoContract]
    public class CrewRequest
    {
        [ProtoMember (1)]
        public CrewInfo crewinfo { get; set; }
    }
}
