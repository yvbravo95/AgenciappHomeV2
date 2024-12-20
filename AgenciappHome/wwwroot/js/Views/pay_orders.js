$(document).ready(function () {
    var porPagar = parseFloat($("#porPagar").html());
    var pagadoCredito = 0;
    var credeb = false;
    $(".hide-search-pago").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Tipo de Pago",
    });

    $("#guardar").click(function () {
        if (validatePay()) {
            var okConfirm = function () {
                var datos = [
                    $("#orderNumber").html(),
                    $(".hide-search-pago").val(),
                    $("#cantPay").val().replace(",", "."),
                    $('#referencia').val(),
                    $("#precio").html().replace(",", "."),
                    $("#check_credito").is(":checked")
                ];

                $.ajax({
                    type: "POST",
                    url: "/Orders/PagarOrden",
                    data: JSON.stringify(datos),
                    dataType: 'json',
                    contentType: 'application/json',
                    async: false,
                    success: function (data) {
                        if (data.success) {
                            if ($("#comprobante")[0].checked)
                                window.open("/Orders/ExportarComprobante?orderNumber=" + $("#orderNumber").html() + "&cantPay=" + $("#cantPay").val(), "_blank");
                            var tipotramite = $('#tipoTramite').html();
                            if (tipotramite == "Combo") {
                                const msg = "El pago se ha realizado correctamente";
                                window.location = `/orders/details/${orderId}?msg=${msg}`;

                            }
                            else {
                                window.location = "/airshipping/Index?msg=successPago&noPago=" + data.number;

                            }
                        }
                        else {
                            toastr.error(data.msg);
                        }
                    }
                });
            };
            confirmationMsg("¿Está seguro que desea registrar este pago?", "¡Esta acción no se puede reestablecer!", okConfirm);
        }
    });

    var validatePay = function () {
        if ($("#cantPay").val() == "" && pagadoCredito == 0){
            showWarningMessage("Atención", "El campo Cant. a pagar o esta vacio o debe ser expresado con punto flotante y no con coma.");
            return false;
        }
        else if ($("#cantPay").val() <= 0 && pagadoCredito == 0) {
            showWarningMessage("Atención", "El campo Cant. a pagar debe ser mayor que 0.");
            return false;
        } else if (parseFloat($("#cantPay").val()) > parseFloat($("#porPagar").html())) {
            showWarningMessage("Atención", "El campo Cant. a pagar debe ser menor o igual que la cantidad que queda por pagar.");
            return false;
        }

        return true;
    };

    //Mostrar y ocultar el campo referencia
    $('[name = "tipoPago"]').on('change', function () {
        var fee = parseFloat($("#fee").html());
        var idvalue = $(this).val();
        var value = $('option[value="' + idvalue + '"]').html();
        if (value == "Zelle" || value == "Cheque" || value == "Transferencia Bancaria") {
            $('#contReferencia').show();
        }
        else {
            $('#contReferencia').hide();
        }

        var debe = parseFloat($("#porPagar").html()) - pagadoCredito;
        if (value == "Crédito o Débito") {
            var debe = parseFloat($("#porPagar").html());
            debe = debe + (debe * (fee / 100));
            var precioTotalValue = parseFloat($("#pagado").html()) + debe;
            $("#precio").html(precioTotalValue.toFixed(2));
            $("#cantPay").attr("max", debe.toFixed(2));
            $("#cantPay").val(debe.toFixed(2));
            $("#porPagar").html((debe).toFixed(2));
            $("#cargo").show();
            porPagar = debe;
            credeb = true;
        }
        else if (credeb) {
            var debe = parseFloat($("#porPagar").html());
            debe = debe / (1 + fee / 100);

            var precioTotalValue = parseFloat($("#pagado").html()) + debe;

            $("#precio").html(precioTotalValue.toFixed(2));
            $("#cantPay").attr("max", debe.toFixed(2));
            $("#cantPay").val(debe.toFixed(2));
            $("#porPagar").html((debe).toFixed(2));
            $("#cargo").hide();
            porPagar = debe;
            credeb = false;
        }

    });  

    $("#check_credito").on('change', function () {
        if ($("#check_credito").is(":checked")) {
            var credito = $("#inputCredito").val();
            if (porPagar - credito > 0) {
                $("#cantPay").prop("max", porPagar - credito);
                pagadoCredito = credito;
            }
            else {
                $("#cantPay").prop("max", 0);
                pagadoCredito = porPagar;
            }
            if ($("#cantPay").val() > porPagar - pagadoCredito) {
                $("#cantPay").val((porPagar - pagadoCredito).toFixed(2));
            }
        }
        else {
            $("#cantPay").prop("max", porPagar);
            pagadoCredito = 0;
        }
    })
});