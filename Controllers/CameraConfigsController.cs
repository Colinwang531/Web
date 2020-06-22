using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using ProtoBuf;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.ProtoBuffer;
using ShipWeb.ProtoBuffer.Models;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class CameraConfigsController : BaseController
    {
        private readonly MyContext _context;

        public CameraConfigsController(MyContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Load()
        {
            var aa = from a in _context.Camera
                     join b in _context.CameraConfig on a.Cid equals b.Cid
                     into c
                     from d in c.DefaultIfEmpty()
                     select new
                     {
                         a.NickName,
                         a.Cid,
                         d.EnableAttendanceIn,
                         d.EnableAttendanceOut,
                         d.EnableFight,
                         d.EnableHelmet,
                         d.EnablePhone,
                         d.EnableSleep,
                         d.GPU,
                         d.Id,
                         d.ShipId,
                         d.Similar,
                     };
            var list = aa.ToList();
            var result = new
            {
                code = 0,
                data = list,
                isSet = ManagerHelp.IsSet
            };
            return new JsonResult(result);

        }
       
        public IActionResult Create(string mList)
        {
            
            List<CameraConfig> modelList = JsonConvert.DeserializeObject<List<CameraConfig>>(mList);
            if (ModelState.IsValid)
            {
                if (ManagerHelp.IsSet)
                {
                    return new JsonResult(new { code = 1, msg = "您没有权限修改数据" });
                }
                //发送消息
                Task.Factory.StartNew(state =>
                {
                    ProtoManager manager = new ProtoManager();
                    string identity = Guid.NewGuid().ToString();
                    List<Configure> list = new List<Configure>();
                    foreach (var item in modelList)
                    {

                        Configure mqModel = new Configure()
                        {
                            cid = item.Cid,
                            enableattendancein = item.EnableAttendanceIn,
                            enableattendanceout = item.EnableAttendanceOut,
                            enablefight = item.EnableFight,
                            enablehelmet = item.EnableHelmet,
                            enablephone = item.EnablePhone,
                            enablesleep = item.EnableSleep,
                            gpu = item.GPU,
                            similar = (float)item.Similar,
                        };
                        list.Add(mqModel);
                    }
                    //返回0为成功
                    int result = manager.AlgorithmSet(identity, list);
                }, TaskCreationOptions.LongRunning);

                foreach (var item in modelList)
                {
                    item.ShipId = ManagerHelp.ShipId;
                    if (item.Id!="")
                    {
                        _context.Update(item);
                    }
                    else
                    {
                        item.Id = Guid.NewGuid().ToString();
                        _context.Add(item);
                    }
                }
                _context.SaveChanges();
            }
            return new JsonResult(new { code = 0 });
        }
    }
}
