using Agenciapp.Common.Contrains;
using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Agenciapp.Service.IReportServices.Models
{
    public class UtilityModel
    {
        public STipo Service { get; set; }
        public string OrderNumber { get; set; }
        public ClientUtility Client { get; set; }
        public EmployeeUtility Employee { get; set; }
        public DateTime Date { get; set; }
        public decimal SalePrice { get; set; }
        public decimal CServicio { get; set; }

        private decimal _cost;
        public decimal Cost
        {
            get
            {
                if (!ByTransferencia && Agency != null && Agency.AgencyId == AgencyName.ReyEnvios)
                {
                    return _cost + FeeTarjeta;
                }
                else
                {
                    return _cost;
                }
            }
            set
            {
                _cost = value;
            }
        }
        public decimal _utility { get; set; }
        public decimal Utility { get 
            {
                if (!ByTransferencia && Agency != null && Agency.AgencyId == AgencyName.ReyEnvios)
                {
                    return _utility - FeeTarjeta;
                }
                else
                {
                    return _utility;
                }
            } 
            set { _utility = value; } }
        public decimal FeeTarjeta
        {
            get
            {
                if(Agency == null)
                {
                    return 0;
                }

                decimal amount = Pays.Where(x => x.TipoPago == "Crédito o Débito").Sum(x => x.PaidValue);
                decimal creditCardFee = Agency.creditCardFee;
                return Math.Round(amount - amount / (1 + creditCardFee / 100), 2);
            }
        }
        public bool ByTransferencia { get; set; }
        public string TransferredAgencyName { get; set; }
        public string TipoServicio { get; set; }
        public string SubServicio { get; set; }
        public bool IsCarrier { get; set; }
        public List<Pay> Pays { get; set; }
        public ServicioConsular ServicioConsular { get; set; }
        public string Minorista { get; set; }

        public Agency Agency { get; set; }

        public class ClientUtility
        {
            public Guid? ClientId { get; set; }
            public string FullName { get; set; }
            public string PhoneNumber { get; set; }
        }

        public class EmployeeUtility
        {
            public Guid? EmployeeId { get; set; }
            public string FullName { get; set; }

        }

        public class Pay
        {
            public Guid Id { get; set; }
            public Guid TipoPagoId { get; set; }
            public decimal PaidValue { get; set; }
            public string TipoPago { get; set; }
            public decimal Utility { get; set; }
            public decimal Costo { get; set; }
        }
    }

}