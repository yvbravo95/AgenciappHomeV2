using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Request
{
    public class MinoristaProductoBodega
    {
        public List<Guid> ids { get; set; }
        public List<Guid> agenciasprecio1 { get; set; }
        public List<Guid> agenciasprecio2 { get; set; }
        public List<Guid> agenciasprecio3 { get; set; }
    }
}
