using Agenciapp.Domain.Enums;
using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Agenciapp.Domain.Models
{
    public class SettingRetailsStore
    {
        [Key]
        public Guid Id { get; set; }
        public Guid AgencyId { get; set; }
        public decimal FeeWholesaler { get; set; }
        public decimal FeeRetail { get; set; }
        public decimal ServiceCost { get; set; }
        public StoreType StoreType { get; set; }

        [ForeignKey("AgencyId")]
        public Agency Agency { get; set; }
    }
}
