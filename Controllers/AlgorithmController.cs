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
            return View();
        }
        public IActionResult Load() 
        {
            if (ManagerHelp.IsShowLandHome)
            {
               return LandLoad();
            }
            var algor = _context.Algorithm.ToList();
            var camera = _context.Camera.ToList();
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
            var protoDate=manager.AlgorithmQuery(ManagerHelp.ShipId);
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
            var device = manager.DeviceQuery(ManagerHelp.ShipId);
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
                var viewModel = JsonConvert.DeserializeObject<AlgorithmViewModel>(model);
                if (viewModel != null)
                {
                    if (ManagerHelp.IsShowLandHome)
                    {
                        ProtoBuffer.Models.AlgorithmInfo algorithm = GetProtoAlgorithm(viewModel);
                        int res=manager.AlgorithmSet(ManagerHelp.ShipId, algorithm);
                        return new JsonResult(new { code = res, msg = res != 0 ? "数据修改失败" : "" });
                    }
                    if (!string.IsNullOrEmpty(viewModel.Id))
                    {
                        var algo = _context.Algorithm.FirstOrDefault(c => c.Id == viewModel.Id);
                        if (algo == null)
                        {
                            return new JsonResult(new { code = 1, msg = "此数据不存在" });
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
                            ShipId = ManagerHelp.ShipId
                        };
                        _context.Algorithm.Add(algo);
                    }
                    //Task.Factory.StartNew(state => {
                    //    ProtoBuffer.Models.AlgorithmInfo algorithm = GetProtoAlgorithm(viewModel);
                    //    int res = manager.AlgorithmSet(ManagerHelp.ShipId, algorithm);
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