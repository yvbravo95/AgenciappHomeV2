namespace AgenciappHome.Models.Payment
{
    public class MerchantModifyTransactionRequest
    {
        public CardInfo CardInfo { get; set; }
        public string ExternalMerchantTransactionId { get; set; }

        public decimal Amount { get; set; }
       
        public string OrderId { get; set; }
        public string OrderDescription { get; set; }
    }
    public class MerchantRefundTransactionRequest
    {
        public string ExternalMerchantTransactionId { get; set; }

        public string CardLastFour { get; set; }
        public decimal Amount { get; set; }
       
        public string OrderId { get; set; }
        public string OrderDescription { get; set; }
    }
}