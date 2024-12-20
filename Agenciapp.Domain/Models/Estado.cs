using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AgenciappHome.Models
{
    public class Estado
    {
        [Key]
        public Guid EstadoId { get; set; }
        public string Name { get; set; }
        public string Abb { get; set; }
        public List<Ciudad> Ciudades { get; set; }
    }

    public class Ciudad
    {
        [Key]
        public Guid CiudadId { get; set; }
        public string Name { get; set; }

        public Guid EstadoId { get; set; }

        public Estado Estado { get; set; }
    }
}
