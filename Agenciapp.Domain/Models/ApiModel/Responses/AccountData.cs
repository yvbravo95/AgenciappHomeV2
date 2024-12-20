using System;

namespace RapidMultiservice.Models.Responses
{
    public class AccountData
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Address Address { get; set; }
    }
}