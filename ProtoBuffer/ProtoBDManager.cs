using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Ocsp;
using Smartweb.Helpers;
using Smartweb.Hubs;
using SmartWeb.DB;
using SmartWeb.Models;
using SmartWeb.ProtoBuffer.Init;
using SmartWeb.ProtoBuffer.Models;
using SmartWeb.Tool;
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
using System.Threading;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer
{
    public class ProtoBDManager
    {
        private readonly MemoryCacheHelper cache = new MemoryCacheHelper();
      
        SendDataMsg sendData = new SendDataMsg();
      

        /// <summary>
        /// 查询所有设备
        /// </summary>
        /// <returns></returns>
        public static List<SmartWeb.Models.Device> DeviceQuery(DeviceInfo info, string did = "")
        {
            List<SmartWeb.Models.Device> list = new List<SmartWeb.Models.Device>();
            using (var _context = new MyContext())
            {
                //查询设备信息
                list = GetDevice(info);
                if (list.Count > 0)
                {
                    var ids = string.Join(',', list.Select(c => c.Id));
                    //查询设备下的摄像机
                    var cameras = _context.Camera.Where(c => ids.Contains(c.DeviceId)).ToList();
                    foreach (var item in list)
                    {
                        item.CameraModelList = new List<Camera>();
                        var cames = cameras.Where(c => c.DeviceId == item.Id);
                        foreach (var cam in cames)
                        {
                            item.CameraModelList.Add(new Camera()
                            {
                                Id = cam.Id,
                                DeviceId = cam.DeviceId,
                                Enable = cam.Enable,
                                Index = cam.Index,
                                IP = cam.IP,
                                NickName = cam.NickName
                            });
                        }
                    }
                }
                return list;
            }
        }
        private static List<SmartWeb.Models.Device> GetDevice(DeviceInfo info)
        {
            using (var context = new MyContext())
            {
                var device = context.Device.Where(c => 1 == 1);
                if (info != null)
                {
                    device = device.Where(c => c.Enable == info.enable);
                    if (!string.IsNullOrEmpty(info.name))
                    {
                        device = device.Where(c => c.Name.Contains(info.name));
                    }
                    if (!string.IsNullOrEmpty(info.nickname))
                    {
                        device = device.Where(c => c.Nickname.Contains(info.nickname));
                    }
                    if (!string.IsNullOrEmpty(info.ip))
                    {
                        device = device.Where(c => c.IP == info.ip);
                    }
                    if (!string.IsNullOrEmpty(info.did))
                    {
                        device = device.Where(c => c.Id == info.did);
                    }
                    if (info.port > 0)
                    {
                        device = device.Where(c => c.Port == info.port);
                    }
                }
                return device.ToList();
            }

        }
        /// <summary>
        /// 添加设备
        /// </summary>
        /// <param name="shipId"></param>
        /// <param name="protoModel"></param>
        /// <returns></returns>
        public static SmartWeb.Models.Device DeviceAdd(DeviceInfo protoModel)
        {
            if (protoModel != null)
            {
                using (var _context = new MyContext())
                {
                    string shipId = "";//_context.Ship.FirstOrDefault().Id;
                    var factory = (SmartWeb.Models.Device.Factory)protoModel.factory;
                    var type = (SmartWeb.Models.Device.Type)protoModel.type;
                    SmartWeb.Models.Device model =_context.Device.FirstOrDefault(c => c.factory == factory && c.type== type && c.IP==protoModel.ip);
                    if (model == null)
                    {
                        model = new SmartWeb.Models.Device();
                        model.factory = factory;
                        model.IP = protoModel.ip;
                        model.Name = protoModel.name;
                        model.Nickname = protoModel.nickname;
                        model.Password = protoModel.password;
                        model.Port = protoModel.port;
                        model.type = type;
                        model.Enable = protoModel.enable;
                        model.Id = Guid.NewGuid().ToString();
                        _context.Device.Add(model);
                        _context.SaveChanges();
                        if (protoModel.camerainfos != null && protoModel.camerainfos.Count > 0)
                        {
                            AddCameras(protoModel.camerainfos, protoModel.did);
                        }
                    }
                    return model;
                }
            }
            return null;
        }
        /// <summary>
        /// 设备修改
        /// </summary>
        /// <param name="protoModel"></param>
        /// <returns></returns>
        public static SmartWeb.Models.Device DeviceUpdate(string did, DeviceInfo protoModel)
        {
            bool isSend = false;
            return DeviceUpdate(did, protoModel, ref isSend);
        }
        /// <summary>
        /// 设备修改
        /// </summary>
        /// <param name="protoModel"></param>
        /// <returns></returns>
        public static SmartWeb.Models.Device DeviceUpdate(string did, DeviceInfo protoModel, ref bool isSend)
        {
            using (var _context = new MyContext())
            {
                if (!string.IsNullOrEmpty(did) && protoModel != null)
                {
                    var model = _context.Device.FirstOrDefault(c => c.Id == did);
                    if (model == null) return null;
                    if (protoModel.name != null)
                    {
                        model.factory = (SmartWeb.Models.Device.Factory)protoModel.factory;
                        model.IP = protoModel.ip;
                        model.Name = protoModel.name;
                        model.Nickname = protoModel.nickname;
                        model.Password = protoModel.password;
                        model.Port = protoModel.port;
                        model.type = (SmartWeb.Models.Device.Type)protoModel.type;
                        model.Enable = protoModel.enable;
                        _context.Device.Update(model);
                        _context.SaveChanges();
                        AddCameras(protoModel.camerainfos, did);
                        isSend = true;
                    }
                    else
                    {
                        AddCameras(protoModel.camerainfos, did,ref isSend);
                    }
                    return model;
                }
                return null;
            }
        }
        public static int AddCameras(List<CameraInfo> cameraInfos, string did) {
            bool flag = false;
            return AddCameras(cameraInfos, did, ref flag);
        }

        /// <summary>
        /// 添加摄像机
        /// </summary>
        /// <param name="cameraInfos"></param>
        /// <param name="did"></param>
        /// <returns></returns>
        public static int AddCameras(List<CameraInfo> cameraInfos, string did,ref bool isSend)
        {
            isSend = false;
            if (cameraInfos != null)
            {
                using (var _context = new MyContext())
                {
                    var dbCameras = _context.Camera.Where(c => c.DeviceId == did).ToList();
                    if (dbCameras.Count==0)
                    {
                        List<Camera> cameras = new List<Camera>();
                        foreach (var item in cameraInfos)
                        {
                            SmartWeb.Models.Camera cam = new SmartWeb.Models.Camera()
                            {
                                DeviceId = did,
                                Enable = item.enable,
                                Id = Guid.NewGuid().ToString(),
                                Index = item.index,
                                IP = item.ip,
                                NickName = string.IsNullOrEmpty(item.nickname) ? "摄像机" + item.index : item.nickname
                            };
                            cameras.Add(cam);
                        }
                        _context.Camera.AddRange(cameras);
                        _context.SaveChanges();
                        return 0;
                    }
                    else
                    {
                        List<Camera> cameras = new List<Camera>();
                        foreach (var item in cameraInfos)
                        {
                            if (!dbCameras.Where(c => c.Index == item.index).Any())
                            {
                                SmartWeb.Models.Camera cam = new SmartWeb.Models.Camera()
                                {
                                    DeviceId = did,
                                    Enable = item.enable,
                                    Id = Guid.NewGuid().ToString(),
                                    Index = item.index,
                                    IP = item.ip,
                                    NickName = string.IsNullOrEmpty(item.nickname) ? "摄像机" + item.index : item.nickname
                                };
                                cameras.Add(cam);
                            }
                            else
                            {
                                var cam = dbCameras.FirstOrDefault(c => c.Index == item.index);
                                if (cam.NickName != item.nickname && cam.Enable == item.enable)
                                {
                                    isSend = true;
                                }
                                cam.Enable = item.enable;
                                cam.NickName = item.nickname;
                                _context.Camera.Update(cam);

                            }
                        }
                        _context.Camera.AddRange(cameras);
                        _context.SaveChanges();
                        return 0;

                    }
                }
            }
            return 1;
        }
        /// <summary>
        /// 设备删除
        /// </summary>
        /// <param name="did"></param>
        /// <returns></returns>
        public static int DeviceDelete(string did)
        {
            using (var _context = new MyContext())
            {
                if (!string.IsNullOrEmpty(did))
                {
                    var embedded = _context.Device.FirstOrDefault(c => c.Id == did);
                    if (embedded != null)
                    {
                        var cameras = _context.Camera.Where(c => c.DeviceId == embedded.Id).ToList();
                        if (cameras.Count > 0)
                        {
                            string cids = string.Join(',', cameras.Select(c => c.Id));
                            var camconf = _context.Algorithm.Where(c => cids.Contains(c.Cid)).ToList();
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
        public static List<CrewInfo> CrewQuery(int uid = 0)
        {
            using (var _context = new MyContext())
            {
                List<CrewInfo> list = new List<CrewInfo>();
                //获取船员信息
                var dbempl = _context.Crew.Where(c => (uid > 0 ? c.Id == uid : 1 == 1)).ToList();
                //获取船员图片
                var pics = _context.CrewPicture.ToList();
                foreach (var item in dbempl)
                {
                    CrewInfo em = new CrewInfo()
                    {
                        job = item.Job,
                        name = item.Name,
                        uid = item.Id.ToString(),
                        pictures = new List<string>()
                    };
                    var picList = pics.Where(c => c.CrewId == item.Id);
                    foreach (var itpic in picList)
                    {
                        string pic = Convert.ToBase64String(itpic.Picture);
                        em.pictures.Add(pic);
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
        /// <returns>0：成功 1:失败 2:数据重复</returns>
        public static int CrewAdd(ref CrewInfo protoModel)
        {
            using (var _context = new MyContext())
            {
                var shipId = "";// _context.Ship.FirstOrDefault().Id;
                if (protoModel != null)
                {
                    string name = protoModel.name;
                    var dbemp = _context.Crew.FirstOrDefault(c => c.Name == name);
                    if (dbemp != null)
                    {
                        return 2;
                    }
                    int uid = Convert.ToInt32(protoModel.uid);
                    dbemp = _context.Crew.FirstOrDefault(c => c.Id == uid);
                    if (dbemp == null)
                    {
                        SmartWeb.Models.Crew employee = new SmartWeb.Models.Crew()
                        {
                            Id = uid,
                            Job = protoModel.job,
                            Name = protoModel.name
                        };
                        _context.Crew.Add(employee);
                        _context.SaveChanges();
                        protoModel.uid = employee.Id.ToString();
                        AddCrewPicture(protoModel.pictures, employee.Id);
                    }
                    return 0;
                }
                return 1;
            }
        }
        private static void AddCrewPicture(List<string> list, int crewId)
        {
            if (list != null && list.Count > 0)
            {
                using (var context = new MyContext())
                {
                    var pices = context.CrewPicture.Where(c => c.CrewId == crewId).ToList();
                    if (pices.Count > 0) context.RemoveRange(pices);
                    foreach (var item in list)
                    {
                        CrewPicture model = new SmartWeb.Models.CrewPicture()
                        {
                            CrewId = crewId,
                            Id = Guid.NewGuid().ToString(),
                            Picture = Convert.FromBase64String(item)
                        };
                        context.CrewPicture.Add(model);
                    }
                    context.SaveChanges();
                }
            }
        }
        /// <summary>
        /// 修改船员
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="protoModel"></param>
        /// <returns>0：成功 1:失败 2:数据重复</returns>
        public static int CrewUpdate(CrewInfo protoModel)
        {
            using (var _context = new MyContext())
            {
                if (protoModel != null)
                {
                    int uid = Convert.ToInt32(protoModel.uid);
                    var model = _context.Crew.FirstOrDefault(c => c.Id == uid);
                    if (model != null)
                    {
                        var dbmodel = _context.Crew.FirstOrDefault(c => c.Name == protoModel.name);
                        if (dbmodel != null && model.Name != protoModel.name)
                        {
                            return 2;
                        }
                        model.Job = protoModel.job;
                        model.Name = protoModel.name;
                        _context.Crew.Update(model);
                        _context.SaveChanges();
                        AddCrewPicture(protoModel.pictures,  uid);
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
        public static int CrewDelete(string id)
        {
            int uid = Convert.ToInt32(id);
            using (var _context = new MyContext())
            {
                if (uid > 0)
                {
                    var model = _context.Crew.FirstOrDefault(c => c.Id == uid);
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
        /// 查询摄像机的配置（算法配置）
        /// </summary>
        /// <param name="reland">是否返回给陆地</param>
        /// <returns></returns>
        public static List<AlgorithmInfo> AlgorithmQuery(bool reland=false,string typeName="")
        {
            using (var _context = new MyContext())
            {
                List<AlgorithmInfo> list = new List<AlgorithmInfo>();
                var config = from a in _context.Algorithm
                             join b in _context.Camera on a.Cid equals b.Id
                             where string.IsNullOrEmpty(typeName)?1==1:(typeName==ManagerHelp.FaceName?(a.Type==AlgorithmType.ATTENDANCE_IN||a.Type==AlgorithmType.ATTENDANCE_OUT) :a.Type== (AlgorithmType)Enum.Parse(typeof(AlgorithmType), typeName.ToUpper()))
                             select new
                             {
                                 a.Id,
                                 a.Cid,
                                 a.Type,
                                 a.GPU,
                                 a.DectectFirst,
                                 a.DectectSecond,
                                 a.Track,
                                 a.Similar,
                                 b.NickName,
                                 b.Index,
                                 b.DeviceId
                             };
                foreach (var item in config)
                {
                    string dbcid = item.Cid;
                    if (!reland) {
                        dbcid = item.DeviceId +":"+item.Cid+":"+ item.Index;
                    }
                    AlgorithmInfo cf = new AlgorithmInfo()
                    {
                        aid = item.Id,
                        cid = dbcid,
                        type = (AlgorithmInfo.Type)item.Type,
                        gpu = item.GPU,
                        similar = item.Similar,
                        track = item.Track,
                        dectectfirst = item.DectectFirst,
                        dectectsecond = item.DectectSecond
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
        public static int AlgorithmSet(AlgorithmInfo protoModel)
        {
            if (protoModel != null)
            {
                using (var context = new MyContext())
                {
                    try
                    {
                        var type = (SmartWeb.Models.AlgorithmType)protoModel.type;
                        if (protoModel.aid != "")
                        {
                            var algo = context.Algorithm.FirstOrDefault(c => c.Id == protoModel.aid);
                            if (algo == null) return 1;
                            if (protoModel.type == AlgorithmInfo.Type.ATTENDANCE_IN || protoModel.type == AlgorithmInfo.Type.ATTENDANCE_OUT)
                            {
                                var data = context.Algorithm.FirstOrDefault(c => c.Cid == protoModel.cid && (c.Type == AlgorithmType.ATTENDANCE_IN || c.Type == AlgorithmType.ATTENDANCE_OUT));
                                if (data != null && data.Id != algo.Id)
                                {
                                    return 2;
                                }
                            }
                            algo.GPU = protoModel.gpu;
                            algo.DectectFirst = protoModel.dectectfirst;
                            algo.DectectSecond = protoModel.dectectsecond;
                            algo.Track = protoModel.track;
                            algo.Similar = protoModel.similar;
                            algo.Type = (SmartWeb.Models.AlgorithmType)protoModel.type;
                            algo.Cid = protoModel.cid;
                            context.Algorithm.Update(algo);
                        }
                        else
                        {
                            //判断是否重复提交
                            var algorithm = context.Algorithm.FirstOrDefault(c => c.Cid == protoModel.cid &&
                                  c.GPU == protoModel.gpu &&
                                  c.Type == type);
                            if (algorithm != null) return 0;
                            if (protoModel.type == AlgorithmInfo.Type.ATTENDANCE_IN || protoModel.type == AlgorithmInfo.Type.ATTENDANCE_OUT)
                            {
                                var data = context.Algorithm.FirstOrDefault(c => c.Cid == protoModel.cid && (c.Type == AlgorithmType.ATTENDANCE_IN || c.Type == AlgorithmType.ATTENDANCE_OUT));
                                if (data != null)
                                {
                                    return 2;
                                }
                            }
                            var shipId = ""; //context.Ship.FirstOrDefault().Id;
                            SmartWeb.Models.Algorithm model = new SmartWeb.Models.Algorithm()
                            {
                                Cid = protoModel.cid,
                                GPU = protoModel.gpu,
                                Id = Guid.NewGuid().ToString(),
                                Similar = protoModel.similar,
                                Track = protoModel.track,
                                DectectSecond = protoModel.dectectsecond,
                                DectectFirst = protoModel.dectectfirst,
                                Type = type
                            };
                            context.Algorithm.Add(model);
                            protoModel.aid = model.Id;
                        }
                        context.SaveChanges();
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        return 1;
                    }
                }
            }
            return 1;
        }

        /// <summary>
        /// 新增组件
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="shipId"></param>
        /// <returns></returns>
        public static int ComponentAdd(string cid)
        {
            using (var context = new MyContext())
            {
                var component = context.Component.FirstOrDefault(c => c.Id == ManagerHelp.ComponentId);
                if (component != null)
                {
                    if (string.IsNullOrEmpty(component.Cid)) {
                        component.Cid = cid;
                        context.Update(component);
                        context.SaveChanges();
                    }
                }
                else
                {
                    string shipId = "";
                    if (ManagerHelp.IsShipPort)
                    {
                        var ship = context.Ship.FirstOrDefault();
                        shipId = ship != null ? ship.Id : "";
                    }
                    component = new SmartWeb.Models.Component()
                    {
                        Cid = cid,
                        Id = ManagerHelp.ComponentId,
                        Line = 0,
                        Name = "WEB",
                        Type = ComponentType.WEB,
                        ShipId = shipId
                    };
                    context.Add(component);
                    context.SaveChanges();
                }
                ManagerHelp.Cid = cid;
            }
            return 0;
        }
       
        /// <summary>
        /// 陆地端修改组件信息
        /// </summary>
        /// <param name="components"></param>
        public static void ComponentUpdateRange(List<ComponentInfo> components)
        {
            if (components != null)
            {
                using (var context = new MyContext())
                {
                    var list = context.Component.Where(c => c.Type != ComponentType.WEB).ToList();                  
                    //保存已在数据库在存在的组件ID
                    List<string> str = new List<string>();
                    //List<SmartWeb.Models.Component> comList = new List<SmartWeb.Models.Component>();

                    //判断流媒体是否启动
                    bool isMed = false;
                    if (components.Where(c=>c.type.Equals(ComponentInfo.Type.MED)).Any()) 
                    {
                        isMed = true;
                    }
                    foreach (var item in components)
                    {
                        if (item.componentid == ManagerHelp.ComponentId) continue;
                        if (item.type == ComponentInfo.Type.WEB) continue;
                        if (list.Where(c => c.Cid == item.componentid).Any())
                        {
                            #region 组件上线
                            var component = list.FirstOrDefault(c => c.Cid == item.componentid);
                            if (component.Line == 1)
                            {
                                component.Line = 0;
                                context.Component.Update(component);
                                context.SaveChanges();
                                if(isMed)SendInitData(item);
                            }
                            str.Add(item.componentid);
                            #endregion
                        }
                        //陆地端添加组件
                        else if (item.type == ComponentInfo.Type.XMQ)
                        {
                            #region 陆地端添加组件
                            string shipId =Guid.NewGuid().ToString();
                            Random rn = new Random();
                            rn.Next(1, 9999);
                            Ship ship = new Ship()
                            {
                                Coordinate = "",
                                CrewNum = 0,
                                Flag = false,
                                Id = shipId,
                                Name = string.IsNullOrEmpty(item.cname) ? "船" + rn : item.cname,
                                type = Ship.Type.AUTO
                            };
                            context.Ship.Add(ship);
                            SmartWeb.Models.Component component = new SmartWeb.Models.Component()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Cid=item.componentid,
                                Line = 0,
                                Name = item.cname,
                                Type = (ComponentType)item.type,
                                ShipId = shipId
                            };
                            context.Component.Add(component);
                            context.SaveChanges();
                            #endregion
                        }
                        else if (ManagerHelp.IsShipPort)
                        {
                            #region 船舶端添加组件
                            SmartWeb.Models.Component component = new SmartWeb.Models.Component()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Cid=item.componentid,
                                Line = 0,
                                Name = item.cname,
                                Type = (ComponentType)item.type,
                                ShipId = ""
                            };
                            context.Component.Add(component);
                            context.SaveChanges();
                            if (isMed) SendInitData(item);
                            #endregion
                        }
                    }
                    if (list.Count != str.Count)
                    {
                        //组件下线
                        ComponentOnline(components,list, str);
                    }
                }
            }
        }
        /// <summary>
        /// 自动发送船对应的组件到消息中心
        /// </summary>
        /// <param name="info"></param>
        private static void SendInitData(ComponentInfo info)
        {
            if (ManagerHelp.IsShipPort&&!ManagerHelp.isInit)
            {
                InitManger im = new InitManger();
                if (info.type==ComponentInfo.Type.MED|| info.type == ComponentInfo.Type.HKD)
                { 
                    //发送设备请求
                    bool flag = im.InitDevice();
                    if (flag)
                    {
                        Task.Factory.StartNew(t =>
                        {
                            while (ManagerHelp.DeviceResult == "")
                            {
                                Thread.Sleep(1000);
                            }
                            //收到设备后发送算法请求
                            im.InitAlgorithm();
                            ManagerHelp.isFaceAlgorithm = true;
                            ManagerHelp.DeviceResult = "";
                        }, TaskCreationOptions.LongRunning);
                    }

                }
                else if (info.type==ComponentInfo.Type.AI)
                {
                    im.InitAlgorithm(info.cname);
                    ManagerHelp.isFaceAlgorithm = true;
                }
            }
        }

        /// <summary>
        /// 组件下线
        /// </summary>
        /// <param name="components"></param>
        /// <param name="list"></param>
        /// <param name="str"></param>
        private static void ComponentOnline(List<ComponentInfo> components, List<SmartWeb.Models.Component> list, List<string> str)
        {
            using (var context=new MyContext())
            {
                if (ManagerHelp.IsShipPort)
                {
                    var delcom = list.Where(c => !str.Contains(c.Cid)).ToList();
                    foreach (var item in delcom)
                    {
                        item.Line = 1;
                        context.Update(item);
                        context.SaveChanges();
                    }
                }
                else
                {
                    if (components.Where(c => c.type == ComponentInfo.Type.XMQ).Any())
                    {
                        var delcom = list.Where(c => !str.Contains(c.Cid)).ToList();
                        foreach (var item in delcom)
                        {
                            item.Line = 1;
                            context.Update(item);
                            context.SaveChanges();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 陆地端设备船信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static int ShipSet(StatusRequest request,ref Ship ship)
        {
            if (request == null) return 1;
            using (var _context = new MyContext())
            {
                //修改船状态信息
                ship = _context.Ship.FirstOrDefault();
                if (ship != null)
                {
                    if (request.text != "" && request.type == StatusRequest.Type.NAME)
                    {
                        ship.Name = request.text;
                        _context.Ship.Update(ship);
                        _context.SaveChanges();
                    }
                    else if (request.type == StatusRequest.Type.SAIL)
                    {
                        ship.type = (SmartWeb.Models.Ship.Type)request.flag;
                        ship.Flag = request.flag == 1 ? true : false;
                        _context.Ship.Update(ship);
                        _context.SaveChanges();
                    }
                }
                return 0;
            }
        }

        public static int ShipSet(bool status)
        {
            try
            {
                using (var _context = new MyContext())
                {
                    //修改船状态信息
                    var ship = _context.Ship.FirstOrDefault();
                    if (ship != null)
                    {
                        ship.Flag = status;
                        _context.Ship.Update(ship);
                        _context.SaveChanges();
                    }
                    return 0;
                }
            }
            catch (Exception ex)
            {
                return 1;
            }
        }
        /// <summary>
        /// 查询船状态
        /// </summary>
        /// <param name="shipId"></param>
        /// <returns></returns>
        public static Ship StatusQuery()
        {
            using (var context = new MyContext())
            {
                var ship = context.Ship.FirstOrDefault();
                return ship;
            }
        }

        /// <summary>
        /// 添加protobuf接收日志
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="t"></param>
        public static void AddReceiveLog<T>(string key, T t, string ErrMsg = "")
        {
            try
            {
                var values = JsonConvert.SerializeObject(t);
                using (var context = new MyContext())
                {
                    ReceiveLog log = new ReceiveLog()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Key = key,
                        Values = values,
                        Exception = ErrMsg,
                        Time = DateTime.Now
                    };
                    context.ReceiveLog.Add(log);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// 添加protobuf接收日志
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="t"></param>
        public static void AddReceiveLog(string key,string values="",string errMsg="")
        {
            try
            {
                using (var context = new MyContext())
                {
                    ReceiveLog log = new ReceiveLog()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Key = key,
                        Values = values,
                        Exception = errMsg,
                        Time = DateTime.Now
                    };
                    context.ReceiveLog.Add(log);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// 将缺岗数据插入报警表
        /// </summary>
        /// <param name="captureInfo"></param>
        public void CaptureAdd(CaptureInfo captureInfo, string xmq)
        {
            try
            {
                if (captureInfo != null)
                {
                    using (var context = new MyContext())
                    {
                        string shipId = "";
                        string cname = "";
                        string cid = "";
                        if (ManagerHelp.IsShipPort)
                        {
                            cid = captureInfo.cid;
                            var camera = context.Camera.FirstOrDefault(c => c.Id == captureInfo.cid);
                            if (camera != null) cname = camera.NickName;
                        }
                        if (cid == "") return;
                        SmartWeb.Models.Alarm alarm = new SmartWeb.Models.Alarm()
                        {
                            Type = SmartWeb.Models.Alarm.AlarmType.CAPTURE,
                            Cid = cid,
                            Cname = cname,
                            Id = Guid.NewGuid().ToString(),
                            Picture = Convert.FromBase64String(captureInfo.picture),
                            ShipId = shipId,
                            Time = DateTime.Now
                        };
                        context.Alarm.Add(alarm);
                        context.SaveChanges();                       
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// AIS数据
        /// </summary>
        /// <param name="ais"></param>
        public void AisAdd(Ais ais) 
        {
            if (ais!=null)
            {
                try
                {
                    using (var context = new MyContext())
                    {
                        var ship = context.Ship.FirstOrDefault();
                        bool flag= ais.status == 0 ? true : false;
                        //当航行状态为自动时才将AIS的状态保存到数据库中
                        if (ship != null && ship.type == 0&&ship.Flag!=flag)
                        {
                            ship.Flag = flag;
                            context.Update(ship);
                            context.SaveChanges();
                            //船状态修改时要推送消息给算法
                            InitManger im = new InitManger();
                            im.InitStatus();
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddReceiveLog("AIS", "", "写入数据失败!" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 获取所有AI组件
        /// </summary>
        /// <returns></returns>
        public static List<SmartWeb.Models.Component> GetComponentByAI()
        {
            List<SmartWeb.Models.Component> components = new List<SmartWeb.Models.Component>();
            using (var context = new MyContext())
            {
                components = context.Component.Where(c => c.Type == ComponentType.AI && c.Line == 0).ToList();
            }
            return components;
        }
        /// <summary>
        /// 获取组件信息
        /// </summary>
        /// <param name="componentId"></param>
        /// <returns></returns>
        public static SmartWeb.Models.Component GetComponentById(string componentId)
        {
            using (var context = new MyContext())
            {
                SmartWeb.Models.Component component = context.Component.FirstOrDefault(c => c.Cid == componentId);
                return component;
            }
        }
    }
}
