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
                var model = JsonConvert.DeserializeObject<SearchAttendanceViewModel>(searchModel);
                int total = 0;
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
        private List<AttendanceViewModel> GetData(SearchAttendanceViewModel model, int pageIndex, int pageSize,out int total) 
        {
            List<AttendanceViewModel> list = new List<AttendanceViewModel>();
            if (model==null)
            {
                model = new SearchAttendanceViewModel();
            }
            total = 0;
            var Attdata = from a in _context.Attendance
                       join b in _context.Crew on a.CrewId equals b.Id
                       where (model.Name != "" ? b.Name.Contains(model.Name) : 1 == 1)
                       select new 
                       { 
                        a.Time,
                        b.Name,
                        a.Behavior,
                        a.Id
                       };
            if (!(string.IsNullOrEmpty(model.StartTime)) && !(string.IsNullOrEmpty(model.EndTime)))
            {
                DateTime dtStart = DateTime.Parse(model.StartTime);
                DateTime dtEnd = DateTime.Parse(model.EndTime);
                Attdata = Attdata.Where(c => c.Time >= dtStart && c.Time <= dtEnd);
            }
            else if (!(string.IsNullOrEmpty(model.StartTime)))
            {
                DateTime dtStart = DateTime.Parse(model.StartTime);
                Attdata = Attdata.Where(c => c.Time >= dtStart);
            }
            else if (!(string.IsNullOrEmpty(model.EndTime)))
            {
                DateTime dtEnd = DateTime.Parse(model.EndTime);
                Attdata = Attdata.Where(c => c.Time <= dtEnd);
            }
            var att = Attdata.ToList();
            var ids = string.Join(',', att.Select(c => c.Id));
            var pic = _context.AttendancePicture.Where(c => ids.Contains(c.AttendanceId)).ToList();
            var data = from a in att
                       join b in pic on a.Id equals b.AttendanceId
                       select new
                       {
                           a.Id,
                           a.Behavior,
                           a.Name,
                           a.Time,
                           b.Picture
                       };
            total = data.Count();
            var dataPage = data.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            foreach (var item in dataPage)
            {
                AttendanceViewModel avm = new AttendanceViewModel()
                {
                    Behavior = item.Behavior == 0 ? "入" : "出",
                    Name = item.Name,
                    Picture = Convert.FromBase64String(Encoding.UTF8.GetString(item.Picture)),
                    Time = item.Time.ToString("yyyy-MM-dd HH:mm:ss")
                };
                list.Add(avm);
            }
            return list;
        }
    }
}