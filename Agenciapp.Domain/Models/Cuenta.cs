using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciappHome.Models
{
    public class Cuenta
    {
        [Key]
        public Guid IdCuenta { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(20)]
        public string Rut { get; set; }

        [StringLength(250)]
        [Display(Name="Dirección")]
        public string Direccion { get; set; }

        [StringLength(50)]
        public string Comuna { get; set; }

        [StringLength(50)]
        public string Ciudad { get; set; }

        [StringLength(30)]
        public string Fono { get; set; }

        [Required]
        [Column("IdTipo_Cuenta")]
        [Display(Name="Tipo de Cuenta")]
        public Guid IdTipoCuenta { get; set; }

        [Display(Name="Tipo de Cuenta")]
        public TipoCuenta TipoCuenta { get; set; }
    }
}