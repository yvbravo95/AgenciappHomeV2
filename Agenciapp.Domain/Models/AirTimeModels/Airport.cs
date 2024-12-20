using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Domain.Models.AirTimeModels
{
    public class Airport
    {
        public Guid Id { get; set; }
        public string iata_airport_code { get; set; }
        public string icao_airport_code { get; set; }
        public string name { get; set; }
        public string city { get; set; }
        public string iata_country_code { get; set; }
        public string country { get; set; }
        public string longitude { get; set; }
        public string latitude { get; set; }
        public string altitude { get; set; }
        public string time_zone { get; set; }
        public string airport_status { get; set; }

    }
}
