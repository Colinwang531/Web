using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class AlarmController : BaseController
    {
        private MyContext _context;
        public AlarmController(MyContext context)
        {
            _context = context;
        }
        public IActionResult Index(bool isShow)
        {
            ViewBag.IsShowLayout = isShow;//显示报警的框架
            return View();
        }
        public IActionResult AlarmShipAll()
        {
            return View();
        }
        public IActionResult Load(int pageIndex, int pageSize)
        {
            try
            {
                string time=DateTime.Now.ToString();
                //查询摄像机名称及其报警类型
                var data = from a in _context.Alarm
                            join b in _context.AlarmInformation on a.Id equals b.AlarmId
                            join c in _context.Camera on b.Cid equals c.Cid
                            join d in _context.AlarmInformationPosition on b.Id equals d.AlarmInformationId
                            where a.ShipId==b.Shipid&&b.Shipid==c.ShipId&&c.ShipId==d.ShipId
                            where b.Type != 5 && a.ShipId==ManagerHelp.ShipId
                            select new
                            {
                                a.Time,
                                Picture = ManagerHelp.DrawAlarm(a.Picture, d.X, d.Y, d.W, d.H),
                                b.Cid,
                                b.Type,
                                c.NickName,
                                b.Id,
                                b.AlarmId
                            };
                string endtime = DateTime.Now.ToString();
                int count = data.ToList().Count;
                var dataPage = data.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                var result = new
                {
                    code = 0,
                    data = dataPage,
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    count = count
                };
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "查询失败!" + ex.Message });
            }

        }
        /// <summary>
        /// 分页获取图片座标位置
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="type"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageCount"></param>
        public IActionResult QueryPage(string cid, string name, int type, string startTime, string endTime, int pageIndex, int pageSize)
        {
            try
            {
                var data = from a in _context.Alarm
                           join b in _context.AlarmInformation on a.Id equals b.AlarmId
                           join c in _context.Camera on b.Cid equals c.Cid
                           join d in _context.AlarmInformationPosition on b.Id equals d.AlarmInformationId
                           where a.ShipId == b.Shipid && b.Shipid == c.ShipId && c.ShipId == d.ShipId
                           where b.Type!=5 && a.ShipId==ManagerHelp.ShipId
                           select new
                           {
                               a.Time,
                               Picture = ManagerHelp.DrawAlarm(a.Picture,d.X,d.Y,d.W,d.H),
                               b.Cid,
                               b.Type,
                               c.NickName,
                               a.Id,
                               b.AlarmId
                           };
                if (!string.IsNullOrEmpty(cid))
                {
                    data = data.Where(c => c.Cid == cid);
                }
                if (!string.IsNullOrEmpty(name))
                {
                    data = data.Where(c => c.NickName.Contains(name));
                }
                if (type > 0)
                {
                    data = data.Where(c => c.Type == type);
                }
                if (!(string.IsNullOrEmpty(startTime)) && !(string.IsNullOrEmpty(endTime)))
                {
                    DateTime dtStart = DateTime.Parse(startTime);
                    DateTime dtEnd = DateTime.Parse(endTime);
                    data = data.Where(c => c.Time >= dtStart && c.Time <= dtEnd);
                }
                else if (!(string.IsNullOrEmpty(startTime)))
                {
                    DateTime dtStart = DateTime.Parse(startTime);
                    data = data.Where(c => c.Time >= dtStart);
                }
                else if (!(string.IsNullOrEmpty(endTime)))
                {
                    DateTime dtEnd = DateTime.Parse(endTime);
                    data = data.Where(c => c.Time <= dtEnd);
                }
                int count = data.ToList().Count();
                var pageList = data.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                var result = new
                {
                    code = 0,
                    data = pageList,
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    count = count
                };
                return new JsonResult(result);
            }
            catch (Exception e)
            {
                return new JsonResult(new { code = 1, msg = "查询失败" });
            }

        }

        public IActionResult LoadAlarm(int pageIndex, int pageSize)
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
                             select new
                             {
                                 b.Id,
                                 a.Time,
                                 a.Picture,
                                 b.Type,
                                 b.Cid,
                                 a.ShipId
                             }).ToList();
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
                var pageList = data.Skip((pageIndex - 1) * pageSize).Take(pageSize);
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
                return new JsonResult(new { code = 1, msg = "查询失败" + ex.Message });
            }
        }
        public IActionResult SearchAlarm(string searchModel)
        {
            var model = JsonConvert.DeserializeObject<SearchAlarmViewModel>(searchModel);
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
        private List<AlarmViewModel> Search(SearchAlarmViewModel model, out int count)
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
                           Picture = ManagerHelp.DrawAlarm(a.Picture, d.X, d.Y, d.W, d.H),
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
       
    }
}