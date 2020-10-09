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
        /// 自增长ID
        /// </summary>
       public int Id { get; set; }
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

       
    }
}
