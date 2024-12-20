using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class BodegaProducto
    {
        [Key]
        [Column("IdBodega_Producto")]
        public Guid IdBodegaProducto { get; set; }

        [Required]
        [Display(Name = "Bodega")]
        public Guid IdBodega { get; set; }
        public Bodega Bodega { get; set; }

        [Required]
        [Display(Name = "Producto")]
        public Guid IdProducto { get; set; }
        public ProductoBodega Producto { get; set; }

        [Required]
        [Display(Name = "Cantidad")]
        [Column("Cantidad", TypeName = "decimal(10, 2)")]
        public decimal Cantidad { get; set; }

        [Display(Name = "Monto")]
        [Column("Monto", TypeName = "decimal(10, 2)")]
        public decimal Monto { get; set; }
    }
}
