using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    /// <summary>
    /// 船员图片表
    /// </summary>
    public class EmployeePicture
    {
      public string Id { get; set; }
        /// <summary>
        /// 船员图片
        /// </summary>
        public byte[] Picture { get; set; }
        /// <summary>
        /// 船员表主键id
        /// </summary>s
        public string EmployeeId { get; set; }
        /// <summary>
        /// 船表主键id
        /// </summary>
        public string ShipId { get; set; }
    }
}
