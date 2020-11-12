using Pomelo.AspNetCore.TimedJob;
using SmartWeb.DB;
using SmartWeb.Models;
using SmartWeb.ProtoBuffer.Models;
using SmartWeb.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer
{
    public class SyncAlarmJob:Job
    {

        private SendDataMsg assembly = new SendDataMsg();
        /// <summary>
        /// 向陆地端推送报警数据
        /// </summary>         
        [Invoke(Interval = 1000 * 60 * 1, SkipWhileExecuting = true)]
        public void SyncAlarmData()
        {
            //陆地端不执行同步数据
            if (!ManagerHelp.IsShipPort) return; 
            try
            {
                SyncAlarm();
                SyncAttendance();
            }
            catch (Exception ex)
            {
            }
        }

        private void SyncAlarm()
        {
            List<SmartWeb.Models.Alarm> alarms = new List<SmartWeb.Models.Alarm>();
            List<SmartWeb.Models.AlarmPosition> alarmPos = new List<SmartWeb.Models.AlarmPosition>();
            using (var context = new MyContext())
            {
                alarms = context.Alarm.Where(c => c.IsSyncSucces == false).OrderBy(s => s.Time).ToList();
                alarmPos = context.AlarmPosition.Where(c => alarms.Select(a => a.Id).Contains(c.AlarmId)).ToList();
            }
            foreach (var item in alarms)
            {
                try
                {
                    //组合发送数据
                    AlarmInfo alarmInfo = GetAlarmInfo(alarmPos, item);
                    ShipPushAlarm(item, alarmInfo);
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        /// <summary>
        /// 向陆地端推送数据
        /// </summary>
        /// <param name="item"></param>
        /// <param name="alarmInfo"></param>
        private void ShipPushAlarm(SmartWeb.Models.Alarm item, AlarmInfo alarmInfo)
        {
            List<SmartWeb.Models.Alarm> recordAlarm = new List<SmartWeb.Models.Alarm>();
            assembly.SendAlarm("upload", alarmInfo);
            recordAlarm.Add(item);
            bool flag = true;
            new TaskFactory().StartNew(() => {
                while (recordAlarm.Count != 0)
                {
                    if (ManagerHelp.LandResponse.Count > 0)
                    {
                        flag = false;
                        //修改同步状态
                        UpdateAlarmSyncStatus(recordAlarm);
                        Thread.Sleep(1000);
                    }
                }
            }).Wait(3000);
            if (flag)
            {
                ShipPushAlarm(item, alarmInfo);
            }
        }

        /// <summary>
        /// 向陆地端推送考勤数据
        /// </summary>
        private void SyncAttendance()
        {
            if (!ManagerHelp.IsShipPort) return;
            List<Attendance> attendances = new List<Attendance>();
            List<AttendancePicture> attendPic = new List<AttendancePicture>();
            using (var context = new MyContext())
            {
                attendances = context.Attendance.Where(c => c.IsSyncSucces == false).OrderBy(c => c.Time).ToList();
                attendPic = context.AttendancePicture.Where(c => attendances.Select(a => a.Id).Contains(c.AttendanceId)).ToList();
            }
            foreach (var item in attendances)
            {
                try
                {
                    //获取考勤数据
                    AlarmInfo alarmInfo = GetAttendanceInfo(attendPic, item);
                    ShipPushAttend(item, alarmInfo);
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
        }
        /// <summary>
        /// 向陆地端推送考勤数据
        /// </summary>
        /// <param name="item"></param>
        /// <param name="alarmInfo"></param>
        private void ShipPushAttend(Attendance item, AlarmInfo alarmInfo)
        {
            List<Attendance> recordAtt = new List<Attendance>();
            assembly.SendAlarm("upload", alarmInfo);
            recordAtt.Add(item);
            bool flag = true;
            new TaskFactory().StartNew(() =>
            {
                while (recordAtt.Count != 0)
                {
                    if (ManagerHelp.LandResponse.Count > 0)
                    {
                        flag = false;
                        //修改同步状态
                        UpdateAttendSyncStatus(recordAtt);
                        Thread.Sleep(1000);
                    }
                }
            }).Wait(10000);
            if (flag)
            {
                ShipPushAttend(item, alarmInfo);
            }
        }
        /// <summary>
        /// 修改同步状态
        /// </summary>
        /// <param name="record"></param>
        private static void UpdateAlarmSyncStatus(List<SmartWeb.Models.Alarm> record)
        {
            var response = ManagerHelp.LandResponse;
            for (int i = 0; i < response.Count; i++)
            {
                try
                {
                    var alarm = response[i];
                    if (record.Where(c => c.Id == alarm.uid).Any())
                    {
                        using (var context = new MyContext())
                        {
                            var remalarm = record.FirstOrDefault(c => c.Id == alarm.uid);
                            if (remalarm == null) continue;
                            //修改状态为已同步
                            remalarm.IsSyncSucces = true;
                            context.Alarm.Update(remalarm);
                            context.SaveChanges();
                            ManagerHelp.LandResponse.Remove(alarm);
                            record.Remove(remalarm);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        /// <summary>
        /// 组合protobuf的报警信息
        /// </summary>
        /// <param name="alarmPos"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static AlarmInfo GetAlarmInfo(List<SmartWeb.Models.AlarmPosition> alarmPos, SmartWeb.Models.Alarm item)
        {
            Models.AlarmInfo alarmInfo = new Models.AlarmInfo()
            {
                cid = item.Cid + ":" + item.Cname + ":" + ManagerHelp.ComponentId,
                picture = item.Picture,
                time = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                type = (Models.AlarmInfo.Type)item.Type,
                uid = item.Id,
                alarmposition = new List<Models.AlarmPosition>()
            };
            if (alarmPos == null) return alarmInfo;
            var positions = alarmPos.Where(c => c.AlarmId == item.Id).ToList();
            foreach (var pos in positions)
            {
                alarmInfo.alarmposition.Add(new Models.AlarmPosition()
                {
                    h = pos.H,
                    w = pos.W,
                    x = pos.X,
                    y = pos.Y
                });
            }

            return alarmInfo;
        }
        /// <summary>
        /// 修改考勤的同步状态
        /// </summary>
        /// <param name="record"></param>
        private static void UpdateAttendSyncStatus(List<Attendance> record)
        {
            var response = ManagerHelp.LandResponse;
            for (int i = 0; i < response.Count; i++)
            {
                var atten = response[i];
                if (record.Where(c => c.Id == atten.uid).Any())
                {
                    using (var context = new MyContext())
                    {
                        var remalarm = record.FirstOrDefault(c => c.Id == atten.uid);
                        if (remalarm == null) continue;
                        remalarm.IsSyncSucces = true;
                        context.Attendance.Update(remalarm);
                        context.SaveChanges();
                        ManagerHelp.LandResponse.Remove(atten);
                        record.Remove(remalarm);
                    }
                }
            }
        }
        /// <summary>
        /// 组合protobuf考勤信息
        /// </summary>
        /// <param name="attendPic"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static AlarmInfo GetAttendanceInfo(List<AttendancePicture> attendPic, Attendance item)
        {
            var picture = attendPic.FirstOrDefault(c => c.AttendanceId == item.Id);
            Models.AlarmInfo alarmInfo = new Models.AlarmInfo()
            {
                cid = item.CameraId + ":" + item.CameraName + ":" + ManagerHelp.ComponentId,
                time = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                type = item.Behavior == 0 ? Models.AlarmInfo.Type.ATTENDANCE_IN : Models.AlarmInfo.Type.ATTENDANCE_OUT,
                uid = item.CrewId + ":" + item.CrewName + ":" + item.CrewJob + ":" + item.Id,
                picture = picture.Picture,
                alarmposition = new List<Models.AlarmPosition>() {
                                     new Models.AlarmPosition(){
                                         x=1,
                                         h=2,
                                         y=3,
                                         w=4
                                     }
                                 }
            };
            return alarmInfo;
        }

        /// <summary>
        /// 数据转操作
        /// </summary>
        /// <param name="xmq">为空：船舶端接收报警 不为空是陆地端接收报警</param>
        /// <param name="alarmInfo">报警实体</param>
        /// <param name="shipId">船ID</param>
        /// <param name="cid">摄像机ID</param>
        /// <param name="cname">摄像机名称</param>
        private void GetData(ref Models.AlarmInfo alarmInfo, ref string cid, ref string cname, ref string webId)
        {
            var ids = alarmInfo.cid.Split(':');
            using (var context = new MyContext())
            {
                if (ManagerHelp.IsShipPort)
                {
                    if (ids.Length == 2)
                    {
                        var cam = context.Camera.FirstOrDefault(c => c.DeviceId == ids[0] && c.Index == Convert.ToInt32(ids[1]));
                        if (cam != null)
                        {
                            cid = cam.Id;
                            cname = cam.NickName;
                        };
                    }
                    string str = Encoding.ASCII.GetString(alarmInfo.picture);
                    byte[] picture = ManagerHelp.ConvertBase64(str);
                    alarmInfo.picture = picture;
                }
                else
                {
                    if (ids.Length == 3)
                    {
                        cid = ids[0];
                        cname = ids[1];
                        webId = ids[2];
                    }
                    alarmInfo.picture = alarmInfo.picture;
                }
            }
            //时间处理
            if (alarmInfo.time != "")
            {
                var times = alarmInfo.time.Split(",");
                if (times.Length > 1)
                {
                    DateTime dt = Convert.ToDateTime(times[0] + " " + times[1]);
                    var timezone = Convert.ToInt32(times[2]);
                    DateTime dtime = dt.AddHours(timezone);
                    alarmInfo.time = dtime.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            else
            {
                alarmInfo.time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 获取船I在
        /// </summary>
        /// <returns></returns>
        private string GetShipId(string xmq)
        {
            string shipId = ManagerHelp.ShipId;
            //陆地端获取相应的船ID
            if (!ManagerHelp.IsShipPort)
            {
                using (var context = new MyContext())
                {
                    var comp = context.Component.FirstOrDefault(c => c.Cid == xmq);
                    if (comp != null)
                    {
                        shipId = comp.ShipId;
                    }
                }
            }
            return shipId;
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
        private static bool AddAttendance(Models.AlarmInfo alarmInfo, string shipId, string cid, string cname)
        {
            bool flag = false;//是否向陆地端推送
            if (alarmInfo.uid != "")
            {
                int uid = 0;
                string identity = Guid.NewGuid().ToString();
                SmartWeb.Models.Crew crew = new SmartWeb.Models.Crew();
                using (var context = new MyContext())
                {
                    #region 考勤信息入库
                    if (ManagerHelp.IsShipPort)
                    {
                        if (alarmInfo.uid == "0")
                        {
                            if (alarmInfo.alarmposition != null && alarmInfo.alarmposition.Count > 0)
                            {
                                uid = alarmInfo.alarmposition[0].w;
                            }
                        }
                        else
                        {
                            //得到船员ID
                            try
                            {
                                uid = Convert.ToInt32(alarmInfo.uid);
                            }
                            catch (Exception)
                            {
                                byte[] array = new byte[1];
                                array = System.Text.Encoding.ASCII.GetBytes(alarmInfo.uid);
                                uid = array[0];
                            }
                        }
                        //查询传入的船员ID是否存在
                        crew = context.Crew.FirstOrDefault(c => c.Id == uid);
                        if (crew == null) return flag;
                        alarmInfo.uid = uid + ":" + crew.Name + ":" + crew.Job;
                        //重复打卡只取最后一次
                        int behavior = alarmInfo.type == ProtoBuffer.Models.AlarmInfo.Type.ATTENDANCE_IN ? 0 : 1;
                        var attes = context.Attendance.Where(c => c.CameraId == cid && c.CrewId == uid && c.Behavior == behavior).OrderByDescending(c => c.CreateTime).Take(1).ToList();
                        if (attes.Count() > 0)
                        {
                            DateTime dtNow = DateTime.Now;
                            var atte = attes[0];
                            //5分钟内重复打卡不记入数据库
                            if ((dtNow - atte.CreateTime).Minutes >= 5)
                            {
                                AttendanceAdd(alarmInfo, shipId, cid, cname);
                                flag = true;
                            }
                        }
                        else
                        {
                            AttendanceAdd(alarmInfo, shipId, cid, cname);
                            flag = true;
                        }
                    }
                    else if (alarmInfo.uid != "0")
                    {
                        AttendanceAdd(alarmInfo, shipId, cid, cname);
                        flag = true;
                    }
                    #endregion
                }
                if (ManagerHelp.IsShipPort)
                {
                    #region 将考勤数据存入内存中
                    if (alarmInfo.type == Models.AlarmInfo.Type.ATTENDANCE_IN && (!ManagerHelp.atWorks.Where(c => c.Uid == uid).Any()))
                    {
                        ManagerHelp.atWorks.Add(new AtWork()
                        {
                            Uid = uid,
                            Line = 1
                        });
                        if (ManagerHelp.atWorks.Count > 0)
                        {
                            ManagerHelp.LiveTime = "";
                        }
                    }
                    else if (alarmInfo.type == Models.AlarmInfo.Type.ATTENDANCE_OUT && ManagerHelp.atWorks.Where(c => c.Uid == uid).Any())
                    {
                        var atwork = ManagerHelp.atWorks.FirstOrDefault(c => c.Uid == uid);
                        ManagerHelp.atWorks.Remove(atwork);
                    }
                    if (ManagerHelp.atWorks.Count == 0) ManagerHelp.LiveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    #endregion

                    #region 发送考勤给IPad
                    if (crew != null && flag)
                    {
                        try
                        {
                            ProtoBDManager.AddReceiveLog("IpadStart", "记录日志开始", "连接地址:" + ManagerHelp.PublisherIP);
                            PublisherService service = new PublisherService();
                            //考勤类型
                            int Behavior = (alarmInfo.type == Models.AlarmInfo.Type.ATTENDANCE_IN) ? 0 : 1;
                            //考勤时间
                            string SignInTime = alarmInfo.time;
                            //考勤人员
                            string EmployeeName = string.IsNullOrEmpty(crew.Name) ? crew.Id.ToString() : crew.Name;
                            //考勤图片
                            string PhotosBuffer = Convert.ToBase64String(alarmInfo.picture);
                            string data = Behavior + "," + SignInTime + "," + EmployeeName + "," + PhotosBuffer;
                            service.Send(data);
                            ProtoBDManager.AddReceiveLog("IpadEnd", "记录日志结束");
                        }
                        catch (Exception ex)
                        {
                            ProtoBDManager.AddReceiveLog("Ipad", "记录日志异常", "错误：" + ex.Message);
                        }
                    }

                    #endregion
                }
            }
            return flag;
        }

        private static void AttendanceAdd(Models.AlarmInfo alarmInfo, string shipId, string cid, string cname)
        {
            if (alarmInfo.time == "")
            {
                alarmInfo.time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            string identity = Guid.NewGuid().ToString();
            using (var context = new MyContext())
            {
                var date = alarmInfo.uid.Split(":");
                int uid = Convert.ToInt32(date[0]);
                string crewName = date[1];
                string crewJob = date[2];
                Attendance attendance = new Attendance()
                {
                    Behavior = alarmInfo.type == ProtoBuffer.Models.AlarmInfo.Type.ATTENDANCE_IN ? 0 : 1,
                    Id = identity,
                    CameraId = cid,
                    CameraName = cname,
                    CrewName = crewName,
                    CrewJob = crewJob,
                    ShipId = shipId,
                    Time = Convert.ToDateTime(alarmInfo.time),
                    CrewId = uid,
                    attendancePictures = new List<AttendancePicture>()
                                    {
                                        new AttendancePicture ()
                                        {
                                             AttendanceId=identity,
                                             Id=Guid.NewGuid().ToString(),
                                             Picture=alarmInfo.picture,
                                             ShipId=shipId
                                        }
                                    }
                };
                context.Attendance.Add(attendance);
                context.SaveChanges();
                if (!ManagerHelp.IsShipPort)
                {
                    alarmInfo.uid = date[3];//陆地端响应时此值为船舶端船员表的主键Id
                }
            }
        }
        /// <summary>
        /// 报警信息入库
        /// </summary>
        /// <param name="alarmInfo"></param>
        /// <param name="shipId"></param>
        /// <param name="cid"></param>
        /// <param name="cname"></param>
        private void AddAlarm(Models.AlarmInfo alarmInfo, string shipId, string cid, string cname)
        {
            using (var context = new MyContext())
            {
                SmartWeb.Models.Alarm model = new SmartWeb.Models.Alarm()
                {
                    Id = Guid.NewGuid().ToString(),
                    Picture = alarmInfo.picture,
                    Time = Convert.ToDateTime(alarmInfo.time),
                    ShipId = shipId,
                    Cid = cid,
                    Cname = cname,
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
            }
        }

        /// <summary>
        /// 陆地响应船舶
        /// </summary>
        /// <param name="alarmInfo"></param>
        /// <param name="toId"></param>
        private void SendResponseShip(AlarmInfo alarmInfo, string toId)
        {
            alarmInfo.picture = Encoding.UTF8.GetBytes("1");
            alarmInfo.alarmposition = new List<Models.AlarmPosition>();
            alarmInfo.alarmposition.Add(new Models.AlarmPosition()
            {
                h = 1,
                x = 1,
                y = 1,
                w = 1
            });
            assembly.SendAlarm("request", alarmInfo, toId);
        }
        /// <summary>
        /// 判断数据是否存在
        /// </summary>
        /// <param name="shipId"></param>
        /// <param name="alarmInfo"></param>
        /// <returns></returns>
        private static bool CheckDBAlarm(string shipId, AlarmInfo alarmInfo)
        {
            bool flag = false;
            string shipAlarmId = alarmInfo.uid;
            if (alarmInfo.type == Models.AlarmInfo.Type.ATTENDANCE_IN || alarmInfo.type == Models.AlarmInfo.Type.ATTENDANCE_OUT)
            {
                shipAlarmId = alarmInfo.uid.Split(',').Last();
            }
            using (var context = new MyContext())
            {
                flag = context.Alarm.Where(c => c.ShipId == shipId && c.ShipAlarmId == shipAlarmId).Any();
            }
            return flag;
        }
    }
}
