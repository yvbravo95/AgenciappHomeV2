using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Response
{
    public class PassportDispatchReport
    {
        public string NoDispatch { get; set; }
        public string AgencyName { get; set; }
        public List<PassportDispatchItem> passportDispatchItems { get; set; }
    }

    public class PassportDispatchItem
    {
        public string OrderNumber { get; set; }
        public string DispatchNumber { get; set; }
        public string ClientName { get; set; }
        public string ServiceType { get; set; }
        public string PassportNumber { get; set; }
        public decimal Cost { get; set; }
    }
}
