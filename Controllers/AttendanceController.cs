using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CodeStyle;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Newtonsoft.Json;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class AttendanceController : BaseController
    {
        private MyContext _context;
        public AttendanceController(MyContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            ViewBag.IsLandHome = base.user.IsLandHome;
            return View();
        }

        /// <summary>
        /// 查询考勤
        /// </summary>
        /// <param name="type">1：考勤入 2：考勤出</param>
        /// <returns></returns>
        public IActionResult Load(int pageIndex,int pageSize)
        {
            try
            {
                int total = 0;
                DateTime dt = DateTime.UtcNow;
                var list = GetData(dt, pageIndex, pageSize, out total);
                var result = new
                {
                    code = 0,
                    data = list,
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    count = total
                };
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "获取数据失败!"+ex.Message });
            }

        }
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="uid">编号</param>
        /// <param name="name">名称</param>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每次显示条数</param>
        /// <returns></returns>
        public IActionResult QueryPage(string dtTime, int pageIndex, int pageSize)
        {
            try
            {
                int total = 0;
                DateTime dt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(dtTime))
                {
                    dt = DateTime.Parse(dtTime);
                }
                var list = GetData(dt, pageIndex, pageSize, out total);
                var result = new
                {
                    code = 0,
                    data = list,
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    count = total
                };
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "查询失败!"+ex.Message });
            }
        }
        private List<AttendanceViewModel> GetData(DateTime dt , int pageIndex, int pageSize,out int total) 
        {
            string shipId = base.user.ShipId;           
            total = _context.Crew.Count();
            var crews = _context.Crew.ToList();
            var startTime =DateTime.Parse(dt.ToString("yyyy-MM-dd 00:00:00"));
            var endTime = DateTime.Parse(dt.ToString("yyyy-MM-dd 23:59:59")); ;
            var attdata = _context.Attendance.Where(c => c.Time >= startTime && c.Time <= endTime && c.ShipId == shipId).ToList();
            var ids = string.Join(',', attdata.Select(c => c.Id));
            var pices = _context.AttendancePicture.Where(c => ids.Contains(c.AttendanceId)).ToList();
            List<AttendanceViewModel> list = new List<AttendanceViewModel>(); 
            foreach (var item in crews)
            {
                AttendanceViewModel model = new AttendanceViewModel()
                {
                    Name = item.Name,
                    attendances = new List<AttendanceView>()
                };                ;
                if (attdata.Where(c => c.CrewId == item.Id).Any())
                {
                    var attes = attdata.Where(c => c.CrewId == item.Id);
                    foreach (var attd in attes)
                    {
                        AttendanceView ad = new AttendanceView()
                        {
                            Behavior = attd.Behavior == 0 ? "入" : "出",
                            Time = attd.Time.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        if (pices.Where(c => c.AttendanceId == attd.Id).Any())
                        {
                            var picture = pices.Where(c => c.AttendanceId == attd.Id).FirstOrDefault().Picture;
                            ad.Picture = Convert.FromBase64String(Encoding.UTF8.GetString(picture));
                        }
                        model.attendances.Add(ad);
                    }

                }
                list.Add(model);
            }

            //根据打卡排列（有打卡的放在前面）           
            var data=(from a in list
                     orderby a.attendances.Count descending
                     select new
                     { 
                        a.Name,
                        a.attendances
                     }).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            List<AttendanceViewModel> listpage = new List<AttendanceViewModel>();
            foreach (var item in data)
            {
                listpage.Add(new AttendanceViewModel()
                {
                    Name = item.Name,
                    attendances = item.attendances
                });

            }
            return listpage;
        }
    }
}