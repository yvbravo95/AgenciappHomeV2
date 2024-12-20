using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgenciappHome.Models;
using AgenciappHome.Models.Payment;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace Agenciapp.Service.Merchants
{
    public class AuthorizePaymentService : IPaymentService
    {
        private readonly databaseContext _context;

        public AuthorizePaymentService(databaseContext context)
        {
            _context = context;
        }
        public async Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> CardPurchase(
            MerchantPurshaseRequest transactionRequest)
        {
            var creditCard = new creditCardType
            {
                cardNumber = transactionRequest.CardInfo.Number,
                expirationDate = transactionRequest.CardInfo.Expiration,
                cardCode = transactionRequest.CardInfo.Cvv
            };

            var paymentType = new paymentType { Item = creditCard };

            var merchantRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),
                amount = transactionRequest.Amount,
                payment = paymentType,
                //refTransId = transactionRequest.OrderId,
                customer = new customerDataType
                {
                    //email = transactionRequest.Email,
                    type = customerTypeEnum.individual,
                    id = transactionRequest.UserId
                        .ToString()
                        .Replace("-", "")
                        .Substring(0, 20),
                    typeSpecified = true,

                },
                profile = new customerProfilePaymentType
                {
                    createProfile = true,
                },
                billTo = new customerAddressType
                {
                    address = transactionRequest.CardInfo.BillingAddress,
                    city = transactionRequest.CardInfo.City,
                    state = transactionRequest.CardInfo.State,
                    country = transactionRequest.CardInfo.CountryIso2,
                    zip = transactionRequest.CardInfo.ZipCode.ToString(),
                    firstName = transactionRequest.CardInfo.FirstName,
                    lastName = transactionRequest.CardInfo.LastName,
                    phoneNumber = transactionRequest.CardInfo.BillingPhone
                }
            };

            var request = new createTransactionRequest
            {
                transactionRequest = merchantRequest,
                refId = transactionRequest.OrderId
                    .Replace("-", "")
                    .Substring(0, 20),

            };
            var controller = new createTransactionController(request);
            controller.Execute();
            var response = controller.GetApiResponse();
            var result = HandleResponse(response, transactionRequest.OrderId);

            if (response.profileResponse?.customerProfileId != null &&
                response.profileResponse?.customerPaymentProfileIdList?.Any() == true)
            {

                var newCard = _context.PaymentCards.Add(new PaymentCard
                {
                    City = transactionRequest.CardInfo.City,
                    Expiration = transactionRequest.CardInfo.Expiration,
                    State = transactionRequest.CardInfo.State,
                    Token = $"{response.profileResponse?.customerProfileId}|{response.profileResponse?.customerPaymentProfileIdList[0]}",
                    Type = transactionRequest.CardInfo.Type,
                    BillingAddress = transactionRequest.CardInfo.BillingAddress,
                    BillingPhone = transactionRequest.CardInfo.BillingPhone,
                    CountryIso2 = transactionRequest.CardInfo.CountryIso2,
                    LastFour = transactionRequest.CardInfo.Number.Substring(transactionRequest.CardInfo.Number.Length - 4),
                    LastName = transactionRequest.CardInfo.LastName,
                    ZipCode = transactionRequest.CardInfo.ZipCode

                });
                await _context.SaveChangesAsync();
            }

            return result;


        }

        public async Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> TokenPurchase(
            MerchantTokenPurchaseRequest transactionRequest)
        {
            var customerProfileId = transactionRequest.PaymentToken.Split('|')[0];
            var customerPaymentProfileId = transactionRequest.PaymentToken.Split('|')[1];
            customerProfilePaymentType profileToCharge = new customerProfilePaymentType();
            profileToCharge.customerProfileId = customerProfileId;
            profileToCharge.paymentProfile = new paymentProfile
            {
                paymentProfileId = customerPaymentProfileId
            };

            var merchantTransactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),
                amount = transactionRequest.Amount,
                profile = profileToCharge,
                //refTransId = transactionRequest.OrderId,

            };

            var request = new createTransactionRequest
            {
                transactionRequest = merchantTransactionRequest,
                refId = transactionRequest.OrderId
                    .Replace("-", "")
                    .Substring(0, 20),
            };

            var controller = new createTransactionController(request);
            controller.Execute();

            var response = controller.GetApiResponse();
            var result = HandleResponse(response, transactionRequest.OrderId);
            return result;
        }

        public async Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> Authorize(
            MerchantPurshaseRequest transactionRequest)
        {
            var creditCard = new creditCardType
            {
                cardNumber = transactionRequest.CardInfo.Number,
                expirationDate = transactionRequest.CardInfo.Expiration,
                cardCode = transactionRequest.CardInfo.Cvv
            };

            var paymentType = new paymentType { Item = creditCard };

            var merchantRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.authOnlyTransaction.ToString(),
                amount = transactionRequest.Amount,
                payment = paymentType,
                //refTransId = transactionRequest.OrderId,
                customer = new customerDataType
                {
                    //email = transactionRequest.Email,
                    type = customerTypeEnum.individual,
                    id = transactionRequest.UserId
                        .ToString()
                        .Replace("-", "")
                        .Substring(0, 20),
                    typeSpecified = true,

                },
                profile = new customerProfilePaymentType
                {
                    createProfile = true,
                },
                billTo = new customerAddressType
                {
                    address = transactionRequest.CardInfo.BillingAddress,
                    city = transactionRequest.CardInfo.City,
                    state = transactionRequest.CardInfo.State,
                    country = transactionRequest.CardInfo.CountryIso2,
                    zip = transactionRequest.CardInfo.ZipCode.ToString(),
                    firstName = transactionRequest.CardInfo.FirstName,
                    lastName = transactionRequest.CardInfo.LastName,
                    phoneNumber = transactionRequest.CardInfo.BillingPhone
                }
            };

            var request = new createTransactionRequest
            {
                transactionRequest = merchantRequest,
                refId = transactionRequest.OrderId
                    .Replace("-", "")
                    .Substring(0, 20),
            };
            var controller = new createTransactionController(request);
            controller.Execute();
            var response = controller.GetApiResponse();
            var result = HandleResponse(response, transactionRequest.OrderId);

            if (response.profileResponse?.customerProfileId != null &&
                response.profileResponse?.customerPaymentProfileIdList?.Any() == true)
            {

                var newCard = _context.PaymentCards.Add(new PaymentCard
                {
                    City = transactionRequest.CardInfo.City,
                    Expiration = transactionRequest.CardInfo.Expiration,
                    State = transactionRequest.CardInfo.State,
                    Token = $"{response.profileResponse?.customerProfileId}|{response.profileResponse?.customerPaymentProfileIdList[0]}",
                    Type = transactionRequest.CardInfo.Type,
                    BillingAddress = transactionRequest.CardInfo.BillingAddress,
                    BillingPhone = transactionRequest.CardInfo.BillingPhone,
                    CountryIso2 = transactionRequest.CardInfo.CountryIso2,
                    LastFour = transactionRequest.CardInfo.Number.Substring(transactionRequest.CardInfo.Number.Length - 4),
                    LastName = transactionRequest.CardInfo.LastName,
                    ZipCode = transactionRequest.CardInfo.ZipCode

                });
                await _context.SaveChangesAsync();
            }

            return result;
        }

        public async Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> TokenAuthorize(
            MerchantTokenPurchaseRequest transactionRequest)
        {
            var customerProfileId = transactionRequest.PaymentToken.Split('|')[0];
            var customerPaymentProfileId = transactionRequest.PaymentToken.Split('|')[1];
            customerProfilePaymentType profileToCharge = new customerProfilePaymentType();
            profileToCharge.customerProfileId = customerProfileId;
            profileToCharge.paymentProfile = new paymentProfile
            {
                paymentProfileId = customerPaymentProfileId
            };

            var merchantTransactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.authOnlyTransaction.ToString(),
                amount = transactionRequest.Amount,
                profile = profileToCharge,
                //refTransId = transactionRequest.OrderId,

            };

            var request = new createTransactionRequest
            {
                transactionRequest = merchantTransactionRequest,
                refId = transactionRequest.OrderId
                    .Replace("-", "")
                    .Substring(0, 20),
            };
            var controller = new createTransactionController(request);
            controller.Execute();
            var response = controller.GetApiResponse();
            var result = HandleResponse(response, transactionRequest.OrderId);
            return result;
        }

        public async Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> Capture(
            MerchantModifyTransactionRequest paymentRequest)
        {
            var merchantTransactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.priorAuthCaptureTransaction.ToString(),
                amount = paymentRequest.Amount,
                refTransId = paymentRequest.ExternalMerchantTransactionId


            };

            var request = new createTransactionRequest
            {
                transactionRequest = merchantTransactionRequest,
                refId = paymentRequest.OrderId
                    .Replace("-", "")
                    .Substring(0, 20),
            };
            var controller = new createTransactionController(request);
            controller.Execute();
            var response = controller.GetApiResponse();
            var result = HandleResponse(response, paymentRequest.OrderId);
            return result;
        }

        public Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> Update(
            MerchantModifyTransactionRequest paymentRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> Refund(
            MerchantRefundTransactionRequest paymentRequest)
        {
            var creditCard = new creditCardType
            {
                cardNumber = paymentRequest.CardLastFour,
                expirationDate = "XXXX"
            };
            var paymentType = new paymentType { Item = creditCard };

            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.refundTransaction.ToString(),    // refund type
                payment = paymentType,
                amount = paymentRequest.Amount,
                refTransId = paymentRequest.ExternalMerchantTransactionId
            };

            var request = new createTransactionRequest
            {
                transactionRequest = transactionRequest,
                // refId = paymentRequest.OrderId
                //     .Replace("-","")
                //     .Substring(0,20),
            };
            var controller = new createTransactionController(request);
            controller.Execute();
            var response = controller.GetApiResponse();
            var result = HandleResponse(response, paymentRequest.OrderId);
            return result;
        }

        public async Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> Void(
            MerchantCancelTransactionRequest paymentRequest)
        {
            var merchantTransactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.voidTransaction.ToString(),
                amount = paymentRequest.Amount,
                refTransId = paymentRequest.ExternalMerchantTransactionId


            };

            var request = new createTransactionRequest
            {
                transactionRequest = merchantTransactionRequest,
                refId = paymentRequest.OrderId
                    .Replace("-", "")
                    .Substring(0, 20),
            };
            var controller = new createTransactionController(request);
            controller.Execute();
            var response = controller.GetApiResponse();
            var result = HandleResponse(response, paymentRequest.OrderId);
            return result;
        }

        public async Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> Validate(
            MerchantPurshaseRequest transactionRequest)
        {

            var creditCard = new creditCardType
            {
                cardNumber = transactionRequest.CardInfo.Number,
                expirationDate = transactionRequest.CardInfo.Expiration,
                cardCode = transactionRequest.CardInfo.Cvv,

            };
            var paymentType = new paymentType { Item = creditCard };
            List<customerPaymentProfileType> paymentProfileList = new List<customerPaymentProfileType>();
            customerPaymentProfileType ccPaymentProfile = new customerPaymentProfileType();
            ccPaymentProfile.payment = paymentType;
            ccPaymentProfile.billTo = new customerAddressType
            {
                address = transactionRequest.CardInfo.BillingAddress,
                city = transactionRequest.CardInfo.City,
                state = transactionRequest.CardInfo.State,
                country = transactionRequest.CardInfo.CountryIso2,
                zip = transactionRequest.CardInfo.ZipCode.ToString(),
                firstName = transactionRequest.CardInfo.FirstName,
                lastName = transactionRequest.CardInfo.LastName,
                phoneNumber = transactionRequest.CardInfo.BillingPhone
            };

            paymentProfileList.Add(ccPaymentProfile);
            List<customerAddressType> addressInfoList = new List<customerAddressType>();
            customerAddressType homeAddress = new customerAddressType();
            homeAddress.address = transactionRequest.CardInfo.BillingAddress;
            homeAddress.city = transactionRequest.CardInfo.City;
            homeAddress.state = transactionRequest.CardInfo.State;
            homeAddress.country = transactionRequest.CardInfo.CountryIso2;
            homeAddress.zip = transactionRequest.CardInfo.ZipCode.ToString();
            homeAddress.phoneNumber = transactionRequest.CardInfo.BillingPhone;


            addressInfoList.Add(homeAddress);
            customerProfileType customerProfile = new customerProfileType();
            customerProfile.description = $"Rapid App, profile for user {transactionRequest.UserId.ToString()} {transactionRequest.Email}";
            customerProfile.email = transactionRequest.Email;
            customerProfile.paymentProfiles = paymentProfileList.ToArray();
            customerProfile.shipToList = addressInfoList.ToArray();

            var request = new createCustomerProfileRequest
            {
                profile = customerProfile,
                validationMode = validationModeEnum.liveMode,

            };
            var controller = new createCustomerProfileController(request);
            controller.Execute();
            var response = controller.GetApiResponse();
            var error = controller.GetErrorResponse();
            if (response == null || response.messages?.resultCode != messageTypeEnum.Ok)
                return new ValidatorOrSucceedResult<MerchantTransactionResponse>
                {
                    IsValidOrSucced = false,
                    ErrorMessage = response?.messages?.message[0].text ?? "Transacción fallida",
                    Obj = new MerchantTransactionResponse
                    {
                        PaymentStatus = PaymentStatus.Declined,
                        Message = response?.messages?.message[0].text ?? "Transacción fallida",
                        FullJsonResponse = response == null ? null : JsonConvert.SerializeObject(response),
                        BusinessTransactionOrderId = transactionRequest.OrderId,

                    }
                };

            return new ValidatorOrSucceedResult<MerchantTransactionResponse>
            {
                IsValidOrSucced = true,

                Obj = new MerchantTransactionResponse
                {
                    PaymentStatus = PaymentStatus.Completed,
                    Message = response.messages?.message[0].text ?? "Transacción completada",

                    BusinessTransactionOrderId = transactionRequest.OrderId,
                    Token = response?.customerProfileId != null &&
                            response?.customerPaymentProfileIdList?.Any() == true
                        ? $"{response?.customerProfileId}|{response?.customerPaymentProfileIdList[0]}"
                        : null
                }
            };
        }

        public async Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> RemoveProfile(string token)
        {
            var customerProfileId = token.Split('|')[0];
            var customerPaymentProfileId = token.Split('|')[1];
            var request = new deleteCustomerProfileRequest
            {
                customerProfileId = customerProfileId,
            };

            //Prepare Request
            var request2 = new deleteCustomerPaymentProfileRequest()
            {
                customerProfileId = customerProfileId,
                customerPaymentProfileId = customerPaymentProfileId
            };
            var controller2 = new deleteCustomerPaymentProfileController(request2);
            controller2.Execute();
            //Send Request to EndPoint
            var response2 = controller2.GetApiResponse();

            var controller = new deleteCustomerProfileController(request);
            controller.Execute();
            //Send Request to EndPoint
            deleteCustomerProfileResponse response = controller.GetApiResponse();

            
            if (response == null
                || response.messages?.resultCode != messageTypeEnum.Ok
                || response2 == null
                || response2.messages?.resultCode != messageTypeEnum.Ok
                )
                return new ValidatorOrSucceedResult<MerchantTransactionResponse>
                {
                    IsValidOrSucced = false,
                    ErrorMessage = response.messages?.message[0].text ?? "Transacción fallida",
                    Obj = new MerchantTransactionResponse
                    {
                        PaymentStatus = PaymentStatus.Declined,
                        Message = response.messages?.message[0].text ?? "Transacción fallida",

                    }
                };

            return new ValidatorOrSucceedResult<MerchantTransactionResponse>
            {
                IsValidOrSucced = true,

                Obj = new MerchantTransactionResponse
                {
                    PaymentStatus = PaymentStatus.Completed,
                    Message = response.messages?.message[0].text ?? "Transacción completada",
                    FullJsonResponse = response == null ? null : JsonConvert.SerializeObject(response),

                }
            };
        }

        private ValidatorOrSucceedResult<MerchantTransactionResponse> HandleResponse(
            createTransactionResponse response, string orderId)
        {
            if (response == null
                || response.messages?.resultCode != messageTypeEnum.Ok
                || response.transactionResponse.responseCode != "1")
                return BadMerchantResponse(response, orderId);

            return OkMerchantResponse(response, orderId);
        }

        private ValidatorOrSucceedResult<MerchantTransactionResponse> BadMerchantResponse(
            createTransactionResponse response, string orderId)
        {
            return new ValidatorOrSucceedResult<MerchantTransactionResponse>
            {
                IsValidOrSucced = false,
                ErrorMessage = response.messages?.message[0].text ?? "Transacción fallida",
                Obj = new MerchantTransactionResponse
                {
                    PaymentStatus = PaymentStatus.Declined,
                    Message = response.messages?.message[0].text ?? "Transacción fallida",
                    //FullJsonResponse = response == null ? null : JsonConvert.SerializeObject(response),
                    BusinessTransactionOrderId = orderId,

                }
            };
        }

        private ValidatorOrSucceedResult<MerchantTransactionResponse> OkMerchantResponse(
            createTransactionResponse response, string orderId)
        {
            return new ValidatorOrSucceedResult<MerchantTransactionResponse>
            {
                IsValidOrSucced = true,

                Obj = new MerchantTransactionResponse
                {
                    AuthCode = response.transactionResponse?.authCode,
                    PaymentStatus = PaymentStatus.Completed,
                    Message = response.messages?.message[0].text ?? "Transacción fallida",
                    FullJsonResponse = response == null ? null : JsonConvert.SerializeObject(response),
                    MerchantTransactionId = response.transactionResponse?.transId,
                    BusinessTransactionOrderId = orderId,
                    Token = response.profileResponse?.customerProfileId != null &&
                            response.profileResponse?.customerPaymentProfileIdList?.Any() == true
                    ? $"{response.profileResponse?.customerProfileId}|{response.profileResponse?.customerPaymentProfileIdList[0]}"
                    : null
                }
            };
        }
    }
}