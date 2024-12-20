var minDate, maxDate;
var index = $("#filtrofecha").data("column");

if (tramite == "Reserva")
    index = $("#filtrofechaReserva").data("column");

$.fn.dataTable.ext.search.push(
    function (settings, data, dataIndex) {
        var min = minDate?.val() ?new Date(minDate?.val()) : null;
        var max = maxDate?.val() ? new Date(maxDate?.val()) : null;
        var date = new Date(data[index]);
        if (
            (!min && !max) ||
            (!min && date <= max) ||
            (min <= date && !max) ||
            (min <= date && date <= max)
        ) {
            return true;
        }
        return false;
    }
);


$(document).ready(function () {
    $(".date-input").mask("99/99/9999")

    //Si se cambia el tipo de tramite o la agencia
    $('#selectTramites').on('change', function () {
        var tramite = $(this).val();
        var agency = $('#selectAgency').val();
        location.href = "/serviciosxPagar/Details?agency=" + agency + "&tramite=" + tramite;
    });

    $('#selectAgency').on('change', function () {
        var agency = $(this).val();
        var tramite = $('#selectTramites').val();
        location.href = "/serviciosxPagar/Details?agency=" + agency + "&tramite=" + tramite;
    });

    var selectedIds = new Array;
    $(".order-select").on("change", function () {
        if ($(this).val() != "all") {
            // Para desabilitar el check todo si se selecciona algun otro check
            $('#checkall').prop('checked', false);

            if ($(this)[0].checked) {
                selectedIds.push($(this).val())
            } else {
                const index = selectedIds.indexOf($(this).val());
                if (index > -1) {
                    selectedIds.splice(index, 1);
                }
            }
            bill();
        }
    });

    function bill() {
        if (selectedIds.length == 0) {
            //Ocultar
            $('#cobro').hide();
        } else {
            $.ajax({
                async: false,
                type: "POST",
                url: "/ServiciosxPagar/getValueCobro",
                data: {
                    ids: selectedIds,
                },
                success: function (response) {
                    if (response.success) {
                        var valortotal = parseFloat(response.data.total)
                        $('#valorOrdenes').html(valortotal.toFixed(2));
                        $('#canttotal').html(selectedIds.length);
                        $('#totalapagar').val(valortotal.toFixed(2));
                        $('#totalapagar').attr('max', valortotal.toFixed(2));
                        //Muestro el contenedor
                        $('#cobro').show();
                        $('#pesoCubiq').html(response.data.pesoCubiq);
                        $('#pesoMaritimo').html(response.data.pesoMaritimo);
                    }
                },
                error: function () {
                    toastr.error("Error al intentar cargar valores", "Error");
                },
                timeout: 10000,
            });
        }
    };

    // Para seleccionar todo
    $('#checkall').on('change', function () {
        selectedIds = new Array;
        if ($(this)[0].checked) {
            $('.order-select').prop('checked', true)
            var elements = $('.order-select');
            $.each(elements, function(a,element){
                if($(element).val() != "all"){
                    selectedIds.push($(element).val());
                }
            })
            bill();
        } else {
            $('.order-select').prop('checked', false)
            bill();
        }
    });

    var calc = function () {
        var valorordenes = parseFloat($('#valorOrdenes').html());
        var refunds = parseFloat($('#refunds').val());
        var otrosPagos = parseFloat($('#otrospagos').val());
        var totalpagar = valorordenes - refunds + otrosPagos;
        $('#totalapagar').val(totalpagar.toFixed(2));

    }

    $("#refunds, #otrospagos").on("keyup", calc).on("change", calc);

    //Facturar
    $('#Bill').on('click', function () {
        var valorOrdenes = parseFloat($('#valorOrdenes').html());
        var totalapagar = parseFloat($('#totalapagar').val());
        var refunds = parseFloat($('#refunds').val());
        var refundsdetalle = $('#refundsdetalle').val();
        var otrospagos = parseFloat($('#otrospagos').val());
        var otrospagosdetalle = $('#otrospagosdetalle').val();
        var data = [
            valorOrdenes, // 0
            totalapagar,// 1
            refunds,// 2
            refundsdetalle, //3
            otrospagos,// 4
            otrospagosdetalle, //5
            JSON.stringify(selectedIds),//6
            isProductShipping, //7
        ]

        $.ajax({
            async: true,
            type: "POST",
            dataType: 'json',
            contentType: 'application/json',
            url: "/Bills/Create",
            data: JSON.stringify(data),
            headers: {
                RequestVerificationToken: $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                if (response != "fails") {
                    location.href = "/Bills";
                }
                else {
                    toastr.error("Error al facturar", "Error");
                }
                $.unblockUI();
            },
            error: function () {
                toastr.error("Error", "Error");
                $.unblockUI();
            },
        });
    });

    $.fn.editable.defaults.mode = 'inline';
    $(function () {
        $(".editable").editable({
            type: 'text',
            validate: function (value) {
                value = value.replace(",", ".");
                if ($.isNumeric(value) == '') {
                    return "Solo se permiten números"
                }
            },
            send: 'always',
            params: function (params) {
                params.id = $(this).data("id");
                params.value = params.value.replace(",", ".");
                params.pk = params.pk.replace(",", ".");
                return params;
            },
            ajaxOptions: {
                type: 'post'
            },
            success: function (response, newValue) {
                bill();
                $("#sum").html((parseFloat($("#sum").html().replace(",", ".")) + response['diff']).toFixed(2));
            },
        });
    });

    var table = $("#bill_table").DataTable({
        dom: "tri",
        paging: false
    });

    $("#search").on('keyup', function () {
        table.search("(" + $(this).val() + ")", true, true);
        table.draw();
    });

    $(".searchColumn").on('keyup', function () {
        var i = $(this).data('column');
        table.column(i).search("(" + $(this).val() + ")", true, true);
        table.draw();
    });

    minDate = $('#min');
    maxDate = $('#max');

    $(".date").datetimepicker({
        format: 'MM/DD/YYYY',
        viewMode: "years",
        widgetPositioning: {
            horizontal: 'auto',
            vertical: 'bottom'
        },
        extraFormats: ["YYYY-MM-DD"]
    }).on("dp.change", function (e) {
        var mirrorId = $(this).data("input");
        if (mirrorId) {
            var mirror = $("#" + mirrorId);
            mirror.val(new Date(e.date).toISOString().substr(0, 10)).trigger("change");
        }
    });

    $("#datefilter").on("click", () => {
        table.draw();
    })
});



