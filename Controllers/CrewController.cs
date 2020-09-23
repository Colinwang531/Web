﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Tls;
using ProtoBuf;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.ProtoBuffer;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class CrewController : BaseController
    {
        private readonly MyContext _context;      
        public static Dictionary<string, byte[]> picBytes;
        private SendDataMsg assembly = new SendDataMsg();
        int timeout = 5000;
        public CrewController(MyContext context)
        {
            _context = context;
        }

        // GET: Employees
        public IActionResult Index()
        {
            picBytes = new Dictionary<string, byte[]>();
            ViewBag.IsSet = base.user.EnableConfigure;
            ViewBag.IsLandHome = base.user.IsLandHome;
            return View();
        }
        /// <summary>
        /// 加载船员列表
        /// </summary>
        /// <returns></returns>
        public IActionResult Load(int pageIndex, int pageSize)
        {
            ViewBag.IsLandHome = false;
            if (base.user.IsLandHome&&!ManagerHelp.IsTest)
            {
                ViewBag.IsLandHome = true;
                return LandLoad();
            }
            else
            {
                return Search("",pageIndex,pageSize);
            }            
        }
        public IActionResult Search(string name, int pageIndex, int pageSize)
        {
            string shipId = base.user.ShipId;
            var datacrew = _context.Crew.Where(c => c.ShipId == shipId && string.IsNullOrEmpty(name) ? 1 == 1 : c.Name.Contains(name)).ToList();
            int count = datacrew.Count();
            var data = datacrew.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var ids = string.Join(',', data.Select(c => c.Id));
            var Pics = _context.CrewPicture.Where(c => ids.Contains(c.CrewId)).ToList();
            foreach (var item in data)
            {
                var picW = Pics.Where(c => c.CrewId == item.Id);
                item.employeePictures = new List<CrewPicture>();
                if (picW.Count() > 0)
                {
                    foreach (var pic in picW)
                    {
                        item.employeePictures.Add(pic);
                    }

                }
            }
            var dataShow = from a in data
                           select new
                           {
                               a.Id,
                               a.Job,
                               a.Name,
                               a.ShipId,
                               employeePictures = from b in a.employeePictures
                                                  select new
                                                  {
                                                      b.Id,
                                                      Picture = Convert.ToBase64String(Convert.FromBase64String(Encoding.UTF8.GetString(b.Picture)))
                                                  }
                           };

            var result = new
            {
                code = 0,
                data = dataShow,
                count = count,
                pageIndex=pageIndex,
                pageSize=pageSize,
                isSet = !string.IsNullOrEmpty(shipId) ? base.user.EnableConfigure : false
            };
            return new JsonResult(result);
        }
        /// <summary>
        /// 陆地端查看船员信息
        /// </summary>
        /// <returns></returns>
        private IActionResult LandLoad()
        {
            string shipId = base.user.ShipId;
            assembly.SendCrewQuery(shipId);
            List<ProtoBuffer.Models.CrewInfo> crewInfos = new List<ProtoBuffer.Models.CrewInfo>();
            try
            {
                bool flag = true;
                new TaskFactory().StartNew(() =>
                {
                    while (flag)
                    {
                        if (ManagerHelp.Reponse != "")
                        {
                            crewInfos = JsonConvert.DeserializeObject<List<ProtoBuffer.Models.CrewInfo>>(ManagerHelp.Reponse);
                            flag = false;
                        }
                        Thread.Sleep(500);
                    }
                }).Wait(timeout);
                flag = false;
            }
            catch (Exception)
            {
            }
            ManagerHelp.Reponse = "";
            var dataShow = from a in crewInfos
                           select new
                           {
                               a.job,
                               a.name,
                               id = a.uid,
                               ShipId = shipId,
                               employeePictures = from b in a.pictures
                                                  select new
                                                  {
                                                      id = Encoding.UTF8.GetString(b).Split(',')[0],
                                                      Picture = Convert.ToBase64String(Convert.FromBase64String(Encoding.UTF8.GetString(b).Split(',')[1]))
                                                  }
                           };
            var result = new
            {
                code = 0,
                data = dataShow,
                count= crewInfos.Count(),
                isSet = !string.IsNullOrEmpty(shipId) ? base.user.EnableConfigure : false
            };
            return new JsonResult(result);
        }
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        public IActionResult UpFile()
        {
            try
            {
                #region 文件上传
                var imgFile = Request.Form.Files[0];
                var s = imgFile.OpenReadStream();
                byte[] bytes = new byte[s.Length];
                s.Seek(0, SeekOrigin.Begin);
                s.Read(bytes, 0, bytes.Length);
                //将流加密码为base64的字符串
                string base64 = Convert.ToBase64String(bytes);
                //将加密后的字符串转换为流
                byte[] by = Encoding.UTF8.GetBytes(base64);
                string identity = Guid.NewGuid().ToString();
                picBytes.Add(identity, by);
                var data = new
                {
                    id = identity,
                    name = imgFile.FileName
                };
                return Json(new { code = 0, data = data, msg = "上传成功" });
                #endregion
            }
            catch (Exception ex)
            {
                return Json(new { code = 1, msg = "上传失败" + ex.Message });
            }

        }

        /// <summary>
        /// 保存（新增/修改）
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="job"></param>
        /// <param name="picIds">图片ID</param>
        /// <returns></returns>
        public IActionResult Save(string id, string name, string job, string picIds)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (base.user.ShipId == "")
                    {
                        return new JsonResult(new { code = 1, msg = "船不存在，无法添加数据" });
                    }
                    List<string> ids = new List<string>();
                    if (picIds != null)
                    {
                        ids = picIds.Split(',').ToList();
                    }
                    #region 陆地端添加/修改船员
                    if (base.user.IsLandHome&&!ManagerHelp.IsTest)
                    {
                        string shipId = base.user.ShipId;
                        string identity = GetIdentity();
                        if (identity == "")
                        {
                            return new JsonResult(new { code = 1, msg = "人脸组件未启动" });
                        }
                        ShipWeb.ProtoBuffer.Models.CrewInfo emp = new ShipWeb.ProtoBuffer.Models.CrewInfo()
                        {
                            job = job,
                            name = name,
                            uid = id
                        };
                        int code = 0;
                        //陆地端添加船员
                        if (string.IsNullOrEmpty(id))
                        { //添加图片
                            if (picBytes.Count > 0 && ids.Count > 0)
                            {
                                emp.pictures = new List<byte[]>();
                                foreach (var item in ids)
                                {
                                    if (picBytes.Where(c => c.Key == item).Any())
                                    {
                                        var pic = picBytes.Where(c => c.Key == item).FirstOrDefault();
                                        emp.pictures.Add(pic.Value);
                                    }
                                }
                            }
                            assembly.SendCrewAdd(emp,shipId+":"+identity);
                            code = 0;
                        }
                        else
                        {
                            emp.pictures = new List<byte[]>();
                            foreach (var item in ids)
                            {
                                if (picBytes.Where(c => c.Key == item).Any())
                                {
                                    byte[] value = picBytes.Where(c => c.Key == item).FirstOrDefault().Value;
                                    string pic = Encoding.UTF8.GetString(value);
                                    byte[] by = Encoding.UTF8.GetBytes(item + "," + pic);
                                    emp.pictures.Add(by);
                                }
                                else
                                {
                                    emp.pictures.Add(Encoding.UTF8.GetBytes(item));
                                }
                            }
                            assembly.SendCrewUpdate(emp,shipId+":"+identity);
                            code = 0;
                        }
                        //清除已经上传了的图片
                        foreach (var item in ids)
                        {
                            picBytes.Remove(item);
                        }
                        return new JsonResult(new { code = code, msg = code == 2 ? "船员名称不能重复" : "数据保存失败" });
                    }
                    #endregion

                    Crew employee;
                    var empdb = _context.Crew.FirstOrDefault(c => c.Name == name);
                    if (!string.IsNullOrEmpty(id))
                    {
                        #region 修改船员信息
                        employee = _context.Crew.FirstOrDefault(c => c.Id == id);
                        if (employee == null)
                        {
                            return new JsonResult(new { code = 1, msg = "数据中不存在此数据" });
                        }
                        if (empdb != null && empdb.Name != employee.Name)
                        {
                            return new JsonResult(new { code = 1, msg = "船员名称重复" });
                        }
                        employee.Name = name;
                        employee.Job = job;
                        var picList = _context.CrewPicture.Where(c => c.CrewId == id).ToList();
                        //记录数据库中存在的图片ID
                        List<string> dbIds = new List<string>();
                        foreach (var item in ids)
                        {
                            if (picBytes.Where(c => c.Key == item).Any())
                            {
                                #region 需要添加的图片
                                var pic = picBytes.Where(c => c.Key == item).FirstOrDefault();
                                CrewPicture ep = new CrewPicture()
                                {
                                    CrewId = employee.Id,
                                    Id = pic.Key,
                                    Picture = pic.Value,
                                    ShipId = employee.ShipId
                                };
                                _context.CrewPicture.Add(ep);

                                #endregion
                            }
                            else if (picList.Where(c => c.Id == item).Any())
                            {
                                dbIds.Add(item);
                            }
                        }
                        //查找当前船员下需要删除的图片
                        var delPicList = picList.Where(c => c.CrewId == id && !dbIds.Contains(c.Id)).ToList();
                        if (delPicList.Count > 0)
                        {
                            _context.CrewPicture.RemoveRange(delPicList);
                        }
                        _context.Crew.Update(employee);
                        #endregion
                    }
                    else
                    {
                        #region 添加船员信息
                        if (empdb != null)
                        {
                            return new JsonResult(new { code = 1, msg = "船员名称不能重复" });
                        }
                        string identity = Guid.NewGuid().ToString();
                        employee = new Crew()
                        {
                            Job = job,
                            Name = name,
                            Id = identity,
                            ShipId = base.user.ShipId
                        };
                        //添加图片
                        if (picBytes.Count > 0 && ids.Count > 0)
                        {
                            List<CrewPicture> list = new List<CrewPicture>();
                            foreach (var item in ids)
                            {
                                if (picBytes.Where(c => c.Key == item).Any())
                                {
                                    CrewPicture pic = new CrewPicture()
                                    {
                                        Id = item,
                                        CrewId = identity,
                                        ShipId = base.user.ShipId,
                                        Picture = picBytes.Where(c => c.Key == item).FirstOrDefault().Value
                                    };
                                    list.Add(pic);
                                }
                            }
                            employee.employeePictures = list;
                        }
                        _context.Add(employee);
                        #endregion
                    }
                    _context.SaveChangesAsync();
                    //清除已经上传了的图片
                    foreach (var item in ids)
                    {
                        picBytes.Remove(item);
                    }
                    if (!ManagerHelp.IsTest)
                    {
                        string identity = GetIdentity();
                        if (identity == "")
                        {
                            return new JsonResult(new { code = 1, msg = "人脸组件未启动" });
                        }
                        //发送netmq消息
                        List<byte[]> bytes = new List<byte[]>();
                        foreach (var item in employee.employeePictures)
                        {
                            bytes.Add(item.Picture);
                        }
                        ProtoBuffer.Models.CrewInfo crewInfo = new ProtoBuffer.Models.CrewInfo()
                        {
                            job = employee.Job,
                            name = employee.Name,
                            uid = employee.Id,
                            pictures = bytes
                        };
                        if (!string.IsNullOrEmpty(id)) { assembly.SendCrewUpdate(crewInfo, identity); }
                        else { assembly.SendCrewAdd(crewInfo, identity); }
                    }
                }
                return new JsonResult(new { code = 0 });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "保存失败!" + ex.Message });
            }
        }

        /// <summary>
        /// 删除界面上的图片
        /// </summary>
        /// <returns></returns>
        public IActionResult DeleteImg()
        {
            picBytes = new Dictionary<string, byte[]>();
            return Json(new { code = 0 });
        }
        /// <summary>
        /// 删除船员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Delete(string id)
        {
            try
            {
                //陆地端远程删除船员
                if (base.user.IsLandHome&&!ManagerHelp.IsTest)
                {
                    if (id == null)
                    {
                        return NotFound();
                    }
                    string shipId = base.user.Id;
                    string identity=GetIdentity();
                    if (identity=="")
                    {
                        return new JsonResult(new { code = 1, msg = "人脸组件未启动" });
                    }
                    assembly.SendCrewDelete(id,shipId+":"+identity);
                    return new JsonResult(new { code = 0, msg = "删除成功!" });
                }
                else
                {
                    if (id == null)return NotFound();
                    var employee = _context.Crew.Find(id);
                    if (employee == null) return NotFound();
                    var employeePictures = _context.CrewPicture.Where(e => e.CrewId == employee.Id).ToList();
                    if (employeePictures.Count() > 0)
                    {
                        //删除船员图片
                        _context.CrewPicture.RemoveRange(employeePictures);
                    }
                    if (!ManagerHelp.IsTest)
                    {
                        string identity = GetIdentity();
                        if (identity == "")
                        {
                            return new JsonResult(new { code = 1, msg = "人脸组件未启动" });
                        }
                        assembly.SendCrewDelete(identity);
                        //删除船员
                        _context.Crew.Remove(employee);
                        _context.SaveChanges();
                    }
                    else
                    { //删除船员
                        _context.Crew.Remove(employee);
                        _context.SaveChanges();
                    }
                    return new JsonResult(new { code = 0, msg = "删除成功!" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "删除失败!" + ex.Message });
            }
        }

        /// <summary>
        /// 获取船员的通讯ID
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        private string GetIdentity()
        {
            if (base.user.IsLandHome)
            {
                string tokenstr = HttpContext.Session.GetString("comtoken");
                List<ComponentToken> tokens = JsonConvert.DeserializeObject<List<ComponentToken>>(tokenstr);
                var component = tokens.FirstOrDefault(c => c.Type == ComponentType.AI&&c.Name==ManagerHelp.FaceName);
                if (component != null)
                {
                    return component.CommId;
                }
            }
            else
            {
                //获取设备的组件ID
                var component = _context.Component.FirstOrDefault(c => c.Type == ComponentType.AI && c.Name == ManagerHelp.FaceName);
                if (component != null)
                {
                    return component.CommId;
                }
            }
            return "";
        }
    }
}
