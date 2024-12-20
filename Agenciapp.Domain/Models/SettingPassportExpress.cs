using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models
{
    public class SettingPassportExpress
    {
        [Key]
        public Guid SettingPassportExpressId { get; set; }
        public Guid AgencyId { get; set; }
        public decimal Costo { get; set; }
        public decimal Price { get; set; }

        public string ServicioConsular { get; set; }
        [ForeignKey("AgencyId")]
        public Agency Agency { get; set; }
    }
}
