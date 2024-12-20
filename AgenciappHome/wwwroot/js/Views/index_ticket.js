var tab = 1;
var nameTable = "ticketPending";
var cantSelect = 0;
var rowSelectedAuto = new Array;
var tabName = "";
var guid_empty = "00000000-0000-0000-0000-000000000000";

const debounce = (func, wait) => {
    let timeout;

    return function executedFunction(...args) {
        const later = () => {
            timeout = null;
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
};

var searchColumn = debounce(function (e, table) {
    var i = $(e.target).data('column');
    table.column(i).search($(e.target).val()).draw();
}, 350);

var searchTable = debounce(function (e) {
    tableMisOrdenes.search($(e.target).val()).draw();
    tablePasaje.search($(e.target).val()).draw();
    tableAuto.search($(e.target).val()).draw();
    tableHotel.search($(e.target).val()).draw();
}, 350);

$("#despachar").click(function () {
    $('#modalDespachar').modal('show');
});

$("#checkrange").change(function () {
    if ($(this).is(':checked')) {
        $('#fechaReporte').prop('disabled', false);
    }
    else {
        $('#fechaReporte').prop('disabled', true);
    }
});

$('#creardespacho').on('click', function(){
    var checkDate = $('#checkrange').is(':checked');
    var date = "";
    if (checkDate) {
        date = $('#fechaReporte').val();
    }
    var emails = $('#emails').val();
    if (rowSelectedAuto.length == 0) {
        toastr.inf("Debe seleccionar al menos un tramite");
        return false;
    }

    $.ajax({
        type: "POST",
        async: true,
        url: "/Ticket/DespachoAuto",
        data: {
            ids: rowSelectedAuto,
            emails: emails,
            rangofecha: date
        },
        beforeSend: function () {
            $.blockUI();
        },
        success: function (response) {
            if (response.success) {
                location.reload();
            }
            else {
                $.unblockUI();
                toastr.info(response.msg);
            }
        },
        error: function (e) {
            console.log(e);
            toastr.error("Ha ocurrido un error");
        }
    })
})

$("#base-tab1").click(function () {
    tab = 1;

    $("#searchBtn").val("");
    nameTable = "ticketPending";
    $("#ticketPending tr").removeClass("hidden");

    if ($("#ticketPending tr").length == 1)
        $("#no_result").removeClass("hidden");
    else
        $("#no_result").addClass("hidden");

    selectedIds = new Array;
    $(".order-select").attr("checked", false);
    $("#gen_report").addClass("hidden");
});

$("#base-tab2").click(function () {
    tab = 2;

    $("#searchBtn").val("");
    nameTable = "ticketCompletada";
    $("#ticketCompletada tr").removeClass("hidden");

    if ($("#ticketCompletada tr").length == 1)
        $("#no_result").removeClass("hidden");
    else
        $("#no_result").addClass("hidden");

    selectedIds = new Array;
    $(".order-select").attr("checked", false);
    $("#gen_report").addClass("hidden");
});

$("#base-tab3").click(function () {
    tab = 3;

    $("#searchBtn").val("");
    nameTable = "ticketCancelada";
    $("#ticketCancelada tr").removeClass("hidden");

    if ($("#ticketCancelada tr").length == 1)
        $("#no_result").removeClass("hidden");
    else
        $("#no_result").addClass("hidden");

    selectedIds = new Array;
    $(".order-select").attr("checked", false);
    $("#gen_report").addClass("hidden");
});

$("#base-tab4").click(function () {
    tab = 4;

    $("#searchBtn").val("");
    nameTable = "ticketAll";
    $("#ticketAll tr").removeClass("hidden");

    if ($("#ticketAll tr").length == 1)
        $("#no_result").removeClass("hidden");
    else
        $("#no_result").addClass("hidden");

    selectedIds = new Array;
    $(".order-select").attr("checked", false);
    $("#gen_report").addClass("hidden");
});

$('.nav-link').on('click', function () {
    rowSelectedAuto = new Array;
    tabName = $(this).attr('data-table');
    if (tabName == "Auto") {
        $('.element-auto').show();
    }
    else {
        $('.element-auto').hide();
    }
})

var allTables = ["MisOrdenes", "Pasaje", "Auto", "Hotel", "PasajeCaneladas", "HotelCaneladas", "AutoCaneladas"]
var currentTab = "base_tabMisOrdenes";
var tablePasaje;
var tableMisOrdenes;
var tableAuto;
var tableHotel;
var tablePasajeCancelada;
var tableAutoCancelada;
var tableHotelCancelada;

$.fn.editable.defaults.mode = 'inline';

function selectRowAuto(e) {
    if (e.target.value == "all") {
        rowSelectedAuto = new Array;
        var rows = $('.select-auto');
        rows.each(function (i, element) {
            if ($(element).val() != "all") {
                if (e.target.checked) {
                    rowSelectedAuto.push($(element).val());
                    $(element).prop('checked', true);
                }
                else {
                    $(element).prop('checked', false);

                }
            }
        })
    }
    else {
        if (e.target.checked) {
            rowSelectedAuto.push(e.target.value);
        }
        else {
            var i = rowSelectedAuto.indexOf(e.target.value);
            if (i >= 0) {
                rowSelectedAuto.splice(i, 1);
            }
        }
    }
}

$(document).ready(function () {

    $('#searchCanceladasPasaje').on('keypress', function (e) {
        if (e.keyCode == "13")
            tablePasajeCancelada.search($(e.target).val()).draw();
    });

    $('#searchCanceladasAuto').on('keypress', function (e) {
        if (e.keyCode == "13")
            tableAutoCancelada.search($(e.target).val()).draw();
    });

    $('#searchCanceladasHotel').on('keypress', function (e) {
        if (e.keyCode == "13")
            tableHotelCancelada.search($(e.target).val()).draw();
    });

    $('[name="tablink"]').on('click', function () {
        $('[name="tablink"]').removeClass('active');
        $(this).addClass('active');

    });

    /****Exportar***/
    $('#exportExcelAccept').attr('href', '/ticket/ExportExcel/?date=' + $('#daterange').val() + "&byReservation=" + $('#exportByReservation').is(':checked'));

    $('#daterange').on('change', function () {
        var byReservation = $('#exportByReservation').is(':checked');
        $('#exportExcelAccept').attr('href', '/ticket/ExportExcel/?date=' + $('#daterange').val() + "&byReservation=" + byReservation)
    });

    $('#exportByReservation').on('change', function () {
        var byReservation = $('#exportByReservation').is(':checked');
        $('#exportExcelAccept').attr('href', '/ticket/ExportExcel/?date=' + $('#daterange').val() + "&byReservation=" + byReservation)
    });

    $('#exportExcelAccept').on('click', function () {
        $('#modalExport').modal('hide');
    })

    $('#exportExcel').on('click', function () {
        $('#modalExport').modal('show');
    });

    $("#confirmBtn").on('click', function () {
        $.post(`/Ticket/Confirmar`, {
            id: $("#confirmTicket").val(),
            number: $("#confirmNumber").val()
        }).then((result) => {
            if (result.success) {
                window.location.reload();
            }
        });
    })

    $("#search").on("change", searchTable);
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
    var block_ele = $("#" + prevContainer + "_div");
    $(block_ele).block(blockOptions);

    for (var i = 0; i < allTables.length; i++) {
        var tableName = allTables[i];
        const baseTabId = `#base_tab${tableName}`;
        var t = null;
        switch (tableName) {
            case "Pasaje":
                t = $('#ticketPasaje');
                tablePasaje = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: false,
                    order: [[0, "desc"]],
                    serverSide: true,
                    processing: false,
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
                        "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i>Procesando...',
                        "search": "Buscar:",
                        "zeroRecords": "Sin resultados encontrados",
                        "paginate": {
                            "first": "Primero",
                            "last": "Ultimo",
                            "next": "Siguiente",
                            "previous": "Anterior"
                        }
                    },
                    ajax: {
                        "url": "/Ticket/List?type=pasaje",
                        "type": 'POST',
                        "dataType": "json"
                    },
                    columnDefs: [
                        {
                            targets: [0],
                            data: "date"
                        },
                        {
                            targets: [1],
                            data: "numReserva",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div><a href="/ticket/details/${item.ticketId}">${data}</a></div>`)
                                if (item.ticketBy) {
                                    html.push(`<div class="tag tag-info">${item.ticketBy}</div>`)
                                }
                                if (item.clientIsCarrier) {
                                    html.push(`<div class="tag tag-success">Carrier</div>`)
                                }
                                if (item.paqueteId != guid_empty && item.paqueteId != null) {
                                    html.push(`<div class="tag tag-success">Paquete</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [2],
                            visible: false,
                            data: "ticketBy"
                        },
                        {
                            targets: [3],
                            visible: false,
                            data: "userAirline",
                            render: function (data, type, item) {
                                if (item.userAirline != "" && item.userAirline != null) {
                                    return data
                                }
                                return "-";
                            },
                        },
                        {
                            targets: [4],
                            data: "clientName",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(data)
                                if (item.isMovileApp) {
                                    html.push('<div class="tag tag-success">App</div>')
                                }
                                return html.join("");
                            },

                        },
                        {
                            targets: [5],
                            data: "clientSurname"
                        },
                        {
                            targets: [6],
                            data: "phone"
                        },
                        {
                            targets: [7],
                            visible: false,
                            data: "numVuelo"
                        },
                        {
                            targets: [8],
                            visible: false,
                            data: "noVueloRegreso"
                        },
                        {
                            targets: [9],
                            visible: false,
                            data: "createdDate"
                        },
                        {
                            targets: [10],
                            data: "dateOut"
                        },
                        {
                            targets: [11],
                            visible: false,
                            data: "dateIn"
                        },
                        {
                            targets: [12],
                            visible: false,
                            data: "horaSalida"
                        },
                        {
                            targets: [13],
                            visible: false,
                            data: "horaRegreso"
                        },
                        {
                            targets: [14],
                            visible: false,
                            data: "fromSalida"
                        },
                        {
                            targets: [15],
                            visible: false,
                            data: "toSalida"
                        },
                        {
                            targets: [16],
                            data: "status",
                            render: function (data, type, item) {
                                if (data == "Pendiente") {
                                    return `<a href="#" class="status" data-id="${item.ticketId}" data-type="select" data-value="1" data-pk="1" data-url="/post" data-title="Select status"></a>`
                                }
                                else if (data == "Cancelada") {
                                    return `<s><a href="#" class="status" data-id="${item.ticketId}" data-type="select" data-value="3" data-pk="3" data-url="/post" data-title="Select status"></a></s>`
                                }
                                else if (data == "Completada") {
                                    return `<a href="#" class="status" data-id="${item.ticketId}" data-type="select" data-value="2" data-pk="2" data-url="/post" data-title="Select status"></a>`
                                }
                                else return "";
                            },
                        },
                        {
                            targets: [17],
                            data: "amount",
                            render: function (data, type, item) {
                                return `<p style="text-align:left;display:inline-block" class="text-info"><b>${data}</b></p>`
                            }
                        },
                        {
                            targets: [18],
                            data: "debit",
                            render: function (data, type, item) {
                                return `<p style="text-align:left;display:inline-block" class="text-danger"><b>${data}</b></p>`
                            }
                        },
                        {
                            targets: [19],
                            data: "ticketId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(`<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(`<div class="dropdown-menu" aria-label="dropdown88fda939-30e3-417f-a848-956e3b110b8d">`)
                                html.push(`<a class="dropdown-item" href="/Ticket/Details/${data}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`<a class="dropdown-item print_report" name="print" data-value="${data}"><i class="fa fa-file-pdf-o"></i>Imprimir Reserva</a>`)
                                html.push(`<a class="dropdown-item print_report" name="printRecibo" data-value="${data}"><i class="fa fa-file-pdf-o"></i>Imprimir Recibo</a>`)
                                if (item.status == "Pendiente") {
                                    html.push(`<a class="dropdown-item" href="/Pago/GestionarPago/${data}?tipo=Reserva"><i class="fa fa-usd"></i>Gestionar pago</a>`)
                                }
                                html.push(`<a class="dropdown-item" href="/Ticket/PaysTicket/${data}"><i class="icon-wallet"></i>Registro de pagos</a>`)
                                if (role == "Agencia" || role == "Empleado") {
                                    html.push(`<a class="dropdown-item" href="/ticket/edit/${data}"><i class="ft-edit"></i>Editar</a>`)
                                    html.push(`<a class="dropdown-item" href="/ticket/EditarPagos/${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`)

                                }
                                html.push(`<a class="dropdown-item" href="#" name="cancel" data-type="ticket" data-id="${data}"><i class="ft-x"></i>Cancelar</a>`)
                                html.push(`<a class="dropdown-item" href="/Reclamo/Create?orderNumber=${item.numReserva}&idClient=${item.clientId}"><i class="ft-file"></i>Crear Ticket</a>`)
                                html.push('</div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },

                    ],
                    createdRow: function (row, data, rowIndex) {
                        $.each($('.status', row), (i, e) => {
                            $(e).editable({
                                source: function (p) {
                                    s = [
                                        {
                                            "1": 'Pendiente',
                                        },
                                        {
                                            "2": 'Completada',
                                        },
                                        {
                                            "3": 'Cancelada',
                                        },

                                    ];
                                    return s[$(this).data('value') - 1];
                                },
                                display: function (value, sourceData) {
                                    var colors = { "": "gray", 1: "#ffb019", 2: "green", 3: "red", 4: "green" },
                                        elem = $.grep(sourceData, function (o) { return o.value == value; });

                                    if (elem.length) {
                                        $(this).text(elem[0].text).css("color", colors[value]);
                                    } else {
                                        $(this).empty();
                                    }
                                },
                                ajaxOptions: {
                                    url: '/Ticket/Index',
                                    type: 'post',
                                    dataType: 'json'
                                },
                                params: function (params) {
                                    params.id = $(this).data("id");
                                    return params;
                                },
                                success: function (response, newValue) {
                                    document.location = "/Ticket?msg=updateStatusOrden&nombre=Alex Bautista";
                                },
                            });
                        })
                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })                        
                        $.each($('[name = "print"]', row), (i, e) => {
                            $(e).click(function () {
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

                        })
                        $.each($('[name="printRecibo"]', row), (i, e) => {
                            $(e).click(function () {
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
                        })
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                        $("#base_tabPasaje").html(`Pasajes (${json.recordsTotal})`);
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tablePasaje));
                })
                $(".column_visible").on("change", function (e) {
                    var i = $(e.target).data('column');
                    tablePasaje.column(i).visible($(e.target).is(':checked'));
                    $.each($(".searchColumn", tablePasaje.column(i)), (i, e) => {
                        $(e).on('keyup', (e) => searchColumn(e, tablePasaje));
                    })
                });
                break;
            case "MisOrdenes":
                t = $('#ticketMisOrdenes');
                tableMisOrdenes = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: false,
                    order: [[0, "desc"]],
                    serverSide: true,
                    processing: false,
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
                    ajax: {
                        "url": "/Ticket/List?type=misordenes",
                        "type": 'POST',
                        "dataType": "json"
                    },
                    columnDefs: [
                        {
                            targets: [0],
                            data: "date"
                        },
                        {
                            targets: [1],
                            data: "numReserva",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div><a href="/ticket/details/${item.ticketId}">${data}</a></div>`)
                                if (item.ticketBy) {
                                    html.push(`<div class="tag tag-info">${item.ticketBy}</div>`)
                                }
                                if (item.clientIsCarrier) {
                                    html.push(`<div class="tag tag-success">Carrier</div>`)
                                }
                                if (item.paqueteId != guid_empty && item.paqueteId != null) {
                                    html.push(`<div class="tag tag-success">Paquete</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [2],
                            visible: false,
                            data: "userAirline",
                            render: function (data, type, item) {
                                if (item.userAirline != "" && item.userAirline != null) {
                                    return data
                                }
                                return "-";
                            },
                        },
                        {
                            targets: [3],
                            data: "clientName",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(data)
                                if (item.isMovileApp) {
                                    html.push('<div class="tag tag-success">App</div>')
                                }
                                return html.join("");
                            },

                        },
                        {
                            targets: [4],
                            data: "clientSurname"
                        },
                        {
                            targets: [5],
                            data: "phone"
                        },
                        {
                            targets: [6],
                            data: "dateOut"
                        },
                        {
                            targets: [7],
                            data: "amount",
                            render: function (data, type, item) {
                                return `<p style="text-align:left;display:inline-block" class="text-info"><b>${data}</b></p>`
                            }
                        },
                        {
                            targets: [8],
                            data: "debit",
                            render: function (data, type, item) {
                                return `<p style="text-align:left;display:inline-block" class="text-danger"><b>${data}</b></p>`
                            }
                        },
                        {
                            targets: [9],
                            data: "ticketId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(`<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(`<div class="dropdown-menu" aria-label="dropdown88fda939-30e3-417f-a848-956e3b110b8d">`)
                                html.push(`<a class="dropdown-item" href="/Ticket/Details/${data}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`<a class="dropdown-item print_report" name="print" data-value="${data}"><i class="fa fa-file-pdf-o"></i>Imprimir Reserva</a>`)
                                html.push(`<a class="dropdown-item print_report" name="printRecibo" data-value="${data}"><i class="fa fa-file-pdf-o"></i>Imprimir Recibo</a>`)
                                if (item.status == "Pendiente") {
                                    html.push(`<a class="dropdown-item" href="/Pago/GestionarPago/${data}?tipo=Reserva"><i class="fa fa-usd"></i>Gestionar pago</a>`)
                                }
                                html.push(`<a class="dropdown-item" href="/Ticket/PaysTicket/${data}"><i class="icon-wallet"></i>Registro de pagos</a>`)
                                if (role == "Agencia" || role == "Empleado") {
                                    html.push(`<a class="dropdown-item" href="/ticket/edit/${data}"><i class="ft-edit"></i>Editar</a>`)
                                    html.push(`<a class="dropdown-item" href="/ticket/EditarPagos/${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`)

                                }
                                html.push(`<a class="dropdown-item" href="#" name="cancel" data-type="ticket" data-id="${data}"><i class="ft-x"></i>Cancelar</a>`)
                                html.push(`<a class="dropdown-item" href="/Reclamo/Create?orderNumber=${item.numReserva}&idClient=${item.clientId}"><i class="ft-file"></i>Crear Ticket</a>`)
                                html.push('</div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },

                    ],
                    createdRow: function (row, data, rowIndex) {
                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })                       
                        $.each($('[name = "print"]', row), (i, e) => {
                            $(e).click(function () {
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

                        })
                        $.each($('[name="printRecibo"]', row), (i, e) => {
                            $(e).click(function () {
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
                        })
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                        $("#base_tabMisOrdenes").html(`Mis Ordenes (${json.recordsTotal})`);
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableMisOrdenes));
                })
                break;
            case "Auto":
                t = $('#ticketAuto');
                tableAuto = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: false,
                    order: [[1, "desc"]],
                    serverSide: true,
                    processing: false,
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
                    ajax: {
                        "url": "/Ticket/List?type=auto",
                        "type": 'POST',
                        "dataType": "json"
                    },
                    columnDefs: [
                        {
                            targets: [0],
                            data: "ticketId",
                            orderable: false,
                            render: function (data, type, item) {
                                return `<label class="custom-control custom-checkbox">
                                            <input type="checkbox" class="custom-control-input select-auto" value="${data}" />
                                            <span class="custom-control-indicator"></span>
                                            <span class="custom-control-description"></span>
                                        </label>`
                            }
                        },
                        {
                            targets: [1],
                            data: "date",                           
                        },
                        {
                            targets: [2],
                            data: "numReserva",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div><a href="/ticket/details/${item.ticketId}">${data}</a></div>`)
                                if (item.ticketBy) {
                                    html.push(`<div class="tag tag-info">${item.ticketBy}</div>`)
                                }
                                if (item.agencyTransferida != null) {
                                    if (item.agencyTransferidaId == idAgency) {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.agency}</div>`)
                                    }
                                    else {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. a ${item.agencyTransferida}</div>`)
                                    }
                                }
                                if (item.clientIsCarrier) {
                                    html.push(`<div class="tag tag-success">Carrier</div>`)
                                }
                                if (item.paqueteId != guid_empty && item.paqueteId != null) {
                                    html.push(`<div class="tag tag-success">Paquete</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [3],
                            data: "clientName",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(data)
                                if (item.isMovileApp) {
                                    html.push('<div class="tag tag-success">App</div>')
                                }
                                return html.join("");
                            },

                        },
                        {
                            targets: [4],
                            data: "clientSurname"
                        },
                        {
                            targets: [5],
                            data: "phone"
                        },
                        {
                            targets: [6],
                            data: "wholesalerName"
                        },
                        {
                            targets: [7],
                            data: 'status',
                            render: function (data, type, item) {
                                html = []
                                if (item.status == "Pendiente") {
                                    html.push(`<a href="#" class="status-auto" data-id="${item.ticketId}" data-type="select" data-value="1" data-pk="1" data-url="/post" data-title="Select status"></a>`)
                                }
                                else if (item.status == 'Completada') {
                                    html.push(`<a href="#" class="status-auto" data-id="${item.ticketId}" data-type="select" data-value="2" data-pk="2" data-url="/post" data-title="Select status"></a>`)
                                }
                                else if (item.status == 'Despachada') {
                                    html.push(`<a href="#" class="status-auto" data-id="${item.ticketId}" data-type="select" data-value="3" data-pk="3" data-url="/post" data-title="Select status"></a>`)
                                }
                                else if (item.status == 'Confirmada') {
                                    html.push(`<a href="#" class="status-auto" data-id="${item.ticketId}" data-type="select" data-value="4" data-pk="4" data-url="/post" data-title="Select status"></a>`)
                                    if (item.numeroConfirmacion)
                                        html.push(`<div class="tag tag-success">${item.numeroConfirmacion}</div>`)

                                }
                                else if (item.status == 'Cancelada') {
                                    html.push(`<s><a href="#" class="status-auto" data-id="${item.ticketId}" data-type="select" data-value="5" data-pk="5" data-url="/post" data-title="Select status"></a></s>`)
                                }
                                if (parseFloat(item.debit) > 0) {
                                    html.push(`<span style="display: inherit;" class="tag tag-default tag-danger">Pendiente</span>`)
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [8],
                            data: "amount",
                            render: function (data, type, item) {
                                return `<p style="text-align:left;display:inline-block" class="text-info"><b>${data}</b></p>`
                            }
                        },
                        {
                            targets: [9],
                            data: "debit",
                            render: function (data, type, item) {
                                return `<p style="text-align:left;display:inline-block" class="text-danger"><b>${data}</b></p>`
                            }
                        },
                        {
                            targets: [10],
                            data: "ticketId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(`<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(`<div class="dropdown-menu" aria-label="dropdown88fda939-30e3-417f-a848-956e3b110b8d">`)
                                html.push(`<a class="dropdown-item" href="/Ticket/Details/${data}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`<a class="dropdown-item print_report" name="print" data-value="${data}"><i class="fa fa-file-pdf-o"></i>Imprimir Reserva</a>`)
                                html.push(`<a class="dropdown-item print_report" name="printRecibo" data-value="${data}"><i class="fa fa-file-pdf-o"></i>Imprimir Recibo</a>`)
                                if (item.debit > 0) {
                                    html.push(`<a class="dropdown-item" href="/Pago/GestionarPago/${data}?tipo=Reserva"><i class="fa fa-usd"></i>Gestionar pago</a>`)
                                }
                                html.push(`<a class="dropdown-item" href="/ticket/EditarPagos/${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`)
                                html.push(`<a class="dropdown-item" href="/Ticket/PaysTicket/${data}"><i class="icon-wallet"></i>Registro de pagos</a>`)
                                if (role == "Agencia" || role == "Empleado")
                                {
                                    html.push(`<a class="dropdown-item" href="/ticket/edit/${data}"><i class="ft-edit"></i>Editar</a>`)
                                }
                                if (item.status == 'Despachada') {
                                    html.push(`<a class="dropdown-item openConfirmar" data-id="${item.ticketId}"><i class="fa fa-check"></i>Confirmar</a>`)
                                }
                                html.push(`<a class="dropdown-item" href="#" name="cancel" data-type="ticket" data-id="${data}"><i class="ft-x"></i>Cancelar</a>`)
                                html.push(`<a class="dropdown-item" href="/Reclamo/Create?orderNumber=${item.numReserva}&idClient=${item.clientId}"><i class="ft-file"></i>Crear Ticket</a>`)
                                html.push('</div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },
                    ],
                    createdRow: function (row, data, rowIndex) {
                        $.each($('.status-auto', row), (i, e) => {
                            $(e).editable({
                                source: function (p) {
                                    s = [
                                        {
                                            "1": 'Pendiente',
                                        },
                                        {
                                            "2": 'Completada',
                                            "3": 'Despachada',
                                            "4": 'Confirmada',
                                        },
                                        {
                                            "2": 'Completada',
                                            "3": 'Despachada',
                                            "4": 'Confirmada'
                                        },
                                        {
                                            "4": 'Confirmada',
                                        },
                                        {
                                            "5": 'Cancelada',
                                        },

                                    ];
                                    return s[$(this).data('value') - 1];

                                },
                                display: function (value, sourceData) {
                                    var colors = { "": "gray", 1: "#ffb019", 2: "green", 3: "#6719ff", 4: "#bcff19", 5: "red" },
                                        elem = $.grep(sourceData, function (o) { return o.value == value; });

                                    if (elem.length) {
                                        $(this).text(elem[0].text).css("color", colors[value]);
                                    } else {
                                        $(this).empty();
                                    }
                                },
                                ajaxOptions: {
                                    url: '/Ticket/ChangeStatusAuto',
                                    type: 'post',
                                    dataType: 'json'
                                },
                                params: function (params) {
                                    params.id = $(this).data("id");
                                    return params;
                                },
                                success: function (response, newValue) {
                                    if (response.success)
                                        document.location = "/Ticket?msg=updateStatusOrden&nombre=Alex Bautista";
                                    else
                                        toastr.info(response.msg);
                                },
                            });
                        })
                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($('.openConfirmar', row), (i, e) => {
                            $(e).on("click", function (e) {
                                $("#confirmTicket").val($(e.target).data("id"));
                                $("#modalConfirmar").modal("show");
                            })
                        })
                        $.each($('[name = "print"]', row), (i, e) => {
                            $(e).click(function () {
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

                        })
                        $.each($('[name="printRecibo"]', row), (i, e) => {
                            $(e).click(function () {
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
                        })
                        $.each($('.select-auto', row), (i, e) => $(e).on('change', f => selectRowAuto(f)))
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                        $("#base_tabAuto").html(`Autos (${json.recordsTotal})`);
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableAuto));
                })
                break;
            case "Hotel":
                t = $('#ticketHotel');
                tableHotel = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: false,
                    order: [[0, "desc"]],
                    serverSide: true,
                    processing: false,
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
                    ajax: {
                        "url": "/Ticket/List?type=hotel",
                        "type": 'POST',
                        "dataType": "json"
                    },
                    columnDefs: [
                        {
                            targets: [0],
                            data: "date",
                        },
                        {
                            targets: [1],
                            data: "numReserva",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div><a href="/ticket/details/${item.ticketId}">${data}</a></div>`)
                                if (item.ticketBy) {
                                    html.push(`<div class="tag tag-info">${item.ticketBy}</div>`)
                                }
                                if (item.clientIsCarrier) {
                                    html.push(`<div class="tag tag-success">Carrier</div>`)
                                }
                                if (item.paqueteId != guid_empty && item.paqueteId != null) {
                                    html.push(`<div class="tag tag-success">Paquete</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [2],
                            data: "clientName",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(data)
                                if (item.isMovileApp) {
                                    html.push('<div class="tag tag-success">App</div>')
                                }
                                return html.join("");
                            },

                        },
                        {
                            targets: [3],
                            data: "clientSurname"
                        },
                        {
                            targets: [4],
                            data: "phone"
                        },
                        {
                            targets: [5],
                            data: "dateIn"
                        },
                        {
                            targets: [6],
                            data: "status",
                            render: function (data, type, item) {
                                if (item.status == "Pendiente") {
                                    return `<a href="#" class="status" data-id="${item.ticketId}" data-type="select" data-value="1" data-pk="1" data-url="/post" data-title="Select status"></a>`
                                }
                                else if (item.status == "Cancelada") {
                                    return `<s><a href="#" class="status" data-id="${item.ticketId}" data-type="select" data-value="3" data-pk="3" data-url="/post" data-title="Select status"></a></s>`
                                }
                                else if (item.status == "Completada") {
                                    return `<a href="#" class="status" data-id="${item.ticketId}" data-type="select" data-value="2" data-pk="2" data-url="/post" data-title="Select status"></a>`
                                }
                                else return ""
                            }
                        },
                        {
                            targets: [7],
                            data: "amount",
                            render: function (data, type, item) {
                                return `<p style="text-align:left;display:inline-block" class="text-info"><b>${data}</b></p>`
                            }
                        },
                        {
                            targets: [8],
                            data: "debit",
                            render: function (data, type, item) {
                                return `<p style="text-align:left;display:inline-block" class="text-danger"><b>${data}</b></p>`
                            }
                        },
                        {
                            targets: [9],
                            data: "ticketId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(`<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(`<div class="dropdown-menu" aria-label="dropdown88fda939-30e3-417f-a848-956e3b110b8d">`)
                                html.push(`<a class="dropdown-item" href="/Ticket/Details/${data}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`<a class="dropdown-item print_report" name="print" data-value="${data}"><i class="fa fa-file-pdf-o"></i>Imprimir Reserva</a>`)
                                html.push(`<a class="dropdown-item print_report" name="printRecibo" data-value="${data}"><i class="fa fa-file-pdf-o"></i>Imprimir Recibo</a>`)
                                if (item.status == "Pendiente") {
                                    html.push(`<a class="dropdown-item" href="/Pago/GestionarPago/${data}?tipo=Reserva"><i class="fa fa-usd"></i>Gestionar pago</a>`)
                                }
                                html.push(`<a class="dropdown-item" href="/Ticket/PaysTicket/${data}"><i class="icon-wallet"></i>Registro de pagos</a>`)
                                if (role == "Agencia" || role == "Empleado") {
                                    html.push(`<a class="dropdown-item" href="/ticket/edit/${data}"><i class="ft-edit"></i>Editar</a>`)
                                    html.push(`<a class="dropdown-item" href="/ticket/EditarPagos/${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`)

                                }
                                html.push(`<a class="dropdown-item" href="#" name="cancel" data-type="ticket" data-id="${data}"><i class="ft-x"></i>Cancelar</a>`)
                                html.push(`<a class="dropdown-item" href="/Reclamo/Create?orderNumber=${item.numReserva}&idClient=${item.clientId}"><i class="ft-file"></i>Crear Ticket</a>`)
                                html.push('</div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },

                    ],
                    createdRow: function (row, data, rowIndex) {
                        $.each($('.status', row), (i, e) => {
                            $(e).editable({
                                source: function (p) {
                                    s = [
                                        {
                                            "1": 'Pendiente',
                                        },
                                        {
                                            "2": 'Completada',
                                        },
                                        {
                                            "3": 'Cancelada',
                                        },

                                    ];
                                    return s[$(this).data('value') - 1];
                                },
                                display: function (value, sourceData) {
                                    var colors = { "": "gray", 1: "#ffb019", 2: "green", 3: "red", 4: "green" },
                                        elem = $.grep(sourceData, function (o) { return o.value == value; });

                                    if (elem.length) {
                                        $(this).text(elem[0].text).css("color", colors[value]);
                                    } else {
                                        $(this).empty();
                                    }
                                },
                                ajaxOptions: {
                                    url: '/Ticket/Index',
                                    type: 'post',
                                    dataType: 'json'
                                },
                                params: function (params) {
                                    params.id = $(this).data("id");
                                    return params;
                                },
                                success: function (response, newValue) {
                                    document.location = "/Ticket?msg=updateStatusOrden&nombre=Alex Bautista";
                                },
                            });
                        })
                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($('[name = "print"]', row), (i, e) => {
                            $(e).click(function () {
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

                        })
                        $.each($('[name="printRecibo"]', row), (i, e) => {
                            $(e).click(function () {
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
                        })
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                        $("#base_tabHotel").html(`Hoteles (${json.recordsTotal})`);
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableHotel));
                })
                break;
            case "PasajeCaneladas":
                t = $('#ticket' + tableName);
                tablePasajeCancelada = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: false,
                    order: [[0, "desc"]],
                    serverSide: true,
                    processing: false,
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
                    ajax: {
                        "url": "/Ticket/List?type=" + tableName,
                        "type": 'POST',
                        "dataType": "json"
                    },
                    columnDefs: [
                        {
                            targets: [0],
                            data: "date",
                        },
                        {
                            targets: [1],
                            data: "numReserva",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div><a href="/ticket/details/${item.ticketId}">${data}</a></div>`)
                                if (item.ticketBy) {
                                    html.push(`<div class="tag tag-info">${item.ticketBy}</div>`)
                                }
                                if (item.paqueteId != guid_empty && item.paqueteId != null) {
                                    html.push(`<div class="tag tag-success">Paquete</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [2],
                            data: "clientName",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(data)
                                if (item.isMovileApp) {
                                    html.push('<div class="tag tag-success">App</div>')
                                }
                                return html.join("");
                            },

                        },
                        {
                            targets: [3],
                            data: "clientSurname"
                        },
                        {
                            targets: [4],
                            data: "phone"
                        },
                        {
                            targets: [5],
                            data: "dateOut"
                        }
                    ]
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tablePasajeCancelada));
                })
                break;
            case "HotelCaneladas":
                t = $('#ticket' + tableName);
                tableHotelCancelada = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: false,
                    order: [[0, "desc"]],
                    serverSide: true,
                    processing: false,
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
                    ajax: {
                        "url": "/Ticket/List?type=" + tableName,
                        "type": 'POST',
                        "dataType": "json"
                    },
                    columnDefs: [
                        {
                            targets: [0],
                            data: "date",
                        },
                        {
                            targets: [1],
                            data: "numReserva",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div><a href="/ticket/details/${item.ticketId}">${data}</a></div>`)
                                if (item.ticketBy) {
                                    html.push(`<div class="tag tag-info">${item.ticketBy}</div>`)
                                }
                                if (item.paqueteId != guid_empty && item.paqueteId != null) {
                                    html.push(`<div class="tag tag-success">Paquete</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [2],
                            data: "clientName",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(data)
                                if (item.isMovileApp) {
                                    html.push('<div class="tag tag-success">App</div>')
                                }
                                return html.join("");
                            },

                        },
                        {
                            targets: [3],
                            data: "clientSurname"
                        },
                        {
                            targets: [4],
                            data: "phone"
                        },
                        {
                            targets: [5],
                            data: "dateOut"
                        }
                    ]
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableHotelCancelada));
                })
                break;
            case "AutoCaneladas":
                t = $('#ticket' + tableName);
                tableAutoCancelada = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: false,
                    order: [[0, "desc"]],
                    serverSide: true,
                    processing: false,
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
                    ajax: {
                        "url": "/Ticket/List?type=" + tableName,
                        "type": 'POST',
                        "dataType": "json"
                    },
                    columnDefs: [
                        {
                            targets: [0],
                            data: "date",
                        },
                        {
                            targets: [1],
                            data: "numReserva",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div><a href="/ticket/details/${item.ticketId}">${data}</a></div>`)
                                if (item.ticketBy) {
                                    html.push(`<div class="tag tag-info">${item.ticketBy}</div>`)
                                }
                                if (item.paqueteId != guid_empty && item.paqueteId != null) {
                                    html.push(`<div class="tag tag-success">Paquete</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [2],
                            data: "clientName",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(data)
                                if (item.isMovileApp) {
                                    html.push('<div class="tag tag-success">App</div>')
                                }
                                return html.join("");
                            },

                        },
                        {
                            targets: [3],
                            data: "clientSurname"
                        },
                        {
                            targets: [4],
                            data: "phone"
                        },
                        {
                            targets: [5],
                            data: "dateOut"
                        }
                    ]
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableAutoCancelada));
                })
                break;
            default:
                t = $('#ticket' + tableName);
                var table = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: false,
                    order: [[0, "desc"]],
                    serverSide: true,
                    processing: false,
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
                    ajax: {
                        "url": "/Ticket/List?type=" + tableName,
                        "type": 'POST',
                        "dataType": "json"
                    },
                    columnDefs: [
                        {
                            targets: [0],
                            data: "date",
                        },
                        {
                            targets: [1],
                            data: "numReserva",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div><a href="/ticket/details/${item.ticketId}">${data}</a></div>`)
                                if (item.ticketBy) {
                                    html.push(`<div class="tag tag-info">${item.ticketBy}</div>`)
                                }
                                if (item.paqueteId != guid_empty && item.paqueteId != null) {
                                    html.push(`<div class="tag tag-success">Paquete</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [2],
                            data: "clientName",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(data)
                                if (item.isMovileApp) {
                                    html.push('<div class="tag tag-success">App</div>')
                                }
                                return html.join("");
                            },

                        },
                        {
                            targets: [3],
                            data: "clientSurname"
                        },
                        {
                            targets: [4],
                            data: "phone"
                        },
                        {
                            targets: [5],
                            data: "dateOut"
                        }
                    ]
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, table));
                })
                break
        }   

        if (!tableName.includes("Caneladas")) 
        {
            $(baseTabId).block(blockOptions);
            $(baseTabId).parent().addClass("disabled");
            $(t).on('processing.dt', function (e, settings, processing) {
                if (processing) {
                    $(baseTabId).block(blockOptions);
                    $(baseTabId).parent().addClass("disabled");
                }
                else {
                    $(baseTabId).unblock();
                    $(baseTabId).parent().removeClass("disabled");
                }
            }) 
        }
    };

    $(block_ele).unblock();   
})
