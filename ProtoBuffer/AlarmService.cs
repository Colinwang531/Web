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
    public class AlarmService
    {
        private SendDataMsg assembly = new SendDataMsg();
        private bool isFirstAlarm=true;
        private bool isFirstAttend = true;
        /// <summary>
        /// 接收报警数据并入库
        /// </summary>
        public void ReviceAlarm()
        {
            Task.Factory.StartNew(state => {
                while (true)
                {
                    try
                    {
                        ReviceData();
                    }
                    catch (Exception ex)
                    {
                    }
                    Thread.Sleep(1000 * 2);
                }
            
            }, TaskCreationOptions.LongRunning);

           
        }

        private void ReviceData()
        {
            var alarmList = ManagerHelp.ReviceAlarms;
            for (int i = 0; i < alarmList.Count; i++)
            {
                var alarm = alarmList[i];
                string cid = "";
                string cname = "";//摄像机名称
                string shipId = alarm.Id;
                string webId = "";//陆地端返回船舶的ID
                if (!ManagerHelp.IsWriteDBSucces) continue;
                List<AlarmCache> list = alarm.alarmCaches;
                for (int j = 0; j < list.Count; j++)
                {
                    try
                    {
                        ManagerHelp.IsWriteDBSucces = false;
                        if (!string.IsNullOrEmpty(list[j].ShipAlarmId)) continue;
                        var alarmInfo = list[j].alarmInfos;                     
                        GetData(ref alarmInfo, ref cid, ref cname, ref webId);
                        if (cid == "" || shipId == "" || alarmInfo.picture == null) continue;
                        bool flag = false;
                        if (alarmInfo.type == Models.AlarmInfo.Type.ATTENDANCE_IN || alarmInfo.type == Models.AlarmInfo.Type.ATTENDANCE_OUT)
                        {
                            //陆地端首次收到消息时判断当条数据是否已经在数据库存在，如果不存在则执行添加，存   
                            if (isFirstAttend && ManagerHelp.IsShipPort == false)
                            {
                                isFirstAttend = false;
                                string shipAlarmId = "";
                                //判断数据是否存在
                                var result = CheckDBAlarm(shipId, alarmInfo, ref shipAlarmId);
                                //数据存在直接响应船舶端
                                if (result)
                                {
                                    list[j].ShipAlarmId = shipAlarmId;
                                    webId = alarmInfo.cid.Split(':')[2];
                                    SendResponseShip(alarmInfo, alarm.Id + ":" + webId);
                                    continue;
                                }
                            }
                            //考勤信息入库
                            flag = AddAttendance(alarmInfo, shipId, cid, cname);
                        }
                        else
                        {
                            string shipAlarmId = alarmInfo.uid;
                            //陆地端首次收到消息时判断当条数据是否已经在数据库存在，如果不存在则执行添加，存   
                            if (isFirstAlarm && ManagerHelp.IsShipPort == false)
                            {
                                isFirstAlarm = false;
                                //判断数据是否存在
                                var result = CheckDBAlarm(shipId, alarmInfo, ref shipAlarmId);
                                //数据存在直接响应船舶端
                                if (result)
                                {
                                    list[j].ShipAlarmId = shipAlarmId;
                                    webId = alarmInfo.cid.Split(':')[2];
                                    SendResponseShip(alarmInfo, alarm.Id + ":" + webId);
                                    continue;
                                }
                            }
                            // 报警信息入库                       
                            AddAlarm(alarmInfo, shipId, cid, cname, shipAlarmId);
                            flag = true;
                            //当船舶端报警类型为睡觉时播放报警提示，在船舶端主页面播放
                            if (alarmInfo.type == Models.AlarmInfo.Type.SLEEP && ManagerHelp.IsShipPort)
                            {
                                PlayerService player = new PlayerService();
                                player.Send("sleep");
                            }
                        }
                        if (flag)
                        {
                            //陆地端响应船舶请求
                            if (!ManagerHelp.IsShipPort)
                            {
                                list[j].ShipAlarmId = alarmInfo.uid;
                                SendResponseShip(alarmInfo, alarm.Id + ":" + webId);
                            }
                            else
                            {
                                //船舶端清空缓存
                                alarm.alarmCaches.Remove(list[j]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
                ManagerHelp.IsWriteDBSucces = true;
            }
        }

        /// <summary>
        /// 数据转操作
        /// </summary>
        /// <param name="xmq">为空：船舶端接收报警 不为空是陆地端接收报警</param>
        /// <param name="alarmInfo">报警实体</param>
        /// <param name="shipId">船ID</param>
        /// <param name="cid">摄像机ID</param>
        /// <param name="cname">摄像机名称</param>
        private void GetData(ref Models.AlarmInfo alarmInfo,ref string cid, ref string cname,ref string webId)
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
        ///// <summary>
        ///// 获取船I在
        ///// </summary>
        ///// <returns></returns>
        //private string GetShipId(string xmq) 
        //{
        //    string shipId = ManagerHelp.ShipId;
        //    //陆地端获取相应的船ID
        //    if (!ManagerHelp.IsShipPort)
        //    {
        //        using (var context = new MyContext())
        //        {
        //            var comp = context.Component.FirstOrDefault(c => c.Cid == xmq);
        //            if (comp != null)
        //            {
        //                shipId = comp.ShipId;
        //            }
        //        }
        //    }
        //    return shipId;
        //}

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
                string ShipAttendanceId = "";
                if (!ManagerHelp.IsShipPort) {
                    ShipAttendanceId = date[3];
                }
                Attendance attendance = new Attendance()
                {
                    Behavior = alarmInfo.type == ProtoBuffer.Models.AlarmInfo.Type.ATTENDANCE_IN ? 0 : 1,
                    Id = identity,
                    CameraId = cid,
                    CameraName = cname,
                    CrewName = crewName,
                    CrewJob = crewJob,
                    ShipId = ManagerHelp.IsShipPort ? "" : shipId,
                    Time = Convert.ToDateTime(alarmInfo.time),
                    ShipAttendanceId= ShipAttendanceId,
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
        private void AddAlarm(Models.AlarmInfo alarmInfo, string shipId, string cid, string cname,string shipAlarmId="")
        {
            using (var context = new MyContext())
            {
                SmartWeb.Models.Alarm model = new SmartWeb.Models.Alarm()
                {
                    Id = Guid.NewGuid().ToString(),
                    Picture = alarmInfo.picture,
                    Time = Convert.ToDateTime(alarmInfo.time),
                    ShipId =ManagerHelp.IsShipPort?"":shipId,
                    Cid = cid,
                    Cname = cname,
                    Type = (SmartWeb.Models.Alarm.AlarmType)alarmInfo.type,
                    ShipAlarmId= shipAlarmId
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
            if ((int)alarmInfo.type==7)
            {
                alarmInfo.type = AlarmInfo.Type.HELMET;
            }
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
        private static bool CheckDBAlarm(string shipId, AlarmInfo alarmInfo,ref string shipAlarmId)
        {
            bool flag = false;
            shipAlarmId = alarmInfo.uid;
            if (alarmInfo.type == Models.AlarmInfo.Type.ATTENDANCE_IN || alarmInfo.type == Models.AlarmInfo.Type.ATTENDANCE_OUT)
            {
                shipAlarmId = alarmInfo.uid.Split(':')[2];
                string id = shipAlarmId;
                using (var context = new MyContext())
                {
                    flag = context.Attendance.Where(c => c.ShipId == shipId && c.ShipAttendanceId == id).Any();
                }
            }
            else
            {
                string id = shipAlarmId;
                using (var context = new MyContext())
                {
                    flag = context.Alarm.Where(c => c.ShipId == shipId && c.ShipAlarmId == id).Any();
                }
            }            
            return flag;
        }
    }
}
