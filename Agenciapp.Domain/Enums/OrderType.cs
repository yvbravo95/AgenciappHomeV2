using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Agenciapp.Domain.Enums
{
    public enum OrderType
    {
        [Description("Combo")] Combo,
        [Description("Reserva Pasaje")] ReservaPasaje,
        [Description("Tienda")] Tienda,
        [Description("Pasaporte")] Pasaporte,
    }
}
