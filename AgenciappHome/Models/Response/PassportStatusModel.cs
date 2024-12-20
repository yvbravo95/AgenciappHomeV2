using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.Response
{
    public class PassportStatusModel
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public string Status { get; set; }
    }
}
