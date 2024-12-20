var credito = false;
let cantSelect = 0;
$.fn.editable.defaults.mode = 'inline';
$(function () {
    $('.status').editable({
        source: function (p) {
            s = [
                {
                    "1": 'Iniciada',
                    "2": 'Pendiente',
                    "4": 'Completada'
                },
                {
                    "1": 'Iniciada',
                    "2": 'Pendiente',
                    "4": 'Completada'
                },
                {//Estado Completada
                    "4": 'Completada',
                    "5": "Enviada"
                },
                {//Estado Enviada
                    "5": "Enviada",
                },
            ];
            return s[$(this).data('value') - 1];
        },
        display: function (value, sourceData) {
            var colors = { "": "gray", 1: "#ffb019", 2: "red", 3: "red", 4: "green", 5: "#ef00ff", 6: "#000da8" },
                elem = $.grep(sourceData, function (o) { return o.value == value; });

            if (elem.length) {
                $(this).text(elem[0].text).css("color", colors[value]);
            } else {
                $(this).empty();
            }

        },
        ajaxOptions: {
            url: '/OrderCubiq/index',
            type: 'post',
            dataType: 'json'
        },
        params: function (params) {
            params.id = $(this).data("id");
            params.credito = credito;
            return params;
        },
        success: function (response, newValue) {
            location.reload();
        },
    });
});

var tables = {};
var allTables = ["pendientes", "predespachada", "almacenusa", "pallets", "transitousacuba", "recibidocuba", "transitocuba", "entregada", "all", "canceladas"]
var currentTab = "base-tab1";
var table;
var tableCancelada;

$('#btnExport').on('click', function () {
    $('#modalExport').modal('show')
})
$('#exportExcelAccept').attr('href', '/OrderCubiq/ExportExcel/?date=' + $('#daterange').val());
$('#daterange').on('change', function () {
    $('#exportExcelAccept').attr('href', '/OrderCubiq/ExportExcel/?date=' + $('#daterange').val())
});
$('#exportExcelAccept').on('click', function () {
    $('#modalExport').modal('hide');
})

$(document).on("change", '[name="checkalltramites"]', function () {
    var paneActive = $('.tab-pane.active');
    var table = paneActive.find("table");

    if ($(this)[0].checked) {
        table.find(".order-select").prop("checked", true).trigger("change");
        console.log(cantSelect);
    } else {
        table.find(".order-select").prop("checked", false).trigger("change");
        console.log(cantSelect);
    }
});

