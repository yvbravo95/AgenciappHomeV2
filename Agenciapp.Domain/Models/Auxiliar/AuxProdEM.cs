using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Auxiliar
{
    public class AuxListProdEM
    {
        public List<AuxProdEm> listProdEM { get; set; }
    }

    public class AuxProdEm
    {
        public Product prod { get; set; }
        public ProductEM productEM { get; set; }

    }
}
