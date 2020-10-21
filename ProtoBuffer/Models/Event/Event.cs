using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer.Models
{
	[ProtoContract]
	public class Event
    {
		public enum Command
		{
			CAPTURE_JPEG_REQ = 1,
			CAPTURE_JPEG_REP = 2
		}
		[ProtoMember(1)]
		/// <summary>
		/// 
		/// </summary>
		public Command command { get; set; }
		/// <summary>
		/// 缺岗信息
		/// </summary>
		[ProtoMember(2)]
		public CaptureInfo captureinfo { get; set; }
    }
}
