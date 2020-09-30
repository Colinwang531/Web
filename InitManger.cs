
using Microsoft.AspNetCore.Http;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.Tool;
using System.Text;
using System.IO;
using ShipWeb.ProtoBuffer;
using ShipWeb.ProtoBuffer.Models;
using NuGet.Frameworks;
using System.Security;
using NetMQ.Sockets;
using ShipWeb.Helpers;
using System.Threading;
using NetMQ;
using System.Diagnostics;
using ShipWeb.Interface;
using System.Reflection;

namespace ShipWeb
{
    public class InitManger
    {

        /// <summary>
        /// 初使化
        /// </summary>
        public static void Init()
        {
            ManagerHelp.atWorks = new List<AtWork>();
            using (var context = new MyContext())
            {
                //船舶端组件注册
                var comList = context.Component.FirstOrDefault(c=>c.Type==ComponentType.WEB&&c.CommId==null);
                if (comList!=null)
                {
                    ManagerHelp.Cid = comList.Id;
                }
                //Component();
                BoatInit();
            }
        }
        /// <summary>
        /// 船舶端组件初使化
        /// 发送组件注册
        /// 发送组件查询
        /// 收到开启组件后发送船状态设置
        /// 发送设备信息
        /// 收到设备成功消息后发送算法配置
        /// 发送船员信息
        /// </summary>
        public static void BoatInit()
        {
            using (var context = new MyContext())
            {
                var ship = context.Ship.FirstOrDefault();
                if (ship == null)
                {
                    Models.Ship model = new Ship()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Flag = false,
                        Name = "船1",
                        type = Ship.Type.PORT
                    };
                    context.Ship.Add(model);
                    context.SaveChanges();
                }
            }
            SendDataMsg assembly = new SendDataMsg();
            //发送组件注册请求
            assembly.SendComponentSign("WEB", ManagerHelp.Cid);
            //发送查询请求
            assembly.SendComponentQuery();
            ManagerHelp.isInit = true;
        }
        /// <summary>
        /// 陆地端注册
        /// </summary>
        public static void LandInit() 
        {
            SendDataMsg assembly = new SendDataMsg();
            assembly.SendComponentSign("WEB", ManagerHelp.Cid);
            assembly.SendComponentQuery();
            ManagerHelp.isInit = false;
            ManagerHelp.isLand = true;
        }
        /// <summary>
        /// 初使化船状态
        /// </summary>
        public static void InitStatus()
        {
            using (var con = new MyContext())
            {
                var components = con.Component.Where(c => c.Type == ComponentType.AI).ToList();
                SendDataMsg assembly = new SendDataMsg();
                var ship = con.Ship.FirstOrDefault();
                StatusRequest request = new StatusRequest()
                {
                    flag = (int)ship.type,
                    type = StatusRequest.Type.SAIL
                };
                foreach (var item in components)
                {
                    assembly.SendStatusSet(request, item.CommId);
                }

            }
        }
        /// <summary>
        /// 初使化设备
        /// </summary>
        public static void InitDevice() 
        {
            List<Models.Component> list = new List<Models.Component>();
            using (var con=new MyContext())
            {
                list = con.Component.Where(c => c.Type != ComponentType.WEB).ToList();
            }
            if (list.Count>0)
            {
                //大华通讯ID
                string DHDIdenity = "";
                //海康通讯Id
                string HKDIdentity = "";
                if (list.Where(c => c.Type == ComponentType.DHD).Any())
                {
                    DHDIdenity = list.FirstOrDefault(c => c.Type == ComponentType.DHD).CommId;
                }
                if (list.Where(c => c.Type == ComponentType.HKD).Any())
                {
                    HKDIdentity = list.FirstOrDefault(c => c.Type == ComponentType.HKD).CommId;
                }
                if (HKDIdentity==""&& DHDIdenity=="")
                {
                    return;
                }
                SendDataMsg assembly = new SendDataMsg();
                var deviceInfos = ProtoBDManager.DeviceQuery(null);
                foreach (var item in deviceInfos)
                {
                    string devIdentity = "";
                    if (item.factory == DeviceInfo.Factory.DAHUA) devIdentity = DHDIdenity;
                    else if (item.factory == DeviceInfo.Factory.HIKVISION) devIdentity = HKDIdentity;
                    //海康和大华组件尚未启动则不需要发送组件注册消息
                    if (devIdentity == "") continue;
                    assembly.SendDeveiceAdd(item, devIdentity);
                }
            }
            else
            {
                ManagerHelp.isInit = false;
            }
           
        }

