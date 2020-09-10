using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    
    public class ShipCount
    {
        /// <summary>
        /// 主键id(船舶总数量)
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// 航行数量
        /// </summary>
        public int? sailCount { get; set; }
        /// <summary>
        /// 停港数量
        /// </summary>
        public int? portCount { get; set; }
        /// <summary>
        /// 船员数量
        /// </summary>
        public int? crewCount { get; set; }
    }
}
