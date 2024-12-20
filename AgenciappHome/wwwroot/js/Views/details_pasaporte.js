
$(document).ready(function () {

    //Imprimir recibo de pago
    $('#printRecive').click(function(){
        var id = $(this).attr("data-value");
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/Passport/createComprobante",
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

    $("#btnSendComprobanteModal").click(function () {
        $("#modalTitle").html("Enviar Comprobante de Pago");
        $("#sendOrder").addClass("hidden");
        $("#sendComprobante").removeClass("hidden");

        $("#sendModal").modal("show");
    });


    $("#sendComprobante").click(function () {
        var idorden = $(this).attr('data-value');
        if (validateEnviar()) {
            var datos = [
                idorden,
                $('#email').val(),
            ];

            $.ajax({
                type: "POST",
                url: "/Passport/EnviarComprobante",
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
                        showErrorMessage("ERROR", "No se ha podido enviar el comprobante de pago");
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

    $('#file').on('change', function () {
        var id = $('#PassportId').val();
        var formData = new FormData();
        var files = $('#file')[0].files;
        for (var i = 0; i < files.length; i++) {
            formData.append('file', files[i]);
        }
        var description = $('#descriptionFile').val();
        if (description == null || description == "") {
            description = ".";
        }
        formData.append("description", description)
        formData.append('passportId', id);
        $.ajax({
            async: true,
            url: '/Passport/AddAttachments',
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
                url: '/Passport/RemoveAttachments',
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

    $("#foto").on("change", () => {
        $("form").submit();
    })
});