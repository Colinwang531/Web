using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer.Models
{
	/// <summary>
	/// 组件注册
	/// </summary>
	[ProtoContract]	
	public class ComponentRequest
	{
		[ProtoMember(1)]
		public ComponentInfo componentinfo { get; set; }
	}
}
