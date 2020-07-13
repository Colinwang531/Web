using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProtoBuf;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.ProtoBuffer;
using ShipWeb.ProtoBuffer.Models;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class UsersController : BaseController
    {
        private readonly MyContext _context;
        private ProtoManager manager;
        public UsersController(MyContext context)
        {
            _context = context;
            manager = new ProtoManager();
        }

        // GET: Users
        public ActionResult Index()
        {
            return View();
        }
        public IActionResult Load()
        {
            var data = _context.Users.Where(c => c.Uid != "admin").ToList();
            var result = new
            {
                code = 0,
                data = data,
                isSet = ManagerHelp.IsSet
            };
            return new JsonResult(result);
        }
        public IActionResult UserSave(string users) {
            var model = JsonConvert.DeserializeObject<Users>(users);
            if (model!=null)
            {
                var userdb = _context.Users.FirstOrDefault(c => c.Name == model.Name);
                if (!string.IsNullOrEmpty(model.Id)&&model.Id!="null")
                {
                    var user = _context.Users.FirstOrDefault(c => c.Id == model.Id);
                    if (user == null)
                    {
                        return new JsonResult(new { code = 1, msg = "该数据不存在" });
                    }
                    if (userdb!=null&& userdb.Name!=user.Name)
                    {
                        return new JsonResult(new { code = 1, msg = "该用户名已存在，请重新修改" });
                    }
                    user.Name = model.Name;
                    user.Password = MD5Help.MD5Encrypt(model.Password);
                    user.EnableConfigure = model.EnableConfigure;
                    user.Enablequery = model.Enablequery;
                    Person person = ConvertModel(user);
                    //int result = manager.UserUpdate(person, users.Uid, id);
                    //if (result == 0)
                    //{
                    _context.Users.Update(user);
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
                    Random rm = new Random();
                    //测试值
                    model.Uid = rm.Next(1111, 9999).ToString();
                    model.Id = identity;
                    Person person = ConvertModel(model);
                    //UserResponse response = manager.UserAdd(person, identity);
                    //if (response.result == 0)
                    //{
                    //    users.Uid = response.uid;
                    //}
                    _context.Users.Add(model);

                }
                _context.SaveChanges();
            }
            return new JsonResult(new { code = 0 });
        }
        /// <summary>
        /// 将本地实体转为传输格式实体
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        private Person ConvertModel(Users users)
        {
            Person person = new Person()
            {
                name = users.Name,
                password = users.Password,
                author = new Author()
                {
                    enableconfigure = users.EnableConfigure,
                    enablequery = users.Enablequery
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

                var users = _context.Users.FirstOrDefault(m => m.Id == id);
                if (users == null)
                {
                    return NotFound();
                }
                //int result = manager.UserDelete(users.Uid, users.Id);
                //if (result == 0)
                //{
                    _context.Users.Remove(users);
                    _context.SaveChanges();
                //}

                return Json(new { code = 0, msg = "删除成功"});
            }
            catch (Exception ex)
            {
                return Json(new { code = 1, msg = "删除失败" + ex.Message });
            }
        }
    }
}
