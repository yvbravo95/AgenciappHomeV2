using Agenciapp.Domain.Enums;
using AgenciappHome.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Agenciapp.Domain.Models
{
    public class PromoCode
    {
        protected PromoCode()
        {

        }
        public PromoCode(string code, decimal value, DateTime dateInit, DateTime dateEnd, OrderType orderType, RateType promoType, Agency agency, User user)
        {
            Code = code;
            CreatedAt = DateTime.UtcNow;
            DateInit = dateInit;
            DateEnd = dateEnd;
            OrderType = orderType;
            PromoType = promoType;
            Agency = agency;
            CreatedBy = user;
            Value = value;
            isActive = true;
        }

        public void Update(string code, decimal value, DateTime dateInit, DateTime dateEnd, OrderType orderType, RateType promoType)
        {
            Code = code;
            DateInit = dateInit;
            DateEnd = dateEnd;
            OrderType = orderType;
            PromoType = promoType;
            Value = value;
        }

        public void ChangeStatus(bool status)
        {
            isActive = status;
        }

        public decimal ApplyDiscount(decimal value)
        {
            if (!isActive)
                return value;

            if (PromoType == RateType.Fijo)
                return value - Value;
            else if(PromoType == RateType.Porciento)
                return value - (value * (Value / 100));
            else
            {
                return value;
            }
        }

        public decimal GetDiscount(decimal value)
        {
            if (!isActive)
                return 0;

            if (PromoType == RateType.Fijo)
                return Value;
            else if (PromoType == RateType.Porciento)
                return value * (Value / 100);
            else
            {
                return 0;
            }
        }

        [Key]public Guid Id { get; private set; }
        public string Code { get; private set; }
        public decimal Value { get; private set; }
        public bool isActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime DateInit { get; private set; }
        public DateTime DateEnd { get; private set; }
        public OrderType OrderType { get; private set; }
        public RateType PromoType { get; private set; }
        public Agency Agency { get; private set; }
        public User CreatedBy { get; private set; }
    }
}
