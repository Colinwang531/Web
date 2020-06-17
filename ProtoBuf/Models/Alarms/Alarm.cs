using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProtoBuf.Models
{
    /// <summary>
    /// 报警类
    /// </summary>
    [ProtoContract]
    public class Alarm
    {
        /// <summary>
        /// 摄像机ID标识
        /// </summary>
        [ProtoMember(1)]
        public string cid { get; set; }
        /// <summary>
        /// 报警时间
        /// </summary>
        [ProtoMember(2)]        
        public string time { get; set; }
        /// <summary>
        /// 报警实时图片，以JEPG格式封装；
        /// </summary>
        [ProtoMember(3)]
        public string picture { get; set; }
        /// <summary>
        /// 报警信息
        /// </summary>
        [ProtoMember(4)]
        public Information information { get; set; }
    }
}
