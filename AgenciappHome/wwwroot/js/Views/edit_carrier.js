$(document).ready(function () {
    
    $("#guardarCarrier").click(function () {
        if (validateCarrier()) {
            var okConfirm = function () {
                var source = [
                    $('#carrierId').val(),
                    $('#carrierName').val(),
                    $('#carrierLastName').val(),
                ];

                $.ajax({
                    type: "POST",
                    url: "/Carriers/EditCarrier",
                    data: JSON.stringify(source),
                    dataType: 'json',
                    contentType: 'application/json',
                    async: false,
                    success: function () {
                        window.location = "/Carriers?msg=successEdit&nombre=" + $('#carrierName').val() + " " + $('#carrierLastName').val();
                    },
                    failure: function (response) {
                        showErrorMessage("ERROR", response.responseText);
                    },
                    error: function (response) {
                        showErrorMessage("ERROR", response.responseText);
                    }
                });
            }
            confirmationMsg("¿Está seguro que desea editar este carrier?", "", okConfirm);
        }
    });

    var validateCarrier = function () {
        if ($("#carrierName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($("#carrierLastName").val() == "") {
            showWarningMessage("Atención", "El campo Apellidos no puede estar vacío.");
            return false;
        }

        return true;
    };
});