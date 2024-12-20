namespace AgenciappHome.Models.Payment
{
    public class PaymentNmiRequest
    {
        public NmiTrasactionTypes Type { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string SecurityKey { get; set; }
        

        public string Descriptor { get; set; }
        
        public string Ccnumber { get; set; }

        public string Ccexp { get; set; }

        public string Cvv { get; set; }

     
        public string Amount { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }

        public string OrderDescription { get; set; }
        
        //public string Currency { get; set; }

        public string Orderid { get; set; }
        //public string Ipaddress { get; set; }

        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        //public string Address1 { get; set; }

        //public string City { get; set; }
        //public string State { get; set; }

        //public string Zip { get; set; }

        public CustomerVault CustomerVault { get; set; }

        public string CustomerVaultId { get; set; }

        

    }
    
    public class ModifyNmiRequest
    {
        public NmiTrasactionTypes Type { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string SecurityKey { get; set; }
        public string Transactionid { get; set; }

        public string Amount { get; set; }

        public string OrderId { get; set; }

    }
    
    public enum CustomerVault
    {
        [Description("add_customer")]
        AddCustomer,

        [Description("update_customer")]
        UpdateCustomer
    }
    public enum NmiTrasactionTypes
    {
        [Description("sale")] Sale,

        [Description("auth")] Authorization,

        [Description("capture")] Capture,

        [Description("void")] voidT,

        [Description("refund")] Refund,

        [Description("credit")] Credit,

        [Description("validate")] Validate,

        [Description("update")] Update
    }
    
    
}