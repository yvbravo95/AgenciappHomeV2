
using System.ComponentModel;
using System.Reflection;

namespace Agenciapp.Common.Class
{
    public static class Extensions
    {
        public static string GetDescription<T>(this T value)
        {
            Type type = value.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("value must by of Enum type", "value");
            }
            MemberInfo[] memberInfo = type.GetMember(value.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }
            return value.ToString();
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string GetOnlyNumbers(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            string aux = string.Empty;
            foreach (var item in value)
            {
                if (char.IsDigit(item))
                    aux += item;
            }
            return aux;
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
