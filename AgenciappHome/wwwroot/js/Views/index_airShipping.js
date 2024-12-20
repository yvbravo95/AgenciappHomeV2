const numberPages = 3;
const formatDate = "MM/DD/YY";
const nameStorage = "list_order_type";
let loadInitSuccess = false;
var searchProductValue = "";
var cantSelect = 0;
const allTables = ["Completada", "Enviada", "Despachada", "Recibida", "Revisada", "Incompleta", "Entregada", "Cancelada"]
var currentTab = "base_tabIniciada_Pendiente";
var tableInitPending;
var tableCompletada;
var tableEnviada;
var tableDespachada;
var tableRecibida;
var tableRevisada;
var tableEntregada;
var tableCancelada;
var tableIncompleta;

const agencyUniversalTravel = "68A559FA-AA00-4D52-93B5-DD833B37ED02".toLowerCase();

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

$('#filtro-estados').select2({
    'placeholder': "Seleccione un estado"
})

$('#filtro-provincias').select2({
    'placeholder': "Provincias a mostrar"
})

$("#select-status-mult").select2({
    'placeholder': "Seleccione estados"
});

$("#select-province-mult").select2({
    'placeholder': "Seleccione provincias"
});

$("#select-retails-mult").select2({
    'placeholder': "Seleccione minoristas"
});

$("#select-type-mult").select2({
    'placeholder': "Seleccione tipo de tramite  "
});

const tableLanguage = {
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
};

$.fn.editable.defaults.mode = 'inline';

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

var search = debounce(function (value, type) {
    switch (type) {
        case "Number":
            tableInitPending.column("numberOrAgencyTransferida:name").search(value).draw();
            tableCompletada.column("numberOrAgencyTransferida:name").search(value).draw();
            tableEnviada.column("numberOrAgencyTransferida:name").search(value).draw();
            tableDespachada.column("numberOrAgencyTransferida:name").search(value).draw();
            tableRecibida.column("numberOrAgencyTransferida:name").search(value).draw();
            tableRevisada.column("numberOrAgencyTransferida:name").search(value).draw();
            tableEntregada.column("numberOrAgencyTransferida:name").search(value).draw();
            tableIncompleta.column("numberOrAgencyTransferida:name").search(value).draw();
            break;
        case "ClientFullData":
            tableInitPending.column("clientFullData:name").search(value).draw();
            tableCompletada.column("clientFullData:name").search(value).draw();
            tableEnviada.column("clientFullData:name").search(value).draw();
            tableDespachada.column("clientFullData:name").search(value).draw();
            tableRecibida.column("clientFullData:name").search(value).draw();
            tableRevisada.column("clientFullData:name").search(value).draw();
            tableEntregada.column("clientFullData:name").search(value).draw();
            tableIncompleta.column("clientFullData:name").search(value).draw();
            break;
        case "ContactFullData":
            tableInitPending.column("contactFullData:name").search(value).draw();
            tableCompletada.column("contactFullData:name").search(value).draw();
            tableEnviada.column("contactFullData:name").search(value).draw();
            tableDespachada.column("contactFullData:name").search(value).draw();
            tableRecibida.column("contactFullData:name").search(value).draw();
            tableRevisada.column("contactFullData:name").search(value).draw();
            tableEntregada.column("contactFullData:name").search(value).draw();
            tableIncompleta.column("contactFullData:name").search(value).draw();
            break;
        case "Bags":
            tableInitPending.column("bags:name").search(value).draw();
            tableCompletada.column("bags:name").search(value).draw();
            tableEnviada.column("bags:name").search(value).draw();
            tableDespachada.column("bags:name").search(value).draw();
            tableRecibida.column("bags:name").search(value).draw();
            tableRevisada.column("bags:name").search(value).draw();
            tableEntregada.column("bags:name").search(value).draw();
            tableIncompleta.column("bags:name").search(value).draw();
            break;
        case "Shippings":
            tableInitPending.column("shippings:name").search(value).draw();
            tableCompletada.column("shippings:name").search(value).draw();
            tableEnviada.column("shippings:name").search(value).draw();
            tableDespachada.column("shippings:name").search(value).draw();
            tableRecibida.column("shippings:name").search(value).draw();
            tableRevisada.column("shippings:name").search(value).draw();
            tableEntregada.column("shippings:name").search(value).draw();
            tableIncompleta.column("shippings:name").search(value).draw();
            break;
        case "Product":
            tableInitPending.column(0).search(value).draw();
            tableCompletada.column(0).search(value).draw();
            tableEnviada.column(0).search(value).draw();
            tableDespachada.column(0).search(value).draw();
            tableRecibida.column(0).search(value).draw();
            tableRevisada.column(0).search(value).draw();
            tableEntregada.column(0).search(value).draw();
            tableIncompleta.column(0).search(value).draw();
            break;
        default:
            break;
    }
}, 350);

var ClearFiltersTables = debounce(function () {

    tableInitPending.columns().search("").draw();
    tableCompletada.columns().search("").draw();
    tableEnviada.columns().search("").draw();
    tableDespachada.columns().search("").draw();
    tableRecibida.columns().search("").draw();
    tableRevisada.columns().search("").draw();
    tableEntregada.columns().search("").draw();
    tableIncompleta.columns().search("").draw();
}, 350);

var rechargeTables = () => {
    tableInitPending.draw();
    tableCompletada.draw();
    tableEnviada.draw();
    tableDespachada.draw();
    tableRecibida.draw();
    tableRevisada.draw();
    tableEntregada.draw();
    tableIncompleta.draw();
}

$.fn.dataTable.ext.errMode = 'none'; //No mostrar warning de alerta de dataTable

$(window).on("load", () => {
    var prevTab = $("#" + currentTab);
    var prevContainer = $(prevTab).data("table");

    var block_ele = $("#" + prevContainer + "_div");
    $(block_ele).block(blockOptions);

    ;

    LoadTableIniciada();
    LoadOtherTables();

    $(block_ele).unblock();

    LoadEvents();
})

