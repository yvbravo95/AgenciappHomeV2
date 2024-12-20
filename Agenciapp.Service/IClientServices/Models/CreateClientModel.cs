using Agenciapp.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Service.IClientServices.Models
{
    public class CreateClientModel
    {
        public DateTime? BirthDate { get; set; }
        [Required]public string Name { get; set; }
        public string Name2 { get; set; }
        [Required]public string LastName { get; set; }
        public string LastName2 { get; set; }
        public string Email { get; set; }
        public string ID { get; set; }
        [Required]public string PhoneNumber { get; set; }
        public string PhoneCubaNumber { get; set; }
        public bool EnableNotifications { get; set; }
        public bool Conflictivo { get; set; }
        public bool IsCarrier { get; set; }
        public CreateAddressModel Address { get; set; }
    }
}
