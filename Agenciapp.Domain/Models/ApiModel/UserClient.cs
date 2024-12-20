using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgenciappHome.Models.ApiModel
{
    // Para la autenticacion de los usuarios en la api
    public class UserClient
    {
        public UserClient()
        {
            FcmTokens = new List<FcmToken>();
        }
        
        [Key]
        public Guid UserClientId { get; set; }
        public string Username { get; set; } //número de telefono
        public bool PhoneConfirmed { get; set; }
        public string newPhone { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string SecureCode { get; set; }
        public DateTime? ExpiresSecureCode { get; set; }
        public string Status { get; set; }
        public Guid AgencyId { get; set; }
        public DateTime Timestamp { get; set; }
        public AppType AppType { get; set; }
        public Client Client { get; set; }
        public ICollection<PaymentCard> PaymentCards { get; set; }

        public List<Invoice> Invoices { get; set; }
        public List<FcmToken> FcmTokens { get; set; }
        public string Stripe_CustomerId { get; set; }
    }

    public class FcmToken
    {
        public Guid Id { get; set; }
        public string Token { get; set; }
        public TokenType TokenType { get; set; }
        public UserClient UserClient { get; set; }
    }

    public enum TokenType
    {
        Combo,
        Passport
    }

    public enum AppType
    {
        RapidM,
        Passport
    }
}
