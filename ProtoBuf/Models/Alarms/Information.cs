using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProtoBuf.Models
{
	/// <summary>
	/// 报警信息
	/// </summary>
	[ProtoContract]
    public class Information
    {
		public enum Type
		{
			/// <summary>
			/// 睡觉
			/// </summary>
			SLEEP = 1,
			/// <summary>
			/// 打架
			/// </summary>
			FIGHT = 2,
			/// <summary>
			/// 安全帽
			/// </summary>
			HELMET = 3,
			/// <summary>
			/// 手机
			/// </summary>
			PHONE = 4,
			/// <summary>
			/// 脸
			/// </summary>
			FACE = 5
		}
		[ProtoMember (1)]
		public Type type = Type.SLEEP;
		[ProtoMember(2)]
		public List<Position> position { get; set; }
		[ProtoMember(3)]
		public string uid { get; set; }
    }
	/// <summary>
	/// 报警坐标
	/// </summary>
	[ProtoContract]
	public class Position
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
