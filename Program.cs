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
        /// �����д������������� ��ʽ SmartWeb.exe -a 192.168.0.17 -p 3002
        /// -a��ָ������MQ��Ϣע��ĵ�ַ -p��ָ����ע����Ϣ�Ķ˿ں�
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var settings = new Dictionary<string, string> {
                {"ipaddr","192.168.0.13" },
                {"port1","61001" },//netmqu�Ķ˿�
                {"port2","5556" },//pub�Ķ˿�
                {"port3","7709" }//web�Ķ˿�
            };
            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .AddCommandLine(args);  //���Խ��յ���Ӧ�ó������
            var configuration = builder.Build();
              var ip = configuration["ipaddr"];
            //����netmq�Ķ˿�
            var port1 = configuration["port1"];
            //��IPad���͵Ķ˿�
            var port2 = configuration["port2"];
            //��½½�ض˵Ķ˿�
            var port3 = configuration["port3"];
            ManagerHelp.IP = "tcp://" +ip + ":" + port1;
            ManagerHelp.PublisherIP = "tcp://*:" + port2; 
            //7708Ϊ½�ض�
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
