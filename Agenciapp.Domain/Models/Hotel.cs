using System;
using System.Collections.Generic;

namespace AgenciappHome.Models
{
    public partial class Hotel
    {
        public Guid Id { get; set; }

        public string Nombre { get; set; }
        public string Desc { get; set; }
        public int CantDias { get; set; }
        public Wholesaler Mayorista { get; set; }
        public Guid MayoristaId { get; set; }
        public decimal Precio { get; set; }
        public decimal Costo { get; set; }
        public Guid AgencyId { get; set; }
        public Agency Agency { get; set; }

        public bool Cancelado { get; set; }

        public string Info { get { return string.Format("{0}-{1}-${2}-{3} días", Nombre, Mayorista?.name, Precio, CantDias); } }
    }
}
