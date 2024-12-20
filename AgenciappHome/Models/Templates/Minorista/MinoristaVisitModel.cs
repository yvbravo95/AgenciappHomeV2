using System;

namespace AgenciappHome.Models.Templates.Minorista
{
    public class MinoristaVisitModel
    {
        public Guid AgencyId { get; set; }
        public string AgencyName { get; set; }
        public string Phone { get; set; }
        public bool Visible { get; set; }
    }
}
