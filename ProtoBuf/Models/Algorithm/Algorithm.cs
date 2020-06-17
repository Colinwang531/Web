using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProtoBuf.Models
{
	/// <summary>
	/// 算法
	/// </summary>
	[ProtoContract]
    public class Algorithm
    {
		public enum Command
		{
			/// <summary>
			/// 算法请求
			/// </summary>
			CONFIGURE_REQ = 1,
			/// <summary>
			/// 算法响应
			/// </summary>
			CONFIGURE_REP = 2,
			/// <summary>
			/// 查询请求
			/// </summary>
			QUERY_REQ = 3,
			/// <summary>
			/// 查询响应
			/// </summary>
			QUERY_REP = 4
		}
		[ProtoMember (1)]
		public Command command = Command.CONFIGURE_REP;
		[ProtoMember(2)]
		public AlgorithmRequest algorithmrequest { get; set; }
		[ProtoMember(3)]
		public AlgorithmResponse algorithmresponse { get; set; }
    }
}
