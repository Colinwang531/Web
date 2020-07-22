using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class DeviceController : BaseController
    {
        private readonly MyContext _context;
        private ProtoBuffer.ProtoManager manager;
        public DeviceController(MyContext context)
        {
            _context = context;
            manager = new ProtoBuffer.ProtoManager();
        }

       
        public IActionResult Index(bool isShow=false,string id="")
        {
            if (!string.IsNullOrEmpty(id))
            {
                ManagerHelp.ShipId = id;
                //陆地端过来不显示报警信息
                ManagerHelp.IsShowAlarm = false;
                ViewBag.LoginName = base.user.Name;
            }
            ViewBag.IsShowLayout = isShow;
            ViewBag.IsSet = base.user.EnableConfigure;
            return View();
        }
        public IActionResult Load()
        {
            if (ManagerHelp.IsShowLandHome)
            {
                return LandLoad();
            }

            var data = _context.Device.Where(c => c.ShipId == ManagerHelp.ShipId).ToList();
            var result = new
            {
                code = 0,
                data = data,
                isSet = !string.IsNullOrEmpty(ManagerHelp.ShipId) ?base.user.EnableConfigure : false
            };
            return new JsonResult(result);
        }
        /// <summary>
        /// 陆地端查看设备信息
        /// </summary>
        /// <returns></returns>
        private IActionResult LandLoad()
        {
            var protoModel=manager.DeviceQuery(ManagerHelp.ShipId);
            var data = from a in protoModel
                       select new
                       {
                           a.ip,
                           a.name,
                           a.nickname,
                           a.password,
                           a.port,
                           a.type,
                           id=a.did,
                           a.factory
                       };
            var result = new
            {
                code = 0,
                data = data,
                isSet = !string.IsNullOrEmpty(ManagerHelp.ShipId) ?base.user.EnableConfigure : false
            };
            return new JsonResult(result);
        }
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Save(string strEmbed)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var model = JsonConvert.DeserializeObject<DeviceViewModel>(strEmbed);
                    //陆地端远程添加设备
                    if (ManagerHelp.IsShowLandHome)
                    {
                        ProtoBuffer.Models.DeviceInfo emb = GetProtoDevice(model);
                        int code = 1;
                        if (!string.IsNullOrEmpty(model.Id))
                        {
                            code = manager.DeveiceUpdate(emb, model.Id, ManagerHelp.ShipId);
                        }
                        else
                        {
                            emb.camerainfos = new List<ProtoBuffer.Models.CameraInfo>() {
                             new ProtoBuffer.Models.CameraInfo(){
                              cid=Guid.NewGuid().ToString(),
                              enable=false,
                              index=11,
                              ip="168.154.0.13",
                              nickname="摄像机UU"
                             }
                        };
                            var result = manager.DeveiceAdd(emb, ManagerHelp.ShipId);
                            code = result.result;
                        }
                        return new JsonResult(new { code = code, msg = code == 0 ? "数据保存成功" : "数据保存失败" });
                    }
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
                        device.Enable = model.Enable == 1 ? true : false;
                        Task.Factory.StartNew(state =>
                        {
                            ProtoBuffer.Models.DeviceInfo emb = GetProtoDevice(model);
                            int result = manager.DeveiceUpdate(emb, device.Id, ManagerHelp.ShipId);
                        }, TaskCreationOptions.LongRunning);
                        _context.Device.Update(device);
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
                            ShipId = ManagerHelp.ShipId,
                            Enable = model.Enable == 0 ? true : false
                        };
                        //测试数据
                        device.CameraModelList = new List<Camera>() {
                        new Camera(){
                         DeviceId = iditity,
                         Enalbe = false,
                         Id = Guid.NewGuid().ToString(),
                         NickName ="摄像机1",
                         IP ="127.0.0.1",
                         ShipId = ManagerHelp.ShipId,
                         Index = 1
                        },
                        new Camera(){
                         DeviceId = iditity,
                         Enalbe = false,
                         Id = Guid.NewGuid().ToString(),
                         NickName ="摄像机2",
                         IP = "127.0.0.1",
                         ShipId = ManagerHelp.ShipId,
                         Index = 2
                        }
                    };
                        _context.Device.Add(device);
                        #region 发送注册设备请求并接收设备下的摄像机信息                    
                        //ProtoBuffer.Models.DeviceInfo emb = GetProtoDevice(model);
                        //ProtoBuffer.Models.DeviceResponse rep = manager.DeveiceAdd(emb, iditity);
                        //if (rep != null && rep.result == 0)
                        //{
                        //    if (rep.embedded.Count == 1 && rep.embedded[0].camerainfos.Count > 0)
                        //    {
                        //        device.CameraModelList = new List<Camera>();
                        //        var repList = rep.embedded[0].camerainfos;
                        //        int count = 0;
                        //        foreach (var item in repList)
                        //        {
                        //            count++;
                        //            Camera cmodel = new Camera()
                        //            {
                        //                Enalbe = item.enable,
                        //                Id = Guid.NewGuid().ToString(),
                        //                NickName = string.IsNullOrEmpty(item.nickname) ? "摄像机" + count : item.nickname,
                        //                IP = item.ip,
                        //                ShipId = model.ShipId,
                        //                Index = item.index,
                        //                DeviceId = device.Id
                        //            };
                        //            device.CameraModelList.Add(cmodel);
                        //        }
                        //    }
                        //    _context.Device.Add(device);
                        //}
                        #endregion
                        #endregion
                    }
                    _context.SaveChangesAsync();
                }
                return new JsonResult(new { code = 0 });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1,msg="数据保存失败!"+ex.Message });
            }
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
                did=model.Id
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
                //陆地端删除设备
                if (ManagerHelp.IsShowLandHome)
                {
                    int result = manager.DeveiceDelete(id, ManagerHelp.ShipId);
                    if (result != 0)
                    {
                        return new JsonResult(new { code = 1, msg = "删除失败!" });
                    }
                    return new JsonResult(new { code = 0 });
                }
                if (id == null)
                {
                    return NotFound();
                }
                var Embedded = _context.Device.Find(id);
                if (Embedded == null)
                {
                    return NotFound();
                }
                //先删除服务器上的再删除本地的
                //int resutl = manager.DeveiceDelete(Embedded.Id, ManagerHelp.ShipId);
                //if (resutl == 0)
                //{
                    var cameras = _context.Camera.Where(c => c.DeviceId == Embedded.Id).ToList();
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
                    _context.Device.Remove(Embedded);
                    _context.SaveChanges();
                //}
                return new JsonResult(new { code = 0, msg = "删除成功!" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "删除失败!" + ex.Message });
            }

        }
    }
}
