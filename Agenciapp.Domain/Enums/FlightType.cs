using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Agenciapp.Domain.Enums
{
    public enum FlightType
    {
        [Description("Ida")]oneWay = 1,
        [Description("Ida y Vuelta")]DueroundTrip_MultiCity = 2
    }
}
