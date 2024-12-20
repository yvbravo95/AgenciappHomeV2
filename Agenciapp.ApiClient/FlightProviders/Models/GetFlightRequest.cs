using Agenciapp.ApiClient.FlightProviders.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.ApiClient.FlightProviders.Models
{
    public class GetFlightRequest
    {
        public SearchType Type { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string DepartureDate { get; set; }
        public string ReturnDate { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public int Infants { get; set; }
    }
}
