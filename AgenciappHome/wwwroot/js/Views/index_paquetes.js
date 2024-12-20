var tab = 1;
var nameTable = "paquetePendiente";
var cantSelect = 0;

var tables = {};
var allTables = ["Pendientes", "Completadas", "Canceladas"]
var currentTab = "base_tabPendientes";

var table;
var tableCancelada;

$('#exportExcel').on('click', function () {
    $('#modalExport').modal('show');
})

$('#exportExcelAccept').attr('href', '/PaqueteTuristico/ExportExcel?date=' + $('#daterange').val());

$('#daterange').on('change', function () {
    $('#exportExcelAccept').attr('href', '/PaqueteTuristico/ExportExcel?date=' + $('#daterange').val())
});

$('#exportExcelAccept').on('click', function () {
    $('#modalExport').modal('hide');
})

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

    /*****Imprimir reserva*******/
    $('[name="print"]').click(function () {
        var id = $(this).attr("data-value");
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/Ticket/createFileTicket",
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
            },
            timeout: 4000,
        });
    });

    $('[name="printRecibo"]').click(function () {
        var id = $(this).attr("data-value");
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/Ticket/createFileTicketRecive",
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
            },
            timeout: 4000,
        });
    });

    let table = null;
    let tableCancelada = null;
    switch (tableName) {
        case "Pendientes":
            table = $('#paquetesPendientes').DataTable({
                "searching": true,
                "dom": "ltip",
                "lengthChange": false,
                "order": [[0, "desc"]],
                //"scrollX": false,
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
        case "Completadas":
            table = $('#paquetesCompletadas').DataTable({
                "searching": true,
                "dom": "ltip",
                "lengthChange": false,
                "order": [[0, "desc"]],
                //"scrollX": true,
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
        case "Canceladas":
            tableCancelada = $('#paquetesCanceladas').DataTable({
                "searching": true,
                "lengthChange": false,
                "dom": "ltip",
                "order": [[0, "desc"]],
                //"scrollX": true,
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
    }

    /****Buscar*****/
    if (table) {
        $(".searchColumn").on('keyup', function (e) {
            var i = $(e.target).data('column');
            table.column(i).search("(" + $(e.target).val() + ")", true, true).draw();
        });
        $('#search').on('keyup change', function (e) {
            table.search($(e.target).val()).draw();
        })
    }
    else if (tableCancelada) {
        $(".searchColumnCancel").on('keyup', function (e) {
            var i = $(e.target).data('column');
            tableCancelada.column(i).search("(" + $(e.target).val() + ")", true, true).draw();
        });
        $('#searchcanceladas').on('keyup change', function (e) {
            tableCancelada.search($(e.target).val()).draw();
        })
    }
}



$(document).ready(function () {
    $('[name="tablink"]').on('click', function () {
        $('[name="tablink"]').removeClass('active');
        $(this).addClass('active');

    });

    /****Cargar tablas****/
    $(".tablePartial").on("click", (e) => {
        var tablink = e.target;
        if (currentTab !== $(tablink).prop("id")) {
            var prevTab = $("#" + currentTab);
            var prevContainer = "div_" + $(prevTab).data("table");
            $("#" + prevContainer).html("");

            var tableName = $(tablink).data("table");
            var container = "div_" + tableName;
            loadTable(container, tableName);
            currentTab = $(tablink).prop("id");
        }
    });    
});

$(window).on("load", () => {
    var prevTab = $("#" + currentTab);
    var prevContainer = $(prevTab).data("table");
    const blockOptions = {
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
    };

    allTables.map(tableName => {
        const baseTabId = `#base_tab${tableName}`;
        if (tableName != "Canceladas") {
            $(baseTabId).block(blockOptions);
            $(baseTabId).parent().addClass("disabled");
        }
        var block_ele = $("#div_" + prevContainer);
        $(block_ele).block(blockOptions);
        $.get("/PaqueteTuristico/GetTable?tableName=" + tableName, (res) => {
            const tab = $("#div_" + tableName);
            tables[tableName] = res;
            if (prevContainer == tableName || tableName == "Canceladas") {
                $(tab).html(res);
                initTable(tableName);
            }
            $(baseTabId).unblock();
            $(block_ele).unblock();
            $(baseTabId).parent().removeClass("disabled");
        });
    });
})