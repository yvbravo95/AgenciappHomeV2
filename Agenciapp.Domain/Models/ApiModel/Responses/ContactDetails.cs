using System;

namespace RapidMultiservice.Models.Responses
{
    public class ContactDetails
    {
        public Guid Id { get; set; }
        public string ContactNumber { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber1 { get; set; }
        public string PhoneNumber2 { get; set; }
        public string Ci { get; set; }
        

        public Address Addres { get; set; }
    }
}