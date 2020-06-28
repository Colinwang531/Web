using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProtoBuf;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.ProtoBuffer;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class ShipController : BaseController
    {
        private readonly MyContext _context;
        public ShipController(MyContext context)
        {
            _context = context;
        }
        public IActionResult Edit()
        {
           var ship= _context.Ship.FirstOrDefault(c=>c.Id==ManagerHelp.ShipId);
            if (ship!=null)
            {
                ViewBag.Id = ship.Id;
                ViewBag.Name = ship.Name;
                ViewBag.Type = ship.Type;
                ViewBag.Flag = ship.Flag;
            }
            else
            {
                ViewBag.Id = "";
                ViewBag.Name = "";
                ViewBag.Type = 1;
                ViewBag.Flag = false;
            }
            ViewBag.isSet = ManagerHelp.IsSet;
           return View();
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Load()
        {
            try
            {
                var data = _context.Ship.ToList();
                var result = new
                {
                    code = 0,
                    data = data
                };
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "获取数据失败!" + ex.Message });
            }
        }
        /// <summary>
        /// 保存船状态
        /// </summary>
        /// <param name="ship"></param>
        /// <returns></returns>
        public IActionResult Save(string id,string name,int type,string status)
        {
            try
            {
                if (!ManagerHelp.IsSet)
                {
                    new JsonResult(new { code =1, msg = "您没有权限修改数据!" });
                }
                if (ModelState.IsValid)
                {
                    Ship ship = new Ship()
                    {
                        Flag = status == "1" ? true : false,
                        Id = id,
                        Name = name,
                        Type = type
                    };
                    ProtoManager manager = new ProtoManager();
                    ShipWeb.ProtoBuffer.Models.StatusRequest sr = new ShipWeb.ProtoBuffer.Models.StatusRequest()
                    {
                        flag = ship.Flag,
                        name = ship.Name,
                        type = (ShipWeb.ProtoBuffer.Models.StatusRequest.Type)ship.Type
                    };

                    //int result = manager.StatesSet(sr, ship.Id);
                    //if (result == 0)
                    //{
                    if (!string.IsNullOrEmpty(id))
                    {
                        _context.Ship.Update(ship);
                    }
                    else
                    {
                        ship.Id = Guid.NewGuid().ToString();
                        ManagerHelp.ShipId = ship.Id;
                        _context.Ship.Add(ship);
                    }
                        _context.SaveChanges();
                    //}
                }
                return new JsonResult(new { code = 0 });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 0 ,msg="数据保存失败"+ex.Message}) ;
            }
        }
    }
}