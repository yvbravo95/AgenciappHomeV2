using Agenciapp.Domain.Models;
using Agenciapp.Domain.Models.ValueObject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AgenciappHome.Models
{
    public partial class Agency
    {
        public Agency()
        {
            Client = new HashSet<Client>();
            Office = new HashSet<Office>();
            Order = new HashSet<Order>();
            Product = new HashSet<Product>();
            Shipping = new HashSet<Shipping>();
            Wholesalers = new HashSet<Wholesaler>();
            AgencyPrecioRefMinoristas=new List<AgencyPrecioRefMinorista>();
            CuentasBancarias = new List<CuentaBancaria>();
            Code = new Random().Next(99999).ToString();
            BagCount = "0";
        }

        public Guid AgencyId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string LegalName { get; set; }
        public string PhoneNumber { get; set; }
        public Address Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public string logoName { get; set; }
        public string AgencyInfo { get; set; }
        public decimal creditCardFee { get; set; }
        public decimal creditCardFee_Combos { get; set; }
        public decimal creditCardFee_Pasaje { get; set; }
        public decimal remesa_entregaCuba { get; set; } // Costo de entrega en cuba para remesas
        public decimal precioFoto { get; set; } //Para pasaporte
        public ICollection<Client> Client { get; set; }
        public ICollection<Office> Office { get; set; }
        public ICollection<Order> Order { get; set; }
        public ICollection<Product> Product { get; set; }
        public ICollection<Shipping> Shipping { get; set; }
        public ICollection<Wholesaler> Wholesalers { get; set; }
        public List<AgencyPrecioRefMinorista> AgencyPrecioRefMinoristas { get; set; }
        public List<CuentaBancaria> CuentasBancarias { get; set; }
        public string Url => string.Concat(Name.Split(" ", StringSplitOptions.RemoveEmptyEntries)).ToLower();
        public EmailAddress EmailAddress { get; set; }
        public string UrlCalbackApiReyenvios { get; set; }
        public Phone Phone { get; set; }
        public string UserApiWholesaler { get; set; }
        public string PassApiWholesaler { get; set; }
        [DefaultValue("1")]public string BagCount { get; set; }
        public string AgencyNumber { get; set; }
        public string Code { get; set; }

    }

    public class ZelleEmailSetting
    {
        [Key]
        public Guid Id { get; set; }
        public Guid AgencyId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        [ForeignKey("AgencyId")]
        public Agency Agency{ get; set; }
    }
}
