using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private int timeout = 5000;//超时退出时间单位秒
        private ILogger<ShipController> _logger;
        public ShipController(MyContext context, ILogger<ShipController>logger)
        {
            _context = context;
            _logger = logger;
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
        public async Task<IActionResult> LoadAll()
        {
            try
            {
                var ship = await _context.Ship.ToListAsync();
                List<ShipViewModel> list = new List<ShipViewModel>();
                foreach (var item in ship)
                {
                    ShipViewModel model = new ShipViewModel()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Line=false//默认离线
                    };
                    model.Line = true;
                    model.flag = item.Flag;
                    //new TaskFactory().StartNew(() => {
                    //    #region 测试数据
                    //    //ProtoBuffer.Models.ComponentResponse rep = new ProtoBuffer.Models.ComponentResponse()
                    //    //{
                    //    //    result = 0,
                    //    //    componentinfos = new List<ProtoBuffer.Models.ComponentInfo>()
                    //    // {
                    //    //     new ProtoBuffer.Models.ComponentInfo ()
                    //    //     {
                    //    //        type= ProtoBuffer.Models.ComponentInfo.Type.WEB
                    //    //     }
                    //    // }
                    //    //};
                    //    #endregion
                    //    ProtoBuffer.Models.ComponentResponse rep = manager.ComponentQuery(item.Id);
                    //    if (rep.result == 0)
                    //    {
                    //        var info = rep.componentinfos.Where(c => c.type == ProtoBuffer.Models.ComponentInfo.Type.WEB);
                    //        if (info.Count() > 0)
                    //        {
                    //            model.Line = true;//在线
                    //            ProtoBuffer.Models.StatusResponse strep = manager.StatusQuery(item.Id);
                    //            if (strep.result == 0)
                    //            {
                    //                model.Name = strep.name;
                    //                model.flag = strep.flag;
                    //                //从船舶端过来的船名与陆地端不同时，更改陆地端的船名
                    //                if (item.Name != strep.name)
                    //                {
                    //                    item.Name = strep.name;
                    //                    _context.Ship.Update(item);
                    //                    _context.SaveChanges();
                    //                }
                    //            }
                    //        }
                    //    }
                    //}).Wait(timeout);                  
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
                _logger.LogError("陆地端获取船信息异常【LoadAll】" + ex.Message);
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
                    new TaskFactory().StartNew(() => {
                        ShipWeb.ProtoBuffer.Models.StatusRequest sr = new ShipWeb.ProtoBuffer.Models.StatusRequest()
                        {
                            type = ShipWeb.ProtoBuffer.Models.StatusRequest.Type.SAIL,
                            flag = type
                        };
                        var res = manager.StatussSet(sr, id);
                        sr = new ProtoBuffer.Models.StatusRequest()
                        {
                            type = ProtoBuffer.Models.StatusRequest.Type.NAME,
                            text = name
                        };
                        var res1 = manager.StatussSet(sr, id);
                        if (res.result==0&&res1.result==0)
                        {
                            return new JsonResult(new { code = 0 });
                        }
                        return new JsonResult(new { code = 1, msg = "修改数据失败" });
                    }).Wait(timeout);

                    #endregion
                }
                else
                {
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
                        new TaskFactory().StartNew(() => {
                            ShipWeb.ProtoBuffer.Models.StatusRequest sr = new ShipWeb.ProtoBuffer.Models.StatusRequest()
                            {
                                type = ShipWeb.ProtoBuffer.Models.StatusRequest.Type.SAIL,
                                flag = type
                            };
                            if (type == (int)Ship.Type.AUTO)
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
                        }).Wait(timeout);
                    }
                    return new JsonResult(new { code = 0 });
                    #endregion
                }
                return new JsonResult(new { code = 1, msg = "请求超时。。。" });
            }
            catch (Exception ex)
            {
                _logger.LogError("保存船信息异常Save("+id+","+name+","+type+")" + ex.Message);
                return new JsonResult(new { code = 0, msg = "数据保存失败" + ex.Message });
            }
        }
    }
}