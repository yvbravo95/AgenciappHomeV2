using Agenciapp.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.IContactServices.Models
{
    public class CreateContactModel
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string PhoneNumberMovil { get; set; }
        public string PhoneNumberCasa { get; set; }
        public string CI { get; set; }
        public CreateAddressContactModel Address { get; set; }
    }
}
