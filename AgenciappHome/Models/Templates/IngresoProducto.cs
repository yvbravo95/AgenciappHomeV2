using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AgenciappHome.Models;

namespace BodegaApp.ViewModels
{
    public class IngresoProducto
    {
        public string NombreProducto { get; set; }
        public string CodigoProducto { get; set; }
        public string DescripcionProducto { get; set; }
        public string CategoriaProducto { get; set; }
        public decimal Cantidad { get; set; }
        public string UnidadMedidaProducto { get; set; }
        public decimal Precio { get; set; }
    }
}