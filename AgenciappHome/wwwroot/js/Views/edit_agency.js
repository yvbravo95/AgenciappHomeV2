$(document).ready(function () {

    $(".hide-search-agencyType").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Tipo",
    });

    $(".hide-search-agencyState").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Estado",
    });

    $(".hide-search-agencyCity").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Ciudad",
    });
    
    $(".select2-container--default").attr("style", "width: 100%;");

    $("#editarAgencia").click(function () {
        if (validateAgency()) {
            var okConfirm = function () {
                var source = [
                    $('#agencyId').val(),
                    $('#agencyName').val(),
                    $('#agencyLegalName').val(),
                    $('.hide-search-agencyType').val(),
                    $('#agencyPhone').val(),
                    $('#agencyAddress').val(),
                    $('.hide-search-agencyCity').val(),
                    $('.hide-search-agencyState').val(),
                    $('#agencyZip').val(),
                ];

                $.ajax({
                    type: "POST",
                    url: "/Agencies/EditAgency",
                    data: JSON.stringify(source),
                    dataType: 'json',
                    contentType: 'application/json',
                    async: false,
                    success: function () {
                        window.location = "/Agencies?msg=successEdit&nombre=" + $('#agencyName').val();
                    },
                    failure: function (response) {
                        showErrorMessage("ERROR", response.responseText);
                    },
                    error: function (response) {
                        showErrorMessage("ERROR", response.responseText);
                    }
                });
            }
            confirmationMsg("¿Está seguro que desea editar esta agencia?", "", okConfirm);
        }
    });

    var validateAgency = function () {
        if ($("#agencyName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($("#agencyLegalName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre Legal no puede estar vacío.");
            return false;
        } else if ($(".hide-search-agencyType").val() == "") {
            showWarningMessage("Atención", "El campo Tipo no puede estar vacío.");
            return false;
        } else if ($("#agencyPhone").val() == "") {
            showWarningMessage("Atención", "El campo Teléfono no puede estar vacío.");
            return false;
        }

        if ($("#agencyPhone").val() != "") {
            var regexPhone = /^(\+1 )?((\(\d{3}\))|\d{3})[ -]\d{3}[ -]\d{4}$/;
            if (!regexPhone.test($("#agencyPhone").val())) {
                showWarningMessage("Atención", "El campo Teléfono no tiene el formato correcto. Formatos aceptados: +1 (123) 456-7899; (123) 456-7899; 123-456-7899; 123 456 7899.");
                return false;
            }
        }

        return true;
    };
});