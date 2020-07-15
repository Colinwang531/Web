using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Org.BouncyCastle.Crypto.Tls;
using ProtoBuf;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.ProtoBuffer;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class EmployeesController : BaseController
    {

        private readonly MyContext _context;
        private ProtoManager manager;
        public static Dictionary<string,byte[]> picBytes;
        public EmployeesController(MyContext context)
        {
            _context = context;
            manager = new ProtoManager();
        }

        // GET: Employees
        public IActionResult Index()
        {
            picBytes = new Dictionary<string, byte[]>();
            return View();
        }
        /// <summary>
        /// 加载船员列表
        /// </summary>
        /// <returns></returns>
        public IActionResult Load()
        {
            if (ManagerHelp.IsShowLandHome)
            {
                return LandLoad();
            }

            var data = _context.Employee.Where(c=>c.ShipId==ManagerHelp.ShipId).ToList();
            var ids = string.Join(',', data.Select(c => c.Id));
            var Pics=_context.EmployeePicture.Where(c => ids.Contains(c.EmployeeId)).ToList();
            foreach (var item in data)
            {
                var picW = Pics.Where(c =>c.EmployeeId== item.Id);
                item.employeePictures = new List<EmployeePicture>();
                if (picW.Count()>0)
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
                               a.Uid,
                               a.Name,
                               a.ShipId,
                               employeePictures =from b in a.employeePictures
                                                   select new {
                                                       b.Id,
                                                       Picture=Convert.ToBase64String(Convert.FromBase64String(Encoding.UTF8.GetString(b.Picture)))
                                                   }
                           };

            var result = new
            {
                code = 0,
                data = dataShow,
                isSet = !string.IsNullOrEmpty(ManagerHelp.ShipId) ? ManagerHelp.IsSet:false
            };
            return new JsonResult(result);
        }
        /// <summary>
        /// 陆地端查看船员信息
        /// </summary>
        /// <returns></returns>
        private IActionResult LandLoad()
        {
            var data=manager.CrewQuery(ManagerHelp.ShipId);
            var dataShow = from a in data
                           select new
                           {
                               a.job,
                               a.name,
                               id=a.uid.Split(',')[0],
                               uid=a.uid.Split(',')[1],
                               ShipId=ManagerHelp.ShipId,
                               employeePictures = from b in a.pictures
                                                  select new
                                                  {
                                                      id= Encoding.UTF8.GetString(b).Split(',')[0],
                                                      Picture = Convert.ToBase64String(Convert.FromBase64String(Encoding.UTF8.GetString(b).Split(',')[1]))
                                                  }
                           };
            var result = new
            {
                code = 0,
                data = dataShow,
                isSet = !string.IsNullOrEmpty(ManagerHelp.ShipId) ? ManagerHelp.IsSet : false
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
                return Json(new { code =0,data=identity, msg = "上传成功" });
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
        public IActionResult Save(string id,string uid,string name,string job,string picIds)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    List<string> ids = new List<string>();
                    if (picIds != null)
                    {
                        ids = picIds.Split(',').ToList();
                    }
                    #region 陆地端添加/修改船员
                    if (ManagerHelp.IsShowLandHome)
                    {
                        ShipWeb.ProtoBuffer.Models.Employee emp = new ShipWeb.ProtoBuffer.Models.Employee()
                        {
                            job = job,
                            name = name
                        };
                        
                        int code = 0;
                        //陆地端添加船员
                        if (string.IsNullOrEmpty(uid))
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
                            string result = manager.CrewAdd(emp, ManagerHelp.ShipId);
                            code = result != "" ? 0 : 1;
                        }
                        else
                        {
                            if (picBytes.Count > 0 && ids.Count > 0)
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
                            }
                            int result = manager.CrewUpdate(emp, uid, ManagerHelp.ShipId);
                            code = result;
                        }
                        //清除已经上传了的图片
                        foreach (var item in ids)
                        {
                            picBytes.Remove(item);
                        }
                        return new JsonResult(new { code = 0 });
                    }
                    #endregion

                    Employee employee;
                    var empdb = _context.Employee.FirstOrDefault(c => c.Name == name);
                    if (!string.IsNullOrEmpty(id))
                    {
                        #region 修改船员信息
                        employee = _context.Employee.FirstOrDefault(c => c.Id == id);
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
                        var picList = _context.EmployeePicture.Where(c => c.EmployeeId == id).ToList();
                        //记录数据库中存在的图片ID
                        List<string> dbIds = new List<string>();
                        foreach (var item in ids)
                        {
                            if (picBytes.Where(c => c.Key == item).Any())
                            {
                                #region 需要添加的图片
                                var pic = picBytes.Where(c => c.Key == item).FirstOrDefault();
                                EmployeePicture ep = new EmployeePicture()
                                {
                                    EmployeeId = employee.Id,
                                    Id = pic.Key,
                                    Picture = pic.Value,
                                    ShipId = employee.ShipId
                                };
                                _context.EmployeePicture.Add(ep);

                                #endregion
                            }
                            else if (picList.Where(c => c.Id == item).Any())
                            {
                                dbIds.Add(item);
                            }
                        }
                        //查找当前船员下需要删除的图片
                        var delPicList = picList.Where(c => c.EmployeeId == id && !dbIds.Contains(c.Id)).ToList();
                        if (delPicList.Count > 0)
                        {
                            _context.EmployeePicture.RemoveRange(picList);
                        }
                        _context.Employee.Update(employee);
                        #endregion
                    }
                    else
                    {
                        #region 添加船员信息
                        if (empdb!=null)
                        {
                            return new JsonResult(new { code = 1, msg = "船员名称不能重复" });
                        }
                        string identity = Guid.NewGuid().ToString();
                        employee = new Employee()
                        {
                            Job = job,
                            Name = name,
                            Id = identity,
                            ShipId = ManagerHelp.ShipId
                        };
                        Random rb = new Random();
                        //测试数据
                        employee.Uid = rb.Next(0001, 9999).ToString(); ;
                        //添加图片
                        if (picBytes.Count > 0 && ids.Count > 0)
                        {
                            List<EmployeePicture> list = new List<EmployeePicture>();
                            foreach (var item in ids)
                            {
                                if (picBytes.Where(c => c.Key == item).Any())
                                {
                                    EmployeePicture pic = new EmployeePicture()
                                    {
                                        Id = item,
                                        EmployeeId = identity,
                                        ShipId = ManagerHelp.ShipId,
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
                }
                return new JsonResult(new { code = 0 });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1 ,msg="添加船员失败"+ex.Message});
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
        public  IActionResult Delete(string id,string uid)
        {
            try
            {
               
                //陆地端远程删除船员
                if (ManagerHelp.IsShowLandHome)
                {
                    if (uid==null)
                    {
                        return NotFound();
                    }
                    manager.CrewDelete(uid, ManagerHelp.ShipId);
                    return new JsonResult(new { code = 0, msg = "删除成功!" });
                }
                if (id == null)
                {
                    return NotFound();
                }
                var employee = _context.Employee.Find(id);
                if (employee == null)
                {
                    return NotFound();
                }
                var employeePictures = _context.EmployeePicture.Where(e => e.EmployeeId == employee.Id).ToList();
                if (employeePictures.Count() > 0)
                {
                    //删除船员图片
                    _context.EmployeePicture.RemoveRange(employeePictures);
                }
                //删除船员
                _context.Employee.Remove(employee);
                _context.SaveChanges();
                return new JsonResult(new { code = 0, msg = "删除成功!" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1, msg = "删除失败!" + ex.Message });
            }
        }

    }
}
