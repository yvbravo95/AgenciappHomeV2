using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Templates
{
    public class ZelleTemplate
    {
        public Guid ZellItemId { get; set; }
        public Guid AgencyId { get; set; }
        public Guid? ReferenceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Code { get; set; }
        public string Client { get; set; }
        public string OrderNumber { get; set; }
        public string Nota { get; set; }
        public STipo Type { get; set; }
    }
}
