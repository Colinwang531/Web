﻿using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShipWeb
{
    public class HeartService:BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            InitManger.Init();
            //船舶端需要定时检查是否缺岗
            //InitManger.LoadNotice();
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
