
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

namespace ShipWeb
{
    public class InitManger
    {

        static ProtoManager manager = new ProtoManager();
        /// <summary>
        /// 初使化
        /// </summary>
        public static void Init()
        {
            using (var context = new MyContext())
            {
                var comList = context.Component.ToList();
                if (comList.Count>0)
                {
                    ManagerHelp.Cid = comList[0].Id;
                }
                Component();
            }
        }
        /// <summary>
        /// 组件注册
        /// </summary>
        private static void Component()
        {
            Task.Factory.StartNew(state => {
                using (var context = new MyContext())
                {
                    //发送组件注册
                    var ship = context.Ship.FirstOrDefault();
                    string shipId = Guid.NewGuid().ToString();
                    if (ship == null)
                    {
                        Models.Ship model = new Ship()
                        {
                            Id = shipId,
                            Flag = false,
                            Name = "船1",
                            type = Ship.Type.PORT
                        };
                        context.Ship.Add(model);
                        context.SaveChanges();
                    }
                    else
                    {
                        shipId = ship.Id;
                    }
                    string name = "组件1";
                    manager.ComponentStart(shipId, ComponentInfo.Type.WEB, name, ManagerHelp.Cid);
                    if (string.IsNullOrEmpty(ManagerHelp.Cid))
                    {
                        //超时时间 等10秒
                        int timeout = 10000;
                        new TaskFactory().StartNew(() => {
                            var msg = manager.ReceiveMessage(ProtoManager.dealer);
                            ComponentResponse rep = msg.component.componentresponse;
                            if (rep != null && rep.result == 0)
                            {
                                var comp = context.Component.FirstOrDefault(c => c.ShipId == shipId);
                                if (comp == null)
                                {
                                    Models.Component model = new Models.Component()
                                    {
                                        Id = rep.cid,
                                        Name = name,
                                        Type = Models.Component.ComponentType.WEB,
                                        ShipId = shipId
                                    };
                                    ManagerHelp.Cid = rep.cid;
                                    context.Component.Add(model);
                                    context.SaveChanges();
                                }
                            }
                            else
                            {
                                //注册失败后继续发送注册请求
                                Component(); 
                            }
                        }).Wait(timeout);
                        //超时后继续注册
                        Component();
                    }
                    using (var cont = new MyContext())
                    {
                        //注册成功推送状态
                        var shipdb = cont.Ship.FirstOrDefault();
                        StatusRequest request = new StatusRequest()
                        {
                            type = StatusRequest.Type.SAIL,
                            flag = (int)shipdb.type
                        };
                        manager.StatussSet(request, shipdb.Id);
                        //发送设备信息
                        int result=GetDevice();
                        if (result==0)
                        {
                            //发送算法信息
                            GetAlgorithm();
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
        /// <summary>
        /// 设备请求
        /// </summary>
        private static int GetDevice() 
        {
            int result = 0;
            using (var context=new MyContext())
            {
                var dev = context.Device.ToList();
                var ids = string.Join(',', dev.Select(c => c.Id));
                var cam = context.Camera.Where(c => ids.Contains(c.DeviceId)).ToList();
                foreach (var item in dev)
                {
                    ProtoBuffer.Models.DeviceInfo emb = new ProtoBuffer.Models.DeviceInfo()
                    {
                        ip = item.IP,
                        name = item.Name,
                        password = item.Password,
                        port = item.Port,
                        nickname = item.Nickname,
                        factory = (ProtoBuffer.Models.DeviceInfo.Factory)item.factory,
                        type = (ProtoBuffer.Models.DeviceInfo.Type)item.type,
                        enable = item.Enable,
                        did = item.Id
                    };
                    var cts = cam.Where(c => c.DeviceId == item.Id);
                    emb.camerainfos = new List<CameraInfo>();
                    foreach (var camera in cts)
                    {
                        emb.camerainfos.Add(new CameraInfo()
                        {
                            cid = camera.Id,
                            index = camera.Index,
                            enable = camera.Enalbe,
                            ip = camera.IP,
                            nickname = camera.NickName
                        });
                    }
                    var res=manager.DeveiceAdd(emb, item.Id);
                    if (res.result!=0)
                    {
                        result = res.result;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 算法请求
        /// </summary>
        private static void GetAlgorithm() 
        {
            using (var context=new MyContext())
            {
                var algo = context.Algorithm.ToList();
                foreach (var model in algo)
                {
                    ProtoBuffer.Models.AlgorithmInfo info = new ProtoBuffer.Models.AlgorithmInfo()
                    {
                        cid = model.Cid,
                        gpu = model.GPU,
                        similar = (float)model.Similar,
                        dectectfirst = (float)model.DectectFirst,
                        dectectsecond = (float)model.DectectSecond,
                        track = (float)model.Track,
                        type = (ProtoBuffer.Models.AlgorithmInfo.Type)model.Type
                    };
                    manager.AlgorithmSet(model.Id, info);
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
                string identity = Guid.NewGuid().ToString();
                manager.Heart(identity, ComponentInfo.Type.WEB, "", ManagerHelp.Cid);
            }
        }
        /// <summary>
        /// 组件退出
        /// </summary>
        public static void Exit()
        {
            manager.ComponentExit(ManagerHelp.Cid, Guid.NewGuid().ToString());
        }
        /// <summary>
        /// 获取报警信息
        /// </summary>
        public static void Alarm()
        {
            #region 获取报警信息并入库
            try
            {
                Task.Factory.StartNew(state =>
                {
                    using (var context = new MyContext())
                    {
                        var ship = context.Ship.ToList();
                        foreach (var itship in ship)
                        {
                            string shipId = itship.Id;
                            string identity = Guid.NewGuid().ToString();
                            ProtoBuffer.Models.Alarm protoalarm = manager.AlarmStart(shipId);
                            if (protoalarm != null)
                            {
                                var alarm = protoalarm.alarminfo;
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

                                            Models.AlarmPosition position = new Models.AlarmPosition()
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

                }, TaskCreationOptions.LongRunning);
            }
            catch (Exception ex)
            {

            }
            #endregion
        }
    }
}
