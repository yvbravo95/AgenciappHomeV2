using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class Zona
    {
        [Key]
        public Guid ZonaId { get; set; }
        public string Name { get; set; }
        public List<Provincia> Provincias { get; set; }

        public decimal Precio { get; set; }
    }
}
