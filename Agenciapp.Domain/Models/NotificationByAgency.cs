using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class NotificationByAgency
    {
        public NotificationByAgency()
        {
        }
        [Key]
        public Guid Id { get; set; }
        public Guid AgencyId { get; set; }
        public bool AirShippment { get; set; }
        public bool Combo { get; set; }
        public bool Reservation { get; set; }
        public bool Reclamo { get; set; }
        [ForeignKey("AgencyId")]
        public Agency Agency { get; set; }
    }
}
