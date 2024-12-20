using System;
using System.Collections.Generic;

namespace AgenciappHome.Models
{
    public partial class AirlineFlights
    {
        public Guid AirlineFlightsId { get; set; }
        public Guid AirlineId { get; set; }
        public Guid FlightsId { get; set; }
    }
}
