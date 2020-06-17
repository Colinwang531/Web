using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProtoBuf.Models
{
	/// <summary>
	/// 组件注册
	/// </summary>
	[ProtoContract]	
	public class ComponentRequest
	{
		public enum Type
		{
			XMQ = 1,
			WEB = 2,
			HKD = 3,
			DHD = 4,
			ALM = 5
		}
		public enum Operate
		{
			/// <summary>
			/// 注册
			/// </summary>
			SIGNIN = 1,
			/// <summary>
			/// 退出
			/// </summary>
			SIGNOUT = 2
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
