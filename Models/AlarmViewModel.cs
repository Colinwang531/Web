using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    public class AlarmViewModel
    {
        public string Name { get; set; }
        public DateTime Time { get; set; }
        public byte[] Picture { get; set; }
        public string Cid { get; set; }
        public int Type { get; set; }
        public string NickName { get; set; }
    }
}
