using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using ProtoBuf;
using ProtoBuf.Models;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class EmbeddedController : Controller
    {
        private readonly MyContext _context;

        public EmbeddedController(MyContext context)
        {
            _context = context;
        }

        // GET: Embedded
        public async Task<IActionResult> Index()
        {
            //if (ManagerHelp.Cid==""|| ManagerHelp.ShipId=="")
            //{
            //    return View("Login/Index");
            //}
           
            return View(await _context.Embedded.ToListAsync());
        }

        public IActionResult Details(string id)
        {
            var list = _context.Camera.ToList();
            return View(list);
        }
        public IActionResult Create()
        {
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Models.Embedded Embedded)
        {
            if (ModelState.IsValid)
            {
                ProtoManager manager = new ProtoManager();               
                string shipId = ManagerHelp.ShipId; //船ID              
                string iditity = Guid.NewGuid().ToString();
                ProtoBuf.Models.Embedded emb = new ProtoBuf.Models.Embedded()
                {
                    ip = Embedded.IP,
                    name = Embedded.Name,
                    password = Embedded.Password,
                    port = Embedded.Port,
                    nickname = Embedded.Nickname
                };
                Embedded.Id = iditity;
                Embedded.ShipId = shipId;
                Embedded.Did = "asdf";
                //模似已接收消息数据
                Embedded.CameraModelList = new List<Models.Camera>()
                {
                     new Models.Camera()
                     {
                         Cid="111",
                         EmbeddedId= iditity,
                         Enalbe=false,
                         Id=Guid.NewGuid().ToString(),
                         Index=1,
                         IP="127.0.0.1",
                         NickName="测试",
                         ShipId= shipId
                     },
                     new Models.Camera()
                     {
                         Cid="2222",
                         EmbeddedId= iditity,
                         Enalbe=false,
                         Id=Guid.NewGuid().ToString(),
                         Index=2,
                         IP="127.0.0.2",
                         NickName="测试",
                         ShipId= shipId

                     }
               };
                //发送注册设备消息
                //DeviceResponse rep = manager.DeveiceAdd(emb, iditity);
                //if (rep != null && rep.result == 0)
                //{
                //    Embedded.Did = rep.did;
                //    if (rep.embedded.Count == 1 && rep.embedded[0].cameras.Count > 0)
                //    {
                //        List<CameraModel> list = new List<CameraModel>();
                //        var repList = rep.embedded[0].cameras;
                //        foreach (var item in repList)
                //        {
                //            string cmId = Guid.NewGuid().ToString();
                //            CameraModel cmodel = new CameraModel()
                //            {
                //                Cid = item.cid,
                //                EmbeddedId = iditity,
                //                Enalbe = item.enable,
                //                Id = cmId,
                //                NickName = item.nickname,
                //                IP = item.ip,
                //                ShipId = shipId,
                //                Index = item.index
                //            };
                //            list.Add(cmodel);
                //        }
                //    }
                _context.Embedded.Add(Embedded);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
                //}
            }
            return View(Embedded);
        }

        // GET: Embedded/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var Embedded = await _context.Embedded.FindAsync(id);
           
            if (Embedded == null)
            {
                return NotFound();
            }
            return View(Embedded);
        }

     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Models.Embedded Embedded)
        {
            if (id != Embedded.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (Embedded != null)
                    {
                        ProtoManager manager = new ProtoManager();
                        ProtoBuf.Models.Embedded emb = new ProtoBuf.Models.Embedded()
                        {
                            ip = Embedded.IP,
                            name = Embedded.Name,
                            password = Embedded.Password,
                            port = Embedded.Port,
                            nickname = Embedded.Nickname
                        };
                        int result = manager.DeveiceUpdate(emb, Embedded.Did, id);
                    }
                    _context.Update(Embedded);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmbeddedExists(Embedded.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(Embedded);
        }

        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Embedded = _context.Embedded.Find(id);
            if (Embedded == null)
            {
                return NotFound();
            }
            ProtoManager manager = new ProtoManager();
            int resutl= manager.DeveiceDelete(Embedded.Did, Embedded.Id);
            if (resutl==0)
            {
               var cameras= _context.Camera.Where(c => c.EmbeddedId == Embedded.Id).ToList();
                foreach (var item in cameras)
                {
                    var cameraConfig=_context.CameraConfig.FirstOrDefault(c => c.Cid == item.Cid);
                    //删除摄像机配置表
                    _context.CameraConfig.Remove(cameraConfig);
                    //删除摄像机表
                    _context.Camera.Remove(item);
                }
                //删除设备表
                _context.Embedded.Remove(Embedded);
                _context.SaveChanges();
            }
            return new JsonResult("数据删除成功");
        }
        private bool EmbeddedExists(string id)
        {
            return _context.Embedded.Any(e => e.Id == id);
        }
    }
}
