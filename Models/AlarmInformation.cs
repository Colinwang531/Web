﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    /// <summary>
    /// 报警消息
    /// </summary>
    public class AlarmInformation
    {
      public string Id { get; set; }
        /// <summary>
        /// 报警类型1：SLEEP 睡觉，2：FIGHT 打架， 3：HELMET 安全帽，4：PHONE 手机，5： FACE人脸
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 摄像机ID
        /// </summary>
        public string Cid { get; set; }
        /// <summary>
        /// 员工ID，当报警类型为人脸时才有值
        /// </summary>
        public string Uid { get; set; }
        /// <summary>
        /// 报警表主键id
        /// </summary>
        public string AlarmId { get; set; }
        /// <summary>
        /// 船表主键id
        /// </summary>
        public string Shipid { get; set; }
        public List<AlarmInformationPosition> alarmInformationPositions { get; set; }
    }
}
