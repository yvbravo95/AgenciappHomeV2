using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class Provincia
    {
        [Key]
        public Guid Id { get; set; }
        public string nombreProvincia { get; set; }
        public Zona Zona { get; set; }
        public PuntoEntrega PuntoEntrega { get; set; }
        public int Order { get; set; }
        public ICollection<Municipio> municipios { get; set; }
    }
}
