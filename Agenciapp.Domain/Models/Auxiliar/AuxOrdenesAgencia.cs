using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AgenciappHome.Models.Auxiliar
{
    public class AuxOrdenesAgencia
    {
        public Agency agency { get; set; }
        public int count { get; set; }
    }
}
