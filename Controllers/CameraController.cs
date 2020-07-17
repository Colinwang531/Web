using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NetMQ;
using ProtoBuf;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class CameraController : BaseController
    {
        private readonly MyContext _context;
        private ProtoBuffer.ProtoManager manager = new ProtoBuffer.ProtoManager();
        public CameraController(MyContext context)
        {
            _context = context;
        }

        // GET: Cameras
        public IActionResult Index(string id)
        {
            ViewBag.id = id.Trim();           
            return View();
        }
        public IActionResult Load(string id)
        {
            if (ManagerHelp.IsShowLandHome)
            {
                return LandLoad(id);
            }
            var camera = _context.Camera.Where(m => m.DeviceId == id.Trim()).ToList();
            var result = new
            {
                code = 0,
                data = camera
            };
            return new JsonResult(result);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="did"></param>
        /// <returns></returns>
        private IActionResult LandLoad(string did) 
        {
            var emdList=manager.DeviceQuery(ManagerHelp.ShipId, did);
            var cams = emdList.Count>0?emdList[0].camerainfos:new List<ProtoBuffer.Models.CameraInfo> ();
            var list = from a in cams
                       select new
                       {
                           id=a.cid,
                           enalbe=a.enable,
                           a.index,
                           a.ip,
                           nickName=a.nickname
                       };
            var result = new
            {
                code = 0,
                data = list
            };
            return new JsonResult(result);
        }
        public IActionResult Save(string id,string did,string nickName,string enalbe)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //陆地端远程修改摄像机信息
                    if (ManagerHelp.IsShowLandHome)
                    {
                        ProtoBuffer.Models.DeviceInfo emb = new ProtoBuffer.Models.DeviceInfo()
                        {
                            camerainfos = new List<ProtoBuffer.Models.CameraInfo>() {
                                 new ProtoBuffer.Models.CameraInfo(){
                                 cid=id,
                                 enable=enalbe == "1" ? true : false,
                                 nickname=nickName
                                 }
                               },
                            did = did
                        };
                        var code = manager.DeveiceUpdate(emb, did, ManagerHelp.ShipId);
                        return new JsonResult(new { code = code, msg = code == 1 ? "数据修改失败" : "数据修改成功" });
                    }

                    Camera camera = _context.Camera.FirstOrDefault(c => c.Id == id);
                    if (camera == null)
                    {
                        return new JsonResult(new { code = 1, msg = "数据不存在" });
                    }
                    camera.NickName = nickName;
                    camera.Enalbe = enalbe == "1" ? true : false;
                    var embModel = _context.Device.FirstOrDefault(e => e.Id == camera.DeviceId);
                    if (embModel != null)
                    {
                        ProtoBuffer.Models.DeviceInfo emb = new ProtoBuffer.Models.DeviceInfo()
                        {
                            camerainfos = new List<ProtoBuffer.Models.CameraInfo>() {
                                 new ProtoBuffer.Models.CameraInfo(){
                                 cid=camera.Id,
                                 index=camera.Index,
                                 enable=camera.Enalbe,
                                 ip=camera.IP,
                                 nickname=camera.NickName
                                 }
                               },
                            did = embModel.IP
                        };
                        Task.Factory.StartNew(state =>
                        {
                            manager.DeveiceUpdate(emb, embModel.Id,ManagerHelp.ShipId);
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
