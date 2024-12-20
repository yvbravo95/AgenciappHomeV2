using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Agenciapp.Domain.Enums
{
    public enum CubiqPackageType
    {
        [Description("")] None,
        [Description("Miscelaneo")]Miscelaneo,
        [Description("Medicina")] Medicina,
        [Description("Alimento")] Alimentos,
        [Description("Duradero")] Duradero
    }
}
