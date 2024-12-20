$(document).ready(function () {
    
    $("#provincia").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Provincia",
    });

    

    $("#editarContacto").click(function () {
        if (validateContacto()) {
            var okConfirm = function () {
                var source = [
                    $('#contactId').val(),
                    $('#contactName').val(),
                    $('#contactLastName').val(),
                    $('#contactPhoneMovil').val(),
                    $('#contactPhoneHome').val(),
                    $('#contactDir').val(),
                    $('#provincia').val(),
                    $('#municipio').val(),
                    $('#reparto').val(),
                    $('#CI').val(),

                ];

                $.ajax({
                    type: "POST",
                    url: "/Contacts/EditContact",
                    data: JSON.stringify(source),
                    dataType: 'json',
                    contentType: 'application/json',
                    async: false,
                    success: function () {
                        var url = $('[name="url"]').val();

                        if (url != "") {
                            window.location = url + "?msg= El Contacto " + $('#contactName').val() + " " + $('#contactLastName').val() + " ha sido editado con éxito";

                        }
                        else {
                            window.location = "/Contacts?msg=successEdit&nombre=" + $('#contactName').val() + " " + $('#contactLastName').val();

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
            confirmationMsg("¿Está seguro que desea editar este contacto?", "", okConfirm);
        }
    });

    var validateContacto = function () {
        if ($("#contactName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($("#contactLastName").val() == "") {
            showWarningMessage("Atención", "El campo Apellidos no puede estar vacío.");
            return false;
        } else if ($("#contactPhoneMovil").val() == "" && $("#contactPhoneHome").val() == "") {
            showWarningMessage("Atención", "Debe introducir al menos un teléfono de contacto.");
            return false;
        } else if ($('#CI').val().length > 0) {
            if ($('#CI').val().length != 11) {
                showWarningMessage("Atención", "El carnet de identidad debe tener 11 dígitos");
                return false;
            }
        }
        else if ($("#contactDir").val() == "") {
            showWarningMessage("Atención", "El campo Dirección no puede estar vacío.");
            return false;
        } else if ($("provincia").val() == "") {
            showWarningMessage("Atención", "El campo Provincia no puede estar vacío.");
            return false;
        } else if ($("#municipio").val() == "") {
            showWarningMessage("Atención", "El campo Municipio no puede estar vacío.");
            return false;
        } else if ($("#reparto").val() == "") {
            showWarningMessage("Atención", "El campo Reparto no puede estar vacío.");
            return false;
        }

        /*
        if ($("#contactPhoneMovil").val() != "") {
            var regexPhoneMovil = /^(\+53 )?5\d{3}[ -]?\d{2}[ -]?\d{2}$/;
            if (!regexPhoneMovil.test($("#contactPhoneMovil").val())) {
                showWarningMessage("Atención", "El campo Teléfono Móvil no tiene el formato correcto. Formatos aceptados: +53 5xxxxxxx; 5xxx xx xx; 5xxx-xx-xx.");
                return false;
            }
        }

        if ($("#contactPhoneHome").val() != "") {
            var regexPhoneMovil = /^(\+53 )?((\d{1}[ ]?\d{3})|(\d{2}[ ]?\d{2}))[ -]?\d{2}[ -]?\d{2}$/;
            if (!regexPhoneMovil.test($("#contactPhoneHome").val())) {
                showWarningMessage("Atención", "El campo Teléfono Casa no tiene el formato correcto. Formatos aceptados: +53 7xxxxxxx; +53 xx xxxxxx; 7xxx-xx-xx; xx xxxxxx.");
                return false;
            }
        }
        */

        return true;
    };

    $('#contactPhoneMovil').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });

    $('#contactPhoneHome').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });
});