using ShipWeb.DB;
using ShipWeb.ProtoBuffer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer
{
    public class ProtoBDManager
    {
        private static MyContext _context = new MyContext();
        private static string shipId = "";
        public ProtoBDManager() {
            if (shipId == "")
            {
                var ship = _context.Ship.FirstOrDefault();
                if (ship != null)
                {
                    shipId = ship.Id;
                }
            }
        }
        /// <summary>
        /// 添加组件信息
        /// </summary>
        /// <param name="componentInfo"></param>
        /// <returns></returns>
        public static string AddComponent(ComponentInfo componentInfo)
        {
            string cid = Process.GetCurrentProcess().Id.ToString();
            shipId = Guid.NewGuid().ToString();
            ShipWeb.Models.Component model = new ShipWeb.Models.Component()
            {
                Id = Guid.NewGuid().ToString(),
                Cid = cid,
                Name = componentInfo.cname,
                ShipId = shipId,
                Type = (ShipWeb.Models.Component.ComponentType)ComponentInfo.Type.WEB
            };
            ShipWeb.Models.Ship ship = new ShipWeb.Models.Ship()
            {
                Flag = false,
                Id = shipId,
                Name = "船1",
                Type = ShipWeb.Models.Ship.ShipType.AUTO
            };
            _context.Ship.Add(ship);
            _context.Components.Add(model);
            _context.SaveChanges();
            return cid;
        }

        /// <summary>
        /// 查询所有设备
        /// </summary>
        /// <returns></returns>
        public static List<Embedded> EmbeddedQuery(string did = "")
        {
            List<Embedded> list = new List<Embedded>();
            //查询设备信息
            var dbembed = _context.Embedded.Where(c => (!string.IsNullOrEmpty(did) ? c.Did == did : 1 == 1)).ToList();
            var cids = string.Join(',', dbembed.Select(c => c.Did));
            //查询设备下的摄像机
            var cameras = _context.Camera.Where(c => cids.Contains(c.Cid)).ToList();
            foreach (var item in dbembed)
            {
                Embedded em = new Embedded()
                {
                    did = item.Did,
                    factory = (Embedded.Factory)item.Factory,
                    ip = item.Id,
                    name = item.Name,
                    nickname = item.Nickname,
                    password = item.Password,
                    port = item.Port,
                    cameras = new List<Camera>()
                };
                var cams = cameras.Where(c => c.EmbeddedId == item.Id);
                foreach (var it in cams)
                {
                    Camera cam = new Camera()
                    {
                        cid = it.Cid,
                        enable = it.Enalbe,
                        index = it.Index,
                        ip = it.IP,
                        nickname = it.NickName
                    };
                    em.cameras.Add(cam);
                }
                list.Add(em);
            }
            return list;
        }
        public static int EmbeddedAdd(Embedded protoModel)
        {
            if (protoModel != null)
            {
                string id = Guid.NewGuid().ToString();
                ShipWeb.Models.Embedded model = new ShipWeb.Models.Embedded()
                {
                    Did = protoModel.did,
                    Factory = (int)protoModel.factory,
                    Id = id,
                    IP = protoModel.ip,
                    Name = protoModel.name,
                    Nickname = protoModel.nickname,
                    Password = protoModel.password,
                    Port = protoModel.port,
                    ShipId = shipId,
                    Type = (int)protoModel.type
                };
                if (protoModel.cameras.Count > 0)
                {
                    var camers = protoModel.cameras;
                    model.CameraModelList = new List<ShipWeb.Models.Camera>();
                    foreach (var item in camers)
                    {
                        ShipWeb.Models.Camera cam = new ShipWeb.Models.Camera()
                        {
                            Cid = item.cid,
                            EmbeddedId = id,
                            Enalbe = item.enable,
                            Id = Guid.NewGuid().ToString(),
                            Index = item.index,
                            IP = item.ip,
                            NickName = item.nickname,
                            ShipId = shipId
                        };
                        model.CameraModelList.Add(cam);
                    }
                }
                _context.Embedded.Add(model);
                _context.SaveChanges();
                return 0;
            }
            return 1;
        }
        /// <summary>
        /// 设备修改
        /// </summary>
        /// <param name="protoModel"></param>
        /// <returns></returns>
        public static int EmbeddedUpdate(string did, Embedded protoModel) {
            if (!string.IsNullOrEmpty(did) && protoModel != null)
            {
                var model = _context.Embedded.FirstOrDefault(c => c.Did == did);
                if (model == null) return 1;
                model.Factory = (int)protoModel.factory;
                model.IP = protoModel.ip;
                model.Name = protoModel.name;
                model.Nickname = protoModel.nickname;
                model.Password = protoModel.password;
                model.Port = protoModel.port;
                model.Type = (int)protoModel.type;
                if (protoModel.cameras.Count > 0)
                {
                    var cams = protoModel.cameras;
                    var camList = _context.Camera.Where(c => c.EmbeddedId == model.Id).ToList();
                    foreach (var item in cams)
                    {
                        if (camList.Where(c => c.Cid == item.cid).Any())
                        {
                            var carmera = camList.FirstOrDefault(c => c.Cid == item.cid);
                            carmera.Enalbe = item.enable;
                            carmera.NickName = item.nickname;
                            _context.Camera.Update(carmera);
                        }
                    }
                }
                _context.Embedded.Update(model);
                _context.SaveChanges();
                return 0;
            }
            return 1;
        }
        /// <summary>
        /// 设备删除
        /// </summary>
        /// <param name="did"></param>
        /// <returns></returns>
        public static int EmbeddedDelete(string did) {
            if (!string.IsNullOrEmpty(did))
            {
                var embedded = _context.Embedded.FirstOrDefault(c => c.Did == did);
                if (embedded != null)
                {
                    var cameras = _context.Camera.Where(c => c.EmbeddedId == embedded.Id).ToList();
                    if (cameras.Count > 0)
                    {
                        string cids = string.Join(',', cameras.Select(c => c.Cid));
                        var camconf = _context.CameraConfig.Where(c => cids.Contains(c.Cid)).ToList();
                        if (camconf.Count > 0)
                        {
                            _context.CameraConfig.RemoveRange(camconf);
                        }
                        _context.Camera.RemoveRange(cameras);
                    }
                    _context.Embedded.Remove(embedded);
                    _context.SaveChanges();
                    return 0;
                }
                return 1;
            }
            return 1;
        }
        /// <summary>
        /// 查询船员信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static List<Employee> EmployeeQuery(string uid = "")
        {
            List<Employee> list = new List<Employee>();
            //获取船员信息
            var dbempl = _context.Employee.Where(c => (!string.IsNullOrEmpty(uid) ? c.Uid == uid : 1 == 1)).ToList();
            var picIds = string.Join(',', dbempl.Select(c => c.Id));
            //获取船员图片
            var pics = _context.EmployeePicture.Where(c => picIds.Contains(c.EmployeeId)).ToList();
            foreach (var item in dbempl)
            {
                Employee em = new Employee()
                {
                    job = item.Job,
                    name = item.Name,
                    pictures = new List<byte[]>()
                };
                var picList = pics.Where(c => c.EmployeeId == item.Id);
                foreach (var itpic in picList)
                {
                    em.pictures.Add(itpic.Picture);
                }
                list.Add(em);
            }
            return list;
        }
        /// <summary>
        /// 添加船员
        /// </summary>
        /// <param name="protoModel"></param>
        /// <returns></returns>
        public static string EmployeeAdd(Employee protoModel) {
            if (protoModel != null)
            {
                string id = Guid.NewGuid().ToString();
                Random rd = new Random();
                ShipWeb.Models.Employee employee = new ShipWeb.Models.Employee()
                {
                    Id = id,
                    Job = protoModel.job,
                    Name = protoModel.name,
                    ShipId = shipId,
                    Uid = rd.Next(0001, 9999).ToString()
                };
                if (protoModel.pictures.Count > 0)
                {
                    employee.employeePictures = new List<ShipWeb.Models.EmployeePicture>();
                    var pics = protoModel.pictures;
                    foreach (var item in pics)
                    {
                        employee.employeePictures.Add(new ShipWeb.Models.EmployeePicture()
                        {
                            EmployeeId = id,
                            Id = Guid.NewGuid().ToString(),
                            Picture = item,
                            ShipId = shipId
                        });
                    }
                }
                _context.Employee.Add(employee);
                _context.SaveChanges();
                return employee.Uid;
            }
            return "";
        }
        /// <summary>
        /// 修改船员
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="protoModel"></param>
        /// <returns></returns>
        public static int EmployeeUpdate(string uid, Employee protoModel)
        {
            if (!string.IsNullOrEmpty(uid) && protoModel != null)
            {
                var model = _context.Employee.FirstOrDefault(c => c.Uid == uid);
                if (model != null)
                {
                    model.Job = protoModel.job;
                    model.Name = protoModel.name;
                    if (protoModel.pictures.Count > 0)
                    {
                        var pics = _context.EmployeePicture.Where(c => c.EmployeeId == model.Id);
                        if (pics.Count() > 0)
                        {
                            _context.EmployeePicture.RemoveRange(pics);
                        }
                        var protoPic = protoModel.pictures;
                        foreach (var item in protoPic)
                        {
                            ShipWeb.Models.EmployeePicture pic = new ShipWeb.Models.EmployeePicture()
                            {
                                EmployeeId = model.Id,
                                Id = Guid.NewGuid().ToString(),
                                Picture = item,
                                ShipId = model.ShipId
                            };
                            _context.EmployeePicture.Add(pic);
                        }
                    }
                    _context.Employee.Update(model);
                    _context.SaveChanges();
                    return 0;
                }
                return 1;
            }
            return 1;
        }
        /// <summary>
        /// 删除船员
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static int EmployeeDelete(string uid) {
            if (!string.IsNullOrEmpty(uid))
            {
                var model = _context.Employee.FirstOrDefault(c => c.Uid == uid);
                if (model != null)
                {
                    var pics = _context.EmployeePicture.Where(c => c.EmployeeId == model.Id);
                    if (pics.Count() > 0)
                    {
                        _context.EmployeePicture.RemoveRange(pics);
                    }
                    _context.Employee.Remove(model);
                    _context.SaveChanges();
                    return 0;
                }
                return 1;
            }
            return 1;
        }
        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="protoModel"></param>
        /// <returns></returns>
        public static int UserAdd(Person protoModel) {
            if (protoModel != null)
            {
                Random rd = new Random();
                ShipWeb.Models.Users user = new ShipWeb.Models.Users()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = protoModel.name,
                    Password = protoModel.password,
                    Uid = rd.Next(0001, 9999).ToString(),
                    EnableConfigure = protoModel.author != null ? protoModel.author.enableconfigure : false,
                    Enablequery = protoModel.author != null ? protoModel.author.enablequery : false
                };
                _context.Users.Add(user);
                _context.SaveChanges();
                return 0;
            }
            return 1;
        }
        /// <summary>
        /// 修改用户
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="protoModel"></param>
        /// <returns></returns>
        public static int UserUpdate(string uid, Person protoModel)
        {
            if (!string.IsNullOrEmpty(uid)&&protoModel!=null)
            {
                var user = _context.Users.FirstOrDefault(c => c.Uid == uid);
                if (user!=null)
                {
                    user.Name = protoModel.name;
                    user.Password = protoModel.password;
                    user.EnableConfigure = protoModel.author != null ? protoModel.author.enableconfigure : false;
                    user.Enablequery = protoModel.author != null ? protoModel.author.enablequery : false;
                    _context.Users.Update(user);
                    _context.SaveChanges();
                    return 0;
                }
                return 1;
            }
            return 1;
        }
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static int UserDelete(string uid) {
            if (!string.IsNullOrEmpty(uid))
            {
                var user = _context.Users.FirstOrDefault(c => c.Uid == uid);
                if (user!=null)
                {
                    _context.Remove(user);
                    _context.SaveChanges();
                    return 0;
                }
                return 1;
            }
            return 1;
        }
        /// <summary>
        /// 查询摄像机的配置（算法配置）
        /// </summary>
        /// <returns></returns>
        public static List<Configure> CameraConfigQuery() 
        {
            List<Configure> list = new List<Configure>();
            var config = from a in _context.Camera
                     join b in _context.CameraConfig on a.Cid equals b.Cid
                     into c
                     from d in c.DefaultIfEmpty()
                     select new
                     {
                         a.NickName,
                         a.Cid,
                         d.EnableAttendanceIn,
                         d.EnableAttendanceOut,
                         d.EnableFight,
                         d.EnableHelmet,
                         d.EnablePhone,
                         d.EnableSleep,
                         d.GPU,
                         d.Id,
                         d.ShipId,
                         d.Similar,
                     };
            foreach (var item in config)
            {
                Configure cf = new Configure()
                {
                    cid = item.Cid,
                    enableattendancein = item.EnableAttendanceIn,
                    enableattendanceout = item.EnableAttendanceOut,
                    enablefight = item.EnableFight,
                    enablehelmet = item.EnableHelmet,
                    enablephone = item.EnablePhone,
                    enablesleep = item.EnableSleep,
                    gpu = item.GPU,
                    similar =(float)item.Similar
                };
                list.Add(cf);
            }
            return list;
        }

        /// <summary>
        /// 陆地端请求修改算法配置
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int CameraConfigSet(List<Configure> list) 
        {
            try
            {
                var cids = string.Join(',', list.Select(c => c.cid));
                var config = _context.CameraConfig.Where(c => cids.Contains(c.Cid)).ToList();
                //查询当前船ID
                var ship = _context.Ship.FirstOrDefault();
                foreach (var item in list)
                {
                    ShipWeb.Models.CameraConfig cc = new ShipWeb.Models.CameraConfig()
                    {
                        Cid = item.cid,
                        EnableAttendanceIn = item.enableattendancein,
                        EnableAttendanceOut = item.enableattendanceout,
                        EnableFight = item.enablefight,
                        EnableHelmet = item.enablehelmet,
                        EnablePhone = item.enablephone,
                        EnableSleep = item.enablesleep,
                        GPU = item.gpu,
                        Similar = item.similar
                    };
                    if (config.Where(c => c.Cid == item.cid).Any())
                    {
                        var con = config.FirstOrDefault(c => c.Cid == item.cid);
                        cc.Id = con.Id;
                        cc.ShipId = con.ShipId;
                        _context.CameraConfig.Update(cc);
                    }
                    else
                    {
                        cc.Id = Guid.NewGuid().ToString();
                        cc.ShipId = ship.Id;
                        _context.CameraConfig.Add(cc);
                    }
                }
                _context.SaveChanges();
                return  0;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }
    }
}
