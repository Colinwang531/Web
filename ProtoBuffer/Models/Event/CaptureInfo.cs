using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer.Models
{
    [ProtoContract]
    public class CaptureInfo
    {
        [ProtoMember(1)]
        /// <summary>
        /// 设备ID
        /// </summary>
        public string did { get; set; }
        [ProtoMember(2)]
        /// <summary>
        /// 摄像机ID
        /// </summary>
        public string cid { get; set; }
        [ProtoMember(3)]
        /// <summary>
        /// 摄像机序号
        /// </summary>
        public int idx { get; set; }
        [ProtoMember(4)]
        /// <summary>
        /// 缺岗图片
        /// </summary>
        public string picture { get; set; }
    }
}
