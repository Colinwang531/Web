using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.Models
{
    /// <summary>
    /// 缓存用户信息
    /// </summary>
    public class UserToken
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 可配置权限
        /// </summary>
        public bool EnableConfigure { get; set; }
        /// <summary>
        /// 可查询权限
        /// </summary>
        public bool Enablequery { get; set; }
        /// <summary>
        /// 船舶端登陆时此值为舶ID,陆地端登陆时此值为单个船的通讯ID
        /// </summary>
        public string ShipId { get; set; }
        /// <summary>
        /// 船名
        /// </summary>
        public string ShipName { get; set; }
        /// <summary>
        /// true:陆地端 false:船舶端
        /// </summary>
        public bool IsLandHome { get; set; }
    }
}
