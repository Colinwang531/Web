using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    public class Users
    {
        /// <summary>
        /// id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 用户UUID标识
        /// </summary>
        public string Uid { get; set; }
        [Required]
        /// <summary>
        /// 用户名称
        /// </summary>
        public string Name { get; set; }
        [Required]
        /// <summary>
        /// 用户密码
        /// </summary>
        public string Password { get ; set; }
        /// <summary>
        /// 配置权限许可标识
        /// </summary>
        public bool EnableConfigure { get; set ; }
        /// <summary>
        /// 查询权限许可标识
        /// </summary>
        public bool Enablequery { get; set; }

    }
}
