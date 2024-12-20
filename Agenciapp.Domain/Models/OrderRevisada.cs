using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class OrderRevisada
    {
        public Guid Id { get; set; }
        public bool IsIncomplete { get; set; }
        public string Description { get; set; }
        public string firmaname { get; set; } // Para las remesas, al entregar una remesa
        public string imagename { get; set; } // Para las remesas, al entregar una remesa
    }
}
