using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Ocsp;
using Smartweb.Helpers;
using Smartweb.Hubs;
using SmartWeb.DB;
using SmartWeb.Models;
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
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer
{
    public class ProtoBDManager
    {
        private readonly MemoryCacheHelper cache = new MemoryCacheHelper();
        private readonly IHubContext<AlarmVoiceHub> hubContext;

        public ProtoBDManager()
        {
        }
        public ProtoBDManager(IHubContext<AlarmVoiceHub> _hubContext)
        {
            this.hubContext = _hubContext;
        }


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
                                NickName = cam.NickName,
                                ShipId = cam.ShipId
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
                    string shipId = _context.Ship.FirstOrDefault().Id;
                    SmartWeb.Models.Device model = new SmartWeb.Models.Device();
                    model.factory = (SmartWeb.Models.Device.Factory)protoModel.factory;
                    model.IP = protoModel.ip;
                    model.Name = protoModel.name;
                    model.Nickname = protoModel.nickname;
                    model.Password = protoModel.password;
                    model.Port = protoModel.port;
                    model.type = (SmartWeb.Models.Device.Type)protoModel.type;
                    model.Enable = protoModel.enable;
                    model.Id = Guid.NewGuid().ToString();
                    model.ShipId = shipId; ;
                    if (protoModel == null) _context.Device.Add(model);
                    else _context.Device.Update(model);
                    _context.SaveChanges();
                    if (protoModel.camerainfos != null && protoModel.camerainfos.Count > 0)
                    {
                        AddCameras(protoModel.camerainfos, protoModel.did);
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
                    }
                    _context.SaveChanges();
                    AddCameras(protoModel.camerainfos, did);
                    return model;
                }
                return null;
            }
        }

        /// <summary>
        /// 添加摄像机
        /// </summary>
        /// <param name="cameraInfos"></param>
        /// <param name="did"></param>
        /// <returns></returns>
        public static int AddCameras(List<CameraInfo> cameraInfos, string did)
        {
            if (cameraInfos != null)
            {
                using (var _context = new MyContext())
                {
                    var dbCameras = _context.Camera.Where(c => c.DeviceId == did).ToList();
                    if (cameraInfos.Count != dbCameras.Count)
                    {
                        List<Camera> cameras = new List<Camera>();
                        string shipId = _context.Ship.FirstOrDefault().Id;
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
                                    NickName = string.IsNullOrEmpty(item.nickname) ? "摄像机" + item.index : item.nickname,
                                    ShipId = shipId
                                };
                                cameras.Add(cam);
                            }
                        }
                        _context.Camera.AddRange(cameras);
                        _context.SaveChanges();
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
        public static int CrewAdd(CrewInfo protoModel)
        {
            using (var _context = new MyContext())
            {
                var shipId = _context.Ship.FirstOrDefault().Id;
                if (protoModel != null)
                {
                    var dbemp = _context.Crew.FirstOrDefault(c => c.Name == protoModel.name);
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
                            Name = protoModel.name,
                            ShipId = shipId
                        };
                        _context.Crew.Add(employee);
                        _context.SaveChanges();
                    }
                    AddCrewPicture(protoModel.pictures, shipId, uid);
                    return 0;
                }
                return 1;
            }
        }
        private static void AddCrewPicture(List<string> list, string shipId, int crewId)
        {
            if (list != null && list.Count > 0)
            {
                using (var context = new MyContext())
                {
                    var pices = context.CrewPicture.Where(c => c.CrewId == crewId).ToList();
                    if (pices.Count > 0) context.RemoveRange(pices);
                    foreach (var item in list)
                    {
                        CrewPicture model= new SmartWeb.Models.CrewPicture()
                        {
                            CrewId = crewId,
                            Id = Guid.NewGuid().ToString(),
                            Picture = Convert.FromBase64String(item),
                            ShipId = shipId
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
                        AddCrewPicture(protoModel.pictures, model.ShipId, uid);
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
        /// <returns></returns>
        public static List<AlgorithmInfo> AlgorithmQuery()
        {
            using (var _context = new MyContext())
            {
                List<AlgorithmInfo> list = new List<AlgorithmInfo>();
                var config = from a in _context.Algorithm
                             join b in _context.Camera on a.Cid equals b.Id
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
                    AlgorithmInfo cf = new AlgorithmInfo()
                    {
                        aid = item.Id + "," + item.NickName,
                        cid = item.DeviceId + ":" + item.Cid + ":" + item.Index,
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
                using (var _context = new MyContext())
                {
                    try
                    {
                        if (protoModel.aid != "")
                        {
                            var algo = _context.Algorithm.FirstOrDefault(c => c.Id == protoModel.aid);
                            if (algo == null) return 1;
                            if (protoModel.type == AlgorithmInfo.Type.ATTENDANCE_IN || protoModel.type == AlgorithmInfo.Type.ATTENDANCE_OUT)
                            {
                                var data = _context.Algorithm.FirstOrDefault(c => c.Cid == protoModel.cid && (c.Type == AlgorithmType.ATTENDANCE_IN || c.Type == AlgorithmType.ATTENDANCE_OUT));
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
                            _context.Algorithm.Update(algo);
                        }
                        else
                        {
                            if (protoModel.type == AlgorithmInfo.Type.ATTENDANCE_IN || protoModel.type == AlgorithmInfo.Type.ATTENDANCE_OUT)
                            {
                                var data = _context.Algorithm.FirstOrDefault(c => c.Cid == protoModel.cid && (c.Type == AlgorithmType.ATTENDANCE_IN || c.Type == AlgorithmType.ATTENDANCE_OUT));
                                if (data != null)
                                {
                                    return 2;
                                }
                            }
                            var shipId = _context.Ship.FirstOrDefault().Id;
                            SmartWeb.Models.Algorithm model = new SmartWeb.Models.Algorithm()
                            {
                                Cid = protoModel.cid,
                                GPU = protoModel.gpu,
                                Id = Guid.NewGuid().ToString(),
                                ShipId = shipId,
                                Similar = protoModel.similar,
                                Track = protoModel.track,
                                DectectSecond = protoModel.dectectsecond,
                                DectectFirst = protoModel.dectectfirst,
                                Type = (SmartWeb.Models.AlgorithmType)protoModel.type
                            };
                            _context.Algorithm.Add(model);
                            protoModel.aid = model.Id;
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
                    component.Cid = cid;
                    context.Update(component);
                    context.SaveChanges();
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
        /// 船舶端添加组件
        /// </summary>
        /// <param name="components"></param>
        /// <returns></returns>
        public static int ComponentAddRange(List<ComponentInfo> components)
        {
            using (var context = new MyContext())
            {
                if (components == null) return 1;
                if (components.Count > 1)
                {
                    var com = context.Component.Where(c => c.Type != ComponentType.WEB);
                    context.Component.RemoveRange(com);
                    string shipId = "";
                    var ship = context.Ship.FirstOrDefault();
                    if (ship != null)
                    {
                        shipId = ship.Id;
                    }
                    List<SmartWeb.Models.Component> list = new List<SmartWeb.Models.Component>();
                    //获取到的数据除WEB外，如果组件重复，那么它的通讯ID取最后一个
                    foreach (var item in components)
                    {
                        if (item.type == ComponentInfo.Type.WEB) continue;
                        if (!list.Where(c => c.Id == item.componentid).Any())
                        {
                            SmartWeb.Models.Component model = new SmartWeb.Models.Component()
                            {
                                Id = item.componentid,
                                Name = item.cname,
                                Type = (ComponentType)item.type,
                                ShipId = shipId
                            };
                            list.Add(model);
                        }
                    }
                    context.Component.AddRange(list);
                    context.SaveChanges();
                }
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
                    var list = context.Component.Where(c =>c.Type!=ComponentType.WEB).ToList();
                    string shipId = "";
                    if (ManagerHelp.IsShipPort)
                    {
                        var ship = context.Ship.FirstOrDefault();
                        if (ship != null)
                        {
                            shipId = ship.Id;
                        }
                    }
                    //保存已在数据库在存在的组件ID
                    List<string> str = new List<string>();
                    List<SmartWeb.Models.Component> comList = new List<SmartWeb.Models.Component>();
                    foreach (var item in components)
                    {
                        if (item.componentid == ManagerHelp.Cid) continue;
                        if (list.Where(c=>c.Id==item.componentid).Any())
                        {
                            var component=list.FirstOrDefault(c => c.Id == item.componentid);
                            component.Line = 0;
                            context.Component.Update(component);
                            str.Add(item.componentid);
                        }
                        //陆地端添加组件
                        else if (item.type== ComponentInfo.Type.XMQ)
                        {
                            #region 陆地端添加组件
                            shipId = Guid.NewGuid().ToString();
                            Random rn = new Random();
                            rn.Next(1, 9999);
                            Ship ship = new Ship()
                            {
                                Coordinate = "",
                                CrewNum = 0,
                                Flag = false,
                                Id = shipId,
                                Name = string.IsNullOrEmpty(item.cname) ? "船"+rn : item.cname,
                                type = Ship.Type.AUTO
                            };
                            context.Ship.Add(ship);
                            SmartWeb.Models.Component component = new SmartWeb.Models.Component()
                            {
                                Id = item.componentid,
                                Line = 0,
                                Name = item.cname,
                                Type = (ComponentType)item.type,
                                ShipId = shipId
                            };
                            comList.Add(component);
                            #endregion
                        }
                        else if(ManagerHelp.IsShipPort)
                        {
                            //船舶端添加组件
                            SmartWeb.Models.Component component = new SmartWeb.Models.Component()
                            {
                                Id = item.componentid,
                                Line = 0,
                                Name = item.cname,
                                Type = (ComponentType)item.type,
                                ShipId = shipId
                            };
                            comList.Add(component);
                        }
                    }
                    if (list.Count != str.Count)
                    {
                        var delcom= list.Where(c => !str.Contains(c.Id)).ToList();
                        foreach (var item in delcom)
                        {
                            item.Line = 1;
                            context.Update(item);
                            context.SaveChanges();
                        }
                    }
                    //批量保存
                    if (comList.Count > 0 )
                    {
                        context.Component.AddRange(comList);
                        context.SaveChanges();
                    } 
                }
            }
        }
        /// <summary>
        /// 陆地端设备船信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static int ShipSet(StatusRequest request)
        {
            if (request == null) return 1;
            using (var _context = new MyContext())
            {
                //修改船状态信息
                var ship = _context.Ship.FirstOrDefault();
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
        /// 报警考勤入库
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="cid"></param>
        public void AlarmAdd(AlarmInfo alarmInfo, string xmq = "")
        {
            //添加日志
            ProtoBDManager.AddReceiveLog<SmartWeb.ProtoBuffer.Models.AlarmInfo>("Alarm", alarmInfo);
            using (var context = new MyContext())
            {
                string shipId = "";
                if (string.IsNullOrEmpty(xmq))
                {
                    var ship = context.Ship.FirstOrDefault();
                    shipId = ship.Id;
                }
                else
                {
                    var comp = context.Component.FirstOrDefault(c => c.Id == xmq);
                    if (comp != null)
                    {
                        shipId = comp.ShipId;
                    }
                }
                if (alarmInfo != null)
                {
                    var ids = alarmInfo.cid.Split(':');
                    string cid = alarmInfo.cid;
                    if (ids.Length == 2)
                    {
                        var cam = context.Camera.FirstOrDefault(c => c.DeviceId == ids[0] && c.Index == Convert.ToInt32(ids[1]));
                        if (cam != null)
                        {
                            cid = cam.Id;
                        };
                    }
                    string str = Encoding.ASCII.GetString(alarmInfo.picture);
                    byte[] picture = ManagerHelp.ConvertBase64(str);
                    if (alarmInfo.type == AlarmInfo.Type.ATTENDANCE_IN || alarmInfo.type == AlarmInfo.Type.ATTENDANCE_OUT)
                    {
                        //考勤信息入库
                        AddAttendance(alarmInfo, context, shipId, cid, picture);
                    }
                    else
                    {
                        #region 报警信息入库                       
                        SmartWeb.Models.Alarm model = new SmartWeb.Models.Alarm()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Picture = picture,
                            Time = Convert.ToDateTime(alarmInfo.time),
                            ShipId = shipId,
                            Cid = cid,
                            Type = (SmartWeb.Models.Alarm.AlarmType)alarmInfo.type
                            //Uid = alarmInfo.uid
                        };
                        var replist = alarmInfo.alarmposition;
                        if (replist != null && replist.Count > 0)
                        {
                            model.alarmPositions = new List<SmartWeb.Models.AlarmPosition>();
                            foreach (var item in replist)
                            {
                                SmartWeb.Models.AlarmPosition position = new SmartWeb.Models.AlarmPosition()
                                {
                                    AlarmId = model.Id,
                                    ShipId = shipId,
                                    Id = Guid.NewGuid().ToString(),
                                    H = item.h,
                                    W = item.w,
                                    X = item.x,
                                    Y = item.y
                                };
                                model.alarmPositions.Add(position);
                            }
                        }
                        context.Alarm.Add(model);
                        context.SaveChanges();
                        SendDataMsg sendData = new SendDataMsg();
                        //向陆地端推送报警信息
                        sendData.SendAlarm("upload", alarmInfo);
                        #endregion
                        //change
                        string cidkey = cache.Get("shipOnlineKey")?.ToString();
                        if (!string.IsNullOrEmpty(cidkey))
                            hubContext.Clients.Client(cidkey).SendAsync("ReceiveAlarmVoice", 200, new { code = 1, type = "bonvoyageSleep", });
                    }
                }
            }
        }
        /// <summary>
        /// 添加考勤信息
        /// </summary>
        /// <param name="alarmInfo"></param>
        /// <param name="context"></param>
        /// <param name="shipId"></param>
        /// <param name="identity"></param>
        /// <param name="cid"></param>
        /// <param name="picture"></param>
        private static void AddAttendance(AlarmInfo alarmInfo, MyContext context, string shipId, string cid, byte[] picture)
        {
            if (alarmInfo.uid != "")
            {
                byte[] array = new byte[1];
                array = System.Text.Encoding.ASCII.GetBytes(alarmInfo.uid);
                //得到船员ID
                int uid = (short)(array[0]);

                #region 考勤信息入库

                string identity = Guid.NewGuid().ToString();
                DateTime dt = Convert.ToDateTime(alarmInfo.time);
                DateTime dtStart = DateTime.Parse(dt.ToString("yyyy-MM-dd 00:00:00"));
                DateTime dtEnd = DateTime.Parse(dt.ToString("yyyy-MM-dd 23:59:59"));
                //重复打卡只取最后一次
                var attes = context.Attendance.FirstOrDefault(c => c.CameraId == alarmInfo.cid && c.CrewId == uid && (c.Time > dtStart && c.Time <= dtEnd));
                if (attes != null)
                {
                    attes.Time = DateTime.Parse(alarmInfo.time);
                    var pic = context.AttendancePicture.Where(c => c.AttendanceId == attes.Id).ToList();
                    if (pic.Count > 0)
                    {
                        context.AttendancePicture.RemoveRange(pic);
                    }
                    if (alarmInfo.picture.Length > 0)
                    {
                        AttendancePicture ap = new AttendancePicture()
                        {
                            AttendanceId = attes.Id,
                            Id = Guid.NewGuid().ToString(),
                            Picture = picture,
                            ShipId = shipId
                        };
                        context.AttendancePicture.Add(ap);
                    }
                    context.Attendance.Update(attes);
                }
                else
                {
                    SmartWeb.Models.Attendance attendance = new Attendance()
                    {
                        Behavior = alarmInfo.type == ProtoBuffer.Models.AlarmInfo.Type.ATTENDANCE_IN ? 0 : 1,
                        Id = identity,
                        CameraId = cid,
                        ShipId = shipId,
                        Time = Convert.ToDateTime(alarmInfo.time),
                        CrewId = uid,
                        attendancePictures = new List<AttendancePicture>()
                                    {
                                        new AttendancePicture ()
                                        {
                                             AttendanceId=identity,
                                             Id=Guid.NewGuid().ToString(),
                                             Picture= picture,
                                             ShipId=shipId
                                        }
                                    }
                    };
                    context.Attendance.Add(attendance);
                    context.SaveChanges();
                }
                #endregion
                #region 将考勤数据存入内存中
                if (alarmInfo.type == AlarmInfo.Type.ATTENDANCE_IN && (!ManagerHelp.atWorks.Where(c => c.Uid == uid).Any()))
                {
                    ManagerHelp.atWorks.Add(new AtWork()
                    {
                        Uid = uid,
                        Line = 1
                    });
                }
                else if (alarmInfo.type == AlarmInfo.Type.ATTENDANCE_OUT && ManagerHelp.atWorks.Where(c => c.Uid == uid).Any())
                {
                    var atwork = ManagerHelp.atWorks.FirstOrDefault(c => c.Uid == uid);
                    ManagerHelp.atWorks.Remove(atwork);
                }
                #endregion
                #region 发送考勤给IPad
                var crew = context.Crew.FirstOrDefault(c => c.Id == uid);
                if (crew != null)
                {
                    PublisherService service = new PublisherService();
                    //考勤类型
                    int Behavior = (alarmInfo.type == AlarmInfo.Type.ATTENDANCE_IN) ? 0 : 1;
                    //考勤时间
                    string SignInTime = alarmInfo.time;
                    //考勤人员
                    string EmployeeName = crew.Name;
                    //考勤图片
                    string PhotosBuffer = Convert.ToBase64String(picture);
                    string data = Behavior + "," + SignInTime + "," + EmployeeName + "," + PhotosBuffer;
                    service.Send(data);
                }

                #endregion
            }
        }

        /// <summary>
        /// 查询船舶端报警消息
        /// </summary>
        /// <param name="dtStart"></param>
        /// <param name="endTime"></param>
        /// <returns>protobuf报警消息</returns>
        public static List<AlarmInfo> GetAlarmInfo(DateTime dtStart, DateTime dtEnd)
        {
            List<AlarmInfo> list = new List<AlarmInfo>();
            using (var context = new MyContext())
            {
                var alarms = context.Alarm.Where(c => c.Time >= dtStart && c.Time <= dtEnd).ToList();
                var ids = string.Join(',', alarms.Select(c => c.Id));
                var postions = context.AlarmPosition.Where(c => ids.Contains(c.AlarmId)).ToList();
                foreach (var item in alarms)
                {
                    AlarmInfo info = new AlarmInfo()
                    {
                        cid = item.ShipId + "," + item.Cid,
                        time = item.Time.ToString("yyyy-MM-dd HH24:mm:ss"),
                        type = (AlarmInfo.Type)item.Type,
                        alarmposition = new List<Models.AlarmPosition>(),
                        //picture=item.Picture
                    };
                    var pos = postions.Where(c => c.AlarmId == item.Id);
                    foreach (var poitem in pos)
                    {
                        info.alarmposition.Add(new Models.AlarmPosition()
                        {
                            w = poitem.W,
                            h = poitem.H,
                            x = poitem.X,
                            y = poitem.Y
                        });
                    }
                    list.Add(info);
                }
            }
            return list;
        }

        /// <summary>
        /// 添加protobuf接收日志
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="t"></param>
        public static void AddReceiveLog<T>(string key, T t, string ErrMsg = "")
        {
            var values = JsonConvert.SerializeObject(t);
            using (var context=new MyContext())
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
        /// <summary>
        /// 将缺岗数据插入报警表
        /// </summary>
        /// <param name="captureInfo"></param>
        public static void CaptureAdd(CaptureInfo captureInfo)
        {
            try
            {
                if (captureInfo != null)
                {
                    using (var context = new MyContext())
                    {
                        var ship = context.Ship.FirstOrDefault();
                        SmartWeb.Models.Alarm alarm = new SmartWeb.Models.Alarm()
                        {
                            Type = SmartWeb.Models.Alarm.AlarmType.CAPTURE,
                            Cid = captureInfo.cid,
                            Id = Guid.NewGuid().ToString(),
                            Picture = Convert.FromBase64String(captureInfo.picture),
                            ShipId = ship == null ? "" : ship.Id,
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

    }
}
