﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.ProtoBuffer;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class HomeController : BaseController
    {
        private readonly MyContext _context;
        public HomeController(MyContext context)
        {
            _context = context;
        }

        public IActionResult LandHome()
        {
            ViewBag.IsSetShip = base.user.EnableConfigure;
            ViewBag.IsShow = base.user.Enablequery;
            ViewBag.isAdmin = base.user.Id == "admin" ? true : false;
            ViewBag.LoginName = base.user.Name;
            string browsertoken = HttpContext.Request.Cookies["token"];
            if (browsertoken != null)
            {
                string urlstr = HttpContext.Session.GetString(browsertoken);
                user = JsonConvert.DeserializeObject<UserToken>(urlstr);
                user.ShipId = "";
                user.IsLandHome = true;
                string userStr = JsonConvert.SerializeObject(user);
                //将请求的url注册
                HttpContext.Session.SetString(browsertoken, userStr);
                //写入浏览器token
                HttpContext.Response.Cookies.Append("token", browsertoken);
            }
            ManagerHelp.IsShowAlarm = false;

            return View();
        }

        public IActionResult LandDataCenter()
        {
            return View();
        }


        #region 数字大屏业务
        /// <summary>
        /// 报警类型分析
        /// </summary>
        /// <returns></returns>
        public JsonResult GetAlarmType(string month)
        {
            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now;
            DateTime now = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            switch (month)
            {
                case "upMonth":
                    startTime = now.AddMonths(-1).AddDays(1 - now.Day);//上月月初
                    endTime = startTime.AddMonths(1).AddDays(-1);//上月月末
                    break;
                case "currentMonth":
                    startTime = now.AddDays(1 - now.Day);//本月月初
                    endTime = startTime.AddMonths(1).AddDays(-1);//本月月末
                    break;
                default:
                    startTime = now.AddYears(-99);
                    endTime = now.AddYears(99);
                    break;
            }

            var result = _context.Alarm.Where(x => startTime < x.Time && x.Time < endTime).GroupBy(x => x.Type).Select(s => (new { Type = s.Key, Num = s.Count() })).OrderBy(x => x.Type);
            return Json(result);
        }

        /// <summary>
        /// 数据统计
        /// </summary>
        /// <returns></returns>
        public JsonResult GetDataStatis()
        {
            int enableDeviceCount = _context.Device.Where(s => s.Enable.Equals(true)).Count();
            int stopDeviceCount = _context.Device.Where(s => s.Enable.Equals(false)).Count();

            int enableCameraCount = _context.Camera.Where(s => s.Enable.Equals(true)).Count();
            int stopCameraCount = _context.Camera.Where(s => s.Enable.Equals(false)).Count();

            return Json(new { enableDeviceCount, stopDeviceCount, enableCameraCount, stopCameraCount });
        }


        /// <summary>
        /// 基本信息
        /// </summary>
        /// <returns></returns>
        public JsonResult GetCountInfo()
        {
            var sql = $"select count(1) Id,count(case when(s.Flag = TRUE) then 1 else null end) sailCount,count(case when(s.Flag = FALSE) then 1 else null end) portCount," +
                $"(SELECT COUNT(*)  FROM Crew c LEFT JOIN Ship ss on c.ShipId = ss.Id) crewCount from Ship s";
            var result = _context.ShipCount.FromSqlRaw(sql);
            return Json(result);
        }

        /// <summary>
        /// 考勤状态(以船员为主表)
        /// </summary>
        /// <returns></returns>
        public JsonResult GetAttendance()
        {
            //无月考勤率
            //var result = _context.Crew.FromSqlRaw("SELECT a.*,a.Name as CrewName,b.Name as ShipName,c.Time,c.Behavior from Crew a LEFT JOIN Ship b on a.ShipId=b.Id LEFT JOIN Attendance c on a.Id=c.CrewId ORDER BY c.Time DESC LIMIT 100");

            //增加月考勤率
            DateTime now = DateTime.Now;
            DateTime startTime = now.AddDays(1 - now.Day);//本月月初
            DateTime endTime = startTime.AddMonths(1).AddDays(-1);//本月月末
            string sql = $"SELECT DISTINCT(a.id),a.Job,a.Name,a.ShipId,a.Name as CrewName,b.Name as ShipName,c.Time,c.Behavior,  TRUNCATE((select ((select count(*) from(select aa.Time, aa.CrewId from Attendance as aa where aa.CrewId = a.Id and aa.Time >= DATE('{startTime:yyyy-MM-dd 00:00:00}') and aa.Time <= DATE('{endTime:yyyy-MM-dd 23:59:59}'))tt) / 22 * 100.00) as chuqin),2) as Rate from Crew a LEFT JOIN Ship b on a.ShipId = b.Id LEFT JOIN Attendance c on a.Id = c.CrewId ORDER BY c.Time DESC LIMIT 100";
            var result = _context.Crew.FromSqlRaw(sql);
            return Json(result);
        }
        #endregion


    }
}
