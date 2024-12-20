using Agenciapp.Common.Services;
using Agenciapp.Domain.Models;
using Agenciapp.Service.IMarketing.Models;
using Agenciapp.Service.Merchants.Stripe;
using AgenciappHome.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Agenciapp.Service.IMarketing
{
    public interface IMarketingService
    {
        Task<MarketingReceiptCampaing> SendCampaing(SendCampaingModel model);
    }

    public class MarketingService : IMarketingService
    {
        private readonly databaseContext _context;
        private readonly IUserResolverService _userResolverService;
        public MarketingService(databaseContext context, IUserResolverService userResolverService)
        {
            _context = context;
            _userResolverService = userResolverService;
        }

        public async Task<MarketingReceiptCampaing> SendCampaing(SendCampaingModel model)
        {
            try
            {
                var user = _userResolverService.GetUser();
                var marketingDb = await _context.Marketings.FirstOrDefaultAsync(x => x.AgencyId == user.AgencyId);
                if (marketingDb == null)
                {
                    throw new Exception("No se ha configurado el servicio de marketing");
                }
                bool isMms = model.Files?.Count > 0;
                decimal price = isMms ? decimal.Parse(marketingDb.PriceMms) : decimal.Parse(marketingDb.PriceSms);
                decimal amount = model.PhoneNumbers.Count * price;

                // Process payment
                if (amount < (decimal)1) throw new Exception("El monto minimo es de $1.00");
                IStripeService stripeService = new StripeService(marketingDb.StripeSecretkey);
                var customer = await stripeService.GetCustomer(marketingDb.StripeCustomerId);
                if (customer.IsFailure)
                {
                    throw new Exception("No se ha encontrado el customer");
                }
                var cards = await stripeService.ListPaymentMethods(marketingDb.StripeCustomerId);
                if (cards.Value.Count() == 0)
                {
                    throw new Exception("No se ha configurado el metodo de pago");
                }
                var card = cards.Value.FirstOrDefault();
                var resultPayment = await stripeService.CreatePaymentIntent(new Merchants.Stripe.Models.StripePayment()
                {
                    Amount = (long)amount,
                    CustomerId = customer.Value.Id,
                    Description = "Marketing Campaign",
                    ReceiptEmail = customer.Value.Email,
                    TokenPaymentCard = card.Id
                });

                if (resultPayment.IsFailure)
                {
                    throw new Exception("No se ha podido procesar el pago");
                }

                // Send campaign
                string from = marketingDb.NumberFrom;
                string accountSid = marketingDb.AccountSid;
                string authToken = marketingDb.AuthToken;
                List<Task> tasks = new List<Task>
                {
                    SendMessage(model.Message, model.PhoneNumbers, from, model.Files, accountSid, authToken)
                };


                var campaing = new MarketingReceiptCampaing()
                {
                    CreatedAt = DateTime.Now,
                    IsMms = model.Files?.Count > 0,
                    Marketing = marketingDb,
                    Message = model.Message,
                    Amount = amount,
                    TotalSend = model.PhoneNumbers.Count,
                    PaymentReference = resultPayment.Value.Id,
                    FailSend = 0,
                    SuccessSend = 0
                };

                List<string> numbersUnsubscribe = new List<string>();

                while (tasks.Count > 0)
                {
                    Task<List<ResponseTwilio>> finishedTask = (Task<List<ResponseTwilio>>)await Task.WhenAny(tasks);
                    try
                    {
                        var result = finishedTask.Result;

                        result.ForEach(response =>
                        {
                            Serilog.Log.Information($"Message: {response.Sms} to {response.toNumber} from {response.fromNumber} - code: {response.ErrorCode} - status: {response.Status}");

                            if (response.ErrorCode == "21610") // Unsubscribe Client
                            {
                                numbersUnsubscribe.Add(response.toNumber);
                            }
                        });

                        int countSuccess = result.Where(x => x.Status == "queued" || x.Status == "sent").Count();
                        campaing.SuccessSend += countSuccess;
                        campaing.FailSend += result.Count - countSuccess;
                    }
                    catch (Exception e)
                    {
                        Serilog.Log.Error(e, e.Message);
                    }

                    tasks.Remove(finishedTask);
                }

                _context.MarketingReceiptCampaings.Add(campaing);
                await _context.SaveChangesAsync();

                if(numbersUnsubscribe.Count > 0)
                {
                    foreach (var number in numbersUnsubscribe)
                    {
                        var client = await _context.Client.Where(x => x.AgencyId == user.AgencyId && x.Phone.Number == number).ToListAsync();
                        if (client.Any())
                        {
                            foreach (var item in client)
                            {
                                item.MarketingStatus = "Unsubscribe";
                                _context.Client.Update(item);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return campaing;
            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, e.Message);
                throw;
            }
        }

        public async Task<List<ResponseTwilio>> SendMessage(string messaje, List<string> numbers, string from, List<Uri> uris, string accountSid, string authToken)
        {
            List<ResponseTwilio> response = new List<ResponseTwilio>();
            foreach (var to in numbers)
            {
                try
                {
                    TwilioClient.Init(accountSid, authToken);
                    var message = await MessageResource.CreateAsync(
                        body: messaje,
                        mediaUrl: uris,
                        from: new PhoneNumber(from),
                        to: new PhoneNumber(to)
                        );
                    response.Add(new ResponseTwilio
                    {
                        fromNumber = from,
                        Status = message.Status.ToString(),
                        Sms = messaje,
                        toNumber = to,
                        Sid = message.Sid,
                        ErrorCode = message.ErrorCode != null ? message.ErrorCode.ToString() : "-"
                    });
                }
                catch (Twilio.Exceptions.ApiException e)
                {
                    response.Add(new ResponseTwilio
                    {
                        fromNumber = from,
                        Status = e.Message,
                        Sms = messaje,
                        toNumber = to,
                        Sid = "-",
                        ErrorCode = e.Code.ToString()
                    });
                }
                catch (Exception e)
                {
                    response.Add(new ResponseTwilio
                    {
                        fromNumber = from,
                        Status = e.Message,
                        Sms = messaje,
                        toNumber = to,
                        Sid = "-",
                        ErrorCode = "ERR"
                    });
                }
            }

            return response;
        }
    }
}
