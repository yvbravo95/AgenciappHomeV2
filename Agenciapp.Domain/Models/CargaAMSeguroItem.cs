using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Agenciapp.Domain.Models
{
    public class CargaAMSeguroItem
    {
        [Key]
        public Guid Id { get; set; }
        public Guid CargaAMSeguroId { get; set; }
        [ForeignKey("CargaAMSeguroId")]
        public CargaAMSeguro CargaAMSeguro { get; set; }
        public Guid CargaId { get; set; }
        [ForeignKey("CargaId")]
        public OrderCubiq Carga { get; set; }
    }
}
