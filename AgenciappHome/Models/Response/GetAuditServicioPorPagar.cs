using System;

namespace AgenciappHome.Models.Response
{
    public class GetAuditServiceByPay
    {
        public Guid ServiceId { get; set; }
        public DateTime Date { get; set; }
        public string ServiceNumber { get; set; }
        public decimal CostService { get; set; }
        public decimal CostServiceByPay { get; set; }
        public STipo Type { get; set; }
    }
}