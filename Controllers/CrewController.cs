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
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Hosting;
using NetMQ;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Tls;
using ProtoBuf;
using Smartweb.Hubs;
using SmartWeb.DB;
using SmartWeb.Models;
using SmartWeb.ProtoBuffer;
using SmartWeb.Tool;

namespace SmartWeb.Controllers
{
    public class CrewController : BaseController
    {
        private readonly MyContext _context;      
        public static Dictionary<string, byte[]> picBytes;
        private SendDataMsg assembly = null;
        private static List<CrewViewModel> crewVMList;
        int timeout = 5000;
        public CrewController(MyContext context)
        {
            _context = context;
            assembly = new SendDataMsg();
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
            if (base.user.IsLandHome)
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
            var datacrew = _context.Crew.Where(c =>  string.IsNullOrEmpty(name) ? 1 == 1 : c.Name.Contains(name)).ToList();
            int count = datacrew.Count();
            var data = datacrew.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            var Pics = _context.CrewPicture.ToList();
            crewVMList = new List<CrewViewModel>();
            foreach (var item in data)
            {
                CrewViewModel model = new CrewViewModel()
                {
                    Id = item.Id,
                    Job = item.Job,
                    Name = item.Name,
                    crewPictureViewModels = new List<CrewPictureViewModel>()
                };
                var picW = Pics.Where(c => c.CrewId == item.Id);
                foreach (var pic in picW)
                {
                    CrewPictureViewModel vmpic = new CrewPictureViewModel()
                    {
                        Id = pic.Id,
                        Picture = Convert.ToBase64String(pic.Picture)
                    };
                    model.crewPictureViewModels.Add(vmpic);
                }
                crewVMList.Add(model);
            }
            var result = new
            {
                code = 0,
                data = crewVMList,
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
            //XMQ的组件ID
            string XMQComId = base.user.ShipId;
            string tokenstr = HttpContext.Session.GetString("comtoken");
            //获取XMQ组件里的WEB组件ID
            string webIdentity = ManagerHelp.GetLandToId(tokenstr);
            assembly.SendCrewQuery(XMQComId+":"+ webIdentity);
            List<ProtoBuffer.Models.CrewInfo> crewInfos = new List<ProtoBuffer.Models.CrewInfo>();
            try
            {
                bool flag = true;
                new TaskFactory().StartNew(() =>
                {
                    while (flag)
                    {
                        if (ManagerHelp.CrewReponse != "")
                        {
                            crewInfos = JsonConvert.DeserializeObject<List<ProtoBuffer.Models.CrewInfo>>(ManagerHelp.CrewReponse);
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
            ManagerHelp.CrewReponse = "";
            crewVMList = new List<CrewViewModel>();
            foreach (var item in crewInfos)
            {
                CrewViewModel model = new CrewViewModel()
                {
                    Id = Convert.ToInt32(item.uid),
                    Job = item.job,
                    Name = item.name,
                    crewPictureViewModels = new List<CrewPictureViewModel>()
                };
                if (item.pictures != null)
                {
                foreach (var pic in item.pictures)
                {
                    CrewPictureViewModel vm = new CrewPictureViewModel()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Picture = pic
                    };
                    model.crewPictureViewModels.Add(vm);
                    }
                }
                crewVMList.Add(model);
            }
            var result = new
            {
                code = 0,
                data = crewVMList,
                count = crewInfos.Count(),
                isSet = !string.IsNullOrEmpty(XMQComId) ? base.user.EnableConfigure : false
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
                string identity = Guid.NewGuid().ToString();
                picBytes.Add(identity, bytes);
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
        public IActionResult Save(int id, string name, string job, string picIds)
        {
            try
            {
                if (base.user.ShipId == "")
                {
                    return new JsonResult(new { code = 1, msg = "船不存在，无法添加数据" });
                }
                int code = 1;
                string errMsg = "";
                List<string> ids = new List<string>();
                if (picIds != null) ids = picIds.Split(',').ToList();
                if (base.user.IsLandHome)
                {
                    #region 陆地端添加/修改船员
                    //xmq的组件ID
                    string XMQComId = base.user.ShipId;
                    string tokenstr = HttpContext.Session.GetString("comtoken");
                    string identity = ManagerHelp.GetLandToId(tokenstr);
                    if (identity == "")
                    {
                        return new JsonResult(new { code = 1, msg = "当前船舶已失联，请重新连接" });
                    }
                    string algoComId = ManagerHelp.GetLandToId(tokenstr, ComponentType.AI, ManagerHelp.FaceName);
                    if (string.IsNullOrEmpty(algoComId))
                    {
                        return new JsonResult(new { code = 1, msg = "人脸组件未启动" });
                    }
                    ProtoBuffer.Models.CrewInfo emp = GetCrewInfo(id, name, job, ids);
                    if (id > 0)assembly.SendCrewUpdate(emp, XMQComId + ":" + identity);
                    else assembly.SendCrewAdd(emp, XMQComId + ":" + identity);
                    code = GetResult();
                    if (code == 2) errMsg = "船员名称不能重复";
                    if (code == 400) errMsg = "网络请求超时。。。";
                    else if (code != 0) errMsg = "船员信息保存失败";
                    //清除已经上传了的图片
                    foreach (var item in ids)
                    {
                        picBytes.Remove(item);
                    }
                    return new JsonResult(new { code = code, msg = code == 2 ? "船员名称不能重复" : "数据保存失败" });
                    #endregion
                }
                else
                {
                    Crew employee = new Crew();
                    if (!CheckData(id, name, ref employee, ref errMsg))
                    {
                        return new JsonResult(new { code = 1, msg = errMsg });
                    }
                    string identity = ManagerHelp.GetShipToId(ComponentType.AI,ManagerHelp.FaceName);
                    if (identity == "")
                    {
                        return new JsonResult(new { code = 1, msg = "人脸组件未启动" });
                    }
                    //增加或修改
                    AddOrUpdate(id, name, job, ids,ref employee);
                    //发送netmq消息
                    var dbPic = _context.CrewPicture.Where(c => c.CrewId == employee.Id).ToList();
                    List<string> bytes = new List<string>();
                    foreach (var item in dbPic)
                    {
                        bytes.Add(Convert.ToBase64String(item.Picture));
                    }
                    ProtoBuffer.Models.CrewInfo crewInfo = new ProtoBuffer.Models.CrewInfo()
                    {
                        job = employee.Job,
                        name = employee.Name,
                        uid = employee.Id.ToString(),
                        pictures = bytes
                    };
                    //if (id > 0) { assembly.SendCrewUpdate(crewInfo, identity); }
                    //else { assembly.SendCrewAdd(crewInfo, identity); }
                    assembly.SendCrewAdd(crewInfo, identity);
                    code = GetResult();
                    if (code == 400) errMsg = "网络请求超时。。。";
                    else if (code != 0) errMsg = "船员信息保存失败";

                }
                return new JsonResult(new { code = code, msg = errMsg });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "保存失败!" + ex.Message });
            }
        }
        /// <summary>
        /// 添加或修改船员
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="job"></param>
        /// <param name="ids"></param>
        /// <param name="employee"></param>
        private void AddOrUpdate(int id, string name, string job, List<string> ids,ref Crew employee)
        {
            employee.Name = name;
            employee.Job = job;
            #region 图片处理
            employee.employeePictures = new List<CrewPicture>();
            List<CrewPictureViewModel> vmPicList = new List<CrewPictureViewModel>();
            var crew = crewVMList.FirstOrDefault(c => c.Id == id);
            if (crew != null) vmPicList = crew.crewPictureViewModels;
            //记录数据库中存在的图片ID
            List<string> dbIds = new List<string>();
            foreach (var item in ids)
            {
                if (picBytes.Where(c => c.Key == item).Any())
                {
                    var pic = picBytes.Where(c => c.Key == item).FirstOrDefault();
                    CrewPicture ep = new CrewPicture()
                    {
                        CrewId = employee.Id,
                        Id = pic.Key,
                        Picture = pic.Value
                    };
                    employee.employeePictures.Add(ep);
                }
                else if (vmPicList.Where(c => c.Id == item).Any())
                {
                    dbIds.Add(item);
                }
            }
            if (dbIds.Count > 0 || (dbIds.Count == 0 && vmPicList.Count > 0))
            {
                var delPicList = _context.CrewPicture.Where(c => c.CrewId == id && !dbIds.Contains(c.Id)).ToList();
                if (delPicList.Count > 0)
                {
                    _context.CrewPicture.RemoveRange(delPicList);
                }
            }
            #endregion
            if (id > 0) _context.Crew.Update(employee);
            else
            {
                _context.Crew.Add(employee);
            }
            //清除已经上传了的图片
            foreach (var item in ids)
            {
                picBytes.Remove(item);
            }
            _context.SaveChanges();
        }

        /// <summary>
        /// 数据校验
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="employee"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        private bool CheckData(int id, string name,ref Crew employee, ref string errMsg)
        {
            var empdb = _context.Crew.FirstOrDefault(c => c.Name == name);
            if (id>0)
            {
                employee = _context.Crew.FirstOrDefault(c => c.Id == id);
                if (employee == null)
                {
                    errMsg = "数据中不存在此数据";
                    return false;
                }
                if (empdb != null && empdb.Name != employee.Name)
                {
                    errMsg = "船员名称重复";
                    return false;
                }
            }
            else
            {
                if (empdb != null)
                {
                    errMsg = "船员名称不能重复";
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 组合protobuf消息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="job"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        private SmartWeb.ProtoBuffer.Models.CrewInfo GetCrewInfo(int id,string name,string job, List<string> ids) 
        {
            ProtoBuffer.Models.CrewInfo emp = new ProtoBuffer.Models.CrewInfo()
            {
                job = job,
                name = name,
                uid = id.ToString()
            };
            emp.pictures = new List<string>();
            List<CrewPictureViewModel> vmPicList = new List<CrewPictureViewModel>();
            if (id>0)
            {
                var crew = crewVMList.FirstOrDefault(c => c.Id == id);
                vmPicList = crew.crewPictureViewModels;
            }
            foreach (var item in ids)
            {
                if (picBytes.Where(c => c.Key == item).Any())
                {
                    byte[] value = picBytes.Where(c => c.Key == item).FirstOrDefault().Value;
                    string pic = Convert.ToBase64String(value);
                    emp.pictures.Add(pic);
                }
                else if (vmPicList.Where(c => c.Id == item).Any())
                {
                    var pic = vmPicList.FirstOrDefault(c => c.Id == item).Picture;
                    emp.pictures.Add(pic);
                }
            }
            return emp;
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
        public IActionResult Delete(int id)
        {
            try
            {
                int code = 1;
                string errMsg = "";
                //陆地端远程删除船员
                if (base.user.IsLandHome)
                {
                    string XMQComId = base.user.ShipId;
                    string tokenstr = HttpContext.Session.GetString("comtoken");
                    string identity = ManagerHelp.GetLandToId(tokenstr);
                    if (identity == "")
                    {
                        return new JsonResult(new { code = 1, msg = "人脸组件未启动" });
                    }
                    assembly.SendCrewDelete(id, XMQComId + ":" + identity);
                    code = GetResult();
                    if (code == 400) errMsg = "网络请求超时。。。";
                    else if (code != 0) errMsg = "船员删除失败";
                }
                else
                {
                    var employee = _context.Crew.Find(id);
                    if (employee == null) return NotFound();
                    var employeePictures = _context.CrewPicture.Where(e => e.CrewId == employee.Id).ToList();
                    if (employeePictures.Count() > 0)
                    {
                        //删除船员图片
                        _context.CrewPicture.RemoveRange(employeePictures);
                    }
                    string identity = ManagerHelp.GetShipToId(ComponentType.AI,ManagerHelp.FaceName);
                    if (identity == "")
                    {
                        return new JsonResult(new { code = 1, msg = "人脸组件未启动" });
                    }
                    assembly.SendCrewDelete(id, identity);
                    //删除船员
                    _context.Crew.Remove(employee);
                    _context.SaveChanges();
                    code = 0;
                }
                return new JsonResult(new { code = code, msg = errMsg });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "删除失败!" + ex.Message });
            }
        }
        /// <summary>
        /// 获取返回状态
        /// </summary>
        /// <returns></returns>
        private int GetResult()
        {
            int result = 1;
            try
            {
                bool flag = true;
                new TaskFactory().StartNew(() =>
                {
                    while (flag && ManagerHelp.CrewResult == "")
                    {
                        Thread.Sleep(100);
                    }
                }).Wait(timeout);
                flag = false;
                if (ManagerHelp.CrewResult != "")
                {
                    result = Convert.ToInt32(ManagerHelp.CrewResult);
                    ManagerHelp.CrewResult = "";
                }
                else
                {
                    result = 400;
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }
    }
}
