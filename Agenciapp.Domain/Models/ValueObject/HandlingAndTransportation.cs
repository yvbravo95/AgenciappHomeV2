using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Domain.Models.ValueObject
{
    [Owned]
    public class HandlingAndTransportation
    {
        public decimal Cost { get; set; }
        public decimal Sale { get; set; }
        public decimal CostCubiq { get; set; }
    }
}
