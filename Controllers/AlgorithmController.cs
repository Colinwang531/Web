using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.ProtoBuffer;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class AlgorithmController : BaseController
    {
        private MyContext _context;
        private ProtoManager manager=new ProtoManager ();
        public AlgorithmController(MyContext context) 
        {
            _context = context;        
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
            var protoDate=manager.AlgorithmQuery(base.user.ShipId);
            var data = from a in protoDate
                       select new
                       {
                           id=a.cid.Split(',')[0],
                           cid=a.cid.Split(',')[1],
                           Type=a.type,
                           GPU=a.gpu,
                           Similar=a.similar,
                           NickName=a.cid.Split(',')[2],
                           DectectFirst=a.dectectfirst,
                           DectectSecond=a.dectectsecond,
                           Track=a.track
                       };
            var device = manager.DeviceQuery(base.user.ShipId);
            List<Camera> cameras = new List<Camera>();
            foreach (var item in device)
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
                    
                    if (base.user.IsLandHome)
                    {
                        ProtoBuffer.Models.AlgorithmInfo algorithm = GetProtoAlgorithm(viewModel);
                        int res=manager.AlgorithmSet(shipId, algorithm);
                        return new JsonResult(new { code = res, msg = res != 0 ? res==2? "一个摄像机只能设置考勤入或考勤出" : "数据修改失败" : "" });
                    }
                    //找出此摄像机下否有考勤数据
                    var data = _context.Algorithm.FirstOrDefault(c => c.ShipId == shipId && c.Cid == viewModel.Cid && (c.Type == AlgorithmType.ATTENDANCE_IN || c.Type == AlgorithmType.ATTENDANCE_OUT));                   
                    if (!string.IsNullOrEmpty(viewModel.Id))
                    {
                        //查看数据是否存在
                        var algo = _context.Algorithm.FirstOrDefault(c => c.Id == viewModel.Id);
                        if (algo == null)
                        {
                            return new JsonResult(new { code = 1, msg = "此数据不存在" });
                        }
                        if (data != null && data.Id != algo.Id)
                        {
                            return new JsonResult(new { code = 1, msg = "一个摄像机只能设置考勤入或考勤出" });
                        }
                        algo.GPU = viewModel.GPU;
                        algo.Type = (AlgorithmType)viewModel.Type;
                        algo.Similar = viewModel.Similar;
                        algo.Cid = viewModel.Cid;
                        algo.DectectFirst =viewModel.DectectFirst;
                        algo.DectectSecond = viewModel.DectectSecond;
                        algo.Track =viewModel.Track;
                        _context.Algorithm.Update(algo);
                    }
                    else
                    {
                        if (data != null)
                        {
                            return new JsonResult(new { code = 1, msg = "一个摄像机只能设置考勤入或考勤出" });
                        }
                        Algorithm algo = new Algorithm()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Cid=viewModel.Cid,
                            GPU = viewModel.GPU,
                            Type = (AlgorithmType)viewModel.Type,
                            Similar =viewModel.Similar,
                            DectectFirst = viewModel.DectectFirst,
                            DectectSecond =viewModel.DectectSecond,
                            Track =viewModel.Track,
                            ShipId = base.user.ShipId
                        };
                        _context.Algorithm.Add(algo);
                    }
                    //Task.Factory.StartNew(state => {
                    //    ProtoBuffer.Models.AlgorithmInfo algorithm = GetProtoAlgorithm(viewModel);
                    //    int res = manager.AlgorithmSet(base.user.ShipId, algorithm);
                    //}, TaskCreationOptions.LongRunning);
                  
                    _context.SaveChanges();
                }
                return new JsonResult(new { code = 0 });

            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "数据保存失败!" + ex.Message });
            }
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