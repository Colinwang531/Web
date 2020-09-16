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
            List<Camera> cameras = new List<Camera>();
            string shipId = base.user.ShipId;
            var comtent = _context.Component.FirstOrDefault(c => c.ShipId == shipId&&c.Type==Component.ComponentType.WEB);
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
                    int code = 1;
                    string msg = "";
                    if (base.user.IsLandHome&&!ManagerHelp.IsTest)
                    {
                        ProtoBuffer.Models.AlgorithmInfo algorithm = GetProtoAlgorithm(viewModel);
                        var compent = _context.Component.Where(c => c.ShipId == shipId && (c.Type == Component.ComponentType.WEB || c.Type == Component.ComponentType.AI));
                        if (compent == null)
                        {
                            return new JsonResult(new { code = 1, msg = "当前船舶未启动算法组件" });
                        }
                        //当前船的通讯ID
                        string shipIdentity = compent.FirstOrDefault(c => c.Type == Component.ComponentType.WEB&&c.ShipId==shipId).Id;
                        //当前船上对应的算法通讯ID
                        string algoIdentity= compent.FirstOrDefault(c => c.Type == Component.ComponentType.AI).Id;
                        if (SendData(algorithm,(shipIdentity + ":" + algoIdentity)))
                        {
                            _context.SaveChanges();
                            code = 0;
                            #region 发送二次请求，暂时不用
                            //int factory = 0;
                            ////根据摄像机获取设备下的通讯ID
                            //foreach (var item in boatDevices)
                            //{
                            //    if (item.camerainfos.Where(c => c.cid == viewModel.Cid).Any())
                            //    {
                            //        factory=(int)item.factory;
                            //    }
                            //}
                            //var comp = _context.Component.FirstOrDefault(c =>c.Type==(factory==(int)Device.Factory.HIKVISION?Component.ComponentType.HKD:Component.ComponentType.DHD));
                            //if (comp == null)
                            //{
                            //    //查询算法组件并入库
                            //    assembly.SendComponentQuery();
                            //    return new JsonResult(new { code = 1, msg = "算法里摄像机对应的设备组件未启动" });
                            //}
                            //if (SendData(algorithm, (shipIdentity+":"+comp.Id)))
                            //{
                            //    _context.SaveChanges();
                            //    code = 0;
                            //}
                            #endregion
                        }
                        return new JsonResult(new { code = code, msg = msg });
                    }
                    else
                    {
                        Algorithm algo = new Algorithm();
                        if (DataCheck(viewModel,algo,ref msg))
                        {
                            return new JsonResult(new { code = 1, msg = msg });
                        }
                        if (!ManagerHelp.IsTest)
                        {
                            var compent = _context.Component.FirstOrDefault(c => c.Type == Component.ComponentType.AI);
                            if (compent == null)
                            {
                                //查询算法组件并入库
                                assembly.SendComponentQuery();
                                return new JsonResult(new { code = 1, msg = "算法组件未启动" });
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
                                algo.Id = Guid.NewGuid().ToString();
                                _context.Algorithm.Add(algo);
                            }
                            else
                            {
                                _context.Algorithm.Update(algo);
                            }
                            viewModel.Id = algo.Id;
                            ProtoBuffer.Models.AlgorithmInfo algorithm = GetProtoAlgorithm(viewModel);
                            if (SendData(algorithm, compent.Id))
                            {
                                _context.SaveChanges();
                                code = 0;
                                #region 发送二次请求 暂时不用
                                ////根据摄像机获取设备下的通讯ID
                                //var factory = _context.Device.FirstOrDefault(c => c.Id == (_context.Camera.FirstOrDefault(d => d.Id == viewModel.Cid).DeviceId)).factory;
                                //compent = _context.Component.FirstOrDefault(c => c.Type == (factory == Device.Factory.HIKVISION ? Component.ComponentType.HKD : Component.ComponentType.DHD));
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
                            code = 1;
                            msg = "请求超时";
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
                            if (!string.IsNullOrEmpty(viewModel.Id))
                            {
                                algo.Id = Guid.NewGuid().ToString();
                                _context.Algorithm.Add(algo);
                            }
                            else
                            {
                                _context.Algorithm.Update(algo);
                            }
                            _context.SaveChanges();
                            code = 0;
                        }
                    }
                  
                }
                return new JsonResult(new { code = 1, msg = "处理界面数据失败" });
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
        public bool DataCheck(AlgorithmViewModel viewModel,Algorithm algo, ref string msg) 
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
        private ProtoBuffer.Models.AlgorithmInfo GetProtoAlgorithm(AlgorithmViewModel model) 
        {
            ProtoBuffer.Models.AlgorithmInfo info = new ProtoBuffer.Models.AlgorithmInfo()
            {
                cid = model.Id==""? model.Cid : (model.Id + ","+model.Cid),
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
    }
}