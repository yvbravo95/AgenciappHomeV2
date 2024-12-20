$(document).ready(function () {
    
    $("#provincia").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Provincia",
    });

   

    $("#guardarNuevoContacto").click(function () {
        if (validateNuevoContacto()) {
            var source = [
                "",
                $('#nuevoContactName').val(),
                $('#nuevoContactLastName').val(),
                $('#nuevoContactPhoneMovil').val(),
                $('#nuevoContactPhoneHome').val(),
                $('#nuevoContactDir').val(),
                $('#provincia').val(),
                $('#municipio').val(),
                $('#reparto').val(),
                $('#CI').val(),
            ];

            $.ajax({
                type: "POST",
                url: "/Contacts/AddContact",
                data: JSON.stringify(source),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function () {
                    window.location = "/Contacts?msg=success&nombre=" + $('#nuevoContactName').val() + " " + $('#nuevoContactLastName').val();
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

    var validateNuevoContacto = function () {
        if ($("#nuevoContactName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($("#nuevoContactLastName").val() == "") {
            showWarningMessage("Atención", "El campo Apellidos no puede estar vacío.");
            return false;
        } else if ($("#nuevoContactPhoneMovil").val() == "" && $("#nuevoContactPhoneHome").val() == "") {
            showWarningMessage("Atención", "Debe introducir al menos un teléfono de contacto.");
            return false;
        } else if ($('#CI').val().length > 0) {
            if ($('#CI').val().length != 11) {
                showWarningMessage("Atención", "El carnet de identidad debe tener 11 dígitos");
                return false;
            }
        }
        else if ($("#nuevoContactDir").val() == "") {
            showWarningMessage("Atención", "El campo Dirección no puede estar vacío.");
            return false;
        } else if ($("#provincia").val() == "") {
            showWarningMessage("Atención", "El campo Provincia no puede estar vacío.");
            return false;
        } else if ($("#Municipio").val() == "") {
            showWarningMessage("Atención", "El campo Municipio no puede estar vacío.");
            return false;
        } else if ($("#reparto").val() == "") {
            showWarningMessage("Atención", "El campo Reparto no puede estar vacío.");
            return false;
        }

        /*
        if ($("#nuevoContactPhoneMovil").val() != "") {
            var regexPhoneMovil = /^(\+53 )?5\d{3}[ -]?\d{2}[ -]?\d{2}$/;
            if (!regexPhoneMovil.test($("#nuevoContactPhoneMovil").val())) {
                showWarningMessage("Atención", "El campo Teléfono Móvil no tiene el formato correcto. Formatos aceptados: +53 5xxxxxxx; 5xxx xx xx; 5xxx-xx-xx.");
                return false;
            }
        }

        if ($("#nuevoContactPhoneHome").val() != "") {
            var regexPhoneMovil = /^(\+53 )?((\d{1}[ ]?\d{3})|(\d{2}[ ]?\d{2}))[ -]?\d{2}[ -]?\d{2}$/;
            if (!regexPhoneMovil.test($("#nuevoContactPhoneHome").val())) {
                showWarningMessage("Atención", "El campo Teléfono Casa no tiene el formato correcto. Formatos aceptados: +53 7xxxxxxx; +53 xx xxxxxx; 7xxx-xx-xx; xx xxxxxx.");
                return false;
            }
        }
        */

        return true;
    };

    $('#nuevoContactPhoneMovil').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });

    $('#nuevoContactPhoneHome').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });
    
});