        /// <summary>
        /// 算法请求
        /// </summary>
        public static void InitAlgorithm() 
        {
            using (var con = new MyContext())
            {
                var components = con.Component.Where(c => c.Type == ComponentType.AI).ToList();
                if (components.Count>0)
                {
                    SendDataMsg assembly = new SendDataMsg();
                    var algorithmInfos = ProtoBDManager.AlgorithmQuery();
                    foreach (var item in algorithmInfos)
                    {
                        //配置的是缺岗类型则传入设备的通讯ID
                        if (item.type==AlgorithmInfo.Type.CAPTURE)
                        {
                            var camera = con.Camera.FirstOrDefault(c => c.Id == item.cid);
                            if (camera == null) continue;
                            var device = con.Device.FirstOrDefault(c => c.Id == camera.DeviceId);
                            if (device == null) continue;
                            var comtype = ComponentType.HKD;
                            if (device.factory == Models.Device.Factory.DAHUA) comtype = ComponentType.DHD;
                            var compontent = con.Component.FirstOrDefault(c=>c.Type== comtype);
                            if (compontent == null) continue;
                            assembly.SendAlgorithmSet(item, compontent.CommId);
                        }
                        else
                        {
                            string name = Enum.GetName(typeof(AlgorithmType), Convert.ToInt32(item.type));
                            if (name == "ATTENDANCE_IN" || name == "ATTENDANCE_OUT")
                            {
                                name = ManagerHelp.FaceName.ToUpper();
                            }
                            if (components.Where(c => c.Name.ToUpper() == name).Any())
                            {
                                string identity = components.FirstOrDefault(c => c.Name.ToUpper() == name).CommId;
                                assembly.SendAlgorithmSet(item, identity);
                            }

                        }
                        
                    }
                }
            }
        }
        /// <summary>
        /// 船员请求
        /// </summary>
        /// <param name="nextIdentity"></param>
        public static void InitCrew() 
        {
            using (var con = new MyContext())
            {
                var component = con.Component.FirstOrDefault(c => c.Type != ComponentType.AI && c.Name == ManagerHelp.FaceName);
                if (component != null)
                {
                    SendDataMsg assembly = new SendDataMsg();
                    var crewInfos = ProtoBDManager.CrewQuery();
                    foreach (var item in crewInfos)
                    {
                        assembly.SendCrewAdd(item, component.CommId);
                    }
                }
            }
        }
        /// <summary>
        /// 心跳查询
        /// </summary>
        public static void HeartBeat()
        {
            if (!string.IsNullOrEmpty(ManagerHelp.Cid))
            {
                SendDataMsg assembly = new SendDataMsg();
                if (!ManagerHelp.isLand)
                {
                    //船舶端发送注册请求
                    assembly.SendComponentSign("WEB", ManagerHelp.Cid);
                }
                else
                {
                    //陆地端定时更新组件信息
                    assembly.SendComponentQuery();
                }
            }
        }
        /// <summary>
        /// 组件退出
        /// </summary>
        public static void Exit()
        {
            SendDataMsg assembly = new SendDataMsg();
            assembly.SendComponentExit(ManagerHelp.Cid);
        }
       
