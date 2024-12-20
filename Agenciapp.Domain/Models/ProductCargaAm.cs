using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Agenciapp.Domain.Models
{
    public class ProductCargaAm
    {
        [Key]
        public Guid Id { get; set; }
        public Guid OrderCubiqId { get; set; }
        public Guid? ProductoBodegaId { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        [ForeignKey("ProductoBodegaId")]
        public ProductoBodega ProductoBodega { get; set; }
        [ForeignKey("OrderCubiqId")]
        public OrderCubiq OrderCubiq { get; set; }
    }
}
