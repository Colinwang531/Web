using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class ComponentController :BaseController
    {
        MyContext _context;
        public ComponentController(MyContext context) {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Load()
        {
            List<ComponentViewModel> list = new List<ComponentViewModel>(); ProtoBuffer.ProtoManager manager = new ProtoBuffer.ProtoManager();
            string identity = "";
            if (base.user.IsLandHome)
            {
                string shipId = base.user.ShipId;
                var shipIdentity = _context.Component.FirstOrDefault(c => c.ShipId == shipId && c.Type == Models.Component.ComponentType.WEB);
                identity = shipIdentity.Id;
            }
            if (ManagerHelp.IsTest)
            {
                list.Add(new ComponentViewModel()
                {
                     Id=Guid.NewGuid().ToString(),
                     name="大华",
                     type=4
                });
                list.Add(new ComponentViewModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    name = "海康",
                    type = 3
                }); 
                list.Add(new ComponentViewModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    name = "报警",
                    type = 5
                });
                list.Add(new ComponentViewModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    name = "算法",
                    type = 1
                });
            }
            else
            {
                var response = manager.ComponentQuery(identity);
                if (response.result == 0 && response.componentinfos.Count > 0)
                {
                    foreach (var item in response.componentinfos)
                    {
                        ComponentViewModel model = new ComponentViewModel()
                        {
                            Id = item.cid,
                            name = item.cname,
                            type = (int)item.type
                        };
                        list.Add(model);
                    }
                }
            }
           
            var data = new
            {
                code = 0,
                data = list
            };
            return new JsonResult(data);
        }
    }
}