using System;
using System.Collections.Generic;

namespace AgenciappHome.Models.Request.Shipping
{
    public class ChangeStatusRecibidaModel
    {
        public ChangeStatusRecibidaModel()
        {
            Ids = new List<Guid>();
        }
        public List<Guid> Ids { get; set; }
    }
}
