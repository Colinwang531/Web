
using Newtonsoft.Json;
using ShipWeb.Helpers;
using ShipWeb.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ShipWeb.Tool
{
    public class ManagerHelp
    {
        // private static ManagerHelp _manager = null;
        /// <summary>
        /// 组件ID（组件注册成功后返回的ID）
        /// </summary>
        public static string Cid = "";
        //是否启动模似数据
        public static bool IsTest = AppSettingHelper.GetSectionValue("IsSimulate") == "true" ? true : false;
        /// <summary>
        /// 发布IP
        /// </summary>
        public static string PublisherIP = "";
        /// <summary>
        /// mq绑定的地址
        /// </summary>
        public static string IP = "";
        /// <summary>
        /// 导出时显示的公司名称
        /// </summary>
        public static string ExportCompany = "";
        /// <summary>
        /// 缺岗执行时间单位分名钟
        /// </summary>
        public static string DepartureTime = "";
        /// <summary>
        /// 是否显示返回陆地端菜单
        /// </summary>
        //public static bool IsShowLandHome = false;
        /// <summary>
        /// 查询报警信息权限
        /// </summary>
        public static bool IsShowAlarm = true;
        private static object Cid_Lock = new object(); //锁同步
        /// <summary>
        /// 存放proto返回的消息
        /// </summary>
        public static string CrewReponse = "";
        public static string DeviceReponse = "";
        public static string ComponentReponse = "";
        public static string AlgorithmReponse = "";
        public static string StatusReponse = "";
        /// <summary>
        /// 人脸算法组件名称
        /// </summary>
        public static string FaceName = "FaceRecognize";
        public static List<AtWork> atWorks = null;
        /// <summary>
        /// 是否初使化
        /// </summary>
        public static bool isInit = false;
        /// <summary>
        /// 是否陆地端给定时间刷新船是否失去联系
        /// </summary>
        public static bool isLand = false;
        //int aa;
        //单例控制dealer只有一个。
        //public ManagerHelp()
        //{
        //    lock (Cid_Lock)
        //    {
        //        if (Cid == null)
        //        {
        //            _manager = new ManagerHelp();
        //        }
        //    }
        //}

        /// <summary>
        /// 未做转换的字节流
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public static byte[] DrawAlarm(string pir, int x, int y, int w, int h)
        {
            //byte[] bytes = Encoding.ASCII.GetBytes("data:image/jpeg;base64," + pir);
            //byte[] bytes = Encoding.ASCII.GetBytes(pir);
            string dummyData = pir.Trim().Replace("%", "").Replace(",", "").Replace(" ", "+");
            if (dummyData.Length % 4 > 0)
            {
                dummyData = dummyData.PadRight(dummyData.Length + 4 - dummyData.Length % 4, '=');
            }
            
            byte[] bytes = Convert.FromBase64String(dummyData);
            try
            {
                //string fullPath = "C:/Users/Dell/Desktop/test1.jpg";
                //byte[] byt = new byte[0];
                //using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                //{
                //    stream.Position = 0;
                //    Byte[] imgByte = new Byte[stream.Length];//把图片转成 Byte型 二进制流 
                //    stream.Read(imgByte, 0, imgByte.Length);//把二进制流读入缓冲区 
                //    stream.Close();
                //    Image image = Image.FromStream(stream);
                //    Graphics.FromImage(image).DrawRectangle(new Pen(Brushes.Red, 5), x, y, w, h);
                //    var ms = new MemoryStream();
                //    image.Save(ms, ImageFormat.Png);
                //    return ms.GetBuffer();
                //}                              

                using (var stream = new MemoryStream(bytes))
                {
                    Image image = Image.FromStream(stream);

                    stream.Position = 0;

                    Graphics.FromImage(image).DrawRectangle(new Pen(Brushes.Red, 5), x, y, w, h);
                    var ms = new MemoryStream();
                    image.Save(ms, ImageFormat.Png);
                    return ms.GetBuffer();
                }
            }
            catch (Exception ex)
            {
                return bytes;
            }
        }
        /// <summary>
        /// 组合Html
        /// </summary>
        /// <param name="list"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string GetHtml(List<AlarmViewModel> list, string time, string shipName, string address)
        {
            string commpany = ExportCompany;
            var sb = new StringBuilder();
            sb.Append(@"<html>
                            <head>
                              <meta charset='UTF-8'>
                              <title></title>
                                <style>
                                    table{
                                        border-collapse: collapse;
                                        border-spacing: 0;
    	                                border-color: grey;
		                                border-left:1px solid #666;
		                                border-bottom:1px solid #666;
		                                border-top:1px solid #666;
                                        background-color: #eee;
		                                width:100%;
		                                font-size: 26px
                                    }
                                    table td 
                                    {
                                        border-right:1px solid #666;
		                                border-top:1px solid #666;
                                        background-color: #fff;
		                                text-algin:center;
		                                padding-left: 20px;
                                    } 
                                    table th
                                    {
                                        border-right:1px solid #666;
                                        border-top:1px solid #666;
		                                text-align: left;
		                                height:50px;
		                                padding-left: 20px;
                                    }
                                    table tr {
                                        transition: all.3s;
		                                height:40px;
                                        -webkit-transition: all.3s;
                                    }
                                </style>                    
                            </head>
                              <body>
                                <div style='text-align: center'>");
            string url = AppDomain.CurrentDomain.BaseDirectory + "/images/head.jpg";
            sb.Append("<img style='width:100%;heihgt:100%;' src='data:image/jpeg;base64," + WebImageToBase64(url) + "'><br>");
            sb.Append("<label style='font-size:65px;font-weight:bold;letter-spacing:5px'>航安人工智能系统报警报告</label><br>");
            string src = AppDomain.CurrentDomain.BaseDirectory + "/images/title.jpg";
            sb.Append("<img style='width:100%;heihgt:100%;' src='data:image/jpeg;base64," + WebImageToBase64(src) + "'><br>");
            sb.Append(@"<table>
                                <tr>
                                    <th>船名</th>                                 
                                    <th>所属公司</th>                                 
                                    <th>报警时间</th>
                                  </tr>
                                  <tr>
                                    <td>" + shipName + @"</td>
                                    <td>" + commpany + @"</td>
                                    <td>" + time + "</td></tr> </table></div> ");
            sb.Append("<div >");
            foreach (var item in list)
            {
                sb.Append("<div style='margin-top:40px;'>");
                string typeView = "未配带安全帽";
                if (item.Type == 2) typeView = "打电话";
                if (item.Type == 3) typeView = "睡觉";
                if (item.Type == 4) typeView = "打架";
                sb.AppendLine("<label style='font-size:46px;font-weight:bold;'>报警类型： " + typeView + "</label><br/>");
                sb.AppendLine("<label style='font-size:46px;font-weight:bold;'>报警区域： " + item.NickName + "</label><br/>");
                byte[] pic = DrawAlarm(item.Picture, item.X, item.Y, item.W, item.H);


                sb.AppendLine("<img style='width: 960px; height: 550px'  src='data:image/jpeg;base64," + Encoding.ASCII.GetString(pic) + "'/><br/>");
                sb.AppendLine("</div>");
            }
            sb.AppendLine("</div></body> </html>");
            return sb.ToString();
        }

        /// <summary>
        /// 图片转换图片流方法
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        private static string imgbytefromimg(string path)
        {
            try
            {
                FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read);
                Byte[] imgByte = new Byte[f.Length];//把图片转成 Byte型 二进制流 
                f.Read(imgByte, 0, imgByte.Length);//把二进制流读入缓冲区 
                f.Close();
                return Convert.ToBase64String(imgByte);
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 下载Web网页的图片，并转换为Base64String格式
        /// </summary>
        /// <param name="urlAddress">图片URL地址</param>
        /// <returns></returns>
        public static string WebImageToBase64(string urlAddress)
        {
            try
            {
                Uri url = new Uri(urlAddress);
                WebRequest webRequest = WebRequest.Create(url);
                WebResponse webResponse = webRequest.GetResponse();
                Bitmap myImage = new Bitmap(webResponse.GetResponseStream());
                MemoryStream ms = new MemoryStream();
                myImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                return Convert.ToBase64String(arr);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
