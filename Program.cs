using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
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
            //while (true)
            //{
                //try
                //{
                    System.Diagnostics.Debug.WriteLine("test");
                    if (args.Length >= 3)
                    {
                        string inputIp = args[1];
                        Console.WriteLine(inputIp);
                        string inputPort = args[3];
                        Console.WriteLine(inputPort);
                        string IP = "tcp://" + inputIp + ":" + inputPort;
                        ManagerHelp.IP = IP;
                    }
                    CreateHostBuilder(args).Build().Run();
                //}
                //catch (Exception ex)
                //{
                //    System.Threading.Thread.Sleep(1000);
                //}
            //}
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
