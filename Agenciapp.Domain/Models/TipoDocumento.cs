using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciappHome.Models
{
    [Table("Tipos_Documento")]
    public class TipoDocumento
    {
        [Key]
        [Column("IdTipo_Documento")]
        public Guid IdTipoDocumento { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
    }
}