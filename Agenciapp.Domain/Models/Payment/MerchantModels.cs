using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AgenciappHome.Models.Payment
{
    public class MerchantPurshaseRequest
    {
        public CardInfo CardInfo { get; set; }

        public decimal Amount { get; set; }
       
        public string OrderId { get; set; }

        public Guid UserId { get; set; }
      
        public string Email { get; set; }

        public string OrderDescription { get; set; }
        public string CustomerProfileId { get; set; }
        public string TransacationDescriptor { get; set; }
    }
}