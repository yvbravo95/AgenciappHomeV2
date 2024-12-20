using System.ComponentModel;

namespace Agenciapp.Domain.Enums
{
    public enum MoneyType
    {
        USD,
        CUP,
        [Description("MLC")] USD_TARJETA
    }
}