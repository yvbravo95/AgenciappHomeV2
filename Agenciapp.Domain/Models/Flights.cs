using System;
using System.Collections.Generic;

namespace AgenciappHome.Models
{
    public partial class Flights
    {
        public Guid FlightsId { get; set; }
        public string CityOut { get; set; }
        public string CityIn { get; set; }
    }
}