const loadTable = (container, tableName) => {
    var block_ele = $("#" + container);

    if (typeof tables[tableName] !== undefined) {
        $(block_ele).html(tables[tableName]);
        initTable(tableName);
    }
    else {
        $(block_ele).block({
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
    }
}

const initTable = (tableName) => {
    $('.dropdown-toggle').dropdown();

    $(".cancelOrder").click(function () {
        var orderId = $(this).attr("orderId");
        var orderNumber = $(this).attr("name");
        var okConfirm = function () {
            var urlDelete = "/OrderCubiq/Cancel/" + orderId;
            $.ajax({
                type: "Post",
                url: urlDelete,
                async: false,
                success: function (res) {
                    if(res.success)
                        document.location = "/OrderCubiq?msg=successCancell&orderNumber=" + orderNumber;
                    else
                        toastr.error(res.msg);
                },
                error: function (e) {
                    console.log(e);
                    toastr.error("No se ha podido cancelar la orden", "Error");
                }
            });
        };
        getCancelConfirmation(okConfirm);
    });

    switch (tableName) {
        case "pendientes":
            table = $('#tableInitPendOrders').DataTable({
                "searching": true,
                "dom": "ltrip",
                lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
                //"order": [[6, "desc"]],
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
            break;
        case "predespachada":
            table = $('#tablePreDespachada').DataTable({
                "searching": true,
                "dom": "ltrip",
                lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
                //"order": [[6, "desc"]],
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
            break;
        case "almacenusa":
            table = $('#tableAlmacenUSA').DataTable({
                "searching": true,
                "dom": "ltrip",
                lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
                //"order": [[6, "desc"]],
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
            break;
        case "pallets":
            table = $('#tablePallets').DataTable({
                "searching": true,
                "dom": "ltrip",
                lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
                //"order": [[6, "desc"]],
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
            break;
        case "transitousacuba":
            table = $('#tableTransitoUSACubaOrders').DataTable({
                "searching": true,
                "dom": "ltrip",
                lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
                //"order": [[6, "desc"]],
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
            break;
        case "recibidocuba":
            table = $('#tableRecibidoOrders').DataTable({
                "searching": true,
                "dom": "ltrip",
                lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
                //"order": [[6, "desc"]],
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
            break;
        case "transitocuba":
            table = $('#tableTransitoOrders').DataTable({
                "searching": true,
                "dom": "ltrip",
                lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
                //"order": [[6, "desc"]],
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
            break;
        case "all":
            table = $('#tableAllOrders').DataTable({
                "searching": true,
                "dom": "ltrip",
                lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
                //"order": [[6, "desc"]],
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
            break;
        case "entregada":
            table = $('#tableEntregadaOrders').DataTable({
                "searching": true,
                "dom": "ltrip",
                lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
                //"order": [[6, "desc"]],
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
            break;
        default:
            break;
    }

    $("#search").on('keyup change', function () {
        table.search($(this).val()).draw();
    });
    $(".searchColumn").on('keyup', function (e) {
        var i = $(e.target).data('column');
        table.column(i).search("(" + $(e.target).val() + ")", true, true).draw();
    });
    table.search($("#search").val()).draw();

    $(document).on('click', ".checkEntrega", function () {
        var orderId = $(this).attr("orderId");
        var orderNumber = $(this).attr("name");
        var okConfirm = function () {
            var urlEntregar = "/OrderCubiq/Index";
            $.ajax({
                type: "Post",
                url: urlEntregar,
                data: {
                    value: 10,
                    id: orderId
                },
                async: false,
                success: function () {
                    document.location = "/OrderCubiq?msg=En entrega " + orderNumber;
                }
            });
        };
        confirmationMsg("¿Está seguro que desea pasar esta orden a entrega?", "", okConfirm);
    });

    $('.pdfpredespacho').on('click', function () {
        const id = $(this).data('id');
        $.ajax({
            async: true,
            type: "GET",
            contentType: "application/x-www-form-urlencoded",
            url: "/OrderCubiq/PdfPreDespacho",
            data: {
                id: id
            },
            success: function (response) {

                fileName = 'document.pdf';
                var byteCharacters = atob(response);
                var byteNumber = new Array(byteCharacters.length);
                for (var i = 0; i < byteCharacters.length; i++) {
                    byteNumber[i] = byteCharacters.charCodeAt(i);
                }
                var byteArray = new Uint8Array(byteNumber);
                var blob = new Blob([byteArray], { type: 'application/pdf' });
                var fileURL = URL.createObjectURL(blob);
                window.open(fileURL);
            },
            error: function () {
                toastr.error("No se ha podido exportar", "Error");
            }
        });
    })

    $(document).on('click', ".checkAduana", function () {
        var orderId = $(this).attr("orderId");
        var orderNumber = $(this).attr("name");
        var okConfirm = function () {
            var urlEntregar = "/OrderCubiq/Index";
            $.ajax({
                type: "Post",
                url: urlEntregar,
                data: {
                    value: 9,
                    id: orderId
                },
                async: false,
                success: function () {
                    document.location = "/OrderCubiq?msg=En aduana " + orderNumber;
                }
            });
        };
        confirmationMsg("¿Está seguro que desea pasar esta orden a aduana?", "", okConfirm);
    });
}

$(document).on("ready", () => {
    $(".tablePartial").on("click", (e) => {
        var tablink = e.target;
        if (currentTab !== $(tablink).prop("id")) {
            var prevTab = $("#" + currentTab);
            var prevContainer = $(prevTab).data("table") + "_div";
            $("#" + prevContainer).html("");
            var tableName = $(tablink).data("table");
            var container = tableName + "_div";
            loadTable(container, tableName);
            currentTab = $(tablink).prop("id");

            // desmarcar opciones seleccionadas
            cantSelect = 0;
            $('#li-predespachar').hide();
            $('#li-crearguia').hide();
            $('#li-addtoguia').hide();
            $(".order-select:checked").each(function () {
                $(this).prop("checked", false);
            });
        }
    });

    $(".selectdos").select2({
        width: '150px'
    });

    $(".selectdos").select2({
        width: '150px'
    });

    $("#btnDespachar").on("click", () => {
        const guia = $("#selectedGuia").val();
        window.open(`/OrderCubiq/Despachar?guia=${guia}`, "_blanck");
        var timeout = setTimeout(() => location.reload(), 1000)
    });

    $("#searchPkt").on("click", () => {
        const pkt = $("#search_pkt").val() ? $("#search_pkt").val() : "";

        allTables.map(tableName => {
            $.get("/OrderCubiq/GetTable?type=" + guide_type + "&tableName=" + tableName + "&searchPkt=" + pkt, (res) => {
                let block_ele = $("#" + tableName + "_div")
                tables[tableName] = res;
                var prevTab = $("#" + currentTab);
                var prevContainer = $(prevTab).data("table");
                if (prevContainer == tableName) {
                    $(block_ele).html(res);
                    initTable(tableName);
                    $(block_ele).unblock();
                }
            });
        })
    })

    $(document).on("change", ".order-select", function () {
        if ($(this)[0].checked) {
            cantSelect++;
            //selectedIds.push($(this).val());
        } else {
            cantSelect--;
            //selectedIds.splice($.inArray($(this).val(), selectedIds), 1);
        }
        var tabactive = $('[name="tab"][class="nav-link tablePartial active"]').attr('data-table');
        if (cantSelect == 0) {
            $('#li-predespachar').hide();
            $('#li-crearguia').hide();
            $('#li-addtoguia').hide();
        } else {
            if (tabactive == "pendientes") {
                $('#li-predespachar').show();
                $('#li-crearguia').show();
                $('#li-addtoguia').show();
            }
            if (tabactive == "pallets") {
                $('#li-crearguia').show();
                $('#li-addtoguia').show();
            }
        }
    });

    $('#btnPredespachar').on('click', function () {
        var selectedIds = [];
        $(".order-select:checked").each(function () {
            selectedIds.push($(this).data('id'));
        });

        swal({
            title: "Crear pre despacho",
            text: "¿Desea crear un pre despacho de los trámites seleccionados?",
            type: "info",
            showCancelButton: true,
            closeOnConfirm: false,
            showLoaderOnConfirm: true,
        }, function () {
            var data = {
                type: guide_type,
                ids: selectedIds
            };
            $.ajax({
                async: true,
                type: "POST",
                contentType: 'application/x-www-form-urlencoded',
                url: "/OrderCubiq/CreatePreDespacho",
                data: data,
                beforeSend: function () {
                    $.blockUI();
                },
                success: function (response) {
                    if (response.success) {
                        location.reload();
                    }
                    else {
                        toastr.error(response.msg);
                    }

                    $.unblockUI();
                    // cerrar swal
                    swal.close();
                },
                error: function () {
                    toastr.error("No se han podido crear el pre despacho", "Error");
                    $.unblockUI();
                    // cerrar swal
                    swal.close();
                },
            });

        });
    });

    $('#btn-modalcrearautopredespacho').on('click', function () {
        const retailId = $('#input-retail').val();
        if (!retailId) {
            toastr.error("Seleccione un minorista", "Error");
            return;
        }
        var data = {
            type: guide_type,
            retailId: retailId
        };
        $.ajax({
            async: true,
            type: "POST",
            contentType: 'application/x-www-form-urlencoded',
            url: "/OrderCubiq/CreateAutoPreDespacho",
            data: data,
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                if (response.success) {
                    location.href = "/OrderCubiq/PreDespachoDetails/" + response.data.id;
                }
                else {
                    toastr.error(response.msg);
                }

                $.unblockUI();
                // cerrar swal
                swal.close();
            },
            error: function () {
                toastr.error("No se han podido crear el pre despacho", "Error");
                $.unblockUI();
                // cerrar swal
                swal.close();
            },
        });
    })

    $('#btn-modalcrearguia').on('click', function () {
        var number = $('#input-guidenumber').val();
        var agente = $('#input-agente').val();
        var smlu = $('#input-smlu').val();
        var seal = $('#input-seal').val();
        var cat = $('#input-cat').val();
        var fechaRecogida = $('#input-fecharecogida').val();
        var fechaViaje = $('#input-fechaviaje').val();
        if(!number) {
            toastr.error("Ingrese un número de guía", "Error");
            return;
        }

        var selectedIds = [];
        $(".order-select:checked").each(function () {
            selectedIds.push($(this).data('id'));
        });

        let data = {
            number: number,
            type: guide_type,
            palletsId: selectedIds,
            agente: agente,
            SMLU: smlu,
            SEAL: seal,
            CAT: cat,
            FechaRecogida: fechaRecogida,
            FechaViaje: fechaViaje
        };

        if (currentTab == "base-tab1") {
            data.ordersId = selectedIds;
            data.palletsId = [];
        }


        $.ajax({
            async: true,
            type: "POST",
            contentType: 'application/x-www-form-urlencoded',
            url: "/OrderCubiq/CreateGuia",
            data: data,
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                if (response.success) {
                    location.href = "/ordercubiq/DetailsGuia/" + response.id;
                }
                else {
                    toastr.error(response.msg);
                }

                $.unblockUI();
            },
            error: function () {
                toastr.error("No se han podido crear la guía", "Error");
                $.unblockUI();
            },
        });

    })

    $('#btnAddToGuia').on('click', function () {
        var selectedIds = [];
        $(".order-select:checked").each(function () {
            selectedIds.push($(this).data('number'));
        });

        var guiaId = $('#selectedGuia2').val();
        if (!guiaId) {
            toastr.error("Seleccione una guía", "Error");
            return;
        }

        var data = {
            guideId: guiaId,
            numbers: selectedIds
        };

        var url = "/OrderCubiq/AddOrdersToGuia";

        if (currentTab == "base-tab8") {
            data = {
                numbers: selectedIds,
                guideId: guiaId
            }

            url = "/OrderCubiq/AddPalletsToGuia";
        }

        $.ajax({
            async: true,
            type: "POST",
            contentType: 'application/x-www-form-urlencoded',
            url: url,
            data: data,
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                if (response.success) {
                    location.reload();
                }
                else {
                    toastr.error(response.msg);
                }

                $.unblockUI();
            },
            error: function () {
                toastr.error("No se han podido agregar los trámites a la guía", "Error");
                $.unblockUI();
            },
        });
    })
});

$(window).on("load", () => {
    const pkt = $("#search_pkt").val() ? $("#search_pkt").val() : "";
    allTables.map( tableName => {
        var prevTab = $("#" + currentTab);
        var prevContainer = $(prevTab).data("table");
        if (prevContainer == tableName) {
            var block_ele = $("#" + prevContainer + "_div");
            $(block_ele).block({
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
        }
        var url = "/OrderCubiq/GetTable?type=" + guide_type + "&tableName=" + tableName + "&searchPkt=" + pkt;
        if (tableName == "recibidas" || tableName == "entrega") url += "&showQty=1000"
         $.get(url, (res) => {
            if (tableName == 'canceladas') {
                $('#canceladas_div').html(res)
                tableCanceladas = $('#tableCanceladas').DataTable({
                    "searching": true,
                    "dom": "ltrip",
                    lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
                    //"order": [[6, "desc"]],
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
                $("#searchCanceladas").on('keyup change', function () {
                    tableCanceladas.search($(this).val()).draw();
                });
            }
            else {
                let block_ele = $("#" + tableName + "_div")
                tables[tableName] = res;

                if (prevContainer == tableName) {
                    $(block_ele).html(res);
                    initTable(tableName);
                    $(block_ele).unblock();
                }
            }
        });
    })
})

$('#showAll').on('click', function () {
    const pkt = $("#search_pkt").val() ? $("#search_pkt").val() : "";
    const tableName = "recibidas";
    $.get("/OrderCubiq/GetTable?type=" + guide_type + "&tableName=" + tableName + "&searchPkt=" + pkt, (res) => {
        let block_ele = $("#" + tableName + "_div")
        tables[tableName] = res;
        var prevTab = $("#" + currentTab);
        var prevContainer = $(prevTab).data("table");
        if (prevContainer == tableName) {
            $(block_ele).html(res);
            initTable(tableName);
            $(block_ele).unblock();
        }
    });
})

$(".nav-tabs .nav-link").on("click", (e) => {
    var nav = $(e.target);
    $.each($(".nav-tabs .nav-link"), (i, value) => {
        if ($(value).prop('id') != nav.prop('id')) {
            $(value).removeClass("active");
        }
    })

    $(".searchColumn").map((i, e) => {
        if ($(e).val() != "") {
            $(e).val("").trigger("change");
        }
    })
})

$(document).on('change', '#inputBarCodePreDespacho', function () {
    var value = $(this).val();

    location.href = "/OrderCubiq/PreDespachoDetailsByNumber?number=" + value;
})

$(document).on('click', ".changeType", function () {
    const id = $(this).data('id');
    const type = $(this).data('type');
    const transitaria = $(this).data('transitaria');
    const number = $(this).data('number');

    $('#noTramite').html(number);
    $('#order-type').val(type).trigger('change');
    $('#order-transitaria').val(transitaria).trigger('change');
    $('#order-id').val(id).trigger('change');

    $('#modalChangeType').modal('show');
})

$('#btn-change-type').on('click', function () {
    const type = $('#order-type').val();
    const transitaria = $('#order-transitaria').val();
    const id = $('#order-id').val();

    $.ajax({
        async: true,
        type: "POST",
        contentType: 'application/x-www-form-urlencoded',
        url: "/OrderCubiq/UpdateTypeAndTransitaria",
        data: {
            id: id,
            type: type,
            transitaria: transitaria
        },
        beforeSend: function () {
            $.blockUI();
        },
        success: function (response) {
            if (response.success) {
                location.href = `/OrderCubiq/Index?msg=Se ha actualizado la informacion del tramite&type=${guide_type}`

            }
            else {
                toastr.error(response.msg);
            }

            $.unblockUI();
        },
        error: function () {
            toastr.error("No se ha podido actualizar la informacion del tramite", "Error");
            $.unblockUI();
        },
    });
})

$('#btn-edit-num-guia').on('click', function () {
    $('#btn-save-num-guia').show();
    $(this).hide();
    $('#input-num-guia').prop('disabled', false);
});

$('#btn-save-num-guia').on('click', function () {
    const value = $('#input-num-guia').val();
    if(!value) {
        toastr.error("Ingrese un número de guía", "Error");
        return;
    }

    $('#btn-save-num-guia').hide();
    $('#btn-edit-num-guia').show();
    $('#input-num-guia').prop('disabled', true);

    const url = "/OrderCubiq/SetNumeroGuia";
    const data = {
        numero: value,
        type: guide_type
    };

    $.ajax({
        async: true,
        type: "POST",
        contentType: 'application/x-www-form-urlencoded',
        url: url,
        data: data,
        beforeSend: function () {
            $.blockUI();
        },
        success: function (response) {
            if (response.success) {
                toastr.success("Número de guía guardado correctamente.");
            }
            else {
                toastr.error(response.msg);
            }

            $.unblockUI();
        },
        error: function () {
            toastr.error("No se ha podido guardar el número de guía", "Error");
            $.unblockUI();
        },
    });

});

function getLibras() {
    // ajax get libras
    $.ajax({
        async: false,
        type: "GET",
        contentType: "application/x-www-form-urlencoded",
        url: "/OrderCubiq/GetLibrasPorEstado?type=" + guide_type,
        success: function (response) {
            if (response.success) {
                $('#lbIniciadasMisc').html(response.data.iniciadasMiscelaneas + " lb");
                $('#lbIniciadasDur').html(response.data.iniciadasDuradero + " lb");
                if (agencyId == agencyRapidCargo) {
                    $('#spanMotos').show();
                    $('#lbIniciadasMotos').html(response.data.iniciadasMotos + " lb");
                }
                

                if (response.data.retailsIniciadas && response.data.retailsIniciadas.length > 0) {
                    const tbody = $('#tblLbRetails tbody');
                    tbody.empty(); // Vaciar el cuerpo de la tabla

                    // Recorrer el array e insertar filas
                    response.data.retailsIniciadas.forEach(item => {
                        const row = `
                          <tr>
                            <td>${item.name}</td>
                            <td>${item.pesoLbMiscelaneas}</td>
                            <td>${item.pesoLbDuradero}</td>
                          </tr>
                        `;
                        tbody.append(row);
                    });

                    $('#show_libras_retails').show();
                }
                else {
                    $('#show_libras_retails').hide();
                }
            }
        },
        error: function () {
            toastr.error("No se ha podido obtener las libras", "Error");
        }
    });
}

getLibras();