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
			CAPTURE_JPEG_REP = 2,
			SYNC_CLOCK = 3,
			SYNC_AIS = 4
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
		/// <summary>
		/// 母钟
		/// </summary>
		[ProtoMember(3)]
		public Clock clock { get; set; }
		/// <summary>
		/// 航海定位系统
		/// </summary>
		[ProtoMember(4)]
		public Ais ais { get; set; }
    }
}
