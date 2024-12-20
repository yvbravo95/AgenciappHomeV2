using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Agenciapp.Domain.Enums;

namespace AgenciappHome.Models
{
    public class RemesaTemplate
    {
        [Required(ErrorMessage = "El campo de Cliente es requerido.")]
        public Guid ClientId { get; set; }
        
        [Required(ErrorMessage = "El campo de Contacto es requerido.")]
        public Guid ContactId { get; set; }
        
        [Required(ErrorMessage = "El campo de Monto es requerido.")]
        [Display(Name = "Número Telefónico")]
        public decimal Monto { get; set; }
        
        [Required(ErrorMessage = "El campo de Porciento de Cobro es requerido.")]
        [Display(Name = "Porciento de Cobro")]
        public decimal Cobro { get; set; }

        public Guid tipoPago { get; set; }

        public Guid wholesaler { get; set; }

        public string tarifa { get; set; }

        public Guid RemesaId { get; set; }

        public decimal Amount { get; set; }
        public decimal ExchangeRate { get; set; }

        public decimal ValorPagado { get; set; }
        public decimal AddPrecio { get; set; }

        public List<RegistroPago> Pagos { get; set; }

        public CardRemittanceTemplate Card { get; set; }
        public string isRemittanceCard { get; set; }
        public string CardNumber { get; set; }

        public AuthorizationCard AuthorizationCard { get; set; }

        public decimal Balance { get; set; }
        public decimal credito { get; set; }
        public MoneyType MoneyType { get; set; }
    }

    public class CardRemittanceTemplate
    {
        public string SenderName { get; set; }
        public string SenderSurName { get; set; }
        public string SenderSecondSurName { get; set; }
        public string SenderDocumentType { get; set; }
        public string SenderNumberIdentityDocument { get; set; }
        public DateTime SenderExpirationDateDocument { get; set; }
        public string SenderCountryBirth { get; set; }
        public DateTime SenderDateBirth { get; set; }
        public decimal SenderAmountRechargueCard { get; set; }
        public string SenderAddressEEUU { get; set; }
        public string SenderPhoneEEUU { get; set; }
        public string RecipientName { get; set; }
        public string RecipientSurname { get; set; }
        public string RecipientSecondSurname { get; set; }
        public string RecipientNumberCI { get; set; }
        public string RecipientCardType { get; set; }
        public string RecipientCardNumber { get; set; }
        public string RecipientAddressCountry { get; set; }
        public string RecipientCity { get; set; }
        public string RecipientProvince { get; set; }
        public string RecipientPhone { get; set; }
        public string NotaTarjeta { get; set; }
    }
}
