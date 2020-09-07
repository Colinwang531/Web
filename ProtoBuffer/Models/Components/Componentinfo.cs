using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer.Models
{
	[ProtoContract]
	public class ComponentInfo
    {
		public enum Type
		{
			/// <summary>
			/// 消息分发组件
			/// </summary>
			XMQ = 1,
			/// <summary>
			/// WEB组件
			/// </summary>
			WEB = 2,
			/// <summary>
			/// 海康组件
			/// </summary>
			HKD = 3,
			/// <summary>
			/// 大华组件
			/// </summary>
			DHD = 4,
			/// <summary>
			/// 报警组件
			/// </summary>
			ALM = 5
		}
		/// <summary>
		/// 注册类型
		/// </summary>
		[ProtoMember(1)]
		public Type type = Type.WEB;
		/// <summary>
		/// 组件ID
		/// </summary>
		[ProtoMember(2)]
		public string cid { get; set; }
		/// <summary>
		/// 组件名称
		/// </summary>
		[ProtoMember(3)]
		public string cname { get; set; }
	}
}
