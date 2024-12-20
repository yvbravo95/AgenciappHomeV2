using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class CuentaBancaria
    {
        public CuentaBancaria()
        {
            registroPagos = new List<RegistroPago>();
        }

        public Guid CuentaBancariaId { get; set; }
        public string NoCuenta { get; set; }
        public Guid AgencyId { get; set; }
        [ForeignKey("AgencyId")]
        public Agency Agency { get; set; }
        public List<RegistroPago> registroPagos { get; set; }
    }
}
