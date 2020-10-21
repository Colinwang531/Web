using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProtoBuf;
using SmartWeb.DB;
using SmartWeb.Models;
using SmartWeb.ProtoBuffer;
using SmartWeb.ProtoBuffer.Models;
using SmartWeb.Tool;

namespace SmartWeb.Controllers
{
    public class UserController : BaseController
    {
        private readonly MyContext _context;
        public UserController(MyContext context)
        {
            _context = context;
        }

        // GET: User
        public ActionResult Index()
        {
            return View();
        }
        public IActionResult Load()
        {
            var data = _context.User.Where(c => c.Id != "admin").ToList();
            var result = new
            {
                code = 0,
                data = data,
                isSet =base.user.EnableConfigure
            };
            return new JsonResult(result);
        }
        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IActionResult Search(string name) 
        {
            var data = _context.User.Where(c => c.Id != "admin"&&string.IsNullOrEmpty(name)?1==1:c.Name.Contains(name)).ToList();
            var result = new
            {
                code = 0,
                data = data,
                isSet = base.user.EnableConfigure
            };
            return new JsonResult(result);
        }
        public IActionResult Save(string users) {
            var model = JsonConvert.DeserializeObject<Models.User>(users);
            if (model!=null)
            {
                var userdb = _context.User.FirstOrDefault(c => c.Name == model.Name);
                if (!string.IsNullOrEmpty(model.Id)&&model.Id!="null")
                {
                    var user = _context.User.FirstOrDefault(c => c.Id == model.Id);
                    if (user == null)
                    {
                        return new JsonResult(new { code = 1, msg = "该数据不存在" });
                    }
                    if (userdb!=null&& userdb.Name!=user.Name)
                    {
                        return new JsonResult(new { code = 1, msg = "该用户名已存在，请重新修改" });
                    }
                    user.Name = model.Name;
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        user.Password = MD5Help.MD5Encrypt(model.Password);
                    }
                    user.EnableConfigure = model.EnableConfigure;
                    user.Enablequery = model.Enablequery;
                    //Person person = ConvertModel(user);
                    //int result = manager.UserUpdate(person, users.Uid, id);
                    //if (result == 0)
                    //{
                    _context.User.Update(user);
                    _context.SaveChanges();
                    //}
                }
                else
                {
                    if (userdb!=null)
                    {
                        return new JsonResult(new { code = 1, msg = "该用户名已存在，不能重复添加" });
                    }
                    model.Password = MD5Help.MD5Encrypt(model.Password);
                    string identity = Guid.NewGuid().ToString();
                    model.Id = identity;
                    //Person person = ConvertModel(model);
                    //UserResponse response = manager.UserAdd(person, identity);
                    //if (response.result == 0)
                    //{
                    //    users.Uid = response.uid;
                    //}
                    _context.User.Add(model);

                }
                _context.SaveChanges();
            }
            return new JsonResult(new { code = 0 });
        }
        /// <summary>
        /// 将本地实体转为传输格式实体
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private Person ConvertModel(Models.User user)
        {
            Person person = new Person()
            {
                name = user.Name,
                password = user.Password,
                author = new Author()
                {
                    enableconfigure = user.EnableConfigure,
                    enablequery = user.Enablequery
                }
            };
            return person;
        }
        public IActionResult Delete(string id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var users = _context.User.FirstOrDefault(m => m.Id == id);
                if (users == null)
                {
                    return NotFound();
                }
                //int result = manager.UserDelete(users.Uid, users.Id);
                //if (result == 0)
                //{
                    _context.User.Remove(users);
                    _context.SaveChanges();
                //}

                return Json(new { code = 0, msg = "删除成功"});
            }
            catch (Exception ex)
            {
                return Json(new { code = 1, msg = "删除失败" + ex.Message });
            }
        }
        public IActionResult UpdatePwd(string ypwd, string xpwd)
        {
            if (!string.IsNullOrEmpty(ypwd)&&!string.IsNullOrEmpty(xpwd))
            {
                byte[]by=HttpContext.Session.Get("uid");
                if (by==null)
                {
                    return Redirect("/Login/Index");
                }
                string uid = Encoding.UTF8.GetString(by);
                var user = _context.User.FirstOrDefault(c => c.Id == uid && c.Password == MD5Help.MD5Encrypt(ypwd));
                if (user==null)
                {
                    return new JsonResult(new { code = 1,msg="原始密码输入有误，请重新输入" });
                }
                user.Password = MD5Help.MD5Encrypt(xpwd);
                _context.User.Update(user);
                _context.SaveChanges();
                return new JsonResult(new { code = 0 });
            }
           
            return new JsonResult(new { code = 1,msg="修改密码失败" });
        }
    }
}
