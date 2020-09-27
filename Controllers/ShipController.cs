using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.ProjectModel;
using Newtonsoft.Json;
using NuGet.Frameworks;
using Org.BouncyCastle.Asn1.Cms;
using ProtoBuf;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.ProtoBuffer;
using ShipWeb.ProtoBuffer.Models;
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
                Ship ship = new Ship();                 
                if (base.user.IsLandHome&&!ManagerHelp.IsTest)
                {
                    string identity = base.user.ShipId;
                    assembly.SendStatusQuery(identity);
                    bool flag = true;
                    new TaskFactory().StartNew(() => {
                        while (ManagerHelp.StatusReponse == "" && flag)
                        {
                            Thread.Sleep(100);
                        }
                    }).Wait(3000);
                    flag = false;
                    if (ManagerHelp.StatusReponse != "")
                    {
                        var response = JsonConvert.DeserializeObject<StatusResponse>(ManagerHelp.StatusReponse);
                        if (response!=null)
                        {
                            ship.Flag = response.flag;
                            ship.Name = response.name;
                        }
                    }
                }
                else
                {
                    ship = _context.Ship.FirstOrDefault();
                }
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
                List<ShipViewModel> list = new List<ShipViewModel>();
                var compents = _context.Component.Where(c => c.Type ==ComponentType.WEB && c.CommId!=null).ToList();
                foreach (var item in compents)
                {
                    ShipViewModel model = new ShipViewModel()
                    {
                        Id = item.CommId,
                        Name = item.Name,
                        Line = item.Line == 0 ? true : false//默认离线
                    };
                    list.Add(model);
                }
                var comLine = compents.Where(c => c.Line == 0);
                bool flag = true;
                new TaskFactory().StartNew(() => {
                    foreach (var item in comLine)
                    {
                        while (ManagerHelp.StatusReponse == "" && flag)
                        {
                            if (ManagerHelp.StatusReponse!="")
                            {
                                var response = JsonConvert.DeserializeObject<StatusResponse>(ManagerHelp.StatusReponse);
                                if (response != null) {
                                    list.FirstOrDefault(c => c.Id == item.Id).Line = response.flag;
                                }
                            }
                            Thread.Sleep(100);
                        }
                    }
                }).Wait(3000);                
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
                int code = 1;
                string errMsg = "";
                if (base.user.IsLandHome&&!ManagerHelp.IsTest)
                {
                    string identity = base.user.ShipId;
                    var components = _context.Component.Where(c => c.Type ==ComponentType.AI).ToList();
                    foreach (var item in components)
                    {
                        ShipWeb.ProtoBuffer.Models.StatusRequest sr = new ShipWeb.ProtoBuffer.Models.StatusRequest()
                        {
                            type = ShipWeb.ProtoBuffer.Models.StatusRequest.Type.SAIL,
                            flag = type
                        };
                        assembly.SendStatusSet(sr, identity + ":"+item.CommId);
                        sr = new ProtoBuffer.Models.StatusRequest()
                        {
                            type = ProtoBuffer.Models.StatusRequest.Type.NAME,
                            text = name
                        };
                        assembly.SendStatusSet(sr, identity + ":" + item.CommId);
                    }
                    code = GetResult();
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
                           var components= _context.Component.Where(c => c.Type ==ComponentType.AI).ToList();
                            foreach (var item in components)
                            {
                                ShipWeb.ProtoBuffer.Models.StatusRequest sr = new ShipWeb.ProtoBuffer.Models.StatusRequest()
                                {
                                    type = ShipWeb.ProtoBuffer.Models.StatusRequest.Type.SAIL,
                                    flag = type
                                };
                                assembly.SendStatusSet(sr,item.CommId);
                                if (ship.Name != name)
                                {
                                    sr = new ProtoBuffer.Models.StatusRequest()
                                    {
                                        type = ProtoBuffer.Models.StatusRequest.Type.NAME,
                                        text = name
                                    };
                                    assembly.SendStatusSet(sr,item.CommId);
                                }

                            }
                           code= GetResult();
                        }
                    }
                    #endregion
                }
                if (code == 400) errMsg = "网络超时。。。";
                else if (code != 0) errMsg = "数据保存失败";
                return new JsonResult(new { code = code,msg=errMsg });
            }
            catch (Exception ex)
            {
                _logger.LogError("保存船信息异常Save("+id+","+name+","+type+")" + ex.Message);
                return new JsonResult(new { code = 0, msg = "数据保存失败" + ex.Message });
            }
        }
        private int GetResult()
        {
            int result = 1;
            bool flag = true;
            new TaskFactory().StartNew(() => {
                while (flag&&ManagerHelp.StatusReponse=="")
                {
                    Thread.Sleep(100);
                }
            }).Wait(timeout);
            flag = false;
            if (ManagerHelp.StatusReponse != "")
            {
                result = Convert.ToInt32(ManagerHelp.StatusReponse);
                ManagerHelp.StatusReponse = "";
            }
            else {
                result = 400;//请求超时
            }
            return result;
        }
    }
}