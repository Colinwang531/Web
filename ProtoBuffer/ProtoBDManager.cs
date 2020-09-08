using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Ocsp;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.ProtoBuffer.Models;
using ShipWeb.Tool;
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
        /// 查询所有设备
        /// </summary>
        /// <returns></returns>
        public static List<DeviceInfo> DeviceQuery(DeviceInfo info, string did = "")
        {
            using (var _context = new MyContext())
            {
                List<DeviceInfo> list = new List<DeviceInfo>();
                //查询设备信息
                List<ShipWeb.Models.Device> devList = GetDevice(info);               
                if (!string.IsNullOrEmpty(did))
                {
                    devList = devList.Where(c => c.Id == did).ToList();
                }
               
                var ids = string.Join(',', devList.Select(c => c.Id));
                //查询设备下的摄像机
                var cameras = _context.Camera.Where(c =>ids.Contains(c.DeviceId)).ToList();
                foreach (var item in devList)
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
                        enable=item.Enable,
                        camerainfos = new List<CameraInfo>()
                    };
                    var cams = cameras.Where(c => c.DeviceId == item.Id);
                    foreach (var it in cams)
                    {
                        CameraInfo cam = new CameraInfo()
                        {
                            cid =it.Id,
                            enable = it.Enable,
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
        private static List<ShipWeb.Models.Device> GetDevice(DeviceInfo info) 
        {
            using (var context=new MyContext())
            {
                var device = context.Device.Where(c=>1==1);
                if (info != null)
                {
                    device=device.Where(c => c.Enable == info.enable);
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
                        device = device.Where(c => c.IP==info.ip);
                    }
                    if (!string.IsNullOrEmpty(info.did))
                    {
                        device = device.Where(c => c.Id == info.did);
                    }
                    if (info.port>0)
                    {
                        device = device.Where(c => c.Port==info.port);
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
        public static int DeviceAdd(DeviceInfo protoModel)
        {
            using (var _context = new MyContext())
            {
                var shipId = _context.Ship.FirstOrDefault().Id;
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
                        Enable=protoModel.enable,
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
                                Enable = item.enable,
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
        public static int DeviceUpdate(string did, DeviceInfo protoModel) 
        {
            using (var _context = new MyContext())
            {
                if (!string.IsNullOrEmpty(did) && protoModel != null)
                {
                    var model = _context.Device.FirstOrDefault(c =>c.Id == did);
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
                        model.Enable = protoModel.enable;
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
                                carmera.Enable = item.enable;
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
        public static int DeviceDelete(string did) 
        {
            using (var _context = new MyContext())
            {
                if (!string.IsNullOrEmpty(did))
                {
                    var embedded = _context.Device.FirstOrDefault(c =>c.Id == did);
                    if (embedded != null)
                    {
                        var cameras = _context.Camera.Where(c => c.DeviceId == embedded.Id).ToList();
                        if (cameras.Count > 0)
                        {
                            string cids = string.Join(',', cameras.Select(c => c.Id));
                            var camconf = _context.Algorithm.Where(c =>cids.Contains(c.Cid)).ToList();
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
        public static List<CrewInfo> CrewQuery(string uid = "")
        {
            using (var _context = new MyContext())
            {
                List<CrewInfo> list = new List<CrewInfo>();
                //获取船员信息
                var dbempl = _context.Crew.Where(c =>(!string.IsNullOrEmpty(uid) ? c.Id == uid : 1 == 1)).ToList();
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
        /// <returns>0：成功 1:失败 2:数据重复</returns>
        public static int CrewUpdate( CrewInfo protoModel)
        {
            using (var _context = new MyContext())
            {
                if (protoModel != null)
                {
                    var model = _context.Crew.FirstOrDefault(c =>c.Id == protoModel.uid);
                    if (model != null)
                    {
                        var dbmodel = _context.Crew.FirstOrDefault(c => c.Name == protoModel.name);
                        if (dbmodel!=null&&model.Name!=protoModel.name)
                        {
                            return 2;
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
                            _context.CrewPicture.RemoveRange(delPicList);
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
        public static int CrewDelete(string uid) 
        {
            using (var _context = new MyContext())
            {
                if (!string.IsNullOrEmpty(uid))
                {
                    var model = _context.Crew.FirstOrDefault(c =>c.Id == uid);
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
        public static List<AlgorithmInfo> CameraConfigQuery() 
        {
            using (var _context = new MyContext())
            {
                List<AlgorithmInfo> list = new List<AlgorithmInfo>();
                var config = from a in _context.Algorithm
                             join b in _context.Camera on a.Cid equals b.Id
                             into c
                             from d in c.DefaultIfEmpty()
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
                                 d.NickName
                             };
                foreach (var item in config)
                {
                    AlgorithmInfo cf = new AlgorithmInfo()
                    {
                        cid = item.Id + "," + item.Cid + "," + item.NickName,
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
        public static int CameraConfigSet(AlgorithmInfo protoModel) 
        {
            using (var _context = new MyContext())
            {
                try
                {
                    if (protoModel!=null)
                    {                       
                        if (protoModel.cid.Split(',') .Length>1)
                        {
                            string id = protoModel.cid.Split(',')[0];
                            string cid = protoModel.cid.Split(',')[1];
                           
                            var algo = _context.Algorithm.FirstOrDefault(c => c.Id == id);
                            if (algo == null) return 1;
                            if (protoModel.type == AlgorithmInfo.Type.ATTENDANCE_IN || protoModel.type == AlgorithmInfo.Type.ATTENDANCE_OUT)
                            {
                                var data = _context.Algorithm.FirstOrDefault(c =>c.Cid == cid && (c.Type == AlgorithmType.ATTENDANCE_IN || c.Type == AlgorithmType.ATTENDANCE_OUT));
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
                            algo.Type = (ShipWeb.Models.AlgorithmType)protoModel.type;
                            algo.Cid = cid;
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
                            ShipWeb.Models.Algorithm model = new ShipWeb.Models.Algorithm()
                            {
                                Cid = protoModel.cid,
                                GPU = protoModel.gpu,
                                Id = Guid.NewGuid().ToString(),
                                ShipId = shipId,
                                Similar = protoModel.similar,
                                Track = protoModel.track,
                                DectectSecond = protoModel.dectectsecond,
                                DectectFirst = protoModel.dectectfirst,
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
        /// 陆地端设备船信息
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
                            Flag = request.flag==1?true:false,
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
                                ship.type = (ShipWeb.Models.Ship.Type)request.flag;
                                ship.Flag = request.flag == 1 ? true : false;
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

        /// <summary>
        /// 查询船状态
        /// </summary>
        /// <param name="shipId"></param>
        /// <returns></returns>
        public static Ship StatusQuery() 
        {
            using (var context=new MyContext())
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
        public static void AlarmAdd(MSG msg)
        {
            using (var context = new MyContext())
            {
                var ship = context.Component.FirstOrDefault(c => c.Id == ManagerHelp.Cid);
                string shipId = ship.Id;
                string identity = Guid.NewGuid().ToString();
                if (msg != null && msg.type == MSG.Type.ALARM && msg.alarm != null)
                {
                    var alarm = msg.alarm.alarminfo;
                    if (alarm.type == ProtoBuffer.Models.AlarmInfo.Type.ATTENDANCE_IN || alarm.type == ProtoBuffer.Models.AlarmInfo.Type.ATTENDANCE_OUT)
                    {
                        #region 考勤信息入库
                        ShipWeb.Models.Attendance attendance = new Attendance()
                        {
                            Behavior = alarm.type == ProtoBuffer.Models.AlarmInfo.Type.ATTENDANCE_IN ? 0 : 1,
                            Id = identity,
                            CameraId = alarm.cid,
                            ShipId = shipId,
                            Time = Convert.ToDateTime(alarm.time),
                            CrewId = alarm.uid,
                            attendancePictures = new List<AttendancePicture>()
                                        {
                                            new AttendancePicture ()
                                            {
                                                 AttendanceId=identity,
                                                 Id=Guid.NewGuid().ToString(),
                                                 Picture= Encoding.UTF8.GetBytes(alarm.picture),
                                                 ShipId=shipId
                                            }
                                        }
                        };
                        context.Attendance.Add(attendance);
                        #endregion
                    }
                    else
                    {
                        #region 报警信息入库
                        ShipWeb.Models.Alarm model = new ShipWeb.Models.Alarm()
                        {
                            Id = identity,
                            Picture = Encoding.UTF8.GetBytes(alarm.picture),
                            Time = Convert.ToDateTime(alarm.time),
                            ShipId = shipId,
                            Cid = alarm.cid,
                            Type = (ShipWeb.Models.Alarm.AlarmType)alarm.type,
                            Uid = alarm.uid
                        };
                        var replist = alarm.position;
                        if (replist.Count > 0)
                        {
                            foreach (var item in replist)
                            {

                                ShipWeb.Models.AlarmPosition position = new ShipWeb.Models.AlarmPosition()
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
                        #endregion
                    }
                    context.SaveChanges();
                }
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
            using (var context=new MyContext())
            {
                var alarms = context.Alarm.Where(c => c.Time >= dtStart && c.Time <= dtEnd).ToList();
                var ids = string.Join(',', alarms.Select(c => c.Id));
                var postions = context.AlarmPosition.Where(c => ids.Contains(c.AlarmId)).ToList();
                foreach (var item in alarms)
                {
                    AlarmInfo info=new AlarmInfo()
                    {
                        cid = item.Cid,
                        time = item.Time.ToString("yyyy-MM-dd HH24:mm:ss"),
                        type = (AlarmInfo.Type)item.Type,
                        position = new List<Models.AlarmPosition>(),
                        picture=Encoding.UTF8.GetString(item.Picture)
                    };
                    var pos = postions.Where(c => c.AlarmId == item.Id);
                    foreach (var poitem in pos)
                    {
                        info.position.Add(new Models.AlarmPosition()
                        {
                             w=poitem.W,
                             h=poitem.H,
                             x=poitem.X,
                             y=poitem.Y
                        });
                    }
                    list.Add(info);
                }
            }
            return list;
        }
    }
}
