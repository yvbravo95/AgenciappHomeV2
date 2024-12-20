using System;
using System.ComponentModel.DataAnnotations;

namespace AgenciappHome.Models
{
    public class CategoriaBodega
    {
        [Key]
        public Guid IdCategoria { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
    }
}