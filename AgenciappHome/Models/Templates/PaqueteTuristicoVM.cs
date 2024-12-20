using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgenciappHome.Models
{
    public class PaqueteTuristicoVM
    {
        [Required(ErrorMessage = "El campo de Cliente es requerido.")]
        public Guid ClientId { get; set; }
        public Client Client { get; set; }

        public Guid PaqueteId { get; set; }

        public string Number { get; set; }

        public DateTime Date { get; set; }

        public string Status { get; set; }

        public decimal Costo { get; set; }
        public decimal Amount { get; set; }
        public decimal Precio { get; set; }
        public decimal ValorPagado { get; set; }
        public decimal Balance { get; set; }
        public decimal OtrosCostos { get; set; }

        public string Nota { get; set; }

        public decimal credito { get; set; }

        public List<RegistroPago> Pagos { get; set; }

        public List<Ticket> Tickets { get; set; }
        public List<Servicio> Servicios { get; set; } 

        public string AuthTypeCard { get; set; }
        public string AuthCardCreditEnding { get; set; }
        public DateTime AuthExpDate { get; set; }
        public string AuthCCV { get; set; }
        public string AuthaddressOfSend { get; set; }
        public string AuthOwnerAddressDiferent { get; set; }
        public string Authemail { get; set; }
        public string Authphone { get; set; }
        public string AuthConvCharge { get; set; }
        public string TotalCharge { get; set; }
        public string AuthSaleAmount { get; set; }

    }
}
