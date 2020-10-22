using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
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
                filterContext.Result = new RedirectResult("/Login/Index");
                return;
            }
            string urlstr = HttpContext.Session.GetString(browsertoken);
            user = JsonConvert.DeserializeObject<UserToken>(urlstr);
            base.OnActionExecuting(filterContext);
            //byte[] result;
            //filterContext.HttpContext.Session.TryGetValue("uid", out result);
            //if (result == null)
            //{
            //    filterContext.Result = new RedirectResult("/Login/Index");
            //    return;
            //}
            //base.OnActionExecuting(filterContext);
        }
    }
}