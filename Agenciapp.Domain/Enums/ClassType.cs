using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Agenciapp.Domain.Enums
{
    public enum ClassType
    {
        [Description("Economy")]Economica = 1,
        EconomicaPremium = 2,
        Ejecutiva = 3,
        Primera = 4
    }
}
