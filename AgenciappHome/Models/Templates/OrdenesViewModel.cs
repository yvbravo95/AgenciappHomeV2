using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Templates
{
    public class OrdenesViewModel
    {
        public Agency Agency { get; set; }
        public DateTime Date { get; set; }
        public string Number { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
    }
}
