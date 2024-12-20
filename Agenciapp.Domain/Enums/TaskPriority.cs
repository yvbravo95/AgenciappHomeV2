using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Agenciapp.Domain.Enums
{
    public enum TaskPriority
    {
        [Description("Alta")] Alta,
        [Description("Media")] Media,
        [Description("Baja")] Baja,

    }
}
