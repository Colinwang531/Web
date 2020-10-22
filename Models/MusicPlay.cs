using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SmartWeb.Models
{
    public class MusicPlay
    {
        [DllImport("winmm.dll")]
        public static extern uint mciSendString(string lpstrCommand, string lpstrReturnString, uint uReturnLength, uint hWndCallback);
        public static void WindowPlaySleepMusic()
        {
            string filePath = AppContext.BaseDirectory + "music/bonvoyageSleep.mp3";
            mciSendString("play" + " " + filePath, null, 0, 0);
        }

        public static void LinuxPlaySleepMusic()
        {
            try
            {

                //dockerfile安装sox(&& apt-get install sox -y && apt-get install sox libsox-fmt-all)
                //执行shell
                string filePath = AppContext.BaseDirectory + "wwwroot/shell/alarm_voice.sh";
                //创建一个ProcessStartInfo对象 使用系统shell 指定命令和参数 设置标准输出
                //var psi = new ProcessStartInfo(filePath) { RedirectStandardOutput = true };
                //var psi = new ProcessStartInfo("dotnet","--info") { RedirectStandardOutput = true };
                var psi = new ProcessStartInfo("play bonvoyageSleep.wav") { RedirectStandardOutput = true };
                //启动
                var proc = Process.Start(psi);
                if (proc == null)
                {
                    Console.WriteLine("Can not exec.");
                }
                else
                {
                    Console.WriteLine("-------------Start read standard output啊啊啊--------------");
                    //开始读取
                    using (var sr = proc.StandardOutput)
                    {
                        while (!sr.EndOfStream)
                        {
                            Console.WriteLine(sr.ReadLine());
                        }

                        if (!proc.HasExited)
                        {
                            proc.Kill();
                        }
                    }
                    Console.WriteLine("---------------Read end嗯嗯嗯------------------");
                    Console.WriteLine($"Exited Code ： {proc.ExitCode}");

                }
            }
            catch (Exception e)
            {

                throw;
            }

        }

    }
}
