$(document).ready(function () {
    //code here
    $('select').select2({
        dropdownParent: $('#sendModalSMS')
    });

    $('[name="selectEmpleado"]').prop('disabled', true);
    var check = "cliente";
    $('[name = "check"]').on('click', function () {
        check = $(this)[0].value;
        if (check == "empleado") {
            $('[name="selectEmpleado"]').prop('disabled', false);
            $('#select').show();
        }
        else {
            $('[name="selectEmpleado"]').prop('disabled', true);
            $('#select').hide();

        }
    });
    $("#sendSMS").click(function () {
        var selectEmpleado = $('[name = "selectEmpleado"]').val();
        datos = [
            $(this).attr("data-value"),
            selectEmpleado,
            check
        ];


        $.ajax({
            type: "POST",
            url: "/Servicios/EnviarSMS",
            data: JSON.stringify(datos),
            dataType: 'json',
            contentType: 'application/json',
            async: false,
            success: function (response) {
                $("#sendModalSMS").modal("hide");
                showOKMessage("Enviar Servicio", response);
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    });

    $('#acepto').on('change', function () {
        var checked = $(this).is(':checked');
        var id = $(this).attr('data-id');
        $.ajax({
            type: "POST",
            url: "/clients/ChangeCheckMessage",
            data: {
                check: checked,
                Id: id
            },
            contentType: "application/x-www-form-urlencoded",
            async: false,
            success: function (response) {
                if (response == "true") {
                    if (checked) {
                        showOKMessage("@Model.cliente.Name  @Model.cliente.LastName", "Ahora recibirá notificaciones por mensaje de texto.");
                    }
                    else {
                        showOKMessage("@Model.cliente.Name  @Model.cliente.LastName", "Ya no recibirá notificaciones por mensaje de texto.");
                    }
                }
                else {
                    showErrorMessage("ERROR", response.responseText);
                }
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    });

    $("#printServicio").click(function () {
        var id = $(this).attr("data-value");
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/Servicios/createFileServicio/",
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
    $("#printRecive").click(function () {
        var id = $(this).attr("data-value");
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/Rechargue/createFileRechargeRecive/",
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

    $("#btnsendServicio").click(function () {
        $("#sendModal").modal("show");
    });

    $("#btnSendComprobanteModal").click(function () {
        $("#modalTitle").html("Enviar Comprobante de Pago de la Reserva");
        $("#sendTicket").addClass("hidden");
        $("#sendComprobante").removeClass("hidden");

        $("#sendModal").modal("show");
    });

    $("#sendServicio").click(function () {
        if (validateEnviar()) {
            var datos = [
                $(this).attr("data-value"),
                $('#email').val(),
            ];

            $.ajax({
                type: "POST",
                url: "/Servicios/EnviarServicio",
                data: JSON.stringify(datos),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function (response) {
                    $("#sendModal").modal("hide");
                    $("body").removeClass("modal-open");
                    $(".modal-backdrop").remove();
                    $("#sendModal").css("display", "none");
                    var aux = response.split('-');
                    if (aux[0] == "success") {
                        showOKMessage("Enviar Recarga por email", aux[1]);
                    }
                    else {
                        showErrorMessage("ERROR", "No se ha podido enviar la recarga por email");
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
                $(this).attr("data-value"),
                $('#email').val(),
            ];

            $.ajax({
                type: "POST",
                url: "/Rechargue/EnviarComprobante",
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
                        showErrorMessage("ERROR", "No se ha podido enviar la reserva");
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


        return true;
    }

    $('#btnAddAttach').on('click', function () {
        var id = $('#ServicioId').val();
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
        formData.append('servicioId', id);
        $.ajax({
            async: true,
            url: '/Servicios/AddAttachments',
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
                url: '/Servicios/RemoveAttachments',
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

});