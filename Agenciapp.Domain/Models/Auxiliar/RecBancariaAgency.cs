using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Auxiliar
{
    public class RecBancariaAgency
    {
        public Guid RecBancariaAgencyId { get; set; }
        public Guid IdTramite { get; set; }
        public DateTime date { get; set; }
        public string number { get; set; }
        public string tipopago { get; set; }
        public decimal monto { get; set; }
        public string details { get; set; }
        public string cliente { get; set; }
        public Guid cashAccountingBoxItemId { get; set; }
        [ForeignKey("cashAccountingBoxItemId")]
        public CashAccountingBoxItem cashAccountingBoxItem { get; set; }
        [NotMapped]
        public Object coincidencia { get; set; }
    }

}
