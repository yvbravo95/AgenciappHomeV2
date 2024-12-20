using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class AuxVentasPorEmpleado
    {
        public List<ParAux<ParAux<Guid,string>, decimal>> servicios = new List<ParAux<ParAux<Guid, string>, decimal>>();
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

}
