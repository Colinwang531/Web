using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProtoBuf.Models
{
	[ProtoContract]
    public class StatusRequest
    {
		public enum Type
		{
			/// <summary>
			/// 航行
			/// </summary>
			SAIL = 1,
			/// <summary>
			/// 自动
			/// </summary>
			AUTO = 2,
			/// <summary>
			/// 名称
			/// </summary>
			NAME = 3

		}
		[ProtoMember(1)]
		public Type type = Type.SAIL;

		/// <summary>
		/// 是否在港口
		/// </summary>
		[ProtoMember(2)]
		public bool flag { get; set; }

		/// <summary>
		/// 修改船名称
		/// </summary>
		[ProtoMember(3)]
		public string name { get; set; }
	}
}
