using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.ProjectModel;
using Newtonsoft.Json;
using NuGet.Frameworks;
using Org.BouncyCastle.Asn1.Cms;
using ProtoBuf;
using Smartweb.Hubs;
using SmartWeb.DB;
using SmartWeb.Models;
using SmartWeb.ProtoBuffer;
using SmartWeb.ProtoBuffer.Models;
using SmartWeb.Tool;

namespace SmartWeb.Controllers
{
    public class ShipController : BaseController
    {
        private readonly MyContext _context;
        private int timeout = 5000;//超时退出时间单位秒
        private ILogger<ShipController> _logger;
        private SendDataMsg assembly = null;

        private readonly IHubContext<AlarmVoiceHub> hubContext;

        public ShipController(MyContext context, ILogger<ShipController> logger, IHubContext<AlarmVoiceHub> _hubContext)
        {
            _context = context;
            _logger = logger;
            this.hubContext = _hubContext;
            assembly = new SendDataMsg(hubContext);
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
                if (base.user.IsLandHome)
                {
                    string browsertoken = HttpContext.Session.GetString("comtoken");
                    string XMQComId = base.user.ShipId + ":" + ManagerHelp.GetLandToId(browsertoken);
                    assembly.SendStatusQuery(XMQComId);
                    bool flag = true;
                    new TaskFactory().StartNew(() =>
                    {
                        while (ManagerHelp.StatusReponse == "" && flag)
                        {
                            Thread.Sleep(1000);
                        }
                    }).Wait(3000);
                    flag = false;
                    if (ManagerHelp.StatusReponse != "")
                    {
                        var response = JsonConvert.DeserializeObject<StatusResponse>(ManagerHelp.StatusReponse);
                        if (response != null)
                        {
                            ship.Flag = response.flag;
                            ship.Name = response.name.Split('|')[0];
                            if (response.name.Split('|').Length > 1)
                            {
                                ship.type = (Ship.Type)Convert.ToInt32(response.name.Split('|')[1]);
                            }
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
                    isSet = base.user.EnableConfigure
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
                //var compents = _context.Component.Where(c => c.Type == ComponentType.XMQ).ToList();
                var compents = from a in _context.Component
                            join b in _context.Ship on a.ShipId equals b.Id into c
                            from d in c.DefaultIfEmpty()
                            where a.Type == ComponentType.XMQ
                            select new
                            {
                                a.Cid,
                                a.ShipId,
                                a.Line,
                                d.Name,
                                d.Flag
                            };
                          
                foreach (var item in compents)  
                {
                    ShipViewModel model = new ShipViewModel()
                    {
                        Id = item.Cid,
                        Name = item.Name,
                        flag=item.Flag,
                        Line = item.Line == 0 ? true : false//默认离线
                    };
                    list.Add(model);
                }
                var comLine = compents.Where(c => c.Line == 0);
                new TaskFactory().StartNew(() =>
                {
                    foreach (var item in comLine)
                    {
                        //根据当前XMQ的ID
                        assembly.SendComponentQuery(item.Cid);
                        Task.Factory.StartNew(state => {
                            while (ManagerHelp.ComponentReponse=="")
                            {
                                Thread.Sleep(1000);
                            }
                            var webcom= JsonConvert.DeserializeObject<ProtoBuffer.Models.ComponentResponse>(ManagerHelp.ComponentReponse);
                            string webId = "";
                            if (webcom != null && webcom.componentinfos.Count > 0)
                            {
                                var web = webcom.componentinfos.FirstOrDefault(c => c.type == ComponentInfo.Type.WEB);
                                if (web != null) webId = web.componentid;
                            }
                            if (webId != "") 
                            {
                                assembly.SendStatusQuery(item.Cid + ":" + webId);
                                Task.Factory.StartNew(ss => {
                                    while (ManagerHelp.StatusReponse=="")
                                    {
                                        Thread.Sleep(1000);
                                    }
                                    var response = JsonConvert.DeserializeObject<StatusResponse>(ManagerHelp.StatusReponse);
                                    if (response != null)
                                    {
                                        list.FirstOrDefault(c => c.Id == item.Cid).flag = response.flag;
                                        LandSave(response, item.ShipId);
                                    }
                                }, TaskCreationOptions.LongRunning);
                            }
                        }, TaskCreationOptions.LongRunning);
                    }
                }).Wait(3000);
                var result = new
                {
                    code = 0,
                    data = list,
                    isSet = base.user.EnableConfigure
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
        /// 更新从船舶端过来的信息
        /// </summary>
        /// <param name="response"></param>
        /// <param name="shipId"></param>
        private void LandSave(StatusResponse response, string shipId)
        {
            using (var context=new MyContext())
            {
                //此次查询的是陆地端的船状态表
                var ship = context.Ship.FirstOrDefault(c => c.Id == shipId);
                if (ship != null)
                {
                    var date = response.name.Split('|');
                    if (!string.IsNullOrEmpty(date[0])) ship.Name = date[0];
                    ship.Flag = response.flag;
                    ship.type =(Ship.Type)Convert.ToInt32(date[1]);
                    context.Ship.Update(ship);
                    context.SaveChanges();
                }
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
                if (base.user.IsLandHome)
                {
                    string cid = base.user.ShipId;
                    string tokenstr = HttpContext.Session.GetString("comtoken");
                    string identity = ManagerHelp.GetLandToId(tokenstr);
                    if (string.IsNullOrEmpty(identity))
                    {
                        return new JsonResult(new { code = 1, msg = "当前船舶已失联，请重新连接" });
                    }
                    Ship ship = new Ship()
                    {
                        type = (Ship.Type)type,
                        Name = name,
                        Flag = type == 1 ? true : false
                    };
                    string toId = cid + ":"+ identity;
                    assembly.SendStatusSet(ship,StatusRequest.Type.SAIL, toId);
                    assembly.SendStatusSet(ship,StatusRequest.Type.NAME,toId);                  
                    code = GetResult();
                    if (code == 0) {
                        var component=_context.Component.FirstOrDefault(c => c.Cid == cid);
                        if (component != null) {
                            var landship = _context.Ship.FirstOrDefault(c => c.Id == component.ShipId);
                            if (landship!=null)
                            {
                                landship.Name = ship.Name;
                                landship.type = ship.type;
                                //航行类型为：自动时，默认状态为停港
                                landship.Flag =ship.Flag;
                                _context.Ship.Update(landship);
                                _context.SaveChanges();
                            }
                        }
                    }
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
                        SendMqMsg(ship);
                        code = 0;
                    }
                    #endregion
                }
                if (code == 400) errMsg = "网络超时。。。";
                else if (code != 0) errMsg = "数据保存失败";
                return new JsonResult(new { code = code, msg = errMsg });
            }
            catch (Exception ex)
            {
                _logger.LogError("保存船信息异常Save(" + id + "," + name + "," + type + ")" + ex.Message);
                return new JsonResult(new { code = 0, msg = "数据保存失败" + ex.Message });
            }
        }
        /// <summary>
        /// 推送消息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="ship"></param>
        private void SendMqMsg(Ship ship)
        {
            var components = _context.Component.Where(c => c.Type == ComponentType.AI).ToList();
            foreach (var item in components)
            {
                assembly.SendStatusSet(ship, StatusRequest.Type.SAIL, item.Id);
            }
        }
        /// <summary>
        /// 获取响状态
        /// </summary>
        /// <returns></returns>
        private int GetResult()
        {
            int result = 1;
            try
            {
                bool flag = true;
                new TaskFactory().StartNew(() =>
                {
                    while (flag && ManagerHelp.StatusResult == "")
                    {
                        Thread.Sleep(1000);
                    }
                }).Wait(timeout);
                flag = false;
                if (ManagerHelp.StatusResult != "")
                {
                    result = Convert.ToInt32(ManagerHelp.StatusResult);
                    ManagerHelp.StatusResult = "";
                }
                else
                {
                    result = 400;//请求超时
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }
       
    }
}