using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ.Sockets;
using ProtoBuf;

namespace ShipWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Task.Factory.StartNew(state =>
            {
                ProtoManager manager = new ProtoManager();
                manager.ComponentStart("22");
            }, TaskCreationOptions.LongRunning);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    //webBuilder.UseStartup<Dearler>();
                });
    }
}
