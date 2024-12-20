using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Agenciapp.Domain.Enums;

namespace AgenciappHome.Models
{
    public class Credito
    {
        public Guid CreditoId { get; set; }
        public Guid ClientId { get; set; }
        public Client Client { get; set; }
        public decimal value { get; set; }
        public string Referencia { get; set; }
    }

    public class Credit
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal value { get; set; }
        public string Referencia { get; set; }
    }

    public class CreditWholesaler: Credit
    {
        public MoneyType MoneyType { get; set; }
        public Guid WholesalerId { get; set; }
        [ForeignKey("WholesalerId")]
        public Wholesaler Wholesaler { get; set; }
    }

    
}
