using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime.Internal.Transform;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.WindowsAzure.Storage.Blob;
using ProtoBuf;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.ProtoBuffer;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class EmployeesController : Controller
    {

        private readonly MyContext _context;
        private ProtoManager manager;
        public static List<byte[]> bylist;
        public EmployeesController(MyContext context)
        {
            _context = context;
            manager = new ProtoManager();
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            bylist = new List<byte[]>();
            return View(await _context.Employee.ToListAsync());
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
                bylist.Add(by);
                return Json(new { code =0, msg = "上传成功" });
                #endregion
            }
            catch (Exception ex)
            {
                return Json(new { code = 1, msg = "上传失败" + ex.Message });
            }
              
        }
       

        // GET: Employees/Create
        public IActionResult Create()
        {
            bylist = new List<byte[]>();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                string identity = Guid.NewGuid().ToString();
               ShipWeb.ProtoBuffer.Models.Employee emp = new ShipWeb.ProtoBuffer.Models.Employee()
                {
                     job=employee.Job,
                      name=employee.Name
                };
                string uid = "222";//manager.CrewAdd(emp, identity);
                if (uid!="")
                {
                    employee.ShipId = ManagerHelp.ShipId;
                    employee.Uid = uid;
                    employee.Id = identity;
                    //添加图片
                    if (bylist.Count>0)
                    {
                        List<EmployeePicture> list = new List<EmployeePicture>();
                        foreach (var item in bylist)
                        {
                            EmployeePicture pic = new EmployeePicture()
                            {
                                Id = Guid.NewGuid().ToString(),
                                EmployeeId = identity,
                                ShipId = ManagerHelp.ShipId,
                                Picture=item
                            };
                            list.Add(pic);
                        }
                        employee.employeePictures = list;
                    }
                }
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // GET: Employees/Edit/5
        public IActionResult Edit(string id)
        {
            bylist = new List<byte[]>();
            if (id == null)
            {
                return NotFound();
            }
            var employee = _context.Employee.Find(id);
            if (employee == null)
            {
                return NotFound();
            }
            //获取船员图片
            var camList = _context.EmployeePicture.Where(c => c.EmployeeId == id).ToList();
            employee.employeePictures = new List<EmployeePicture>();
            List<string> picUrl = new List<string>();
            foreach (var item in camList)
            {
                bylist.Add(item.Picture);
                ////将流图片转成加密后base64字符串
                string base64 = Encoding.UTF8.GetString(item.Picture);
                ////将加密后的字符串转为流
                byte[] by =Convert.FromBase64String(base64);
                employee.employeePictures.Add(new EmployeePicture()
                {
                    EmployeeId = item.EmployeeId,
                    Id = item.Id,
                    Picture = by,
                    ShipId = item.ShipId
                });

            }
            return View(employee);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id,Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    ShipWeb.ProtoBuffer.Models.Employee emp = new ShipWeb.ProtoBuffer.Models.Employee()
                    {
                        job = employee.Job,
                        name = employee.Name
                    };
                    if (bylist.Count>0)
                    {
                        emp.pictures = bylist;
                        employee.employeePictures = new List<EmployeePicture>();
                        var delepic = _context.EmployeePicture.Where(c => c.EmployeeId == employee.Id).ToList();
                        foreach (var item in delepic)
                        {
                            _context.Remove(item);
                        }
                        foreach (var item in bylist)
                        {
                            EmployeePicture pic = new EmployeePicture()
                            {
                                EmployeeId = employee.Id,
                                Id = Guid.NewGuid().ToString(),
                                Picture = item,
                                ShipId = employee.ShipId
                            };
                            _context.EmployeePicture.Add(pic);
                        }
                    }
                    
                   // manager.CrewUpdate(emp, employee.Uid, employee.Id);
                    _context.Employee.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }
        /// <summary>
        /// 删除界面上的图片
        /// </summary>
        /// <returns></returns>
        public IActionResult DeleteImg()
        {
            bylist = new List<byte[]>();
            return Json(new { code = 0 });
        }
        /// <summary>
        /// 删除船员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public  IActionResult Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee =  _context.Employee.Find(id);
            if (employee == null)
            {
                return NotFound();
            }
            ProtoManager manager = new ProtoManager();
            int result=manager.CrewDelete(employee.Uid, employee.Id);
            if (result==0)
            {
               var employeePictures= _context.EmployeePicture.Where(e => e.EmployeeId == employee.Id).ToList();
                foreach (var item in employeePictures)
                {
                    //删除船员图片
                    _context.EmployeePicture.Remove(item);
                }
                //删除船员
                _context.Employee.Remove(employee);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }


        private bool EmployeeExists(string id)
        {
            return _context.Employee.Any(e => e.Id == id);
        }
    }
}
