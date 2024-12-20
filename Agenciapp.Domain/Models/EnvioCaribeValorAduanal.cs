using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class EnvioCaribeValorAduanal
    {
        [Key]
        public Guid EnvioCaribeValorAduanalId { get; set; }
        public Guid ValorAduanalId { get; set; }
        public Guid EnvioCaribeId { get; set; }

        public EnvioCaribe EnvioCaribe { get; set; }
        public ValorAduanal ValorAduanal { get; set; }
    }
}
