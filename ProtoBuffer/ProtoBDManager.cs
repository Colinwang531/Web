using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Ocsp;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.ProtoBuffer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer
{
    public class ProtoBDManager
    {
      
        /// <summary>
        /// 添加组件信息
        /// </summary>
        /// <param name="componentInfo"></param>
        /// <returns></returns>
        public static string AddComponent(string identity,ComponentInfo componentInfo)
        {
            using (var _context = new MyContext())
            {
                string cid = Process.GetCurrentProcess().Id.ToString();
                ShipWeb.Models.Component model = new ShipWeb.Models.Component()
                {
                    Id = identity,
                    Name = componentInfo.cname,
                    ShipId = "",
                    Type = (ShipWeb.Models.Component.ComponentType)ComponentInfo.Type.WEB
                };
                _context.Component.Add(model);
                _context.SaveChanges();
                return cid;
            }
        }

        /// <summary>
        /// 查询所有设备
        /// </summary>
        /// <returns></returns>
        public static List<DeviceInfo> EmbeddedQuery(string shipId,string did = "")
        {
            using (var _context = new MyContext())
            {
                List<DeviceInfo> list = new List<DeviceInfo>();
                //查询设备信息
                var dbembed = _context.Device.Where(c =>c.ShipId==shipId&& (!string.IsNullOrEmpty(did) ? c.Id == did : 1 == 1)).ToList();
                var ids = string.Join(',', dbembed.Select(c => c.Id));
                //查询设备下的摄像机
                var cameras = _context.Camera.Where(c =>c.ShipId==shipId&& ids.Contains(c.DeviceId)).ToList();
                foreach (var item in dbembed)
                {
                    DeviceInfo em = new DeviceInfo()
                    {
                        did =item.Id,
                        factory = (DeviceInfo.Factory)item.factory,
                        ip = item.IP,
                        name = item.Name,
                        nickname = item.Nickname,
                        password = item.Password,
                        port = item.Port,
                        type=(DeviceInfo.Type)item.type,
                        camerainfos = new List<CameraInfo>()
                    };
                    var cams = cameras.Where(c => c.DeviceId == item.Id);
                    foreach (var it in cams)
                    {
                        CameraInfo cam = new CameraInfo()
                        {
                            cid =it.Id,
                            enable = it.Enalbe,
                            index = it.Index,
                            ip = it.IP,
                            nickname = it.NickName
                        };
                        em.camerainfos.Add(cam);
                    }
                    list.Add(em);
                }
                return list;
            }
        }
        public static int EmbeddedAdd(string shipId,DeviceInfo protoModel)
        {
            using (var _context = new MyContext())
            {
                if (protoModel != null)
                {
                    string id = Guid.NewGuid().ToString();
                    ShipWeb.Models.Device model = new ShipWeb.Models.Device()
                    {
                        factory = (ShipWeb.Models.Device.Factory)protoModel.factory,
                        Id = id,
                        IP = protoModel.ip,
                        Name = protoModel.name,
                        Nickname = protoModel.nickname,
                        Password = protoModel.password,
                        Port = protoModel.port,
                        ShipId = shipId,
                        type = (ShipWeb.Models.Device.Type)protoModel.type
                    };
                    if (protoModel.camerainfos!=null&&protoModel.camerainfos.Count > 0)
                    {
                        var camers = protoModel.camerainfos;
                        model.CameraModelList = new List<ShipWeb.Models.Camera>();
                        int i = 0;
                        foreach (var item in camers)
                        {
                            i++;
                            ShipWeb.Models.Camera cam = new ShipWeb.Models.Camera()
                            {
                                DeviceId = id,
                                Enalbe = item.enable,
                                Id = Guid.NewGuid().ToString(),
                                Index = item.index,
                                IP = item.ip,
                                NickName = item.nickname==""?"摄像机"+i:item.nickname,
                                ShipId = shipId
                            };
                            model.CameraModelList.Add(cam);
                        }
                    }
                    _context.Device.Add(model);
                    _context.SaveChanges();
                    return 0;
                }
                return 1;
            }
        }
        /// <summary>
        /// 设备修改
        /// </summary>
        /// <param name="protoModel"></param>
        /// <returns></returns>
        public static int EmbeddedUpdate(string shipId,string did, DeviceInfo protoModel) 
        {
            using (var _context = new MyContext())
            {
                if (!string.IsNullOrEmpty(did) && protoModel != null)
                {
                    var model = _context.Device.FirstOrDefault(c =>c.ShipId==shipId&& c.Id == did);
                    if (model == null) return 1;
                    if (protoModel.name!=null)
                    {
                        model.factory = (ShipWeb.Models.Device.Factory)protoModel.factory;
                        model.IP = protoModel.ip;
                        model.Name = protoModel.name;
                        model.Nickname = protoModel.nickname;
                        model.Password = protoModel.password;
                        model.Port = protoModel.port;
                        model.type = (ShipWeb.Models.Device.Type)protoModel.type;
                        _context.Device.Update(model);
                    }                    
                    if (protoModel.camerainfos!=null&&protoModel.camerainfos.Count > 0)
                    {
                        string cids = string.Join(',', protoModel.camerainfos.Select(c => c.cid));
                        var cams = protoModel.camerainfos;
                        var camList = _context.Camera.Where(c => c.DeviceId == model.Id&& cids.Contains(c.Id)).ToList();
                        foreach (var item in cams)
                        {
                            if (camList.Where(c => c.Id == item.cid).Any())
                            {
                                var carmera = camList.FirstOrDefault(c => c.Id == item.cid);
                                carmera.Enalbe = item.enable;
                                carmera.NickName = item.nickname;
                                _context.Camera.Update(carmera);
                            }
                        }
                    }
                    _context.SaveChanges();
                    return 0;
                }
                return 1;
            }
        }
        /// <summary>
        /// 设备删除
        /// </summary>
        /// <param name="did"></param>
        /// <returns></returns>
        public static int EmbeddedDelete(string shipId,string did) {
            using (var _context = new MyContext())
            {
                if (!string.IsNullOrEmpty(did))
                {
                    var embedded = _context.Device.FirstOrDefault(c =>c.ShipId==shipId&& c.Id == did);
                    if (embedded != null)
                    {
                        var cameras = _context.Camera.Where(c => c.DeviceId == embedded.Id).ToList();
                        if (cameras.Count > 0)
                        {
                            string cids = string.Join(',', cameras.Select(c => c.Id));
                            var camconf = _context.Algorithm.Where(c =>c.ShipId==shipId&& cids.Contains(c.Cid)).ToList();
                            if (camconf.Count > 0)
                            {
                                _context.Algorithm.RemoveRange(camconf);
                            }
                            _context.Camera.RemoveRange(cameras);
                        }
                        _context.Device.Remove(embedded);
                        _context.SaveChanges();
                        return 0;
                    }
                    return 1;
                }
                return 1;
            }
        }
        /// <summary>
        /// 查询船员信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static List<CrewInfo> EmployeeQuery(string shipId,string uid = "")
        {
            using (var _context = new MyContext())
            {
                List<CrewInfo> list = new List<CrewInfo>();
                //获取船员信息
                var dbempl = _context.Crew.Where(c =>c.ShipId==shipId&& (!string.IsNullOrEmpty(uid) ? c.Id == uid : 1 == 1)).ToList();
                var picIds = string.Join(',', dbempl.Select(c => c.Id));
                //获取船员图片
                var pics = _context.CrewPicture.Where(c => picIds.Contains(c.CrewId)).ToList();
                foreach (var item in dbempl)
                {
                    CrewInfo em = new CrewInfo()
                    {
                        job = item.Job,
                        name = item.Name,
                        uid =item.Id,
                        pictures = new List<byte[]>()
                    };
                    var picList = pics.Where(c => c.CrewId == item.Id);
                    foreach (var itpic in picList)
                    {
                        string pic = Encoding.UTF8.GetString(itpic.Picture);
                        byte[] by = Encoding.UTF8.GetBytes(itpic.Id + "," + pic);
                        em.pictures.Add(by);
                    }
                    list.Add(em);
                }
                return list;
            }
        }
        /// <summary>
        /// 添加船员
        /// </summary>
        /// <param name="protoModel"></param>
        /// <returns></returns>
        public static int EmployeeAdd(string shipId,CrewInfo protoModel)
        {
            using (var _context = new MyContext())
            {
                if (protoModel != null)
                {
                    var dbemp = _context.Crew.FirstOrDefault(c => c.Name == protoModel.name);
                    if (dbemp != null)
                    {
                        return 1;
                    }
                    string id = Guid.NewGuid().ToString();
                    ShipWeb.Models.Crew employee = new ShipWeb.Models.Crew()
                    {
                        Id = id,
                        Job = protoModel.job,
                        Name = protoModel.name,
                        ShipId = shipId
                    };
                    if (protoModel.pictures!=null&&protoModel.pictures.Count > 0)
                    {
                        employee.employeePictures = new List<ShipWeb.Models.CrewPicture>();
                        var pics = protoModel.pictures;
                        foreach (var item in pics)
                        {
                            employee.employeePictures.Add(new ShipWeb.Models.CrewPicture()
                            {
                                CrewId = id,
                                Id = Guid.NewGuid().ToString(),
                                Picture = item,
                                ShipId = shipId
                            });
                        }
                    }
                    _context.Crew.Add(employee);
                    _context.SaveChanges();
                    return 0;
                }
                return 1;
            }
        }
        /// <summary>
        /// 修改船员
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="protoModel"></param>
        /// <returns></returns>
        public static int EmployeeUpdate(string shipId, CrewInfo protoModel)
        {
            using (var _context = new MyContext())
            {
                if (protoModel != null)
                {
                    var model = _context.Crew.FirstOrDefault(c =>c.ShipId==shipId&& c.Id == protoModel.uid);
                    if (model != null)
                    {
                        var dbmodel = _context.Crew.FirstOrDefault(c => c.Name == protoModel.name);
                        if (dbmodel!=null && dbmodel.Name != model.Name)
                        {
                            return 1;
                        }
                        model.Job = protoModel.job;
                        model.Name = protoModel.name;
                        var pics = _context.CrewPicture.Where(c => c.CrewId == model.Id);
                        //记录数据库中存在的图片ID
                        List<string> dbIds = new List<string>();
                        if (protoModel.pictures!=null&&protoModel.pictures.Count > 0)
                        {
                            var protoPic = protoModel.pictures;
                            foreach (var item in protoPic)
                            {
                                string str = Encoding.UTF8.GetString(item);
                                var ids = str.Split(',');
                                if (ids.Length>1)
                                {
                                    ShipWeb.Models.CrewPicture pic = new ShipWeb.Models.CrewPicture()
                                    {
                                        CrewId = model.Id,
                                        Id =ids[0],
                                        Picture =Encoding.UTF8.GetBytes(ids[1]),
                                        ShipId = model.ShipId
                                    };
                                    _context.CrewPicture.Add(pic);
                                }
                                else if (pics.Where(c => c.Id == ids[0]).Any())
                                {
                                    dbIds.Add(ids[0]);
                                }
                            }

                        }
                        //查找当前船员下需要删除的图片
                        var delPicList = pics.Where(c => !dbIds.Contains(c.Id)).ToList();
                        if (delPicList.Count > 0)
                        {
                            _context.CrewPicture.RemoveRange(pics);
                        }
                        _context.Crew.Update(model);
                        _context.SaveChanges();
                        return 0;
                    }
                    return 1;
                }
                return 1;
            }
        }
        /// <summary>
        /// 删除船员
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static int EmployeeDelete(string shipId,string uid) {
            using (var _context = new MyContext())
            {
                if (!string.IsNullOrEmpty(uid))
                {
                    var model = _context.Crew.FirstOrDefault(c =>c.ShipId==shipId&& c.Id == uid);
                    if (model != null)
                    {
                        var pics = _context.CrewPicture.Where(c => c.CrewId == model.Id);
                        if (pics.Count() > 0)
                        {
                            _context.CrewPicture.RemoveRange(pics);
                        }
                        _context.Crew.Remove(model);
                        _context.SaveChanges();
                        return 0;
                    }
                    return 1;
                }
                return 1;
            }
        }
        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="protoModel"></param>
        /// <returns></returns>
        public static int UserAdd(Person protoModel) {
            using (var _context = new MyContext())
            {
                if (protoModel != null)
                {
                    ShipWeb.Models.User user = new ShipWeb.Models.User()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = protoModel.name,
                        Password = protoModel.password,
                        EnableConfigure = protoModel.author != null ? protoModel.author.enableconfigure : false,
                        Enablequery = protoModel.author != null ? protoModel.author.enablequery : false
                    };
                    _context.User.Add(user);
                    _context.SaveChanges();
                    return 0;
                }
                return 1;
            }
        }
        /// <summary>
        /// 修改用户
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="protoModel"></param>
        /// <returns></returns>
        public static int UserUpdate(string uid, Person protoModel)
        {
            using (var _context = new MyContext())
            {
                if (!string.IsNullOrEmpty(uid) && protoModel != null)
                {
                    var user = _context.User.FirstOrDefault(c => c.Id == uid);
                    if (user != null)
                    {
                        user.Name = protoModel.name;
                        user.Password = protoModel.password;
                        user.EnableConfigure = protoModel.author != null ? protoModel.author.enableconfigure : false;
                        user.Enablequery = protoModel.author != null ? protoModel.author.enablequery : false;
                        _context.User.Update(user);
                        _context.SaveChanges();
                        return 0;
                    }
                    return 1;
                }
                return 1;
            }
        }
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static int UserDelete(string uid) {
            using (var _context = new MyContext())
            {
                if (!string.IsNullOrEmpty(uid))
                {
                    var user = _context.User.FirstOrDefault(c => c.Id  == uid);
                    if (user != null)
                    {
                        _context.Remove(user);
                        _context.SaveChanges();
                        return 0;
                    }
                    return 1;
                }
                return 1;
            }
        }
        /// <summary>
        /// 查询摄像机的配置（算法配置）
        /// </summary>
        /// <returns></returns>
        public static List<AlgorithmInfo> CameraConfigQuery(string shipId) 
        {
            using (var _context = new MyContext())
            {
                List<AlgorithmInfo> list = new List<AlgorithmInfo>();
                var config = from a in _context.Algorithm
                             join b in _context.Camera on a.Cid equals b.Id
                             into c
                             from d in c.DefaultIfEmpty()
                             where a.ShipId == shipId
                             select new
                             {
                                 a.Id,
                                 a.Cid,
                                 a.Type,
                                 a.GPU,
                                 a.Similar,
                                 d.NickName
                             };
                foreach (var item in config)
                {
                    AlgorithmInfo cf = new AlgorithmInfo()
                    {
                        cid =item.Id+","+item.Cid+","+item.NickName,
                        type = (AlgorithmInfo.Type)item.Type,
                        gpu = item.GPU,
                        similar = (float)item.Similar
                    };
                    list.Add(cf);
                }
                return list;
            }
        }

        /// <summary>
        /// 陆地端请求修改算法配置
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int CameraConfigSet(string shipId,AlgorithmInfo protoModel) 
        {
            int result = 1;
            using (var _context = new MyContext())
            {
                try
                {
                    if (protoModel!=null)
                    {
                        if (protoModel.cid!="")
                        {
                            string id = protoModel.cid.Split(',')[0];
                            string cid = protoModel.cid.Split(',')[1];
                            var algo = _context.Algorithm.FirstOrDefault(c => c.Id == id);
                            if (algo == null) result = 1;
                            algo.GPU = protoModel.gpu;
                            algo.Similar = protoModel.similar;
                            algo.Type = (ShipWeb.Models.AlgorithmType)protoModel.type;
                            algo.Cid = cid;
                            _context.Algorithm.Update(algo);
                        }
                        else
                        {
                            ShipWeb.Models.Algorithm model = new ShipWeb.Models.Algorithm()
                            {
                                Cid = protoModel.cid,
                                GPU = protoModel.gpu,
                                Id = Guid.NewGuid().ToString(),
                                ShipId = shipId,
                                Similar = protoModel.similar,
                                Type = (ShipWeb.Models.AlgorithmType)protoModel.type
                            };
                            _context.Algorithm.Add(model);
                        }
                    }   
                    _context.SaveChanges();
                    return 0;
                }
                catch (Exception ex)
                {
                    return 1;
                }
            }
        }
        /// <summary>
        /// 向陆地端添加船
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static int ShipSet(string identity,StatusRequest request)
        {
            try
            {
                using (var _context = new MyContext())
                {
                    //identity 保存的是两个值，第一个为shipId,第二个cid组件id
                    var ids = identity.Split(',');
                    //有两个值时船舶端进行了添加船的请求，此时也要向陆在端添加船
                    if (ids.Length>1)
                    {
                        //查询组件ID是否在陆地端的组件表中存在
                        var comp=_context.Component.FirstOrDefault(c => c.Id == ids[1]);
                        if (comp!=null)
                        {
                            comp.ShipId = ids[0];
                            //修改陆地端组件表中对应的船ID
                            _context.Component.Update(comp);
                        }
                        ShipWeb.Models.Ship ship = new ShipWeb.Models.Ship()
                        {
                            Id = ids[0],
                            Flag = request.flag,
                            type = (ShipWeb.Models.Ship.Type)request.type
                        };
                        _context.Ship.Add(ship);
                    }
                    else
                    {
                        //修改船状态信息
                        var ship = _context.Ship.FirstOrDefault(c => c.Id == ids[0]);
                        if (ship!=null)
                        {
                            if (request.text!=""&&request.type==StatusRequest.Type.NAME)
                            {
                                ship.Name = request.text;
                                _context.Ship.Update(ship);
                                _context.SaveChanges();
                            }
                            else if(request.type == StatusRequest.Type.SAIL)
                            {
                                ship.type = (ShipWeb.Models.Ship.Type)request.type;
                                ship.Flag = request.flag;
                                _context.Ship.Update(ship);
                                _context.SaveChanges();
                            }
                        }
                    }                    
                    return 0;
                }
            }
            catch (Exception ex)
            {
                return 1;
            }
        }
    }
}
