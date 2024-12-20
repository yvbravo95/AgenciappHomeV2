using System;

namespace AgenciappHome.Models.PaymentCards
{
    public class PaymentCardListItem
    {
        public Guid Id { get; set; }

        public string LastFour { get; set; }

        public string Type { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Expiration { get; set; }
    }
    
    
}