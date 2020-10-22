using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.Tool
{
    public static class DateTimeExtentions
    {
        public static DateTime ToCstTime(this DateTime time)
        {
            Instant now = SystemClock.Instance.GetCurrentInstant();
            var shanghaiZone = DateTimeZoneProviders.Tzdb["Asia/Shanghai"];
            DateTime dttime= now.InZone(shanghaiZone).ToDateTimeUnspecified();
            return dttime;
        }
    }
}
