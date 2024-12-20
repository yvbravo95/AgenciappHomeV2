using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.IContactServices.Models
{
    public class EditContactModel
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string PhoneNumberMovil { get; set; }
        public string PhoneNumberCasa { get; set; }
        public string CI { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Province { get; set; }
        public string Municipality { get; set; }
        public string Reparto { get; set; }
    }
}
