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

        public IActionResult Index(string id = "")
        {
            ViewBag.shipId = id;
            return View();
        }
        public IActionResult LandHome()
        {
            byte[] by = HttpContext.Session.Get("uid");
            string uid = Encoding.UTF8.GetString(by);
            ViewBag.isAdmin = uid.ToLower() == "admin" ? true : false;
            var user = _context.User.FirstOrDefault(c => c.Id == uid);
            ViewBag.IsSetShip = user != null ? user.EnableConfigure : false;
            ViewBag.IsShow = user != null ? user.Enablequery : false;
            ManagerHelp.ShipId = "";
            ManagerHelp.IsShowLandHome = true;
            ManagerHelp.IsShowAlarm = false;

            return View();
        }
    }
}
