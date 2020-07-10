using ShipWeb.DB;
using ShipWeb.ProtoBuffer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer
{
    public class ProtoBDManager
    {
        private static MyContext _context = new MyContext();

        /// <summary>
        /// 添加组件信息
        /// </summary>
        /// <param name="componentInfo"></param>
        /// <returns></returns>
        public static string AddComponent(ComponentInfo componentInfo)
        {
            string cid = Process.GetCurrentProcess().Id.ToString();
            string shipId = Guid.NewGuid().ToString();
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
        public static List<Embedded> EmbeddedQuery(string did="")
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
