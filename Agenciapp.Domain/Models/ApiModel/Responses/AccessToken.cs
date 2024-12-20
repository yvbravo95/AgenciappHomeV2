using System;

namespace RapidMultiservice.Models.Responses
{
    public class AccessToken
    {
        public string Token { get; set; }
        public DateTime ExpirationUtc { get; set; }
        public string Name { get; set; }
    }
}