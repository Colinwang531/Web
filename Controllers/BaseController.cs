using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Smartweb.Hubs;
using SmartWeb.Models;

namespace SmartWeb.Controllers
{
    public class BaseController : Controller
    {

        public UserToken user;

        /// <summary>  
        /// 请求过滤处理
        ///</summary> 
        /// <param name="filterContext"></param> 
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string browsertoken = HttpContext.Request.Cookies["token"];
            if (browsertoken == null || HttpContext.Session.Get(browsertoken) == null)
            {
                //var result = RedirectToAction("Index", "Login", new { temp = "redi" });
                RedirectResult result = new RedirectResult("/Login/NeedLogin");    /////这个控制器是为了实现跳转 
                filterContext.Result = result;
                base.OnActionExecuting(filterContext);
                return;
            }
            else
            {
                string urlstr = HttpContext.Session.GetString(browsertoken);
                user = JsonConvert.DeserializeObject<UserToken>(urlstr);
                base.OnActionExecuting(filterContext);
            }
        }
    }
}