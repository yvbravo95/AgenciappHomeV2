using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Domain.Models
{
    public class ReporteCobro
    {
        protected ReporteCobro()
        {

        }
        public ReporteCobro(decimal salesDay, decimal initialValue, decimal charged, decimal chargedDiferido, decimal saldoReal, decimal reajuste, Agency agency, DateTime createdAt)
        {
            CreatedAt = createdAt;
            SalesDay = salesDay;
            InitialValue = initialValue;
            Charged = charged;
            ChargedDiferido = chargedDiferido;
            Total = initialValue + reajuste + (salesDay - charged);
            SaldoReal = saldoReal;
            Reajuste = reajuste;
            Agency = agency;
        }

        [Key] public Guid Id { get; set; }
        public DateTime CreatedAt { get; private set; }
        public decimal SalesDay { get; private set; } // Ventas totales del dia
        public decimal InitialValue { get; private set; } //Saldo inicial
        public decimal Charged { get; private set; } // Lo que se Cobro
        public decimal ChargedDiferido { get; private set; } // Lo que se Cobro diferido
        public decimal Total { get; private set; } // Resultado
        public decimal Reajuste { get; set; }
        public decimal SaldoReal { get; set; }
        public Agency Agency { get; private set; }
    }
}
