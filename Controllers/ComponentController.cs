﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Tls;
using Smartweb.Hubs;
using SmartWeb.DB;
using SmartWeb.Models;
using SmartWeb.ProtoBuffer;
using SmartWeb.ProtoBuffer.Models;
using SmartWeb.Tool;

namespace SmartWeb.Controllers
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
            string identity = "";
            if (base.user.IsLandHome)
            {
                identity = base.user.ShipId;
            }
            List<Models.Component> components = new List<Models.Component>();
            SendDataMsg assembly = new SendDataMsg();
            assembly.SendComponentQuery(identity);
            ProtoBuffer.Models.ComponentResponse response = new ProtoBuffer.Models.ComponentResponse();
            //Task.WhenAny();
            bool flag = true;
            new TaskFactory().StartNew(() =>
            {
                while (ManagerHelp.ComponentReponse == "" && flag)
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
                    ManagerHelp.ComponentReponse = "";
                    if (response.result == 0 && response.componentinfos != null && response.componentinfos.Count > 0)
                    {
                        SaveData(response.componentinfos);
                        foreach (var item in response.componentinfos)
                        {
                            if (item.componentid==ManagerHelp.ComponentId) continue;
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
        }
    }
}