using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
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

        #endregion


    }
}
