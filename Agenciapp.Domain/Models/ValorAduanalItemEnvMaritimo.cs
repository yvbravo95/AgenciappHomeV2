using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgenciappHome.Models
{
    public partial class ValorAduanalItemEnvMaritimo
    {
        [Key]
        public Guid ValorAduanalItemId { get; set; }
        public Guid ValorAduanalId { get; set; }
        public Guid EnvioId { get; set; }

        public EnvioMaritimo Envio { get; set; }
        public ValorAduanal ValorAduanal { get; set; }
    }
}
