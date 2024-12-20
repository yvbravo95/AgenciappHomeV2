using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciappHome.Models
{
    [Table("Estados_Movimiento")]
    public class EstadoMovimiento
    {
        [Key]
        [Column("IdEstado_Movimiento")]
        public Guid IdEstadoMovimiento { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
    }
}