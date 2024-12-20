using System;
using System.Collections.Generic;
using System.Text;

namespace Agenciapp.Service.PassportServices.IChequeServices.Models
{
    public class CreateChequeModel
    {
        public Guid ManifiestoId { get; set; }
        public int Number1 { get; set; }
        public int Number2 { get; set; }
    }
}
