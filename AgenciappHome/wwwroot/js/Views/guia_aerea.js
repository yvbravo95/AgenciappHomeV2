$(document).ready(function () {
    var cantSelect = 0;
    var selectedIds;
    $(".order-select").on("change", function () {
        if ($(this).val() != "all") {
            // Para desabilitar el check todo si se selecciona algun otro check
            $('#checkall').prop('checked', false);

            if ($(this)[0].checked) {
                cantSelect++;
            } else {
                cantSelect--;
            }
            count();
        }
    });

    function count() {
        if (cantSelect == 0) {
            $('#cobro').hide();
        } 
        else {
            //Mostrar 
            selectedIds = [];
            $(".order-select").each(function (i, e) {
                if (e.checked) {
                    if (e.value != "all") {
                        selectedIds.push($(e).val());
                    }
                }
            });
            var valortotal = 0;
            for (let index = 0; index < selectedIds.length; index++) {
                const id = selectedIds[index];
                valortotal += parseFloat($("#" + id).html().replace(",", "."));
            }
            $("#total").val(cantSelect);
            $("#pesototal").val(valortotal.toFixed(2));
            $('#cobro').show();
        }
    }

    // Para seleccionar todo
    $('#checkall').on('change', function () {
        if ($(this)[0].checked) {
            $('.order-select').prop('checked', true)
            cantSelect = $('.order-select').length - 1;
            count();
        } else {
            $('.order-select').prop('checked', false)
            cantSelect = 0;
            count();
        }
    });

    $('#Crear').on('click', function () {
        if(!$("#awb").val())
        {
            showWarningMessage("Atención", "El campo Guía Aérea No. no puede estar vacío.");
            return;
        }
        selectedIds = [];
            $(".order-select").each(function (i, e) {
                if (e.checked) {
                    if (e.value != "all") {
                        selectedIds.push($(e).val());
                    }
                }
            });

        var data = [
            $("#awb").val(),
            selectedIds
        ];

        $.ajax({
            async: false,
            type: "POST",
            dataType: 'json',
            contentType: 'application/json',
            url: "/OrderCubiq/GuiaAerea",
            data: JSON.stringify(data),
            headers: {
                RequestVerificationToken: $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response != "fails") {
                    location.href = "/OrderCubiq/IndexGuia";
                }
                else {
                    toastr.error("Error al crear guía aérea", "Error");
                }
            },
            error: function () {
                toastr.error("Error", "Error");
            },
            timeout: 10000,
        });
    });

    var table = $("#bill_table").DataTable({
        searching: true,
        dom: "tri",
        paging: false
    });
    $("#search").on('keyup', function () {
        table.search($(this).val());
        table.draw();
    });
});



