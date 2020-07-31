using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        private ProtoManager manager = new ProtoManager();
        public ShipController(MyContext context)
        {
            _context = context;
        }
        public IActionResult Edit(bool isShow = false)
        {
            string shipId = base.user.ShipId;
            var ship = _context.Ship.FirstOrDefault(c => c.Id == shipId);
            if (ship != null)
            {
                ViewBag.Id = ship.Id;
                ViewBag.Name = ship.Name;
                ViewBag.Type = ship.type;
                ViewBag.Flag = ship.Flag;
            }
            else
            {
                ViewBag.Id = "";
                ViewBag.Name = "";
                ViewBag.Type = 1;
                ViewBag.Flag = false;
            }
            ViewBag.isShow = isShow;
            ViewBag.IsSet = base.user.EnableConfigure;
            ViewBag.LoginName = base.user.Name;
            return View();
        }
        public IActionResult Index()
        {
            ViewBag.IsLandHome = base.user.IsLandHome;
            return View();
        }
        public IActionResult Load()
        {
            try
            {
                string shipId = base.user.ShipId;
                var ship = _context.Ship.FirstOrDefault(c => c.Id == shipId);
                var result = new
                {
                    code = 0,
                    data = ship,
                    isSet =base.user.EnableConfigure
                };
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "获取数据失败!" + ex.Message });
            }
        }
        /// <summary>
        /// 陆地端获取船信息
        /// </summary>
        /// <returns></returns>
        public IActionResult LoadAll()
        {
            try
            {
                var ship = _context.Ship.ToList();
                List<ShipViewModel> list = new List<ShipViewModel>();
                foreach (var item in ship)
                {
                    ShipViewModel model = new ShipViewModel()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Line=false//默认离线
                    };
                    #region 测试数据
                    ProtoBuffer.Models.ComponentResponse rep = new ProtoBuffer.Models.ComponentResponse()
                    {
                         result=0,
                         componentinfos=new List<ProtoBuffer.Models.ComponentInfo>() 
                         {
                             new ProtoBuffer.Models.ComponentInfo ()
                             {
                                type= ProtoBuffer.Models.ComponentInfo.Type.WEB
                             }
                         }
                    };
                    #endregion
                    //ProtoBuffer.Models.ComponentResponse rep = manager.ComponentQuery(item.Id);
                    if (rep.result == 0)
                    {
                        var info = rep.componentinfos.Where(c => c.type == ProtoBuffer.Models.ComponentInfo.Type.WEB);
                        if (info.Count() > 0)
                        {
                            model.Line = true;//在线
                            ProtoBuffer.Models.StatusResponse strep = manager.StatusQuery(item.Id);
                            if (strep.result == 0)
                            {
                                model.Name = strep.name;
                                model.flag = strep.flag;
                                //从船舶端过来的船名与陆地端不同时，更改陆地端的船名
                                if (item.Name != strep.name)
                                {
                                    item.Name = strep.name;
                                    _context.Ship.Update(item);
                                    _context.SaveChanges();
                                }
                            }
                        }
                    }
                    list.Add(model);
                }
                var result = new
                {
                    code = 0,
                    data = list,
                    isSet =base.user.EnableConfigure
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
        public IActionResult Save(string id, string name, int type)
        {
            try
            {
                if (!base.user.EnableConfigure)
                {
                    new JsonResult(new { code = 1, msg = "您没有权限修改数据!" });
                }
                if (base.user.IsLandHome)
                {
                    #region 陆地端登陆船舶端修改船状态
                    ShipWeb.ProtoBuffer.Models.StatusRequest sr = new ShipWeb.ProtoBuffer.Models.StatusRequest()
                    {
                        type = ShipWeb.ProtoBuffer.Models.StatusRequest.Type.SAIL,
                        flag = type
                    };
                    manager.StatussSet(sr,id);
                    sr = new ProtoBuffer.Models.StatusRequest()
                    {
                        type = ProtoBuffer.Models.StatusRequest.Type.NAME,
                        text = name
                    };
                    manager.StatussSet(sr,id);
                    return new JsonResult(new { code = 0 });
                    #endregion
                }
                #region 船舶端修改船状态
                if (!string.IsNullOrEmpty(id))
                {                   
                    var ship = _context.Ship.FirstOrDefault(c => c.Id == id);
                    if (ship != null)
                    {
                        ship.Name = name;
                        ship.type = (Ship.Type)type;
                        //航行类型为：自动时，默认状态为停港
                        ship.Flag = type == 1 ? true : false;
                    }
                    _context.Ship.Update(ship);
                    _context.SaveChanges();
                    ShipWeb.ProtoBuffer.Models.StatusRequest sr = new ShipWeb.ProtoBuffer.Models.StatusRequest()
                    {
                        type = ShipWeb.ProtoBuffer.Models.StatusRequest.Type.SAIL,
                        flag = type
                    };
                    if (type==(int)Ship.Type.AUTO)
                    {
                        var result = manager.StatussSet(sr, ship.Id);
                        if (result.result == 0)
                        {
                            ship.Flag = result.flag;
                            _context.Ship.Update(ship);
                            _context.SaveChanges();
                        }

                    }
                    if (ship.Name != name)
                    {
                        sr = new ProtoBuffer.Models.StatusRequest()
                        {
                            type = ProtoBuffer.Models.StatusRequest.Type.NAME,
                            text = name
                        };
                        manager.StatussSet(sr, ship.Id);
                    }
                }
                else
                {
                    Ship ship = new Ship()
                    {
                        Id = id,
                        Name = name,
                        type = Ship.Type.AUTO,
                        Flag=false
                    };
                    //注册船信息时查询组件是中已经有船ID
                    var comp = _context.Component.FirstOrDefault(c => c.Id == ManagerHelp.Cid);
                    if (comp != null)
                    {
                        ship.Id = string.IsNullOrEmpty(comp.ShipId) ? Guid.NewGuid().ToString() : comp.ShipId;
                    }
                    _context.Ship.Add(ship);
                    _context.SaveChanges();
                    //修改船航行状态
                    ShipWeb.ProtoBuffer.Models.StatusRequest sr = new ShipWeb.ProtoBuffer.Models.StatusRequest()
                    {
                        type = ShipWeb.ProtoBuffer.Models.StatusRequest.Type.SAIL,
                        flag =type
                    };
                    manager.StatussSet(sr, ship.Id);
                    //修改船名
                    sr = new ProtoBuffer.Models.StatusRequest()
                    {
                        type = ProtoBuffer.Models.StatusRequest.Type.NAME,
                        text = name
                    };
                    manager.StatussSet(sr, ship.Id);
                }
                return new JsonResult(new { code = 0 });
                #endregion
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 0, msg = "数据保存失败" + ex.Message });
            }
        }
    }
}