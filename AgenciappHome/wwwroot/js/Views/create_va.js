$(document).ready(function () {
    
    $("#guardarNuevoVA").click(function () {
        if (validateNuevoVA()) {
            var source = [
                $('#nuevoVAName').val(),
                $('#nuevoVADescripcion').val(),
                $('#nuevoVAUm').val(),
                $('#nuevoVAValor').val().replace(".", ","),
                $('#nuevoVAArticulos').val(),
                $('#nuevoVAObservaciones').val(),
            ];

            $.ajax({
                type: "POST",
                url: "/ValorAduanals/AddVA",
                data: JSON.stringify(source),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function () {
                    window.location = "/ValorAduanals?msg=success&nombre=" + $('#nuevoVAName').val();
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

    var validateNuevoVA = function () {
        if ($("#nuevoVAName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($("#nuevoVAUm").val() == "") {
            showWarningMessage("Atención", "El campo Um Legal no puede estar vacío.");
            return false;
        } else if ($("#nuevoVAValor").val() == "") {
            showWarningMessage("Atención", "El campo Valor no puede estar vacío.");
            return false;
        } else if ($('#nuevoVAArticulos').val() == "") {
            showWarningMessage("Atención", "El campo Artículos no puede estar vacío.");
            return false;
        }

        if ($("#nuevoVAValor").val() != "") {
            var regexPhone = /^\d+(\.\d+)?$/;
            if (!regexPhone.test($("#nuevoVAValor").val())) {
                showWarningMessage("Atención", "El campo Valor no tiene el formato correcto. Debe ser un valor numérico positivo.");
                return false;
            }

            if ($("#nuevoVAValor").val() <= 0) {
                showWarningMessage("Atención", "El campo Valor debe ser mayor que 0.");
                return false;
            }
        }

        return true;
    };
});