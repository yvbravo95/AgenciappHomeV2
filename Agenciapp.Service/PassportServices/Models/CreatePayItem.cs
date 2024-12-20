using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.PassportServices.Models
{
    public class CreatePayItem
    {
        public Guid PaymentTypeId { get; set; }
        public decimal Amount { get; set; }
    }
}
