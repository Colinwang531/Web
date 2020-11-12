using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer.Models
{
    public class MeterAsyncQueue
    {
        public string Id { get; set; }
        public List<AlarmCache> alarmCaches { get; set; }
    }
}
