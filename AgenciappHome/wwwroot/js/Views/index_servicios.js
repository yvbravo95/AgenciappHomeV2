
var cantSelect = 0;

$('[name="cancelar"]').on('click', function () {
    var servicioId = $(this).attr('data-id');
    $.ajax({
        async: true,
        type: "POST",
        url: "/Servicios/Index",
        data: {
            value: 6,
            id: servicioId
        },
        beforeSend: function () {
            $.blockUI();
        },
        success: function (response) {
            location.href = "/Servicios/Index?msg=El servicio ha sido cancelado"
        },
        error: function () {
            toastr.error("No se ha podido despachar", "Error");
            $.unblockUI();
        },
        timeout: 10000,
    });
});

// code here
$('#mayorista').select2({
    placeholder: "Mayorista a despachar",
    val: null,
});

// Para mostrar el boton de despacho al seleccionarse las iniciadas
$('[name="tab"]').on('click', function () {
    if ($(this).attr('data-type') == "completadas") {
        $('#despachar').show();
    }
    else {
        $('#despachar').hide();
    }
});

$('#despachar').on('click', function () {
    $('#modalDespachar').modal('show');
});

$("#creardespacho").click(function () {
    var date = $('#daterangeReport').val();
    var numerodespacho = $('#numerodespacho').val();
    var mayorista = $('#mayorista').val();
    if (numerodespacho != "" && mayorista != "") {
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/Servicios/despachar",
            data: {
                date: date,
                numerodespacho: numerodespacho,
                idmayorista: mayorista
            },
            success: function (response) {
                var aux = response.split('-');
                if (aux[0] == "error") {
                    toastr.error(aux[1]);
                }
                else {
                    location.reload();
                }
            },
            error: function () {
                toastr.error("No se ha podido despachar", "Error");
            },
            timeout: 10000,
        });
    }
    else {
        toastr.warning("Verifique que los campos número de despacho y mayorista no estén vacíos");

    }

});

$('[name = "tab"]').on('click', function () {
    $(".searchColumn").map((i, e) => {
        if ($(e).val() != "") {
            $(e).val("").trigger("change");
        }
    })
    $('[name = "tab"]').removeClass('active');
    $('.order-select').prop('checked', false)
    $('.checkalltramites').prop('checked', false)
    cantSelect = 0;
    $("#nextState").addClass("hidden");
    $(this).addClass('active');

})

$("#nextState").on("click", () => {
    var cState = $('[class="nav-link active"]').data("type");
    var nextState = "";
    switch (cState) {
        case "pendientes":
            nextState = (agency == agencyDCubaWashington) ? "consulado" : "completado"
            break;
        case "consulado":
            nextState = "completado";
            break;
        case "completadas":
            nextState = (agency == agencyDCubaWashington) ? "enviado" : "despachado"
            break;
        case "despachadas":
            nextState = "recibido";
            break;
        case "enviadas":
            nextState = "entregado";
            break;
        case "recibidas":
            nextState = "entregado";
            break;
        default:
    }

    var s = $(".tab-pane.active").find("table").find(".order-select");
    var selectedIds = [];

    s.each((i, x) => {
        if ($(x).prop("checked")) {
            selectedIds.push($(x).val());
        }
    })
    console.log(selectedIds)
    $.ajax({
        async: true,
        type: "POST",
        url: "/Servicios/ChangeStatus",
        data: {
            value: nextState,
            ids: selectedIds
        },
        beforeSend: function () {
            $.blockUI();
        },
        success: function (response) {
            location.href = "/Servicios/Index?msg=El servicio ha cambiado de estado"
        },
        error: function () {
            toastr.error("No se ha podido cambiar el estado", "Error");
            $.unblockUI();
        },
        timeout: 10000,
    });
})

var divPendiente = $('#DivPendiente'),
    divConsulado = $('#DivConsulado'),
    divCompletada = $('#DivCompletada'),
    divDespachada = $('#DivDespachada'),
    divRecibida = $('#DivRecibida'),
    divEnviada = $('#DivEnviada'),
    divEntregada = $('#DivEntregada'),
    divCancelada = $('#DivCancelada');

