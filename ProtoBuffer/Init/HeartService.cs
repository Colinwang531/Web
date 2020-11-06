using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Smartweb.Hubs;
using SmartWeb.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartWeb.ProtoBuffer.Init
{
    public class HeartService:BackgroundService
    {
        InitManger manger = new InitManger();
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            manger.Init();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //心跳
                    manger.HeartBeat();
                }
                catch (Exception ex)
                {
                    //LogHelper.Error(ex.Message);
                }
                await Task.Delay(1000 * 30, stoppingToken);//单位秒
            }
        }
    }
}
