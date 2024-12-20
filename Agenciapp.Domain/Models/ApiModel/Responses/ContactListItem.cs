using System;

namespace RapidMultiservice.Models.Responses
{
    public class ContactListItem
    {
        public Guid Id { get; set; }
        public string ContactNumber { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber1 { get; set; } //Móvil
        public string PhoneNumber2 { get; set; } //Fijo
    }
}