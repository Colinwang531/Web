using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProtoBuf;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class CamerasController : BaseController
    {
        private readonly MyContext _context;

        public CamerasController(MyContext context)
        {
            _context = context;
        }

        // GET: Cameras
        public IActionResult Index(string id)
        {
            ViewBag.id = id;
            return View();
        }
        public IActionResult Load(string id)
        {
            var camera = _context.Camera.Where(m => m.EmbeddedId == id.Trim()).ToList();
            var result = new
            {
                code = 0,
                data = camera
            };
            return new JsonResult(result);
        }
      
        public IActionResult Save(string id,string nickName,string enalbe)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Camera camera = _context.Camera.FirstOrDefault(c => c.Id == id);
                    if (camera == null)
                    {
                        return new JsonResult(new { code = 1, msg = "数据不存在" });
                    }
                    camera.NickName = nickName;
                    camera.Enalbe = enalbe == "1" ? true : false;
                    var embModel = _context.Embedded.FirstOrDefault(e => e.Id == camera.EmbeddedId);
                    if (embModel != null)
                    {
                        ProtoBuffer.ProtoManager manager = new ProtoBuffer.ProtoManager();
                        ProtoBuffer.Models.Embedded emb = new ProtoBuffer.Models.Embedded()
                        {
                            cameras = new List<ProtoBuffer.Models.Camera>() {
                                 new ProtoBuffer.Models.Camera(){
                                 cid=camera.Cid,
                                 index=camera.Index,
                                 enable=camera.Enalbe,
                                 ip=camera.Id,
                                 nickname=camera.NickName
                                 }
                               },
                            did = embModel.Did
                        };
                        Task.Factory.StartNew(state =>
                        {
                            manager.DeveiceUpdate(emb, embModel.Did, camera.Id);
                        }, TaskCreationOptions.LongRunning);
                        _context.Update(camera);
                        _context.SaveChangesAsync();
                    };
                }
                catch (Exception ex)
                {
                    return new JsonResult(new { code = 1, msg = "数据保存失败！" + ex.Message });
                }

            }
            return new JsonResult(new { code = 0 });
        }


    }
}
