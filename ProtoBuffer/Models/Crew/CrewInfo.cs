using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer.Models
{
    /// <summary>
    /// 雇员信息
    /// </summary>
    [ProtoContract]
    public class CrewInfo
    {
        [ProtoMember(1)]
        public string uid { get; set; }
        /// <summary>
        /// 雇员名称
        /// </summary>
        [ProtoMember(2)]
        public string name { get; set; }
        /// <summary>
        /// 雇员负责工作
        /// </summary>
        [ProtoMember(3)]
        public string job { get; set; }
        /// <summary>
        /// 雇员照片
        /// </summary>
        [ProtoMember(4)]
        public List<byte[]> pictures { get; set; }
    }
}