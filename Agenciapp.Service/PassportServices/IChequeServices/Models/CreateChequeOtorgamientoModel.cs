using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.PassportServices.IChequeServices.Models
{
    public class CreateChequeOtorgamientoModel
    {
        public Guid ManifiestoId { get; set; }
        public Guid PassportId { get; set; }
        public int Number { get; set; }
    }
}
