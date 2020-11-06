using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Cms;
using SmartWeb.DB;
using SmartWeb.Interface;
using SmartWeb.Models;
using SmartWeb.ProtoBuffer;
using SmartWeb.Tool;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Smartweb.Hubs;

namespace SmartWeb.Controllers
{
    public class AlgorithmController : BaseController
    {
        private MyContext _context;
        private int timeout = 5000;//超时时间
        SendDataMsg assembly = new SendDataMsg ();
        private ILogger<AlgorithmController> _logger;
        //缓存船舶端的设备信息
        private static List<ProtoBuffer.Models.DeviceInfo> boatDevices = new List<ProtoBuffer.Models.DeviceInfo>();
        private static  List<Camera> cameras;
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
            if (base.user.IsLandHome)
            {
                return LandLoad();
            }
            string shipId = base.user.ShipId;
            var algor = _context.Algorithm.Where(c => c.ShipId == shipId).ToList();
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
                camera = camera
            };
            return new JsonResult(result);
        }
        public IActionResult LandLoad()
        {
            string identtity = base.user.ShipId;
            string browsertoken = HttpContext.Session.GetString("comtoken");
            string webId = ManagerHelp.GetLandToId(browsertoken);
            List<AlgorithmViewModel> algorithms = new List<AlgorithmViewModel>();
            List<ProtoBuffer.Models.AlgorithmInfo> protoDate = new List<ProtoBuffer.Models.AlgorithmInfo>();
            if (string.IsNullOrEmpty(webId))
            {
                return new JsonResult(new { code = 0, data = algorithms });
            }
            assembly.SendAlgorithmQuery(identtity + ":" + webId);
            assembly.SendDeveiceQuery(identtity + ":" + webId);
            bool flag = true;
            new TaskFactory().StartNew(() =>
            {
                while (flag)
                {
                    if (ManagerHelp.AlgorithmReponse != "" && ManagerHelp.DeviceReponse != "")
                    {
                        flag = false;
                    }
                    Thread.Sleep(100);
                }
            }).Wait(timeout);
            flag = false;
            try
            {
                if (ManagerHelp.AlgorithmReponse != "")
                {
                    protoDate = JsonConvert.DeserializeObject<List<ProtoBuffer.Models.AlgorithmInfo>>(ManagerHelp.AlgorithmReponse);
                }
                if (ManagerHelp.DeviceReponse != "")
                {
                    boatDevices = JsonConvert.DeserializeObject<List<ProtoBuffer.Models.DeviceInfo>>(ManagerHelp.DeviceReponse);
                }
                ManagerHelp.AlgorithmReponse = "";
                ManagerHelp.DeviceReponse = "";
            }
            catch (Exception)
            {
            }
            cameras = new List<Camera>();
            foreach (var item in boatDevices)
            {
                if (item.camerainfos != null)
                {
                    foreach (var cam in item.camerainfos)
                    {
                        Camera model = new Camera()
                        {
                            Id = cam.cid,
                            NickName = cam.nickname,
                            DeviceId = item.did,
                            Index=cam.index
                        };
                        cameras.Add(model);
                    }
                }
            }
            foreach (var item in protoDate)
            {
                string cid = item.cid;
                string nickName = "";
                if (cameras.Where(c => c.Id == cid).Any()) {
                    nickName = cameras.FirstOrDefault(c => c.Id == cid).NickName;
                }
                algorithms.Add(new AlgorithmViewModel()
                {
                    Cid = cid,
                    NickName=nickName,
                    DectectFirst = item.dectectfirst,
                    DectectSecond = item.dectectsecond,
                    GPU = item.gpu,
                    Id = item.aid,
                    Similar = item.similar,
                    Track = item.track,
                    Type = (int)item.type
                });
            }
            
            var result = new
            {
                code = 0,
                data = algorithms,
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
                string shipId = base.user.ShipId;//为陆地端时保存的是XMQ的组件ID
                if (shipId == "")
                {
                    return new JsonResult(new { code = 1, msg = "船不存在，无法添加数据" });
                }
                var viewModel = JsonConvert.DeserializeObject<AlgorithmViewModel>(model);
                if (viewModel != null)
                {
                    int code = 1;
                    string errMsg = "";
                    string AIName = Enum.GetName(typeof(AlgorithmType), viewModel.Type);
                    if (base.user.IsLandHome)
                    {
                        //获取当前船舶的组件ID（webId）
                        string tokenstr = HttpContext.Session.GetString("comtoken");
                        string identity =ManagerHelp.GetLandToId(tokenstr);
                        if (string.IsNullOrEmpty(identity))
                        {
                            return new JsonResult(new { code = 1, msg = "当前船舶已失联，请重新连接" });
                        }
                        //获取当前组件下的某个算法
                        string algoComId= ManagerHelp.GetLandToId(tokenstr,ComponentType.AI, AIName);
                        if (string.IsNullOrEmpty(algoComId) && viewModel.Type != (int)AlgorithmType.CAPTURE)
                        {
                            string name = GetViewName((AlgorithmType)viewModel.Type);
                            return new JsonResult(new { code = 1, msg = "算法【" + name + "】组件未启动" });
                        }
                        var cam = cameras.FirstOrDefault(c => c.Id == viewModel.Cid);
                        //算法里的摄像机ID=设备ID:摄像机ID:摄像机通道
                        string cid = cam.DeviceId + ":" + cam.Id + ":" + cam.Index;
                        //向船舶端发送算法配置请求
                        ProtoBuffer.Models.AlgorithmInfo algorithm = GetProtoAlgorithm(viewModel, cid);
                        code = SendData(algorithm, (shipId + ":" + identity));
                        if (code == 400) errMsg = "网络请求超时。。。";
                        else if(code!=0)errMsg = "算法配置失败";
                    }
                    else
                    {
                        Algorithm algo = new Algorithm();
                        if (!DataCheck(viewModel, ref algo, ref errMsg))
                        {
                            return new JsonResult(new { code = 1, msg = errMsg });
                        }
                        //获取船舶某个算法的组件ID
                        string identity = ManagerHelp.GetShipToId(ComponentType.AI, AIName); 
                        if (string.IsNullOrEmpty(identity)&& viewModel.Type != (int)AlgorithmType.CAPTURE)
                        {
                            string name = GetViewName((AlgorithmType)viewModel.Type);
                            return new JsonResult(new { code = 1, msg = "算法【" + name + "】组件未启动" });
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
                        //缺岗直接进库不用发消息
                        if (viewModel.Type == (int)AlgorithmType.CAPTURE)
                        {
                            _context.SaveChanges();
                            code = 0;
                            ManagerHelp.LiveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        else
                        {
                            viewModel.Id = algo.Id;
                            var camera = _context.Camera.FirstOrDefault(c => c.Id == viewModel.Cid);
                            if (camera != null)
                            {
                                string cid = camera.DeviceId + ":" + camera.Id + ":" + camera.Index;
                                ProtoBuffer.Models.AlgorithmInfo algorithm = GetProtoAlgorithm(viewModel, cid);
                                ManagerHelp.isFaceAlgorithm = false;
                                if (viewModel.Type==(int)AlgorithmType.ATTENDANCE_IN||viewModel.Type==(int)AlgorithmType.ATTENDANCE_OUT)
                                {
                                    ManagerHelp.isFaceAlgorithm = true;
                                }
                                code = SendData(algorithm, identity);
                                if (code == 0)
                                {
                                    _context.SaveChanges();
                                }
                                else if (code == 2) errMsg = "网络请求超时。。。";
                                else errMsg = "算法配置失败";
                            }
                        }
                    }
                    return new JsonResult(new { code = code, msg = errMsg });
                }
                return new JsonResult(new { code = 1, msg = "操作界面数据失败!" });
            }
            catch (Exception ex)
            {
                _logger.LogError("保存算法配置失败", model);
                return new JsonResult(new { code = 1, msg = "数据保存失败!" + ex.Message });
            }
        }
        private int SendData(ProtoBuffer.Models.AlgorithmInfo algorithmInfo, string nextIdentity)
        {
            int result = 1;
            try
            {
                assembly.SendAlgorithmSet(algorithmInfo, nextIdentity);
                bool flag = true;
                new TaskFactory().StartNew(() =>
                {
                    while (flag && ManagerHelp.AlgorithmResult == "")
                    {
                        Thread.Sleep(1000);
                    }
                }).Wait(timeout);
                flag = false;
                if (ManagerHelp.AlgorithmResult != "")
                {
                    result = Convert.ToInt32(ManagerHelp.AlgorithmResult);
                    ManagerHelp.AlgorithmResult = "";
                }
                else
                {
                    result = 400;//网络超时
                }
            }
            catch (Exception ex)
            {
            }
            return result;
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