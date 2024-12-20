using Agenciapp.Service.Merchants.Stripe.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Stripe;
using System.Threading.Tasks;

namespace Agenciapp.Service.Merchants.Stripe
{
    public interface IStripeService
    {
        Task<Result<Customer>> RegisterCustomer(RegisterCustomerModel model);
        Task<Result<Customer>> GetCustomer(string tokenId);
        Task<Result<Customer>> UpdateCustomer(UpdateCustomerModel model);
        Task<Result<Card>> RegisterPaymentCard(RegisterPaymentCard model);
        Task<Result<Card>> GetPaymentCard(string cardId, string customerId);
        Task<Result<Card>> UpdatePaymentCard(UpdatePaymentCard model);
        Task<Result<Card>> RemovePaymentCard(string cardId, string customerId);
        Task<Result<StripeList<Card>>> ListPaymentsCard(string customerId);
        Task<Result<Charge>> CreatePayment(StripePayment model);
        Task<Result<StripeList<PaymentMethod>>> ListPaymentMethods(string customerId);
        Task<Result<PaymentIntent>> CreatePaymentIntent(StripePayment model);
    }

    public class StripeService : IStripeService
    {
        public StripeService(string secretKey)
        {
            StripeConfiguration.ApiKey = secretKey;
        }

        public async Task<Result<Customer>> RegisterCustomer(RegisterCustomerModel model)
        {
            var options = new CustomerCreateOptions
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Address = new AddressOptions
                {
                    City = model.City,
                    Country = model.Country,
                    PostalCode = model.PostalCode,
                    State = model.State,
                    Line1 = model.Line1,
                    Line2 = model.Line2
                }
            };
            var service = new CustomerService();
            Customer customer = await service.CreateAsync(options);
            Serilog.Log.Information($"Customer: \n {JsonConvert.SerializeObject(customer)}");

            return Result.Success(customer);
        }

        public async Task<Result<Customer>> GetCustomer(string tokenId)
        {
            var service = new CustomerService();
            return Result.Success(await service.GetAsync(tokenId));
        }

        public async Task<Result<Customer>> UpdateCustomer(UpdateCustomerModel model)
        {
            var options = new CustomerUpdateOptions
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Address = new AddressOptions
                {
                    City = model.City,
                    Country = model.Country,
                    Line1 = model.Line1,
                    Line2 = model.Line2,
                    PostalCode = model.PostalCode,
                    State = model.State
                }
            };
            var service = new CustomerService();
            Customer customer = await service.UpdateAsync(model.TokenId, options);
            return Result.Success(customer);
        }

        public async Task<Result<Card>> RegisterPaymentCard(RegisterPaymentCard model)
        {
            var tokenOption = new TokenCreateOptions
            {
                Card = new TokenCardOptions
                {
                    Number = model.CardNumber,
                    ExpMonth = model.ExpireMonth.ToString(),
                    ExpYear = model.ExpireYear.ToString(),
                    Cvc = model.Cvc,
                    Name = model.Name,
                    AddressCity = model.City,
                    AddressCountry = model.Country,
                    AddressState = model.State,
                    AddressZip = model.Zip,
                    AddressLine1 = model.AddressLine1,
                    AddressLine2 = model.AddressLine2
                },
            };
            var serviceToken = new TokenService();
            var token = serviceToken.Create(tokenOption);

            var options = new CardCreateOptions
            {
                Source = token.Id
            };
            var service = new CardService();
            Card card = await service.CreateAsync(model.CustomerId, options);
            Serilog.Log.Information("Stripe Register Payment Card: \n" + JsonConvert.SerializeObject(model));
            return Result.Success(card);
        }

        public async Task<Result<Card>> GetPaymentCard(string cardId, string customerId)
        {
            var service = new CardService();
            Card card = await service.GetAsync(
              customerId,
              cardId
            );
            return Result.Success(card);
        }

        public async Task<Result<Card>> UpdatePaymentCard(UpdatePaymentCard model)
        {
            var options = new CardUpdateOptions
            {
                ExpMonth = model.ExpireMonth,
                ExpYear = model.ExpireYear,
                Name = model.Name,
                AddressCity = model.City,
                AddressCountry = model.Country,
                AddressState = model.State,
                AddressZip = model.Zip,
                AddressLine1 = model.AddressLine1,
                AddressLine2 = model.AddressLine2
            };
            var service = new CardService();
            Card card = await service.UpdateAsync(
              model.CustomerId,
              model.CardId,
              options
            );

            return Result.Success(card);
        }

        public async Task<Result<Card>> RemovePaymentCard(string cardId, string customerId)
        {
            var service = new CardService();
            Card card = await service.DeleteAsync(
              customerId,
              cardId
            );

            return Result.Success(card);
        }

        public async Task<Result<StripeList<Card>>> ListPaymentsCard(string customerId)
        {
            var service = new CardService();

            var cards = await service.ListAsync(customerId);

            return Result.Success(cards);
        }

        //list payment methods
        public async Task<Result<StripeList<PaymentMethod>>> ListPaymentMethods(string customerId)
        {
            var service = new PaymentMethodService();
            var paymentMethods = await service.ListAsync(new PaymentMethodListOptions
            {
                Customer = customerId,
                Type = "card"
            });

            return Result.Success(paymentMethods);
        }

        public async Task<Result<Charge>> CreatePayment(StripePayment model)
        {
            
            var options = new ChargeCreateOptions
            {
                Amount = model.Amount,
                Currency = "usd",
                Source = model.TokenPaymentCard,
                Description = model.Description,
                ReceiptEmail = model.ReceiptEmail,
                Customer = model.CustomerId
            };
            var service = new ChargeService();
            Charge ch = await service.CreateAsync(options);
            Serilog.Log.Information($"Payment: \n {JsonConvert.SerializeObject(ch)}");
            return Result.Success(ch);
        }

        // create payment intent
        public async Task<Result<PaymentIntent>> CreatePaymentIntent(StripePayment model)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = model.Amount * 100,
                Currency = "usd",
                PaymentMethod = model.TokenPaymentCard,
                Customer = model.CustomerId,
                Confirm = true,
                Description = model.Description,
                ReceiptEmail = model.ReceiptEmail
            };
            var service = new PaymentIntentService();
            PaymentIntent paymentIntent = await service.CreateAsync(options);
            Serilog.Log.Information($"Payment: \n {JsonConvert.SerializeObject(paymentIntent)}");
            return Result.Success(paymentIntent);
        }
    }
}
