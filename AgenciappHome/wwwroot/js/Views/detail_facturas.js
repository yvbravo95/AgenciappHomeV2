$(document).ready(function () {

    $("#btnSendOrderModal").click(function () {
        $("#modalTitle").html("Enviar");
        $("#sendComprobante").addClass("hidden");
        $("#sendOrder").removeClass("hidden");

        $("#sendModal").modal("show");
    });

    $("#btnSendComprobanteModal").click(function () {
        $("#modalTitle").html("Enviar Factura");
        $("#sendOrder").addClass("hidden");
        $("#sendComprobante").removeClass("hidden");

        $("#sendModal").modal("show");
    });
   

    $("#printfactura").click(function () {
        var id = $(this).attr('data-value');
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/Facturas/createFileFactura",
            data: {
                id: id
            },
            success: function (response) {

                fileName = 'document.pdf';
                var byteCharacters = atob(response);
                var byteNumber = new Array(byteCharacters.length);
                for (var i = 0; i < byteCharacters.length; i++) {
                    byteNumber[i] = byteCharacters.charCodeAt(i);
                }
                var byteArray = new Uint8Array(byteNumber);
                var blob = new Blob([byteArray], { type: 'application/pdf' });
                var fileURL = URL.createObjectURL(blob);
                window.open(fileURL);
            },
            error: function () {
                toastr.error("No se ha podido exportar", "Error");
            }
        });
    });

    $("#sendOrder").click(function () {
        if (validateEnviar()) {
            var datos = [
                $("#id_orden").html(),
                $('#email').val(),
            ];

            $.ajax({
                type: "POST",
                url: "/Facturas/EnviarFactura",
                data: JSON.stringify(datos),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function (response) {
                    $("#sendModal").modal("hide");
                    $("body").removeClass("modal-open");
                    $(".modal-backdrop").remove();
                    $("#sendModal").css("display", "none");
                    if (response != "") {
                        showOKMessage("Enviar Factura", response);
                    }
                    else {
                        showErrorMessage("ERROR", "No se ha podido enviar la factura");
                    }
                },
                failure: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                },
                error: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                }
            });
        }
    });

    $("#sendComprobante").click(function () {
        if (validateEnviar()) {
            var datos = [
                $("#id_orden").html(),
                $('#email').val(),
            ];

            $.ajax({
                type: "POST",
                url: "/Facturas/EnviarFactura",
                data: JSON.stringify(datos),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function (response) {
                    $("#sendModal").modal("hide");
                    $("body").removeClass("modal-open");
                    $(".modal-backdrop").remove();
                    $("#sendModal").css("display", "none");
                    if (response != "") {
                        showOKMessage("Enviar Factura", response);
                    }
                    else {
                        showErrorMessage("ERROR", "No se ha podido enviar la remesa");
                    }
                },
                failure: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                },
                error: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                }
            });
        }
    });

    var validateEnviar = function () {
        if ($('#email').val() == "") {
            showWarningMessage("Atención", "El campo Email no puede estar vacío.");
            return false;
        }

        var regexEmail = /^([a-zA-Z0-9_\.\-\+])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;
        if (!regexEmail.test($('#email').val())) {
            showWarningMessage("Atención", "El campo Email no tiene el formato correcto.");
            return false;
        }

        return true;
    }

    $(".deleteOrder").click(function () {
        var orderId = $(this).attr("orderId");
        var orderNumber = $(this).attr("name");
        var okConfirm = function () {
            var urlDelete = "/Reservas/Delete/" + orderId;
            $.ajax({
                type: "GET",
                url: urlDelete,
                async: false,
                success: function () {
                    document.location = "/Remesas?msg=successDelete&orderNumber=" + orderNumber;
                }
            });
        };
        getDelConfirmation(okConfirm);
    });
});