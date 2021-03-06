﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer.Models
{
	[ProtoContract]
    public class Status
    {
		public enum Command
		{
			/// <summary>
			/// 设置请求
			/// </summary>
			SET_REQ = 1,
			/// <summary>
			/// 设置响应
			/// </summary>
			SET_REP = 2,
			QUERY_REQ = 3,
			QUERY_REP = 4
		}
		/// <summary>
		/// 执行的操作 必填
		/// </summary>
		[ProtoMember (1)]
		public Command command = Command.SET_REQ;
		[ProtoMember(2)]
		public StatusRequest statusrequest { get; set; }
		[ProtoMember(3)]
		public StatusResponse statusresponse { get; set; }
    }
}
