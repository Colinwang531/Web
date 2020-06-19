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
using ShipWeb.ProtoBuffer;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyContext _context;

        public HomeController(MyContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        //组件查询
        public IActionResult Load()
        {
            try
            {
                ProtoManager manager = new ProtoManager();
                string identity = Guid.NewGuid().ToString();
                var rep = manager.ComponentQuery(identity);
                //ManagerHelp.components = new Dictionary<int, string>();
                string cid = rep.cid;
                var data = rep.componentinfos;
                ManagerHelp.ShipId = "";
                //如果查询的组件中有web标识，那么就是陆地端
                var type=data.Where(c => c.type == ProtoBuffer.Models.ComponentInfo.Type.WEB);
                if (type.Count()==0)
                {
                    //陆地端是没有修改权限。只能查看
                    ManagerHelp.IsSet = false;
                }
                else
                {
                    ManagerHelp.IsSet = true;
                    string shipId = "";
                    //是否有般存在
                    var ship = _context.Ship.FirstOrDefault();
                    if (ship == null)
                    {
                        shipId = Guid.NewGuid().ToString();
                        Models.Ship sm = new Models.Ship()
                        {
                            Flag = false,
                            Id = shipId,
                            Name = "船1",
                            Type = 1
                        };
                        _context.Ship.Add(sm);
                        _context.SaveChanges();
                    }
                    else
                    {
                        shipId = ship.Id;
                    }
                    ManagerHelp.ShipId = shipId;
                }
                var result = new
                {
                    code = 0,
                    data = data,
                    cid = cid
                };
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "获取组件失败！" + ex.Message });
            }
        }
    }
}
