using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class MinoristaPasaporte
    {
        [Key]
        public Guid Id { get; set; }

        public Guid AgencyId { get; set; }
        
        public Agency Agency { get; set; }

        public string Nombre { get; set; }

        public string Codigo { get; set; }

        public decimal Pricio1 { get; set; }
        public decimal Pricio2 { get; set; }
        public decimal Pricio3 { get; set; }
        public decimal Pricio4 { get; set; }
        public decimal Pricio5 { get; set; }
        public decimal PricioP1Exp { get; set; }
        public decimal PricioP2Exp { get; set; }
    }    
}
