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

    $('#authCard').on('click', function () {
        var id = $(this).attr("data-value");
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/Orders/createFileAuthCard",
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
    })

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
            url: "/Orders/createOrderComprobante",
            data: {
                id: id
            },
            beforeSend: function () {
                $.blockUI();
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

                $.unblockUI();
            },
            error: function () {
                toastr.error("No se ha podido exportar", "Error");
                $.unblockUI();
            },
            timeout: 60000,
        });
    });

    $("#printOrder").click(function () {
        var id = $(this).attr("data-value");
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/Orders/createFileOrder",
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
                url: "/Orders/EnviarOrden",
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
                        showOKMessage("Enviar Orden", response);
                    }
                    else {
                        showErrorMessage("ERROR", "No se ha podido enviar el paquete");
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
                url: "/Orders/EnviarComprobante",
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
                        showErrorMessage("ERROR", "No se ha podido enviar el paquete");
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
            var urlDelete = "/Orders/Delete/" + orderId;
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
    });

    let actualBagNumber = 0;
    $('.bolsa-edit').on('click', function () {
        const id = $(this).data('bag');
        actualBagNumber = $('.bolsa-input').val() ?? 0;
        $(this).hide();
        $(`.bolsa-confirm[data-bag=${id}]`).show();
        $(`.bolsa-cancel[data-bag=${id}]`).show();
        $(`.bolsa-input[data-bag=${id}]`).attr('disabled', false);
    })
    $('.bolsa-cancel').on('click', function () {
        const id = $(this).data('bag');
        $(`.bolsa-edit[data-bag=${id}]`).show();
        $(`.bolsa-confirm[data-bag=${id}]`).hide();
        $(this).hide();
        $(`.bolsa-input[data-bag=${id}]`).attr('disabled', true);
        $(`.bolsa-input[data-bag=${id}]`).val(actualBagNumber).trigger('change');
    })

    $('.bolsa-confirm').on('click', function () {
        const id = $(this).data('bag');
        const value = parseInt($(`.bolsa-input[data-bag=${id}]`).val());
        const bag = $(`.bolsa-input[data-bag=${id}]`).data('bag');
        const order = $(this).data('order');
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/Orders/SetFisicalBag",
            data: {
                value: value,
                bag: bag,
                orderId: order
            },
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                $(`.bolsa-edit[data-bag=${id}]`).show();
                $(`.bolsa-confirm[data-bag=${id}]`).hide();
                $(`.bolsa-cancel[data-bag=${id}]`).hide();
                $(`.bolsa-input[data-bag=${id}]`).attr('disabled', true);
                $.unblockUI();
            },
            error: function (e) {
                console.log(e);
                toastr.error("No se ha podido guardar la informacion", "Error");
                $.unblockUI();
            },
        });

    })

    $('#btnAddAttach').on('click', function () {
        var id = $('#OrderId').val();
        var formData = new FormData();
        var files = $('#file')[0].files;
        for (var i = 0; i < files.length; i++) {
            formData.append('file', files[i]);
        }
        var description = $('#descriptionFile').val();
        if (description == null || description == "") {
            toastr.error("Debe especificar una descripcion para el fichero")
            return false;
        }
        formData.append("description", description)
        formData.append('orderId', id);
        $.ajax({
            async: true,
            url: '/airshipping/AddAttachments',
            type: 'POST',
            data: formData,
            processData: false,  // tell jQuery not to process the data
            contentType: false,  // tell jQuery not to set contentType
            beforeSend: function () {
                $.blockUI();
            },
            success: function (data) {
                if (data.success) {
                    location.reload();
                }
                else {
                    toastr.error(data.msg);
                }
                $.unblockUI();
            },
            error: function () {
                toastr.error("No se ha podido añadir el adjunto")
                $.unblockUI();
            }
        });
    })

    $('[name="deleteFile"]').on('click', function () {
        let result = confirm("¿Está seguro que desea eliminar el adjunto?")
        const id = $(this).attr('data-id');
        if (result) {
            $.ajax({
                async: true,
                url: '/airshipping/RemoveAttachments',
                type: 'POST',
                data: {
                    id: id
                },
                beforeSend: function () {
                    $.blockUI();
                },
                success: function (data) {
                    if (data.success) {
                        location.reload();
                    }
                    else {
                        toastr.error(data.msg);
                    }
                    $.unblockUI();
                },
                error: function () {
                    toastr.error("No se ha podido eliminar el adjunto")
                    $.unblockUI();
                }
            });
        }
    })

    $('#file').on('change', function () {
        const name = $(this)[0].files[0].name;
        $('#descriptionFile').val(name)
    })
});