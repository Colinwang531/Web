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



    }
}
