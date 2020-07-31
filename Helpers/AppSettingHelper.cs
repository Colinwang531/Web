using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.IO;

namespace ShipWeb.Helpers
{
    public static class AppSettingHelper
    {
        /// <summary>
        /// 配置
        /// </summary>
        static IConfiguration Configuration { get; set; }

        static AppSettingHelper()
        {
            Configuration = new ConfigurationBuilder()
                //1. 设置当前目录为基础目录
                .SetBasePath(Directory.GetCurrentDirectory())
                //2. 加载json文件 
                .Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = true, Optional = true })                
                .Build();
        }

        /// <summary>
        /// 获取配置节信息
        /// </summary>
        /// <param name="section">配置节，层级关系以英文:分隔表示(例：Logging:LogLevel:Default)</param>
        /// <returns></returns>
        public static string GetSectionValue(string section)
        {
            return Configuration[section];
        }

        /// <summary>
        /// 获取数据库连接
        /// </summary>
        /// <param name="name">配置节，数据库连接名</param>
        /// <returns></returns>
        public static string GetConnectionString(string name)
        {
            return Configuration.GetConnectionString(name);
        }
    }
}
