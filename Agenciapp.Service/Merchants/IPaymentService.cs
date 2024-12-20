using System;
using System.Threading.Tasks;
using AgenciappHome.Models.Payment;

namespace Agenciapp.Service.Merchants
{
    public interface IPaymentService
    {
        Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> CardPurchase(MerchantPurshaseRequest transactionRequest);

        Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> TokenPurchase(MerchantTokenPurchaseRequest transactionRequest);

        Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> Authorize(MerchantPurshaseRequest transactionRequest);

        Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> TokenAuthorize(MerchantTokenPurchaseRequest transactionRequest);

        Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> Capture(MerchantModifyTransactionRequest transactionRequest);

        Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> Update(MerchantModifyTransactionRequest transactionRequest);

        Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> Refund(MerchantRefundTransactionRequest transactionRequest);

        Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> Void(MerchantCancelTransactionRequest transactionRequest);

        Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> Validate(MerchantPurshaseRequest transactionRequest);

        Task<ValidatorOrSucceedResult<MerchantTransactionResponse>> RemoveProfile(string token);
    }
}