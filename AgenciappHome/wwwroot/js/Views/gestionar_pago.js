$(document).on("ready", function () {
    var pagadoCredito = 0;

    $("#tipoPago").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Tipo de Pago",
    });

    $("#guardar").click(function () {
        if (validatePay()) {
            var okConfirm = function () {
                $("#pago_form").submit();
            };
            confirmationMsg("¿Está seguro que desea registrar este pago?", "¡Esta acción no se puede reestablecer!", okConfirm);
        }
    });

    $("#check_credito").on('change', function () {
        if ($("#check_credito").is(":checked")) {
            var credito = $("#inputCredito").val();
            if (porPagar - credito > 0) {
                pagadoCredito = credito;
            }
            else {
                pagadoCredito = porPagar;
            }
            $("#cantPay").prop("disabled", true);
            $("#cantPay").val(0);
        }
        else {
            $("#cantPay").prop("disabled", false);
            $("#cantPay").prop("max", porPagar);
            pagadoCredito = 0;
        }
        calc();
    })

    var validatePay = function () {
        if ($("#cantPay").val() == "" && pagadoCredito == 0) {
            showWarningMessage("Atención", "El campo Cant. a pagar o esta vacio o debe ser expresado con punto flotante y no con coma.");
            return false;
        }
        else if ($("#cantPay").val() <= 0 && pagadoCredito == 0) {
            showWarningMessage("Atención", "El campo Cant. a pagar debe ser mayor que 0.");
            return false;
        } else if (parseFloat($("#porPagar").html()) < 0) {
            showWarningMessage("Atención", "El campo Cant. a pagar debe ser menor o igual que la cantidad que queda por pagar.");
            return false;
        }
        return true;
    };

    //Mostrar y ocultar el campo referencia
    $('#tipoPago').on('change', function () {
        var value = $("#tipoPago option:selected").text();
        if (value == "Zelle" || value == "Cheque" || value == "Transferencia Bancaria" || value == "Financiamiento") {
            $('#contReferencia').show();
        }
        else {
            $('#contReferencia').hide();
        }
        if (value == "Crédito o Débito") {
            $('#AddAuthorization').show();
            $("#cargo").show();
        }
        else {
            $('#AddAuthorization').hide();
            $("#cargo").hide();
        }
        if (value == "Zelle") {
            $("#btnModalZelle").show()
        }
        else {
            $("#btnModalZelle").hide()
        }
        calc();
    });

    $("#cantPay").on("change", function () {
        calcMoneyPayment();
    })

    function calc() {
        var value = $("#tipoPago option:selected").text();
        var precioTotalValue = porPagar - pagadoCredito;
        var total = precioTotal;
        if (value == "Crédito o Débito") {
            //Valor Sale Amount en authorization card
            $('[name = "AuthorizationCard.saleAmount"]').val(precioTotalValue.toFixed(2));
            var pago_fee = (precioTotalValue * (fee / 100));
            precioTotalValue = precioTotalValue + pago_fee;
            total = total + pago_fee;
            $('[name = "AuthorizationCard.TotalCharge"]').val(precioTotalValue.toFixed(2));
        }
        if (!$("#check_credito").is(":checked")) {
            $("#cantPay").val(precioTotalValue.toFixed(2));
            $("#cantPay").attr('max', precioTotalValue.toFixed(2));
            $("#porPagar").html(0);
        }
        else{
            $("#cantPay").val(0);
            $("#porPagar").html(precioTotalValue);
        }
        $("#precio").html(total.toFixed(2));
        $("#Total").val(total.toFixed(2));
        
    }

    function calcMoneyPayment() {
        var precioTotalValue = precioTotal;
        var max = precioTotalValue;
        var pagado = parseFloat($("#cantPay").val());
        var tipoPago = $("#tipoPago option:selected").text();

        if (tipoPago == "Crédito o Débito") {
            var pagdoReal = pagado / (1 + fee / 100)
            var feeCrDeb = pagado - pagdoReal;
            precioTotalValue = precioTotalValue + feeCrDeb;
            max = max + max * fee / 100;

            //Valor Sale Amount en authorization card
            $('[name = "AuthorizationCard.saleAmount"]').val(pagdoReal.toFixed(2));
            $('[name = "AuthorizationCard.TotalCharge"]').val(pagado);
        }

        precioTotalValue = precioTotalValue.toFixed(2);
        var balanceValue = 0;
        if ($("#check_credito").is(":checked")) {
            var pagoCredito = parseFloat($("#credito").html()).toFixed(2);
            pagoCredito = pagoCredito > precioTotalValue ? precioTotalValue : pagoCredito;
            balanceValue = precioTotalValue - pagado - pagoCredito - yaPagado;
        }
        else {
            balanceValue = precioTotalValue - pagado - yaPagado;
        }

        $("#Total").val(precioTotalValue);
        $("#precio").html(precioTotalValue);
        $("#cantPay").attr("max", max.toFixed(2));
        $("#porPagar").html(balanceValue.toFixed(2));
    };

    calc();
});