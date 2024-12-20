using AgenciappHome.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgenciappHome.Models
{
    [Table("Movimientos_Productos")]
    public class MovimientoProducto
    {
        [Key]
        [Column("IdMovimiento_Producto")]
        public Guid IdMovimientoProducto { get; set; }

        public Guid IdAgency { get; set; }

        [Required]
        [Display(Name="Movimiento")]
        public Guid IdMovimiento { get; set; }
        public Movimiento Movimiento { get; set; }

        [Required]
        [Display(Name="Producto")]
        public Guid IdProducto { get; set; }
        public ProductoBodega Producto { get; set; }

        [Required]
        [Column("Cantidad", TypeName="decimal(10, 2)")]
        public decimal Cantidad { get; set; }

        [Column("Precio", TypeName="decimal(11, 2)")]
        public decimal Precio { get; set; }

        public DateTime Date { get; set; }
    }
}