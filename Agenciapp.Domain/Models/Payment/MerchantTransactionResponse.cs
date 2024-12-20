namespace AgenciappHome.Models.Payment
{
    public class MerchantTransactionResponse
    {
        public PaymentStatus PaymentStatus { get; set; }

        public string MerchantTransactionId { get; set; }

        public string AuthCode { get; set; }

        public string Token { get; set; }

        public string BusinessTransactionOrderId { get; set; }

        public string Message { get; set; }

        public string CardType { get; set; }
        public string  FullJsonResponse { get; set; }
    }
    public enum PaymentStatus
    {
        Initial,
        Completed,
        Authorized,
        Declined,
        Cancelled,
        Error
    }
}