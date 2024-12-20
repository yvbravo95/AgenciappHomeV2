using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AgenciappHome
{
    public static class Extensions
    {
        public static string GetDescription<T>(this T value)
        {
            if (value == null)
                return string.Empty;

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
    
        public static string SinTildes(this string texto) =>
        new String(
            texto.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray()
        )
        .Normalize(NormalizationForm.FormC);
    }

}

