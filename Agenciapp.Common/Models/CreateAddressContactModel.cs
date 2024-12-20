using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Common.Models
{
    public class CreateAddressContactModel
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Province { get; set; }
        public string Municipality { get; set; }
        public string Reparto { get; set; }
    }
}
