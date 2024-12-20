$(document).ready(function () {
    
    $("#saveSetting").click(function () {
        if (validateSetting()) {
            var okConfirm = function () {
                var source = [
                    $('#emailServer').val(),
                    $('#emailPort').val(),
                    $('#emailUser').val(),
                    $('#emailPass').val(),
                ];

                $.ajax({
                    type: "POST",
                    url: "/Home/SaveSetting",
                    data: JSON.stringify(source),
                    dataType: 'json',
                    contentType: 'application/json',
                    async: false,
                    success: function () {
                        showOKMessage("Configuración guardada", "Los cambios se han guardado con éxito");
                    },
                    failure: function (response) {
                        showErrorMessage("ERROR", response.responseText);
                    },
                    error: function (response) {
                        showErrorMessage("ERROR", response.responseText);
                    }
                });
            }
            confirmationMsg("¿Está seguro que desea guardar esta configuración?", "", okConfirm);
        }
    });

    var validateSetting = function () {
        if ($("#emailServer").val() == "") {
            showWarningMessage("Atención", "El campo Dirección del Servidor no puede estar vacío.");
            return false;
        } else if ($("#emailUser").val() == "") {
            showWarningMessage("Atención", "El campo Usuario no puede estar vacío.");
            return false;
        } else if ($("#emailPass").val() == "") {
            showWarningMessage("Atención", "El campo Contraseña no puede estar vacío.");
            return false;
        }

        var regexPhone = /^\d+(?!\.)$/;
        if (!regexPhone.test($("#emailPort").val())) {
            showWarningMessage("Atención", "El campo Puerto no tiene el formato correcto. Solo acepta números enteros positivos.");
            return false;
        }

        if ($("#emailPort").val() <= 0) {
            showWarningMessage("Atención", "El campo Puerto debe ser mayor que 0.");
            return false;
        }

        return true;
    };
});