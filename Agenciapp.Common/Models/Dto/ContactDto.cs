using System;

namespace Agenciapp.Common.Models.Dto
{
    public class ContactDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public PhoneDto Phone1 { get; set; }
        public PhoneDto Phone2 { get; set; }

        public string FullData { get { return $"{Name} {LastName}"; } }
    }
}