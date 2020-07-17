using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                //查询记录考虑的摄像机
                var info =( from a in _context.AlarmInformation
                         join b in _context.Algorithm on a.Cid equals b.Cid
                         where (a.Type == Models.AlarmType.ATTENDANCE_IN||a.Type==Models.AlarmType.ATTENDANCE_OUT)&&a.Shipid==ManagerHelp.ShipId
                         select new
                         {
                             a.AlarmId,
                             a.Uid,
                             b.Type
                         }).ToList();
                var alarmIds = string.Join(',', info.Select(c => c.AlarmId));
                var uids = string.Join(',', info.Select(c => c.Uid));
                //查询考勤图片
                var alarm = _context.Alarm.Where(c => alarmIds.Contains(c.Id)).ToList();
                //查询船员信息
                var employee = _context.Crew.Where(c => uids.Contains(c.Id)).ToList();
                //组合数据
                var data = from a in info
                               join b in alarm on a.AlarmId equals b.Id
                               join c in employee on a.Uid equals c.Id
                               select new
                               {
                                   Picture = Convert.FromBase64String(Encoding.UTF8.GetString(b.Picture)),
                                   Time = b.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                                   c.Id,
                                   c.Name,
                                   a.Type
                               };

                int count = data.Count();
                var pageList = data.Skip((pageIndex - 1) * pageSize).Take(pageSize);
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
                //查询记录考虑的摄像机
                var info = (from a in _context.AlarmInformation
                            join b in _context.Algorithm on a.Cid equals b.Cid
                            where (a.Type == Models.AlarmType.ATTENDANCE_IN || a.Type == Models.AlarmType.ATTENDANCE_OUT) && a.Shipid == ManagerHelp.ShipId
                            select new
                            {
                                a.AlarmId,
                                a.Uid,
                                b.Type
                            }).ToList();
                var alarmIds = string.Join(',', info.Select(c => c.AlarmId));
                var uids = string.Join(',', info.Select(c => c.Uid));
                //查询考勤图片
                var alarmWhere = _context.Alarm.Where(c => alarmIds.Contains(c.Id));
                if (!(string.IsNullOrEmpty(startTime)) && !(string.IsNullOrEmpty(endTime)))
                {
                    DateTime dtStart = DateTime.Parse(startTime);
                    DateTime dtEnd = DateTime.Parse(endTime);
                    alarmWhere = alarmWhere.Where(c => c.Time >= dtStart && c.Time <= dtEnd);
                }
                else if (!(string.IsNullOrEmpty(startTime)))
                {
                    DateTime dtStart = DateTime.Parse(startTime);
                    alarmWhere = alarmWhere.Where(c => c.Time >= dtStart);
                }
                else if (!(string.IsNullOrEmpty(endTime)))
                {
                    DateTime dtEnd = DateTime.Parse(endTime);
                    alarmWhere = alarmWhere.Where(c => c.Time <= dtEnd);
                }
                var alarm = alarmWhere.ToList();
                //查询船员信息
                var employee = _context.Crew.Where(c => uids.Contains(c.Id)&&
                                                    (!string.IsNullOrEmpty(uid) ? c.Id == uid:1==1)&&
                                                    (!string.IsNullOrEmpty(name)? c.Name.Contains(name):1==1)).ToList();
                //组合数据
                var data = from a in info
                           join b in alarm on a.AlarmId equals b.Id
                           join c in employee on a.Uid equals c.Id
                           select new
                           {
                               Picture = Convert.FromBase64String(Encoding.UTF8.GetString(b.Picture)),
                               Time = b.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                               c.Id,
                               c.Name,
                               a.Type
                           };

                int count = data.Count();
                var pageList = data.Skip((pageIndex - 1) * pageSize).Take(pageSize);
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