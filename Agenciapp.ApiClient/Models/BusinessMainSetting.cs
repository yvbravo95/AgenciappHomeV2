using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.ApiClient.Models
{
    public class BusinessMainSetting
    {
        public string BaseUrl { get; set; }
        public Client Client { get; set; }
    }

    public class Client
    {
        public string GetByFilter { get; set; }
    }
}
