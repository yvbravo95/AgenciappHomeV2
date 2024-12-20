using System;
using System.Collections.Generic;

namespace AgenciappHome.Models
{
    public partial class ValorAduanalItem
    {
        public Guid ValorAduanalItemId { get; set; }
        public Guid? ValorAduanalId { get; set; }
        public Guid? AduanaId { get; set; }
        public Guid? OrderId { get; set; }

        public Order Order { get; set; }
        public ValorAduanal ValorAduanal { get; set; }
        public Aduana Aduana { get; set; }
    }
}
