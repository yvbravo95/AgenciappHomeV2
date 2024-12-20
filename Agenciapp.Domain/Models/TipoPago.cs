using System;
using System.Collections.Generic;

namespace AgenciappHome.Models
{
    public partial class TipoPago
    {
        public TipoPago()
        {
            Order = new HashSet<Order>();
        }

        public Guid TipoPagoId { get; set; }
        public string Type { get; set; }

        public ICollection<Order> Order { get; set; }
    }
}
