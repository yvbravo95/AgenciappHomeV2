using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Agenciapp.Domain.Models
{
    public class CargaAMSeguro
    {
        [Key]
        public Guid Id { get; set; }
        public Guid AgencyId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Value { get; set; }

        [ForeignKey("AgencyId")]
        public Agency Agency { get; set; }
    }
}
