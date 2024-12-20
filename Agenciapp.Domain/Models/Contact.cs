using System;
using System.Collections.Generic;

namespace AgenciappHome.Models
{
    public partial class Contact
    {
        public Contact()
        {
            Order = new HashSet<Order>();
        }

        public Guid ContactId { get; set; }
        public string ContactNumber { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber1 { get; set; }
        public string PhoneNumber2 { get; set; }
        public Phone Phone1 { get; set; }
        public Phone Phone2 { get; set; }
        public Address Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CI { get; set; }

        public ICollection<Order> Order { get; set; }

        public string FullData { get { return Name + " " + LastName; } }
    }
}
