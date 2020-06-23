using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShipWeb.Models;
using ShipWeb.Helpers;

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
            modelBuilder.Entity<Users>();
            modelBuilder.Entity<Ship>();
            modelBuilder.Entity<Component>();
            modelBuilder.Entity<Embedded>();
            modelBuilder.Entity<Camera>();
            modelBuilder.Entity<CameraConfig>();
            modelBuilder.Entity<Employee>();
            modelBuilder.Entity<EmployeePicture>();
            modelBuilder.Entity<Alarm>();
            modelBuilder.Entity<AlarmInformation>();
            modelBuilder.Entity<AlarmInformationPosition>();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseMySql(AppSettingHelper.GetConnectionString("dbconn"));
        }

        /// <summary>
        /// 用户
        /// </summary>
        public DbSet<Users> Users { get; set; }
        /// <summary>
        /// 船(状态)
        /// </summary>
        public DbSet<Models.Ship> Ship { get; set; }
        /// <summary>
        /// 组件
        /// </summary>
        public DbSet<Component> Components { get; set; }
        /// <summary>
        /// 设备
        /// </summary>
        public DbSet<Embedded> Embedded { get; set; }
        /// <summary>
        /// 摄像机
        /// </summary>
        public DbSet<Camera> Camera { get; set; }
        /// <summary>
        /// 摄像机算法(配置)
        /// </summary>
        public DbSet<CameraConfig> CameraConfig { get; set; }
        /// <summary>
        /// 船员
        /// </summary>
        public DbSet<Employee> Employee { get; set; }
        /// <summary>
        /// 船员图片
        /// </summary>
        public DbSet<EmployeePicture> EmployeePicture { get; set; }
        /// <summary>
        /// 报警
        /// </summary>
        public DbSet<Alarm> Alarm { get; set; }
        /// <summary>
        /// 报警信息
        /// </summary>
        public DbSet<AlarmInformation> AlarmInformation { get; set; }
        /// <summary>
        /// 报警位置
        /// </summary>
        public DbSet<AlarmInformationPosition> AlarmInformationPosition { get; set; }
    }
}
