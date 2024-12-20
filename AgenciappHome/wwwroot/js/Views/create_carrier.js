$(document).ready(function () {
    
    $("#guardarNuevoCarrier").click(function () {
        if (validateNuevoCarrier()) {
            
            var source = [
                $('#nuevoCarrierName').val(),
                $('#nuevoCarrierLastName').val(),
                $('#nuevoCarrierPhone').val(),
                $('#nuevoCarrierEmail').val(),
                $('#nuevoCarrierAddress').val(),
                $('#country').val(),
                $('#city').val(),
                $('#nuevoCarrierZip').val(),
                $('#agency').val(),
                $("#ClientId").val()
            ];

            $.ajax({
                type: "POST",
                url: "/Carriers/AddCarrier",
                data: JSON.stringify(source),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function () {
                    window.location = "/Carriers?msg=success&nombre=" + $('#nuevoCarrierName').val() + " " + $('#nuevoCarrierLastName').val();
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

    var validateNuevoCarrier = function () {
        if ($("#nuevoCarrierName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($("#nuevoCarrierLastName").val() == "") {
            showWarningMessage("Atención", "El campo Apellidos no puede estar vacío.");
            return false;
        }

        return true;
    };


    $(".select2-placeholder-selectClient").select2({
        placeholder: "Buscar cliente por teléfono, nombre o apellido",
        val: null,
        ajax: {
            type: 'POST',
            dataType: "json",
            delay: 500,
            url: '/Clients/findClient',
            data: function (params) {
                var query = {
                    search: params.term,
                }

                // Query parameters will be ?search=[term]&type=public
                return query;
            },
            processResults: function (data) {

                // Transforms the top-level key of the response object from 'items' to 'results'
                return {
                    results: $.map(data, function (obj) {

                        return { id: obj.clientId, text: obj.fullData, conflictivo: obj.conflictivo };
                    })
                };
            }
        }
    });

    $("#ClientId").on('change', function () {
        var value = $(this).val();
        selectedClient = value;
        $.ajax({
            type: "POST",
            url: "/Clients/GetClient",
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(value),
            async: false,
            success: function (data) {
                //Datos del Cliente en Authorization Card
                $('#nuevoCarrierName').val(data.name);
                $('#nuevoCarrierLastName').val(data.lastName);
                $('#nuevoCarrierPhone').val(data.movil);
                $('#nuevoCarrierEmail').val(data.email);
                $('#country').val(data.country);
                $('#city').val(data.city);
                $('#nuevoCarrierAddress').val(data.calle);
                $("#nuevoCarrierZip").val(data.zip);
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            }
        });
    });

});