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
using SmartWeb.DB;
using SmartWeb.Models;
using SmartWeb.Tool;

namespace SmartWeb.Controllers
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
                var list = GetData(null, pageIndex, pageSize, out total);
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
        public IActionResult QueryPage(string searchModel, int pageIndex, int pageSize)
        {
            try
            {
                int total = 0;
                SearchAttendance model = new SearchAttendance();
                if (searchModel!="")
                {
                    model = JsonConvert.DeserializeObject<SearchAttendance>(searchModel);
                }
                var list = GetData(model, pageIndex, pageSize, out total);
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
        private List<AttendanceViewModel> GetData(SearchAttendance search, int pageIndex, int pageSize,out int total) 
        {
            if (search==null)
            {
                search = new SearchAttendance();
            }
            var arrWhere = _context.Attendance.Where(c=>1==1);
            if (!(string.IsNullOrEmpty(search.StartTime)) && !(string.IsNullOrEmpty(search.EndTime)))
            {
                DateTime dtStart = DateTime.Parse(search.StartTime);
                DateTime dtEnd = DateTime.Parse(search.EndTime);
                arrWhere = arrWhere.Where(c => c.Time >= dtStart && c.Time <= dtEnd);
            }
            else if (!(string.IsNullOrEmpty(search.StartTime)))
            {
                DateTime dtStart = DateTime.Parse(search.StartTime);
                arrWhere = arrWhere.Where(c => c.Time >= dtStart);
            }
            else if (!(string.IsNullOrEmpty(search.EndTime)))
            {
                DateTime dtEnd = DateTime.Parse(search.EndTime);
                arrWhere = arrWhere.Where(c => c.Time <= dtEnd);
            }
            if (!string.IsNullOrEmpty(search.Behavior))
            {
                int dehav = Convert.ToInt32(search.Behavior);
                arrWhere = arrWhere.Where(c => c.Behavior == dehav);
            }
            var attdata = arrWhere.ToList();
            var crewWhere = _context.Crew.Where(c => attdata.Select(c => c.CrewId).Contains(c.Id));           
            if (!string.IsNullOrEmpty(search.Name))
            {
                crewWhere = crewWhere.Where(c => c.Name.Contains(search.Name));
            }
            if (!string.IsNullOrEmpty(search.Job))
            {
                crewWhere = crewWhere.Where(c => c.Job.Contains(search.Job));
            }
            var crewdata = crewWhere.ToList();
            total = crewdata.Count;
            var pices = _context.AttendancePicture.Where(c => attdata.Select(c => c.Id).Contains(c.AttendanceId)).ToList();
            List<AttendanceViewModel> list = new List<AttendanceViewModel>();          
            var pageData = (from a in attdata
                            join b in crewdata on a.CrewId equals b.Id
                            select new
                            { 
                                a.Id,
                                a.CrewId,
                                a.Behavior,
                                a.CreateTime,
                                a.Time,
                                b.Job,
                                b.Name
                            }).OrderByDescending(c=>c.CreateTime).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            foreach (var item in pageData)
            {
                string picture = "";
                if (pices.Where(c => c.AttendanceId == item.Id).Any()) {
                    picture = Convert.ToBase64String(pices.FirstOrDefault(c => c.AttendanceId == item.Id).Picture);
                }               
                AttendanceViewModel model = new AttendanceViewModel()
                {
                    Behavior = item.Behavior == 0 ? "入" : "出",
                    Time = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    Picture = picture,
                    Job = item.Job,
                    Name = item.Name
                };
                list.Add(model);
            }
            return list;
        }
    }
}