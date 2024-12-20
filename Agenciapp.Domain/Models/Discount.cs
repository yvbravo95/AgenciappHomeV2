using Agenciapp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Models
{
    public class Discount
    {
        public Guid Id { get; set; }
        public decimal Value { get; set; }
        public RateType Type { get; set; }
    }
}
