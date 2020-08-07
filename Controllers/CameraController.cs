using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private int timeout = 5000;//请求超时时间 单位秒
        private ILogger<CameraController> _logger;
        public CameraController(MyContext context, ILogger<CameraController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Cameras
        public IActionResult Index(string id)
        {
            ViewBag.id = id.Trim();
            ViewBag.IsSet = base.user.EnableConfigure;
            ViewBag.IsLandHome = base.user.IsLandHome;
            return View();
        }
        public IActionResult Load(string id)
        {
            if (base.user.IsLandHome)
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
            var emdList=manager.DeviceQuery(base.user.ShipId, did);
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
        public IActionResult Save(string id,string did,string nickName,string enable)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int code = 1;
                    string msg = "";
                    //陆地端远程修改摄像机信息
                    if (base.user.IsLandHome)
                    {
                        ProtoBuffer.Models.DeviceInfo emb = new ProtoBuffer.Models.DeviceInfo()
                        {
                            camerainfos = new List<ProtoBuffer.Models.CameraInfo>() {
                                 new ProtoBuffer.Models.CameraInfo(){
                                 cid=id,
                                 enable=enable == "1" ? true : false,
                                 nickname=nickName
                                 }
                               },
                            did = did
                        };
                        new TaskFactory().StartNew(() => {
                            var res = manager.DeveiceUpdate(emb, did, base.user.ShipId);
                            code = res;
                            msg = code == 1 ? "数据修改失败" : "";
                        }).Wait(timeout);
                    }
                    else
                    {
                        Camera camera = _context.Camera.FirstOrDefault(c => c.Id == id);
                        if (camera == null)
                        {
                            return new JsonResult(new { code = 1, msg = "数据不存在" });
                        }
                        camera.NickName = nickName;
                        camera.Enable = enable == "1" ? true : false;
                        var embModel = _context.Device.FirstOrDefault(e => e.Id == camera.DeviceId);
                        if (embModel != null)
                        {
                            ProtoBuffer.Models.DeviceInfo emb = new ProtoBuffer.Models.DeviceInfo()
                            {
                                camerainfos = new List<ProtoBuffer.Models.CameraInfo>() {
                                 new ProtoBuffer.Models.CameraInfo(){
                                 cid=camera.Id,
                                 index=camera.Index,
                                 enable=camera.Enable,
                                 ip=camera.IP,
                                 nickname=camera.NickName
                                 }
                               },
                                did = embModel.IP
                            };
                            new TaskFactory().StartNew(() =>
                            {
                                int result = manager.DeveiceUpdate(emb, embModel.Id, base.user.ShipId);
                                if (result == 0)
                                {
                                    _context.Update(camera);
                                    _context.SaveChangesAsync();
                                    code = 0;
                                }
                                else
                                {
                                    msg = "数据修改失败";
                                }
                            }).Wait(timeout);
                        };
                    }
                    msg = (code == 1 && msg == "") ? "请求超时。。。" : msg;
                    return new JsonResult(new { code = code, msg = msg });
                }
                catch (Exception ex)
                {
                    _logger.LogError("修改摄像机信息失败", "Save(" + id + "," + did + "," + nickName + "," + enable + ")");
                    return new JsonResult(new { code = 1, msg = "数据保存失败！" + ex.Message });
                }

            }
            return new JsonResult(new { code = 0 });
        }


    }
}
