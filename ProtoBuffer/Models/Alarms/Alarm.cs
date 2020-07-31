
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer.Models
{
    /// <summary>
    /// 报警类
    /// </summary>
    [ProtoContract]
    public class Alarm
    {
        public enum Command
        {
            NOTIFY = 1
        }
        [ProtoMember(1)]
        public Command command = Command.NOTIFY;
        [ProtoMember(2)]
        public AlarmInfo alarminfo { get; set; }
    }
}
