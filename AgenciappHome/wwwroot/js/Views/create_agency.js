$(document).ready(function () {

    $(".hide-search-newAgencyType").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Tipo",
    });

    $(".hide-search-newAgencyState").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Estado",
    });

    $(".hide-search-newAgencyCity").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Ciudad",
    });
    
    $(".select2-container--default").attr("style", "width: 100%;");

    $("#guardarNuevaAgencia").click(function () {
        if (validateNuevaAgencia()) {
            var source = [
                $('#nuevoAgencyName').val(),
                $('#nuevoAgencyLegalName').val(),
                $('.hide-search-newAgencyType').val(),
                $('#nuevoAgencyPhone').val(),
                $('#nuevoAgencyAddress').val(),
                $('.hide-search-newAgencyCity').val(),
                $('.hide-search-newAgencyState').val(),
                $('#nuevoAgencyZip').val(),
            ];

            $.ajax({
                type: "POST",
                url: "/Agencies/AddAgency",
                data: JSON.stringify(source),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function () {
                    window.location = "/Agencies?msg=success&nombre=" + $('#nuevoAgencyName').val();
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

    var validateNuevaAgencia = function () {
        if ($("#nuevoAgencyName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($("#nuevoAgencyLegalName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre Legal no puede estar vacío.");
            return false;
        } else if ($(".hide-search-newAgencyType").val() == "") {
            showWarningMessage("Atención", "El campo Tipo no puede estar vacío.");
            return false;
        } else if ($("#nuevoAgencyPhone").val() == "") {
            showWarningMessage("Atención", "El campo Teléfono no puede estar vacío.");
            return false;
        }

        if ($("#nuevoAgencyPhone").val() != "") {
            var regexPhone = /^(\+1 )?((\(\d{3}\))|\d{3})[ -]\d{3}[ -]\d{4}$/;
            if (!regexPhone.test($("#nuevoAgencyPhone").val())) {
                showWarningMessage("Atención", "El campo Teléfono no tiene el formato correcto. Formatos aceptados: +1 (123) 456-7899; (123) 456-7899; 123-456-7899; 123 456 7899.");
                return false;
            }
        }

        return true;
    };
});