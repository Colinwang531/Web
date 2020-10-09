using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer.Models
{
	/// <summary>
	/// 报警信息
	/// </summary>
	[ProtoContract]
    public class AlarmInfo
    {
		public enum Type
		{
			/// <summary>
			/// 安全帽
			/// </summary>
			HELMET = 1,
			/// <summary>
			/// 打电话
			/// </summary>
			PHONE = 2,
			/// <summary>
			/// 睡觉
			/// </summary>
			SLEEP = 3,
			/// <summary>
			/// 打架
			/// </summary>
			FIGHT = 4,
			/// <summary>
			/// 考勤入
			/// </summary>
			ATTENDANCE_IN = 5,

			/// <SUMMARY>
			/// 考勤出
			/// </SUMMARY>
			ATTENDANCE_OUT = 6
		}
		[ProtoMember (1)]
		public Type type = Type.SLEEP;
		[ProtoMember(2)]
		/// <summary>
		/// 摄像机ID
		/// </summary>
		public string cid { get; set; }

		[ProtoMember(3)]
		/// <summary>
		/// 报警时间
		/// </summary>
		public string time { get; set; }

		[ProtoMember(4)]
		/// <summary>
		/// 报警图片
		/// </summary>
		public byte[] picture { get; set; }

		[ProtoMember(5)]
		public List<AlarmPosition> position { get; set; }

		[ProtoMember(6)]
		public int uid { get; set; }
    }
	/// <summary>
	/// 报警坐标
	/// </summary>
	[ProtoContract]
	public class AlarmPosition
	{
		[ProtoMember (1)]
		public int x { get; set; }
		[ProtoMember(2)]
		public int y { get; set; }
		[ProtoMember(3)]
		public int w { get; set; }
		[ProtoMember(4)]
		public int h { get; set; }
	}
}
