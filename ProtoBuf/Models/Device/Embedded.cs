using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProtoBuf.Models
{
	[ProtoContract]
    public class Embedded
    {
		public enum Factory
		{
			/// <summary>
			/// 海康
			/// </summary>
			HIKVISION = 1,
			/// <summary>
			/// 大华
			/// </summary>
			DAHUA = 2,
			/// <summary>
			/// 伊顿
			/// </summary>
			EATON = 3
		}
		public enum Type
		{
			DVR = 1,
			NVR = 2,
			IPC = 3
		}
		/// <summary>
		/// true表示启动设备，false表示停用设备
		/// </summary>
		[ProtoMember (1)]
		public bool enable = false;
		/// <summary>
		/// 设备厂商类型
		/// </summary>
		[ProtoMember(2)]
		public Factory factory = Factory.DAHUA;
		/// <summary>
		/// 设备类型
		/// </summary>
		[ProtoMember(3)]		
		public Type type = Type.IPC;
		/// <summary>
		/// 设备登陆名称
		/// </summary>
		[ProtoMember(4)]
		public string name { get; set; }
		/// <summary>
		/// 设备登陆密码
		/// </summary>
		[ProtoMember(5)]
		public string password { get; set; }
		/// <summary>
		/// 设备IP地址；
		/// </summary>
		[ProtoMember(6)]
		public string ip { get; set; }
		/// <summary>
		/// 设备端口号
		/// </summary>
		[ProtoMember(7)]
		public int port { get; set; }
		/// <summary>
		/// 设备ID标识；
		/// </summary>
		[ProtoMember(8)]
		public string did { get; set; }
		/// <summary>
		/// 设备别名
		/// </summary>
		[ProtoMember(9)]
		public string nickname { get; set; }
		/// <summary>
		/// 设备可用摄像机集合
		/// </summary>
		[ProtoMember(10)]
		public List<Camera> cameras { get; set; }
	}
	[ProtoContract]
	public class Camera
	{
		[ProtoMember(1)]
		public string cid { get; set; }
		[ProtoMember(2)]
		public int index { get; set; }
		[ProtoMember(3)]
		public bool enable { get; set; }
		[ProtoMember(4)]
		public string nickname { get; set; }
		[ProtoMember(5)]
		public string ip { get; set; }
	}
}
