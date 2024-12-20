using Agenciapp.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Agenciapp.Service.IPromoCodeServices.Models
{
    public class CreateCodeModel
    {
        [Required] public string Code { get; set; }
        [Required] public decimal Value { get; set; }
        [Required] public DateTime DateInit { get; set; }
        [Required] public DateTime DateEnd { get; set; }
        [Required] public OrderType OrderType { get; set; }
        [Required] public RateType PromoType { get; set; }
    }
}
