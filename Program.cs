using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        /// �����д������������� ��ʽ SmartWeb.exe -a 192.168.0.17 -p 3002
        /// -a��ָ������MQ��Ϣע��ĵ�ַ -p��ָ����ע����Ϣ�Ķ˿ں�
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var settings = new Dictionary<string, string> {
                {"ipaddr","127.0.0.1" },
                {"port","3002" }
            };

            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .AddCommandLine(args);  //���Խ��յ���Ӧ�ó������
            var configuration = builder.Build();
            var ip = configuration["ipaddr"];
            var port = configuration["port"];
           // ManagerHelp.IP = "tcp://" +ip + ":" + port;
            //7708Ϊ½�ض�
            if (port == "7708") {
                ManagerHelp.IsShipPort = false;
            }
            CreateHostBuilder(args).Build().Run();
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
