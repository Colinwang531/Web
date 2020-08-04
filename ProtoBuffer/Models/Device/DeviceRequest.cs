using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer.Models
{
    [ProtoContract]
    public class DeviceRequest
    {
        [ProtoMember (1)]
        public DeviceInfo deviceinfo { get; set; }
        [ProtoMember(2)]
        public string did { get; set; }
    }
}
