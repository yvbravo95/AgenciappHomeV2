using System;
using System.ComponentModel.DataAnnotations;
using AgenciappHome.Models.ApiModel;

namespace AgenciappHome.Models
{
    public class PaymentCard
    {
        [Key] public Guid Id { get; set; }

        [MaxLength(4)] public string LastFour { get; set; }

        [MaxLength(32)] public string Type { get; set; }

        [MaxLength(64)] public string FirstName { get; set; }

        [MaxLength(64)] public string LastName { get; set; }

        [MaxLength(8)] public string Expiration { get; set; }
        [MaxLength(64)] public string BillingAddress { get; set; }
        [MaxLength(64)] public string State { get; set; }
        [MaxLength(64)] public string City { get; set; }

        [MaxLength(2)] public string CountryIso2 { get; set; }
        [MaxLength(8)] public int ZipCode { get; set; }
        [MaxLength(32)] public string BillingPhone { get; set; }
        [MaxLength(64)] public string Token { get; set; }

        
        public Guid? UserClientId { get; set; }
        public UserClient UserClient { get; set; }
        
    }
}