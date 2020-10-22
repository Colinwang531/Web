using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.Models
{
    public class User
    {
        /// <summary>
        /// id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 用户密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 配置权限许可标识
        /// </summary>
        public bool EnableConfigure { get; set; }
        /// <summary>
        /// 查询权限许可标识
        /// </summary>
        public bool Enablequery { get; set; }
    }
}
