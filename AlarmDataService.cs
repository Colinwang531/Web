using Microsoft.Extensions.Hosting;
using ShipWeb.ProtoBuffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShipWeb
{
    public class AlarmDataService: BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //InitManger.Alarm();
                }
                catch (Exception ex)
                {
                    //LogHelper.Error(ex.Message);
                }
                await Task.Delay(50000, stoppingToken);//等待1秒
            }
        }
    }
}
