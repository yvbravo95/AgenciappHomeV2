namespace AgenciappHome.Models.Payment
{
    public class NmiTransactionResponseModel
    {
        public TransactionResponseType TransactionResponse { get; set; }
        public string ResponseText { get; set; }
        public string AuthCode { get; set; }
        public string TransactionId { get; set; }
        public string ResponseCode { get; set; }
        public string AvsResponse { get; set; }
        public string CvvResponse { get; set; }
        public string Orderid { get; set; }

        public string CustomerVaultId { get; set; }
        



    }
    public enum TransactionResponseType
    {
        [Description("Transaction Approved")]
        TransactionApproved,

        [Description("Transaction Declined")]
        TransactionDeclined,

        [Description("Error in transaction data or system error ")]
        Error
    }
}