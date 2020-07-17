using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class AlgorithmController : BaseController
    {
        private MyContext _context;
        public AlgorithmController(MyContext context) 
        {
            _context = context;        
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Load() {
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
                           a.DetectThreshold_1,
                           a.DetectThreshold_2,
                           a.TrackThreshold
                       };
            var result = new
            {
                code = 0,
                data = data,
                camera=camera
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
                    if (!string.IsNullOrEmpty(viewModel.Id))
                    {
                        var algo = _context.Algorithm.FirstOrDefault(c => c.Id == viewModel.Id);
                        if (algo == null)
                        {
                            return new JsonResult(new { code = 1, msg = "此数据不存在" });
                        }
                        algo.GPU = viewModel.GPU;
                        algo.Type = (AlgorithmType)viewModel.Type;
                        algo.Similar = string.IsNullOrEmpty(viewModel.Similar) ? 0 : Convert.ToDouble(viewModel.Similar);
                        algo.DetectThreshold_1 = string.IsNullOrEmpty(viewModel.DetectThreshold_1) ? 0 : Convert.ToDouble(viewModel.DetectThreshold_1);
                        algo.DetectThreshold_2 = string.IsNullOrEmpty(viewModel.DetectThreshold_2) ? 0 : Convert.ToDouble(viewModel.DetectThreshold_2);
                        algo.TrackThreshold = string.IsNullOrEmpty(viewModel.TrackThreshold) ? 0 : Convert.ToDouble(viewModel.TrackThreshold);
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
                            Similar = string.IsNullOrEmpty(viewModel.Similar) ? 0 : Convert.ToDouble(viewModel.Similar),
                            DetectThreshold_1 = string.IsNullOrEmpty(viewModel.DetectThreshold_1) ? 0 : Convert.ToDouble(viewModel.DetectThreshold_1),
                            DetectThreshold_2 = string.IsNullOrEmpty(viewModel.DetectThreshold_2) ? 0 : Convert.ToDouble(viewModel.DetectThreshold_2),
                            TrackThreshold = string.IsNullOrEmpty(viewModel.TrackThreshold) ? 0 : Convert.ToDouble(viewModel.TrackThreshold),
                            ShipId = ManagerHelp.ShipId
                        };
                        _context.Algorithm.Add(algo);
                    }
                    _context.SaveChanges();
                }
                return new JsonResult(new { code = 0 });

            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "数据保存失败!" + ex.Message });
            }
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