using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Common.Models.Dto
{
    public class AddressDto
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
        public string CountryIso2 { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public string FullAddress { get; set; }
    }
}