        /// <summary>
        /// 获取报警信息
        /// </summary>
        public static void Alarm()
        {
            #region 获取报警信息并入库
            try
            {
                SendDataMsg assembly = new SendDataMsg();
                using (var context = new MyContext())
                {
                    var ship = context.Ship.ToList();
                    foreach (var itship in ship)
                    {
                        string shipId = itship.Id;
                        string identity = Guid.NewGuid().ToString();
                        var component = context.Component.FirstOrDefault(c => c.ShipId == itship.Id && c.Type == ComponentType.WEB);
                        ProtoBuffer.Models.Alarm protoalarm = new ProtoBuffer.Models.Alarm();
                        assembly.SendAlarm(component.Id);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            #endregion
        }

        /// <summary>
        /// 发送缺岗通知
        /// </summary>
        public static void LoadNotice()
        {
            SendDataMsg assembly = new SendDataMsg();
            Task.Factory.StartNew(state => {
                //获取间隔时间
                int departureTime =Convert.ToInt32(AppSettingHelper.GetSectionValue("DepartureTime"));
                while (true)
                {
                    using (var context = new MyContext())
                    {
                        //获取船的航行状态
                        var ship = context.Ship.FirstOrDefault();
                        if (ship == null) continue;                       
                        DateTime dt = DateTime.Now;
                        //ManagerHelp.atWorks 考勤人数的集合
                        if (ship.Flag && ManagerHelp.atWorks.Count <= 0)
                        {
                            var algo = context.Algorithm.Where(c => c.Type == AlgorithmType.CAPTURE);
                            if (algo.Count() > 0)
                            {
                                var camares = context.Camera.Where(c => algo.Select(a => a.Cid).Contains(c.Id));
                                foreach (var item in camares)
                                {
                                    var device = context.Device.FirstOrDefault(c => c.Id == item.DeviceId);
                                    if (device == null) continue;
                                    var component = context.Component.FirstOrDefault(c => c.Type == (device.factory == Models.Device.Factory.DAHUA ? ComponentType.DHD : ComponentType.HKD));
                                    if (component == null) continue;
                                    CaptureInfo captureInfo = new CaptureInfo()
                                    {
                                        cid = item.Id,
                                        did = item.DeviceId,
                                        idx = item.Index
                                    };
                                    assembly.SendCapture(captureInfo, component.CommId);
                                }
                            }
                        }
                    }
                    Thread.Sleep(departureTime * 1000 * 60);//单位毫秒
                }
            }, TaskCreationOptions.LongRunning);
        }
        public static void TestAttendance()
        {
          
            var pathDir = AppContext.BaseDirectory + "/testImg/";
            var images = Directory.GetFiles(pathDir);
            using (var context = new MyContext())
            {
                var comList = context.Component.FirstOrDefault(c => c.Type == ComponentType.WEB && c.CommId == null);
                if (comList != null)
                {
                    ManagerHelp.Cid = comList.Id;
                }
                int index = 0;
                foreach (var item in images)
                {
                    FileStream fs = new FileStream(item, FileMode.Open, FileAccess.Read);
                    byte[] byt = new byte[fs.Length];
                    fs.Read(byt, 0, Convert.ToInt32(fs.Length));
                    fs.Close();
                    AlarmInfo info = new AlarmInfo()
                    {
                        cid = "0b7c18ed-a17b-4cfd-9a29-f52f6baa725a",
                        uid = "321c896f-3533-428d-bd9e-f59fd2ef20bd",
                        picture = Convert.ToBase64String(byt),
                        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        type = AlarmInfo.Type.ATTENDANCE_IN
                    };
                    ProtoBDManager.AlarmAdd(info);
                    ///string pics = Convert.ToBase64String(byt);
                    //string ids = string.Join(',', context.Attendance.Select(d => d.CrewId));
                    //var crew = context.Crew.Where(c=>!ids.Contains(c.Id)).ToList();
                    //string shipId = "2eb85d83-1144-428d-a640-54b7a343851a";
                    //string crewId = crew[index].Id;
                    //for (int i = 0; i < 2; i++)
                    //{
                    //    string identity = Guid.NewGuid().ToString();
                    //    Attendance attendance = new Attendance()
                    //    {
                    //        Behavior = i,
                    //        Id = identity,
                    //        CameraId =i==0? "bc03715d-eb40-48f6-8fd3-174534353fa8": "f0b180bd-68d0-4811-bc97-dec5fe57b501",
                    //        ShipId = shipId,
                    //        Time = DateTime.Now,
                    //        CrewId = crewId,
                    //        attendancePictures = new List<AttendancePicture>()
                    //        {
                    //            new AttendancePicture ()
                    //            {
                    //                 AttendanceId=identity,
                    //                 Id=Guid.NewGuid().ToString(),
                    //                 Picture= byt,//Convert.FromBase64String(pics),
                    //                 ShipId=shipId
                    //            }
                    //        }
                    //    };
                    //    context.Attendance.Add(attendance);
                    //}
                    //index++;
                }
                context.SaveChanges();
            }
        }
    }
}
