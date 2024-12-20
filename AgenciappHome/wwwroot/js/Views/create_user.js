$(document).ready(function () {
    
    $(".hide-search-newUserTipo").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Rol",
    });

    $(".select2-placeholder-selectAgency").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Agencia",
    });
    
    $(".select2-container--default").attr("style", "width: 100%;");

    /*
    $("#guardarNuevoUsuario").click(function () {
        if (validateNuevoUsuario()) {
            var source = [
                $('#Username').val(),
                $('#Email').val(),
                $('#Firstname').val(),
                $('#Lastname').val(),
                $('#Password').val(),
                $('#Password').val(),
                $('.hide-search-newUserTipo').val(),
                $('.select2-placeholder-selectAgency').val(),
            ];

            $.ajax({
                type: "POST",
                url: "/Users/AddUser",
                data: JSON.stringify(source),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function () {
                    window.location = "/Users?msg=success&nombre=" + $('#nuevoUserName').val() + " " + $('#nuevoUserLastName').val();
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

    var validateNuevoUsuario = function () {
        if ($("#Username").val() == "") {
            showWarningMessage("Atención", "El campo Nombre de Usuario no puede estar vacío.");
            return false;
        } else if ($("#Email").val() == "") {
            showWarningMessage("Atención", "El campo Correo no puede estar vacío.");
            return false;
        } else if ($("#Username").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($("#Lastname").val() == "") {
            showWarningMessage("Atención", "El campo Apellidos no puede estar vacío.");
            return false;
        } else if ($("#Password").val() == "") {
            showWarningMessage("Atención", "El campo Contraseña no puede estar vacío.");
            return false;
        } else if ($("#ConfirmPassword").val() == "") {
            showWarningMessage("Atención", "El campo Contraseña(Confirmación) no puede estar vacío.");
            return false;
        }

        if ($("#Password").val() != $("#ConfirmPassword").val()) {
            showWarningMessage("Atención", "La Contraseña y su Confirmación no coinciden.");
            return false;
        }

        if ($("#Email").val() != "") {
            var regexEmail = /^([a-zA-Z0-9_\.\-\+])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;
            if (!regexEmail.test($("#Email").val())) {
                showWarningMessage("Atención", "El campo Email no tiene el formato correcto.");
                return false;
            }
        }

        return true;
    };
    */
});