using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer.Models
{
    [ProtoContract]
    public class Person
    {
        /// <summary>
        /// 用户
        /// </summary>
        [ProtoMember(1)]
        public string name { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        [ProtoMember(2)]
        public string password { get; set; }
        /// <summary>
        /// 授权
        /// </summary>
        [ProtoMember(3)]
        public Author author { get; set; }
    }

    /// <summary>
    /// 授权类
    /// </summary>
    [ProtoContract]
    public class Author
    {
        /// <summary>
        /// 配置权限许可标识
        /// </summary>
        [ProtoMember(1)]
        public bool enableconfigure { get; set; }
        /// <summary>
        /// 查询权限许可标识。
        /// </summary>
        [ProtoMember(2)]
        public bool enablequery { get; set; }
    }
}
