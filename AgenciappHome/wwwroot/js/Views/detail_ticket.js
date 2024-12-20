
$("#printReserva").click(function () {
    var id = $(this).attr("data-value");
    $.ajax({
        async: true,
        type: "POST",
        contentType: "application/x-www-form-urlencoded",
        url: "/Ticket/createFileTicket",
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

$("#printRecive").click(function () {
    var id = $(this).attr("data-value");
    $.ajax({
        async: true,
        type: "POST",
        contentType: "application/x-www-form-urlencoded",
        url: "/Ticket/createFileTicketRecive",
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
        error: function (error) {
            console.log(error);
            toastr.error("No se ha podido exportar", "Error");
        },
        timeout: 20000,
    });
});

$("#authCard").click(function () {
    var id = $(this).attr("data-value");
    $.ajax({
        async: true,
        type: "POST",
        contentType: "application/x-www-form-urlencoded",
        url: "/Ticket/createFileAuthCard",
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
        timeout: 60000,
    });
});



$("#btnSendTicketModal").click(function () {
    $("#modalTitle").html("Enviar Reserva");
    $("#sendComprobante").addClass("hidden");
    $("#sendTicket").removeClass("hidden");

    $("#sendModal").modal("show");
});

$("#btnSendComprobanteModal").click(function () {
    $("#modalTitle").html("Enviar Comprobante de Pago de la Reserva");
    $("#sendTicket").addClass("hidden");
    $("#sendComprobante").removeClass("hidden");

    $("#sendModal").modal("show");
});




$("#sendTicket").click(function () {
    if (validateEnviar()) {
        var datos = [
            $(this).attr("data-value"),
            $('#email').val(),
        ];

        $.ajax({
            type: "POST",
            url: "/Ticket/EnviarTicket",
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
                    showOKMessage("Enviar Reserva", response);
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

$("#sendComprobante").click(function () {
    if (validateEnviar()) {
        var datos = [
            $(this).attr("data-value"),
            $('#email').val(),
        ];

        $.ajax({
            type: "POST",
            url: "/Ticket/EnviarComprobante",
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

    var regexEmail = /^[a-zA-Z0-9_.-]+@[a-zA-Z0-9-.]+\.[a-zA-Z0-9]{2,4}$/;
    if (!regexEmail.test($('#email').val())) {
        showWarningMessage("Atención", "El campo Email no tiene el formato correcto.");
        return false;
    }

    return true;
}

$('#btnAddAttach').on('click', function () {
    var id = $('#TicketId').val();
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
    formData.append('ticketId', id);
    $.ajax({
        async: true,
        url: '/Ticket/AddAttachments',
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
            url: '/Ticket/RemoveAttachments',
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