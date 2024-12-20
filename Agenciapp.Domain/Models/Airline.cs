using System;
using System.Collections.Generic;

namespace AgenciappHome.Models
{
    public partial class Airline
    {
        public Guid AirlineId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
