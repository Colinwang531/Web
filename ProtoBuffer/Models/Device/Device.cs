using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer.Models
{
	/// <summary>
	/// 设备
	/// </summary>
	[ProtoContract]
	public class Device
	{
		public enum Command
		{
			NEW_REQ = 1,
			NEW_REP = 2,
			DELETE_REQ = 3,
			DELETE_REP = 4,
			MODIFY_REQ = 5,
			MODIFY_REP = 6,
			QUERY_REQ = 7,
			QUERY_REP = 8
		}
		[ProtoMember (1)]
		public Command command = Command.NEW_REQ;
		[ProtoMember(2)]
		public DeviceRequest devicerequest { get; set; }
		[ProtoMember(3)]
		public DeviceResponse deviceresponse { get; set; }
    }
}
