$(document).ready(function () {
    var cantSelect = 0;
    var selectedIds = new Array;

    $(document).on("change", ".order-select", function () {
        if ($(this)[0].checked) {
            cantSelect++;
            selectedIds.push($(this).val());
        } else {
            cantSelect--;
            selectedIds.splice($.inArray($(this).val(), selectedIds), 1);
        }

        if (cantSelect == 0) {
            $('#btnDespachar').hide();
        } else {
            $('#btnDespachar').show();
        }
    });


    $("#btnDespachar").on('click', function () {
        $('#modalDespacho').modal('show');
    });
    $('#btnModalDespacho').on('click', function () {
        var distributor = $('#selectDistributor').val();

        $.ajax({
            async: true,
            method: "POST",
            url: "/EmpleadoCuba/Distribuir",
            data: {
                orders: selectedIds,
                userid: distributor
            },
            beforeSend: function () {
                $.blockUI();
            }, 
            success: function (response) {
                if (response) {
                    location.href = "/EmpleadoCuba/OrdenesEnviadas?msg=" + response.msg;
                }
                else {
                    toast.error(response.msg);
                }
                $.unblockUI();
            },
            error: function (response) {
                toastr.error("No se ha podido despachar");
                $.unblockUI();
            }
           
        })
    });
});