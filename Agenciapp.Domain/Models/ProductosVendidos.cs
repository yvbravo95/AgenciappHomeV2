using System;

using System.ComponentModel.DataAnnotations;


namespace AgenciappHome.Models
{
    public class ProductosVendidos
    {
        [Key]
        public Guid ProductosVendidiosId { get; set; }
        public ProductoBodega Product { get; set; }
        public Guid ProductId { get; set; }
        public int Cantidad { get; set; }
        public decimal Price { get; set; }
    }
}
