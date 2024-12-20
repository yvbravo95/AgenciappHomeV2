using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgenciappHome.Models
{
    public partial class Office
    {
        public Office()
        {
            Order = new HashSet<Order>();
            Shipping = new HashSet<Shipping>();
            formBuilder = new List<formBuilder>();
        }
        [Key]
        public Guid OfficeId { get; set; }
        public Guid AgencyId { get; set; }
        public string Name { get; set; }

        public Agency Agency { get; set; }
        public Phone OfficePhone { get; set; }
        public Address OfficeAddress { get; set; }
        public ICollection<Order> Order { get; set; }
        public ICollection<Shipping> Shipping { get; set; }
        public List<formBuilder> formBuilder { get; set; }

    }
}
