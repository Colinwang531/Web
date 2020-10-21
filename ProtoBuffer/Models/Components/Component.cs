using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer.Models
{
	/// <summary>
	/// 组件类
	/// </summary>
	[ProtoContract]
    public class Component
    {
		public enum Command
		{
			/// <summary>
			/// 组件注册请求
			/// </summary>
			SIGNIN_REQ = 1,
			/// <summary>
			/// 组件注册响应
			/// </summary>
			SIGNIN_REP = 2,
			/// <summary>
			/// 组件退出请求
			/// </summary>
			SIGNOUT_REQ = 3,
			/// <summary>
			/// 组件退出响应
			/// </summary>
			SIGNOUT_REP = 4,
			/// <summary>
			/// 组件查询请求
			/// </summary>
			QUERY_REQ=5,
			/// <summary>
			/// 组件查询响应
			/// </summary>
			QUERY_REP = 6
		}
		/// <summary>
		/// 组件命令
		/// </summary>
		[ProtoMember(1)]
		public Command command = Command.SIGNIN_REP;
		/// <summary>
		/// 组件请求
		/// </summary>
		[ProtoMember(2)]
		public ComponentRequest componentrequest { get; set; }
		/// <summary>
		/// 组件响应
		/// </summary>
		[ProtoMember(3)]
		public ComponentResponse componentresponse { get; set; }
    }
}
