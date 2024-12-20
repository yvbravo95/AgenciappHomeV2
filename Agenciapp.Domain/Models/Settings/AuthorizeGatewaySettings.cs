namespace RapidMultiservice.Models.Settings
{
    public interface IAuthorizeGatewaySettings
    {
        string ApiLoginId { get; set; }
        string TransactionKey { get; set; }
    }

    public class AuthorizeGatewaySettings : IAuthorizeGatewaySettings
    {
        public string ApiLoginId { get; set; }
        public string TransactionKey { get; set; }
      
    }
}