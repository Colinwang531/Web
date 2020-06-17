using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProtoBuf;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.ProtoBuffer;

namespace ShipWeb.Controllers
{
    public class ShipController : Controller
    {
        private readonly MyContext _context;
        public ShipController(MyContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
           var ship= _context.Ship.FirstOrDefault();
           return View(ship);
        }
        /// <summary>
        /// 保存船状态
        /// </summary>
        /// <param name="ship"></param>
        /// <returns></returns>
        public IActionResult Save(Ship ship)
        {
            if (ModelState.IsValid)
            {
                ProtoManager manager = new ProtoManager();
                ShipWeb.ProtoBuffer.Models.StatusRequest sr = new ShipWeb.ProtoBuffer.Models.StatusRequest()
                {
                    flag = ship.Flag,
                    name = ship.Name,
                    type = (ShipWeb.ProtoBuffer.Models.StatusRequest.Type)ship.Type
                };

                int result=manager.StatesSet(sr, ship.Id);
                if (result==0)
                {
                    _context.Ship.Update(ship);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}