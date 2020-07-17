using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class AlgorithmConfigController : BaseController
    {
        private MyContext _context;
        public AlgorithmConfigController(MyContext context) 
        {
            _context = context;        
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Load() {
            var algor = _context.AlgorithmConfig.ToList();
            var camera = _context.Camera.ToList();
            var data = from a in algor
                       join b in camera on a.Cid equals b.Cid
                       select new
                       {
                           a.Id,
                           a.Type,
                           a.GPU,
                           a.Similar,
                           a.Cid,
                           b.NickName
                       };
            var result = new
            {
                code = 0,
                data = data,
                camera=camera
            };
            return new JsonResult(result);
        }
        public IActionResult Save(string id, int type, string cid, int gpu, string similar)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var algo = _context.AlgorithmConfig.FirstOrDefault(c => c.Id == id);
                    if (algo == null)
                    {
                        return new JsonResult(new { code = 1, msg = "此数据不存在" });
                    }
                    algo.Cid = cid;
                    algo.GPU = gpu;
                    algo.Type = (AlgorithmType)type;
                    algo.Similar = string.IsNullOrEmpty(similar) ? 0 : Convert.ToDouble(similar);
                    _context.AlgorithmConfig.Update(algo);
                }
                else
                {
                    AlgorithmConfig algo = new AlgorithmConfig()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Cid = cid,
                        GPU = gpu,
                        Type = (AlgorithmType)type,
                        Similar = string.IsNullOrEmpty(similar) ? 0 : Convert.ToDouble(similar),
                        ShipId= ManagerHelp.ShipId
                    };
                    _context.AlgorithmConfig.Add(algo);
                }
                _context.SaveChanges();
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
                var algo = _context.AlgorithmConfig.FirstOrDefault(c => c.Id == id);
                if (algo == null)
                {
                    return new JsonResult(new { code = 1, msg = "此数据不存在" });
                }
                _context.AlgorithmConfig.Remove(algo);
                _context.SaveChanges();
                return new JsonResult(new { code = 0 });
            }
            return new JsonResult(new { code = 1, msg = "未接收到要删除的数据"});
        }
    }
}