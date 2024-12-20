namespace AgenciappHome.Models.Payment
{
    public class MerchantCancelTransactionRequest
    {
        public VoidReason? VoidReason { get; set; }
        public string ExternalMerchantTransactionId { get; set; }

        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string OrderDescription { get; set; }

    }
}