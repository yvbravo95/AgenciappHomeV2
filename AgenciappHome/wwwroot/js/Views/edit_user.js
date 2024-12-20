$(document).ready(function () {
    
    $(".hide-search-editUserTipo").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Rol",
    });

    $(".select2-placeholder-selectAgency").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Agencia",
    });
    
    $(".select2-container--default").attr("style", "width: 100%;");

    $("#guardarUsuario").click(function () {
        if (validateUsuario()) {
            var okConfirm = function () {
                var source = [
                    $('#editUserId').val(),
                    $('#editUserUsername').val(),
                    $('#editUserEmail').val(),
                    $('#editUserName').val(),
                    $('#editUserLastName').val(),
                    $('#editUserPassword').val(),
                    $('.hide-search-editUserTipo').val(),
                    $('.select2-placeholder-selectAgency').val(),
                ];

                $.ajax({
                    type: "POST",
                    url: "/Users/EditUser",
                    data: JSON.stringify(source),
                    dataType: 'json',
                    contentType: 'application/json',
                    async: false,
                    success: function () {
                        window.location = "/Users?msg=successEdit&nombre=" + $('#editUserName').val() + " " + $('#editUserLastName').val();
                    },
                    failure: function (response) {
                        showErrorMessage("ERROR", response.responseText);
                    },
                    error: function (response) {
                        showErrorMessage("ERROR", response.responseText);
                    }
                });
            }
            confirmationMsg("¿Está seguro que desea editar este usuario?", "", okConfirm);
        }
    });

    var validateUsuario = function () {
        if ($("#editUserUsername").val() == "") {
            showWarningMessage("Atención", "El campo Nombre de Usuario no puede estar vacío.");
            return false;
        } else if ($("#editUserEmail").val() == "") {
            showWarningMessage("Atención", "El campo Correo no puede estar vacío.");
            return false;
        } else if ($("#editUserName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($("#editUserLastName").val() == "") {
            showWarningMessage("Atención", "El campo Apellidos no puede estar vacío.");
            return false;
        } else if ($("#editUserPassword").val() == "") {
            showWarningMessage("Atención", "El campo Contraseña no puede estar vacío.");
            return false;
        } else if ($("#editUserConfirm").val() == "") {
            showWarningMessage("Atención", "El campo Contraseña(Confirmación) no puede estar vacío.");
            return false;
        }

        if ($("#editUserPassword").val() != $("#editUserConfirm").val()) {
            showWarningMessage("Atención", "La Contraseña y su Confirmación no coinciden.");
            return false;
        }

        if ($("#editUserEmail").val() != "") {
            var regexEmail = /^([a-zA-Z0-9_\.\-\+])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;
            if (!regexEmail.test($("#editUserEmail").val())) {
                showWarningMessage("Atención", "El campo Email no tiene el formato correcto.");
                return false;
            }
        }

        return true;
    };
});