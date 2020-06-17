using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer.Models
{
	/// <summary>
	/// 船员
	/// </summary>
	[ProtoContract]
    public class Crew
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
		/// <summary>
		/// 执行的操作 必填
		/// </summary>
		[ProtoMember (1)]
		public Command command = Command.NEW_REQ;
		[ProtoMember(2)]
		public CrewRequest crewrequest { get; set; }
		[ProtoMember(3)]
		public CrewResponse crewresponse { get; set; }
    }
}
