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
        private int timeout = 5000;//超时退出时间单位秒
        private ILogger<ShipController> _logger;
        private SendDataMsg assembly = new SendDataMsg();
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
                var compents = _context.Component.Where(c => c.Type == Component.ComponentType.WEB && c.ShipId != null).ToList();
                foreach (var item in ship)
                {
                    ShipViewModel model = new ShipViewModel()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Line=false//默认离线
                    };
                    if (ManagerHelp.IsTest)
                    {
                        model.Line = true;
                        model.flag = item.Flag;
                    }
                    else
                    {
                        var nextIdentity = compents.FirstOrDefault(c => c.ShipId == item.Id).Id;
                        assembly.SendStatusQuery(nextIdentity);
                        ProtoBuffer.Models.StatusResponse response = new ProtoBuffer.Models.StatusResponse();
                        try
                        {
                            bool flag = true;
                            new TaskFactory().StartNew(() =>
                            {
                                while (flag)
                                {
                                    if (ManagerHelp.Reponse != "")
                                    {
                                        response = JsonConvert.DeserializeObject<ProtoBuffer.Models.StatusResponse>(ManagerHelp.Reponse);
                                        flag = false;
                                    }
                                }
                            }).Wait(timeout);
                            flag = false;
                        }
                        catch (Exception)
                        {
                        }
                        if (response.result==0)
                        {
                            model.Line = true;//在线
                            model.Name = response.name;
                            model.flag = response.flag;
                            //从船舶端过来的船名与陆地端不同时，更改陆地端的船名
                            if (item.Name != response.name)
                            {
                                item.Name = response.name;
                                _context.Ship.Update(item);
                                _context.SaveChanges();
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
                if (base.user.IsLandHome&&!ManagerHelp.IsTest)
                {
                    var component = _context.Component.FirstOrDefault(c => c.Type == Component.ComponentType.WEB && c.ShipId == id);
                    #region 陆地端登陆船舶端修改船状态
                    ShipWeb.ProtoBuffer.Models.StatusRequest sr = new ShipWeb.ProtoBuffer.Models.StatusRequest()
                    {
                        type = ShipWeb.ProtoBuffer.Models.StatusRequest.Type.SAIL,
                        flag = type
                    };
                    assembly.SendStatusSet(sr, component.Id);
                    sr = new ProtoBuffer.Models.StatusRequest()
                    {
                        type = ProtoBuffer.Models.StatusRequest.Type.NAME,
                        text = name
                    };
                    assembly.SendStatusSet(sr, component.Id);                   
                    return new JsonResult(new { code = 0 });
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
                        if (!ManagerHelp.IsTest)
                        {
                            ShipWeb.ProtoBuffer.Models.StatusRequest sr = new ShipWeb.ProtoBuffer.Models.StatusRequest()
                            {
                                type = ShipWeb.ProtoBuffer.Models.StatusRequest.Type.SAIL,
                                flag = type
                            };
                            assembly.SendStatusSet(sr);
                            if (ship.Name != name)
                            {
                                sr = new ProtoBuffer.Models.StatusRequest()
                                {
                                    type = ProtoBuffer.Models.StatusRequest.Type.NAME,
                                    text = name
                                };
                                assembly.SendStatusSet(sr);
                            }
                        }
                    }
                    return new JsonResult(new { code = 0 });
                    #endregion
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("保存船信息异常Save("+id+","+name+","+type+")" + ex.Message);
                return new JsonResult(new { code = 0, msg = "数据保存失败" + ex.Message });
            }
        }
    }
}