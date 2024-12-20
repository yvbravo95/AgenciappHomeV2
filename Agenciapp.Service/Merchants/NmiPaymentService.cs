using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using AgenciappHome.Models.Payment;
using RapidMultiservice.Models.Settings;

namespace Agenciapp.Service.Merchants
{
    public class NmiPaymentService : IPaymentService
    {
        private IRequestBodyHelper _requestBodyHelper;
        private readonly INmiSettings _nmiSettings;
        private readonly IHttpClientFactory _httpClientFactory;


        public NmiPaymentService(IRequestBodyHelper requestBodyHelper,
            INmiSettings nmiSettings,
            IHttpClientFactory httpClientFactory)
        {
            _requestBodyHelper = requestBodyHelper;
            _nmiSettings = nmiSettings;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> CardPurchase(
            MerchantPurshaseRequest transactionRequest)
        {
            var paymentRequest = new PaymentNmiRequest()
            {
                Amount = transactionRequest.Amount.ToString("N", CultureInfo.InvariantCulture),
                // Username = _configuration.NmiUsername,
                // Password = _configuration.NmiPassword,
                SecurityKey = _nmiSettings.NmiSecurityKey,
                Type = NmiTrasactionTypes.Sale,

                CustomerVault = CustomerVault.AddCustomer,
                Ccnumber = transactionRequest.CardInfo.Number,
                Ccexp = transactionRequest.CardInfo.Expiration,
                Cvv = transactionRequest.CardInfo.Cvv,
                Descriptor = transactionRequest.TransacationDescriptor,
                Orderid = transactionRequest.OrderId,
                Email = transactionRequest.Email,
                Phone = transactionRequest.CardInfo.BillingPhone,
                OrderDescription = transactionRequest.OrderDescription
            };
            return await MakeMerchantAction(paymentRequest, transactionRequest.OrderId);
        }

        public async Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> TokenPurchase(
            MerchantTokenPurchaseRequest transactionRequest)
        {
            var paymentRequest = new PaymentNmiRequest()
            {
                Amount = transactionRequest.Amount.ToString("N", CultureInfo.InvariantCulture),
                //Username = _configuration.NmiUsername,
                //Password = _configuration.NmiPassword,
                SecurityKey = _nmiSettings.NmiSecurityKey,
                Type = NmiTrasactionTypes.Sale,
                CustomerVault = CustomerVault.UpdateCustomer, //TODO: Check if neccesary
                // Ccnumber = transactionRequest.CardInfo.Number,
                // Ccexp = transactionRequest.CardInfo.Expiration,
                // Cvv = transactionRequest.CardInfo.Cvv,
                Descriptor = transactionRequest.TransacationDescriptor,
                Orderid = transactionRequest.OrderId,
                CustomerVaultId = transactionRequest.PaymentToken,
            };
            return await MakeMerchantAction(paymentRequest, transactionRequest.OrderId);
        }

        public async Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> Authorize(
            MerchantPurshaseRequest transactionRequest)
        {
            var paymentRequest = new PaymentNmiRequest()
            {
                Amount = transactionRequest.Amount.ToString("N", CultureInfo.InvariantCulture),
                // Username = _configuration.NmiUsername,
                //Password = _configuration.NmiPassword,
                SecurityKey = _nmiSettings.NmiSecurityKey,
                Type = NmiTrasactionTypes.Authorization,

                CustomerVault = CustomerVault.AddCustomer,
                Ccnumber = transactionRequest.CardInfo.Number,
                Ccexp = transactionRequest.CardInfo.Expiration,
                Cvv = transactionRequest.CardInfo.Cvv,
                Descriptor = transactionRequest.TransacationDescriptor,
                Orderid = transactionRequest.OrderId,
                Email = transactionRequest.Email,
                Phone = transactionRequest.CardInfo.BillingPhone,
                OrderDescription = transactionRequest.OrderDescription
            };
            return await MakeMerchantAction(paymentRequest, transactionRequest.OrderId);
        }

        public async Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> TokenAuthorize(
            MerchantTokenPurchaseRequest transactionRequest)
        {
            var paymentRequest = new PaymentNmiRequest()
            {
                Amount = transactionRequest.Amount.ToString("N", CultureInfo.InvariantCulture),
                //Username = _configuration.NmiUsername,
                //Password = _configuration.NmiPassword,
                SecurityKey = _nmiSettings.NmiSecurityKey,
                Type = NmiTrasactionTypes.Authorization,
                CustomerVault = CustomerVault.UpdateCustomer, //TODO: Check if neccesary
                // Ccnumber = transactionRequest.CardInfo.Number,
                // Ccexp = transactionRequest.CardInfo.Expiration,
                // Cvv = transactionRequest.CardInfo.Cvv,
                Descriptor = transactionRequest.TransacationDescriptor,
                Orderid = transactionRequest.OrderId,
                CustomerVaultId = transactionRequest.PaymentToken,
            };
            return await MakeMerchantAction(paymentRequest, transactionRequest.OrderId);
        }


        public async Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> Capture(
            MerchantModifyTransactionRequest modifyTransactionRequest)
        {
            var paymentRequest = new ModifyNmiRequest()
            {
                Amount = modifyTransactionRequest.Amount.ToString("N", CultureInfo.InvariantCulture),
                //Username = _configuration.NmiUsername,
                //Password = _configuration.NmiPassword,
                SecurityKey = _nmiSettings.NmiSecurityKey,
                Transactionid = modifyTransactionRequest.ExternalMerchantTransactionId.ToString(),
                Type = NmiTrasactionTypes.Capture
            };
            return await MakeMerchantAction(paymentRequest, modifyTransactionRequest.OrderId);
        }


        public async Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> Refund(
            MerchantRefundTransactionRequest refundTransactionRequest)
        {
            var paymentRequest = new ModifyNmiRequest()
            {
                Amount = refundTransactionRequest.Amount.ToString("N", CultureInfo.InvariantCulture),
                // Username = _configuration.NmiUsername,
                //Password = _configuration.NmiPassword,
                SecurityKey = _nmiSettings.NmiSecurityKey,
                Transactionid = refundTransactionRequest.ExternalMerchantTransactionId.ToString(),
                Type = NmiTrasactionTypes.Refund
            };
            return await MakeMerchantAction(paymentRequest, refundTransactionRequest.OrderId);
        }


        public async Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> Update(
            MerchantModifyTransactionRequest updatePaymentRequest)
        {
            var paymentRequest = new ModifyNmiRequest()
            {
                Amount = updatePaymentRequest.Amount.ToString("N", CultureInfo.InvariantCulture),
                // Username = _configuration.NmiUsername,
                //Password = _configuration.NmiPassword,
                SecurityKey = _nmiSettings.NmiSecurityKey,
                Transactionid = updatePaymentRequest.ExternalMerchantTransactionId.ToString(),
                Type = NmiTrasactionTypes.Update,
                OrderId = updatePaymentRequest.OrderId.ToString()
            };

            return await MakeMerchantAction(paymentRequest, updatePaymentRequest.OrderId);
        }


        public async Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> Void(
            MerchantCancelTransactionRequest cancelTransactionRequest)
        {
            var paymentRequest = new ModifyNmiRequest()
            {
                Amount = cancelTransactionRequest.Amount.ToString("N", CultureInfo.InvariantCulture),
                // Username = _configuration.NmiUsername,
                // Password = _configuration.NmiPassword,
                SecurityKey = _nmiSettings.NmiSecurityKey,
                Transactionid = cancelTransactionRequest.ExternalMerchantTransactionId,
                Type = NmiTrasactionTypes.Refund
            };
            return await MakeMerchantAction(paymentRequest, cancelTransactionRequest.OrderId);
        }


        public async Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> Validate(
            MerchantPurshaseRequest transactionRequest)
        {
            var paymentRequest = new PaymentNmiRequest()
            {
                // Amount = "0.00",
                // Username = _configuration.NmiUsername,
                //Password = _configuration.NmiPassword,
                SecurityKey = _nmiSettings.NmiSecurityKey,
                Type = NmiTrasactionTypes.Validate,
                CustomerVault = CustomerVault.AddCustomer,
                Ccnumber = transactionRequest.CardInfo.Number,
                Ccexp = transactionRequest.CardInfo.Expiration,
                Cvv = transactionRequest.CardInfo.Cvv,

                Email = transactionRequest.Email,
                Phone = transactionRequest.CardInfo.BillingPhone,
            };
            return await MakeMerchantAction(paymentRequest, transactionRequest.OrderId);
        }

        public Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> RemoveProfile(string token)
        {
            throw new NotImplementedException();
        }

        private async Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> MakeMerchantAction(
            object paymentModel,
            string merchantTransactionId)
        {
            var responseString = string.Empty;
            try
            {
                var values = _requestBodyHelper.GetBody(paymentModel);
                var httpClient = _httpClientFactory.CreateClient("Nmi");
                var response = await httpClient.PostAsync("", new FormUrlEncodedContent(values));
                responseString = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                return new ValidatorOrSucceedResult<MerchantTransactionResponse>
                {
                    IsValidOrSucced = false,
                    ErrorMessage = "Merchant request failed"
                };
            }

            try
            {
                var dicResponse = ParseResponse(responseString);

                var nmiResult = new NmiTransactionResponseModel
                {
                    TransactionResponse = (TransactionResponseType)(int.Parse(dicResponse["response"]) - 1),
                    AuthCode = dicResponse["authcode"],
                    ResponseCode = ResponseCodes.NmiResultCode.ContainsKey(dicResponse["response_code"])
                        ? ResponseCodes.NmiResultCode[dicResponse["response_code"]]
                        : string.IsNullOrEmpty(dicResponse["response_code"])
                            ? string.Empty
                            : $"No Value found for code: {dicResponse["response_code"]}",
                    ResponseText = dicResponse["responsetext"],
                    TransactionId = dicResponse["transactionid"],
                    CvvResponse = ResponseCodes.NmiCvvResponseCodes.ContainsKey(dicResponse["cvvresponse"])
                        ? ResponseCodes.NmiCvvResponseCodes[dicResponse["cvvresponse"]]
                        : string.IsNullOrEmpty(dicResponse["cvvresponse"])
                            ? string.Empty
                            : $"No Value found for cvv code: {dicResponse["cvvresponse"]}",
                    AvsResponse = dicResponse["avsresponse"],
                    Orderid = dicResponse["orderid"],
                    CustomerVaultId = dicResponse.ContainsKey("customer_vault_id")
                        ? dicResponse["customer_vault_id"]
                        : null
                };
                var result = new MerchantTransactionResponse()
                {
                    MerchantTransactionId = nmiResult.Orderid,
                    BusinessTransactionOrderId = nmiResult.TransactionId,
                    AuthCode = nmiResult.AuthCode,
                    Token = nmiResult.CustomerVaultId,
                    Message = $"{nmiResult.ResponseCode} | {nmiResult.ResponseText}",
                    FullJsonResponse = responseString,
                };
                switch (nmiResult.TransactionResponse)
                {
                    case TransactionResponseType.TransactionApproved:
                        var type = paymentModel.GetType();
                        var property = type.GetProperty("Type");
                        var transactionType = (NmiTrasactionTypes)property.GetValue(paymentModel);
                        if (transactionType == NmiTrasactionTypes.Sale)
                            result.PaymentStatus = PaymentStatus.Completed;
                        else if (transactionType == NmiTrasactionTypes.Authorization)
                            result.PaymentStatus = PaymentStatus.Authorized;
                        else if (transactionType == NmiTrasactionTypes.Capture)
                            result.PaymentStatus = PaymentStatus.Completed;
                        else if (transactionType == NmiTrasactionTypes.Update)
                            result.PaymentStatus = PaymentStatus.Completed;
                        else if (transactionType == NmiTrasactionTypes.voidT)
                            result.PaymentStatus = PaymentStatus.Cancelled;
                        else if (transactionType == NmiTrasactionTypes.Refund)
                            result.PaymentStatus = PaymentStatus.Completed;

                        break;
                    case TransactionResponseType.TransactionDeclined:
                        result.PaymentStatus = PaymentStatus.Declined;
                        break;
                    case TransactionResponseType.Error:
                        result.PaymentStatus = PaymentStatus.Error;
                        break;
                    default:
                        break;
                }

                return new ValidatorOrSucceedResult<MerchantTransactionResponse>
                {
                    Obj = result,
                    IsValidOrSucced = true
                };
            }
            catch (Exception e)
            {
                return new ValidatorOrSucceedResult<MerchantTransactionResponse>
                {
                    IsValidOrSucced = false,
                    ErrorMessage = "Merchant response processing failed"
                };
            }
        }

        private Dictionary<string, string> ParseResponse(string response)
        {
            var resut = new Dictionary<string, string>();
            foreach (var item in response.Split('&'))
            {
                resut.Add(item.Split('=')[0], item.Split('=')[1]);
            }

            return resut;
        }
    }
}