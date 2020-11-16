using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.Models
{
    /// <summary>
    /// 设备
    /// </summary>
    public class Device
    {
        public Device() {
            this.ShipId = "";
        }
        public enum Factory
        {
            [Display(Name = "海康")]
            /// <summary>
            /// 海康
            /// </summary>
            HIKVISION = 1,
            [Display(Name = "大华")]
            /// <summary>
            /// 大华
            /// </summary>
            DAHUA = 2,
            [Display(Name = "伊顿")]
            /// <summary>
            /// 伊顿
            /// </summary>
            EATON = 3
        }
        public enum Type
        {
            [Display(Name = "DVR")]
            DVR = 1,
            [Display(Name = "NVR")]
            NVR = 2,
            [Display(Name = "IPC")]
            IPC = 3
        }
        /// <summary>
        /// 设备是否开启
        /// </summary>
        public bool Enable { get; set; }
        public string Id { get; set; }
        /// <summary>
        /// 设备厂商 1：HIKVISION 海康，2：DAHUA 大华，3：EATON 伊顿
        /// </summary>
        public Factory factory { get; set; }
        /// <summary>
        /// 设备类型1：DVR，2：NVR，3：IPC
        /// </summary>
        public Type type { get; set; }
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
        public List<Camera> CameraModelList { get; set; }

    }
}
