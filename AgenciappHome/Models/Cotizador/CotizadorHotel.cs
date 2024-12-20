using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class CotizadorHotel
    {
        public Guid Id { get; set; }

        public List<CHotel> hotels { get; set; }

        public Guid ClientId { get; set; }

        public DateTime CreateAt { get; set; }
        public string Desde { get; set; }
        public string Hasta { get; set; }

        public string nombreAgente { get; set; }
        public string contacto { get; set; }
    }

    public class CHotel
    {
        public string name { get; set; }
        public string ubicacion { get; set; }
        public decimal precioAvionAdulto { get; set; }
        public decimal precioAvionMenor { get; set; }
        public decimal precioAvionInfante { get; set; }
        public decimal precioAvionTotal { get; set; }
        public decimal precioAuto { get; set; }
        public decimal precioTotal { get; set; }
        public decimal depositoInicial { get; set; }
        public List<CHabitacion> habitaciones { get; set; }

        public string link { get; set; }
    }

    public class CHabitacion
    {
        public string tipo { get; set; }
        public int cantidadAdultos { get; set; }
        public int cantidadInfantes { get; set; }
        public int cantidadMenores { get; set; }
        public decimal precioAdulto { get; set; }
        public decimal[] precioMenores { get; set; }
        public decimal precioInfantes { get; set; }
        public decimal precioTotal { get; set; }
        public string comentarios { get; set; }
    }
}
