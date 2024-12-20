using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class RegistroEnvioEmails
    {
        public Guid RegistroEnvioEmailsId { get; set; }
        public Guid AgencyId { get; set; }
        public string AgencyName { get; set; }
        public DateTime fecha { get; set; }
        public string destinatario { get; set; }
        public string descripción { get; set; }
        public string status { get; set; }
    }
}
