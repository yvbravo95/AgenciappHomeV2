using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgenciappHome.Models.Request
{
    public class CreateGuiaModel
    {
        [Required] public string Number { get; set; }
        [Required] public string Agente { get; set; }
        [Required] public string Type { get; set; }

        public List<Guid> palletsId { get; set; }
        public List<Guid> OrdersId { get; set; } // Para agencias que no usan pallets

        public string SMLU { get; set; }
        public string SEAL { get; set; }
        public string CAT { get; set; }
        public string Manifiesto { get; set; }
        public DateTime? FechaRecogida { get; set; }
        public DateTime? FechaViaje { get; set; }
    }
}