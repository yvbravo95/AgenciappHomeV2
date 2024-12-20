using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Response
{
    public class PrintBill
    {
        public Bill Bill { get; set; }
        public string Empleado { get; set; }
        public string AgencyName { get; set; }
        public string AgencyAddress { get; set; }
        public string AgencyPhone { get; set; }
        public string AgencyLogo { get; set; }
    }
}
