using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShipWeb.DB;
using ShipWeb.Models;
using Microsoft.AspNetCore.Http;
using ProtoBuf;
using ProtoBuf.Models;
using ShipWeb.Tool;

namespace ShipWeb.Controllers
{
    public class LoginController : Controller
    {
        private readonly MyContext _context;

        public LoginController(MyContext context)
        {
            _context = context;
        }
        /// <summary>
        /// 登陆页面
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 登陆注册
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IActionResult UserLogin(string name,string password)
        {
            var usersModel = _context.Users.FirstOrDefault(u => u.Name == name);
            if (usersModel==null)
            {
                return Json("用户名或密码不正确");
            }
            else 
            {
                if (usersModel.Password!= MD5Help.MD5Encrypt(password))
                {
                    return Json("用户名或密码不正确");
                }
                //保存登陆的用户ID
                HttpContext.Session.SetString("uid", usersModel.Uid);
                return Json("suscess");
            }
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        /// <summary>
        /// 注册
        /// </summary>
        /// <returns></returns>
        public JsonResult Save([FromBody] Users user)
        {
           
            if (user!=null)
            {
                string identity = Guid.NewGuid().ToString();
                ProtoManager manager = new ProtoManager();
                Person per = new Person() { 
                    name=user.Name,
                     password=user.Password
                };
                UserResponse rep= manager.UserAdd(per, identity);
                if (rep!=null&&rep.result==0)
                {
                    user.Id = identity;
                    user.Uid = rep.uid;
                    if (rep.persons.Count==1)
                    {
                        //user.EnableConfigure = rep.persons[0].author.enableconfigure;
                        //user.Enablequery = rep.persons[0].author.enablequery;
                    }                    
                }
                _context.Add(user);
                return new JsonResult("注册成功");
            }
            return new JsonResult("注册失败");
        }
    }
}