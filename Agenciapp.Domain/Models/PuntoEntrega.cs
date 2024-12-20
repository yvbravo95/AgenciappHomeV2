using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace AgenciappHome.Models
{
    public class PuntoEntrega
    {
        public Guid PuntoEntregaId { get; set; }
        public string Direccion { get; set; }

    }
}