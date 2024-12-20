using System;
using System.Collections.Generic;

namespace AgenciappHome.Models
{
    
    public class Pais
    {
        public Pais()
        {
        }

        public Guid PaisId { get; set; }
        public string Nombre { get; set; }
        public string Nacinalidad { get; set; }
        public string Codigo { get; set; }
        public int Orden { get; set; }

    }
}
