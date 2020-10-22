using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.Models
{
    public class SearchAttendanceViewModel
    {
        public SearchAttendanceViewModel() {
            Name = "";
            StartTime = "";
            EndTime = "";
        }
        public string Name { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
