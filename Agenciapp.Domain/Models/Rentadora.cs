using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AgenciappHome.Models
{
    public class Rentadora
    {
        public Guid RentadoraId { get; set; }
        public List<AuxReserva> Autos { get; set; }
        public Guid AgencyId { get; set; }
        public string Nombre { get; set; }

        public bool Visible { get; set; }
        public Guid WholesalerId { get; set; }
        public Wholesaler Wholesaler { get; set; }
    }
}