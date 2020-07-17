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
                ManagerHelp.ShipId = shipid;
            }
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
                //取出报警表中指定条数的数据
                var alarm = (from a in _context.Alarm
                             join b in _context.AlarmInformation on a.Id equals b.AlarmId
                             join c in _context.Algorithm on b.Cid equals c.Cid
                             where b.Type!=AlarmType.ATTENDANCE_IN&&b.Type!=AlarmType.ATTENDANCE_OUT && a.ShipId == ManagerHelp.ShipId 
                             select new
                             {
                                 b.Id,
                                 a.Time,
                                 a.Picture,
                                 b.Type,
                                 b.Cid,
                                 a.ShipId
                             }).ToList();
                //获取报警信息表ID
                var infoIds = string.Join(',', alarm.Select(c => c.Id));
                ///获取报像机ID
                var cids = string.Join(',', alarm.Select(c => c.Cid));
                var camera = _context.Camera.Where(c => cids.Contains(c.Id)).ToList();
                //给报警图片加上位置
                var pics = _context.AlarmInformationPosition.Where(c => infoIds.Contains(c.AlarmInformationId)).ToList();
                //组合数据
                var dataPage = (from a in alarm
                           join b in pics on a.Id equals b.AlarmInformationId
                           join c in camera on a.Cid equals c.Id
                           select new
                           {
                               Time = a.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                               c.Id,
                               c.NickName,
                               a.Type,
                               Picture = Convert.FromBase64String(Encoding.UTF8.GetString(a.Picture)),
                               b.X,
                               b.Y,
                               b.W,
                               b.H
                           }).Skip((pageIndex-1)*pageSize).Take(pageSize);

                //查询总条数
                var total = from a in alarm
                            join b in camera on a.Cid equals b.Id
                            select new
                            {
                                a.Id
                            };
                var result = new
                {
                    code = 0,
                    data = dataPage,
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    count = total.Count()
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
                            join c in _context.Algorithm on b.Cid equals c.Id
                            where (type > 0 ? b.Type == (AlarmType)type : 1 == 1) && a.ShipId == ManagerHelp.ShipId && b.Type != AlarmType.ATTENDANCE_IN&&b.Type!=AlarmType.ATTENDANCE_OUT
                           select new
                           {
                               a.Time,
                               a.Picture,
                               b.Type,
                               b.Cid,
                               b.Id
                           };
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
                var alarm = data.ToList();
                //获取报警信息表ID
                var infoIds = string.Join(',', data.Select(c => c.Id));
                ///获取报像机ID
                var cids = string.Join(',', data.Select(c => c.Cid));
                //给报警图片加上位置
                var pics = _context.AlarmInformationPosition.Where(c => infoIds.Contains(c.AlarmInformationId)).ToList();
                //查询摄像机信息
                var camera = _context.Camera.Where(c => (!string.IsNullOrEmpty(cid) ? c.Id == cid : 1 == 1) &&
                                    (!string.IsNullOrEmpty(name) ? c.NickName.Contains(name) : 1 == 1) && cids.Contains(c.Id)).ToList();

                //组合数据
                var pageList = (from a in alarm
                                join b in pics on a.Id equals b.AlarmInformationId
                               join c in camera on a.Cid equals c.Id
                               select new
                               {
                                   Time = a.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                                   a.Type,
                                   a.Cid,
                                   c.NickName,
                                   Picture = Convert.FromBase64String(Encoding.UTF8.GetString(a.Picture)),
                                   b.X,
                                   b.Y,
                                   b.W,
                                   b.H
                               }).Skip((pageIndex-1)*pageSize).Take(pageSize);
                //查询总条数
                var total = from a in alarm
                            join b in camera on a.Cid equals b.Id
                            select new
                            {
                                a.Id
                            };                
                int count = total.Count();
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
                //取出报警表中指定条数的数据
                var alarm = (from a in _context.Alarm
                             join b in _context.AlarmInformation on a.Id equals b.AlarmId
                             join c in _context.Algorithm on b.Cid equals c.Cid
                             where b.Type!=AlarmType.ATTENDANCE_IN&&b.Type!=AlarmType.ATTENDANCE_OUT
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
                var camera = _context.Camera.Where(c => cids.Contains(c.Id)).ToList();
                var ship = _context.Ship.Where(c => shipIds.Contains(c.Id)).ToList();

                var pics = _context.AlarmInformationPosition.Where(c => infoIds.Contains(c.AlarmInformationId)).ToList();
                var pageList = (from a in alarm
                            join b in pics on a.Id equals b.AlarmInformationId
                            join c in camera on a.Cid equals c.Id
                            join e in ship on a.ShipId equals e.Id
                            select new
                            {
                                e.Name,
                                Time = a.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                                a.Cid,
                                a.Type,
                                c.NickName,
                                Picture = Convert.FromBase64String(Encoding.UTF8.GetString(a.Picture)),
                                b.X,
                                b.Y,
                                b.W,
                                b.H
                            }).Skip((pageIndex-1)*pageSize).Take(pageSize);
                var total = from a in alarm
                            join b in camera on a.Cid equals b.Id
                            select new
                            {
                                a.Id
                            };
                var result = new
                {
                    code = 0,
                    data = pageList,
                    ship = _context.Ship,
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    count = total.Count()
                };            
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "查询失败" + ex.Message });
            }

        }
        public IActionResult SearchAlarm(string searchModel, int pageIndex, int pageSize)
        {
            var model = JsonConvert.DeserializeObject<SearchAlarmViewModel>(searchModel);
            var data = from a in _context.Alarm
                       join b in _context.AlarmInformation on a.Id equals b.AlarmId
                       join c in _context.Algorithm on b.Cid equals c.Cid
                       join d in _context.Ship on a.ShipId equals d.Id
                       where (model.Type > 0 ? b.Type ==(AlarmType)model.Type : 1 == 1) && (!string.IsNullOrEmpty(model.ShipId) ? d.Id == model.ShipId : 1 == 1) && b.Type != AlarmType.ATTENDANCE_IN&&b.Type!=AlarmType.ATTENDANCE_OUT
                       select new
                       {
                           a.Time,
                           d.Name,
                           a.Picture,
                           b.Type,
                           b.Cid,
                           b.Id
                       };
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
            var alarm = data.ToList();
            ///获取报像机ID
            var cids = string.Join(',', alarm.Select(c => c.Cid));
            var infoIds = string.Join(',', alarm.Select(c => c.Id));
            //查询摄像机信息
            var camera = _context.Camera.Where(c => (!string.IsNullOrEmpty(model.Cid) ? c.Id == model.Cid : 1 == 1) &&
                                (!string.IsNullOrEmpty(model.Name) ? c.NickName.Contains(model.Name) : 1 == 1) && cids.Contains(c.Id)).ToList();

            //给报警图片加上位置
            var pics = _context.AlarmInformationPosition.Where(c => infoIds.Contains(c.AlarmInformationId)).ToList();
            //组合数据
            var pageList = (from a in alarm
                            join b in pics on a.Id equals b.AlarmInformationId
                            join c in camera on a.Cid equals c.Id
                            select new
                            {
                                Time=a.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                                a.Name,
                                c.Id,
                                c.NickName,
                                a.Type,
                                Picture = Convert.FromBase64String(Encoding.UTF8.GetString(a.Picture)) ,
                                b.X,
                                b.Y,
                                b.W,
                                b.H
                            }).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var total = from a in alarm
                        join b in camera on a.Cid equals b.Id
                        select new
                        {
                            a.Id
                        };
            var result = new
            {
                code = 0,
                data = pageList,
                pageIndex = pageIndex,
                pageSize = pageSize,
                count = total.Count()
            };
            return new JsonResult(result);
        }
    }
}