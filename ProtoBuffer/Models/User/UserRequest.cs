using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer.Models
{
	[ProtoContract]
	public class UserRequest
	{
		[ProtoMember (1)]
		public Person person { get; set; }
		[ProtoMember(2)]
		public string uid { get; set; }
	}
}
