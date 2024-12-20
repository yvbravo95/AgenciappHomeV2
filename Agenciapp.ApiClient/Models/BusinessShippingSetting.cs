using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.ApiClient.Models
{
    public class BusinessShippingSetting
    {
        public string BaseUrl { get; set; }
        public Shipping Shipping { get; set; }
    }

    public class Shipping
    {
        public string GetByFilter { get; set; }
        public string GetById { get; set; }
    }
}
