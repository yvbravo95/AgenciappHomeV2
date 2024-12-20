using System;
using System.Collections.Generic;

namespace AgenciappHome.Models
{
    public partial class ValorAduanal
    {
        public ValorAduanal()
        {
            ValorAduanalItem = new HashSet<ValorAduanalItem>();
        }

        public Guid ValorAduanalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Um { get; set; }
        public decimal Value { get; set; }
        public string Article { get; set; }
        public string Observaciones { get; set; }

        public ICollection<ValorAduanalItem> ValorAduanalItem { get; set; }
        public ICollection<EnvioCaribeValorAduanal> EnvioCaribeValorAduanals { get; set; }
    }
}
