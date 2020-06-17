using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProtoBuf.Models
{
    [ProtoContract]
    public class CrewResponse
    {
        [ProtoMember (1)]
        public int result { get; set; }
        [ProtoMember(2)]
        public string uid { get; set; }
        [ProtoMember(3)]
        public List<Employee> employees { get; set; }
    }
}
