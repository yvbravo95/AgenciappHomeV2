using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Agenciapp.Domain.Models
{
    public class ApiPassportSetting
    {
        [Key] public Guid Id { get; set; }
        public string Token { get; set; }
        public string Description { get; set; }
        public string UrlCallback { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid? WholesalerId { get; set; }
        public string ApiLoginId { get; set; }
        public string TransactionKey { get; set; }
        public string Stripe_SecretKey { get; set; }
        public bool TestMerchant { get; set; }
        public decimal ServiceCost { get; set; }
        public decimal CreditCardFee { get; set; }
        public string ZelleEmail { get; set; }
        public Guid AgencyId { get; set; }
        public string AgencyInfo { get; set; }
        public PhoneSupport PhoneSupport { get; set; }

    }

    [Owned]
    public class PhoneSupport
    {
        public string Phone { get; set; }
        public string TextDefault { get; set; }
    }
}
