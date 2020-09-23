using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Tls;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.ProtoBuffer;
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
                string identity = "";
                List<Component> components = new List<Component>();
                if (base.user.IsLandHome)
                {
                    string shipId = base.user.ShipId;//陆地端登陆时存放的是组件ID
                    var shipIdentity = _context.Component.FirstOrDefault(c => c.Id==shipId&&c.Type == ComponentType.WEB);
                    identity = shipIdentity.CommId;
                }
                SendDataMsg assembly = new SendDataMsg();
                assembly.SendComponentQuery(identity);
                ProtoBuffer.Models.ComponentResponse response=new ProtoBuffer.Models.ComponentResponse ();
                //Task.WhenAny();
                //获取异步处理结果
                bool flag = true;
                new TaskFactory().StartNew(() =>
                {
                    while (flag)
                    {
                        if (ManagerHelp.Reponse != "")
                        {
                            try
                            {
                                response = JsonConvert.DeserializeObject<ProtoBuffer.Models.ComponentResponse>(ManagerHelp.Reponse);
                            }
                            catch (Exception)
                            {
                            }
                            finally
                            {
                                ManagerHelp.Reponse = "";
                                flag = false;
                            }
                        }
                        Thread.Sleep(500);
                    }
                }).Wait(3000);               
                flag = false;
                if (response.result == 0 && response.componentinfos!=null&&response.componentinfos.Count > 0)
                {
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

            var data = new
            {
                code = 0,
                data = list
            };
            return new JsonResult(data);
        }
    }
}