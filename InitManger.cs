
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
            using (var context=new MyContext())
            {
                var comList = context.Component.ToList();
                if (comList.Count == 0)
                {
                    Component();
                }
                else
                {
                    ManagerHelp.Cid = comList[0].Id;
                }
            }
        }
        /// <summary>
        /// 组件注册
        /// </summary>
        private static void Component()
        {
            Task.Factory.StartNew(state =>
            {
                using (var context = new MyContext())
                {
                    //发送组件注册
                    string iditity = Guid.NewGuid().ToString();
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
                    var protoId = iditity + "," + shipId;
                    ComponentResponse rep = manager.ComponentStart(iditity, ComponentInfo.Type.WEB, name);
                    if (rep != null && rep.result == 0)
                    {
                        Models.Component model = new Models.Component()
                        {
                            Id = iditity,
                            Name = name,
                            Type = Models.Component.ComponentType.WEB,
                            ShipId = shipId
                        };
                        ManagerHelp.Cid = rep.cid;

                        context.Component.Add(model);
                        context.SaveChanges();
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
        /// <summary>
        /// 心跳查询
        /// </summary>
        public static void HeartBeat()
        {
            Task.Factory.StartNew(state =>
            {
                if (!string.IsNullOrEmpty(ManagerHelp.Cid))
                {
                    ComponentResponse rep = manager.ComponentStart(ManagerHelp.Cid, ComponentInfo.Type.WEB, "", ManagerHelp.Cid);
                }
            }, TaskCreationOptions.LongRunning);
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
                        string shipId = "";
                        var ship = context.Ship.FirstOrDefault();
                        if (ship != null)
                        {
                            shipId = ship.Id;
                            string identity = Guid.NewGuid().ToString();
                            ProtoBuffer.Models.Alarm alarm = manager.AlarmStart(identity);
                            if (alarm != null)
                            {
                                if (alarm.alarminfo.type == ProtoBuffer.Models.AlarmInfo.Type.ATTENDANCE_IN || alarm.alarminfo.type == ProtoBuffer.Models.AlarmInfo.Type.ATTENDANCE_OUT)
                                {
                                    #region 考勤信息入库
                                    ShipWeb.Models.Attendance attendance = new Attendance()
                                    {
                                        Behavior = alarm.alarminfo.type == ProtoBuffer.Models.AlarmInfo.Type.ATTENDANCE_IN ? 0 : 1,
                                        Id = identity,
                                        CameraId = alarm.cid,
                                        ShipId = shipId,
                                        Time = Convert.ToDateTime(alarm.time),
                                        CrewId = alarm.alarminfo.uid,
                                        attendancePictures = new List<AttendancePicture>() {
                                         new AttendancePicture (){
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
                                        alarmInfo = new Models.AlarmInfo()
                                        {
                                            AlarmId = identity,
                                            Id = Guid.NewGuid().ToString(),
                                            Shipid = ManagerHelp.ShipId,
                                            Type = (AlarmType)alarm.alarminfo.type,
                                            Uid = alarm.alarminfo.uid,
                                            alarmPositions = new List<Models.AlarmPosition>()
                                        }
                                    };
                                    var replist = alarm.alarminfo.position;
                                    if (replist.Count > 0)
                                    {
                                        foreach (var item in replist)
                                        {

                                            Models.AlarmPosition position = new Models.AlarmPosition()
                                            {
                                                AlarmInfoId = model.alarmInfo.Id,
                                                ShipId = ManagerHelp.ShipId,
                                                Id = Guid.NewGuid().ToString(),
                                                H = item.h,
                                                W = item.w,
                                                X = item.x,
                                                Y = item.y
                                            };
                                            model.alarmInfo.alarmPositions.Add(position);
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
