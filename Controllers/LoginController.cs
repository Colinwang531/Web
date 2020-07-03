using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShipWeb.DB;
using ShipWeb.Models;
using Microsoft.AspNetCore.Http;
using ProtoBuf;
using ShipWeb.Tool;
using ShipWeb.ProtoBuffer;
using ShipWeb.ProtoBuffer.Models;
using System.Text;
using Microsoft.AspNetCore.Identity;

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
        /// 登陆
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IActionResult UserLogin(string name,string password)
        {
            var usersModel = _context.Users.FirstOrDefault(u => u.Name == name && u.Password == MD5Help.MD5Encrypt(password));
            if (usersModel == null)
            {
                return new JsonResult(new { code = 1, msg = "用户名或密码不正确" });
            }
            else
            {
                //保存登陆的用户ID
                HttpContext.Session.Set("uid", Encoding.UTF8.GetBytes(usersModel.Uid));
                //保存用户可操作的权限 admin 最高权限
                ManagerHelp.IsSet = usersModel.Uid.ToLower() == "admin" ? true : usersModel.EnableConfigure;
                ManagerHelp.LoginName = name;
                bool flag = false;//判断船是否存在
                var ship = _context.Ship.FirstOrDefault();
                if (ship != null)
                {
                    ManagerHelp.ShipId = ship.Id;
                    flag = true;
                }
                //登陆成功后跳转组件页面
                //return RedirectToAction(nameof(Index), "Home");
                return new JsonResult(new { code = 0, flag = flag });
            }
        }
        /// <summary>
        /// 退出
        /// </summary>
        /// <returns></returns>
        public IActionResult SignOut()
        {
            //消除缓存
            HttpContext.Session.Remove("uid");
            ManagerHelp.ShipId = "";
            ManagerHelp.IsShowLandHome = false;
            InitManger.Exit();
            return RedirectToAction(nameof(Index));
        }
    }
}