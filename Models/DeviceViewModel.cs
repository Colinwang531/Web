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
        public bool Enable { get; set; }
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
        public List<CameraViewModel> cameraViews{get;set;}
    }
    public class CameraViewModel {
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
    }
}
