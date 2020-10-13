using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    public class AlarmViewModel
    {
        public string Id { get; set; }
        /// <summary>
        /// 船名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 报警时间
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// 报警图片
        /// </summary>
        public string Picture { get; set; }        
        /// <summary>
        /// 报警类型
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 摄像机名称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 报警位置x
        /// </summary>
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
    }
}
