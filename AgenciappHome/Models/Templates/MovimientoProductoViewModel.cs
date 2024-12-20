using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BodegaApp.ViewModels
{
    public class MovimientoProductoViewModel
    {
        public string Tipo { set; get; }

        public string Bodega { set; get; }

        public string Producto { set; get; }

        public decimal Cantidad { set; get; }

        public decimal Precio { set; get; }

        public DateTime Date { get; set; }
    }
}
