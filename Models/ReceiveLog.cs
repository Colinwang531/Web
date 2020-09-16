using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    /// <summary>
    /// 接收protobuf消息日志
    /// </summary>
    public class ReceiveLog
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 拉收类型值
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 收到的数据
        /// </summary>
        public string Values { get; set; }
        /// <summary>
        /// 接收时间
        /// </summary>
        public DateTime Time { get; set; }
    }
}
