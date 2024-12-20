$(document).ready(function () {

    $(".select2-placeholder-selectAgency").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Agencia",
    });

    $(".hide-search-newOfficeState").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Estado",
    });

    $(".hide-search-newOfficeCity").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Ciudad",
    });
    
    $(".select2-container--default").attr("style", "width: 100%;");

    $("#guardarNuevaOficina").click(function () {
        if (validateNuevaOficina()) {
            var source = [
                $('#nuevoOfficeName').val(),
                $('.select2-placeholder-selectAgency').val(),
                $('#nuevoOfficePhone').val(),
                $('#nuevoOfficeAddress').val(),
                $('.hide-search-newOfficeCity').val(),
                $('.hide-search-newOfficeState').val(),
                $('#nuevoOfficeZip').val(),
            ];

            $.ajax({
                type: "POST",
                url: "/Offices/AddOffice",
                data: JSON.stringify(source),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function () {
                    window.location = "/Offices?msg=success&nombre=" + $('#nuevoOfficeName').val();
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

    var validateNuevaOficina = function () {
        if ($("#nuevoOfficeName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($("#nuevoOfficePhone").val() == "") {
            showWarningMessage("Atención", "El campo Teléfono no puede estar vacío.");
            return false;
        }

        if ($("#nuevoOfficePhone").val() != "") {
            var regexPhone = /^(\+1 )?((\(\d{3}\))|\d{3})[ -]\d{3}[ -]\d{4}$/;
            if (!regexPhone.test($("#nuevoOfficePhone").val())) {
                showWarningMessage("Atención", "El campo Teléfono no tiene el formato correcto. Formatos aceptados: +1 (123) 456-7899; (123) 456-7899; 123-456-7899; 123 456 7899.");
                return false;
            }
        }

        return true;
    };
});