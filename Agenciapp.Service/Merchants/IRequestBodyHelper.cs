using System;
using System.Collections.Generic;
using AgenciappHome.Models.Payment;

namespace Agenciapp.Service.Merchants
{
    public interface IRequestBodyHelper
    {
        Dictionary<string, string> GetBody(object obj);

    }
    public class RequestBodyHelper : IRequestBodyHelper
    {
        public Dictionary<string, string> GetBody(object obj)
        {
            var resutl = new Dictionary<string, string>();
            foreach (var property in obj.GetType().GetProperties())
            {
                var prop = obj.GetType().GetProperty(property.Name);
                var value = prop.GetValue(obj);
                if (value != null)
                {
                    var type = property.PropertyType;
                    if (type.IsEnum)
                    {
                        var en = value as Enum;
                        var description = Description.GetDescription(en);
                        if (description != "validate")
                            resutl.Add(ConvertCamel(property.Name), description);
                    }
                    else
                    {
                        resutl.Add(ConvertCamel(property.Name), value.ToString());
                    }
                }
            }

            return resutl;
        }

        private string ConvertCamel(string input)
        {
            var result = string.Empty;
            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]) && i != 0)
                    result += "_";
                result += input[i];
            }

            return result.ToLower();
        }
    }
}