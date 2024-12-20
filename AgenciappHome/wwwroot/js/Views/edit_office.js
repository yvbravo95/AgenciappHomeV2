$(document).ready(function () {

    $(".select2-placeholder-selectAgency").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Agencia",
    });

    $(".hide-search-officeState").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Estado",
    });

    $(".hide-search-officeCity").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Ciudad",
    });
    
    $(".select2-container--default").attr("style", "width: 100%;");

    $("#editarOficina").click(function () {
        if (validateoffice()) {
            var okConfirm = function () {
                var source = [
                    $('#officeId').val(),
                    $('#officeName').val(),
                    $('#officePhone').val(),
                    $('.select2-placeholder-selectAgency').val(),
                    $('#officeAddress').val(),
                    $('.hide-search-officeCity').val(),
                    $('.hide-search-officeState').val(),
                    $('#officeZip').val(),
                ];

                $.ajax({
                    type: "POST",
                    url: "/Offices/EditOffice",
                    data: JSON.stringify(source),
                    dataType: 'json',
                    contentType: 'application/json',
                    async: false,
                    success: function () {
                        window.location = "/Offices?msg=successEdit&nombre=" + $('#officeName').val();
                    },
                    failure: function (response) {
                        showErrorMessage("ERROR", response.responseText);
                    },
                    error: function (response) {
                        showErrorMessage("ERROR", response.responseText);
                    }
                });
            }
            confirmationMsg("¿Está seguro que desea editar esta oficina?", "", okConfirm);
        }
    });

    var validateoffice = function () {
        if ($("#officeName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($(".hide-search-officeType").val() == "") {
            showWarningMessage("Atención", "El campo Tipo no puede estar vacío.");
            return false;
        } else if ($("#officePhone").val() == "") {
            showWarningMessage("Atención", "El campo Teléfono no puede estar vacío.");
            return false;
        }

        if ($("#officePhone").val() != "") {
            var regexPhone = /^(\+1 )?((\(\d{3}\))|\d{3})[ -]\d{3}[ -]\d{4}$/;
            if (!regexPhone.test($("#officePhone").val())) {
                showWarningMessage("Atención", "El campo Teléfono no tiene el formato correcto. Formatos aceptados: +1 (123) 456-7899; (123) 456-7899; 123-456-7899; 123 456 7899.");
                return false;
            }
        }

        return true;
    };
});