using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ShipWeb.Extend
{
    /// <summary>
    /// 枚举特性操作
    /// @author Ahri 2020.6.23
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// 获取当前枚举值的Remark
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetRemark(this Enum value)
        {
            string remark = string.Empty;
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());

            object[] attrs = fieldInfo.GetCustomAttributes(typeof(RemarkAttribute), false);
            RemarkAttribute attr = (RemarkAttribute)attrs.FirstOrDefault(a => a is RemarkAttribute);
            if (attr == null)
            {
                remark = fieldInfo.Name;
            }
            else
            {
                remark = attr.Remark;
            }
            return remark;
        }
        /// <summary>
        /// 获取当前枚举的全部Remark
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<KeyValuePair<string, string>> GetAllRemarks(this Enum value)
        {
            Type type = value.GetType();
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            foreach (var field in type.GetFields())
            {
                if (field.FieldType.IsEnum)
                {
                    object temp = field.GetValue(value);
                    Enum enumValue = (Enum)temp;
                    int intValue = (int)temp;
                    result.Add(new KeyValuePair<string, string>(intValue.ToString(), enumValue.GetRemark()));
                }
            }
            return result;
        }

        /// <summary>
        /// 获取枚举列表
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<KeyValuePair<string, string>> GetAll(this Type enumType)
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            foreach (var field in enumType.GetFields())
            {
                if (field.FieldType.IsEnum)
                {
                    object temp = field.GetValue(enumType);
                    Enum key = (Enum)temp;
                    string val = ((int)enumType.InvokeMember(field.Name, BindingFlags.GetField, null, null, null)).ToString();
                    result.Add(new KeyValuePair<string, string>(val, key.GetRemark()));
                }
            }
            return result;
        }
    }

    /// <summary>
    /// RemarkAttribute 字段描述特性
    /// @author Ahri 2020.6.23
    /// </summary>
    public class RemarkAttribute : Attribute
    {
        private string _remark = string.Empty;
        public RemarkAttribute(string remark) { _remark = remark; }
        /// <summary>
        /// 字段描述
        /// </summary>
        public string Remark { get { return _remark; } set { _remark = value; } }
    }
}
