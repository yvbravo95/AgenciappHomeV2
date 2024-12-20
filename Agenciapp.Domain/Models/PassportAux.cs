using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class PassportAux
    {
        [Key]
        public string Name { get; set; }
        public string RealName { get; set; }
    }
}
