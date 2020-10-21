using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer.Models
{
    [ProtoContract]
    public class ComponentResponse
    {
        /// <summary>
        /// 返回结果 0：成功 
        /// </summary>
        [ProtoMember (1)]
        public int result { get; set; }
        /// <summary>
        /// 组件ID
        /// </summary>
        [ProtoMember(2)]
        public string cid { get; set; }

        [ProtoMember(3)]
        /// <summary>
        /// 组件信息
        /// </summary>
        public List<ComponentInfo> componentinfos { get; set; }
    }
}
