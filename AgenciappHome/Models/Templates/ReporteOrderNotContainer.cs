using System.Linq;

namespace AgenciappHome.Models.Templates
{
    public class ReporteOrderNotContainer
    {
        public IQueryable<OrderCubiq> Orders { get; set; }
        public GuiaAerea Guia { get; set; }
    }
}