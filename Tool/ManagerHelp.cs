
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
        //船ID
        public static string ShipId = "";
        /// <summary>
        /// 组件ID（组件注册成功后返回的ID）
        /// </summary>
        public static string Cid = "";

        /// <summary>
        /// 是否显示返回陆地端菜单
        /// </summary>
        public static bool IsShowLandHome = false;
        /// <summary>
        /// 显示登陆名称
        /// </summary>
        //public static string LoginName = "";
        /// <summary>
        /// 是否有设置权限
        /// </summary>
        //public static bool IsSet = true;
        /// <summary>
        /// 查询报警信息权限
        /// </summary>
        public static bool IsShowAlarm = true;
        //public static Dictionary<int, string> components;
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
            try
            {
                using (var stream = new MemoryStream(byt, 0, byt.Length, false, true))
                {
                    Image image = Image.FromStream(stream);
                    Graphics.FromImage(image).DrawRectangle(new Pen(Brushes.Red, 5), x, y, w, h);
                    var ms = new MemoryStream();
                    image.Save(ms, ImageFormat.Png);
                    return ms.GetBuffer();
                }
            }
            catch (Exception ex)
            {
                return byt;
            }
        }
    }
}
