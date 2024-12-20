$("#guardar").click(function () {
    var id = $(this).attr('data-ticketId');
    console.log(id);
    if (validatePay()) {
        var okConfirm = function () {
            var datos = [
                $("#orderNumber").html(),
                $(".hide-search-pago").val(),
                $("#cantPay").val().replace(",", "."),
            ];

            $.ajax({
                type: "POST",
                url: "/Ticket/PagarTicket",
                data: JSON.stringify(datos),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function (data) {
                    if ($("#comprobante")[0].checked)
                        window.open("/Ticket/ExportTicketRecive/" + id, "_blank");

                    window.location = "/Ticket/Index?msg=successPago&noPago=" + data.number;
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
    } else if (parseFloat($("#cantPay").val()) > parseFloat($("#porPagar").html())) {
        showWarningMessage("Atención", "El campo Cant. a pagar debe ser menor o igual que la cantidad que queda por pagar.");
        return false;
    }

    return true;
};