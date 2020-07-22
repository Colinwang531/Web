﻿using System;
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
            var ship = _context.Ship.FirstOrDefault(c => c.Id == ManagerHelp.ShipId);
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
            return View();
        }
        public IActionResult Load()
        {
            try
            {
                var ship = _context.Ship.FirstOrDefault(c => c.Id == ManagerHelp.ShipId);
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
        public IActionResult LoadAll()
        {
            try
            {
                var ship = _context.Ship.ToList();
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
                if (ManagerHelp.IsShowLandHome)
                {
                    #region 陆地端登陆船舶端修改船状态
                    ShipWeb.ProtoBuffer.Models.StatusRequest sr = new ShipWeb.ProtoBuffer.Models.StatusRequest()
                    {
                        type = ShipWeb.ProtoBuffer.Models.StatusRequest.Type.SAIL,
                        flag = type
                    };
                    manager.StatesSet(sr,id);
                    sr = new ProtoBuffer.Models.StatusRequest()
                    {
                        type = ProtoBuffer.Models.StatusRequest.Type.NAME,
                        text = name
                    };
                    manager.StatesSet(sr,id);
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
                        ship.Flag = type == 0 ? false : true;
                    }
                    _context.Ship.Update(ship);
                    _context.SaveChanges();
                    ShipWeb.ProtoBuffer.Models.StatusRequest sr = new ShipWeb.ProtoBuffer.Models.StatusRequest()
                    {
                        type = ShipWeb.ProtoBuffer.Models.StatusRequest.Type.SAIL,
                        flag = type
                    };
                    var result= manager.StatesSet(sr, ship.Id);
                    if (result.result==0)
                    {
                        ship.Flag = result.flag;
                        _context.Ship.Update(ship);
                        _context.SaveChanges();
                    }
                    if (ship.Name != name)
                    {
                        sr = new ProtoBuffer.Models.StatusRequest()
                        {
                            type = ProtoBuffer.Models.StatusRequest.Type.NAME,
                            text = name
                        };
                        manager.StatesSet(sr, ship.Id);
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
                    manager.StatesSet(sr, ship.Id);
                    //修改船名
                    sr = new ProtoBuffer.Models.StatusRequest()
                    {
                        type = ProtoBuffer.Models.StatusRequest.Type.NAME,
                        text = name
                    };
                    manager.StatesSet(sr, ship.Id);
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