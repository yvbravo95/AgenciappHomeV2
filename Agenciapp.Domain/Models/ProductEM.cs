using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class ProductEM
    {
        [Key]
        public Guid Id { get; set; }
        public Guid IdProduct { get; set; }
        public Guid IdEnvioMaritimo { get; set; }
        public int cantidad { get; set; }
        public decimal Peso { get; set; }
        [ForeignKey("IdProduct")]
        public Product Product { get; set; }
    }
}
