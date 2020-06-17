using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace ProtoBuf
{
    public class ProtoBufHelp
    {
        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <typeparam name="T">protobuf实体</typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static byte[] Serialize<T>(T model)
        {
            try
            {

                //涉及格式转换，需要用到流，将二进制序列化到流中
                using (MemoryStream ms = new MemoryStream())
                {
                    //使用ProtoBuf工具的序列化方法
                    ProtoBuf.Serializer.Serialize<T>(ms, model);

                    //定义二级制数组，保存序列化后的结果
                    byte[] result = new byte[ms.Length];
                    //将流的位置设为0，起始点
                    ms.Position = 0;
                    //将流中的内容读取到二进制数组中
                    ms.Read(result, 0, result.Length);
                    return result;
                }
            }
            catch (Exception ex)
            {
                // var aa=LoggerMessage.Define(LogLevel.Information, new EventId(1, nameof(Serialize)), "序列化开始");
                return null;
            }
        }

        /// 将收到的消息反序列化成对象
        /// < returns>The serialize.< /returns>
        /// < param name="msg">收到的消息.</param>
        public static T DeSerialize<T>(byte[] msg)
        {
            T result = default(T);
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    //将消息写入流中
                    ms.Write(msg, 0, msg.Length);
                    //将流的位置归0
                    ms.Position = 0;
                    //使用工具反序列化对象
                    result = ProtoBuf.Serializer.Deserialize<T>(ms);
                    return result;
                }
            }
            catch (Exception ex)
            {
                return result;
            }
        }
        /// <summary>
        /// 时间类型
        /// </summary>
        /// <returns></returns>
        public static int TimeSpan()
        {
            int timeSpan = ConvertDateTimeToInt(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            return timeSpan;
        }
        /// <summary>
        /// 把整型转为时间格式
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(int d)
        {
            DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0);
            startTime = startTime.AddSeconds(d).ToLocalTime();
            return startTime;
        }
        /// <summary>
        /// 字节流转字符串
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string strbyte(byte[] data)
        {
            string str = System.Text.Encoding.UTF8.GetString(data);
            return str;
        }
        /// <summary>
        /// 把时间格式转为整型
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int ConvertDateTimeToInt(string dt)
        {
            DateTime dt1 = new DateTime(1970, 1, 1, 8, 0, 0);
            DateTime dt2 = Convert.ToDateTime(dt);
            return Convert.ToInt32((dt2 - dt1).TotalSeconds);
        }
        /// <summary>
        /// 根据图片路径返回图片的字节流byte[]
        /// </summary>
        /// <param name="imagePath">图片路径</param>
        /// <returns>返回的字节流</returns>
        public static byte[] getImageByte(string imagePath)
        {
            FileStream files = new FileStream(imagePath, FileMode.Open);
            byte[] imgByte = new byte[files.Length];
            files.Read(imgByte, 0, imgByte.Length);
            files.Close();
            return imgByte;
        }

        /// <summary>
        /// 将字节流转换为图片
        /// </summary>
        /// <param name="Bytes"></param>
        /// <returns></returns>
        public static Image byteArrayToImage(byte[] Bytes)
        {
            MemoryStream ms = new MemoryStream(Bytes);
            return Bitmap.FromStream(ms, true);
        }
    }
}
