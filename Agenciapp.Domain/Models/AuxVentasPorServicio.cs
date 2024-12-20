using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class AuxVentasPorServicio
    {
        public List<ParAux<string, decimal>> servicios = new List<ParAux<string, decimal>>();
        public decimal getTotal()
        {
            decimal total = 0;
            foreach (var item in servicios)
            {
                total += item.elem2;
            }
            return total;
        }

    }

    public class ParAux<X, Y>
    {
        public X elem1 { get; set; }
        public Y elem2 { get; set; }
        public ParAux(X elem1,Y elem2)
        {
            this.elem1 = elem1;
            this.elem2 = elem2;
        }
    }

    public class VentasXServicio
    {
        public string nombre { get; set; }
        public decimal importe { get; set; }
        public int cantidad { get; set; }
        public VentasXServicio(string nombre, decimal importe, int cantidad)
        {
            this.nombre = nombre;
            this.importe = importe;
            this.cantidad = cantidad;
        }
    }

}
