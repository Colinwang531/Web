using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Cms;
using ShipWeb.DB;
using ShipWeb.Interface;
using ShipWeb.Models;
using ShipWeb.ProtoBuffer;
using ShipWeb.Tool;
using Microsoft.AspNetCore.Http;

namespace ShipWeb.Controllers
{
    public class AlgorithmController : BaseController
    {
        private MyContext _context;
        private int timeout = 5000;//超时时间
        SendDataMsg assembly = new SendDataMsg();
        private ILogger<AlgorithmController> _logger;
        //缓存船舶端的设备信息
        List<ProtoBuffer.Models.DeviceInfo> boatDevices = new List<ProtoBuffer.Models.DeviceInfo>();
        List<Camera> cameras = new List<Camera>();
        public AlgorithmController(MyContext context, ILogger<AlgorithmController> logger) 
        {
            _context = context;
            _logger = logger;
        }
        public IActionResult Index()
        {
            ViewBag.IsSet = base.user.EnableConfigure;
            ViewBag.IsLandHome = base.user.IsLandHome;
            return View();
        }
        public IActionResult Load() 
        {
            if (!ManagerHelp.IsTest)
            {
                if (base.user.IsLandHome)
                {
                    return LandLoad();
                }
            }           
            string shipId = base.user.ShipId;
            var algor = _context.Algorithm.Where(c=>c.ShipId==shipId).ToList();
            var camera = _context.Camera.Where(c => c.ShipId == shipId).ToList();
            var data = from a in algor
                       join b in camera on a.Cid equals b.Id
                       select new
                       {
                           a.Id,
                           a.Type,
                           a.GPU,
                           a.Similar,
                           a.Cid,
                           b.NickName,
                           a.DectectFirst,
                           a.DectectSecond,
                           a.Track
                       };
            var result = new
            {
                code = 0,
                data = data,
                camera=camera
            };
            return new JsonResult(result);
        }
        public IActionResult LandLoad()
        {
            string shipId = base.user.ShipId;
            var comtent = _context.Component.FirstOrDefault(c => c.ShipId == shipId&&c.Type==ComponentType.WEB);
            List<ProtoBuffer.Models.AlgorithmInfo> protoDate = new List<ProtoBuffer.Models.AlgorithmInfo>();
            assembly.SendAlgorithmQuery(comtent.Id);
            try
            {
                bool flag = true;
                new TaskFactory().StartNew(() =>
                {
                    while (flag)
                    {
                        if (ManagerHelp.Reponse != "")
                        {
                            protoDate = JsonConvert.DeserializeObject<List<ProtoBuffer.Models.AlgorithmInfo>>(ManagerHelp.Reponse);
                            flag = false;
                        }
                    }
                }).Wait(timeout);
                flag = false;
            }
            catch (Exception)
            {
            }
            var data = from a in protoDate
                       select new
                       {
                           id = a.aid.Split(',')[0],
                           cid = a.cid,
                           Type = a.type,
                           GPU = a.gpu,
                           Similar = a.similar,
                           NickName = a.cid.Split(',')[1],
                           DectectFirst = a.dectectfirst,
                           DectectSecond = a.dectectsecond,
                           Track = a.track
                       };
            assembly.SendDeveiceQuery(comtent.Id);
            
            try
            {
                bool flag = true;
                new TaskFactory().StartNew(() =>
                {
                    while (flag)
                    {
                        if (ManagerHelp.Reponse != "")
                        {
                            boatDevices = JsonConvert.DeserializeObject<List<ProtoBuffer.Models.DeviceInfo>>(ManagerHelp.Reponse);
                            flag = false;
                        }
                        Thread.Sleep(500);
                    }
                }).Wait(timeout);
                flag = false;
            }
            catch (Exception)
            {
            }
            ManagerHelp.Reponse = "";
            foreach (var item in boatDevices)
            {
                var camList = item.camerainfos;
                foreach (var cam in camList)
                {
                    Camera model = new Camera()
                    {
                        Id = cam.cid,
                        NickName = cam.nickname
                    };
                    cameras.Add(model);
                }
            }
            var result = new
            {
                code = 0,
                data = data,
                camera = cameras
            };
            return new JsonResult(result);
        }
        /// <summary>
        /// 保存算法
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Save(string model)
        {
            try
            {
                string shipId = base.user.ShipId;
                if (shipId == "")
                {
                    return new JsonResult(new { code = 1, msg = "船不存在，无法添加数据" });
                }
                var viewModel = JsonConvert.DeserializeObject<AlgorithmViewModel>(model);
                if (viewModel != null)
                {
                    if (base.user.IsLandHome&&!ManagerHelp.IsTest)
                    {
                        string identity = GetIdentity(viewModel.Type);
                        if (identity == null)
                        {
                            string name = GetViewName((AlgorithmType)viewModel.Type);
                            return new JsonResult(new { code = 1, msg = "算法【" + name + "】组件未启动" });
                        }
                        var cam = cameras.FirstOrDefault(c => c.Id == viewModel.Cid);
                        string cid = cam.DeviceId + ":" + cam.Id + ":" + cam.Index;
                        ProtoBuffer.Models.AlgorithmInfo algorithm = GetProtoAlgorithm(viewModel, cid);
                        if (SendData(algorithm,(shipId + ":" + identity)))
                        {
                            _context.SaveChanges();
                            return new JsonResult(new { code = 0 });
                        }
                        else
                        {
                            return new JsonResult(new { code = 1, msg = "网络连接超时!" });
                        }
                    }
                    else
                    {
                        Algorithm algo = new Algorithm();
                        string msg = "";
                        if (!DataCheck(viewModel,ref algo,ref msg))
                        {
                            return new JsonResult(new { code = 1, msg = msg });
                        }
                        if (!ManagerHelp.IsTest)
                        {
                            //获取枚举对应的名称
                            string identity = GetIdentity(viewModel.Type);
                            if (identity == null)
                            {
                                string name = GetViewName((AlgorithmType)viewModel.Type);
                                return new JsonResult(new { code = 1, msg = "算法【"+name+"】组件未启动" });
                            }
                            algo.GPU = viewModel.GPU;
                            algo.Type = (AlgorithmType)viewModel.Type;
                            algo.Similar = viewModel.Similar;
                            algo.Cid = viewModel.Cid;
                            algo.DectectFirst = viewModel.DectectFirst;
                            algo.DectectSecond = viewModel.DectectSecond;
                            algo.Track = viewModel.Track;
                            algo.ShipId = base.user.ShipId;
                            if (!string.IsNullOrEmpty(viewModel.Id))
                            {
                                algo.Id = viewModel.Id;
                                _context.Algorithm.Update(algo);
                            }
                            else
                            {
                                algo.Id = Guid.NewGuid().ToString();
                                _context.Algorithm.Add(algo);
                            }
                            viewModel.Id = algo.Id;
                            var camera = _context.Camera.FirstOrDefault(c => c.Id == viewModel.Cid);
                            string cid = camera.DeviceId + ":" + camera.Id + ":" + camera.Index;
                            ProtoBuffer.Models.AlgorithmInfo algorithm = GetProtoAlgorithm(viewModel,cid);
                            if (SendData(algorithm, identity))
                            {
                                _context.SaveChanges();
                                #region 发送二次请求 暂时不用
                                ////根据摄像机获取设备下的通讯ID
                                //var factory = _context.Device.FirstOrDefault(c => c.Id == (_context.Camera.FirstOrDefault(d => d.Id == viewModel.Cid).DeviceId)).factory;
                                //compent = _context.Component.FirstOrDefault(c => c.Type == (factory == Device.Factory.HIKVISION ? ComponentType.HKD : ComponentType.DHD));
                                //if (compent == null)
                                //{
                                //    //查询算法组件并入库
                                //    assembly.SendComponentQuery();
                                //    return new JsonResult(new { code = 1, msg = "算法里摄像机对应的设备组件未启动" });
                                //}
                                //if (SendData(algorithm, compent.Id)) {
                                //    _context.SaveChanges();
                                //    code = 0;
                                //}
                                #endregion
                            }
                            else
                            {
                                return new JsonResult(new { code = 1, msg = "网络连接超时!" });
                            }
                        }
                        else
                        {
                            algo.GPU = viewModel.GPU;
                            algo.Type = (AlgorithmType)viewModel.Type;
                            algo.Similar = viewModel.Similar;
                            algo.Cid = viewModel.Cid;
                            algo.DectectFirst = viewModel.DectectFirst;
                            algo.DectectSecond = viewModel.DectectSecond;
                            algo.Track = viewModel.Track;
                            algo.ShipId = base.user.ShipId;
                            if (string.IsNullOrEmpty(viewModel.Id))
                            {
                                algo.Id = Guid.NewGuid().ToString();
                                _context.Algorithm.Add(algo);
                            }
                            else
                            {
                                algo.Id = viewModel.Id;
                                _context.Algorithm.Update(algo);
                            }
                            _context.SaveChanges();
                        }
                        return new JsonResult(new { code = 0 });
                    }
                  
                }
                return new JsonResult(new { code = 1, msg = "操作界面数据失败!" });
            }
            catch (Exception ex)
            {
                _logger.LogError("保存算法配置失败", model);
                return new JsonResult(new { code = 1, msg = "数据保存失败!" + ex.Message });
            }
        }
        private bool SendData(ProtoBuffer.Models.AlgorithmInfo algorithm, string nextIdentity) 
        {
            assembly.SendAlgorithmSet(algorithm, nextIdentity);
            try
            {
                bool flag = true;
                new TaskFactory().StartNew(() =>
                {
                    while (flag)
                    {
                        if (ManagerHelp.Reponse != "")
                        {
                            flag = false;
                        }
                        Thread.Sleep(500);
                    }
                }).Wait(timeout);
                flag = false;
            }
            catch (Exception)
            {
            }
            if (ManagerHelp.Reponse == "OK")
            {
                ManagerHelp.Reponse = "";
                return true;
            }
            return false;
        }
       /// <summary>
       /// 数据校验
       /// </summary>
       /// <param name="viewModel"></param>
       /// <param name="algo"></param>
       /// <param name="msg"></param>
       /// <returns></returns>
        public bool DataCheck(AlgorithmViewModel viewModel,ref Algorithm algo, ref string msg) 
        {
            if (!string.IsNullOrEmpty(viewModel.Id))
            {
                //查看数据是否存在
                algo = _context.Algorithm.FirstOrDefault(c => c.Id == viewModel.Id);
                if (algo == null)
                {
                    msg = "此数据不存在";
                    return false;
                }
                if (viewModel.Type == (int)AlgorithmType.ATTENDANCE_IN || viewModel.Type == (int)AlgorithmType.ATTENDANCE_OUT)
                { //找出此摄像机下否有考勤数据
                    var data = _context.Algorithm.FirstOrDefault(c =>c.Cid == viewModel.Cid && (c.Type == AlgorithmType.ATTENDANCE_IN || c.Type == AlgorithmType.ATTENDANCE_OUT));
                    if (data != null && data.Id != algo.Id)
                    {
                        msg = "一个摄像机只能设置考勤入或考勤出";                        
                        return false;
                    }
                }
            }
            else
            {
                if (viewModel.Type == (int)AlgorithmType.ATTENDANCE_IN || viewModel.Type == (int)AlgorithmType.ATTENDANCE_OUT)
                {
                    //找出此摄像机下否有考勤数据
                    var data = _context.Algorithm.FirstOrDefault(c =>c.Cid == viewModel.Cid && (c.Type == AlgorithmType.ATTENDANCE_IN || c.Type == AlgorithmType.ATTENDANCE_OUT));
                    if (data != null)
                    {
                        msg = "一个摄像机只能设置考勤入或考勤出";
                        return false;
                    }
                }
            }
            return true;
        }
        public IActionResult Query(string type, string cid)
        {
            string shipId = base.user.ShipId;
            int agotype = string.IsNullOrEmpty(type) ? 0 : Convert.ToInt32(type);
            var data = from a in _context.Algorithm
                       join b in _context.Camera on a.Cid equals b.Id
                       where a.ShipId==shipId&&(agotype>0 ? a.Type == (AlgorithmType)agotype:1==1) &&(!string.IsNullOrEmpty(cid)? a.Cid == cid:1==1) 
                       select new
                       {
                           a.Id,
                           a.Type,
                           a.GPU,
                           a.Similar,
                           a.Cid,
                           b.NickName,
                           a.DectectFirst,
                           a.DectectSecond,
                           a.Track
                       };
            var result = new
            {
                code = 0,
                data = data.ToList()
            };
            return new JsonResult(result);
        }
        /// <summary>
        /// 处理protoBuf消息实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private ProtoBuffer.Models.AlgorithmInfo GetProtoAlgorithm(AlgorithmViewModel model,string cid) 
        {
            ProtoBuffer.Models.AlgorithmInfo info = new ProtoBuffer.Models.AlgorithmInfo()
            {
                cid =cid,
                aid=model.Id,
                gpu = model.GPU,
                similar = (float)model.Similar,
                dectectfirst = (float)model.DectectFirst,
                dectectsecond = (float)model.DectectSecond,
                track = (float)model.Track,
                type = (ProtoBuffer.Models.AlgorithmInfo.Type)model.Type
            };
            return info;
        }
        public IActionResult Delete(string id) 
        {
            if (!string.IsNullOrEmpty(id))
            {
                var algo = _context.Algorithm.FirstOrDefault(c => c.Id == id);
                if (algo == null)
                {
                    return new JsonResult(new { code = 1, msg = "此数据不存在" });
                }
                _context.Algorithm.Remove(algo);
                _context.SaveChanges();
                return new JsonResult(new { code = 0 });
            }
            return new JsonResult(new { code = 1, msg = "未接收到要删除的数据"});
        }

