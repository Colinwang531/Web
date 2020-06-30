using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.ProtoBuffer;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class HomeController : BaseController
    {
        private readonly MyContext _context;
        public HomeController(MyContext context)
        {
            _context = context;
        }

        public IActionResult Index(string id = "")
        {
            ViewBag.shipId = id;
            return View();
        }
        public IActionResult LandHome()
        {
            byte[] byt = HttpContext.Session.Get("uid");
            string uid = Encoding.UTF8.GetString(byt);
            var data = _context.Ship.ToList();
            ViewBag.isAdmin =  uid.ToLower() == "admin" ? true : false;
            ManagerHelp.ShipId = "";
            return View(data);
        }
        public IActionResult LoadAlarm(int pageIndex,int pageSize)
        {
            try
            {
                SearchAlarmViewModel model = new SearchAlarmViewModel()
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };
                DateTime dts = DateTime.Now;
                //取出报警表中指定条数的数据
                var alarm = (from a in _context.Alarm
                         join b in _context.AlarmInformation on a.Id equals b.AlarmId
                         where b.Type != 5
                         select new { 
                            b.Id,
                            a.Time,
                            a.Picture,
                            b.Type,
                            b.Cid,
                            a.ShipId
                         }).ToList();
                //var alarm = _context.Alarm.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                //var alarmIds = string.Join(',', alarm.Select(c => c.Id));
                //var alarmInfo = _context.AlarmInformation.Where(c => alarmIds.Contains(c.AlarmId)).ToList();
                var infoIds = string.Join(',', alarm.Select(c => c.Id));
                var cids = string.Join(',', alarm.Select(c => c.Cid));
                var shipIds = string.Join(',', alarm.Select(c => c.ShipId));
                var alarmInfoPos = _context.AlarmInformationPosition.Where(c => infoIds.Contains(c.AlarmInformationId)).ToList();
                var camera = _context.Camera.Where(c => cids.Contains(c.Cid)).ToList();
                var ship = _context.Ship.Where(c => shipIds.Contains(c.Id)).ToList();
                var data = (from a in alarm
                           join c in alarmInfoPos on a.Id equals c.AlarmInformationId
                           join d in camera on a.Cid equals d.Cid
                           join e in ship on a.ShipId equals e.Id
                           select new
                           {
                               e.Name,
                               a.ShipId,
                               a.Time,
                               Picture = ManagerHelp.DrawAlarm(a.Picture, c.X, c.Y, c.W, c.H),
                               a.Cid,
                               a.Type,
                               d.NickName,
                           }).AsParallel();
                int count = data.Count();
                var pageList= data.Skip((pageIndex - 1)*pageSize).Take(pageSize);
                DateTime dte = DateTime.Now;
                var result = new
                {
                    code = 0,
                    data = pageList,
                    ship = _context.Ship.ToList(),
                    pageIndex = model.PageIndex,
                    pageSize = model.PageSize,
                    count = count
                };
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "查询失败"+ex.Message });
            }
        }
        public IActionResult SearchAlarm(string searchModel)
        {
            var model=JsonConvert.DeserializeObject<SearchAlarmViewModel>(searchModel);
            int count = 0;
            var pageList = Search(model, out count);
            var result = new
            {
                code = 0,
                data = pageList,
                pageIndex = model.PageIndex,
                pageSize = model.PageSize,
                count = count
            };
            return new JsonResult(result);
        }
        private List<AlarmViewModel> Search(SearchAlarmViewModel model,out int count)
        {
            var data = from a in _context.Alarm
                       join b in _context.AlarmInformation on a.Id equals b.AlarmId
                       join c in _context.Camera on b.Cid equals c.Cid
                       join d in _context.AlarmInformationPosition on b.Id equals d.AlarmInformationId
                       join e in _context.Ship on a.ShipId equals e.Id
                       where a.ShipId == b.Shipid && b.Shipid == c.ShipId && c.ShipId == d.ShipId && e.Id == a.ShipId && b.Type != 5
                       select new
                       {
                           e.Name,
                           a.ShipId,
                           a.Time,
                           Picture =ManagerHelp.DrawAlarm(a.Picture, d.X, d.Y, d.W, d.H),
                           b.Cid,
                           b.Type,
                           c.NickName,
                       };
            if (model != null)
            {
                if (!string.IsNullOrEmpty(model.Cid))
                {
                    data = data.Where(c => c.Cid == model.Cid);
                }
                if (!string.IsNullOrEmpty(model.ShipId))
                {
                    data = data.Where(c => c.ShipId == model.ShipId);
                }
                if (!string.IsNullOrEmpty(model.Name))
                {
                    data = data.Where(c => c.NickName.Contains(model.Name));
                }
                if (model.Type > 0)
                {
                    data = data.Where(c => c.Type == model.Type);
                }
                if (!(string.IsNullOrEmpty(model.StartTime)) && !(string.IsNullOrEmpty(model.EndTime)))
                {
                    DateTime dtStart = DateTime.Parse(model.StartTime);
                    DateTime dtEnd = DateTime.Parse(model.EndTime);
                    data = data.Where(c => c.Time >= dtStart && c.Time <= dtEnd);
                }
                else if (!(string.IsNullOrEmpty(model.StartTime)))
                {
                    DateTime dtStart = DateTime.Parse(model.StartTime);
                    data = data.Where(c => c.Time >= dtStart);
                }
                else if (!(string.IsNullOrEmpty(model.EndTime)))
                {
                    DateTime dtEnd = DateTime.Parse(model.EndTime);
                    data = data.Where(c => c.Time <= dtEnd);
                }
            }
            count = data.Count();
            var queryList = data.Skip((model.PageIndex - 1) * model.PageSize).Take(model.PageSize).ToList();
            List<AlarmViewModel> list = new List<AlarmViewModel>();
            foreach (var item in queryList)
            {
                AlarmViewModel vmodel = new AlarmViewModel()
                {
                    Name = item.Name,
                    NickName = item.NickName,
                    Picture = item.Picture,
                    Time = item.Time,
                    Cid = item.Cid,
                    Type = item.Type
                };
                list.Add(vmodel);
            }
            return list;
        }
        /// <summary>
        /// 加载单个船的主页面
        /// </summary>
        /// <param name="shipId">如果存在则是客户端查看的单个船</param>
        /// <returns></returns>
        public IActionResult Load(string shipId)
        {
            try
            {
                //是否显示返回陆地端主页面
                bool flag = true;
                if (string.IsNullOrEmpty(shipId))
                {
                    flag = false;
                    //是否有般存在
                    var ship = _context.Ship.FirstOrDefault();
                    if (ship == null)
                    {
                        shipId = Guid.NewGuid().ToString();
                        Models.Ship sm = new Models.Ship()
                        {
                            Flag = false,
                            Id = shipId,
                            Name = "船1",
                            Type = 1
                        };
                        _context.Ship.Add(sm);
                        _context.SaveChanges();
                    }
                    else
                    {
                        shipId = ship.Id;
                    }
                }

                ManagerHelp.ShipId = shipId;

                var result = new
                {
                    code = 0,
                    isReturn = flag
                };
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "获取组件失败！" + ex.Message });
            }
        }

    }
}
