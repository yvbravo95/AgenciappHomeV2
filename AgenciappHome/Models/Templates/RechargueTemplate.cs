using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgenciappHome.Models
{
    public class RechargueTemplate
    {
        [Required(ErrorMessage = "El campo de Cliente es requerido.")]
        public Guid ClientId { get; set; }
        
        [Required(ErrorMessage = "El campo de Número Telefónico es requerido.")]
        [Display(Name = "Número Telefónico")]
        public string NumberPhone { get; set; }
        
        [Required(ErrorMessage = "El campo de Cantidad es requerido.")]
        [Display(Name = "Cantidad")]
        public decimal Count { get; set; }
        
        [Required(ErrorMessage = "El campo de Importe es requerido.")]
        [Display(Name = "Importe")]
        public decimal Import { get; set; }
        
        public decimal Balance { get; set; }

        public decimal costoMayorista { get; set; }
        public decimal valorPagado { get; set; } //Valor pagado
        public Guid tipoPago { get; set; }

        public string mayorista { get; set; }
        public string tiporecarga { get; set; }
        public decimal precioventa { get; set; }

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

        public List<RegistroPago> RegistroPagos { get; set; }

        public string Credito { get; set; }
        public string Nota { get; set; }

    }
}
