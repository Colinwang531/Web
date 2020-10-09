using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Tls;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.ProtoBuffer;
using ShipWeb.ProtoBuffer.Models;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class ComponentController :BaseController
    {
        MyContext _context;
        int timeout = 3000;
        public ComponentController(MyContext context) {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Load()
        {
            List<ComponentViewModel> list = new List<ComponentViewModel>();
            string shipId = base.user.ShipId;//陆地端登陆时存放的是组件ID
            if (ManagerHelp.IsTest)
            {
                list.Add(new ComponentViewModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    name = Enum.GetName(typeof(ComponentType), Convert.ToInt32(ComponentType.DHD)),
                    type = (int)ComponentType.DHD
                });
                list.Add(new ComponentViewModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    name = Enum.GetName(typeof(ComponentType), Convert.ToInt32(ComponentType.ALM)),
                    type = (int)ComponentType.ALM
                });
                list.Add(new ComponentViewModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    name = Enum.GetName(typeof(ComponentType), Convert.ToInt32(ComponentType.MED)),
                    type = (int)ComponentType.MED
                });
                list.Add(new ComponentViewModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    name = Enum.GetName(typeof(ComponentType), Convert.ToInt32(ComponentType.HKD)),
                    type = (int)ComponentType.HKD
                });
            }
            else
            {
                string identity = "";
                List<Models.Component> components = new List<Models.Component>();
                if (base.user.IsLandHome)
                {
                    var shipIdentity = _context.Component.FirstOrDefault(c => c.Id == shipId && c.Type == ComponentType.WEB);
                    identity = shipIdentity.Id;
                }
                SendDataMsg assembly = new SendDataMsg();
                assembly.SendComponentQuery(identity);
                ProtoBuffer.Models.ComponentResponse response = new ProtoBuffer.Models.ComponentResponse();
                //Task.WhenAny();
                bool flag = true;
                new TaskFactory().StartNew(() =>
                {
                    while (ManagerHelp.ComponentReponse == ""&&flag)
                    {
                        Thread.Sleep(100);
                    }
                }).Wait(3000);
                flag = false;
                try
                {
                    if (ManagerHelp.ComponentReponse != "")
                    {
                        response = JsonConvert.DeserializeObject<ProtoBuffer.Models.ComponentResponse>(ManagerHelp.ComponentReponse);
                        ManagerHelp.ComponentReponse="";
                        if (response.result == 0 && response.componentinfos != null && response.componentinfos.Count > 0)
                        {
                            SaveData(response.componentinfos);
                            foreach (var item in response.componentinfos)
                            {
                                if (item.type == ProtoBuffer.Models.ComponentInfo.Type.WEB) continue;
                                ComponentViewModel model = new ComponentViewModel()
                                {
                                    Id = item.componentid,
                                    name = item.cname,
                                    type = (int)item.type
                                };
                                list.Add(model);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    
                }
            }
            var data = new
            {
                code = 0,
                data = list
            };
            return new JsonResult(data);
        }
        /// <summary>
        /// 将获取到的组件信息存入数据库中
        /// </summary>
        /// <param name="componentInfos"></param>
        private void SaveData(List<ComponentInfo> componentInfos) 
        {
            if (base.user.IsLandHome)
            {
                //陆地端将数据写入缓存中
                List<ComponentToken> tokens = new List<ComponentToken>();
                foreach (var item in componentInfos) 
                {
                    if (item.type == ProtoBuffer.Models.ComponentInfo.Type.WEB) continue;
                    ComponentToken token = new ComponentToken()
                    {
                        Id = item.componentid,
                        //CommId = item.commid,
                        Name = item.cname,
                        Type = (ComponentType)item.type
                    };
                    tokens.Add(token);
                }
                string com = JsonConvert.SerializeObject(tokens);
                HttpContext.Session.SetString("comtoken", com);
            }
            else
            {
                //船舶端将数据写入数据库
                var components = _context.Component.Where(c => c.Type != ComponentType.WEB).ToList();
                _context.RemoveRange(components);
                foreach (var item in componentInfos)
                {
                    if (item.type == ComponentInfo.Type.WEB) continue;
                    ShipWeb.Models.Component model = new ShipWeb.Models.Component()
                    {
                        Id = item.componentid,
                        Name = item.cname,
                        Type = (ComponentType)item.type
                    };
                    _context.Component.Add(model);
                }
                _context.SaveChanges();
            }
        }
    }
}