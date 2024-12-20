using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.ApiClient.FlightProviders.Models
{
    public class FlightDto
    {
        public string Price { get; set; }
        public string Type { get; set; }
        public FlightInfo DepartFlight { get; set; }
        public FlightInfo ReturnFlight { get; set; }

        public class FlightInfo
        {
            public string FlightNumber { get; set; }
            public string DepartureTime { get; set; }
            public string ArrivalTime { get; set; }
            public string Route { get; set; }
            public string EmptySeats { get; set; }
            public string Duration { get; set; }
        }
    }
}
