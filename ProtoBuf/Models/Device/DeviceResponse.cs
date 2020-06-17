using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProtoBuf.Models
{
    [ProtoContract]
    public class DeviceResponse
    {
        [ProtoMember (1)]
        public int result { get; set; }
        [ProtoMember(2)]
        public string did { get; set; }
        [ProtoMember(3)]
        public List<Embedded> embedded { get; set; }
    }
}
