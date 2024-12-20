$(document).ready(function () {
    
    $(".hide-search-clientState, .hide-search-clientState").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Estado",
    });

    $(".hide-search-clientCity, .hide-search-clientCity").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Ciudad",
    });
    
    $(".select2-container--default").attr("style", "width: 100%;");

    $("#editarCliente").click(function () {
        if (validateCliente()) {
            var okConfirm = function () {
                var source = [
                    $('#clientId').val(),
                    $('#Name').val(),
                    $('#Lastname').val(),
                    $('#Email').val(),
                    $('#Phone').val(),
                    $('#Address').val(),
                    $('#city').val(),
                    $('#state').val(),
                    $('#Zip').val(),
                    $('#phoneCuba').val(),
                    $('#acepto').is(':checked'),
                    $('#ID').val(),
                    $("#fechaNac").val(),//12
                    $('#Nota').val(), //13
                    $('#Conflictivo').is(':checked'), //14
                    
                ];

                $.ajax({
                    type: "POST",
                    url: "/Clients/EditClient",
                    data: JSON.stringify(source),
                    dataType: 'json',
                    contentType: 'application/json',
                    async: false,
                    success: function (response) {
                        if (!response.success) {
                            toastr.error(response.msg);
                        }
                        else {
                            var url = $('[name="url"]').val();
                            if (url != "") {
                                window.location = url + "?msg= El Cliente " + $('#Name').val() + " " + $('#Lastname').val() + " ha sido editado con éxito";

                            }
                            else {
                                window.location = "/Clients?msg=successEdit&nombre=" + $('#Name').val() + " " + $('#Lastname').val();
                            }
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
            confirmationMsg("¿Está seguro que desea editar este cliente?", "", okConfirm);
        }
    });

    var validateCliente = function () {
        if ($("#Name").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        }
        if ($("#Lastname").val() == "") {
            showWarningMessage("Atención", "El campo Apellidos no puede estar vacío.");
            return false;
        }
        if ($("#Phone").val() == "") {
            showWarningMessage("Atención", "El campo Móvil no puede estar vacío.");
            return false;
        }
        if ($("#Email").val() != "") {
            var regexEmail = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
            if (!regexEmail.test($("#Email").val())) {
                showWarningMessage("Atención", "El campo Email no tiene el formato correcto.");
                return false;
            }
        }
        return true;
    };
});