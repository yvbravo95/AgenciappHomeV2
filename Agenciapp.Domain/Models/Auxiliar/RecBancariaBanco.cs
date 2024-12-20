using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Auxiliar
{
    public class RecBancariaBanco
    {
        public DateTime date { get; set; }
        public string description { get; set; }
        public decimal amount { get; set; }
        public string tipopago { get; set; }
        public string cliente { get; set; }
        public Object coincidencia { get; set; }

    }
}
