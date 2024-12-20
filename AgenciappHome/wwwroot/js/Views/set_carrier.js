$(document).ready(function () {

    $(".select2-placeholder-selectCarrier").select2({
        placeholder: "Sin Carrier",
        val: null
    });

    $(".select2-container--default").attr("style", "width: 100%;");

    $("#saveShippingCarrier").click(function () {
        var okConfirm = function () {
            var datos = [
                $("#packingId").val(), // 0
                $(".select2-placeholder-selectCarrier").val(), //1
                $("#no_vuelo").val(),//2
                $("#fecha").val(),//3
                $("#costoPasaje").val(), //4
                $("#gastoCuba").val(), //5
                $("#gastoUsa").val(), //6
                $("#NotaEnvio").val(),//7
            ];

            $.ajax({
                type: "POST",
                url: "/Shippings/PutCarrier",
                data: JSON.stringify(datos),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function () {
                    document.location = "/Shippings?msg=successCarrier&noEquipaje=" + $("#no_equipaje").html();
                },
                failure: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                },
                error: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                }
            });
        };
        if( $(".select2-placeholder-selectCarrier").val() != "" &&  $("#no_vuelo").val() != "" && $("#fecha").val() != "")
        {
            confirmationMsg("¿Está seguro que desea asignar este carrier?", "", okConfirm);
        }
        else{
            showErrorMessage("Error", "Debe llenar todos los campos");
        }
    });

    $('[name = "selectCarrier"]').on('change', function () {
        var carrierId = $(this).val();
        $.ajax({
            type: "POST",
            url: "/Shippings/getCostoPasajeCarrier",
            data: {
                carrierId: carrierId
            },
            
            async: false,
            success: function (response) {
                $('#costoPasaje').val(response);
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    });
});