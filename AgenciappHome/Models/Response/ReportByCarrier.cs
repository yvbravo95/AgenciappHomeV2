using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Response
{
    public class ReportByCarrier
    {
        public string AgencyName { get; set; }
        public string AgencyAddress { get; set; }
        public string AgencyPhone { get; set; }
        public string UrlLogoAgency { get; set; }
        public List<Shipping> Shippings { get; set; }
    }
}
