using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ShipWeb.Controllers
{
    public class BaseController : Controller
    {
        /// <summary>  
        /// 请求过滤处理
        ///</summary> 
        /// <param name="filterContext"></param> 
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            byte[] result;
            filterContext.HttpContext.Session.TryGetValue("uid", out result);
            if (result == null)
            {
                filterContext.Result = new RedirectResult("/Login/Index");
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}