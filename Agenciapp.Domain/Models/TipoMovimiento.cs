using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciappHome.Models
{
    [Table("Tipos_Movimiento")]
    public class TipoMovimiento
    {
        [Key]
        [Column("IdTipo_Movimiento")]
        public Guid IdTipoMovimiento { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
    }
}