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

        public IActionResult Index(string id="")
        {
            if (string.IsNullOrEmpty(id)&&ManagerHelp.ShipId=="")
            {
                ProtoManager manager = new ProtoManager();
                string identity = Guid.NewGuid().ToString();
                //查询组件
                //var rep = manager.ComponentQuery(identity);

                //测试赋值
                var rep = new ProtoBuffer.Models.ComponentResponse()
                {
                    componentinfos = new List<ProtoBuffer.Models.ComponentInfo>() {
                    new ProtoBuffer.Models.ComponentInfo(){
                     type= ProtoBuffer.Models.ComponentInfo.Type.WEB,
                     cid="001",
                     cname="测试"
                    }
                  }
                };
                string cid = rep.cid;
                var data = rep.componentinfos;
                //如果查询的组件中有web标识，那么就是陆地端
                var type = data.FirstOrDefault(c => c.type == ProtoBuffer.Models.ComponentInfo.Type.WEB);
                if (type != null)
                {
                    //陆地端是没有修改权限。只能查看
                    ManagerHelp.IsSet = false;
                    return RedirectToAction(nameof(LandHome));
                }
            }

            ViewBag.shipId = id;
            return View();
        }
        public IActionResult LandHome()
        {
            ManagerHelp.ShipId = "";
            return View();
        }
        /// <summary>
        /// 加载陆地端信息
        /// </summary>
        /// <returns></returns>
        public IActionResult LandLoad()
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
                return new JsonResult(new { code = 1, msg = "获取数据失败!"+ex.Message });
            }
        }
        /// <summary>
        /// 加载单个船的主页面
        /// </summary>
        /// <param name="shipId">如果存在则是客户端查看的单个船</param>
        /// <returns></returns>
        public IActionResult Load(string shipId)
        {
            try
            {
                //是否显示返回陆地端主页面
                bool flag = true;
                if (string.IsNullOrEmpty(shipId))
                {
                    flag = false;
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
                }
                ManagerHelp.ShipId = shipId;

                var result = new
                {
                    code = 0,
                    isReturn= flag
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
