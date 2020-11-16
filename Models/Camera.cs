using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWeb.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Camera
    {
        public Camera() {
            this.ShipId = "";
        }
        /// <summary>
        /// 摄像机表
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 摄像机序号
        ///  </summary>
        public int Index { get; set; }
        /// <summary>
        /// 是否启用设备，true表示启动设备，false表示停用设备
        /// </summary>
        public bool Enable { get; set; }
        /// <summary>
        /// 摄像机别名
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 摄像机ip地址
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// 设备表主建id
        /// </summary>
        public string DeviceId { get; set; }
        /// <summary>
        /// 船表主键id
        /// </summary>
        public string ShipId { get; set; }
        //public CameraConfig cameraConfig { get; set; }
    }
}
