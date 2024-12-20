using Agenciapp.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.ApiModel
{
    public class Invoice
    {
        public static String STATUS_ENPROGESO = "En Progreso";
        public static String STATUS_PROCESADA = "Procesada";
        public static String STATUS_INICIADA = "Iniciada";
        public static String STATUS_PENDIENTE = "Pendiente";
        public static String STATUS_ENTREGADA = "Entregada";

        public Invoice()
        {
            Orders = new List<Order>();
        }

        [Key]
        public Guid InvoiceId { get; set; }
        public string Number { get; set; }
        public DateTime DateTime { get; set; }
        public string Status { get; set; }
        public string TransactionId { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Charges { get; set; }
        public Guid ContactId { get; set; }
        [ForeignKey("ContactId")]
        public Contact Contact { get; set; }
        public List<Order> Orders { get; set; }
        public Guid? UserClientId { get; set; }
        [ForeignKey("UserClientId")]
        public UserClient UserClient { get; set; }
        public Guid AgencyId { get; set; }
        [ForeignKey("AgencyId")]
        public Agency Agency { get; set; }
        public ICollection<InvoiceProductoBodega> InvoiceProductoBodega { get; set; }
        public Discount Discount { get; set; }
        public PaymentType PaymentType { get; set; }
        public string ZelleNumber { get; set; }
        public string CreditCardLastFour { get; set; }
    }

    public class InvoiceProductoBodega
    {
        [Key, Column(Order = 1)]
        public Guid InvoiceId { get; set; }
        [Key, Column(Order = 2)]
        public Guid ProductoBodegaId { get; set; }
        public Invoice Invoice { get; set; }
        public ProductoBodega ProductoBodega { get; set; }
        public decimal Amount { get; set; }
        
    }

    public enum PaymentType
    {
        Card,
        Zelle,
        None
    }
}
