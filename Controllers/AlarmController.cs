using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShipWeb.DB;
using ShipWeb.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

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
        public IActionResult Load(int pageIndex,int pageSize)
        {
            try
            {
                //查询摄像机名称及其报警类型
                var data = from a in _context.Alarm
                           join b in _context.AlarmInformation on a.Id equals b.AlarmId
                           join c in _context.Camera on b.Cid equals c.Cid
                           select new
                           {
                               a.Time,
                               Picture= Convert.FromBase64String(Encoding.UTF8.GetString(a.Picture)),
                               b.Cid,
                               b.Type,
                               c.NickName,
                               a.Id,
                               b.AlarmId
                           };

                var datalist = data.ToList();
                var dataPage = datalist.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                //查询图片是座标位置
                string ids = string.Join(",", datalist.Select(c => c.Id));
                //var positions= _context.AlarmInformationPosition.Where(c => ids.Contains(c.AlarmInformationId));
                //if (positions!=null)
                //{

                //}
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
                return new JsonResult(new { code = 1, msg = "查询失败!"+ex.Message });
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
        public IActionResult QueryPage(string cid,string name, int type,string startTime,string endTime, int pageIndex, int pageSize) 
        {
            try
            {
                var data = from a in _context.Alarm
                           join b in _context.AlarmInformation on a.Id equals b.AlarmId
                           join c in _context.Camera on b.Cid equals c.Cid
                           select new
                           {
                               a.Time,
                               Picture = Convert.FromBase64String(Encoding.UTF8.GetString(a.Picture)),
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
                if (type>0)
                {
                    data = data.Where(c => c.Type==type);
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
                    data= pageList,
                    pageIndex=pageIndex,
                    pageSize=pageSize,
                    count=count
                };
                return new JsonResult(result);
            }
            catch (Exception e)
            {
                return new JsonResult(new { code = 1, msg = "查询失败" });
            }
           
        }
        public int ImgZoom(byte[] bytes,int width,int height)
        {
            //byte[] imageBytes = Convert.FromBase64String(Encoding.UTF8.GetString(bytes));
            
            Image<Rgba32> image = SixLabors.ImageSharp.Image.Load(bytes);

            using (var stream = new MemoryStream(bytes, 0, bytes.Length, false, true))
            {
                System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
            }

            if (width>0)
            {
                double percent = (double)200 / image.Width;
                int w1 = Convert.ToInt32(width * percent);
                return w1;
            }
            else
            {
                double percent = (double)260 / image.Height;
                int y1 = Convert.ToInt32((double)height * percent);
                return y1;
            }
        }
    }
}