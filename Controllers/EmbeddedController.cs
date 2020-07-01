using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class EmbeddedController : BaseController
    {
        private readonly MyContext _context;
        private ProtoBuffer.ProtoManager manager;
        public EmbeddedController(MyContext context)
        {
            _context = context;
            manager = new ProtoBuffer.ProtoManager();
        }

       
        public IActionResult Index(string id="")
        {
            if (!string.IsNullOrEmpty(id))
            {
                ManagerHelp.ShipId = id;
            }
            return View();
        }
        public IActionResult Load()
        {
            var data = _context.Embedded.Where(c=>c.ShipId==ManagerHelp.ShipId).ToList();
            var result = new {
                code = 0,
                data = data,
                isSet = !string.IsNullOrEmpty(ManagerHelp.ShipId) ? ManagerHelp.IsSet : false
            };
            return new JsonResult(result);
        }
        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Save(Models.Embedded Embedded)
        {
            if (ModelState.IsValid)
            {            
                string shipId = ManagerHelp.ShipId; //船ID              
                string iditity = Guid.NewGuid().ToString();
                ProtoBuffer.Models.Embedded emb = new ProtoBuffer.Models.Embedded()
                {
                    ip = Embedded.IP,
                    name = Embedded.Name,
                    password = Embedded.Password,
                    port = Embedded.Port,
                    nickname = Embedded.Nickname
                };
                Embedded.Id = iditity;
                Embedded.ShipId = shipId;

                //测试数据
                Random rd = new Random();
                Embedded.Did = rd.Next(111, 999).ToString();
                ////发送注册设备消息
                //ProtoBuffer.Models.DeviceResponse rep = manager.DeveiceAdd(emb, iditity);
                //if (rep != null && rep.result == 0)
                //{
                //    Embedded.Did = rep.did;
                //    if (rep.embedded.Count == 1 && rep.embedded[0].cameras.Count > 0)
                //    {
                //        List<Camera> list = new List<Camera>();
                //        var repList = rep.embedded[0].cameras;
                //        foreach (var item in repList)
                //        {
                //            string cmId = Guid.NewGuid().ToString();
                //            Camera cmodel = new Camera()
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
        public async Task<IActionResult> EditSave(string id, Models.Embedded Embedded)
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
                      await Task.Factory.StartNew(state => {
                            ProtoBuffer.Models.Embedded emb = new ProtoBuffer.Models.Embedded()
                            {
                                ip = Embedded.IP,
                                name = Embedded.Name,
                                password = Embedded.Password,
                                port = Embedded.Port,
                                nickname = Embedded.Nickname
                            };
                            int result = manager.DeveiceUpdate(emb, Embedded.Did, id);
                        }, TaskCreationOptions.LongRunning);
                        
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
            try
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
                //先删除服务器上的再删除本地的
                //int resutl = manager.DeveiceDelete(Embedded.Did, Embedded.Id);
                //if (resutl == 0)
                //{
                    var cameras = _context.Camera.Where(c => c.EmbeddedId == Embedded.Id).ToList();
                    foreach (var item in cameras)
                    {
                        var cameraConfig = _context.CameraConfig.FirstOrDefault(c => c.Cid == item.Cid);
                        if (cameraConfig != null)
                        {
                            //删除摄像机配置表
                            _context.CameraConfig.Remove(cameraConfig);
                        }
                        //删除摄像机表
                        _context.Camera.Remove(item);
                    }
                    //删除设备表
                    _context.Embedded.Remove(Embedded);
                    _context.SaveChanges();
                //}
                return new JsonResult(new { code = 0, msg = "删除成功!" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "删除失败!" + ex.Message });
            }
           
        }
        private bool EmbeddedExists(string id)
        {
            return _context.Embedded.Any(e => e.Id == id);
        }
    }
}
