using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciappHome.Models
{
    [Table("Unidades_Medida")]
    public class UnidadMedida
    {
        [Key]
        [Column("IdUnidad_Medida")]
        public Guid IdUnidadMedida { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
    }
}