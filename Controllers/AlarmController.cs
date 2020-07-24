using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.Tool;
using System.Text;
using System.Net.WebSockets;

namespace ShipWeb.Controllers
{
    public class AlarmController : BaseController
    {
        private MyContext _context;
        public AlarmController(MyContext context)
        {
            _context = context;
        }
        public IActionResult Index(bool isShow,string shipid="")
        {
            ViewBag.IsShowLayout = isShow;//显示报警的框架
            if (!string.IsNullOrEmpty(shipid))
            {
                base.user.ShipId = shipid;
            }
            ViewBag.IsLandHome = base.user.IsLandHome;
            ViewBag.LoginName = base.user.Name;
            return View();
        }
        public IActionResult AlarmShipAll()
        {
            return View();
        }
        /// <summary>
        /// 船舶端查询报警信息
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IActionResult Load(int pageIndex, int pageSize)
        {
            try
            {
                int total = 0;
                var list = GetDate(new SearchAlarmViewModel() { ShipId = base.user.ShipId }, pageIndex, pageSize, out total);
                var result = new
                {
                    code = 0,
                    data = list,
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    count = total
                };
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "查询失败!" + ex.Message });
            }

        }
        /// <summary>
        /// 船舶端分页查询
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="type"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageCount"></param>
        public IActionResult QueryPage(string searchModel, int pageIndex, int pageSize)
        {
            try
            {
                var model = JsonConvert.DeserializeObject<SearchAlarmViewModel>(searchModel);
                int total = 0;
                model.ShipId = base.user.ShipId;
                var list = GetDate(model, pageIndex, pageSize, out total);
                var result = new
                {
                    code = 0,
                    data = list,
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    count = total
                };
                return new JsonResult(result);
            }
            catch (Exception e)
            {
                return new JsonResult(new { code = 1, msg = "查询失败" });
            }

        }
        /// <summary>
        /// 陆地端查询报警信息
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IActionResult LoadAlarm(int pageIndex, int pageSize)
        {
            try
            {
                int total = 0;
                var list = GetDate(null, pageIndex, pageSize, out total);
                var result = new
                {
                    code = 0,
                    data = list,
                    ship = _context.Ship,
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    count = total
                };            
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "查询失败" + ex.Message });
            }

        }
        /// <summary>
        /// 陆地端分布查询报警信息
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IActionResult SearchAlarm(string searchModel, int pageIndex, int pageSize)
        {

            var model = JsonConvert.DeserializeObject<SearchAlarmViewModel>(searchModel);
            int total = 0;
            var list = GetDate(model, pageIndex, pageSize, out total);
            var result = new
            {
                code = 0,
                data = list,
                pageIndex = pageIndex,
                pageSize = pageSize,
                count = total
            };
            return new JsonResult(result);

        }
        private List<AlarmViewModel> GetDate(SearchAlarmViewModel model, int pageIndex, int pageSize, out int total)
        {
            List<AlarmViewModel> list = new List<AlarmViewModel>();
            total = 0;
            if (model == null)
            {
                model = new SearchAlarmViewModel();
            }
            //查询船信息
            var ship = _context.Ship.Where(c => (model.ShipId == "" ? 1 == 1 : c.Id == model.ShipId)).ToList();
            var shipIds = string.Join(',', ship.Select(c => c.Id));
            var alarmData = from a in _context.Alarm
                            join b in _context.AlarmInfo on a.Id equals b.AlarmId
                            where shipIds.Contains(a.ShipId) && (b.Type != AlarmType.ATTENDANCE_IN && b.Type != AlarmType.ATTENDANCE_OUT) && (model.Type == 0 ? 1 == 1 : b.Type == (AlarmType)model.Type)
                            select new
                            {
                                a.Id,
                                a.Cid,
                                a.Picture,
                                a.Time,
                                b.Type,
                                a.ShipId,
                                infoId = b.Id
                            };
            if (!(string.IsNullOrEmpty(model.StartTime)) && !(string.IsNullOrEmpty(model.EndTime)))
            {
                DateTime dtStart = DateTime.Parse(model.StartTime);
                DateTime dtEnd = DateTime.Parse(model.EndTime);
                alarmData = alarmData.Where(c => c.Time >= dtStart && c.Time <= dtEnd);
            }
            else if (!(string.IsNullOrEmpty(model.StartTime)))
            {
                DateTime dtStart = DateTime.Parse(model.StartTime);
                alarmData = alarmData.Where(c => c.Time >= dtStart);
            }
            else if (!(string.IsNullOrEmpty(model.EndTime)))
            {
                DateTime dtEnd = DateTime.Parse(model.EndTime);
                alarmData = alarmData.Where(c => c.Time <= dtEnd);
            }
            var alarm = alarmData.ToList();
            var cids = string.Join(',', alarm.Select(c => c.Cid));
            //查询摄像机信息
            var camera = _context.Camera.Where(c => (model.Name == "" ? 1 == 1 : c.NickName.Contains(model.Name)) && cids.Contains(c.Id)).ToList();
            var infoIds = string.Join(',', alarm.Select(c => c.infoId));
            //查询位置信息
            var pics = _context.AlarmPosition.Where(c => infoIds.Contains(c.AlarmInfoId)).ToList();
            //组合数据
            var data = from a in alarm
                       join b in camera on a.Cid equals b.Id
                       join c in pics on a.infoId equals c.AlarmInfoId
                       join d in ship on a.ShipId equals d.Id
                       select new
                       {
                           a.Time,
                           a.Id,
                           d.Name,
                           b.NickName,
                           a.Type,
                           a.Picture,
                           c.X,
                           c.Y,
                           c.W,
                           c.H
                       };
            total = data.Count();
            var datapage = data.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            foreach (var item in datapage)
            {
                AlarmViewModel avm = new AlarmViewModel()
                {
                    Id = item.Id,
                    Name = item.Name,
                    NickName = item.NickName,
                    Picture = Convert.FromBase64String(Encoding.UTF8.GetString(item.Picture)),
                    Type = (int)item.Type,
                    Time = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    H = item.H,
                    W = item.W,
                    X = item.X,
                    Y = item.Y
                };
                list.Add(avm);
            }
            return list;
        }
    }
}