
using Microsoft.AspNetCore.Http;
using ProtoBuf;
using ProtoBuf.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShipWeb.DB;
using ShipWeb.Models;
using ShipWeb.Tool;
using System.Text;
using System.IO;

namespace ShipWeb
{
    public class InitManger
    {

        static  ProtoManager manager = new ProtoManager();
        static MyContext _context = new MyContext();
        private static string shipId = "";
        /// <summary>
        /// 初使化
        /// </summary>
        public static void Init()
        {

            var _context = new MyContext();
            //是否有般存在
            var ship = _context.Ship.FirstOrDefault();
            if (ship == null)
            {
                shipId = Guid.NewGuid().ToString();
                Models.Ship sm = new Models.Ship()
                {
                    Flag = false,
                    Id = shipId,
                    Name = "船1",
                    Type = 1
                };
                _context.Ship.Add(sm);
                _context.SaveChanges();
            }
            else
            {
                shipId = ship.Id;
            }
            //HttpContext.Session.SetString("shipId", shipId); 
            ManagerHelp.ShipId = shipId;
            Task.Factory.StartNew(state => {
                //发送组件注册
                string iditity = Guid.NewGuid().ToString();
                string name = "组件1";
                ComponentResponse rep = manager.ComponentStart(iditity, 2, name);
                if (rep != null && rep.result == 0)
                {
                    Models.Component model = new Models.Component()
                    {
                        Cid = rep.cid,
                        Id = iditity,
                        Name = name,
                        Type = 2
                    };
                    ManagerHelp.Cid = rep.cid;
                    _context.Components.Add(model);
                }
            }, TaskCreationOptions.LongRunning);
        }
        /// <summary>
        /// 获取报警信息
        /// </summary>
        public static void Alarm()
        {
            string identity = Guid.NewGuid().ToString();

            #region 测试数据
            string path = AppContext.BaseDirectory + "/images/1591355250.png";
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] byt = new byte[fs.Length];
            fs.Read(byt, 0,Convert.ToInt32(fs.Length));
            fs.Close();
            string pics=Convert.ToBase64String(byt);
            ShipWeb.Models.Alarm model = new ShipWeb.Models.Alarm()
            {
                Id = identity,
                Picture = Encoding.UTF8.GetBytes(pics),//alarm.picture
                Time = DateTime.Now, 
                ShipId = shipId,
                alarmInformation = new AlarmInformation()
                {
                    AlarmId = identity,
                    Cid = "2222", //alarm.cid,
                    Id = Guid.NewGuid().ToString(),
                    Shipid = shipId,
                    Type = 1,
                    Uid = "",
                    alarmInformationPositions = new List<AlarmInformationPosition>()
                }
            };
            AlarmInformationPosition position = new AlarmInformationPosition()
            {
                AlarmInformationId = model.alarmInformation.Id,
                ShipId = shipId,
                Id = Guid.NewGuid().ToString(),
                H = 200,
                W = 150,
                X = 30, 
                Y = 20 
            };
            model.alarmInformation.alarmInformationPositions.Add(position);
            
            //操作入库
            _context.Alarm.Add(model);
            _context.SaveChanges();
            #endregion

            #region 获取报警信息并入库
            //ProtoBuf.Models.Alarm alarm = manager.AlarmStart(identity);
            //if (alarm != null)
            //{
            //    ShipWeb.Models.Alarm model = new ShipWeb.Models.Alarm()
            //    {
            //        Id = identity,
            //        Picture = Encoding.UTF8.GetBytes(alarm.picture),
            //        Time = Convert.ToDateTime(alarm.time),
            //        ShipId = shipId,
            //        alarmInformation = new AlarmInformation()
            //        {
            //            AlarmId = identity,
            //            Cid = alarm.cid,
            //            Id = Guid.NewGuid().ToString(),
            //            Shipid = shipId,
            //            Type = (int)alarm.information.type,
            //            Uid = alarm.information.uid,
            //            alarmInformationPositions = new List<AlarmInformationPosition>()
            //        }
            //    };
            //    List<Position> replist = alarm.information.position;
            //    if (replist.Count > 0)
            //    {
            //        foreach (var item in replist)
            //        {
            //            AlarmInformationPosition position = new AlarmInformationPosition()
            //            {
            //                AlarmInformationId = model.alarmInformation.Id,
            //                ShipId = shipId,
            //                Id = Guid.NewGuid().ToString(),
            //                H = item.h,
            //                W = item.w,
            //                X = item.x,
            //                Y = item.y
            //            };
            //            model.alarmInformation.alarmInformationPositions.Add(position);
            //        }
            //    }
            //    //操作入库
            //    _context.Alarm.Add(model);
            //    _context.SaveChanges();
            //}
            #endregion
        }
    }
}
