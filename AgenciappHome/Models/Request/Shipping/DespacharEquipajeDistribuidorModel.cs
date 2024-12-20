using System;
using System.Collections.Generic;

namespace AgenciappHome.Models.Request.Shipping
{
    public class DespacharEquipajeDistribuidorModel
    {
        public DespacharEquipajeDistribuidorModel()
        {
            ShippingIds = new List<Guid>();
        }
        public Guid UserId { get; set; }
        public List<Guid> ShippingIds { get; set; }
    }
}
