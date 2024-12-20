using System;
using System.Collections.Generic;

namespace AgenciappHome.Models
{
    public partial class Carrier
    {
        public Carrier()
        {
            Shipping = new HashSet<Shipping>();
        }

        public Guid CarrierId { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime CreateAt { get; set; }
        public Agency Agency { get; set; }
        public Guid AgencyId { get; set; }
        public Client Client { get; set; }
        public Guid? ClientId { get; set; }

        public ICollection<Shipping> Shipping { get; set; }

        public string FullName { get{return Name + " " + LastName;} }

        public Address Address { get;  set; }
    }
}
