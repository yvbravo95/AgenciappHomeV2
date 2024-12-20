using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.ApiClient.Models
{
    public class SecuritySetting
    {
        public string BaseUrl { get; set; }
        public LoginSetting Login { get; set; }

        public class LoginSetting
        {
            public string Authenticate { get; set; }
        }
    }
}