function LoadTableIniciada() {
    var tableName = "Iniciada";
    const baseTabId = `#base_tab${tableName}`
    var t = $('#tableIniciada_Pendiente');
    tableInitPending = t.DataTable({
        searching: true,
        dom: "ltip",
        lengthChange: false,
        order: [[1, "desc"]],
        serverSide: true,
        processing: false,
        language: tableLanguage,
        ajax: $.fn.dataTable.pipeline({
            url: `/airshipping/GetData?status=Iniciada`,
            type: 'POST',
            pages: numberPages, // number of pages to cache,
            data: function (dtp) {
                return dtp;
            },
            dataFilter: function (data) {
                var json = jQuery.parseJSON(data);
                $("#base_tabIniciada").html(`Iniciadas (${json.recordsTotal})`);
                return JSON.stringify(json); // return JSON string
            },
            statusCode: {
                401: function () {
                    location.href = "/Account/Login"
                }
            }
        }),
        columnDefs: [
            {
                targets: [0],
                data: "id",
                orderable: false,
                render: function (data, type, item) {
                    var html = '<label class="custom-control custom-checkbox">' +
                        `<input type="checkbox" class="custom-control-input order-select" data-isComodin="${item.wholesalerComodin}" data-type="Paquete" data-id=${item.id} value="${item.number}" />` +
                        '<span class="custom-control-indicator"></span>' +
                        '<span class="custom-control-description"></span>' +
                        '</label>';
                    return html;
                }
            },
            {
                targets: [1],
                data: "createdAt",
                render: function (data, type, item) {
                    const init = new moment(data);
                    const end = new moment();
                    var duration = moment.duration(end.diff(init));
                    var html = [];
                    html.push(moment(data).format(formatDate));
                    html.push(`<div><span class="tag tag-default tag-warning">${duration.days()}d ${duration.hours()}h ${duration.minutes()}m</span></div>`);
                    return html.join("");
                }
            },
            {
                targets: [2],
                data: "number",
                name: "numberOrAgencyTransferida",
                render: function (data, type, item) {
                    var html = [];
                    html.push(`<a href="/orders/details/${item.id}">${data}</a>`);
                    if (item.userFirstName) {
                        html.push(`<div>${item.userFirstName}</div>`)
                    }
                    if (item.noOrden) {
                        html.push(`<p style="color:red">${item.noOrden}</p>`)
                    }

                    if (item.agencyTransferidaId != null) {
                        if (item.agencyTransferidaId == agencyId) {
                            html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.agencyName}</div>`);
                        }
                        else {
                            html.push(`<div style="display: block !important;" class="tag tag-info">T. a ${item.agencyTransferredName}</div>`);
                        }
                    }
                    if (item.express) {
                        html.push('<div class="tag tag-danger">Express</div>');
                    }
                    if (item.problem) {
                        html.push('<div class="tag tag-danger">Problema</div>');
                    }


                    return html.join("");
                },
            },
            {
                targets: [3],
                visible: true,
                data: "clientFullData",
                name: "clientFullData",
                render: function (data, type, item) {
                    var html = [];
                    html.push(`<div title="${item.clientFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">`);
                    html.push(`${item.clientFullData}`);
                    html.push(`</div>`)
                    html.push(`<div>${item.clientPhoneNumber}</div>`)
                    return html.join("");
                }
            },
            {
                targets: [4],
                visible: true,
                data: "bags",
                name: "bags",
                render: function (data, type, item) {
                    var html = [];
                    for (var i = 0; i < data.length; i++) {
                        html.push(`<div>${data[i].number}</div>`)
                    }
                    // add province
                    if (item.contactAddressProvince) {
                        html.push(`<div>${item.contactAddressProvince}</div>`)
                    }
                    return html.join("");
                },
            },
            {
                targets: [5],
                data: "contactFullData",
                name: "contactFullData",
                render: function (data, type, item) {
                    return `<div title="${item.contactFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">${item.contactFullData}</div>`
                }
            },
            {
                targets: [6],
                data: "type",
            },
            {
                targets: [7],
                data: "status",
                render: function (data, type, item) {
                    var html = [];
                    if (item.agencyTransferidaId != null) {
                        if (item.agencyTransferidaId != agencyId) {
                            if (item.status == "Iniciada") {
                                html.push(`<p style="color:orange;margin-top:0px;margin-bottom:0px;display:inline;">${item.status}</p>`);
                            }
                            else if (item.status == "Pendiente") {
                                html.push(`<p style="color:darkred;margin-top:0px;margin-bottom:0px;display:inline;">${item.status}</p>`)
                            }
                        }
                        else {
                            if (item.status == "Iniciada") {
                                html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="1" data-pk="1" data-url="/post" data-title="Select status"></a>`)
                            }
                            else if (item.status == "Pendiente") {
                                html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="2" data-pk="2" data-url="/post" data-title="Select status"></a>`)
                            }
                        }
                    }
                    else {
                        if (item.status == "Iniciada") {
                            html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="1" data-pk="1" data-url="/post" data-title="Select status"></a>`);
                        }
                        else if (item.status == "Pendiente") {
                            html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="2" data-pk="2" data-url="/post" data-title="Select status"></a>`)
                        }
                    }

                    if (item.lackSend || item.lackReview) {
                        html.push(`<div style="display: block !important;margin-top:3px;width:30px;" class="tag tag-danger">Parcial</div>`);
                    }
                    html.push("<br/>")
                    if (item.balance > 0) {
                        html.push(`<div class="tag tag-danger">Pendiente</div>`)
                    }

                    return html.join("");
                }
            },
            {
                targets: [8],
                data: "amount",
                render: function (data, type, item) {
                    var html = [];
                    if (item.agencyTransferidaId == agencyId) {
                        html.push(`<text>${parseFloat(item.wholesalerCost).toFixed(2)}</text >`)
                    }
                    else {
                        html.push(`<text>${parseFloat(item.amount).toFixed(2)}</text >`)
                    }

                    return html.join("");
                },
            },
            {
                targets: [9],
                data: "id",
                render: function (data, type, item) {
                    var html = [];
                    html.push('<div class="dropdown">');
                    html.push('<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>');
                    html.push(`<div class="dropdown-menu" aria-label="dropdown${item.number}">`);
                    html.push(`<a class="dropdown-item " href="/OrderNew/ViewBag/${data}"><i class="ft-layout"></i>Ver Bolsas</a>`);
                    if (item.balance > 0) {
                        html.push(`<a class="dropdown-item" href="/Orders/PayOrder?orderNumber=${item.number}"><i class="fa fa-usd"></i>Gestionar pago</a>`);
                    }
                    html.push(`<a class="dropdown-item" href="/Orders/PaysOrder?id=${data}"><i class="icon-wallet"></i>Registro de pagos</a>`);
                    if (agencyId !== agencyMdl || (agencyId === agencyMdl && userRole == "Agencia")) {
                        if (agencyId == agencyUniversalTravel) {
                            if (userRole == "Agencia") {
                                html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`);
                            }
                        }
                        else {
                            html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`);
                        }
                    }
                    html.push(`<a class="dropdown-item" href="/OrderNew/TrackingOrden?id=${data}"><i class="icon-list"></i>Tracking Orden</a>`);
                    html.push(`<a class="dropdown-item print_report" data-orderid="${data}"><i class="fa fa-file-pdf-o"></i>Imprimir Envío</a>`);

                    if (agencyId == agencyUniversalTravel) {
                        if (userRole == "Agencia") {
                            html.push(`<a class="dropdown-item" href="/OrderNew/Edit?id=${data}"><i class="ft-edit"></i>Editar</a>`);
                        }
                    }
                    else {
                        html.push(`<a class="dropdown-item" href="/OrderNew/Edit?id=${data}"><i class="ft-edit"></i>Editar</a>`);
                    }
                    html.push(`<a class="dropdown-item" href="/OrderNew/EditBolsa?id=${data}"><i class="fa fa-shopping-bag"></i>Editar Bolsas</a>`);
                    html.push(`<a class="dropdown-item" href="/Orders/Details?id=${data}"><i class="ft-info"></i>Detalles</a>`);
                    html.push(`<a class="dropdown-item" href="/Reclamo/Create?type=2&orderNumber=${item.number}&idClient=${item.clientId}"><i class="ft-file"></i>Crear Ticket</a>`);
                    html.push(`<a class="dropdown-item" href="#" name="btnChangeContact" data-id="${data}" data-clientId="${item.clientId}"><i class="ft-user"></i>Cambiar Contacto</a>`);
                    if (agencyId == agencyUniversalTravel) {
                        if (userRole == "Agencia") {
                            html.push(`<a class="dropdown-item" href="#" name="cancel" data-type="order" data-id="${data}"><i class="ft-x"></i>Cancelar</a>`);
                        }
                    }
                    else {
                        html.push(`<a class="dropdown-item" href="#" name="cancel" data-type="order" data-id="${data}"><i class="ft-x"></i>Cancelar</a>`);
                    }
                    html.push('</div>');
                    html.push('</div>');
                    return html.join("");
                },
            },

        ],
        createdRow: function (row, data, rowIndex) {
            InitRow(row);
        },
        initComplete: function (settings, json) {
            $(baseTabId).unblock();
            $(baseTabId).parent().removeClass("disabled");
            loadInitSuccess = true;
        }
    });
    $.each($(".searchColumn", t), (i, e) => {
        $(e).on('keyup', (e) => searchColumn(e, tableInitPending));
    })

    if (!tableName.includes("Cancelada")) {
        $(baseTabId).block(blockOptions);
        $(baseTabId).parent().addClass("disabled");
    }
    if (!tableName.includes("Cancelada")) {
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
}

async function LoadOtherTables() {
    while (!loadInitSuccess)
    {
        await new Promise(r => setTimeout(r, 1000));
    }

    for (var i = 0; i < allTables.length; i++) {
        var tableName = allTables[i];
        const baseTabId = `#base_tab${tableName}`
        switch (tableName) {
            case "Completada":
                var t = $('#tableCompletada');
                tableCompletada = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: false,
                    order: [[1, "desc"]],
                    serverSide: true,
                    processing: false,
                    language: tableLanguage,
                    ajax: $.fn.dataTable.pipeline({
                        url: `/airshipping/GetData?status=Completada`,
                        type: 'POST',
                        pages: numberPages, // number of pages to cache,
                        data: function (dtp) {
                            return dtp;
                        },
                        dataFilter: function (data) {
                            var json = jQuery.parseJSON(data);
                            $('#base_tabCompletada').html(`Completadas (${json.recordsTotal})`);
                            return JSON.stringify(json); // return JSON string
                        },
                        statusCode: {
                            401: function () {
                                location.href = "/Account/Login"
                            }
                        }
                    }),
                    columnDefs: [
                        {
                            targets: [0],
                            data: "id",
                            render: function (data, type, item) {
                                var html = '<label class="custom-control custom-checkbox">' +
                                    `<input type="checkbox" class="custom-control-input order-select" data-isComodin="${item?.wholesalerComodin}" data-type="Paquete" value="${item.number}" />` +
                                    '<span class="custom-control-indicator"></span>' +
                                    '<span class="custom-control-description"></span>' +
                                    '</label>';
                                return html;
                            }
                        },
                        {
                            targets: [1],
                            data: "createdAt",
                            render: function (data, type, item) {
                                const init = new moment(data);
                                const end = new moment();
                                var duration = moment.duration(end.diff(init));
                                var html = [];
                                html.push(moment(data).format(formatDate));
                                html.push(`<div><span class="tag tag-default tag-warning">${duration.days()}d ${duration.hours()}h ${duration.minutes()}m</span></div>`);
                                return html.join("");
                            }
                        },
                        {
                            targets: [2],
                            data: "number",
                            name: "numberOrAgencyTransferida",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a href="/orders/details/${item.id}">${data}</a>`);
                                if (item.userFirstName) {
                                    html.push(`<div>${item.userFirstName}</div>`)
                                }
                                if (item.noOrden) {
                                    html.push(`<p style="color:red">${item.noOrden}</p>`)
                                }

                                if (item.agencyTransferidaId != null) {
                                    if (item.agencyTransferidaId == agencyId) {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.agencyName}</div>`);
                                    }
                                    else {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. a ${item.agencyTransferredName}</div>`);
                                    }
                                }
                                if (item.express) {
                                    html.push('<div class="tag tag-danger">Express</div>');
                                }

                                if (item.problem) {
                                    html.push('<div class="tag tag-danger">Problema</div>');
                                }

                                return html.join("");
                            },
                        },
                        {
                            targets: [3],
                            visible: true,
                            data: "clientFullData",
                            name: "clientFullData",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div title="${item.clientFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">`);
                                html.push(`${item.clientFullData}`);
                                html.push(`</div>`)
                                html.push(`<div>${item.clientPhoneNumber}</div>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [4],
                            visible: true,
                            data: "bags",
                            name: "bags",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    html.push(`<div>${data[i].number}</div>`)
                                }
                                if (item.contactAddressProvince) {
                                    html.push(`<div>${item.contactAddressProvince}</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [5],
                            data: "contactFullData",
                            name: "contactFullData",
                            render: function (data, type, item) {
                                return `<div title="${item.contactFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">${item.contactFullData}</div>`
                            }
                        },
                        {
                            targets: [6],
                            data: "type",
                        },
                        {
                            targets: [7],
                            data: "status",
                            render: function (data, type, item) {
                                var html = [];
                                if (item.agencyTransferidaId != null) {
                                    if (item.agencyTransferidaId != agencyId) {
                                        if (item.status == "Iniciada") {
                                            html.push(`<p style="color:orange;margin-top:0px;margin-bottom:0px;display:inline;">${item.status}</p>`);
                                        }
                                        else if (item.status == "Pendiente") {
                                            html.push(`<p style="color:darkred;margin-top:0px;margin-bottom:0px;display:inline;">${item.status}</p>`)
                                        }
                                        else if (item.status == "Completada") {
                                            html.push(`<p style="color:blue;margin-top:0px;margin-bottom:0px;display:inline;">${item.status}</p>`)
                                        }
                                    }
                                    else {
                                        if (item.status == "Iniciada") {
                                            html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="1" data-pk="1" data-url="/post" data-title="Select status"></a>`)
                                        }
                                        else if (item.status == "Pendiente") {
                                            html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="2" data-pk="2" data-url="/post" data-title="Select status"></a>`)
                                        }
                                        else if (item.status == "Completada") {
                                            html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="4" data-pk="4" data-url="/post" data-title="Select status"></a>`)
                                        }
                                    }
                                }
                                else {
                                    if (item.status == "Iniciada") {
                                        html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="1" data-pk="1" data-url="/post" data-title="Select status"></a>`);
                                    }
                                    else if (item.status == "Pendiente") {
                                        html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="2" data-pk="2" data-url="/post" data-title="Select status"></a>`)
                                    }
                                    else if (item.status == "Completada") {
                                        html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="4" data-pk="4" data-url="/post" data-title="Select status"></a>`)
                                    }
                                }

                                if (item.lackSend || item.lackReview) {
                                    html.push(`<div style="display: block !important;margin-top:3px;width:30px;" class="tag tag-danger">Parcial</div>`);
                                }
                                html.push("<br/>")
                                if (item.balance > 0) {
                                    html.push(`<div class="tag tag-danger">Pendiente</div>`)
                                }

                                return html.join("");
                            }
                        },
                        {
                            targets: [8],
                            data: "shippings",
                            name: "shippings",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    const item = data[i];
                                    html.push(`<div><a href="/Shippings/Details/${item.id}">${item.number}</a></div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [9],
                            data: "id",
                            render: function (data, type, item) {
                                var html = [];
                                html.push('<div class="dropdown">');
                                html.push('<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>');
                                html.push(`<div class="dropdown-menu" aria-label="dropdown${item.number}">`);
                                html.push(`<a class="dropdown-item " href="/OrderNew/ViewBag/${data}"><i class="ft-layout"></i>Ver Bolsas</a>`);
                                if (item.balance > 0) {
                                    html.push(`<a class="dropdown-item" href="/Orders/PayOrder?orderNumber=${item.number}"><i class="fa fa-usd"></i>Gestionar pago</a>`);
                                }
                                html.push(`<a class="dropdown-item" href="/Orders/PaysOrder?id=${data}"><i class="icon-wallet"></i>Registro de pagos</a>`);
                                if (agencyId !== agencyMdl || (agencyId === agencyMdl && userRole == "Agencia")) {
                                    if (agencyId == agencyUniversalTravel) {
                                        if (userRole == "Agencia") {
                                            html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`);
                                        }
                                    }
                                    else {
                                        html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`);
                                    }
                                }
                                html.push(`<a class="dropdown-item" href="/OrderNew/TrackingOrden?id=${data}"><i class="icon-list"></i>Tracking Orden</a>`);
                                html.push(`<a class="dropdown-item print_report" data-orderid="${data}"><i class="fa fa-file-pdf-o"></i>Imprimir Envío</a>`);
                                html.push(`<a class="dropdown-item" href="/OrderNew/EditBolsa?id=${data}"><i class="fa fa-shopping-bag"></i>Editar Bolsas</a>`);
                                html.push(`<a class="dropdown-item" href="/Orders/Details?id=${data}"><i class="ft-info"></i>Detalles</a>`);
                                html.push(`<a class="dropdown-item" href="/Reclamo/Create?type=2&orderNumber=${item.number}&idClient=${item.clientId}"><i class="ft-file"></i>Crear Ticket</a>`);
                                html.push(`<a class="dropdown-item" href="#" name="btnChangeContact" data-id="${data}" data-clientId="${item.clientId}"><i class="ft-user"></i>Cambiar Contacto</a>`);
                                html.push('</div>');
                                html.push('</div>');
                                return html.join("");
                            },
                        },

                    ],
                    createdRow: function (row, data, rowIndex) {
                        InitRow(row);
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");

                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableCompletada));
                })
                break;
            case "Enviada":
                var t = $('#tableEnviada');
                tableEnviada = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: false,
                    order: [[1, "desc"]],
                    serverSide: true,
                    processing: false,
                    language: tableLanguage,
                    ajax: $.fn.dataTable.pipeline({
                        url: `/airshipping/GetData?status=Enviada`,
                        type: 'POST',
                        pages: numberPages, // number of pages to cache,
                        data: function (dtp) {
                            return dtp;
                        },
                        dataFilter: function (data) {
                            var json = jQuery.parseJSON(data);
                            $('#base_tabEnviada').html(`Enviadas (${json.recordsTotal})`);
                            return JSON.stringify(json); // return JSON string
                        },
                        statusCode: {
                            401: function () {
                                location.href = "/Account/Login"
                            }
                        }
                    }),
                    columnDefs: [
                        {
                            targets: [0],
                            data: "id",
                            render: function (data, type, item) {
                                var html = '<label class="custom-control custom-checkbox">' +
                                    `<input type="checkbox" class="custom-control-input order-select" data-isComodin="${item.wholesalerComodin}" data-type="Paquete" value="${item.number}" />` +
                                    '<span class="custom-control-indicator"></span>' +
                                    '<span class="custom-control-description"></span>' +
                                    '</label>';
                                return html;
                            }
                        },
                        {
                            targets: [1],
                            data: "shippingDate",
                            render: function (data, type, item) {
                                const init = new moment(item.createdAt);
                                const end = new moment();
                                var duration = moment.duration(end.diff(init));
                                var html = [];
                                html.push(moment(data).format(formatDate));
                                html.push(`<div><span class="tag tag-default tag-warning">${duration.days()}d ${duration.hours()}h ${duration.minutes()}m</span></div>`);
                                return html.join("");
                            }
                        },
                        {
                            targets: [2],
                            data: "number",
                            name: "numberOrAgencyTransferida",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a href="/orders/details/${item.id}">${data}</a>`);
                                if (item.userFirstName) {
                                    html.push(`<div>${item.userFirstName}</div>`)
                                }
                                if (item.noOrden) {
                                    html.push(`<p style="color:red">${item.noOrden}</p>`)
                                }

                                if (item.agencyTransferidaId != null) {
                                    if (item.agencyTransferidaId == agencyId) {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.agencyName}</div>`);
                                    }
                                    else {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. a ${item.agencyTransferredName}</div>`);
                                    }
                                }
                                if (item.express) {
                                    html.push('<div class="tag tag-danger">Express</div>');
                                }
                                if (item.problem) {
                                    html.push('<div class="tag tag-danger">Problema</div>');
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [3],
                            visible: true,
                            data: "clientFullData",
                            name: "clientFullData",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div title="${item.clientFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">`);
                                html.push(`${item.clientFullData}`);
                                html.push(`</div>`)
                                html.push(`<div>${item.clientPhoneNumber}</div>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [4],
                            visible: true,
                            data: "bags",
                            name: "bags",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    html.push(`<div>${data[i].number}</div>`)
                                }
                                if (item.contactAddressProvince) {
                                    html.push(`<div>${item.contactAddressProvince}</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [5],
                            data: "contactFullData",
                            name: "contactFullData",
                            render: function (data, type, item) {
                                return `<div title="${item.contactFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">${item.contactFullData}</div>`
                            }
                        },
                        {
                            targets: [6],
                            data: "type",
                        },
                        {
                            targets: [7],
                            data: "status",
                            render: function (data, type, item) {
                                var html = [];
                                if (item.agencyTransferidaId != null) {
                                    if (item.agencyTransferidaId != agencyId) {
                                        html.push(`<p style="color:gold;margin-top:0px;margin-bottom:0px;display:inline;">${item.status}</p>`);
                                    }
                                    else {
                                        if (item.status == "Enviada") {
                                            html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="5" data-pk="5" data-url="/post" data-title="Select status"></a>`)
                                        }
                                    }
                                }
                                else {
                                    if (item.status == "Enviada") {
                                        html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="5" data-pk="5" data-url="/post" data-title="Select status"></a>`)
                                    }
                                }

                                if (item.lackSend || item.lackReview) {
                                    html.push(`<div style="display: block !important;margin-top:3px;width:30px;" class="tag tag-danger">Parcial</div>`);
                                }
                                html.push("<br/>")
                                if (item.balance > 0) {
                                    html.push(`<div class="tag tag-danger">Pendiente</div>`)
                                }

                                return html.join("");
                            }
                        },
                        {
                            targets: [8],
                            data: "shippings",
                            name: "shippings",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    const item = data[i];
                                    html.push(`<div><a href="/Shippings/Details/${item.id}">${item.number}</a></div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [9],
                            data: "id",
                            render: function (data, type, item) {
                                var html = [];
                                html.push('<div class="dropdown">');
                                html.push('<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>');
                                html.push(`<div class="dropdown-menu" aria-label="dropdown${item.number}">`);
                                html.push(`<a class="dropdown-item " href="/OrderNew/ViewBag/${data}"><i class="ft-layout"></i>Ver Bolsas</a>`);
                                if (item.balance > 0) {
                                    html.push(`<a class="dropdown-item" href="/Orders/PayOrder?orderNumber=${item.number}"><i class="fa fa-usd"></i>Gestionar pago</a>`);
                                }
                                html.push(`<a class="dropdown-item" href="/Orders/PaysOrder?id=${data}"><i class="icon-wallet"></i>Registro de pagos</a>`);
                                if (agencyId !== agencyMdl || (agencyId === agencyMdl && userRole == "Agencia")) {
                                    if (agencyId == agencyUniversalTravel) {
                                        if (userRole == "Agencia") {
                                            html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`);
                                        }
                                    }
                                    else {
                                        html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`);
                                    }
                                }
                                if (item.problem) {
                                    html.push(`<a class="dropdown-item confirm" data-orderid="${data}"><i class="fa fa-check"></i>Confirmar</a>`);
                                }
                                html.push(`<a class="dropdown-item" href="/OrderNew/TrackingOrden?id=${data}"><i class="icon-list"></i>Tracking Orden</a>`);
                                html.push(`<a class="dropdown-item print_report" data-orderid="${data}"><i class="fa fa-file-pdf-o"></i>Imprimir Envío</a>`);
                                html.push(`<a class="dropdown-item" href="/OrderNew/EditBolsa?id=${data}"><i class="fa fa-shopping-bag"></i>Editar Bolsas</a>`);
                                html.push(`<a class="dropdown-item" href="/Orders/Details?id=${data}"><i class="ft-info"></i>Detalles</a>`);
                                html.push(`<a class="dropdown-item" href="/Reclamo/Create?type=2&orderNumber=${item.number}&idClient=${item.clientId}"><i class="ft-file"></i>Crear Ticket</a>`);
                                html.push(`<a class="dropdown-item" href="#" name="btnChangeContact" data-id="${data}" data-clientId="${item.clientId}"><i class="ft-user"></i>Cambiar Contacto</a>`);
                                html.push('</div>');
                                html.push('</div>');
                                return html.join("");
                            },
                        },

                    ],
                    createdRow: function (row, data, rowIndex) {
                        InitRow(row);
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableEnviada));
                })
                break;
            case "Despachada":
                var t = $('#tableDespachada');
                tableDespachada = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: false,
                    order: [[1, "desc"]],
                    serverSide: true,
                    processing: false,
                    language: tableLanguage,
                    ajax: $.fn.dataTable.pipeline({
                        url: `/airshipping/GetData?status=Despachada`,
                        type: 'POST',
                        pages: numberPages, // number of pages to cache,
                        data: function (dtp) {
                            return dtp;
                        },
                        dataFilter: function (data) {
                            var json = jQuery.parseJSON(data);
                            $('#base_tabDespachada').html(`Despachadas (${json.recordsTotal})`);
                            return JSON.stringify(json); // return JSON string
                        },
                        statusCode: {
                            401: function () {
                                location.href = "/Account/Login"
                            }
                        }
                    }),
                    columnDefs: [
                        {
                            targets: [0],
                            data: "id",
                            render: function (data, type, item) {
                                var html = '<label class="custom-control custom-checkbox">' +
                                    `<input type="checkbox" class="custom-control-input order-select" data-isComodin="${item.wholesalerComodin}" data-type="Paquete" value="${item.number}" />` +
                                    '<span class="custom-control-indicator"></span>' +
                                    '<span class="custom-control-description"></span>' +
                                    '</label>';
                                return html;
                            }
                        },
                        {
                            targets: [1],
                            data: "dispatchDate",
                            render: function (data, type, item) {
                                const init = new moment(item.createdAt);
                                const end = new moment();
                                var duration = moment.duration(end.diff(init));
                                var html = [];
                                html.push(moment(data).format(formatDate));
                                html.push(`<div><span class="tag tag-default tag-warning">${duration.days()}d ${duration.hours()}h ${duration.minutes()}m</span></div>`);
                                return html.join("");
                            }
                        },
                        {
                            targets: [2],
                            data: "number",
                            name: "numberOrAgencyTransferida",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a href="/orders/details/${item.id}">${data}</a>`);
                                if (item.userFirstName) {
                                    html.push(`<div>${item.userFirstName}</div>`)
                                }
                                if (item.noOrden) {
                                    html.push(`<p style="color:red">${item.noOrden}</p>`)
                                }

                                if (item.agencyTransferidaId != null) {
                                    if (item.agencyTransferidaId == agencyId) {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.agencyName}</div>`);
                                    }
                                    else {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. a ${item.agencyTransferredName}</div>`);
                                    }
                                }
                                if (item.express) {
                                    html.push('<div class="tag tag-danger">Express</div>');
                                }
                                if (item.problem) {
                                    html.push('<div class="tag tag-danger">Problema</div>');
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [3],
                            visible: true,
                            data: "clientFullData",
                            name: "clientFullData",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div title="${item.clientFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">`);
                                html.push(`${item.clientFullData}`);
                                html.push(`</div>`)
                                html.push(`<div>${item.clientPhoneNumber}</div>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [4],
                            visible: true,
                            data: "bags",
                            name: "bags",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    html.push(`<div>${data[i].number}</div>`)
                                }
                                if (item.contactAddressProvince) {
                                    html.push(`<div>${item.contactAddressProvince}</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [5],
                            data: "contactFullData",
                            name: "contactFullData",
                            render: function (data, type, item) {
                                return `<div title="${item.contactFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">${item.contactFullData}</div>`
                            }
                        },
                        {
                            targets: [6],
                            data: "principalDistributorFullName",
                            render: function (data, type, item) {
                                if (data)
                                    return data;
                                return "";
                            }
                        },
                        {
                            targets: [7],
                            data: "type",
                        },
                        {
                            targets: [8],
                            data: "status",
                            render: function (data, type, item) {
                                var html = [];
                                if (item.agencyTransferidaId != null) {
                                    if (item.agencyTransferidaId != agencyId) {
                                        html.push(`<p style="color:gold;margin-top:0px;margin-bottom:0px;display:inline;">${item.status}</p>`);
                                    }
                                    else {
                                        if (item.status == "Despachada") {
                                            html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="6" data-pk="6" data-url="/post" data-title="Select status"></a>`)
                                        }
                                    }
                                }
                                else {
                                    if (item.status == "Despachada") {
                                        html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="6" data-pk="6" data-url="/post" data-title="Select status"></a>`)
                                    }
                                }

                                if (item.lackSend || item.lackReview) {
                                    html.push(`<div style="display: block !important;margin-top:3px;width:30px;" class="tag tag-danger">Parcial</div>`);
                                }
                                html.push("<br/>")
                                if (item.balance > 0) {
                                    html.push(`<div class="tag tag-danger">Pendiente</div>`)
                                }

                                return html.join("");
                            }
                        },
                        {
                            targets: [9],
                            data: "shippings",
                            name: "shippings",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    const item = data[i];
                                    html.push(`<div><a href="/Shippings/Details/${item.id}">${item.number}</a></div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [10],
                            data: "id",
                            render: function (data, type, item) {
                                var html = [];
                                html.push('<div class="dropdown">');
                                html.push('<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>');
                                html.push(`<div class="dropdown-menu" aria-label="dropdown${item.number}">`);
                                html.push(`<a class="dropdown-item " href="/OrderNew/ViewBag/${data}"><i class="ft-layout"></i>Ver Bolsas</a>`);
                                if (item.balance > 0) {
                                    html.push(`<a class="dropdown-item" href="/Orders/PayOrder?orderNumber=${item.number}"><i class="fa fa-usd"></i>Gestionar pago</a>`);
                                }
                                html.push(`<a class="dropdown-item" href="/Orders/PaysOrder?id=${data}"><i class="icon-wallet"></i>Registro de pagos</a>`);
                                if (agencyId !== agencyMdl || (agencyId === agencyMdl && userRole == "Agencia")) {
                                    if (agencyId == agencyUniversalTravel) {
                                        if (userRole == "Agencia") {
                                            html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`);
                                        }
                                    }
                                    else {
                                        html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`);
                                    }
                                }
                                if (item.problem) {
                                    html.push(`<a class="dropdown-item confirm" data-orderid="${data}"><i class="fa fa-check"></i>Confirmar</a>`);
                                }
                                html.push(`<a class="dropdown-item" href="/OrderNew/TrackingOrden?id=${data}"><i class="icon-list"></i>Tracking Orden</a>`);
                                html.push(`<a class="dropdown-item print_report" data-orderid="${data}"><i class="fa fa-file-pdf-o"></i>Imprimir Envío</a>`);
                                html.push(`<a class="dropdown-item" href="/OrderNew/EditBolsa?id=${data}"><i class="fa fa-shopping-bag"></i>Editar Bolsas</a>`);
                                html.push(`<a class="dropdown-item" href="/Orders/Details?id=${data}"><i class="ft-info"></i>Detalles</a>`);
                                html.push(`<a class="dropdown-item" href="/Reclamo/Create?type=2&orderNumber=${item.number}&idClient=${item.clientId}"><i class="ft-file"></i>Crear Ticket</a>`);
                                html.push(`<a class="dropdown-item" href="#" name="btnChangeContact" data-id="${data}" data-clientId="${item.clientId}"><i class="ft-user"></i>Cambiar Contacto</a>`);
                                html.push('</div>');
                                html.push('</div>');
                                return html.join("");
                            },
                        },

                    ],
                    createdRow: function (row, data, rowIndex) {
                        InitRow(row);
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableDespachada));
                })
                break;
            case "Recibida":
                var t = $('#tableRecibida');
                tableRecibida = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: false,
                    order: [[1, "desc"]],
                    serverSide: true,
                    processing: false,
                    language: tableLanguage,
                    ajax: $.fn.dataTable.pipeline({
                        url: `/airshipping/GetData?status=Recibida`,
                        type: 'POST',
                        pages: numberPages, // number of pages to cache,
                        data: function (dtp) {
                            return dtp;
                        },
                        dataFilter: function (data) {
                            var json = jQuery.parseJSON(data);
                            $('#base_tabRecibida').html(`Recibidas (${json.recordsTotal})`);
                            return JSON.stringify(json); // return JSON string
                        },
                        statusCode: {
                            401: function () {
                                location.href = "/Account/Login"
                            }
                        }
                    }),
                    columnDefs: [
                        {
                            targets: [0],
                            data: "id",
                            render: function (data, type, item) {
                                var html = '<label class="custom-control custom-checkbox">' +
                                    `<input type="checkbox" class="custom-control-input order-select" data-isComodin="${item.wholesalerComodin}" data-type="Paquete" value="${item.number}" />` +
                                    '<span class="custom-control-indicator"></span>' +
                                    '<span class="custom-control-description"></span>' +
                                    '</label>';
                                return html;
                            }
                        },
                        {
                            targets: [1],
                            data: "receivedDate",
                            render: function (data, type, item) {
                                const init = new moment(item.createdAt);
                                const end = new moment();
                                var duration = moment.duration(end.diff(init));
                                var html = [];
                                html.push(moment(data).format(formatDate));
                                html.push(`<div><span class="tag tag-default tag-warning">${duration.days()}d ${duration.hours()}h ${duration.minutes()}m</span></div>`);
                                return html.join("");
                            }
                        },
                        {
                            targets: [2],
                            data: "number",
                            name: "numberOrAgencyTransferida",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a href="/orders/details/${item.id}">${data}</a>`);
                                if (item.userFirstName) {
                                    html.push(`<div>${item.userFirstName}</div>`)
                                }
                                if (item.noOrden) {
                                    html.push(`<p style="color:red">${item.noOrden}</p>`)
                                }

                                if (item.agencyTransferidaId != null) {
                                    if (item.agencyTransferidaId == agencyId) {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.agencyName}</div>`);
                                    }
                                    else {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. a ${item.agencyTransferredName}</div>`);
                                    }
                                }
                                if (item.express) {
                                    html.push('<div class="tag tag-danger">Express</div>');
                                }
                                if (item.problem) {
                                    html.push('<div class="tag tag-danger">Problema</div>');
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [3],
                            visible: true,
                            data: "clientFullData",
                            name: "clientFullData",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div title="${item.clientFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">`);
                                html.push(`${item.clientFullData}`);
                                html.push(`</div>`)
                                html.push(`<div>${item.clientPhoneNumber}</div>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [4],
                            visible: true,
                            data: "bags",
                            name: "bags",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    html.push(`<div>${data[i].number}</div>`)
                                }
                                if (item.contactAddressProvince) {
                                    html.push(`<div>${item.contactAddressProvince}</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [5],
                            data: "contactFullData",
                            name: "contactFullData",
                            render: function (data, type, item) {
                                return `<div title="${item.contactFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">${item.contactFullData}</div>`
                            }
                        },
                        {
                            targets: [6],
                            data: "principalDistributorFullName",
                            render: function (data, type, item) {
                                if (data)
                                    return data;
                                return "";
                            }
                        },
                        {
                            targets: [7],
                            data: "type",
                        },
                        {
                            targets: [8],
                            data: "status",
                            render: function (data, type, item) {
                                var html = [];
                                if (item.agencyTransferidaId != null) {
                                    if (item.agencyTransferidaId != agencyId) {
                                        html.push(`<p style="color:gold;margin-top:0px;margin-bottom:0px;display:inline;">${item.status}</p>`);
                                    }
                                    else {
                                        if (item.status == "Recibida") {
                                            html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="9" data-pk="8" data-url="/post" data-title="Select status"></a>`)
                                        }
                                        else if (item.status == "No Entregada") {
                                            html.push('<p style="color:darkred;margin-top:0px;margin-bottom:0px;">@item.Status</p>')
                                        }
                                    }
                                }
                                else {
                                    if (item.status == "Recibida") {
                                        html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="9" data-pk="8" data-url="/post" data-title="Select status"></a>`)
                                    }
                                    else if (item.status == "No Entregada") {
                                        html.push('<p style="color:darkred;margin-top:0px;margin-bottom:0px;">@item.Status</p>')
                                    }
                                }

                                if (item.lackSend || item.lackReview) {
                                    html.push(`<div style="display: block !important;margin-top:3px;width:30px;" class="tag tag-danger">Parcial</div>`);
                                }
                                html.push("<br/>")
                                if (item.balance > 0) {
                                    html.push(`<div class="tag tag-danger">Pendiente</div>`)
                                }

                                return html.join("");
                            }
                        },
                        {
                            targets: [9],
                            data: "shippings",
                            name: "shippings",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    const item = data[i];
                                    html.push(`<div><a href="/Shippings/Details/${item.id}">${item.number}</a></div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [10],
                            data: "id",
                            render: function (data, type, item) {
                                var html = [];
                                html.push('<div class="dropdown">');
                                html.push('<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>');
                                html.push(`<div class="dropdown-menu" aria-label="dropdown${item.number}">`);
                                html.push(`<a class="dropdown-item " href="/OrderNew/ViewBag/${data}"><i class="ft-layout"></i>Ver Bolsas</a>`);
                                if (item.balance > 0) {
                                    html.push(`<a class="dropdown-item" href="/Orders/PayOrder?orderNumber=${item.number}"><i class="fa fa-usd"></i>Gestionar pago</a>`);
                                }
                                html.push(`<a class="dropdown-item" href="/Orders/PaysOrder?id=${data}"><i class="icon-wallet"></i>Registro de pagos</a>`);
                                if (agencyId !== agencyMdl || (agencyId === agencyMdl && userRole == "Agencia")) {
                                    if (agencyId == agencyUniversalTravel) {
                                        if (userRole == "Agencia") {
                                            html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`);
                                        }
                                    }
                                    else {
                                        html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`);
                                    }
                                }
                                if (item.problem) {
                                    html.push(`<a class="dropdown-item confirm" data-orderid="${data}"><i class="fa fa-check"></i>Confirmar</a>`);
                                }
                                html.push(`<a class="dropdown-item" href="/OrderNew/TrackingOrden?id=${data}"><i class="icon-list"></i>Tracking Orden</a>`);
                                html.push(`<a class="dropdown-item print_report" data-orderid="${data}"><i class="fa fa-file-pdf-o"></i>Imprimir Envío</a>`);
                                html.push(`<a class="dropdown-item" href="/OrderNew/EditBolsa?id=${data}"><i class="fa fa-shopping-bag"></i>Editar Bolsas</a>`);
                                html.push(`<a class="dropdown-item" href="/Orders/Details?id=${data}"><i class="ft-info"></i>Detalles</a>`);
                                html.push(`<a class="dropdown-item" href="/Reclamo/Create?type=2&orderNumber=${item.number}&idClient=${item.clientId}"><i class="ft-file"></i>Crear Ticket</a>`);
                                html.push(`<a class="dropdown-item" href="#" name="btnChangeContact" data-id="${data}" data-clientId="${item.clientId}"><i class="ft-user"></i>Cambiar Contacto</a>`);
                                html.push('</div>');
                                html.push('</div>');
                                return html.join("");
                            },
                        },

                    ],
                    createdRow: function (row, data, rowIndex) {
                        InitRow(row);
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableRecibida));
                })
                break;
            case "Incompleta":
                var t = $('#tableIncompleta');
                tableIncompleta = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: false,
                    order: [[1, "desc"]],
                    serverSide: true,
                    processing: false,
                    language: tableLanguage,
                    ajax: $.fn.dataTable.pipeline({
                        url: `/airshipping/GetData?status=Recibiendo`,
                        type: 'POST',
                        pages: numberPages, // number of pages to cache,
                        data: function (dtp) {
                            return dtp;
                        },
                        dataFilter: function (data) {
                            var json = jQuery.parseJSON(data);
                            $('#base_tabIncompleta').html(`Incompleta (${json.recordsTotal})`);
                            return JSON.stringify(json); // return JSON string
                        },
                        statusCode: {
                            401: function () {
                                location.href = "/Account/Login"
                            }
                        }
                    }),
                    columnDefs: [
                        {
                            targets: [0],
                            data: "id",
                            render: function (data, type, item) {
                                var html = '<label class="custom-control custom-checkbox">' +
                                    `<input type="checkbox" class="custom-control-input order-select" data-isComodin="${item.wholesalerComodin}" data-type="Paquete" value="${item.number}" />` +
                                    '<span class="custom-control-indicator"></span>' +
                                    '<span class="custom-control-description"></span>' +
                                    '</label>';
                                return html;
                            }
                        },
                        {
                            targets: [1],
                            data: "dispatchDate",
                            render: function (data, type, item) {
                                const init = new moment(item.createdAt);
                                const end = new moment();
                                var duration = moment.duration(end.diff(init));
                                var html = [];
                                html.push(moment(data).format(formatDate));
                                html.push(`<div><span class="tag tag-default tag-warning">${duration.days()}d ${duration.hours()}h ${duration.minutes()}m</span></div>`);
                                return html.join("");
                            }
                        },
                        {
                            targets: [2],
                            data: "number",
                            name: "numberOrAgencyTransferida",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a href="/orders/details/${item.id}">${data}</a>`);
                                if (item.userFirstName) {
                                    html.push(`<div>${item.userFirstName}</div>`)
                                }
                                if (item.noOrden) {
                                    html.push(`<p style="color:red">${item.noOrden}</p>`)
                                }

                                if (item.agencyTransferidaId != null) {
                                    if (item.agencyTransferidaId == agencyId) {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.agencyName}</div>`);
                                    }
                                    else {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. a ${item.agencyTransferredName}</div>`);
                                    }
                                }
                                if (item.express) {
                                    html.push('<div class="tag tag-danger">Express</div>');
                                }
                                if (item.problem) {
                                    html.push('<div class="tag tag-danger">Problema</div>');
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [3],
                            visible: true,
                            data: "clientFullData",
                            name: "clientFullData",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div title="${item.clientFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">`);
                                html.push(`${item.clientFullData}`);
                                html.push(`</div>`)
                                html.push(`<div>${item.clientPhoneNumber}</div>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [4],
                            visible: true,
                            data: "bags",
                            name: "bags",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    html.push(`<div>${data[i].number}</div>`)
                                }
                                if (item.contactAddressProvince) {
                                    html.push(`<div>${item.contactAddressProvince}</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [5],
                            data: "contactFullData",
                            name: "contactFullData",
                            render: function (data, type, item) {
                                return `<div title="${item.contactFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">${item.contactFullData}</div>`
                            }
                        },
                        {
                            targets: [6],
                            data: "principalDistributorFullName",
                            render: function (data, type, item) {
                                if (data)
                                    return data;
                                return "";
                            }
                        },
                        {
                            targets: [7],
                            data: "type",
                        },
                        {
                            targets: [8],
                            data: "status",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<p style="color:gold;margin-top:0px;margin-bottom:0px;display:inline;">${item.status}</p>`)

                                if (item.lackSend || item.lackReview) {
                                    html.push(`<div style="display: block !important;margin-top:3px;width:30px;" class="tag tag-danger">Parcial</div>`);
                                }
                                html.push("<br/>")
                                if (item.balance > 0) {
                                    html.push(`<div class="tag tag-danger">Pendiente</div>`)
                                }

                                return html.join("");
                            }
                        },
                        {
                            targets: [9],
                            data: "shippings",
                            name: "shippings",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    const item = data[i];
                                    html.push(`<div><a href="/Shippings/Details/${item.id}">${item.number}</a></div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [10],
                            data: "id",
                            render: function (data, type, item) {
                                var html = [];
                                html.push('<div class="dropdown">');
                                html.push('<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>');
                                html.push(`<div class="dropdown-menu" aria-label="dropdown${item.number}">`);
                                if (item.problem) {
                                    html.push(`<a class="dropdown-item confirm" data-orderid="${data}"><i class="fa fa-check"></i>Confirmar</a>`);
                                }
                                html.push(`<a class="dropdown-item " href="/OrderNew/ViewBag/${data}"><i class="ft-layout"></i>Ver Bolsas</a>`);
                                if (item.balance > 0) {
                                    html.push(`<a class="dropdown-item" href="/Orders/PayOrder?orderNumber=${item.number}"><i class="fa fa-usd"></i>Gestionar pago</a>`);
                                }
                                html.push(`<a class="dropdown-item" href="/Orders/PaysOrder?id=${data}"><i class="icon-wallet"></i>Registro de pagos</a>`);
                                if (agencyId !== agencyMdl || (agencyId === agencyMdl && userRole == "Agencia")) {
                                    if (agencyId == agencyUniversalTravel) {
                                        if (userRole == "Agencia") {
                                            html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`);
                                        }
                                    }
                                    else {
                                        html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`);
                                    }
                                }
                                html.push(`<a class="dropdown-item" href="/OrderNew/TrackingOrden?id=${data}"><i class="icon-list"></i>Tracking Orden</a>`);
                                html.push(`<a class="dropdown-item print_report" data-orderid="${data}"><i class="fa fa-file-pdf-o"></i>Imprimir Envío</a>`);
                                html.push(`<a class="dropdown-item" href="/Orders/Details?id=${data}"><i class="ft-info"></i>Detalles</a>`);
                                html.push(`<a class="dropdown-item" href="#" name="btnChangeContact" data-id="${data}" data-clientId="${item.clientId}"><i class="ft-user"></i>Cambiar Contacto</a>`);
                                html.push('</div>');
                                html.push('</div>');
                                return html.join("");
                            },
                        },

                    ],
                    createdRow: function (row, data, rowIndex) {
                        InitRow(row);
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableIncompleta));
                })
                break;
            case "Revisada":
                var t = $('#tableRevisada');
                tableRevisada = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: false,
                    order: [[1, "desc"]],
                    serverSide: true,
                    processing: false,
                    language: tableLanguage,
                    ajax: $.fn.dataTable.pipeline({
                        url: `/airshipping/GetData?status=Revisada`,
                        type: 'POST',
                        pages: numberPages, // number of pages to cache,
                        data: function (dtp) {
                            return dtp;
                        },
                        dataFilter: function (data) {
                            var json = jQuery.parseJSON(data);
                            $('#base_tabRevisada').html(`Revisadas (${json.recordsTotal})`);
                            return JSON.stringify(json); // return JSON string
                        },
                        statusCode: {
                            401: function () {
                                location.href = "/Account/Login"
                            }
                        }
                    }),
                    columnDefs: [
                        {
                            targets: [0],
                            data: "id",
                            render: function (data, type, item) {
                                var html = '<label class="custom-control custom-checkbox">' +
                                    `<input type="checkbox" class="custom-control-input order-select" data-isComodin="${item.wholesalerComodin}" data-type="Paquete" value="${item.number}" />` +
                                    '<span class="custom-control-indicator"></span>' +
                                    '<span class="custom-control-description"></span>' +
                                    '</label>';
                                return html;
                            }
                        },
                        {
                            targets: [1],
                            data: "createdAt",
                            render: function (data, type, item) {
                                const init = new moment(item.createdAt);
                                const end = new moment();
                                var duration = moment.duration(end.diff(init));
                                var html = [];
                                html.push(moment(data).format(formatDate));
                                html.push(`<div><span class="tag tag-default tag-warning">${duration.days()}d ${duration.hours()}h ${duration.minutes()}m</span></div>`);
                                return html.join("");
                            }
                        },
                        {
                            targets: [2],
                            data: "number",
                            name: "numberOrAgencyTransferida",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a href="/orders/details/${item.id}">${data}</a>`);
                                if (item.userFirstName) {
                                    html.push(`<div>${item.userFirstName}</div>`)
                                }
                                if (item.noOrden) {
                                    html.push(`<p style="color:red">${item.noOrden}</p>`)
                                }

                                if (item.agencyTransferidaId != null) {
                                    if (item.agencyTransferidaId == agencyId) {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.agencyName}</div>`);
                                    }
                                    else {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. a ${item.agencyTransferredName}</div>`);
                                    }
                                }
                                if (item.express) {
                                    html.push('<div class="tag tag-danger">Express</div>');
                                }
                                if (item.problem) {
                                    html.push('<div class="tag tag-danger">Problema</div>');
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [3],
                            visible: true,
                            data: "clientFullData",
                            name: "clientFullData",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div title="${item.clientFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">`);
                                html.push(`${item.clientFullData}`);
                                html.push(`</div>`)
                                html.push(`<div>${item.clientPhoneNumber}</div>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [4],
                            visible: true,
                            data: "bags",
                            name: "bags",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    html.push(`<div>${data[i].number}</div>`)
                                }
                                if (item.contactAddressProvince) {
                                    html.push(`<div>${item.contactAddressProvince}</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [5],
                            data: "contactFullData",
                            name: "contactFullData",
                            render: function (data, type, item) {
                                return `<div title="${item.contactFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">${item.contactFullData}</div>`
                            }
                        },
                        {
                            targets: [6],
                            data: "type",
                        },
                        {
                            targets: [7],
                            data: "status",
                            render: function (data, type, item) {
                                var html = [];
                                if (item.agencyTransferidaId != null) {
                                    if (item.agencyTransferidaId != agencyId) {
                                        html.push(`<p style="color:green;margin-top:0px;margin-bottom:0px;display:inline;">${item.status}</p>`);
                                    }
                                    else {
                                        if (item.status == "Revisada") {
                                            html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="7" data-pk="7" data-url="/post" data-title="Select status"></a>`)
                                            if (item.orderReviewIsIncomplete) {
                                                html.push('<div class="tag tag-warning">Incompleta</div>')
                                            }
                                        }
                                        else if (item.status == "No Entregada") {
                                            html.push('<p style="color:darkred;margin-top:0px;margin-bottom:0px;">@item.Status</p>')
                                        }
                                    }
                                }
                                else {
                                    if (item.status == "Revisada") {
                                        html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="7" data-pk="7" data-url="/post" data-title="Select status"></a>`)
                                        if (item.orderReviewIsIncomplete) {
                                            html.push('<div class="tag tag-warning">Incompleta</div>')
                                        }
                                    }
                                    else if (item.status == "No Entregada") {
                                        html.push('<p style="color:darkred;margin-top:0px;margin-bottom:0px;">@item.Status</p>')
                                    }
                                }

                                if (item.lackSend || item.lackReview) {
                                    html.push(`<div style="display: block !important;margin-top:3px;width:30px;" class="tag tag-danger">Parcial</div>`);
                                }
                                html.push("<br/>")
                                if (item.balance > 0) {
                                    html.push(`<div class="tag tag-danger">Pendiente</div>`)
                                }

                                return html.join("");
                            }
                        },
                        {
                            targets: [8],
                            data: "shippings",
                            name: "shippings",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    const item = data[i];
                                    html.push(`<div><a href="/Shippings/Details/${item.id}">${item.number}</a></div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [9],
                            data: "id",
                            render: function (data, type, item) {
                                var html = [];
                                html.push('<div class="dropdown">');
                                html.push('<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>');
                                html.push(`<div class="dropdown-menu" aria-label="dropdown${item.number}">`);
                                html.push(`<a class="dropdown-item " href="/OrderNew/ViewBag/${data}"><i class="ft-layout"></i>Ver Bolsas</a>`);
                                if (item.balance > 0) {
                                    html.push(`<a class="dropdown-item" href="/Orders/PayOrder?orderNumber=${item.number}"><i class="fa fa-usd"></i>Gestionar pago</a>`);
                                }
                                html.push(`<a class="dropdown-item" href="/Orders/PaysOrder?id=${data}"><i class="icon-wallet"></i>Registro de pagos</a>`);
                                if (agencyId !== agencyMdl || (agencyId === agencyMdl && userRole == "Agencia")) {
                                    if (agencyId == agencyUniversalTravel) {
                                        if (userRole == "Agencia") {
                                            html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`);
                                        }
                                    }
                                    else {
                                        html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`);
                                    }
                                }
                                if (item.problem) {
                                    html.push(`<a class="dropdown-item confirm" data-orderid="${data}"><i class="fa fa-check"></i>Confirmar</a>`);
                                }
                                html.push(`<a class="dropdown-item" href="/OrderNew/TrackingOrden?id=${data}"><i class="icon-list"></i>Tracking Orden</a>`);
                                html.push(`<a class="dropdown-item print_report" data-orderid="${data}"><i class="fa fa-file-pdf-o"></i>Imprimir Envío</a>`);
                                html.push(`<a class="dropdown-item" href="/OrderNew/EditBolsa?id=${data}"><i class="fa fa-shopping-bag"></i>Editar Bolsas</a>`);
                                html.push(`<a class="dropdown-item" href="/Orders/Details?id=${data}"><i class="ft-info"></i>Detalles</a>`);
                                html.push(`<a class="dropdown-item" href="/Reclamo/Create?type=2&orderNumber=${item.number}&idClient=${item.clientId}"><i class="ft-file"></i>Crear Ticket</a>`);
                                html.push(`<a class="dropdown-item" href="#" name="btnChangeContact" data-id="${data}" data-clientId="${item.clientId}"><i class="ft-user"></i>Cambiar Contacto</a>`);
                                html.push('</div>');
                                html.push('</div>');
                                return html.join("");
                            },
                        },

                    ],
                    createdRow: function (row, data, rowIndex) {
                        InitRow(row);
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableRevisada));
                })
                break;
            case "Entregada":
                var t = $('#tableEntregada');
                tableEntregada = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: false,
                    order: [[1, "desc"]],
                    serverSide: true,
                    processing: false,
                    language: tableLanguage,
                    ajax: $.fn.dataTable.pipeline({
                        url: `/airshipping/GetData?status=Entregada`,
                        type: 'POST',
                        pages: numberPages, // number of pages to cache,
                        data: function (dtp) {
                            return dtp;
                        },
                        dataFilter: function (data) {
                            var json = jQuery.parseJSON(data);
                            $('#base_tabEntregada').html(`Entregadas (${json.recordsTotal})`);
                            return JSON.stringify(json); // return JSON string
                        },
                        statusCode: {
                            401: function () {
                                location.href = "/Account/Login"
                            }
                        }
                    }),
                    columnDefs: [
                        {
                            targets: [0],
                            data: "id",
                            render: function (data, type, item) {
                                var html = '<label class="custom-control custom-checkbox">' +
                                    `<input type="checkbox" class="custom-control-input order-select" data-isComodin="${item.wholesalerComodin}" data-type="Paquete" value="${item.number}" />` +
                                    '<span class="custom-control-indicator"></span>' +
                                    '<span class="custom-control-description"></span>' +
                                    '</label>';
                                return html;
                            }
                        },
                        {
                            targets: [1],
                            data: "deliverDate",
                            render: function (data, type, item) {
                                const init = new moment(data);
                                const end = new moment();
                                var duration = moment.duration(end.diff(init));
                                var html = [];
                                html.push(moment(data).format(formatDate));
                                html.push(`<div><span class="tag tag-default tag-warning">${duration.days()}d ${duration.hours()}h ${duration.minutes()}m</span></div>`);
                                return html.join("");
                            }
                        },
                        {
                            targets: [2],
                            data: "number",
                            name: "numberOrAgencyTransferida",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a href="/orders/details/${item.id}">${data}</a>`);
                                if (item.userFirstName) {
                                    html.push(`<div>${item.userFirstName}</div>`)
                                }
                                if (item.noOrden) {
                                    html.push(`<p style="color:red">${item.noOrden}</p>`)
                                }

                                if (item.agencyTransferidaId != null) {
                                    if (item.agencyTransferidaId == agencyId) {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.agencyName}</div>`);
                                    }
                                    else {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. a ${item.agencyTransferredName}</div>`);
                                    }
                                }
                                if (item.express) {
                                    html.push('<div class="tag tag-danger">Express</div>');
                                }
                                if (item.problem) {
                                    html.push('<div class="tag tag-danger">Problema</div>');
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [3],
                            visible: true,
                            data: "clientFullData",
                            name: "clientFullData",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div title="${item.clientFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">`);
                                html.push(`${item.clientFullData}`);
                                html.push(`</div>`)
                                html.push(`<div>${item.clientPhoneNumber}</div>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [4],
                            visible: true,
                            data: "bags",
                            name: "bags",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    html.push(`<div>${data[i].number}</div>`)
                                }
                                if (item.contactAddressProvince) {
                                    html.push(`<div>${item.contactAddressProvince}</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [5],
                            data: "contactFullData",
                            name: "contactFullData",
                            render: function (data, type, item) {
                                return `<div title="${item.contactFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">${item.contactFullData}</div>`
                            }
                        },
                        {
                            targets: [6],
                            data: "type",
                        },
                        {
                            targets: [7],
                            data: "status",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(item.status);

                                if (item.lackSend || item.lackReview) {
                                    html.push(`<div style="display: block !important;margin-top:3px;width:30px;" class="tag tag-danger">Parcial</div>`);
                                }
                                html.push("<br/>")
                                if (item.balance > 0) {
                                    html.push(`<div class="tag tag-danger">Pendiente</div>`)
                                }

                                return html.join("");
                            }
                        },
                        {
                            targets: [8],
                            data: "shippings",
                            name: "shippings",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    const item = data[i];
                                    html.push(`<div><a href="/Shippings/Details/${item.id}">${item.number}</a></div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [9],
                            data: "id",
                            render: function (data, type, item) {
                                var html = [];
                                html.push('<div class="dropdown">');
                                html.push('<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>');
                                html.push(`<div class="dropdown-menu" aria-label="dropdown${item.number}">`);
                                html.push(`<a class="dropdown-item " href="/OrderNew/ViewBag/${data}"><i class="ft-layout"></i>Ver Bolsas</a>`);
                                if (item.balance > 0) {
                                    html.push(`<a class="dropdown-item" href="/Orders/PayOrder?orderNumber=${item.number}"><i class="fa fa-usd"></i>Gestionar pago</a>`);
                                }
                                html.push(`<a class="dropdown-item" href="/Orders/PaysOrder?id=${data}"><i class="icon-wallet"></i>Registro de pagos</a>`);
                                if (agencyId !== agencyMdl || (agencyId === agencyMdl && userRole == "Agencia")) {
                                    if (agencyId == agencyUniversalTravel) {
                                        if (userRole == "Agencia") {
                                            html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`);
                                        }
                                    }
                                    else {
                                        html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`);
                                    }
                                }
                                if (item.problem) {
                                    html.push(`<a class="dropdown-item confirm" data-orderid="${data}"><i class="fa fa-check"></i>Confirmar</a>`);
                                }
                                html.push(`<a class="dropdown-item" href="/OrderNew/TrackingOrden?id=${data}"><i class="icon-list"></i>Tracking Orden</a>`);
                                html.push(`<a class="dropdown-item print_report" data-orderid="${data}"><i class="fa fa-file-pdf-o"></i>Imprimir Envío</a>`);
                                html.push(`<a class="dropdown-item" href="/OrderNew/EditBolsa?id=${data}"><i class="fa fa-shopping-bag"></i>Editar Bolsas</a>`);
                                html.push(`<a class="dropdown-item" href="/Orders/Details?id=${data}"><i class="ft-info"></i>Detalles</a>`);
                                html.push(`<a class="dropdown-item" href="/Reclamo/Create?type=2&orderNumber=${item.number}&idClient=${item.clientId}"><i class="ft-file"></i>Crear Ticket</a>`);
                                html.push('</div>');
                                html.push('</div>');
                                return html.join("");
                            },
                        },

                    ],
                    createdRow: function (row, data, rowIndex) {
                        InitRow(row);
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableEntregada));
                })
                break;
            case "Cancelada":
                var t = $('#tableCancelada');
                tableCancelada = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: false,
                    order: [[1, "desc"]],
                    serverSide: true,
                    processing: false,
                    language: tableLanguage,
                    ajax: {
                        "url": `/airshipping/GetData?status=Cancelada`,
                        "type": 'POST',
                        "dataType": "json",
                        "statusCode": {
                            401: function () {
                                location.href = "/Account/Login"
                            }
                        }
                    },
                    columnDefs: [
                        {
                            targets: [0],
                            data: "createdAt",
                            render: function (data, type, item) {
                                const init = new moment(data);
                                const end = new moment();
                                var duration = moment.duration(end.diff(init));
                                var html = [];
                                html.push(moment(data).format(formatDate));
                                html.push(`<div><span class="tag tag-default tag-warning">${duration.days()}d ${duration.hours()}h ${duration.minutes()}m</span></div>`);
                                return html.join("");
                            }
                        },
                        {
                            targets: [1],
                            data: "number",
                            name: "numberOrAgencyTransferida",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a href="/orders/details/${item.id}">${data}</a>`);
                                if (item.noOrden) {
                                    html.push(`<p style="color:red">${item.noOrden}</p>`)
                                }

                                if (item.agencyTransferidaId != null) {
                                    if (item.agencyTransferidaId == agencyId) {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.agencyName}</div>`);
                                    }
                                    else {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. a ${item.agencyTransferredName}</div>`);
                                    }
                                }
                                if (item.express) {
                                    html.push('<div class="tag tag-danger">Express</div>');
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [2],
                            visible: true,
                            data: "clientFullData",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div title="${item.clientFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">`);
                                html.push(`${item.clientFullData}`);
                                html.push(`</div>`)
                                html.push(`<div>${item.clientPhoneNumber}</div>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [3],
                            data: "contactFullData",
                            render: function (data, type, item) {
                                return `<div title="${item.contactFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">${item.contactFullData}</div>`
                            }
                        },
                        {
                            targets: [4],
                            data: "type",
                        },
                        {
                            targets: [5],
                            data: "amount",
                            render: function (data, type, item) {
                                var html = [];
                                if (item.agencyTransferidaId != null && item.agencyTransferidaId == agencyId) {
                                    html.push(`<text>${parseFloat(item.wholesalerCost).toFixed(2)}</text >`)
                                }
                                else {
                                    html.push(`<text>${parseFloat(item.amount).toFixed(2)}</text >`)
                                }

                                return html.join("");
                            },
                        },
                        {
                            targets: [6],
                            data: "id",
                            render: function (data, type, item) {
                                var html = [];
                                html.push('<div class="dropdown">');
                                html.push('<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>');
                                html.push(`<div class="dropdown-menu" aria-label="dropdown${item.number}">`);
                                html.push(`<a class="dropdown-item " href="/OrderNew/ViewBag/${data}"><i class="ft-layout"></i>Ver Bolsas</a>`);
                                html.push(`<a class="dropdown-item" href="/OrderNew/TrackingOrden?id=${data}"><i class="icon-list"></i>Tracking Orden</a>`);
                                html.push(`<a class="dropdown-item print_report" data-orderid="${data}"><i class="fa fa-file-pdf-o"></i>Imprimir Envío</a>`);
                                html.push(`<a class="dropdown-item" href="/Orders/Details?id=${data}"><i class="ft-info"></i>Detalles</a>`);
                                html.push('</div>');
                                html.push('</div>');
                                return html.join("");
                            },
                        },

                    ],
                    createdRow: function (row, data, rowIndex) {
                    },
                    initComplete: function (settings, json) {

                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableCancelada));
                })
                break;
            default:
                break;
        }

        if (!tableName.includes("Cancelada")) {
            $(baseTabId).block(blockOptions);
            $(baseTabId).parent().addClass("disabled");
        }
        if (!tableName.includes("Cancelada")) {
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
}

function InitRow(row) {
    $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); });
    $.each($('.status', row), (i, e) => {
        $(e).editable({
            source: function (p) {
                var s = [];
                if (agencyId == agencyRapidM || agencyId == agencyEnvieConFe || agencyId == agency555) {
                    s = [
                        {
                            "1": 'Iniciada',
                            "3": 'Cancelada',
                            "4": 'Completada'
                        },
                        {
                            "1": 'Iniciada',
                            "4": 'Completada'
                        },
                        {
                            "3": 'Cancelada',
                        },
                        {//Estado Completada
                            "4": 'Completada',
                            "5": "Enviada"
                        },
                        {//Estado Enviada
                            "5": "Enviada",
                            "8": "Entregada"
                        },
                        {//Estado Despachada
                            "6": "Despachada",
                            "9": "Recibida",
                            "8": "Entregada"
                        },
                        {//Estado Revisada
                            "7": 'Revisada',
                            "8": "Entregada"
                        },
                        {//Estado Recibida
                            "9": 'Recibida',
                            "8": "Entregada",
                        },
                    ];
                }
                else if (agencyId == agencyBelloCaribe) {
                    s = [
                        {
                            "1": 'Iniciada',
                            "3": 'Cancelada',
                            "4": 'Completada'
                        },
                        {
                            "1": 'Iniciada',
                            "4": 'Completada'
                        },
                        {
                            "3": 'Cancelada',
                        },
                        {//Estado Completada
                            "1": 'Iniciada',
                            "4": 'Completada',
                            "5": "Enviada"
                        },
                        {//Estado Enviada
                            "1": "Iniciada",
                            "5": "Enviada",
                            "7": "Revisada"
                        },
                        {//Estado Despachada
                            "6": "Despachada",
                            "8": "Entregada"
                        },
                        {//Estado Revisada
                            "7": 'Revisada',
                            "8": "Entregada"
                        },
                    ];
                }
                else {
                    s = [
                        {
                            "1": 'Iniciada',
                            "3": 'Cancelada',
                            "4": 'Completada'
                        },
                        {
                            "1": 'Iniciada',
                            "4": 'Completada'
                        },
                        {
                            "3": 'Cancelada',
                        },
                        {//Estado Completada
                            "4": 'Completada',
                            "5": "Enviada"
                        },
                        {//Estado Enviada
                            "5": "Enviada",
                            "7": "Revisada"
                        },
                        {//Estado Despachada
                            "6": "Despachada",
                            "8": "Entregada"
                        },
                        {//Estado Revisada
                            "7": 'Revisada',
                            "8": "Entregada"
                        },
                    ];
                }


                return s[$(this).data('pk') - 1];

            },
            validate: function (x) {

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
                url: '/OrderNew/index',
                type: 'post',
                dataType: 'json'
            },
            params: function (params) {
                params.id = $(this).data("id");
                return params;
            },
            success: function (response, newValue) {
                if (response.success)
                    location.reload();
                else
                    toastr.error(response.msg);

                $.unblockUI();
                //showOKMessage("Cambio de estatus", "Se ha cambiado satifactoriamente el estatus");
            },
            //showbuttons: false
        });
    })

    $.each($('.print_report', row), (i, e) => {
        $(e).click(function () {
            var orderid = $(this).data("orderid")
            $.ajax({
                async: true,
                type: "POST",
                contentType: "application/x-www-form-urlencoded",
                url: "/Orders/createOrderComprobante",
                data: {
                    id: orderid
                },
                beforeSend: function () {
                    $.blockUI();
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
                    $.unblockUI();
                },
                error: function () {
                    toastr.error("No se ha podido exportar", "Error");
                    $.unblockUI();
                },
                timeout: 4000,
            });
        });
    });

    $.each($('.confirm', row), (i, e) => {
        $(e).click(function () {
            var orderid = $(this).data("orderid")
            $.ajax({
                async: true,
                type: "GET",
                contentType: "application/x-www-form-urlencoded",
                url: "/ordernew/confirmorder?orderid=" + orderid,
                beforeSend: function () {
                    $.blockUI();
                },
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        rechargeTables();
                    }
                    else
                        toastr.error(response.message);

                    $.unblockUI();
                },
                error: function () {
                    toastr.error("Ha ocurrido un error", "Error");
                    $.unblockUI();
                },
                timeout: 4000,
            });
        });
    });

    $.each($('[name="btnChangeContact"]', row), (i, e) => {
        $(e).click(function () {
            modalChangeContact(e);
        });
    });
}

function LoadEvents() {

    $('#searchNumber').on('keypress', function (e) {
        if (e.key == "Enter")
            search($(this).val(), "Number");
    });
    $('#btnSearchNumber').on('click', function (e) {
        search($('#searchNumber').val(), "Number");
    });

    $('#searchClient').on('keypress', function (e) {
        if (e.key == "Enter")
            search($(this).val(), "ClientFullData");
    });

    $('#btnSearchClient').on('click', function (e) {
        search($('#searchClient').val(), "ClientFullData");
    });

    $('#searchContact').on('keypress', function (e) {
        if (e.key == "Enter")
            search($(this).val(), "ContactFullData");
    });
    $('#btnSearchContact').on('click', function (e) {
        search($('#searchContact').val(), "ContactFullData");
    });

    $('#searchBag').on('keypress', function (e) {
        if (e.key == "Enter")
            search($(this).val(), "Bags");
    });
    $('#btnSearchBag').on('click', function (e) {
        search($('#searchBag').val(), "Bags");
    });

    $('#searchShipping').on('keypress', function (e) {
        if (e.key == "Enter")
            search($(this).val(), "Shippings");
    });
    $('#btnSearchShipping').on('click', function (e) {
        search($('#searchShipping').val(), "Shippings");
    });

    $('#searchProduct').on('keypress', function (e) {
        if (e.key == "Enter")
            search($(this).val(), "Product");
    });
    $('#btnSearchProduct').on('click', function (e) {
        search($('#searchProduct').val(), "Product");
    });

    $('#clearFilters').on('click', function () {
        $('#searchNumber').val("");
        $('#searchClient').val("");
        $('#searchContact').val("");
        $('#searchBag').val("");
        $('#searchShipping').val("");
        $('#searchProduct').val("");
        ClearFiltersTables();
    })

    $(document).on("change", ".order-select", function () {
        if ($(this)[0].checked) {
            cantSelect++;
            //selectedIds.push($(this).val());
        } else {
            cantSelect--;
            //selectedIds.splice($.inArray($(this).val(), selectedIds), 1);
        }

        showOptions();
    });

    function showOptions() {
        var tabactive = $('[name = "tab"][class = "nav-link tablePartial active"]');
        if (/*selectedIds.length*/ cantSelect == 0) {
            $("#gen_report").addClass("hidden");
            $("#btn_transferir").addClass("hidden");
            $("#entregar-orders").addClass("hidden");
            $("#sendStatusBtn").hide();
            //Si el tab es completadas
            if (tabactive.attr('data-type') == "enviadas") {
                $('#despachar').hide();
            }
        } else {
            $("#gen_report").removeClass("hidden");
            $("#btn_transferir").removeClass("hidden");
            $("#entregar-orders").removeClass("hidden");
            $("#sendStatusBtn").hide();
            //Si el tab es completadas
            if (tabactive.attr('data-type') == "enviadas") {
                $('#despachar').show();
            }
            else if (tabactive.attr('data-type') == "iniciadas/pendientes") {
                $('#sendStatusBtn').show();
            }
        }
    }

    $('#sendStatusBtn').on('click', function () {
        selectedIds = new Array;
        let isValid = true;
        $(document).find(".order-select").each(function (i, e) {
            if (e.checked) {
                const number = $(e).data('id');
                selectedIds.push(number);
            }
        });

        if (selectedIds.length == 0) {
            toastr.warning("No se han seleccionado trámites para pasar a estado enviado");
            return false;
        }

        //ajax transferencia

        swal({
            title: "Enviar Paquetes",
            text: "¿Está seguro que desea pasar a estado Enviado los trámites seleccionados?",
            type: "info",
            showCancelButton: true,
            closeOnConfirm: false,
            showLoaderOnConfirm: true,
        }, function () {
            var data = {
                ids: selectedIds
            };
            $.ajax({
                async: true,
                type: "POST",
                contentType: 'application/x-www-form-urlencoded',
                url: "/OrderNew/SendStatusOrders",
                data: data,
                success: function (response) {
                    location.reload();
                },
                error: function () {
                    toastr.error("No se han podido cambiar de estado los tramites", "Error");
                },
            });

        });
    })


    $('#despachar').on('click', function () {
        $('#modalDespachar').modal('show');
    });

    // Para mostrar el boton de despacho al seleccionarse las iniciadas
    $('[name="tab"]').on('click', function () {
        if (cantSelect == 0) {
            $('#despachar').hide();
        }
        else {
            if ($(this).attr('data-type') == "enviadas") {
                $('#despachar').show();
            }
            else {
                $('#despachar').hide();
            }
        }

    });

    $("#creardespacho").click(function () {
        selectedIds = new Array;
        $(".order-select").each(function (i, e) {
            if (e.checked) {
                selectedIds.push($(e).val());
            }
        });

        var distributorId = $('#ditribuidorId').val();
        if (!distributorId) {
            toastr.warning("Debe seleccionar un distribuidor")
            return false;
        }
        if (selectedIds.length == 0) {
            toastr.warning("Debe seleccionar al menos un trámite para despachar");
            return false;
        }
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/ordernew/despachoADistribuidor",
            data: {
                ids: selectedIds,
                distributorId: distributorId,
            },
            beforeSend: function () {
                $.blockUI({
                    message: '<div class="ft-refresh-cw icon-spin font-medium-2"></div>',
                    timeout: 60000,
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
                if (response.success) {
                    location.href = "/AirShipping/Index?msg=" + response.msg;
                }
                else {
                    toastr.warning(response.msg);
                }
                $.unblockUI()
            },
            error: function () {
                toastr.error("No se ha podido despachar", "Error");
                $.unblockUI()
            },
            timeout: 20000,
        });
    });

    $('#btn_transferir').on('click', function () {
        $('#ModalTransferir').modal("show");
    })

    $('#transferir').on('click', function () {
        selectedIds = new Array;
        let isValid = true;
        $(document).find(".order-select").each(function (i, e) {
            if (e.checked) {
                const isComodin = $(e).attr("data-isComodin");
                const type = $(e).attr("data-type");
                const number = $(e).val();
                if (isComodin != "True") {
                    toastr.warning("El trámite " + number + " no pertenece a un mayorista comodín");
                    isValid = false;
                }
                if (type != "Paquete") {
                    toastr.warning("El trámite " + number + " no es de tipo Paquete");
                    isValid = false;
                }
                selectedIds.push(number);
            }
        });

        if (!isValid)
            return false;

        if (selectedIds.length == 0) {
            toastr.warning("No se han seleccionado trámites para transferir");
            return false;
        }

        //ajax transferencia

        swal({
            title: "Transferir Paquete",
            text: "¿Está seguro que desea transferir los paquetes?",
            type: "info",
            showCancelButton: true,
            closeOnConfirm: false,
            showLoaderOnConfirm: true,
        }, function () {
            var wholesalerId = $('#selectWholesaler').val();
            var data = {
                wholesalerId: wholesalerId,
                ordersNumber: selectedIds
            };
            $.ajax({
                async: true,
                type: "POST",
                contentType: 'application/x-www-form-urlencoded',
                url: "/OrderNew/TransferirOrden",
                data: data,
                beforeSend: function () {
                    $.blockUI();
                },
                success: function (response) {
                    if (response.success) {
                        location.reload();
                    }
                    else {
                        toastr.warning(response.msg);
                    }

                    $.unblockUI();
                },
                error: function () {
                    toastr.error("No se han podido transferir los paquetes", "Error");
                    $.unblockUI();
                },
            });

        });
    });

    $('#entregar-orders').on('click', function () {
        selectedIds = new Array;
        let isValid = true;
        $(document).find(".order-select").each(function (i, e) {
            if (e.checked) {
                //const type = $(e).attr("data-type");
                const number = $(e).val();
                /*if (type != "Paquete") {
                    toastr.warning("El trámite " + number + " no es de tipo Paquete");
                    isValid = false;
                }*/
                selectedIds.push(number);
            }
        });

        if (!isValid)
            return false;

        if (selectedIds.length == 0) {
            toastr.warning("No se han seleccionado trámites para entregar");
            return false;
        }

        //ajax transferencia

        swal({
            title: "Entregar Paquete",
            text: "¿Está seguro que desea pasar a estado Entregado los trámites seleccionados?",
            type: "info",
            showCancelButton: true,
            closeOnConfirm: false,
            showLoaderOnConfirm: true,
        }, function () {
            var data = {
                ids: selectedIds
            };
            $.ajax({
                async: true,
                type: "POST",
                contentType: 'application/x-www-form-urlencoded',
                url: "/OrderNew/EntregarPaquetes",
                data: data,
                success: function (response) {
                    location.reload();
                },
                error: function () {
                    toastr.error("No se han podido cambiar de estado los tramites", "Error");
                },
            });

        });
    });

    $('#notification').click(function () {
        var isActive = $(this).attr('isActive');
        if (isActive == "true") {
            $(this).attr('isActive', "false");
            isActive = false;
        }
        else {
            $(this).attr('isActive', "true");
            isActive = true;
        }
        $.ajax({
            type: "POST",
            async: true,
            url: "/OrderNew/ChangeNotification",
            data: {
                isActive: isActive
            },
            beforeSend: function () {

            },
            success: function (response) {
                if (response.success) {
                    if (isActive) {
                        $('#notification').removeClass('fa-bell-slash-o')
                        $('#notification').addClass('fa-bell-o')
                    }
                    else {
                        $('#notification').removeClass('fa-bell-o')
                        $('#notification').addClass('fa-bell-slash-o')
                    }
                }
                else {
                    toastr.error(response.msg);
                }
            },
            error: function (response) {
                toastr.error("No se ha podido cambiar el estado de las notificaciones");
            }
        })
    })

    $('#exportExcelAccept').on('click', function () {
        const dateRange = $('#daterange').val();
        const dateType = $('#select_type_date').val();
        const status = $('#select-status-mult').val()
        const provinces = $('#select-province-mult').val()
        const type = $('#select-type-mult').val()
        const retails = $('#select-retails-mult').val()

        const url = `/OrderNew/ExportExcel?date=${dateRange}&status=${status}&province=${provinces}&type=${type}&retails=${retails}&dateType=${dateType}`;
        window.open(url, '_blank');
        $('#modalExport').modal('hide');
    })

    $('#exportExcel').on('click', function () {
        $('#modalExport').modal('show');
    });

    $("#exportReporteAccept").click(function () {
        var date = $('#daterangeReport').val();
        var type = $("#ul_tab").find('a.active').data('type');

        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/OrderNew/ExportReporte",
            data: {
                date: date,
                type: type

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
            timeout: 60000,
        });
    });

    $("#exportReporteBolsasAccept").click(function () {
        var date = $('#daterangeBolsas').val();
        var type = $("#ul_tab").find('a.active').data('type');

        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/OrderNew/ExportReporteBolsas",
            data: {
                date: date,
                type: type

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
            timeout: 20000,
        });
    });

    $("#exportReportePoundsAccept").click(function () {
        var date = $('#daterangePunds').val();
        var type = $("#ul_tab").find('a.active').data('type');

        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/OrderNew/ExportReportByPunds",
            beforeSend: function () {
                $.blockUI();
            },
            data: {
                date: date,
                type: type

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

                $.unblockUI()
            },
            error: function () {
                $.unblockUI()
                toastr.error("No se ha podido exportar", "Error");
            },
            timeout: 20000,
        });
    });


    $("#exportReportehmAccept").click(function () {
        var date = $('#daterangehm').val();
        var status = $("#filtro-estados").val();
        var provinces = $("#filtro-provincias").val();

        let url = `/OrderNew/ExportReporteHmPaquetes?date=${date}&status=${status}`;
        if (provinces) {
            for (var i = 0; i < provinces.length; i++) {
                url += `&provinces[${i}]=${provinces[i]}`
            }
        }

        $.ajax({
            async: true,
            type: "GET",
            contentType: "application/x-www-form-urlencoded",
            url: url,
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
            timeout: 20000,
        });
    });

    $('#gen_reportFecha').on('click', function () {
        $('#modalReporte').modal('show');
    });

    $('#gen_reportBolsas').on('click', function () {
        $('#modalReporteBolsas').modal('show');
    });

    $('#gen_reportPounds').on('click', function () {
        $('#modalReportePunds').modal('show');
    });

    $('#gen_reporthm').on('click', function () {
        $('#modalReportehm').modal('show');
    });

    $('#exportReporteAccept').on('click', function () {
        $('#modalExport').modal('hide');
    });

    $("#gen_report").click(function () {

        selectedIds = new Array;

        $(document).find(".order-select").each(function (i, e) {
            if (e.checked) {
                selectedIds.push($(e).val());
            }
        });
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/Orders/Report",
            data: {
                ids: selectedIds
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

    $('[name="checkalltramites"]').on("change", function () {
        var paneActive = $('[class="tab-pane active"]');
        var table = paneActive.find("table");
        var type = $('[class="nav-link tablePartial active"]').attr("data-type");

        if ($(this)[0].checked) {
            table.find(".order-select").prop("checked", true);
            cantSelect = table.find(".order-select").length;
        } else {
            table.find(".order-select").prop("checked", false);
            cantSelect = 0;
        }

        showOptions();
    });
}

function onlyUnique(value, index, self) {
    return self.indexOf(value) === index;
}

function modalChangeContact(e) {
    const clientId = $(e).data('clientid');
    const orderId = $(e).data('id');

    $('#ref_client_id').val(clientId);
    $('#ref_order_id').val(orderId);

    // agregar contactos a select
    $.ajax({
        async: true,
        type: "GET",
        contentType: "application/x-www-form-urlencoded",
        url: "/Contacts/GetContactsOfAClient",
        data: {
            id: clientId
        },
         beforeSend: function () {
            $.blockUI();
        },
        success: function (response) {
            $.unblockUI()
            const contacts = response;
            const selectContact = $('#selectContacto');
            selectContact.empty();
            selectContact.append(`<option value="">Seleccione un contacto</option>`);
            $.each(contacts, function (i, contact) {
                selectContact.append(`<option value="${contact.contactId}">${contact.phone1}-${contact.name} ${contact.lastName}</option>`);
            });
            $('#modalContacto').modal('show');

        },
        error: function () {
            $.unblockUI();
            toastr.error("No se han podido cargar los contactos", "Error");
        }
    });
}


$('#seleccionarContacto').on('click', changeContact)
function changeContact() {
    const contactId = $('#selectContacto').val();
    const orderId = $('#ref_order_id').val();

if (!contactId) {
        toastr.warning("Debe seleccionar un contacto");
        return false;
    }

$.ajax({
        async: true,
        type: "POST",
        contentType: "application/x-www-form-urlencoded",
        url: "/OrderNew/UpdateContact",
        data: {
            contactId: contactId,
            orderId: orderId
        },
        beforeSend: function () {
            $.blockUI();
        },
        success: function (response) {
            $.unblockUI()
            if (response.success) {
                toastr.success(response.msg);
                $('#modalContacto').modal('hide');
                location.reload();
            } else {
                toastr.error(response.msg);
            }
        },
        error: function () {
            $.unblockUI();
            toastr.error("No se ha podido cambiar el contacto", "Error");
        }
    });
}
