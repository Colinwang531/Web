using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Cms;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class DeviceController : BaseController
    {
        private readonly MyContext _context;
        private ProtoBuffer.ProtoManager manager=new ProtoBuffer.ProtoManager();
        private int timeout = 5000;//等待时间
        private ILogger<DeviceController> _logger;
        public DeviceController(MyContext context, ILogger<DeviceController> logger)
        {
            _context = context;
            _logger = logger;
        }

       
        public IActionResult Index(bool isShow=false,string id="",string shipName="")
        {
            if (!string.IsNullOrEmpty(id))
            {
                string browsertoken = HttpContext.Request.Cookies["token"];
                if (browsertoken!=null)
                {
                    string urlstr = HttpContext.Session.GetString(browsertoken);
                    user = JsonConvert.DeserializeObject<UserToken>(urlstr);
                    user.ShipId = id;
                    user.IsLandHome = true;
                    user.ShipName = shipName;
                    string userStr = JsonConvert.SerializeObject(user);
                    //将请求的url注册
                    HttpContext.Session.SetString(browsertoken, userStr);
                    //写入浏览器token
                    HttpContext.Response.Cookies.Append("token", browsertoken);
                }               
                //陆地端过来不显示报警信息
                ManagerHelp.IsShowAlarm = false;
                ViewBag.LoginName = base.user.Name;
                ViewBag.ShipName = base.user.ShipName; 
                ViewBag.src = "/Device/Index";
                ViewBag.layuithis = "device";
            }
            ViewBag.IsLandHome = base.user.IsLandHome;
            ViewBag.IsShowLayout = isShow;
            ViewBag.IsSet = base.user.EnableConfigure;
            return View();
        }
        public IActionResult Index2(bool isShow = false, string id = "") {
            if (!string.IsNullOrEmpty(id))
            {
                string browsertoken = HttpContext.Request.Cookies["token"];
                if (browsertoken != null)
                {
                    string urlstr = HttpContext.Session.GetString(browsertoken);
                    user = JsonConvert.DeserializeObject<UserToken>(urlstr);
                    user.ShipId = id;
                    user.IsLandHome = true;
                    string userStr = JsonConvert.SerializeObject(user);
                    //将请求的url注册
                    HttpContext.Session.SetString(browsertoken, userStr);
                    //写入浏览器token
                    HttpContext.Response.Cookies.Append("token", browsertoken);
                }
                //陆地端过来不显示报警信息
                ManagerHelp.IsShowAlarm = false;
                ViewBag.LoginName = base.user.Name;
                ViewBag.src = "/Device/Index";
            }
            ViewBag.IsLandHome = base.user.IsLandHome;
            ViewBag.IsShowLayout = isShow;
            ViewBag.IsSet = base.user.EnableConfigure;
            return View();
        }
        public IActionResult Load2()
        {
            
            if (base.user.IsLandHome )
            {
                return LandLoad();
            }
            string shipId = base.user.ShipId;
            var data = _context.Device.Where(c => c.ShipId == shipId).ToList();           
            var result = new
            {
                code = 0,
                data = data,
                isSet = !string.IsNullOrEmpty(shipId) ?base.user.EnableConfigure : false
            };
            return new JsonResult(result);
        }
        public IActionResult Load() 
        {
            string shipId = base.user.ShipId;
            var data = _context.Device.Where(c => c.ShipId == shipId).ToList(); 
            var ids = string.Join(',', data.Select(c => c.Id));
            //查询设备下的摄像机
            var cameras = _context.Camera.Where(c => c.ShipId == shipId && ids.Contains(c.DeviceId)).ToList();
            foreach (var item in data)
            {
                var cams = cameras.Where(c => c.DeviceId == item.Id);
                item.CameraModelList = new List<Camera>();
                foreach (var it in cams)
                {
                    Camera cam = new Camera()
                    {
                        DeviceId = item.Id,
                        Index=it.Index,
                        Id = it.Id,
                        Enable = it.Enable,
                        IP = it.IP,
                        NickName = it.NickName,
                        ShipId = it.ShipId
                    };
                    item.CameraModelList.Add(cam);
                }
            }
            var result = new
            {
                code = 0,
                data = data,
                isSet = !string.IsNullOrEmpty(shipId) ? base.user.EnableConfigure : false
            };
            return new JsonResult(result);
        }
        /// <summary>
        /// 陆地端查看设备信息
        /// </summary>
        /// <returns></returns>
        private IActionResult LandLoad()
        {
            List<DeviceViewModel> list = new List<DeviceViewModel>();
            new TaskFactory().StartNew(() =>
            {
                var protoModel = manager.DeviceQuery(base.user.ShipId);
                foreach (var item in protoModel)
                {
                    DeviceViewModel model = new DeviceViewModel()
                    {
                        Enable = item.enable,
                        Factory = (int)item.factory,
                        Id = item.did,
                        IP = item.ip,
                        Name = item.name,
                        Nickname = item.nickname,
                        Password = item.password,
                        Port = item.port,
                        Type = (int)item.type
                    };
                    var cam = item.camerainfos;
                    model.cameraViews = new List<CameraViewModel>();
                    foreach (var it in cam)
                    {
                        model.cameraViews.Add(new CameraViewModel()
                        {
                            Index = it.index,
                            Id = it.cid,
                            Enable = it.enable,
                            IP = it.ip,
                            NickName = it.nickname,
                            DeviceId = item.did
                        });
                    }
                }
            }).Wait(timeout);
            var result = new
            {
                code = 0,
                data = list,
                isSet = !string.IsNullOrEmpty(base.user.ShipId) ? base.user.EnableConfigure : false
            };
            return new JsonResult(result);
        }
        
        public IActionResult Save(string strEmbed)
        {
            try
            {
                int code = 1;
                string msg = "";
                if (ModelState.IsValid)
                {
                    if (base.user.ShipId == "")
                    {
                        return new JsonResult(new { code = 1, msg = "船不存在，无法添加数据" });
                    }
                    var model = JsonConvert.DeserializeObject<DeviceViewModel>(strEmbed);
                    //陆地端远程添加设备
                    if (base.user.IsLandHome)
                    {
                        ProtoBuffer.Models.DeviceInfo emb = GetProtoDevice(model);
                        new TaskFactory().StartNew(() =>
                        {
                            if (!string.IsNullOrEmpty(model.Id))
                            {
                                code = manager.DeveiceUpdate(emb, model.Id, base.user.ShipId);
                            }
                            else
                            {
                            emb.camerainfos = new List<ProtoBuffer.Models.CameraInfo>()
                                {
                                   new ProtoBuffer.Models.CameraInfo()
                                   {
                                      cid=Guid.NewGuid().ToString(),
                                      enable=false,
                                      index=11,
                                      ip="168.154.0.13",
                                      nickname="摄像机UU"
                                   }
                                 };
                            var result = manager.DeveiceAdd(emb, base.user.ShipId);
                                code = result.result;
                            }
                            msg = code == 0 ? "" : "数据保存失败";

                        }).Wait(timeout);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(model.Id))
                        {
                            #region 修改
                            var device = _context.Device.FirstOrDefault(c => c.Id == model.Id);
                            if (device == null)
                            {
                                return new JsonResult(new { code = 1, msg = "数据不存在" });
                            }
                            device.IP = model.IP;
                            device.Name = model.Name;
                            device.Nickname = model.Nickname;
                            device.Password = model.Password;
                            device.Port = model.Port;
                            device.type = (Device.Type)model.Type;
                            device.factory = (Device.Factory)model.Factory;
                            device.Enable = model.Enable;
                            //测试数据 
                            _context.Device.Update(device);
                            _context.SaveChanges();
                            code = 0;
                            //new System.Threading.Tasks.TaskFactory().StartNew(() => {
                            //    ProtoBuffer.Models.DeviceInfo emb = GetProtoDevice(model);
                            //    int result = manager.DeveiceUpdate(emb, device.Id, base.user.ShipId);
                            //    if (result == 0)
                            //    {
                            //        _context.Device.Update(device);
                            //        _context.SaveChanges();
                            //    }
                            //    code = result;
                            //    msg = result != 0 ? "数据修改失败!" : "";
                            //}).Wait(timeout);
                            #endregion
                        }
                        else
                        {
                            #region 添加
                            string iditity = Guid.NewGuid().ToString();
                            Device device = new Device()
                            {
                                IP = model.IP,
                                Name = model.Name,
                                Nickname = model.Nickname,
                                Password = model.Password,
                                Port = model.Port,
                                type = (Device.Type)model.Type,
                                factory = (Device.Factory)model.Factory,
                                Id = iditity,
                                ShipId = base.user.ShipId,
                                Enable = model.Enable
                            };
                            //测试数据
                            device.CameraModelList = new List<Camera>() {
                            new Camera(){
                             DeviceId = iditity,
                             Enable = false,
                             Id = Guid.NewGuid().ToString(),
                             NickName ="摄像机1",
                             IP ="127.0.0.1",
                             ShipId = base.user.ShipId,
                             Index = 1
                            },
                            new Camera(){
                             DeviceId = iditity,
                             Enable = false,
                             Id = Guid.NewGuid().ToString(),
                             NickName ="摄像机2",
                             IP = "127.0.0.1",
                             ShipId = base.user.ShipId,
                             Index = 2
                            }
                            };
                            _context.Device.Add(device);
                            _context.SaveChanges();
                            code = 0;
                            #region 发送注册设备请求并接收设备下的摄像机信息                    
                            //ProtoBuffer.Models.DeviceInfo emb = GetProtoDevice(model);
                            //emb.did = device.Id;
                            //new System.Threading.Tasks.TaskFactory().StartNew(() => {
                            //    ProtoBuffer.Models.DeviceResponse rep = manager.DeveiceAdd(emb, iditity);
                            //    if (rep != null && rep.result == 0)
                            //    {
                            //        if (rep.deviceinfos.Count == 1 && rep.deviceinfos[0].camerainfos.Count > 0)
                            //        {
                            //            device.CameraModelList = new List<Camera>();
                            //            var repList = rep.deviceinfos[0].camerainfos;
                            //            int count = 0;
                            //            foreach (var item in repList)
                            //            {
                            //                count++;
                            //                Camera cmodel = new Camera()
                            //                {
                            //                    Enable = item.enable,
                            //                    Id = Guid.NewGuid().ToString(),
                            //                    NickName = string.IsNullOrEmpty(item.nickname) ? "摄像机" + count : item.nickname,
                            //                    IP = item.ip,
                            //                    ShipId = model.ShipId,
                            //                    Index = item.index,
                            //                    DeviceId = device.Id
                            //                };
                            //                device.CameraModelList.Add(cmodel);
                            //            }
                            //        }
                            //        _context.Device.Add(device);
                            //        _context.SaveChangesAsync();
                            //        code = 0;
                            //    }
                            //    else
                            //    {
                            //        msg = "数据修改失败!";
                            //    }
                            //}).Wait(timeout);
                            #endregion
                            #endregion
                        }
                    }                   
                }
                msg = (code == 1 && msg == "") ? "请求超时。。。" : msg;
                return new JsonResult(new { code = code, msg = msg });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存设备信息异常", strEmbed);
                return new JsonResult(new { code = 1, msg = "数据保存失败!" + ex.Message });
            }
        }

        public IActionResult CamSave(string id, string did, string nickName, string enable)
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
                            _context.Update(camera);
                            _context.SaveChangesAsync();
                            code = 0;

                            //new TaskFactory().StartNew(() =>
                            //{
                            //    int result = manager.DeveiceUpdate(emb, embModel.Id, base.user.ShipId);
                            //    if (result == 0)
                            //    {
                            //        _context.Update(camera);
                            //        _context.SaveChangesAsync();
                            //        code = 0;
                            //    }
                            //    else
                            //    {
                            //        msg = "数据修改失败";
                            //    }
                            //}).Wait(timeout);
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
        /// <summary>
        /// 得到protoBuf消息实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private ProtoBuffer.Models.DeviceInfo GetProtoDevice(DeviceViewModel model) 
        {
            ProtoBuffer.Models.DeviceInfo emb = new ProtoBuffer.Models.DeviceInfo()
            {
                ip = model.IP,
                name = model.Name,
                password = model.Password,
                port = model.Port,
                nickname = model.Nickname,
                factory = (ProtoBuffer.Models.DeviceInfo.Factory)model.Factory,
                type = (ProtoBuffer.Models.DeviceInfo.Type)model.Type,
                enable = model.Enable,
                did = model.Id
            };
            return emb;
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
                int code = 1;
                string msg = "";
                //陆地端删除设备
                if (base.user.IsLandHome)
                {
                    new TaskFactory().StartNew(() => {
                        code = manager.DeveiceDelete(id, base.user.ShipId);
                        msg = code != 0 ? "删除数据失败" : "";
                    }).Wait(timeout);                   
                }
                else
                {
                    var device = _context.Device.Find(id);
                    if (device == null)
                    {
                        return NotFound();
                    }
                    //先删除服务器上的再删除本地的
                    //new TaskFactory().StartNew(() =>
                    //{
                    //    int resutl = manager.DeveiceDelete(device.Id, base.user.ShipId);
                    //    if (resutl == 0)
                    //    {
                            var cameras = _context.Camera.Where(c => c.DeviceId == device.Id).ToList();
                            var cids = string.Join(',', cameras.Select(c => c.Id));
                            if (cids != "")
                            {
                                var cameraConfig = _context.Algorithm.Where(c => cids.Contains(c.Id));
                                if (cameraConfig.Count() > 0)
                                {
                                    //删除摄像机配置表
                                    _context.Algorithm.RemoveRange(cameraConfig);
                                }
                                //删除摄像机表
                                _context.Camera.RemoveRange(cameras);
                            }
                            //删除设备表
                            _context.Device.Remove(device);
                            _context.SaveChanges();
                            code = 0;
                    //    }
                    //    msg = code != 0 ? "删除数据失败" : "";
                    //}).Wait(timeout);
                }
                msg = (code == 1 && msg == "") ? "请求超时。。。" : msg;
                return new JsonResult(new { code = code, msg = msg });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除设备异常", id);
                return new JsonResult(new { code = 1, msg = "删除失败!" + ex.Message });
            }

        }
    }
}
