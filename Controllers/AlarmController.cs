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
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Diagnostics;
using DinkToPdf;
using DinkToPdf.Contracts;
using ShipWeb.Interface;
using Newtonsoft.Json.Schema;

namespace ShipWeb.Controllers
{
    public class AlarmController : BaseController
    {
        private MyContext _context;
        private IConverter _converter;
        private IPDFService _PDFService;
        public AlarmController(MyContext context, IConverter converter, IPDFService pDFService)
        {
            _context = context;
            _converter = converter;
            _PDFService = pDFService;
        }
        public IActionResult AlarmInfo(bool flag=false,bool isShip=false) 
        {
            var sleep = _context.Alarm.Where(c => c.Type == Alarm.AlarmType.SLEEP).ToList();
            var fight = _context.Alarm.Where(c => c.Type == Alarm.AlarmType.FIGHT).ToList();
            var helmet = _context.Alarm.Where(c => c.Type == Alarm.AlarmType.HELMET).ToList();
            var phone = _context.Alarm.Where(c => c.Type == Alarm.AlarmType.PHONE).ToList();
            ViewBag.sleep = sleep.Count; 
            ViewBag.fight = fight.Count;
            ViewBag.helmet = helmet.Count;
            ViewBag.phone = phone.Count;
            ViewBag.IsShowLayout = flag;
            ViewBag.IsShip = isShip;
            if (isShip)
            {
                ViewBag.src = "/Alarm/AlarmInfo?flag=false&isShip="+isShip;
                ViewBag.layuithis = "alarm";
                ViewBag.IsLandHome = false;
                ViewBag.ShipName = base.user.ShipName;
            }
            return View();
        }
        public IActionResult AlarmList(string type,bool flag=false,bool isShip=false) 
        {
            string name = "打架";
            if (type == "1") name = "安全帽";
            else if (type == "2") name = "打电话";
            else if (type == "3") name = "睡觉";
            ViewBag.TypeName = name;
            ViewBag.type = type;
            ViewBag.IsShowLayout = flag;
            ViewBag.IsShip = isShip;
            return View();
        }

        /// <summary>
        /// 船舶端查询报警信息
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IActionResult Load(int pageIndex, int pageSize,int type)
        {
            try
            {
                int total = 0;
                var list = GetDate(new SearchAlarmViewModel() { ShipId = base.user.ShipId, Type=type }, pageIndex, pageSize, out total);
                var result = new
                {
                    code = 0,
                    data = list,
                    ship = _context.Ship.ToList(),
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
        /// 导出
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public IActionResult ExportPdf(string shipId,string name,int type,string startTime,string endTime)
        {
            int total = 0;
            SearchAlarmViewModel model = new SearchAlarmViewModel()
            {
                Name = name,
                EndTime = endTime,
                StartTime = startTime,
                Type = type,
                ShipId=shipId
            };
            shipId = string.IsNullOrEmpty(shipId) ? base.user.ShipId : shipId;
            var ship = _context.Ship.Where(c =>c.Id==shipId).FirstOrDefault();
            var list = GetDate(model, 1, 1000000, out total);
            string time = startTime + "~" + endTime; 
            string html =ManagerHelp.GetHtml(list, time,ship.Name);          
            //生成PDF
            var pdfBytes = _PDFService.CreatePDF(html);

            return File(pdfBytes, "application/pdf", "报警信息.pdf");
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
            var ship = _context.Ship.Where(c => (string.IsNullOrEmpty(model.ShipId) ? 1 == 1 : c.Id == model.ShipId)).ToList();
            var shipIds = string.Join(',', ship.Select(c => c.Id));
            var alarmData = _context.Alarm.Where(c => shipIds.Contains(c.ShipId) && (c.Type != Alarm.AlarmType.ATTENDANCE_IN && c.Type != Alarm.AlarmType.ATTENDANCE_OUT) && (model.Type == 0 ? 1 == 1 : c.Type == (Alarm.AlarmType)model.Type));

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
            var camera = _context.Camera.Where(c => (string.IsNullOrEmpty(model.Name) ? 1 == 1 : c.NickName.Contains(model.Name)) && cids.Contains(c.Id)).ToList();
            var ids = string.Join(',', alarm.Select(c => c.Id));
            //查询位置信息
            var pics = _context.AlarmPosition.Where(c => ids.Contains(c.AlarmId)).ToList();
            //组合数据
            var data = from a in alarm
                       join b in camera on a.Cid equals b.Id
                       join c in pics on a.Id equals c.AlarmId
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