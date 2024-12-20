using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class AuthorizationCard
    {
        [Key]
        public Guid Id { get; set; }
        public string typeCard { get; set; }
        public string CardCreditEnding { get; set; }
        public string CCV { get; set; }
        public DateTime ExpDate { get; set; }
        public DateTime Date { get; set; }
        public string addressOfSend { get; set; } // Direccion donde se va a enviar
        public string OwnerAddressDiferent { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string saleAmount { get; set; }
        public string ConvCharge { get; set; }
        public string TotalCharge { get; set; }
        
    }
}
