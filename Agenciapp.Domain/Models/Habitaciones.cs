using System;
using System.ComponentModel.DataAnnotations;

namespace AgenciappHome.Models
{
    public class Habitaciones
    {
        public Guid Id { get; set; }
        public TipoHabitacion Tipo { get; set; }
        public string TipoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class TipoHabitacion
    {
        [Key]
        public string Tipo { get; set; }
        public int Order { get; set; }
    }
}