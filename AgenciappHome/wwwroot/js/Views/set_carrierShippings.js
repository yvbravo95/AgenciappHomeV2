$(document).ready(function () {

    $(".select2-placeholder-selectCarrier").select2({
        placeholder: "Sin Carrier",
        val: null
    });

    $(".select2-placeholder-selectDistributor").select2({
        placeholder: "Seleccione un distribuidor",
        val: null
    });

    $(".select2-container--default").attr("style", "width: 100%;");

    $('#CarrierId').on('change', function () {
        var carrierId = $(this).val();
        $.ajax({
            type: "POST",
            url: "/Shippings/getCostoPasajeCarrier",
            data: {
                carrierId: carrierId
            },
            
            async: false,
            success: function (response) {
                $('#CostoPasaje').val(response);
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