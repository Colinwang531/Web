using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ.Sockets;
using ProtoBuf;
using SmartWeb.ProtoBuffer;
using SmartWeb.Tool;

namespace SmartWeb
{
    public class Program
    {
        /// <summary>
        /// 命令行传入的入数据入口 格式 SmartWeb.exe -a 192.168.0.17 -p 3002
        /// -a：指的是向MQ消息注册的地址 -p：指的是注册消息的端口号
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var settings = new Dictionary<string, string> {
                {"ipaddr","192.168.0.13" },
                {"port1","61001" },//netmqu的端口
                {"port2","5556" },//pub的端口
                {"port3","7709" }//web的端口
            };
            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .AddCommandLine(args);  //可以接收调试应用程序参数
            var configuration = builder.Build();
              var ip = configuration["ipaddr"];
            //连接netmq的端口
            var port1 = configuration["port1"];
            //向IPad推送的端口
            var port2 = configuration["port2"];
            //登陆陆地端的端口
            var port3 = configuration["port3"];
            ManagerHelp.IP = "tcp://" +ip + ":" + port1;
            ManagerHelp.PublisherIP = "tcp://*:" + port2; 
            //7708为陆地端
            if (port3== "7708") {
                ManagerHelp.IsShipPort = false;
            }
            while (true)
            {
                try
                {
                    CreateHostBuilder(args).Build().Run();
                }
                catch (Exception ex)
                {
                    //CreateHostBuilder(args).Build().Run();
                }
                Thread.Sleep(10000);
            }
            
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    //logging.AddDebug();
                });
    }
}
