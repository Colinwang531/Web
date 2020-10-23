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
using SmartWeb.DB;
using SmartWeb.Models;
using SmartWeb.Tool;
using System.Text;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Diagnostics;
using DinkToPdf;
using DinkToPdf.Contracts;
using SmartWeb.Interface;
using Newtonsoft.Json.Schema;

namespace SmartWeb.Controllers
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
            ViewBag.IsShowLayout = flag;
            ViewBag.IsShip = isShip;
            if (isShip)
            {
                string shipId = base.user.ShipId;
                ViewBag.src = "/Alarm/AlarmInfo?flag=false&isShip="+isShip;
                ViewBag.layuithis = "alarm";
                ViewBag.IsLandHome = false;
                ViewBag.ShipName = base.user.ShipName;
                ViewBag.sleep = _context.Alarm.Where(c => c.Type == Alarm.AlarmType.SLEEP&&c.ShipId== shipId).Count();
                ViewBag.fight = _context.Alarm.Where(c => c.Type == Alarm.AlarmType.FIGHT && c.ShipId == shipId).Count();
                ViewBag.helmet = _context.Alarm.Where(c => c.Type == Alarm.AlarmType.HELMET && c.ShipId == shipId).Count();
                ViewBag.phone = _context.Alarm.Where(c => c.Type == Alarm.AlarmType.PHONE && c.ShipId == shipId).Count();
            }
            else
            {
                ViewBag.sleep = _context.Alarm.Where(c => c.Type == Alarm.AlarmType.SLEEP).Count();
                ViewBag.fight = _context.Alarm.Where(c => c.Type == Alarm.AlarmType.FIGHT).Count();
                ViewBag.helmet = _context.Alarm.Where(c => c.Type == Alarm.AlarmType.HELMET).Count();
                ViewBag.phone = _context.Alarm.Where(c => c.Type == Alarm.AlarmType.PHONE).Count();
            }
            ManagerHelp.isInit = false;
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
            if (!base.user.IsLandHome)
            {
                shipId = base.user.ShipId;
                model.ShipId = base.user.ShipId;
            }
            var ship = _context.Ship.Where(c =>c.Id==shipId).FirstOrDefault();
            var list = GetDate(model, 1, 1000000, out total);
            string time = startTime + "~" + endTime;
            var address=HttpContext.Request.Scheme + "://" + HttpContext.Request.Host;
            string html =ManagerHelp.GetHtml(list, time,ship.Name, address);          
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
            if (!base.user.IsLandHome)
            {
                model.ShipId = base.user.ShipId;
            }
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
            else if (!string.IsNullOrEmpty(model.Name)) 
            {
                alarmData = alarmData.Where(c => c.Cname.Contains(model.Name));
            }
            var alarm = alarmData.OrderByDescending(c=>c.CreatDate).ToList();
            //组合数据
            var data = from a in alarm
                       join d in ship on a.ShipId equals d.Id
                       select new
                       {
                           a.Time,
                           a.Id,
                           d.Name,
                           NickName=a.Cname,
                           a.Type,
                           a.Picture
                       };
            total = data.Count();
            var datapage = data.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var ids = string.Join(',', datapage.Select(c => c.Id));
            //查询位置信息
            var pics = _context.AlarmPosition.Where(c => ids.Contains(c.AlarmId)).ToList();
            foreach (var item in datapage)
            {
                string picture =  Convert.ToBase64String(item.Picture);
                AlarmViewModel avm = new AlarmViewModel()
                {
                    Id = item.Id,
                    Name = item.Name,
                    NickName = item.NickName,
                    Picture = picture,
                    Type = (int)item.Type,
                    Time = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                };
                if (pics!=null&&pics.Count>0)
                {
                    if (pics.Where(c=>c.AlarmId==item.Id).Any())
                    {
                        var pic = pics.FirstOrDefault(c => c.AlarmId == item.Id);
                        avm.H = pic.H;
                        avm.W = pic.W;
                        avm.X = pic.X;
                        avm.Y = pic.Y;
                    }
                }
                list.Add(avm);
            }
            return list;
        }
    }
}