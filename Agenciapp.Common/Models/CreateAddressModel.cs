using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Common.Models
{
    public class CreateAddressModel
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string Countryiso2 { get; set; }
    }
}
