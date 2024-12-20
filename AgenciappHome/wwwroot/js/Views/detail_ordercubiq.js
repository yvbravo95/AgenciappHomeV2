$(document).ready(function () {

    var url = decodeURIComponent(window.location);
    var params = url.split("?")[1];
    if (params != null) {
        var params = params.split("&");
        var msg = params[0].split("=")[1];
        if (msg == "success") {
            var orderNumber = params[1].split("=")[1];
            showOKMessage("Nueva Orden", "Envío " + orderNumber + " creado con éxito");
        }
    }

    $("#btnSendOrderModal").click(function () {
        $("#modalTitle").html("Enviar");
        $("#sendComprobante").addClass("hidden");
        $("#sendOrder").removeClass("hidden");

        $("#sendModal").modal("show");
    });

    $("#btnSendComprobanteModal").click(function () {
        $("#modalTitle").html("Enviar Comprobante de Pago");
        $("#sendOrder").addClass("hidden");
        $("#sendComprobante").removeClass("hidden");

        $("#sendModal").modal("show");
    });
    
    $("#printOrderRecive").click(function () {
        var id = $(this).attr("data-value");
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/OrderCubiq/createOrderComprobante",
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
            },
            timeout: 20000,
        });
    });

    $("#printOrder").click(function () {
        var id = $(this).attr("data-value");
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/OrderCubiq/createFileOrder",
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
            },
            timeout: 4000,
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
                url: "/OrderCubiq/EnviarOrden",
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
                        showOKMessage("Enviar Trámite", response);
                    }
                    else {
                        showErrorMessage("ERROR", "No se ha podido enviar el trámite");
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
                url: "/OrderCubiq/EnviarComprobante",
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
                        showOKMessage("Enviar Comprobante de Pago", response);
                    }
                    else {
                        showErrorMessage("ERROR", "No se ha podido enviar el trámite");
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

    $("#imprimirOrden").click(function () {
        var id = $(this).attr("data-value");
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/OrderCubiq/ImprimirOrden",
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
            },
            timeout: 20000,
        });
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

    /*$(".deleteOrder").click(function () {
        var orderId = $(this).attr("orderId");
        var orderNumber = $(this).attr("name");
        var okConfirm = function () {
            var urlDelete = "/OrderCubiq/Delete/" + orderId;
            $.ajax({
                type: "GET",
                url: urlDelete,
                async: false,
                success: function () {
                    document.location = "/Orders?msg=successDelete&orderNumber=" + orderNumber;
                }
            });
        };
        getDelConfirmation(okConfirm);
    });*/
});