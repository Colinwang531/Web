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
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;

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
            HttpContext.Session.Remove("uid");
            ManagerHelp.ShipId = "";
            ManagerHelp.IsShowLandHome = false;
            return View();
        }

        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IActionResult UserLogin(string name,string password)
        {
            if (string.IsNullOrEmpty(ManagerHelp.Cid))
            {
                return new JsonResult(new { code = 1, msg = "组件还在自动注册中，请稍等" });
            }
            var usersModel = _context.User.FirstOrDefault(u => u.Name == name && u.Password == MD5Help.MD5Encrypt(password));
            if (usersModel == null)
            {
                return new JsonResult(new { code = 1, msg = "用户名或密码不正确" });
            }
            else
            {

                //保存登陆的用户ID
                HttpContext.Session.Set("uid", Encoding.UTF8.GetBytes(usersModel.Id));
                //保存用户可操作的权限 admin 最高权限
                ManagerHelp.IsShowAlarm = usersModel.Id.ToLower() == "admin" ? true : usersModel.Enablequery;
                bool flag = false;//判断船是否存在
                var ship = _context.Ship.FirstOrDefault();
                if (ship != null)
                {
                    ManagerHelp.ShipId = ship.Id;
                    flag = true;
                }
                string userStr = JsonConvert.SerializeObject(usersModel);
                string browsertoken = HttpContext.Request.Cookies["token"];
                if (browsertoken == null)
                {
                    //生成token
                    string token = Guid.NewGuid().ToString();
                    //将请求的url注册
                    HttpContext.Session.SetString(token, userStr);
                    //写入浏览器token
                    HttpContext.Response.Cookies.Append("token", token);
                }
                else
                {
                    //将请求的url注册
                    HttpContext.Session.SetString(browsertoken, userStr);
                    //写入浏览器token
                    HttpContext.Response.Cookies.Append("token", browsertoken);
                }
                return new JsonResult(new { code = 0, flag = flag });
            }
        }
        /// <summary>
        /// 退出
        /// </summary>
        /// <returns></returns>
        public IActionResult SignOut()
        {
            string browsertoken = HttpContext.Request.Cookies["token"];
            HttpContext.Response.Cookies.Delete("token");
            HttpContext.Session.Clear();


            //消除缓存
            //HttpContext.Session.Remove("uid");
            ManagerHelp.ShipId = "";
            ManagerHelp.IsShowLandHome = false;
            InitManger.Exit();
            return RedirectToAction(nameof(Index));
        }
    }
}