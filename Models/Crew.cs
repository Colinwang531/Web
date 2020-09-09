using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    /// <summary>
    /// 船员表
    /// </summary>
    public class Crew
    {
        /// <summary>
        /// 
        /// </summary>
      public string Id { get; set; }
        /// <summary>
        /// 船员名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 工作内容
        /// </summary>
        public string Job { get; set; }
        /// <summary>
        /// 船表主键id
        /// </summary>
        public string ShipId { get; set; }
        /// <summary>
        /// 船员图片
        /// </summary>
        public List<CrewPicture> employeePictures { get; set; }

        #region 扩展属性
        /// <summary>
        /// 船名
        /// </summary>
        public string ShipName { get; set; }
        /// <summary>
        /// 船员名
        /// </summary>
        public string CrewName { get; set; }
        /// <summary>
        /// 考勤时间
        /// </summary>
        public DateTime? Time { get; set; }
        /// <summary>
        /// 行为
        /// </summary>
        public int? Behavior { get; set; }
        /// <summary>
        /// 月考勤率
        /// </summary>
        public double? Rate { get; set; }
        #endregion
    }
}
