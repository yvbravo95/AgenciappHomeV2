using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Agenciapp.Domain.Enums
{
    public enum TaskStatus
    {
        [Description("No Iniciado")]NoIniciado,
        [Description("En Curso")] EnCurso,
        [Description("Completado")] Completado,
        [Description("Cancelado")] Cancelado,

    }
}
