using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Agenciapp.Domain.Enums
{
    public enum Scales
    {
        [Description("Cualquier Cantidad")]CualquierCantidad,
        [Description("Sin Escalas")]SinEscalas,
        [Description("1 escala")]OneScale,
        [Description("2 escala")]TwoScale,
    }
}
