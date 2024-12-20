using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Domain.Models
{
    public class FlightCubaSearch
    {
        public FlightCubaSearch()
        {
            CreatedAt = DateTime.Now;
        }

        [Key]
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Type { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string DepartureDate { get; set; }
        public string ReturnDate { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public int Infants { get; set; }

        public string StringResult { get; set; }
    }
}
