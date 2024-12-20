$(document).ready(function () {
    
    $(".hide-search-clientState, .hide-search-newClientState").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Estado",
    });

    $(".hide-search-clientCity, .hide-search-newClientCity").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Ciudad",
    });
    
    $(".select2-container--default").attr("style", "width: 100%;");

    $("#guardarNuevoCliente").click(function () {
        if (validateNuevoCliente()) {
            var source = [
                $('#nuevoClientName').val(),
                $('#nuevoClientLastName').val(),
                $('#nuevoClientEmail').val(),
                $('#nuevoClientMovil').val(),
                $('#nuevoClientAddress').val(),
                $('.hide-search-newClientCity').val(),
                $('.hide-search-newClientState').val(),
                $('#nuevoClientZip').val(),
            ];

            $.ajax({
                type: "POST",
                url: "/Clients/AddClient",
                data: JSON.stringify(source),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function (response) {
                    window.location = "/Clients/Tramites/" + response.idClient;
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

    var validateNuevoCliente = function () {
        if ($("#nuevoClientName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($("#nuevoClientLastName").val() == "") {
            showWarningMessage("Atención", "El campo Apellidos no puede estar vacío.");
            return false;
        } else if ($("#nuevoClientMovil").val() == "") {
            showWarningMessage("Atención", "El campo Teléfono no puede estar vacío.");
            return false;
        }


        if ($("#nuevoClientEmail").val() != "") {
            var regexEmail = /^([a-zA-Z0-9_\.\-\+])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;
            if (!regexEmail.test($("#nuevoClientEmail").val())) {
                showWarningMessage("Atención", "El campo Email no tiene el formato correcto.");
                return false;
            }
        }

        return true;
    };
});