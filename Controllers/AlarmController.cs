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
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class AlarmController : Controller
    {
        private MyContext _context;
        public AlarmController(MyContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Load(int pageIndex, int pageSize)
        {
            try
            {
                //查询摄像机名称及其报警类型
                var data = from a in _context.Alarm
                           join b in _context.AlarmInformation on a.Id equals b.AlarmId
                           join c in _context.Camera on b.Cid equals c.Cid
                           join d in _context.AlarmInformationPosition on b.Id equals d.AlarmInformationId
                           where b.Type!=5                           
                           select new
                           {
                               a.Time,
                               Picture = ManagerHelp.DrawAlarm(a.Picture,d.X,d.Y,d.W,d.H),
                               b.Cid,
                               b.Type,
                               c.NickName,
                               b.Id,
                               b.AlarmId
                           };
                var datalist = data.ToList();
                var dataPage = datalist.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                int count = datalist.Count;
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
                           where b.Type!=5
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
                var list = data.ToList();
                int count = list.Count();
                var pageList = list.Skip((pageIndex - 1) * pageSize).Take(pageSize);
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
        ///// <summary>
        ///// 给报警图片画位置
        ///// </summary>
        ///// <param name="bytes">已经做了转换的字节流</param>
        ///// <param name="position"></param>
        ///// <returns></returns>
        //public byte[] DrawAlarm(byte[] bytes, AlarmInformationPosition position)
        //{
        //    using (var stream = new MemoryStream(bytes, 0, bytes.Length, false, true))
        //    {
        //        Image image = Image.FromStream(stream);
        //        Graphics.FromImage(image).DrawRectangle(new Pen(Brushes.Red,5), position.X, position.Y, position.W, position.H);
        //        var ms = new MemoryStream();
        //        image.Save(ms, ImageFormat.Png);
        //        return ms.GetBuffer();
        //    }
        //}
       
    }
}