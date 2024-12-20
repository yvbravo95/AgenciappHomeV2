$(document).ready(function () {

    //Si se cambia el tipo de tramite o la agencia
    $('#selectTramites').on('change', function () {
        var tramite = $(this).val();
        var agency = $('#selectAgency').val();
        location.href = "/servicioxCobrars/PendienteCobro?agency=" + agency + "&tramite=" + tramite;

    });
    $('#selectAgency').on('change', function () {
        var agency = $(this).val();
        var tramite = $('#selectTramites').val();
        location.href = "/servicioxCobrars/PendienteCobro?agency=" + agency + "&tramite=" + tramite;
    });
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

            facturar();
            
        }
    });

    function facturar() {
        if (cantSelect == 0) {
            //Ocultar
            $('#cobro').hide();
        } else {

            //Mostrar 
            selectedIds = new Array;

            $(".order-select").each(function (i, e) {
                if (e.checked) {
                    if (e.value != "all") {
                        selectedIds.push($(e).val());
                    }
                }
            });
            $.ajax({
                async: false,
                type: "POST",
                url: "/servicioxCobrars/getValueCobro",
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
                    }
                    else{
                        toastr.error(response.msg);
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
        if ($(this)[0].checked) {
            $('.order-select').prop('checked', true)
            cantSelect = $('.order-select').length - 1;
            facturar();
        } else {
            $('.order-select').prop('checked', false)
            cantSelect = 0;
            facturar();
        }
    });

    var calc = function() {
        var valorordenes = parseFloat($('#valorOrdenes').html());
        var refunds = parseFloat($('#refunds').val());
        var otrosPagos = parseFloat($('#otrospagos').val());
        var totalpagar = valorordenes - refunds + otrosPagos;
        $('#totalapagar').val(totalpagar.toFixed(2));

    }

    $("#refunds, #otrospagos").on("keyup", calc).on("change", calc);

    //Facturar
    $('#Facturar').on('click', function () {
        $(this).prop('disabled', true);
        

        var valorOrdenes = parseFloat($('#valorOrdenes').html());
        var totalapagar = parseFloat($('#totalapagar').val());
        var refunds = parseFloat($('#refunds').val());
        var refundsdetalle = $('#refundsdetalle').val();
        var otrospagos = parseFloat($('#otrospagos').val());
        var otrospagosdetalle = $('#otrospagosdetalle').val();
        var conceptofacturar = $('#conceptofacturar').val();
        var transferirDia = $('#transferirdia').val();

        var data = [
            valorOrdenes, // 0
            totalapagar,// 1
            refunds,// 2
            refundsdetalle, //3
            otrospagos,// 4
            otrospagosdetalle, //5
            conceptofacturar, //6
            transferirDia,//7
            JSON.stringify(selectedIds)//8
        ]

        $.ajax({
            async: false,
            type: "POST",
            dataType: 'json',
            contentType: 'application/json',
            url: "/Facturas/facturar",
            data: JSON.stringify(data),
            beforeSend: function () {
                //Bloqueo la pantalla del usuario
                $.blockUI({
                    message: '<div class="ft-refresh-cw icon-spin font-medium-2"></div>',
                    overlayCSS: {
                        backgroundColor: '#FFF',
                        opacity: 0.8,
                        cursor: 'wait'
                    },
                    css: {
                        border: 0,
                        padding: 0,
                        backgroundColor: 'transparent'
                    }
                });
            },
            success: function (response) {
                if (response != "fails") {

                    //Envio por email la factura
                    enviarfactura(response.id, response.email);

                    location.href = "/Facturas/Index"
                }
                else {
                    toastr.error("Error al facturar", "Error");
                    $.unblockUI();
                    $(this).prop('disabled', false);
                }
            },
            error: function () {
                toastr.error("Error al facturar", "Error");
                $.unblockUI();
                $(this).prop('disabled', false);
            },
            timeout: 10000,
        });
    });

    function enviarfactura(id, email) {
        var datos = [
            id,
            email,
        ];

        $.ajax({
            type: "POST",
            url: "/Facturas/EnviarFactura",
            data: JSON.stringify(datos),
            dataType: 'json',
            contentType: 'application/json',
            async: false,
            success: function (response) {
               
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    }

    var selectGuides = $('#selectGuide');
    selectGuides.select2({
        placeholder: "Seleccione las guias a filtrar"
    });
    $('#filerByGuide').on('click', function () {
        var guides = $(selectGuides).val();
        var url = `/servicioxCobrars/PendienteCobro?agency=${agency}&tramite=${tramite}`;
        if (guides) {
            guides.map((element, i) => {
                url += `&guideNumbers[${i}]=${element}`;
            })
        }

        location.href = url;
    })

});



