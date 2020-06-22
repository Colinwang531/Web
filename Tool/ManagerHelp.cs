using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShipWeb.Tool
{
    public class ManagerHelp
    {
        private static ManagerHelp _manager = null;
        public static string ShipId = "";
        public static string Cid = "5555";
        /// <summary>
        /// 是否有设置权限
        /// </summary>
        public static bool IsSet = true;
        public static Dictionary<int, string> components;
        private static object ShipId_Lock = new object(); //锁同步
        private static object Cid_Lock = new object(); //锁同步
        //int aa;
        //单例控制dealer只有一个。
        public ManagerHelp()
        {
            lock (ShipId_Lock)
            {
                if (ShipId == null)
                {
                    _manager = new ManagerHelp();
                }
            }
            lock (Cid_Lock)
            {
                if (Cid == null)
                {
                    _manager = new ManagerHelp();
                }
            }
        }
        /// <summary>
        /// 未做转换的字节流
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public static byte[] DrawAlarm(byte[] bytes, int x, int y, int w, int h)
        {
            byte[] byt = Convert.FromBase64String(Encoding.UTF8.GetString(bytes));
            using (var stream = new MemoryStream(byt, 0, byt.Length, false, true))
            {
                Image image = Image.FromStream(stream);
                Graphics.FromImage(image).DrawRectangle(new Pen(Brushes.Red, 5), x, y, w, h);
                var ms = new MemoryStream();
                image.Save(ms, ImageFormat.Png);
                return ms.GetBuffer();
            }
        }
    }
}
