using Microsoft.Extensions.Hosting;
using ShipWeb.Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShipWeb.ProtoBuffer.Init
{
    public class HeartService:BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            InitManger.Init();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //心跳
                    InitManger.HeartBeat();
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
