using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer.Models
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
			/// 导航系统
			/// </summary>
			AIS = 2,
			/// <summary>
			/// 母钟
			/// </summary>
			CLOCK = 3,
			/// <summary>
			/// 名称
			/// </summary>
			NAME = 4

		}
		[ProtoMember(1)]
		public Type type = Type.SAIL;

		/// <summary>
		/// 当类型为SAIL 1:航行 2:停港
		/// </summary>
		[ProtoMember(2)]
		public int flag { get; set; }

		/// <summary>
		/// 当类型为CLOCK/NAME 此值有效
		/// </summary>
		[ProtoMember(3)]
		public string text { get; set; }
	}
}
