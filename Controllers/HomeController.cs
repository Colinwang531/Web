using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using ShipWeb.DB;
using ShipWeb.Models;

namespace ShipWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {

            _logger = logger;
        }

        public IActionResult Index()
        {
           
            return View();
        }

       
       
        [HttpPost]
        public JsonResult Show([FromBody]TestModel data)
        {
            JsonResult json = new JsonResult(data.Name);
            return json;
        }
    }
}
