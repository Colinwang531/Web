using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer.Models
{
    public class AlarmCache
    {
        public AlarmInfo alarmInfos { get; set; }
        /// <summary>
        /// 陆地端接收时此值有效
        /// </summary>
        public string ShipAlarmId { get; set; }
    }
}
