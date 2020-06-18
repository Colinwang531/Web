using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CodeStyle;
using ShipWeb.DB;

namespace ShipWeb.Controllers
{
    public class AttendanceController : Controller
    {
        private MyContext _context;
        public AttendanceController(MyContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 查询考勤
        /// </summary>
        /// <param name="type">1：考勤入 2：考勤出</param>
        /// <returns></returns>
        public IActionResult Load(int type=1)
        {
            try
            {
                var data = from a in _context.Alarm
                           join b in _context.AlarmInformation on a.Id equals b.AlarmId
                           join c in _context.CameraConfig on b.Cid equals c.Cid
                           join d in _context.Employee on b.Uid equals d.Uid
                           where (type == 1 ? c.EnableAttendanceIn == true : c.EnableAttendanceOut == true)
                           select new
                           {
                               d.Uid,
                               d.Name,
                               a.Time,
                               c.EnableAttendanceIn,
                               c.EnableAttendanceOut,
                               Picture = Convert.ToBase64String(Convert.FromBase64String(Encoding.UTF8.GetString(a.Picture))),
                           };
                var list = data.ToList();
                return new JsonResult(new { code = 0, data = list });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "获取数据失败!"+ex.Message });
            }

        }
    }
}