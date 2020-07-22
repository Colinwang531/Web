using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    public class DeviceViewModel
    {

        public string Id { get; set; }
        /// <summary>
        /// 设备厂商 1：HIKVISION 海康，2：DAHUA 大华，3：EATON 伊顿
        /// </summary>
        public int Factory { get; set; }
        /// <summary>
        /// 设备类型1：DVR，2：NVR，3：IPC
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 设备状态 0:开启 1：关闭
        /// </summary>
        public int Enable { get; set; }
        /// <summary>
        /// 设备登录名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 设备登录密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 设备IP地址
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// 设备端口号
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 船表主键id
        /// </summary>
        public string ShipId { get; set; }

        /// <summary>
        /// 设备别名
        /// </summary>
        public string Nickname { get; set; }
    }
}
