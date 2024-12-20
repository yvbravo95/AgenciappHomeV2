using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Agenciapp.Domain.Models
{
    public class MarketingReceiptCampaing
    {
        public Guid Id { get; set; }
        public Guid MarketingId { get; set; }

        public DateTime CreatedAt { get; set; }
        public int TotalSend { get; set; }
        public int SuccessSend { get; set; }
        public int FailSend { get; set; }

        public bool IsMms { get; set; }

        public decimal Amount { get; set; }

        public string Message { get; set; }
        public string PaymentReference { get; set; }

        [ForeignKey("MarketingId")]
        public Marketing Marketing { get; set; }
    }
}
