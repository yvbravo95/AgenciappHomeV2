using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace AgenciappHome.Controllers.Class
{
    public class DataCookie
    {
        public static void setCookie(string key, string value, HttpResponse response)
        {
            var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions()
            {
                Path = "/", HttpOnly = false, IsEssential = true, //<- there
                Expires = DateTime.Now.AddMonths(1), 
            };
            response.Cookies.Append(key, value,cookieOptions);
            var y = response.Cookies;
        }

        public static string getCookie(string key, HttpRequest request)
        {
            var y = request.Cookies.ToList();
            return request.Cookies[key];
        }
    }
}
