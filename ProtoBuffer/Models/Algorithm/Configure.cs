using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer.Models
{
	/// <summary>
	/// 算法配置
	/// </summary>
	[ProtoContract]
    public class Configure
    {
		/// <summary>
		/// 摄像机uuid标识
		/// </summary>
		[ProtoMember (1)]
		public string cid { get; set; }
		/// <summary>
		/// gpu序号
		/// </summary>
		[ProtoMember(2)]
		public int gpu { get; set; }
		/// <summary>
		/// 是否启用睡觉标识
		/// </summary>
		[ProtoMember(3)]
		public bool enablesleep { get; set; }
		/// <summary>
		/// 是否启用打架算法
		/// </summary>
		[ProtoMember(4)]
		public bool enablefight { get; set; }
		/// <summary>
		/// 是否启用安全帽
		/// </summary>
		[ProtoMember(5)]
		public bool enablehelmet { get; set; }
		/// <summary>
		/// 是否启用打电话
		/// </summary>
		[ProtoMember(6)]
		public bool enablephone { get; set; }
		/// <summary>
		/// 是否请求考勤入
		/// </summary>
		[ProtoMember(7)]
		public bool enableattendancein { get; set; }
		/// <summary>
		/// 是否请求考勤出
		/// </summary>
		[ProtoMember(8)]
		public bool enableattendanceout { get; set; }
		/// <summary>
		/// 人脸相似度
		/// </summary>
		[ProtoMember(9)]
		public float similar { get; set; }
	}
}
