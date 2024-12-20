using System;
using System.ComponentModel.DataAnnotations;

namespace AgenciappHome.Models
{
    public class CostoCarga
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? AgencyId { get; set; }
        public decimal Value { get; set; }
        public decimal Value2 { get; set; }
        public Zona Zona { get; set; }
    }
}