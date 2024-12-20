using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Agenciapp.ApiClient.Utils
{
    public static class QueryUtils
    {
        public static string GetQuery<T>(T data)
        {
            var properties = from p in data.GetType().GetProperties()
                             where p.GetValue(data, null) != null
                             select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(data, null).ToString());

            string queryString = string.Join("&", properties.ToArray());
            return queryString;
        }
    }
}
