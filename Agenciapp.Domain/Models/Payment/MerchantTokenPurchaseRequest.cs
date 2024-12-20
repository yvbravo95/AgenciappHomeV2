namespace AgenciappHome.Models.Payment
{
    public class MerchantTokenPurchaseRequest
    {
       public decimal Amount { get; set; }
        public string PaymentToken { get; set; }
        public string OrderId { get; set; }

        
        public string Email { get; set; }

        public string OrderDescription { get; set; }
        public string TransacationDescriptor { get; set; }
    }
}