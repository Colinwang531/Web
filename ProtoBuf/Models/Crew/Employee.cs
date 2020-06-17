using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProtoBuf.Models
{
    /// <summary>
    /// 雇员信息
    /// </summary>
    [ProtoContract]
    public class Employee
    {
        /// <summary>
        /// 雇员名称
        /// </summary>
        [ProtoMember(1)]
        public string name { get; set; }
        /// <summary>
        /// 雇员负责工作
        /// </summary>
        [ProtoMember(2)]
        public string job { get; set; }
        /// <summary>
        /// 雇员照片
        /// </summary>
        [ProtoMember(3)]
        public List<byte[]> pictures { get; set; }
    }
}