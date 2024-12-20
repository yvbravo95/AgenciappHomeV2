using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgenciappHome.Models.Request.Order
{
    public class TransferirAMayoristaMV
    {
        [Required] public Guid WholesalerId { get; set; }
        public List<string> OrdersNumber { get; set; }
    }
}
