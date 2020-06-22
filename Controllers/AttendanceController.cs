using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CodeStyle;
using ShipWeb.DB;
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
                var data = from a in _context.Alarm
                           join b in _context.AlarmInformation on a.Id equals b.AlarmId
                           join c in _context.CameraConfig on b.Cid equals c.Cid
                           join d in _context.Employee on b.Uid equals d.Uid
                           join e in _context.AlarmInformationPosition on b.Id equals e.AlarmInformationId
                           where a.ShipId==b.Shipid&&b.Shipid==c.ShipId&&c.ShipId==d.ShipId&&d.ShipId==e.ShipId
                           where b.Type==5 && a.ShipId==ManagerHelp.ShipId
                           select new
                           {
                               d.Uid,
                               d.Name,
                               a.Time,
                               c.EnableAttendanceIn,
                               c.EnableAttendanceOut,
                               Picture =ManagerHelp.DrawAlarm(a.Picture,e.X,e.Y,e.W,e.H),
                           };
                var list = data.ToList();
                int count = list.Count;
                var pageList=list.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                var result = new
                {
                    code = 0,
                    data = pageList,
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    count = count
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
        public IActionResult QueryPage(string uid, string name, string startTime,string endTime, int pageIndex, int pageSize)
        {
            try
            {
                var data = from a in _context.Alarm
                           join b in _context.AlarmInformation on a.Id equals b.AlarmId
                           join c in _context.CameraConfig on b.Cid equals c.Cid
                           join d in _context.Employee on b.Uid equals d.Uid
                           join e in _context.AlarmInformationPosition on b.Id equals e.AlarmInformationId
                           where a.ShipId == b.Shipid && b.Shipid == c.ShipId && c.ShipId == d.ShipId && d.ShipId == e.ShipId
                           where b.Type == 5 && a.ShipId == ManagerHelp.ShipId
                           select new
                           {
                               d.Uid,
                               d.Name,
                               a.Time,
                               c.EnableAttendanceIn,
                               c.EnableAttendanceOut,
                               Picture = ManagerHelp.DrawAlarm(a.Picture, e.X, e.Y, e.W, e.H),
                           };
                if (!string.IsNullOrEmpty(uid))
                {
                    data=data.Where(c => c.Uid == uid);
                }
                if (!string.IsNullOrEmpty(name))
                {
                    data=data.Where(c => c.Name.Contains(name));
                }
                if (!(string.IsNullOrEmpty(startTime))&&!(string.IsNullOrEmpty(endTime)))
                {
                    DateTime dtStart = DateTime.Parse(startTime);
                    DateTime dtEnd = DateTime.Parse(endTime);
                    data = data.Where(c => c.Time >= dtStart && c.Time <= dtEnd);
                }
                else if (!(string.IsNullOrEmpty(startTime)))
                {
                    DateTime dtStart = DateTime.Parse(startTime);
                    data = data.Where(c => c.Time >= dtStart );
                }
                else if (!(string.IsNullOrEmpty(endTime)))
                {
                    DateTime dtEnd = DateTime.Parse(endTime);
                    data = data.Where(c => c.Time <= dtEnd);
                }
                var list = data.ToList();
                int count = list.Count();
                var pageList = list.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                var result = new
                {
                    code = 0,
                    data = pageList,
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    count = count
                };
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "查询失败!"+ex.Message });
            }
        }
    }
}