using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.Models
{
    public enum ComponentType
    {
		/// <summary>
		/// 消息分发组件
		/// </summary>
		XMQ = 1,
		/// <summary>
		/// WEB组件
		/// </summary>
		WEB = 2,
		/// <summary>
		/// 海康组件
		/// </summary>
		HKD = 3,
		/// <summary>
		/// 大华组件
		/// </summary>
		DHD = 4,
		/// <summary>
		/// 报警组件
		/// </summary>
		ALM = 5,
		/// <summary>
		/// 算法组件
		/// </summary>
		AI = 6,
		/// <summary>
		/// 流媒体组件
		/// </summary>
		MED = 7
	}
}
