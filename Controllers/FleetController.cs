using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ShipWeb.DB;
using ShipWeb.Models;

namespace ShipWeb.Controllers
{
    public class FleetController : Controller
    {
        MyContext _context;
        public FleetController(MyContext context) {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Load() 
        {
            var fleet = _context.Fleet.ToList();
            var ship=_context.Ship.ToList();
            var user = _context.User.ToList();
            var data = from a in fleet
                       join b in user on a.UserId equals b.Id
                       select new
                       {
                           a.Id,
                           a.Name,
                           a.UserId,
                           userName = b.Name,
                           a.Phone,
                           a.ShipIds,
                           shipName = (string.Join(',', (_context.Ship.Where(c => a.ShipIds.Contains(c.Id)).Select(d => d.Name))))
                       };

            var result = new
            {
                code = 0,
                data = data,
                ship = ship,
                user=user
            };
            return new JsonResult(result);
        }
        //public IActionResult GetShip(bool add = false) 
        //{
        //    List<Ship> list = _context.Ship.ToList();
        //    if (add)
        //    {
        //        list = list.Where(c => !(_context.Fleet.Select(c => c.ShipIds).Contains(c.Id))).ToList();
        //    }
        //    var data = from a in list
        //               select new
        //               {
        //                   name = a.Name,
        //                   value = a.Id
        //               };
        //    var result = new
        //    {
        //        code = 0,
        //        data = data
        //    };
        //    return new JsonResult(result);
        //}
        /// <summary>
        /// 保存船队信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Save(string model)
        {
            try
            {
                var fleetVM = JsonConvert.DeserializeObject<Fleet>(model);
                if (!string.IsNullOrEmpty(fleetVM.Id))
                {
                    var fleet = _context.Fleet.FirstOrDefault(c => c.Id == fleetVM.Id);
                    if (fleet == null)
                    {
                        return new JsonResult(new { code = 1, msg = "数据不存在" });
                    }
                    if (_context.Fleet.Where(c => c.Name == fleetVM.Name.Trim()).Count() > 1)
                    {
                        return new JsonResult(new { code = 1, msg = "船队名称不能重复" });
                    }
                    fleet.Name = fleetVM.Name.Trim();
                    fleet.UserId = fleetVM.UserId;
                    fleet.Phone = fleetVM.Phone;
                    fleet.ShipIds = fleetVM.ShipIds;
                    _context.Fleet.Update(fleet);
                }
                else
                {
                    if (_context.Fleet.Where(c => c.Name == fleetVM.Name.Trim()).Count() > 0)
                    {
                        return new JsonResult(new { code = 1, msg = "船队名称不能重复" });
                    }
                    fleetVM.Id = Guid.NewGuid().ToString();
                    _context.Fleet.Add(fleetVM);
                }
                _context.SaveChanges();
                return new JsonResult(new { code = 0 });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1,msg="数据保存异常" });
            }
        }
        /// <summary>
        /// 删除船队
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Delete(string id) 
        {
            if (!string.IsNullOrEmpty(id))
            {
                var fleet = _context.Fleet.FirstOrDefault(c => c.Id == id);
                if (fleet == null)
                {
                    return new JsonResult(new { code = 1, msg = "数据不存在" });
                }
                _context.Fleet.Remove(fleet);
                _context.SaveChanges();
            }
            return new JsonResult(new { code = 0 }); 
        }
    }
}