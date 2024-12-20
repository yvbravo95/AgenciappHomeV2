using System;

namespace Agenciapp.Domain.Models
{
    public class RegistroCobro
    {
        public Guid Id { get; set;}
        public string Action { get; set;}
        public string OrderNumber { get; set;}
        public decimal Value { get; set;}
        public DateTime Date { get; set;}
        public Guid? MinoristaAgency { get; set;}
        public Guid? MinoristaClient { get; set;}
        public DateTime? DateCobro { get; set;} 
        public decimal OldValue { get; set;}
        public Guid Mayorista { get; set;}
    }
}