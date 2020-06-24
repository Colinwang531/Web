using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Core;
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
        public async Task<IActionResult> Index(string id)
        {
            var list = _context.Camera.Where(c => c.EmbeddedId == id && c.ShipId== ManagerHelp.ShipId).ToListAsync();
            return View(await list);
        }

        // GET: Cameras/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var camera = await _context.Camera
                .FirstOrDefaultAsync(m => m.Id == id);
            if (camera == null)
            {
                return NotFound();
            }

            return View(camera);
        }
        // GET: Cameras/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var camera = await _context.Camera.FindAsync(id);
            if (camera == null)
            {
                return NotFound();
            }
            return View(camera);
        }
        public async Task<IActionResult> EditSave(string id,Camera camera)
        {
            if (id != camera.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                   var embModel=_context.Embedded.FirstOrDefault(e => e.Id == camera.EmbeddedId);
                    if (embModel!=null)
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
                        manager.DeveiceUpdate(emb, embModel.Did, camera.Id);
                    }
                    _context.Update(camera);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CameraExists(camera.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Cameras", new {id=camera.EmbeddedId });
            }
            return View(camera);
        }


        private bool CameraExists(string id)
        {
            return _context.Camera.Any(e => e.Id == id);
        }
    }
}
