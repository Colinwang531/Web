using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        private ProtoManager manager = new ProtoManager();
        public UsersController(MyContext context)
        {
            _context = context;
        }

        // GET: Users
        public ActionResult Index()
        {
            return View();
        }
        public IActionResult Load()
        {
            var data = _context.Users.Where(c => c.Name != "admin").ToList();
            var result = new
            {
                code = 0,
                data = data,
                isSet = ManagerHelp.IsSet
            };
            return new JsonResult(result);
        }
        public IActionResult Create()
        {
            return View();
        }
        /// <summary>
        /// 新旧密码对比
        /// </summary>
        /// <param name="pwd">原密码</param>
        /// <param name="newPwd">新密码</param>
        public IActionResult Save(string id, string name, bool enableConfigure, bool enablequery, string pwd, string newPwd)
        {
            try
            {
                //获取数据库中的数据，查出密码
                var users = _context.Users.FirstOrDefault(c => c.Id == id);
                if (users == null)
                {
                    return new JsonResult(new { code = 1, msg = "数据库中不存在条数" });
                }
                //是否做了密码修改
                if (!string.IsNullOrEmpty(pwd))
                {
                    string md5pwd = MD5Help.MD5Encrypt(pwd);
                    if (users.Password != md5pwd)
                    {
                        return new JsonResult(new { code = 1, msg = "原始密码输入有误" });
                    }
                }
                if (!string.IsNullOrEmpty(newPwd))
                {
                    //将新密码加密
                    users.Password = MD5Help.MD5Encrypt(newPwd);
                }
                users.Name = name;
                users.EnableConfigure = enableConfigure;
                users.Enablequery = enablequery;

                Person person = ConvertModel(users);
                int result = manager.UserUpdate(person, users.Uid, id);
                if (result == 0)
                {
                    _context.Users.Update(users);
                    _context.SaveChanges();
                }
                return new JsonResult(new { code = 0 });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 1,msg="数据修改失败!"+ex.Message });
            }
        }
        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSave(Users users)
        {
            if (ModelState.IsValid)
            {
                string identity = Guid.NewGuid().ToString();
                ProtoManager manager = new ProtoManager();
                users.Uid = "11111";
                users.Id = identity;
                Person person = ConvertModel(users);
                UserResponse response = manager.UserAdd(person, identity);
                if (response.result == 0)
                {
                    users.Uid = response.uid;
                }
                _context.Add(users);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(users);
        }
        /// <summary>
        /// 将本地实体转为传输格式实体
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        private Person ConvertModel(Users users)
        {
            users.Password = MD5Help.MD5Encrypt(users.Password); 
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
        public IActionResult Edit(string id)
        {
            ViewBag.Id = id;
            var users = _context.Users.Find(id);
            if (users != null)
            {
                ViewBag.Name = users.Name;
                ViewBag.Password = users.Password;
                ViewBag.EnableConfigure = users.EnableConfigure;
                ViewBag.Enablequery = users.Enablequery;
                ViewBag.Uid = users.Uid;
            }
            return View();
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
                ProtoManager manager = new ProtoManager();
                int result = manager.UserDelete(users.Uid, users.Id);
                if (result == 0)
                {
                    _context.Users.Remove(users);
                    _context.SaveChanges();
                }

                return Json(new { code = 0, msg = "删除成功"});
            }
            catch (Exception ex)
            {
                return Json(new { code = 1, msg = "删除失败" + ex.Message });
            }
        }


        private bool UsersExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }



        #region Ahri 2020-06-18 

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="courseExampleId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        //[HttpGet]
        //public async Task<IActionResult> List(int page, int limit)
        //{
        //    var users = await _context.Users.Where(c => c.Name != "admin").ToListAsync();
        //    return Content(JsonHelper.SerializePageResult(CodeResult.SUCCESS, users.Skip((page - 1) * limit).Take(limit).ToList(), users.Count));
        //}


        #endregion
    }
}
