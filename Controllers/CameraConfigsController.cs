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
using Org.BouncyCastle.Crypto.Tls;
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
        private ProtoManager manager = new ProtoManager();
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
            if (ManagerHelp.IsShowLandHome)
            {
                return LandLoad();
            }
            var aa = from a in _context.Camera
                     join b in _context.CameraConfig on a.Cid equals b.Cid
                     into c
                     from d in c.DefaultIfEmpty()
                     where a.ShipId==ManagerHelp.ShipId
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
                msg="",
                isSet = !string.IsNullOrEmpty(ManagerHelp.ShipId) ? ManagerHelp.IsSet:false
            };
            return new JsonResult(result);

        }
        private IActionResult LandLoad()
        {
            var alg=manager.AlgorithmQuery(ManagerHelp.ShipId);
            var data = from a in alg
                       select new
                       {
                           id = a.cid.Split(',')[0],
                           Cid = a.cid.Split(',')[1],
                           NickName = a.cid.Split(',')[2],
                           EnableAttendanceIn = a.enableattendancein,
                           EnableAttendanceOut = a.enableattendanceout,
                           EnableFight = a.enablefight,
                           EnableHelmet = a.enablehelmet,
                           EnablePhone = a.enablephone,
                           EnableSleep = a.enablesleep,
                           GPU = a.gpu,
                           a.similar
                       };
            var result = new
            {
                code = 0,
                data = data,
                msg = "",
                isSet = !string.IsNullOrEmpty(ManagerHelp.ShipId) ? ManagerHelp.IsSet : false
            };
            return new JsonResult(result);
        }
        public IActionResult Create(string mList)
        {
            
            List<CameraConfig> modelList = JsonConvert.DeserializeObject<List<CameraConfig>>(mList);
            if (ModelState.IsValid)
            {
                if (!ManagerHelp.IsSet)
                {
                    return new JsonResult(new { code = 1, msg = "您没有权限修改数据" });
                }
                //发送消息
                Task.Factory.StartNew(state =>
                {
                    List<Configure> list = new List<Configure>();
                    foreach (var item in modelList)
                    {
                        Configure mqModel = new Configure()
                        {
                            cid =item.Id+","+item.Cid,
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
                    int result = manager.AlgorithmSet(ManagerHelp.ShipId, list);
                }, TaskCreationOptions.LongRunning);
                //陆地端设置算法配置是不写入陆地端的库的
                if (!ManagerHelp.IsShowLandHome)
                {
                    foreach (var item in modelList)
                    {
                        item.ShipId = ManagerHelp.ShipId;
                        if (item.Id != "null" && item.Id != "")
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
            }
            return new JsonResult(new { code = 0 });
        }
    }
}
