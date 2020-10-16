using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShipWeb.Models;
using ShipWeb.Helpers;
using System.Data.Common;

namespace ShipWeb.DB
{
    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions<MyContext> options) : base(options)
        {

        }
        public MyContext()
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>();
            modelBuilder.Entity<Ship>();
            modelBuilder.Entity<Component>();
            modelBuilder.Entity<Device>();
            modelBuilder.Entity<Camera>();
            modelBuilder.Entity<Crew>();
            modelBuilder.Entity<CrewPicture>();
            modelBuilder.Entity<Alarm>();
            modelBuilder.Entity<AlarmPosition>();
            modelBuilder.Entity<Attendance>();
            modelBuilder.Entity<AttendancePicture>(); 
            modelBuilder.Entity<Fleet>();
            modelBuilder.Entity<ReceiveLog>();
            modelBuilder.Entity<SysDictionary>();
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {

                if (!optionsBuilder.IsConfigured)
                {
                    optionsBuilder.UseMySQL(AppSettingHelper.GetConnectionString("dbconn"));
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// 用户
        /// </summary>
        public DbSet<User> User { get; set; }
        /// <summary>
        /// 船(状态)
        /// </summary>
        public DbSet<Models.Ship> Ship { get; set; }
        /// <summary>
        /// 组件
        /// </summary>
        public DbSet<Component> Component { get; set; }
        /// <summary>
        /// 设备
        /// </summary>
        public DbSet<Device> Device { get; set; }
        /// <summary>
        /// 摄像机
        /// </summary>
        public DbSet<Camera> Camera { get; set; }
        /// <summary>
        /// 船员
        /// </summary>
        public DbSet<Crew> Crew { get; set; }      
        /// <summary>
        /// 船员图片
        /// </summary>
        public DbSet<CrewPicture> CrewPicture { get; set; }
        /// <summary>
        /// 报警
        /// </summary>
        public DbSet<Alarm> Alarm { get; set; }          
        /// <summary>
        /// 报警位置
        /// </summary>
        public DbSet<AlarmPosition> AlarmPosition { get; set; }
        /// <summary>
        /// 算法配置
        /// </summary>
        public DbSet<Algorithm> Algorithm { get; set; }
        /// <summary>
        /// 考勤
        /// </summary>
        public DbSet<Attendance> Attendance { get; set; }
        /// <summary>
        /// 考勤图片
        /// </summary>
        public DbSet<AttendancePicture> AttendancePicture { get; set; }
        /// <summary>
        /// 船队管理
        /// </summary>
        public DbSet<Fleet> Fleet { get; set; }
        /// <summary>
        /// 接收日志
        /// </summary>
        public DbSet<ReceiveLog> ReceiveLog { get; set; }

        /// <summary>
        /// 接收日志
        /// </summary>
        public DbSet<SysDictionary> SysDictionary { get; set; }
        #region 扩展查询对象
        /// <summary>
        /// 船舶数量统计
        /// </summary>
        public DbSet<ShipCount> ShipCount { get; set; }
        #endregion
    }
}