        /// <summary>
        /// 获取所添加算法的通讯ID
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        private string GetIdentity(int type)
        {
            string name = Enum.GetName(typeof(AlgorithmType), type);
            if (name== "ATTENDANCE_IN"||name== "ATTENDANCE_OUT")
            {
                name = ManagerHelp.FaceName.ToUpper();
            }
            if (base.user.IsLandHome)
            {
                string tokenstr = HttpContext.Session.GetString("comtoken");
                List<ComponentToken> tokens = JsonConvert.DeserializeObject<List<ComponentToken>>(tokenstr);
                var component = tokens.FirstOrDefault(c => c.Type == ComponentType.AI&&c.Name.ToUpper()==name);
                if (component != null)
                {
                    return component.CommId;
                }
            }
            else
            {
                //获取设备的组件ID
                var component = _context.Component.FirstOrDefault(c => c.Type == ComponentType.AI && c.Name.ToUpper() == name);
                if (component != null)
                {
                    return component.CommId;
                }
            }
            return "";
        }
        private string GetViewName(AlgorithmType type)
        {
            string name = "";
            switch (type)
            {
                case AlgorithmType.HELMET:
                    name = "安全帽";
                    break;
                case AlgorithmType.PHONE:
                    name = "打电话";
                    break;
                case AlgorithmType.SLEEP:
                    name = "睡觉";
                    break;
                case AlgorithmType.FIGHT:
                    name = "打架";
                    break;
                case AlgorithmType.ATTENDANCE_IN:
                    name = "考勤入";
                    break;
                case AlgorithmType.ATTENDANCE_OUT:
                    name = "考勤出";
                    break;
                default:
                    break;
            }
            return name;
        }
    }
}