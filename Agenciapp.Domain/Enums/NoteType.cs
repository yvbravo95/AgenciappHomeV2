using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Agenciapp.Domain.Enums
{
    public enum NoteType
    {
        [Description("Nota de Crédito")] NC,
        [Description("Nota de Débito")] ND
    }
}
