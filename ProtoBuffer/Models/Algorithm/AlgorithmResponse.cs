﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer.Models
{
    [ProtoContract]
    public class AlgorithmResponse
    {
        [ProtoMember (1)]
        public int result { get; set; }

        [ProtoMember(2)]
        public List<AlgorithmInfo> configures { get; set; }
    }
}
