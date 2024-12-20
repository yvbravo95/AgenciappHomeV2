using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.IAirShippingServices.Models
{
    public class OrderWholesaler
    {
        public string ApiUser { get; set; }
        public string ApiPass { get; set; }
        public string Action { get; set; }
        public ParamsWholesaler Params { get; set; }
    }
}
