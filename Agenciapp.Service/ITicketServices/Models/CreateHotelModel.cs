using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Agenciapp.Service
{
    public class CreateHotelModel
    {
        public string Number { get; set; }
        public string ReferenceNumber { get; set; }
        public string Description { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public int Infants { get; set; }
        public int Rooms { get; set; }
        public decimal Charges { get; set; }
        public decimal Commission { get; set; }
        public decimal Cost { get; set; }
        public decimal Discount { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public DateTime DateIn { get; set; }
        public DateTime DateOut { get; set; }
        public Guid ClientId { get; set; }
        public Guid? WholesalerId { get; set; }
        public Guid? PaqueteId { get; set; }
        public Guid OfficeId { get; set; }
        public Guid UserId { get; set; }
        public List<PayTicket> Pays { get; set; }
    }
}
