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
using ShipWeb.DB;
using ShipWeb.Helpers;
using ShipWeb.Models;
using ShipWeb.ProtoBuffer;
using ShipWeb.ProtoBuffer.Models;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
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
