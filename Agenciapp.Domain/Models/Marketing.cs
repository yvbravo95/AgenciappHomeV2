using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Agenciapp.Domain.Models
{
    public class Marketing
    {
        [Key]
        public Guid Id { get; set; }
        public Guid AgencyId { get; set; }
        public string StripeCustomerId { get; set; }
        public string StripeSecretkey { get; set; }
        public string NumberFrom { get; set; }
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
        public string PriceMms { get; set; }
        public string PriceSms { get; set; }

        [ForeignKey("AgencyId")]
        public Agency Agency { get; set; }


    }
}
