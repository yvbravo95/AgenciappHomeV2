using System;
using AgenciappHome.Models;

namespace Agenciapp.Domain.Models
{
    public class AccessGuiaAereaAgency
    {   
        protected AccessGuiaAereaAgency()
        {
            
        }
        public AccessGuiaAereaAgency(Agency agency, GuiaAerea guiaAerea)
        {
            Agency = agency;
            GuiaAerea = guiaAerea;
        }

        public Guid Id { get; set; }
        public Agency Agency { get; set; }
        public GuiaAerea GuiaAerea { get; set; }
    }
}