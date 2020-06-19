using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer.Models
{
	/// <summary>
	/// 消息协议
	/// </summary>
	[ProtoContract]
	public class MSG
	{
		public enum Type
		{
			/// <summary>
			/// 空
			/// </summary>
			NONE = 0,
			/// <summary>
			/// 报警
			/// </summary>
			ALARM = 1,
			/// <summary>
			/// 算法
			/// </summary>
			ALGORITHM = 2,
			/// <summary>
			/// 组件
			/// </summary>
			COMPONENT = 3,
			/// <summary>
			/// 般员
			/// </summary>
			CREW = 4,
			/// <summary>
			/// 设备
			/// </summary>
			DEVICE = 5,
			/// <summary>
			/// 状态
			/// </summary>
			STATUS = 6,
			/// <summary>
			/// 用户
			/// </summary>
			USER = 7
		}
		/// <summary>
		/// 消息类型 必填
		/// </summary>
		[ProtoMember(1)]
		public Type type = Type.NONE;
		/// <summary>
		/// 发送消息的序号 必填
		/// </summary>
		[ProtoMember(2)]
		public long sequence { get; set; }
		/// <summary>
		/// 发送时间（长整型）必填
		/// </summary>
		[ProtoMember(3)]
		public long timestamp { get; set; }
		/// <summary>
		/// 报警
		/// </summary>
		[ProtoMember(4)]
		public Alarm alarm { get; set; }
		/// <summary>
		/// 算法
		/// </summary>
		[ProtoMember(5)]
		public Algorithm algorithm { get; set; }
		/// <summary>
		/// 组件
		/// </summary>
		[ProtoMember(6)]
		public Component component { get; set; }
		/// <summary>
		/// 船员
		/// </summary>
		[ProtoMember(7)]
		public Crew crew { get; set; }
		/// <summary>
		/// 设备
		/// </summary>
		[ProtoMember(8)]
		public Device device { get; set; }
		/// <summary>
		/// 状态
		/// </summary>
		[ProtoMember(9)]
		public Status status { get; set; }
		/// <summary>
		/// 用户
		/// </summary>
		[ProtoMember(10)]
		public User user { get; set; }
	}
}
