using System;
using System.Collections.Generic;
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
        public IActionResult Load(int type = 1)
        {
            var data = from a in _context.Alarm
                       join b in _context.AlarmInformation on a.Id equals b.AlarmId
                       join c in _context.Camera on b.Cid equals c.Cid
                       into d
                       from ss in d.DefaultIfEmpty()
                       where b.Type == type
                       select new
                       {
                           a.Picture,
                           a.Time,
                           b.Cid,
                           ss.NickName,
                           b.Id,
                           b.Type
                       };
            return new JsonResult(new { code = 0, data =data.ToList()});
            //if (data.ToList().Count>0)
            //{
            //    string id = data.ToList()[0].Id;
            //    string pic = Convert.ToBase64String(Convert.FromBase64String(Encoding.UTF8.GetString(data.ToList()[0].Picture)));
            //    var podata=_context.AlarmInformationPosition.Where(c => c.AlarmInformationId==id);
            //    var position = from a in podata
            //                   select new
            //                   {
            //                       a.W,
            //                       a.H,
            //                       a.X,
            //                       a.Y,
            //                       Picture = pic
            //                   };
            //    return new JsonResult(new { code = 0, data = new { alarms = data.ToList(), position = position.ToList() } });
            //}
            return new JsonResult(new { code = 1, msg = "查询失败" });
        }
        /// <summary>
        /// 分页获取图片座标位置
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="type"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageCount"></param>
        public IActionResult Query(string cid, int type, int pageIndex, int pageSize, int pageTotal) 
        {
            try
            {
                var aa = from a in _context.AlarmInformationPosition
                         join b in _context.AlarmInformation on a.AlarmInformationId equals b.Id
                         join c in _context.Alarm on b.AlarmId equals c.Id
                         into ss
                         from su in ss.DefaultIfEmpty()
                         where b.Type == type && b.Cid == cid                         
                         select new
                         {
                             a.H,
                             a.W,
                             a.X,
                             a.Y,
                             su.Picture
                         };
                int count = aa.ToList().Count;
                var dataPage = aa.ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize);
                pageTotal = count % pageSize == 0 ? count / pageSize : (count / pageSize) + 1;

                var position = from a in dataPage
                               select new
                               {
                                   w=ImgZoom(a.Picture,a.W,0),
                                   h= ImgZoom(a.Picture,0,a.H),
                                   a.X,
                                   a.Y,
                                   Picture =  Convert.ToBase64String(Convert.FromBase64String(Encoding.UTF8.GetString(a.Picture)))
                               };
                var result = new
                {
                    code = 0,
                    data= position.ToList(),
                    pageIndex=pageIndex,
                    pageSize=pageSize,
                    pageTotal=pageTotal==0?1: pageTotal
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
            byte[] imageBytes = Convert.FromBase64String(Encoding.UTF8.GetString(bytes));
            //  转成图片
            Image<Rgba32> image = Image.Load(imageBytes);
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