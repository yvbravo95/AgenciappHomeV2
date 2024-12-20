using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Domain.Models.DBViewModels
{
    public class HMIncompleteOrdersReceived
    {
        [Key]
        public string Number { get; set; }
        public string Status { get; set; }
        public int CantidadEnviada { get; set; }
        public int CantidadRecibida { get; set; }
    }
}
