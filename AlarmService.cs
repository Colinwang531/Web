using Microsoft.Extensions.Hosting;
using ShipWeb.ProtoBuffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShipWeb
{
    public class AlarmService: BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //报警
                    //InitManger.Alarm();
                }
                catch (Exception ex)
                {
                    //LogHelper.Error(ex.Message);
                }
                await Task.Delay(1000*60*30, stoppingToken);//单位秒
            }
        }
    }
}
