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
            ViewBag.loginip = AppSettingHelper.GetSectionValue("Video:loginip");
            ViewBag.port = AppSettingHelper.GetSectionValue("Video:port");
            ViewBag.username = AppSettingHelper.GetSectionValue("Video:username");
            ViewBag.password = AppSettingHelper.GetSectionValue("Video:password");
            ViewBag.autoPlay = AppSettingHelper.GetSectionValue("Video:autoPlay");            
            return View();
        }


    }
}
