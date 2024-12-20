using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgenciappHome.Models
{
    public class PrecioPasaporte
    {
        public Guid Id { get; set; }
        public ServicioConsular ServicioConsular { get; set; }
        public decimal Precio { get; set; }
    }
}
