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
using Org.BouncyCastle.Crypto.Tls;
using ShipWeb.DB;
using ShipWeb.Interface;
using ShipWeb.Models;
using ShipWeb.ProtoBuffer;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class DeviceController : BaseController
    {
        private readonly MyContext _context;
        private int timeout = 5000;//等待时间
        private ILogger<DeviceController> _logger;
        private SendDataMsg assembly = new SendDataMsg();
        public DeviceController(MyContext context, ILogger<DeviceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 设备界面显示
        /// </summary>
        /// <param name="isShow"></param>
        /// <param name="id">不为空时是陆地端跳转</param>
        /// <param name="shipName"></param>
        /// <returns></returns>
        public IActionResult Index(bool isShow = false, string id = "", string shipName = "")
        {
            if (!string.IsNullOrEmpty(id))
            {
                string browsertoken = HttpContext.Request.Cookies["token"];
                if (browsertoken != null)
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
                ManagerHelp.isInit = false;
                ViewBag.LoginName = base.user.Name;
                ViewBag.ShipName = base.user.ShipName;
                ViewBag.src = "/Device/Index";
                ViewBag.layuithis = "device";

                #region 缓存当前船下的组件信息，提供给设备，算法，船员，船状态操作
                assembly.SendComponentQuery(id);
                bool flag = true;
                new TaskFactory().StartNew(() => {
                    while (ManagerHelp.ComponentReponse == ""&& flag)
                    {
                        Thread.Sleep(100);
                    }
                    if (ManagerHelp.ComponentReponse != "")
                    {
                        ProtoBuffer.Models.ComponentResponse response = JsonConvert.DeserializeObject<ProtoBuffer.Models.ComponentResponse>(ManagerHelp.ComponentReponse);
                        ManagerHelp.ComponentReponse = "";
                        List<ComponentToken> tokens = new List<ComponentToken>();
                        if (response.result == 0 && response.componentinfos != null)
                        {
                            foreach (var item in response.componentinfos)
                            {
                                ComponentToken token = new ComponentToken()
                                {
                                    Id = item.componentid,
                                    //CommId = item.commid,
                                    Name = item.cname,
                                    Type = (ComponentType)item.type
                                };
                                tokens.Add(token);
                            }
                        }
                        string com = JsonConvert.SerializeObject(tokens);
                        HttpContext.Session.SetString("comtoken", com);
                    }
                }).Wait(timeout);
                flag = false;
                #endregion
            }
            ViewBag.IsLandHome = base.user.IsLandHome;
            ViewBag.IsShowLayout = isShow;
            ViewBag.IsSet = base.user.EnableConfigure;
            return View();
        }
        public IActionResult Load() 
        {
                if (base.user.IsLandHome)
                {
                    return LandLoad();
                }     
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
            List<Device> list = new List<Device>();
            string identity = base.user.ShipId;
            //发送查询设备请求
            assembly.SendDeveiceQuery(identity);
            bool flag = true;
            new TaskFactory().StartNew(() =>
            {
                while (flag && ManagerHelp.DeviceReponse == "")
                {
                    Thread.Sleep(500);
                }
            }).Wait(timeout);
            flag = false;
            if (ManagerHelp.DeviceReponse != "")
            {
                List<ProtoBuffer.Models.DeviceInfo> devices = JsonConvert.DeserializeObject<List<ProtoBuffer.Models.DeviceInfo>>(ManagerHelp.DeviceReponse);
                ManagerHelp.DeviceReponse = "";
                foreach (var item in devices)
                {
                    Device model = new Device()
                    {
                        Enable = item.enable,
                        factory = (Device.Factory)((int)item.factory),
                        Id = item.did,
                        IP = item.ip,
                        Name = item.name,
                        Nickname = item.nickname,
                        Password = item.password,
                        Port = item.port,
                        type = (Device.Type)((int)item.type)
                    };
                    var cam = item.camerainfos;
                    model.CameraModelList = new List<Camera>();
                    if (cam!=null)
                    {
                        foreach (var it in cam)
                        {
                            model.CameraModelList.Add(new Camera()
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
                    list.Add(model);
                }
            }
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
                string shipId = base.user.ShipId;
                int code = 1;
                string msg = "";

                if (shipId == "")
                {
                    return new JsonResult(new { code = 1, msg = "船不存在，无法添加数据" });
                }
                var model = JsonConvert.DeserializeObject<DeviceViewModel>(strEmbed);
                //陆地端远程添加设备
                if (base.user.IsLandHome)
                {
                        string identity = GetIdentity(model.Factory);
                        if (identity == null)
                        {
                            return new JsonResult(new { code = 1, msg = Enum.GetName(typeof(Device.Factory), Convert.ToInt32(model.Factory)) + "组件未启动" });
                        }
                        Device emb = GetProtoDevice(model);
                        if (!string.IsNullOrEmpty(model.Id))
                        {
                            assembly.SendDeveiceUpdate(emb, shipId + ":" + identity, model.Id);
                        }
                        else
                        {
                            assembly.SendDeveiceAdd(emb, shipId + ":" + identity);
                        }
                        code = GetResult();
                    
                }
                else
                {
                    Device device = new Device();
                    if (!string.IsNullOrEmpty(model.Id))
                    {
                        device = _context.Device.FirstOrDefault(c => c.Id == model.Id);
                        if (device == null)
                        {
                            return new JsonResult(new { code = 1, msg = "数据不存在" });
                        }
                    }
                    device.IP = model.IP;
                    device.Name = model.Name;
                    device.Nickname = model.Nickname;
                    device.Password = model.Password;
                    device.Port = model.Port;
                    device.type = (Device.Type)model.Type;
                    device.factory = (Device.Factory)model.Factory;
                    device.Enable = model.Enable;
                 
                        string identity = GetIdentity(model.Factory);
                        if (identity == "")
                        {
                            return new JsonResult(new { code = 1, msg = Enum.GetName(typeof(Device.Factory), Convert.ToInt32(device.factory)) + "组件未启动" });
                        }
                        SendDataMsg assembly = new SendDataMsg();
                        if (string.IsNullOrEmpty(model.Id))
                        {
                            device.Id = Guid.NewGuid().ToString();
                            device.ShipId = base.user.ShipId;
                            _context.Device.Add(device);
                            _context.SaveChanges();
                            model.Id = device.Id;
                            Device emb = GetProtoDevice(model);
                            assembly.SendDeveiceAdd(emb, identity);
                        }
                        else
                        {
                            _context.Device.Update(device);
                            _context.SaveChanges();
                            Device emb = GetProtoDevice(model);
                            assembly.SendDeveiceUpdate(emb, identity, device.Id);
                        }
                        code = GetResult();
                    
                }

                if (code == 400) msg = "请求超时。。。";
                else if (code != 0) msg = "请输入正确的设备参数";
                return new JsonResult(new { code = code, msg = msg });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存设备信息异常", strEmbed);
                return new JsonResult(new { code = 1, msg = "数据保存失败!" + ex.Message });
            }
        }

        public IActionResult CamSave(string id, string did, int factory, string nickName, string enable)
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
                        string shipId = base.user.ShipId;
                        string identity = GetIdentity(factory);
                        if (identity=="")
                        {
                            return new JsonResult(new { code = 1, msg = Enum.GetName(typeof(Device.Factory), Convert.ToInt32(factory)) + "组件未启动" });
                        }
                        Device emb = new Device
                        {
                            Id = did,
                            CameraModelList = new List<Camera>() {
                                new Camera() {
                                    NickName = nickName,
                                    Enable = enable == "1" ? true : false,
                                    Id = id
                                }
                            }
                        };
                        assembly.SendDeveiceUpdate(emb, shipId+":"+identity, did);
                        code = GetResult();
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
                            //获取设备的组件ID
                            string identity = GetIdentity(factory);

                            Device emb = new Device
                            {
                                Id = did,
                                CameraModelList = new List<Camera>()
                                {
                                    new Camera() {
                                        NickName = nickName,
                                        Enable = enable == "1" ? true : false,
                                        Id = id
                                    }
                                }
                            };
                            _context.Update(camera);
                            _context.SaveChangesAsync();
                            assembly.SendDeveiceUpdate(emb, identity, embModel.Id);
                            code = GetResult();

                        };
                    }
                    if (code == 2) msg = "请求超时。。。";
                    else if (code != 0) msg = "配置摄像机信息失败";
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
        private ShipWeb.Models.Device GetProtoDevice(DeviceViewModel model) 
        {
            Device device = new Device();
            device.IP = model.IP;
            device.Name = model.Name;
            device.Nickname = model.Nickname;
            device.Password = model.Password;
            device.Port = model.Port;
            device.type = (Device.Type)model.Type;
            device.factory = (Device.Factory)model.Factory;
            device.Enable = model.Enable;
            return device;
        }
        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Delete(string id,int factory)
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
                    string shipId = base.user.ShipId;
                    //获取设备的组件ID
                    string identity = GetIdentity(factory);
                    if (identity == "")
                    {
                        return new JsonResult(new { code = 1, msg = Enum.GetName(typeof(Device.Factory), Convert.ToInt32(factory)) + "组件未启动" });
                    }
                   assembly.SendDeveiceDelete(shipId + ":"+identity, id);
                   code = GetResult();             
                }
                else
                {
                    var device = _context.Device.Find(id);
                    if (device == null)
                    {
                        return NotFound();
                    }
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
                        //获取设备的组件ID
                        string identity = GetIdentity(factory);
                        if (identity == "")
                        {
                            return new JsonResult(new { code = 1, msg = Enum.GetName(typeof(Device.Factory), Convert.ToInt32(factory)) + "组件未启动" });
                        }
                        SendDataMsg assembly = new SendDataMsg();
                        assembly.SendDeveiceDelete(identity, device.Id);
                        if (GetResult()==0)
                        {
                            _context.SaveChanges();
                            code = 0;
                        }
                   
                }

                if (code == 2) msg = "请求超时。。。";
                else if (code != 0) msg = "请配置正确的设备参数";
                return new JsonResult(new { code = code, msg = msg });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除设备异常", id);
                return new JsonResult(new { code = 1, msg = "删除失败!" + ex.Message });
            }

        }

        /// <summary>
        /// 获取所添加设备的通讯ID
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        private string GetIdentity(int factory)
        {
            var type = ManagerHelp.GetComponentType((int)factory);
            if (base.user.IsLandHome)
            {
                string tokenstr = HttpContext.Session.GetString("comtoken");
                if (string.IsNullOrEmpty(tokenstr)) return "";
                List<ComponentToken> tokens = JsonConvert.DeserializeObject<List<ComponentToken>>(tokenstr);
                var component = tokens.FirstOrDefault(c => c.Type == type);
                if (component!=null)
                {
                    return component.CommId;
                }
            }
            else
            {
                //获取设备的组件ID
                var component = _context.Component.FirstOrDefault(c => c.Type == type&&c.Line==0);
                if (component!=null)
                {
                    return component.Id;
                }
            }
            return "";
        }

        /// <summary>
        /// 取返回结果
        /// </summary>
        /// <returns></returns>
        private int GetResult()
        {
            int result = 1;
            bool flag = true;
            new TaskFactory().StartNew(() => {
                while (ManagerHelp.DeviceReponse==""&&flag)
                {
                    Thread.Sleep(100);
                }
            }).Wait(timeout);
            flag = false;
            if (!string.IsNullOrEmpty(ManagerHelp.DeviceReponse))
            {
                result = Convert.ToInt32(ManagerHelp.DeviceReponse);
            }
            else
            {
                result = 400;//请求超时
            }
            ManagerHelp.DeviceReponse = "";
            return result;
        }
    }
}
