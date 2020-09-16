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
	public class AlgorithmInfo
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
		/// <summary>
		/// 算法类型
		/// </summary>
		[ProtoMember(1)]
		public Type type = Type.HELMET;
		/// <summary>
		/// gpu序号
		/// </summary>
		[ProtoMember(2)]
		public int gpu { get; set; }
		/// <summary>
		/// 摄像机uuid标识
		/// </summary>
		[ProtoMember(3)]
		public string cid { get; set; }

		[ProtoMember(4)]
		/// <summary>
		/// 算法Id
		/// </summary>
		public string aid { get; set; }
		/// <summary>
		/// 人脸相似度
		/// </summary>
		[ProtoMember(5)]
		public float dectectfirst { get; set; }
		/// <summary>
		/// 人脸相似度
		/// </summary>
		[ProtoMember(6)]
		public float track { get; set; }
		/// <summary>
		/// 人脸相似度
		/// </summary>
		[ProtoMember(7)]
		public float dectectsecond { get; set; }
		/// <summary>
		/// 人脸相似度
		/// </summary>
		[ProtoMember(8)]
		public float similar { get; set; }
	}
}
