using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ShipWeb.Models
{
    public class MusicPlay
    {
        [DllImport("winmm.dll")]
        public static extern uint mciSendString(string lpstrCommand, string lpstrReturnString, uint uReturnLength, uint hWndCallback);
        public static void PlaySleepMusic() 
        {
            string filePath = AppContext.BaseDirectory + "/music/bonvoyageSleep.mp3";
            mciSendString("play" + " " + filePath, null, 0, 0);
        }
    }
}
