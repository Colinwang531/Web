using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProtoBuf;
using SmartWeb.DB;
using SmartWeb.Helpers;
using SmartWeb.Models;
using SmartWeb.ProtoBuffer;
using SmartWeb.ProtoBuffer.Models;
using SmartWeb.Tool;

namespace SmartWeb.Controllers
{
    public class VideoController : BaseController
    {
        private readonly MyContext _context;

        public VideoController(MyContext context)
        {
            _context = context;
        }

        public ActionResult Index()
        {
            var temp = DapperContext.Query<Models.Device>($"SELECT * FROM Device").FirstOrDefault();
            if (temp != null)
            {
                ViewBag.loginip = temp.IP;
                ViewBag.port = temp.Port;
                ViewBag.username = temp.Name;
                ViewBag.password = temp.Password;
                ViewBag.autoPlay = "true";
            }
            return View();
        }


    }
}
