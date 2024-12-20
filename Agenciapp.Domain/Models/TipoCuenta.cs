using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciappHome.Models
{
    [Table("Tipos_Cuenta")]
    public class TipoCuenta
    {
        [Key]
        [Column("IdTipo_Cuenta")]
        public Guid IdTipoCuenta { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
    }
}