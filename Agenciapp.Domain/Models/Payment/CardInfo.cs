

using FluentValidation;

namespace AgenciappHome.Models.Payment
{
    public class CardInfo
    {
        public string Type1 { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }
        
        public string Number { get; set; }
        public string Cvv { get; set; }
        public string Expiration { get; set; }
        public string BillingAddress { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        
        public string CountryIso2 { get; set; }
        public int ZipCode { get; set; }
        public string  BillingPhone { get; set; }

        public string Type { get; set; }
    }

    public class CardInfoValidartor : AbstractValidator<CardInfo>
    {
        public CardInfoValidartor()
        {
            RuleFor(x => x.City).NotEmpty().MaximumLength(64);
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(64);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(64);
            RuleFor(x => x.State).NotEmpty().MaximumLength(64);
            RuleFor(x => x.Type).NotEmpty().MaximumLength(32);
            RuleFor(x => x.BillingAddress).NotEmpty().MaximumLength(64);
            RuleFor(x => x.BillingPhone).NotEmpty().MaximumLength(32);
            RuleFor(x => x.ZipCode).NotNull();
            RuleFor(x => x.City).NotEmpty().MaximumLength(64);
            RuleFor(x => x.CountryIso2).NotEmpty().Length(2);
            RuleFor(x => x.Expiration).NotEmpty().Length(4);
            RuleFor(x => x.Number).NotEmpty().MaximumLength(32).MinimumLength(12);
            RuleFor(x => x.Cvv).NotEmpty().MinimumLength(3).MaximumLength(5);
        }
    }
}