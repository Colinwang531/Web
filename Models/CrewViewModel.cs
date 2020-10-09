using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    public class CrewViewModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 船员名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 工作内容
        /// </summary>
        public string Job { get; set; }
        public List<CrewPictureViewModel> crewPictureViewModels { get; set; }
    }
    public class CrewPictureViewModel
    { 
        public string Id { get; set; }
        public string Picture { get; set; }
    }
}
