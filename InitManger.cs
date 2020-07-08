
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

        static  ProtoManager manager = new ProtoManager();
        static MyContext _context = new MyContext();
        /// <summary>
        /// 初使化
        /// </summary>
        public static void Init()
        {
           var comList= _context.Components.ToList();
            if (comList.Count==0)
            {
                Component();
            }
            else
            {
                ManagerHelp.Cid = comList[0].Cid;
            }
            
        }
        /// <summary>
        /// 组件注册
        /// </summary>
        private static void Component() {
            Task.Factory.StartNew(state =>
            {
                //发送组件注册
                string iditity = Guid.NewGuid().ToString();
                ManagerHelp.ShipId = Guid.NewGuid().ToString();
                string name = "组件1";
                ComponentResponse rep = manager.ComponentStart(iditity, 2, name);
                if (rep != null && rep.result == 0)
                {
                    Models.Component model = new Models.Component()
                    {
                        Cid =rep.cid,
                        Id = iditity,
                        Name = name,
                        Type = Models.Component.ComponentType.WEB,
                        ShipId = ManagerHelp.ShipId
                    };
                    ManagerHelp.Cid = rep.cid;
                    ManagerHelp.ComponentType = model.Type;

                    _context.Components.Add(model);
                    _context.SaveChanges();
                }
            }, TaskCreationOptions.LongRunning);
        }
        /// <summary>
        /// 心跳查询
        /// </summary>
        public static void HeartBeat()
        {
            Task.Factory.StartNew(state => {
                if (!string.IsNullOrEmpty(ManagerHelp.Cid))
                {
                    string iditity = Guid.NewGuid().ToString();
                    ComponentResponse rep = manager.ComponentStart(iditity, (int)ManagerHelp.ComponentType, "", ManagerHelp.Cid);
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
                    string shipId = "";
                    var ship = _context.Ship.FirstOrDefault();
                    if (ship != null)
                    {
                        shipId = ship.Id;
                        string identity = Guid.NewGuid().ToString();
                        ProtoBuffer.Models.Alarm alarm = manager.AlarmStart(identity);
                        if (alarm != null)
                        {
                            ShipWeb.Models.Alarm model = new ShipWeb.Models.Alarm()
                            {
                                Id = identity,
                                Picture = Encoding.UTF8.GetBytes(alarm.picture),
                                Time = Convert.ToDateTime(alarm.time),
                                ShipId = shipId,
                                alarmInformation = new AlarmInformation()
                                {
                                    AlarmId = identity,
                                    Cid = alarm.cid,
                                    Id = Guid.NewGuid().ToString(),
                                    Shipid = ManagerHelp.ShipId,
                                    Type = (int)alarm.information.type,
                                    Uid = alarm.information.uid,
                                    alarmInformationPositions = new List<AlarmInformationPosition>()
                                }
                            };
                            List<Position> replist = alarm.information.position;
                            if (replist.Count > 0)
                            {
                                foreach (var item in replist)
                                {
                                    AlarmInformationPosition position = new AlarmInformationPosition()
                                    {
                                        AlarmInformationId = model.alarmInformation.Id,
                                        ShipId = ManagerHelp.ShipId,
                                        Id = Guid.NewGuid().ToString(),
                                        H = item.h,
                                        W = item.w,
                                        X = item.x,
                                        Y = item.y
                                    };
                                    model.alarmInformation.alarmInformationPositions.Add(position);
                                }
                            }
                            //操作入库
                            _context.Alarm.Add(model);
                            _context.SaveChanges();
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
