using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
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

       
        public IActionResult Index(bool isShow=false,string id="")
        {
            if (!string.IsNullOrEmpty(id))
            {
                ManagerHelp.ShipId = id;
                //陆地端过来不显示报警信息
                ManagerHelp.IsShowAlarm = false;
            }
            ViewBag.IsShowLayout = isShow;
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
        /// <summary>
        /// 陆地端查看设备信息
        /// </summary>
        /// <returns></returns>
        private IActionResult LandLoad()
        {
            string identity = Guid.NewGuid().ToString();
            var data=manager.DeviceQuery(identity);
            var result = new
            {
                code = 0,
                data = "",
                isSet = !string.IsNullOrEmpty(ManagerHelp.ShipId) ? ManagerHelp.IsSet : false
            };
            return new JsonResult(result);
        }
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Save(string strEmbed)
        {
            if (ModelState.IsValid)
            {
                Models.Embedded model = JsonConvert.DeserializeObject<Models.Embedded>(strEmbed);
                if (!string.IsNullOrEmpty(model.Id))
                {
                    #region 修改
                    var embedded =_context.Embedded.FirstOrDefault(c => c.Id == model.Id);
                    if (embedded==null)
                    {
                        return new JsonResult(new { code = 1, msg = "数据不存在" });
                    }
                    embedded.IP = model.IP;
                    embedded.Name = model.Name;
                    embedded.Nickname = model.Nickname;
                    embedded.Password = model.Password;
                    embedded.Port = model.Port; ;
                    embedded.Type = model.Type;
                    embedded.Factory = model.Factory;
                    Task.Factory.StartNew(state =>
                    {
                        ProtoBuffer.Models.Embedded emb = new ProtoBuffer.Models.Embedded()
                        {
                            ip = embedded.IP,
                            name = embedded.Name,
                            password = embedded.Password,
                            port = embedded.Port,
                            nickname = embedded.Nickname
                        };
                        int result = manager.DeveiceUpdate(emb, embedded.Did, embedded.Id);
                    }, TaskCreationOptions.LongRunning);
                    _context.Update(embedded);
                    #endregion
                }
                else
                {
                    #region 添加
                    string iditity = Guid.NewGuid().ToString();
                    ProtoBuffer.Models.Embedded emb = new ProtoBuffer.Models.Embedded()
                    {
                        ip = model.IP,
                        name = model.Name,
                        password = model.Password,
                        port = model.Port,
                        nickname = model.Nickname
                    };
                    model.Id = iditity;
                    model.ShipId = ManagerHelp.ShipId;

                    //测试数据
                    Random rd = new Random();
                    model.Did = rd.Next(1111, 9999).ToString();
                    model.CameraModelList = new List<Camera>() {
                        new Camera(){
                         Cid=rd.Next(111,999).ToString(),
                         EmbeddedId = iditity,
                         Enalbe = false,
                         Id = Guid.NewGuid().ToString(),
                         NickName ="摄像机1",
                         IP ="127.0.0.1",
                         ShipId = model.ShipId,
                         Index = 1
                        },
                         new Camera(){
                         Cid=rd.Next(111,999).ToString(),
                         EmbeddedId = iditity,
                         Enalbe = false,
                         Id = Guid.NewGuid().ToString(),
                         NickName ="摄像机2",
                         IP = "127.0.0.1",
                         ShipId = model.ShipId,
                         Index = 2
                        }
                    };
                    _context.Embedded.Add(model);

                    #region 发送注册设备请求并接收设备下的摄像机信息
                    //ProtoBuffer.Models.DeviceResponse rep = manager.DeveiceAdd(emb, iditity);
                    //if (rep != null && rep.result == 0)
                    //{
                    //    model.Did = rep.did;
                    //    if (rep.embedded.Count == 1 && rep.embedded[0].cameras.Count > 0)
                    //    {
                    //        model.CameraModelList = new List<Camera>();
                    //        var repList = rep.embedded[0].cameras;
                    //        foreach (var item in repList)
                    //        {
                    //            Camera cmodel = new Camera()
                    //            {
                    //                Cid = item.cid,
                    //                EmbeddedId = iditity,
                    //                Enalbe = item.enable,
                    //                Id = Guid.NewGuid().ToString(),
                    //                NickName = item.nickname,
                    //                IP = item.ip,
                    //                ShipId = model.ShipId,
                    //                Index = item.index
                    //            };
                    //            model.CameraModelList.Add(cmodel);
                    //        }
                    //    }
                    //_context.Embedded.Add(model);
                    //}
                    #endregion
                    #endregion
                }
                _context.SaveChangesAsync();
            }
            return new JsonResult(new { code = 0 });
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
    }
}
