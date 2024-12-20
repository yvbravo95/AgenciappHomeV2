class PaymentElavon {
    token = null;
    constructor() { }

    //Authenticate
    authenticate() {
        $.ajax({
            async: true,
            type: "POST",
            url: "/MerchantElavon/AuthenticateToken",
            data: {

            },
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                if (response.success) {
                    token = response.token;
                }

                $.unblockUI();
            },
            error: function (e) {
                $.unblockUI();
                toastr.error("No se ha podido autenticar para proceder al pago.")
            }
        })
    }

    pay() {
        if (this.token != null) {
            const sdk = new window.Elavon3DSWebSDK({
                baseUrl: 'https://uat.gw.fraud.eu.elavonaws.com/3ds2',
                // token from <baseUrl>/token (ex: https://uat.gw.fraud.eu.elavonaws.com/token)
                token:
                    token,
            });
            sdk
                .web3dsFlow({
                    messageId: 'TEST-MSG_ID',
                    purchaseAmount: '150',
                    acctNumber: '4000000000000077',
                    cardExpiryDate: '2312',
                    purchaseCurrency: '840',
                    purchaseExponent: '2',
                    messageCategory: '01',
                    transType: '01',
                    threeDSRequestorAuthenticationInd: '01',
                    challengeIframeElement: document.getElementById('holder'),
                })
                .then((response) => {
                    showResult(response);
                })
                .catch((e) => {
                    console.error(e);
                    showResult(e);
                });
        }
    }

    showResult(result) {
        document.getElementById('holder').innerHTML = `<pre>${JSON.stringify(
            result,
            null,
            2,
        )}</pre>`;
    }
}

$('.paybtn').click(function () {
    var payment = new PaymentElavon();
    payment.authenticate();
    payment.pay();
})
