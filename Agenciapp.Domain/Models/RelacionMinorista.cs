using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AgenciappHome.Models.ApiModel;

namespace AgenciappHome.Models
{
    public class RelacionMinorista
    {
        public Guid Id { get; set; }
        public Guid AgencyId { get; set; }
        public Guid MinoristaId { get; set; }
        public int Tipo { get; set; }
        public STipo Modulo { get; set; }
    }
}