var tbls = [];
var tblCancelada = null;

window.onload = async () => {
    const loadTable = async (container, tableName) => {
        $(container).block({
            message: '<span class="semibold"> Loading...</span>',
            overlayCSS: {
                backgroundColor: '#fff',
                opacity: 0.8,
                cursor: 'wait'
            },
            css: {
                border: 0,
                padding: 0,
                backgroundColor: 'transparent'
            }
        });
        await $.get("/Servicios/GetTable?tableName=" + tableName, (res) => {
            $(container).html(res);
            initTable(tableName);
            $(container).unblock();
        });
    }

    const initTable = (tableName) => {
        $('.dropdown-toggle').dropdown();
        loadStatus();
        var selector;
        var Name = "";
        switch (tableName) {
            case "Pendiente":
                selector = $('#tablePending');
                Name = 'tablePending';
                break;
            case "Consulado":
                selector = $('#tableConsulado');
                Name = 'tableConsulado';
                break;
            case "Completada":
                selector = $('#tableSuccess');
                Name = 'tableSuccess';
                break;
            case "Despachada":
                selector = $('#tableDespachadas');
                Name = 'tableDespachadas';
                break;
            case "Entregada":
                selector = $('#tableEntregadas');
                Name = 'tableEntregadas';
                break;
            case "Enviado":
                selector = $('#tableEnviadas');
                Name = 'tableEnviadas';
                break;
            case "Recibida":
                selector = $('#tableRecibidas');
                Name = 'tableRecibidas';
                break;
            case "Cancelado":
                selector = $('#tableCanceladas');
                Name = 'tableCanceladas';
                break;
            default:
                break;
        }

        var table = selector.DataTable({
            "searching": true,
            "lengthChange": false,
            "order": [[0, "desc"]],
            language: {
                "decimal": "",
                "emptyTable": "No hay información",
                "info": "Mostrando _START_ a _END_ de _TOTAL_ Entradas",
                "infoEmpty": "Mostrando 0 a 0 de 0 Entradas",
                "infoFiltered": "(Filtrado de _MAX_ total entradas)",
                "infoPostFix": "",
                "thousands": ",",
                "lengthMenu": "Mostrar _MENU_ Entradas",
                "loadingRecords": "Cargando...",
                "processing": "Procesando...",
                "search": "Buscar:",
                "zeroRecords": "Sin resultados encontrados",
                "paginate": {
                    "first": "Primero",
                    "last": "Ultimo",
                    "next": "Siguiente",
                    "previous": "Anterior"
                }
            },
        });

        if (tableName != "Cancelado") {
            tbls.push(table);
        }
        else {
            tblCancelada = table;
        }

        $('#' + Name + '_filter').hide();
    }

    const loadStatus = () => {
        $.fn.editable.defaults.mode = 'inline';
        
        if(agency == agencyDCubaWashington){
            var linkFedex = "";

            $(function () {
                $('.status').editable({
                    source: function (p) {
                        s = [
                            {
                                "1": 'Pendiente',
                                "2": 'Consulado',
                            },
                            {
                                "2": 'Consulado',
                                "3": 'Completado'
                            },
                            {
                                "3": 'Completado',
                                "4": 'Enviado'
                            },
                            {
                                "4": 'Enviado',
                                "5": 'Entregado',
                            },
                            {
                                "5": 'Entregado',
                            },
                            {
                                "6": 'Cancelado'
                            }
                        ];
                        return s[$(this).data('value') - 1];
    
                    },
                    validate: function(x){
                        if (x == "Enviado" || x == 4)
                            linkFedex = prompt("Escriba el link de Fedex:");
                    },
                    display: function (value, sourceData) {
                        var colors = { "": "gray", 1: "#ffb019", 2: "green", 3: "red" },
                            elem = $.grep(sourceData, function (o) { return o.value == value; });
    
                        if (elem.length) {
                            $(this).text(elem[0].text).css("color", colors[value]);
                        } else {
                            $(this).empty();
                        }
    
                    },
                    ajaxOptions: {
                        url: '/Servicios/indexDCuba',
                        type: 'post',
                        dataType: 'json'
                    },
                    params: function (params) {
                        params.id = $(this).data("id");
                        params.linkFedex = linkFedex;
                        return params;
                    },
                    success: function (response, newValue) {
                        location.href = "/Servicios/Index?msg=" + response;
                        //showOKMessage("Cambio de estatus", "Se ha cambiado satifactoriamente el estatus");
                    },
                    error: function(e){
                        linkFedex = "";
                    }
                    //showbuttons: false
                });
            });
        }
        else{
            $(function () {
                $('.status').editable({
                    source: function (p) {
                        s = [
                            {
                                "1": 'Pendiente',
                                "2": 'Completado',
                            },
                            {
                                "2": 'Completado',
                                "3": 'Despachado',
                            },
                            {
                                "2": 'Completado',
                                "3": 'Despachado',
                                "4": 'Recibido',
                            },
                            {
                                "3": 'Despachado',
                                "4": 'Recibido',
                                "5": 'Entregado',
                            },
                            {
                                "4": 'Recibido',
                                "5": 'Entregado',
                            },
                            {
                                "6": 'Cancelado',
                            },
    
                        ];
                        return s[$(this).data('value') - 1];
    
                    },
                    display: function (value, sourceData) {
                        var colors = { "": "gray", 1: "#ffb019", 2: "green", 3: "red" },
                            elem = $.grep(sourceData, function (o) { return o.value == value; });
    
                        if (elem.length) {
                            $(this).text(elem[0].text).css("color", colors[value]);
                        } else {
                            $(this).empty();
                        }
    
                    },
                    ajaxOptions: {
                        url: '/Servicios/index',
                        type: 'post',
                        dataType: 'json'
                    },
                    params: function (params) {
                        params.id = $(this).data("id");
                        return params;
                    },
                    success: function (response, newValue) {
                        location.href = "/Servicios/Index?msg=" + response;
                        //showOKMessage("Cambio de estatus", "Se ha cambiado satifactoriamente el estatus");
                    },
                    //showbuttons: false
                });
            });
        }
        
    }

    const ExecuteLoadTable = async () => {
        await loadTable(divPendiente, "Pendiente");
        if(agency == agencyDCubaWashington){
            await loadTable(divConsulado, "Consulado");
            await loadTable(divEnviada, "Enviado");
        }
        else{
            await loadTable(divDespachada, "Despachada");
            await loadTable(divRecibida, "Recibida");
        }
        await loadTable(divCompletada, "Completada");
        await loadTable(divEntregada, "Entregada");
        await loadTable(divCancelada, "Cancelado");

        $('#search').on('keyup change', function () {
            var search = $(this);
            tbls.forEach((item) => {
                if (item != null)
                    item.search($(search).val()).draw();
            })
        });

        $(".searchColumn").on('keyup change', function (e) {
            var i = $(e.target).data('column');
            tbls.forEach((item) => {
                if (item != null && item.column(i))
                    item.column(i).search("(" + $(e.target).val() + ")", true, true).draw();
            })
        });

        $('#searchcanceladas').on('keyup change', function () {
            if (tblCancelada != null) {
                tblCancelada.search($(this).val()).draw();
            }
        });

        $(".order-select").on("change", function () {
            $('.checkalltramites').prop("checked", false);

            if ($(this)[0].checked) {
                cantSelect++;
            } else {
                cantSelect--;
            }
            var type = $('[class="nav-link active"]').attr("data-type");
            if (cantSelect == 0) {
                $("#nextState").addClass("hidden");
            } else {
                $("#nextState").removeClass("hidden");
            }
        });

        $('.checkalltramites').on("change", function () {
            var t = $(".tab-pane.active").find("table")
            var type = $('[class="nav-link active"]').data("type");

            if ($(this)[0].checked) {
                t.find(".order-select").prop("checked", true);
                cantSelect = t.find(".order-select").length;
                $("#nextState").removeClass("hidden");
            } else {
                t.find(".order-select").prop("checked", false);
                cantSelect = 0;
                $("#nextState").addClass("hidden");
            }
        });
    }

    await ExecuteLoadTable();
}