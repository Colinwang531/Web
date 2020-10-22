using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Smartweb.Helpers;

namespace Smartweb.Hubs
{
    public class AlarmVoiceHub : Hub
    {
        private readonly MemoryCacheHelper cache = new MemoryCacheHelper();

        /// <summary>
        /// 构造 注入
        /// </summary>    
        public AlarmVoiceHub()
        {

        }

        /// <summary>
        /// 建立连接时触发
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("ReceiveAlarmVoice", -1, new { title = "系统消息", content = $"{Context.ConnectionId} 上线" });
        }

        /// <summary>
        /// 离开连接时触发
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await UserOffline();
            await Clients.All.SendAsync("ReceiveAlarmVoice", -4, new { title = "系统消息", content = $"{Context.ConnectionId} 离线" });
        }

        #region 推送业务处理
        /// <summary>
        /// 用户上线【缓存userid+cid】
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UserOnline()
        {
            #region 缓存ConnectionId
            var cid = Context.ConnectionId;
            cache.Set("shipOnlineKey", cid);
            await Clients.Client(cid).SendAsync("ReceiveAlarmVoice", -2, new { title = "系统消息", content = "上线成功" });
            #endregion

            return true;
        }

        /// <summary>
        /// 用户离线【移除缓存】
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UserOffline()
        {
            var cid = Context.ConnectionId;
            cache.RemoveByValue(cid);
            await Clients.Client(cid).SendAsync("ReceiveAlarmVoice", -3, new { title = "系统消息", content = "离线成功" });
            return true;
        }
        #endregion

        public async Task<bool> TestAlarmV()
        {
            var cid = Context.ConnectionId;
            if (!string.IsNullOrEmpty(cid))
                await Clients.Client(cid).SendAsync("ReceiveAlarmVoice", 200, new { code = 1, type = "bonvoyageSleep" });
            return true;
        }

    }
}
