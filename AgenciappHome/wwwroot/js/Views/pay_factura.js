$("#guardar").click(function () {
    var id = $(this).attr('data-facturaId');
    if (validatePay()) {
        var okConfirm = function () {
            var datos = [
                id,
                $(".hide-search-pago").val(),
                $("#cantPay").val().replace(",", "."),
                $('[name = "cuentaBancaria"]').val(),
                $('#referencia').val(),
                $('#description').val()
            ];
            $.ajax({
                type: "POST",
                url: "/Facturas/PagarFactura",
                data: JSON.stringify(datos),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function (data) {
                    if ($("#comprobante")[0].checked)
                        ""//window.open("/Bill/ExportBillRecive/" + id, "_blank");
                    else
                        window.location = "/Facturas/Index";
                }
            });
        };
        confirmationMsg("¿Está seguro que desea registrar este pago?", "¡Esta acción no se puede reestablecer!", okConfirm);
    }
});

var validatePay = function () {
    if ($("#cantPay").val() == "") {
        showWarningMessage("Atención", "El campo Cant. a pagar o esta vacio o debe ser expresado con punto flotante y no con coma.");
        return false;
    }
    else if ($("#cantPay").val() <= 0) {
        showWarningMessage("Atención", "El campo Cant. a pagar debe ser mayor que 0.");
        return false;
    } else if (parseFloat($("#cantPay").val().replace(",", ".")) > parseFloat($("#porPagar").html().replace(",", "."))) {
        showWarningMessage("Atención", "El campo Cant. a pagar debe ser menor o igual que la cantidad que queda por pagar.");
        return false;
    }

    return true;
};

//Mostrar y ocultar el campo referencia
$('[name = "tipoPago"]').on('change', function () {
    var idvalue = $(this).val();
    var value = $('option[value="' + idvalue + '"]').html();
    if (value == "Zelle" || value == "Cheque" || value == "Crédito o Débito") {
        $('#contReferencia').show();
    }
    else {
        $('#contReferencia').hide();
    }
});  