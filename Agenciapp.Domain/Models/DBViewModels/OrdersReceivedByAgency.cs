using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Domain.Models.DBViewModels
{
    public class OrdersReceivedByAgency
    {
        [Key]
        public Guid OrderId { get; set; }
        public Guid PrincipalDistributorId { get; set; }
        public string Estado { get; set; }
        public DateTime Date { get; set; }
        public string AgencyName { get; set; }
        public string AgencyTransferredName { get; set; }
    }
}
