using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.ApiClient.FlightProviders.Models
{
    public class ScrapeResponse
    {
        public IEnumerable<FlightDto> Flights { get; set; }
        public string Error { get; set; }
    }
}
