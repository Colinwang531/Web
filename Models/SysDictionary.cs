using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.Models
{
    /// <summary>
    /// 系统配置文件
    /// </summary>
    public class SysDictionary
    {
        public int Id { get; set; }
        public string key { get; set; }
        public string value { get; set; }
    }
}
