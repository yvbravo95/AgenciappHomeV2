using System;

namespace RapidMultiservice.Models
{
    public class Address
    {
        public Guid Id { get; set; }
        public Guid ReferenceId { get; set; }
        public bool Current { get; set; }
        public string Type { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string countryiso2 { get; set; }

    }
}