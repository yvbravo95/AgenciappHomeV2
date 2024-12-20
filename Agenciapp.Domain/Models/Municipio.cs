using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class Municipio
    {
        public Guid Id { get; set; }
        public string nombreMunicipio { get; set; }
        public Provincia provincia { get; set; }

        public string ZonaCubiq { get; set; }
    }
}
