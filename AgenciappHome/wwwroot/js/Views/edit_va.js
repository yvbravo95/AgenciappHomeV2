$(document).ready(function () {
    
    $("#guardarVA").click(function () {
        if (validateVA()) {
            var okConfirm = function () {
                var source = [
                    $('#VAId').val(),
                    $('#VAName').val(),
                    $('#VADescripcion').val(),
                    $('#VAUm').val(),
                    $('#VAValor').val().replace(".", ","),
                    $('#VAArticulos').val(),
                    $('#VAObservaciones').val(),
                ];

                $.ajax({
                    type: "POST",
                    url: "/ValorAduanals/EditVA",
                    data: JSON.stringify(source),
                    dataType: 'json',
                    contentType: 'application/json',
                    async: false,
                    success: function () {
                        window.location = "/ValorAduanals?msg=successEdit&nombre=" + $('#VAName').val();
                    },
                    failure: function (response) {
                        showErrorMessage("ERROR", response.responseText);
                    },
                    error: function (response) {
                        showErrorMessage("ERROR", response.responseText);
                    }
                });
            }
            confirmationMsg("¿Está seguro que desea editar este valor aduanal?", "", okConfirm);
        }
    });

    var validateVA = function () {
        if ($("#VAName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($("#VAUm").val() == "") {
            showWarningMessage("Atención", "El campo Um Legal no puede estar vacío.");
            return false;
        } else if ($("#VAValor").val() == "") {
            showWarningMessage("Atención", "El campo Valor no puede estar vacío.");
            return false;
        } else if ($('#VAArticulos').val() == "") {
            showWarningMessage("Atención", "El campo Artículos no puede estar vacío.");
            return false;
        }

        if ($("#VAValor").val() != "") {
            var regexPhone = /^\d+(\.\d+)?$/;
            if (!regexPhone.test($("#VAValor").val())) {
                showWarningMessage("Atención", "El campo Valor no tiene el formato correcto. Debe ser un valor numérico positivo.");
                return false;
            }

            if ($("#VAValor").val() <= 0) {
                showWarningMessage("Atención", "El campo Valor debe ser mayor que 0.");
                return false;
            }
        }

        return true;
    };
});