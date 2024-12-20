var tab = 1;
const numberPages = 3;
var nameTable = "tableIniciada";
var cantSelect = 0;
const formatDate = "MM/DD/YY";
const allTables = ["Iniciada", "Despachada", "Entregada", "Cancelada", "All"];
if (role == "EmpleadoCuba") {
    allTables = ["Despachada", "Entregada", "Cancelada"];
}
var currentTab = "base_tabIniciada";

var tableIniciada;
var tableDespachada;
var tableEntregada;
var tableCancelada;
var tableAll;

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
}, 600);

var search = debounce(function (value, type) {
    switch (type) {
        case "Number":
            tableIniciada.column("number:name").search(value).draw();
            tableDespachada.column("number:name").search(value).draw();
            tableEntregada.column("number:name").search(value).draw();
            tableAll.column("number:name").search(value).draw();
            break;
        case "ClientName":
            tableIniciada.column("clientFullData:name").search(value).draw();
            tableDespachada.column("clientFullData:name").search(value).draw();
            tableEntregada.column("clientFullData:name").search(value).draw();
            tableAll.column("clientFullData:name").search(value).draw();
            break;
        case "ClientPhone":
            tableIniciada.column("clientPhoneNumber:name").search(value).draw();
            tableDespachada.column("clientPhoneNumber:name").search(value).draw();
            tableEntregada.column("clientPhoneNumber:name").search(value).draw();
            tableAll.column("clientPhoneNumber:name").search(value).draw();
            break;
        case "ContactName":
            tableIniciada.column("contactFullData:name").search(value).draw();
            tableDespachada.column("contactFullData:name").search(value).draw();
            tableEntregada.column("contactFullData:name").search(value).draw();
            tableAll.column("contactFullData:name").search(value).draw();
            break;
        case "Product":
            tableIniciada.column("products:name").search(value).draw();
            tableDespachada.column("products:name").search(value).draw();
            tableEntregada.column("products:name").search(value).draw();
            tableAll.column("products:name").search(value).draw();
        case "SearchPending":
            tableIniciada.column("searchPending:name").search(value).draw();
            tableDespachada.column("searchPending:name").search(value).draw();
            tableEntregada.column("searchPending:name").search(value).draw();
            tableAll.column("searchPending:name").search(value).draw();
            break;
        default:
            break;
    }
}, 350);

var ClearFiltersTables = debounce(function () {
    tableIniciada.columns().search("").draw();
    tableDespachada.columns().search("").draw();
    tableEntregada.columns().search("").draw();
    tableCancelada.columns().search("").draw();
}, 350);

const columnsDespachada = [
    {
        targets: [0],
        orderable: false,
        data: "id",
        render: function (data, type, item) {
            var html = '<label class="custom-control custom-checkbox">' +
                `<input type="checkbox" class="custom-control-input order-select" value="${item.number}" />` +
                '<span class="custom-control-indicator"></span>' +
                '<span class="custom-control-description"></span>' +
                '</label>';
            return html;
        }
    },
    {
        targets: [1],
        data: "date",
        render: function (data, type, item) {
            return moment(data, formatDate).format(formatDate);
        }
    },
    {
        targets: [2],
        data: "number",
        name: "number",
        render: function (data, type, item) {
            var html = [];
            html.push(`<a href="/orders/details/${item.id}">${data}</a>`);

            if (item.invoiceId != null && item.isCreatedMovileApp) {
                html.push(`<div class="tag tag-success">Inv. ${item.invoiceNumber}</div>`)
            }

            if (item.noOrden) {
                html.push(`<div class="tag tag-success">${item.noOrden}</div>`)
            }

            if (item.agencyTransferidaId != null) {
                if (item.agencyTransferidaId == agencyId) {
                    html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.agencyName}</div>`);
                }
                else {
                    html.push(`<div style="display: block !important;" class="tag tag-info">T. a ${item.agencyTransferidaName}</div>`);
                }
            }
            else if (item.minoristaId != null) {
                html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.retailerName}</div>`)
            }

            if (item.express) {
                html.push(`<div class="tag tag-danger">Express</div>`)
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

            if (item.invoiceId != null && item.isCreatedMovileApp) {
                html.push(`<div class="tag tag-success">App</div>`)
            }
            return html.join("");
        }
    },
    {
        targets: [4],
        visible: true,
        data: "clientPhoneNumber",
        name: "clientPhoneNumber"
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
        data: "contactAddressProvince",
    },
    {
        targets: [7],
        data: "products",
        orderable: false,
        name: "products",
        render: function (data, type, item) {
            var html = [];
            if (data.length > 0) {
                var firstProduct = data[0];
                var products = [];
                var qty = 0;
                for (var i = 0; i < data.length; i++) {
                    const item = data[i];
                    qty += item.quantity;
                    products.push(item.name)
                }
                html.push(`<p style="padding:0px;margin:0px;">${firstProduct.wholesalerName ?? ""} (<b>${qty}</b>)</p>`);
                html.push(`<p style="padding:0px;margin:0px;">- ${products.join(" | ")}</p`);
            }

            return html.join("");
        }
    },
    {
        targets: [8],
        data: "status",
        orderable: false,
        name: "status",
        render: function (data, type, item) {
            var html = [];
            if (item.agencyTransferidaId != null) {
                if (item.agencyTransferidaId != agencyId) {
                    html.push(`<p style="color:blue;margin-top:0px;margin-bottom:0px;display:inline;">${data}</p>`);
                }
                else {
                    if(data == "Despachada")
                        html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="4" data-pk="4" data-url="/post" data-title="Select status"></a>`)
                    else
                        html.push(`<p style="color:blue;margin-top:0px;margin-bottom:0px;display:inline;">${data}</p>`);
                }
            }
            else {
                if (data == "Despachada")
                    html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="4" data-pk="4" data-url="/post" data-title="Select status"></a>`)
                else
                    html.push(`<p style="color:blue;margin-top:0px;margin-bottom:0px;display:inline;">${data}</p>`);
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
        data: "dispatchDate",
        render: function (data, type, item) {
            if (data)
                return moment(data, formatDate).format(formatDate);
            return "";
        }
    },
    {
        targets: [10],
        data: "id",
        render: function (data, type, item) {
            var html = [];
            html.push('<div class="dropdown">');
            html.push('<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>');
            html.push(`<div class="dropdown-menu" aria-label="dropdown${item.number}">`);

            if (item.balance > 0) {
                html.push(`<a class="dropdown-item" href="/Orders/PayOrder?orderNumber=${item.number}"><i class="fa fa-usd"></i>Gestionar pago</a>`)
            }
            html.push(`<a class="dropdown-item" href="/Orders/PaysOrder?id=${item.id}"><i class="icon-wallet"></i>Registro de pagos</a>`)
            html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${item.id}"><i class="fa fa-usd"></i>Editar Pagos</a>`)
            html.push(`<a class="dropdown-item" href="/OrderNew/TrackingOrden?id=${item.id}"><i class="icon-list"></i>Tracking Orden</a>`)
            html.push(`<a class="dropdown-item" href="/OrderNew/EditCombo?id=${item.id}"><i class="ft-info"></i>Editar</a> `)
            html.push(`<a class="dropdown-item print_report" data-orderid="${item.id}"><i class="fa fa-file-pdf-o"></i>Imprimir Envío</a>`)
            html.push(`<a class="dropdown-item" href="/Orders/Details?id=${item.id}"><i class="ft-info"></i>Detalles</a>`)
            html.push(`<a class="dropdown-item" href="/Reclamo/Create?type=1&orderNumber=${item.number}&idClient=${item.clientId}"><i class="ft-file"></i>Crear Ticket</a>`)
            html.push(`<a class="dropdown-item" href="#" name="cancel" data-type="combo" data-id="${item.id}"><i class="ft-x"></i>Cancelar</a>`)

            html.push('</div>');
            html.push('</div>');
            return html.join("");
        },
    },
    {
        targets: [11],
        data: "searchPending",
        name: "searchPending",
        visible: false,
        render: function (data, type, item) {
            return "";
        }
    },
];

const columnsDespachadaEmpleadoCuba = [
    {
        targets: [0],
        orderable: false,
        data: "id",
        render: function (data, type, item) {
            var html = '<label class="custom-control custom-checkbox">' +
                `<input type="checkbox" class="custom-control-input order-select" value="${item.number}" />` +
                '<span class="custom-control-indicator"></span>' +
                '<span class="custom-control-description"></span>' +
                '</label>';
            return html;
        }
    },
    {
        targets: [1],
        data: "date",
        render: function (data, type, item) {
            return moment(data, formatDate).format(formatDate);
        }
    },
    {
        targets: [2],
        data: "number",
        name: "number",
        render: function (data, type, item) {
            var html = [];
            html.push(`<a href="/orders/details/${item.id}">${data}</a>`);

            if (item.invoiceId != null && item.isCreatedMovileApp) {
                html.push(`<div class="tag tag-success">Inv. ${item.invoiceNumber}</div>`)
            }

            if (item.noOrden) {
                html.push(`<div class="tag tag-success">${item.noOrden}</div>`)
            }

            if (item.agencyTransferidaId != null) {
                if (item.agencyTransferidaId == agencyId) {
                    html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.agencyName}</div>`);
                }
                else {
                    html.push(`<div style="display: block !important;" class="tag tag-info">T. a ${item.agencyTransferidaName}</div>`);
                }
            }
            else if (item.minoristaId != null) {
                html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.retailerName}</div>`)
            }

            if (item.express) {
                html.push(`<div class="tag tag-danger">Express</div>`)
            }

            return html.join("");
        },
    },
    {
        targets: [3],
        data: "contactFullData",
        name: "contactFullData",
        render: function (data, type, item) {
            return `<div title="${item.contactFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">${item.contactFullData}</div>`
        }
    },
    {
        targets: [4],
        data: "contactAddressProvince",
    },
    {
        targets: [5],
        data: "status",
        orderable: false,
        name: "status",
        render: function (data, type, item) {
            var html = [];
            if (item.agencyTransferidaId != null) {
                if (item.agencyTransferidaId != agencyId) {
                    html.push(`<p style="color:blue;margin-top:0px;margin-bottom:0px;display:inline;">${data}</p>`);
                }
                else {
                    if (data == "Despachada")
                        html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="4" data-pk="4" data-url="/post" data-title="Select status"></a>`)
                    else
                        html.push(`<p style="color:blue;margin-top:0px;margin-bottom:0px;display:inline;">${data}</p>`);
                }
            }
            else {
                if (data == "Despachada")
                    html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="4" data-pk="4" data-url="/post" data-title="Select status"></a>`)
                else
                    html.push(`<p style="color:blue;margin-top:0px;margin-bottom:0px;display:inline;">${data}</p>`);
            }

            html.push("<br/>")
            if (item.balance > 0) {
                html.push(`<div class="tag tag-danger">Pendiente</div>`)
            }

            return html.join("");
        }
    },
    {
        targets: [6],
        data: "dispatchDate",
        render: function (data, type, item) {
            if (data)
                return moment(data, formatDate).format(formatDate);
            return "";
        }
    },
    {
        targets: [7],
        data: "id",
        render: function (data, type, item) {
            var html = [];
            html.push('<div class="dropdown">');
            html.push('<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>');
            html.push(`<div class="dropdown-menu" aria-label="dropdown${item.number}">`);

            html.push(`<a class="dropdown-item print_report" data-orderid="${item.id}"><i class="fa fa-file-pdf-o"></i>Imprimir Envío</a>`)
            html.push(`<a class="dropdown-item" href="/Orders/Details?id=${item.id}"><i class="ft-info"></i>Detalles</a>`)
            html.push(`<a class="dropdown-item" href="/Reclamo/Create?type=1&orderNumber=${item.number}&idClient=${item.clientId}"><i class="ft-file"></i>Crear Ticket</a>`)
            html.push(`<a class="dropdown-item" href="#" name="cancel" data-type="combo" data-id="${item.id}"><i class="ft-x"></i>Cancelar</a>`)

            html.push('</div>');
            html.push('</div>');
            return html.join("");
        },
    },
    {
        targets: [8],
        data: "searchPending",
        name: "searchPending",
        visible: false,
        render: function (data, type, item) {
            return "";
        }
    },
];

const columnsEntregada = [
    {
        targets: [0],
        orderable: false,
        data: "id",
        render: function (data, type, item) {
            var html = '<label class="custom-control custom-checkbox">' +
                `<input type="checkbox" class="custom-control-input order-select" value="${item.number}" />` +
                '<span class="custom-control-indicator"></span>' +
                '<span class="custom-control-description"></span>' +
                '</label>';
            return html;
        }
    },
    {
        targets: [1],
        data: "date",
        render: function (data, type, item) {
            return moment(data, formatDate).format(formatDate);
        }
    },
    {
        targets: [2],
        data: "number",
        name: "number",
        render: function (data, type, item) {
            var html = [];
            html.push(`<a href="/orders/details/${item.id}">${data}</a>`);

            if (item.invoiceId != null && item.isCreatedMovileApp) {
                html.push(`<div class="tag tag-success">Inv. ${item.invoiceNumber}</div>`)
            }

            if (item.noOrden) {
                html.push(`<div class="tag tag-success">${item.noOrden}</div>`)
            }

            if (item.agencyTransferidaId != null) {
                if (item.agencyTransferidaId == agencyId) {
                    html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.agencyName}</div>`);
                }
                else {
                    html.push(`<div style="display: block !important;" class="tag tag-info">T. a ${item.agencyTransferidaName}</div>`);
                }
            }
            else if (item.minoristaId != null) {
                html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.retailerName}</div>`)
            }

            if (item.express) {
                html.push(`<div class="tag tag-danger">Express</div>`)
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

            if (item.invoiceId != null && item.isCreatedMovileApp) {
                html.push(`<div class="tag tag-success">App</div>`)
            }
            return html.join("");
        }
    },
    {
        targets: [4],
        visible: true,
        data: "clientPhoneNumber",
        name: "clientPhoneNumber"
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
        data: "contactAddressProvince",
    },
    {
        targets: [7],
        data: "products",
        orderable: false,
        name: "products",
        render: function (data, type, item) {
            var html = [];
            if (data.length > 0) {
                var firstProduct = data[0];
                var products = [];
                var qty = 0;
                for (var i = 0; i < data.length; i++) {
                    const item = data[i];
                    qty += item.quantity;
                    products.push(item.name)
                }
                html.push(`<p style="padding:0px;margin:0px;">${firstProduct.wholesalerName ?? ""} (<b>${qty}</b>)</p>`);
                html.push(`<p style="padding:0px;margin:0px;">- ${products.join(" | ")}</p`);
            }

            return html.join("");
        }
    },
    {
        targets: [8],
        data: "status",
        orderable: false,
        name: "status",
        render: function (data, type, item) {
            var html = [];
            if (item.agencyTransferidaId != null) {
                if (item.agencyTransferidaId != agencyId) {
                    html.push(`<p style="color:gold;margin-top:0px;margin-bottom:0px;display:inline;">${data}</p>`);
                }
                else {
                    html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="5" data-pk="5" data-url="/post" data-title="Select status"></a>`)
                }
            }
            else {
                html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="5" data-pk="5" data-url="/post" data-title="Select status"></a>`)
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
        data: "dispatchDate",
        render: function (data, type, item) {
            if (data)
                return moment(data, formatDate).format(formatDate);
            return "";
        }
    },
    {
        targets: [10],
        data: "id",
        render: function (data, type, item) {
            var html = [];
            html.push('<div class="dropdown">');
            html.push('<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>');
            html.push(`<div class="dropdown-menu" aria-label="dropdown${item.number}">`);

            if (item.balance > 0) {
                html.push(`<a class="dropdown-item" href="/Orders/PayOrder?orderNumber=${item.number}"><i class="fa fa-usd"></i>Gestionar pago</a>`)
            }
            html.push(`<a class="dropdown-item" href="/Orders/PaysOrder?id=${item.id}"><i class="icon-wallet"></i>Registro de pagos</a>`)
            html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${item.id}"><i class="fa fa-usd"></i>Editar Pagos</a>`)
            html.push(`<a class="dropdown-item" href="/OrderNew/TrackingOrden?id=${item.id}"><i class="icon-list"></i>Tracking Orden</a>`)
            html.push(`<a class="dropdown-item" href="/OrderNew/EditCombo?id=${item.id}"><i class="ft-info"></i>Editar</a> `)
            html.push(`<a class="dropdown-item print_report" data-orderid="${item.id}"><i class="fa fa-file-pdf-o"></i>Imprimir Envío</a>`)
            html.push(`<a class="dropdown-item" href="/Orders/Details?id=${item.id}"><i class="ft-info"></i>Detalles</a>`)
            html.push(`<a class="dropdown-item" href="/Reclamo/Create?type=1&orderNumber=${item.number}&idClient=${item.clientId}"><i class="ft-file"></i>Crear Ticket</a>`)
            html.push(`<a class="dropdown-item" href="#" name="cancel" data-type="combo" data-id="${item.id}"><i class="ft-x"></i>Cancelar</a>`)

            html.push('</div>');
            html.push('</div>');
            return html.join("");
        },
    },
    {
        targets: [11],
        data: "searchPending",
        name: "searchPending",
        visible: false,
        render: function (data, type, item) {
            return "";
        }
    },
];

const columnsAll = [
    {
        targets: [0],
        orderable: false,
        data: "id",
        render: function (data, type, item) {
            var html = '<label class="custom-control custom-checkbox">' +
                `<input type="checkbox" class="custom-control-input order-select" value="${item.number}" />` +
                '<span class="custom-control-indicator"></span>' +
                '<span class="custom-control-description"></span>' +
                '</label>';
            return html;
        }
    },
    {
        targets: [1],
        data: "date",
        render: function (data, type, item) {
            return moment(data, formatDate).format(formatDate);
        }
    },
    {
        targets: [2],
        data: "number",
        name: "number",
        render: function (data, type, item) {
            var html = [];
            html.push(`<a href="/orders/details/${item.id}">${data}</a>`);

            if (item.invoiceId != null && item.isCreatedMovileApp) {
                html.push(`<div class="tag tag-success">Inv. ${item.invoiceNumber}</div>`)
            }

            if (item.noOrden) {
                html.push(`<div class="tag tag-success">${item.noOrden}</div>`)
            }

            if (item.agencyTransferidaId != null) {
                if (item.agencyTransferidaId == agencyId) {
                    html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.agencyName}</div>`);
                }
                else {
                    html.push(`<div style="display: block !important;" class="tag tag-info">T. a ${item.agencyTransferidaName}</div>`);
                }
            }
            else if (item.minoristaId != null) {
                html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.retailerName}</div>`)
            }

            if (item.express) {
                html.push(`<div class="tag tag-danger">Express</div>`)
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

            if (item.invoiceId != null && item.isCreatedMovileApp) {
                html.push(`<div class="tag tag-success">App</div>`)
            }
            return html.join("");
        }
    },
    {
        targets: [4],
        visible: true,
        data: "clientPhoneNumber",
        name: "clientPhoneNumber"
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
        data: "contactAddressProvince",
    },
    {
        targets: [7],
        data: "products",
        orderable: false,
        name: "products",
        render: function (data, type, item) {
            var html = [];
            if (data.length > 0) {
                var firstProduct = data[0];
                var products = [];
                var qty = 0;
                for (var i = 0; i < data.length; i++) {
                    const item = data[i];
                    qty += item.quantity;
                    products.push(item.name)
                }
                html.push(`<p style="padding:0px;margin:0px;">${firstProduct.wholesalerName ?? ""} (<b>${qty}</b>)</p>`);
                html.push(`<p style="padding:0px;margin:0px;">- ${products.join(" | ")}</p`);
            }

            return html.join("");
        }
    },
    {
        targets: [8],
        data: "status",
        orderable: false,
        name: "status",
        render: function (data, type, item) {
            var html = [];
            html.push(`<p style="color:gold;margin-top:0px;margin-bottom:0px;display:inline;">${data}</p>`);

            html.push("<br/>")
            if (item.balance > 0) {
                html.push(`<div class="tag tag-danger">Pendiente</div>`)
            }

            return html.join("");
        }
    },
    {
        targets: [9],
        data: "id",
        render: function (data, type, item) {
            var html = [];
            html.push('<div class="dropdown">');
            html.push('<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>');
            html.push(`<div class="dropdown-menu" aria-label="dropdown${item.number}">`);

            if (item.balance > 0) {
                html.push(`<a class="dropdown-item" href="/Orders/PayOrder?orderNumber=${item.number}"><i class="fa fa-usd"></i>Gestionar pago</a>`)
            }
            html.push(`<a class="dropdown-item" href="/Orders/PaysOrder?id=${item.id}"><i class="icon-wallet"></i>Registro de pagos</a>`)
            html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${item.id}"><i class="fa fa-usd"></i>Editar Pagos</a>`)
            html.push(`<a class="dropdown-item" href="/OrderNew/TrackingOrden?id=${item.id}"><i class="icon-list"></i>Tracking Orden</a>`)
            html.push(`<a class="dropdown-item" href="/OrderNew/EditCombo?id=${item.id}"><i class="ft-info"></i>Editar</a> `)
            html.push(`<a class="dropdown-item print_report" data-orderid="${item.id}"><i class="fa fa-file-pdf-o"></i>Imprimir Envío</a>`)
            html.push(`<a class="dropdown-item" href="/Orders/Details?id=${item.id}"><i class="ft-info"></i>Detalles</a>`)
            html.push(`<a class="dropdown-item" href="/Reclamo/Create?type=1&orderNumber=${item.number}&idClient=${item.clientId}"><i class="ft-file"></i>Crear Ticket</a>`)
            html.push(`<a class="dropdown-item" href="#" name="cancel" data-type="combo" data-id="${item.id}"><i class="ft-x"></i>Cancelar</a>`)

            html.push('</div>');
            html.push('</div>');
            return html.join("");
        },
    }
];

const columnsEntregadaEmpleadoCuba = [
    {
        targets: [0],
        orderable: false,
        data: "id",
        render: function (data, type, item) {
            var html = '<label class="custom-control custom-checkbox">' +
                `<input type="checkbox" class="custom-control-input order-select" value="${item.number}" />` +
                '<span class="custom-control-indicator"></span>' +
                '<span class="custom-control-description"></span>' +
                '</label>';
            return html;
        }
    },
    {
        targets: [1],
        data: "date",
        render: function (data, type, item) {
            return moment(data, formatDate).format(formatDate);
        }
    },
    {
        targets: [2],
        data: "number",
        name: "number",
        render: function (data, type, item) {
            var html = [];
            html.push(`<a href="/orders/details/${item.id}">${data}</a>`);

            if (item.invoiceId != null && item.isCreatedMovileApp) {
                html.push(`<div class="tag tag-success">Inv. ${item.invoiceNumber}</div>`)
            }

            if (item.noOrden) {
                html.push(`<div class="tag tag-success">${item.noOrden}</div>`)
            }

            if (item.agencyTransferidaId != null) {
                if (item.agencyTransferidaId == agencyId) {
                    html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.agencyName}</div>`);
                }
                else {
                    html.push(`<div style="display: block !important;" class="tag tag-info">T. a ${item.agencyTransferidaName}</div>`);
                }
            }
            else if (item.minoristaId != null) {
                html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.retailerName}</div>`)
            }

            if (item.express) {
                html.push(`<div class="tag tag-danger">Express</div>`)
            }

            return html.join("");
        },
    },
    {
        targets: [3],
        data: "contactFullData",
        name: "contactFullData",
        render: function (data, type, item) {
            return `<div title="${item.contactFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">${item.contactFullData}</div>`
        }
    },
    {
        targets: [4],
        data: "contactAddressProvince",
    },
    {
        targets: [5],
        data: "status",
        orderable: false,
        name: "status",
        render: function (data, type, item) {
            var html = [];
            if (item.agencyTransferidaId != null) {
                if (item.agencyTransferidaId != agencyId) {
                    html.push(`<p style="color:gold;margin-top:0px;margin-bottom:0px;display:inline;">${data}</p>`);
                }
                else {
                    html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="5" data-pk="5" data-url="/post" data-title="Select status"></a>`)
                }
            }
            else {
                html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="5" data-pk="5" data-url="/post" data-title="Select status"></a>`)
            }

            html.push("<br/>")
            if (item.balance > 0) {
                html.push(`<div class="tag tag-danger">Pendiente</div>`)
            }

            return html.join("");
        }
    },
    {
        targets: [6],
        data: "dispatchDate",
        render: function (data, type, item) {
            if (data)
                return moment(data, formatDate).format(formatDate);
            return "";
        }
    },
    {
        targets: [7],
        data: "id",
        render: function (data, type, item) {
            var html = [];
            html.push('<div class="dropdown">');
            html.push('<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>');
            html.push(`<div class="dropdown-menu" aria-label="dropdown${item.number}">`);

            html.push(`<a class="dropdown-item print_report" data-orderid="${item.id}"><i class="fa fa-file-pdf-o"></i>Imprimir Envío</a>`)
            html.push(`<a class="dropdown-item" href="/Orders/Details?id=${item.id}"><i class="ft-info"></i>Detalles</a>`)
            html.push(`<a class="dropdown-item" href="/Reclamo/Create?type=1&orderNumber=${item.number}&idClient=${item.clientId}"><i class="ft-file"></i>Crear Ticket</a>`)
            html.push(`<a class="dropdown-item" href="#" name="cancel" data-type="combo" data-id="${item.id}"><i class="ft-x"></i>Cancelar</a>`)

            html.push('</div>');
            html.push('</div>');
            return html.join("");
        },
    },
    {
        targets: [8],
        data: "searchPending",
        name: "searchPending",
        visible: false,
        render: function (data, type, item) {
            return "";
        }
    },
];

const columnsCancelada = [
    {
        targets: [0],
        data: "date",
        render: function (data, type, item) {
            return moment(data, formatDate).format(formatDate);
        }
    },
    {
        targets: [1],
        data: "number",
        name: "number",
        render: function (data, type, item) {
            var html = [];
            html.push(`<a href="/orders/details/${item.id}">${data}</a>`);

            if (item.invoiceId != null && item.isCreatedMovileApp) {
                html.push(`<div class="tag tag-success">Inv. ${item.invoiceNumber}</div>`)
            }

            if (item.noOrden) {
                html.push(`<div class="tag tag-success">${item.noOrden}</div>`)
            }

            if (item.agencyTransferidaId != null) {
                if (item.agencyTransferidaId == agencyId) {
                    html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.agencyName}</div>`);
                }
                else {
                    html.push(`<div style="display: block !important;" class="tag tag-info">T. a ${item.agencyTransferidaName}</div>`);
                }
            }
            else if (item.minoristaId != null) {
                html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.retailerName}</div>`)
            }

            if (item.express) {
                html.push(`<div class="tag tag-danger">Express</div>`)
            }

            return html.join("");
        },
    },
    {
        targets: [2],
        visible: true,
        data: "clientFullData",
        name: "clientFullData",
        render: function (data, type, item) {
            var html = [];
            html.push(`<div title="${item.clientFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">`);
            html.push(`${item.clientFullData}`);
            html.push(`</div>`)

            if (item.invoiceId != null && item.isCreatedMovileApp) {
                html.push(`<div class="tag tag-success">App</div>`)
            }
            return html.join("");
        }
    },
    {
        targets: [3],
        data: "products",
        orderable: false,
        name: "products",
        render: function (data, type, item) {
            var html = [];
            if (data.length > 0) {
                var firstProduct = data[0];
                var products = [];
                var qty = 0;
                for (var i = 0; i < data.length; i++) {
                    const item = data[i];
                    qty += item.quantity;
                    products.push(item.name)
                }
                html.push(`<p style="padding:0px;margin:0px;">${firstProduct.wholesalerName ?? ""} (<b>${qty}</b>)</p>`);
                html.push(`<p style="padding:0px;margin:0px;">- ${products.join(" | ")}</p`);
            }

            return html.join("");
        }
    },
    {
        targets: [4],
        data: "status",
        orderable: false,
        name: "status",
    },
    {
        targets: [5],
        data: "amount",
        render: function (data, type, item) {
            var html = [];
            if (item.agencyTransferidaId != null) {
                if (item.agencyTransferidaId == agencyId) {
                    var aux = item.wholesalerCost + item.otherCost;
                    html.push(`<text>$ ${aux.toFixed(2)}</text>`)
                }
                else {
                    html.push(`<text>${parseFloat(item.amount).toFixed(2)}</text >`)
                }
            }
            else if (item.minoristaId != null) {
                var aux = item.wholesalerCost + item.otherCost;
                html.push(`<text>$ ${aux.toFixed(2)}</text>`)
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
            html.push(`<a class="dropdown-item" href="/OrderNew/TrackingOrden?id=${item.id}"><i class="icon-list"></i>Tracking Orden</a>`)
            html.push(`<a class="dropdown-item print_report" data-orderid="${item.id}"><i class="fa fa-file-pdf-o"></i>Imprimir Envío</a>`)
            html.push(`<a class="dropdown-item" href="/Orders/Details?id=${item.id}"><i class="ft-info"></i>Detalles</a>`)

            html.push('</div>');
            html.push('</div>');
            return html.join("");
        },
    },
];

const columnsCanceladaEmpleadoCuba = [
    {
        targets: [0],
        data: "date",
        render: function (data, type, item) {
            return moment(data, formatDate).format(formatDate);
        }
    },
    {
        targets: [1],
        data: "number",
        name: "number",
        render: function (data, type, item) {
            var html = [];
            html.push(`<a href="/orders/details/${item.id}">${data}</a>`);

            if (item.invoiceId != null && item.isCreatedMovileApp) {
                html.push(`<div class="tag tag-success">Inv. ${item.invoiceNumber}</div>`)
            }

            if (item.noOrden) {
                html.push(`<div class="tag tag-success">${item.noOrden}</div>`)
            }

            if (item.agencyTransferidaId != null) {
                if (item.agencyTransferidaId == agencyId) {
                    html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.agencyName}</div>`);
                }
                else {
                    html.push(`<div style="display: block !important;" class="tag tag-info">T. a ${item.agencyTransferidaName}</div>`);
                }
            }
            else if (item.minoristaId != null) {
                html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.retailerName}</div>`)
            }

            if (item.express) {
                html.push(`<div class="tag tag-danger">Express</div>`)
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
        targets: [3],
        data: "status",
        orderable: false,
        name: "status",
    },
    {
        targets: [4],
        data: "amount",
        render: function (data, type, item) {
            var html = [];
            if (item.agencyTransferidaId != null) {
                if (item.agencyTransferidaId == agencyId) {
                    var aux = item.wholesalerCost + item.otherCost;
                    html.push(`<text>$ ${aux.toFixed(2)}</text>`)
                }
                else {
                    html.push(`<text>${parseFloat(item.amount).toFixed(2)}</text >`)
                }
            }
            else if (item.minoristaId != null) {
                var aux = item.wholesalerCost + item.otherCost;
                html.push(`<text>$ ${aux.toFixed(2)}</text>`)
            }
            else {
                html.push(`<text>${parseFloat(item.amount).toFixed(2)}</text >`)
            }

            return html.join("");
        },
    },
    {
        targets: [5],
        data: "id",
        render: function (data, type, item) {
            var html = [];
            html.push('<div class="dropdown">');
            html.push('<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>');
            html.push(`<div class="dropdown-menu" aria-label="dropdown${item.number}">`);
            html.push(`<a class="dropdown-item" href="/OrderNew/TrackingOrden?id=${item.id}"><i class="icon-list"></i>Tracking Orden</a>`)
            html.push(`<a class="dropdown-item print_report" data-orderid="${item.id}"><i class="fa fa-file-pdf-o"></i>Imprimir Envío</a>`)
            html.push(`<a class="dropdown-item" href="/Orders/Details?id=${item.id}"><i class="ft-info"></i>Detalles</a>`)

            html.push('</div>');
            html.push('</div>');
            return html.join("");
        },
    },
];

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
        switch (tableName) {
            case "Iniciada":
                var t = $('#tableIniciada');
                tableIniciada = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: true,
                    lengthMenu: [[10, 100], [10,"All"]],
                    order: [[1, "desc"]],
                    serverSide: true,
                    processing: false,
                    language: tableLanguage,
                    ajax: $.fn.dataTable.pipeline({
                        url: `/airshipping/GetDataCombos?status=Iniciada`,
                        pages: numberPages, // number of pages to cache,
                        data: function (dtp) {
                            return dtp;
                        },
                        dataFilter: function (data) {
                            var json = jQuery.parseJSON(data);
                            $("#base_tabIniciada").html(`Iniciadas (${json.recordsTotal})`);
                            return JSON.stringify(json); // return JSON string
                        }
                    }),
                    columnDefs: [
                        {
                            targets: [0],
                            orderable: false,
                            data: "id",
                            render: function (data, type, item) {
                                var html = '<label class="custom-control custom-checkbox">' +
                                    `<input type="checkbox" class="custom-control-input order-select" value="${item.number}" />` +
                                    '<span class="custom-control-indicator"></span>' +
                                    '<span class="custom-control-description"></span>' +
                                    '</label>';
                                return html;
                            }
                        },
                        {
                            targets: [1],
                            data: "date",
                            render: function (data, type, item) {
                                return moment(data, formatDate).format(formatDate);
                            }
                        },
                        {
                            targets: [2],
                            data: "number",
                            name: "number",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a href="/orders/details/${item.id}">${data}</a>`);

                                if (item.invoiceId != null && item.isCreatedMovileApp) {
                                    html.push(`<div class="tag tag-success">Inv. ${item.invoiceNumber}</div>`)
                                }

                                if (item.noOrden) {
                                    html.push(`<div class="tag tag-success">${item.noOrden}</div>`) 
                                }

                                if (item.agencyTransferidaId != null) {
                                    if (item.agencyTransferidaId == agencyId) {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.agencyName}</div>`);
                                    }
                                    else {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">T. a ${item.agencyTransferidaName}</div>`);
                                    }
                                }
                                else if (item.minoristaId != null) {
                                    html.push(`<div style="display: block !important;" class="tag tag-info">T. de ${item.retailerName}</div>`)
                                }

                                if(item.express)
                                {
                                    html.push(`<div class="tag tag-danger">Express</div>`) 
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

                                if (item.invoiceId != null && item.isCreatedMovileApp) {
                                    html.push(`<div class="tag tag-success">App</div>`) 
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [4],
                            visible: true,
                            data: "clientPhoneNumber",
                            name: "clientPhoneNumber"
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
                            data: "contactAddressProvince",
                        },
                        {
                            targets: [7],
                            data: "products",
                            orderable: false,
                            name: "products",
                            render: function (data, type, item) {
                                var html = [];
                                if (data.length > 0) {
                                    var firstProduct = data[0];
                                    var products = [];
                                    var qty = 0;
                                    for (var i = 0; i < data.length; i++) {
                                        const item = data[i];
                                        qty += item.quantity;
                                        products.push(item.name)
                                    }
                                    html.push(`<p style="padding:0px;margin:0px;">${firstProduct.wholesalerName ?? ""} (<b>${qty}</b>)</p>`);
                                    html.push(`<p style="padding:0px;margin:0px;">- ${products.join(" | ")}</p`);
                                }

                                return html.join("");
                            }
                        },
                        {
                            targets: [8],
                            data: "status",
                            orderable : false,
                            name: "status",
                            render: function (data, type, item) {
                                var html = [];
                                if (item.agencyTransferidaId != null) {
                                    if (item.agencyTransferidaId != agencyId) {
                                        if (item.status == "Iniciada") {
                                            html.push(`<p style="color:orange;margin-top:0px;margin-bottom:0px;display:inline;">${data}</p>`);
                                        }
                                    }
                                    else {
                                        if (item.status == "Iniciada") {
                                            html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="1" data-pk="1" data-url="/post" data-title="Select status"></a>`)   
                                        }
                                    }
                                }
                                else {
                                    if (item.status == "Iniciada") {
                                        html.push(`<a href="#" class="status" data-id="${item.id}" data-type="select" data-value="1" data-pk="1" data-url="/post" data-title="Select status"></a>`)   
                                    }
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
                            data: "amount",
                            render: function (data, type, item) {
                                var html = [];
                                if (item.agencyTransferidaId != null) {
                                    if (item.agencyTransferidaId == agencyId) {
                                        var aux = item.wholesalerCost + item.otherCost;
                                        html.push(`<text>$ ${aux.toFixed(2)}</text>`)
                                    }
                                    else {
                                        html.push(`<text>${parseFloat(item.amount).toFixed(2)}</text >`)
                                    }
                                }
                                else if (item.minoristaId != null) {
                                    var aux = item.wholesalerCost + item.otherCost;
                                    html.push(`<text>$ ${aux.toFixed(2)}</text>`)
                                }
                                else {
                                    html.push(`<text>${parseFloat(item.amount).toFixed(2)}</text >`)
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

                                if (role != "EmpleadoCuba") {
                                    if (item.balance > 0) {
                                        html.push(`<a class="dropdown-item" href="/Orders/PayOrder?orderNumber=${item.number}"><i class="fa fa-usd"></i>Gestionar pago</a>`)
                                    }
                                    html.push(`<a class="dropdown-item" href="/Orders/PaysOrder?id=${item.id}"><i class="icon-wallet"></i>Registro de pagos</a>`)
                                    html.push(`<a class="dropdown-item" href="/Orders/EditarPagos?id=${item.id}"><i class="fa fa-usd"></i>Editar Pagos</a>`)
                                    html.push(`<a class="dropdown-item" href="/OrderNew/TrackingOrden?id=${item.id}"><i class="icon-list"></i>Tracking Orden</a>`)
                                }

                                if (role === "Agencia" || role === "Empleado") {
                                    html.push(`<a class="dropdown-item" href="/OrderNew/EditCombo?id=${item.id}"><i class="ft-info"></i>Editar</a> `)
                                }

                                html.push(`<a class="dropdown-item print_report" data-orderid="${item.id}"><i class="fa fa-file-pdf-o"></i>Imprimir Envío</a>`)
                                html.push(`<a class="dropdown-item" href="/Orders/Details?id=${item.id}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`<a class="dropdown-item" href="/Reclamo/Create?type=1&orderNumber=${item.number}&idClient=${item.clientId}"><i class="ft-file"></i>Crear Ticket</a>`)
                                html.push(`<a class="dropdown-item" href="#" name="cancel" data-type="combo" data-id="${item.id}"><i class="ft-x"></i>Cancelar</a>`)

                                html.push('</div>');
                                html.push('</div>');
                                return html.join("");
                            },
                        },
                        {
                            targets: [11],
                            data: "searchPending",
                            name: "searchPending",
                            visible: false,
                            render: function (data, type, item) {
                                return "";
                            }
                        },

                    ],
                    createdRow: function (row, data, rowIndex) {
                        InitRow(row);
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                    },
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableIniciada));
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
                        url: `/airshipping/GetDataCombos?status=Despachada`,
                        pages: numberPages, // number of pages to cache,
                        data: function (dtp) {
                            return dtp;
                        },
                        dataFilter: function (data) {
                            var json = jQuery.parseJSON(data);
                            $("#base_tabDespachada").html(`Despachadas (${json.recordsTotal})`);
                            return JSON.stringify(json); // return JSON string
                        }
                    }),
                    columnDefs: role == "EmpleadoCuba" ? columnsDespachadaEmpleadoCuba : columnsDespachada,
                    createdRow: function (row, data, rowIndex) {
                        InitRow(row);
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                        
                    },
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableDespachada));
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
                        url: `/airshipping/GetDataCombos?status=Entregada`,
                        pages: numberPages, // number of pages to cache,
                        data: function (dtp) {
                            return dtp;
                        },
                        dataFilter: function (data) {
                            var json = jQuery.parseJSON(data);
                            $("#base_tabEntregada").html(`Entregadas (${json.recordsTotal})`);
                            return JSON.stringify(json); // return JSON string
                        }
                    }),
                    columnDefs: role == "EmpleadoCuba" ? columnsEntregadaEmpleadoCuba : columnsEntregada,
                    createdRow: function (row, data, rowIndex) {
                        InitRow(row);
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                    },
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
                    ajax: $.fn.dataTable.pipeline({
                        url: `/airshipping/GetDataCombos?status=Cancelada`,
                        pages: numberPages, // number of pages to cache,
                        data: function (dtp) {
                            return dtp;
                        },
                        dataFilter: function (data) {
                            var json = jQuery.parseJSON(data);
                            $("#base_tabCancelada").html(`Canceladas (${json.recordsTotal})`);
                            return JSON.stringify(json); // return JSON string
                        }
                    }),
                    columnDefs: role == "EmpleadoCuba" ? columnsCanceladaEmpleadoCuba : columnsCancelada,
                    createdRow: function (row, data, rowIndex) {
                        InitRow(row);
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                        
                    },
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableCancelada));
                })
                break;
            case "All":
                var t = $('#tableAll');
                tableAll = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: true,
                    order: [[1, "desc"]],
                    serverSide: true,
                    processing: false,
                    language: tableLanguage,
                    ajax: $.fn.dataTable.pipeline({
                        url: `/airshipping/GetDataCombos?status=All`,
                        pages: numberPages, // number of pages to cache,
                        data: function (dtp) {
                            return dtp;
                        },
                        dataFilter: function (data) {
                            var json = jQuery.parseJSON(data);
                            $("#base_tabAll").html(`Todas (${json.recordsTotal})`);
                            return JSON.stringify(json); // return JSON string
                        }
                    }),
                    columnDefs: columnsAll,
                    createdRow: function (row, data, rowIndex) {
                        InitRow(row);
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                    },
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableAll));
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

    $(block_ele).unblock();
    LoadEvents();

})

function InitRow(row) {
    var credito = false;
    $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); });
    $.each($('.status', row), (i, e) => {
        $(e).editable({
            source: function (p) {
                s = [
                    {
                        "1": 'Iniciada',
                        //"3": 'Cancelada',
                        "4": 'Despachada'
                    },
                    {
                        "1": 'Iniciada',
                        //"3": 'Cancelada',
                        "4": 'Despachada'
                    },
                    {
                        //"3": 'Cancelada',
                    },
                    {//Estado Despachada
                        "1": 'Iniciada',
                        "4": 'Despachada',
                        "5": "Entregada"
                    },
                    {//Estado Entregada
                        //"3": 'Cancelada',
                        "4": 'Despachada',
                        "5": "Entregada"
                    },

                ];
                return s[$(this).data('value') - 1];

            },
            validate: function (x) {
                if (x == "Cancelada")
                    if (!confirm("Desea asignarle el valor pagado como crédito al cliente"))
                        credito = false;
                    else
                        credito = true;
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
                url: '/OrderNew/IndexCombo',
                type: 'post',
                dataType: 'json'
            },
            params: function (params) {
                params.id = $(this).data("id");
                params.credito = credito;
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
                url: "/Orders/createFileOrder",
                data: {
                    id: orderid
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

    });

    $.each($('.order-select', row), (i, e) => {
        $(e).on('change', function () {
            if ($(this).val() != "all") {
                // Para desabilitar el check todo si se selecciona algun otro check
                $('#checkalltramites').prop('checked', false);
                if ($(this)[0].checked) {
                    cantSelect++;
                } else {
                    cantSelect--;
                }

                var tabactive = $('[name = "tab"][class = "nav-link active"]');
                if (/*selectedIds.length*/ cantSelect == 0) {
                    $("#gen_report").addClass("hidden");

                } else {
                    $("#gen_report").removeClass("hidden");
                }

                updatedespacho(); //Para actualizar la cantidad de tramites a despachar
            }
        })
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

    $('#searchClientName').on('keypress', function (e) {
        if (e.key == "Enter")
            search($(this).val(), "ClientName");
    });
    $('#btnSearchClientName').on('click', function (e) {
        search($('#searchClientName').val(), "ClientName");
    });

    $('#searchClientPhone').on('keypress', function (e) {
        if (e.key == "Enter")
            search($(this).val(), "ClientPhone");
    });
    $('#btnSearchClientPhone').on('click', function (e) {
        search($('#searchClientPhone').val(), "ClientPhone");
    });

    $('#searchContactName').on('keypress', function (e) {
        if (e.key == "Enter")
            search($(this).val(), "ContactName");
    });
    $('#btnSearchContactName').on('click', function (e) {
        search($('#searchContactName').val(), "ContactName");
    });

    $('#searchProduct').on('keypress', function (e) {
        if (e.key == "Enter")
            search($(this).val(), "Product");
    });
    $('#btnSearchProduct').on('click', function (e) {
        search($('#searchProduct').val(), "Product");
    });

    $('#searchPending').on('click', function (e) {
        search($(this).is(':checked'), "SearchPending");
    });

    $('#clearFilters').on('click', function () {
        $('#searchNumber').val("");
        $('#searchClientName').val("");
        $('#searchClientPhone').val("");
        $('#searchContactName').val("");
        $('#searchProduct').val("");
        $('#searchPending').prop('checked', false)
        ClearFiltersTables();
    })

    var url = decodeURIComponent(window.location);
    var params = url.split("?")[1];
    if (params != null) {
        var params = params.split("&");
        var msg = params[0].split("=")[1];
        if (msg == "success") {
            var orderNumber = params[1].split("=")[1];
            showOKMessage("Nueva Orden", "Orden " + orderNumber + " creada con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "updateStatusOrden") {
            var orderNumber = params[1].split("=")[1];
            showOKMessage("Actualización de Estado", "Estatus actualizado a " + orderNumber, { "timeOut": 0, "closeButton": true });
        } else if (msg == "successEdit") {
            var orderNumber = params[1].split("=")[1];
            showOKMessage("Editar Orden", "Orden " + orderNumber + " editada con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successDelete") {
            var orderNumber = params[1].split("=")[1];
            showOKMessage("Eliminar Orden", "Orden " + orderNumber + " eliminada con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successCancell") {
            var orderNumber = params[1].split("=")[1];
            showOKMessage("Cancelar Orden", "Orden " + orderNumber + " cancelada con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successPago") {
            var pagoNumber = params[1].split("=")[1];
            showOKMessage("Registro de Pago", "Se ha efectuado el pago de la orden " + pagoNumber + " con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successEntregada") {
            var orderNumber = params[1].split("=")[1];
            showOKMessage("Orden Entregada", "Orden " + ordenNumber + " marcada como entregada con éxito", { "timeOut": 0, "closeButton": true });
        } else {
            var aux = msg.split('-');
            if (aux.length == 2) {
                if (aux[0] == "success") {
                    toastr.success(aux[1]);
                }
                else {
                    toastr.success(aux[1]);
                }
            }

        }
    }

    $("[data-toggle='tab']").click(function () {
        $("[data-toggle='tab']").removeClass("active");
    });

    $("#gen_report").click(function () {

        selectedIds = new Array;

        $(".order-select").each(function (i, e) {
            if (e.checked) {
                if (e.value != "all") {
                    selectedIds.push($(e).val());
                }
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

    // Para seleccionar todo
    $('#checkalltramites').on('change', function () {
        var table = $('#' + nameTable);
        if ($(this)[0].checked) {
            table.find('.order-select').prop('checked', true)
            cantSelect = table.find('.order-select').length - 1;
            $("#gen_report").removeClass("hidden");
        } else {
            table.find('.order-select').prop('checked', false)
            cantSelect = 0;
            $("#gen_report").addClass("hidden");
        }
        updatedespacho(); //Para actualizar la cantidad de tramites a despachar
    });

    var searchAction = function () {
        var searchVal = $("#searchField").val().toLowerCase();

        $("#" + nameTable + " tr").removeClass("hidden");

        var tBody = $("#" + nameTable + " > tbody")[0];

        for (var i = 0; i < tBody.rows.length; i++) {
            var fila = tBody.rows[i];
            if (!$(fila.children[1]).html().toLowerCase().includes(searchVal) && !$(fila.children[5]).html().toLowerCase().includes(searchVal))
                $(fila).addClass("hidden");
        }

        var cantHide = $("#" + nameTable + " tr.hidden").length;
        if (tBody.rows.length == cantHide)
            $("#no_result").removeClass("hidden");
        else
            $("#no_result").addClass("hidden");
    };

    $("#btnSearch").click(searchAction);
    $("#searchField").on("keyup", searchAction);

    $("#base-tab1").click(function () {
        tab = 1;

        $("#searchBtn").val("");
        nameTable = "tableIniciada";
        $("#tableIniciada tr").removeClass("hidden");

        if ($("#tableIniciada tr").length == 1)
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
        nameTable = "tableDespachada";
        $("#tableDespachada tr").removeClass("hidden");

        if ($("#tableDespachada tr").length == 1)
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
        nameTable = "tableEntregada";
        $("#tableEntregada tr").removeClass("hidden");

        if ($("#tableEntregada tr").length == 1)
            $("#no_result").removeClass("hidden");
        else
            $("#no_result").addClass("hidden");

        selectedIds = new Array;
        $(".order-select").attr("checked", false);
        $("#gen_report").addClass("hidden");
    });

    
    $(".checkEntregada").click(function () {
        var orderId = $(this).attr("orderId");
        var orderNumber = $(this).attr("name");
        var okConfirm = function () {
            var urlEntregar = "/Orders/CheckEntregada/" + orderId;
            $.ajax({
                type: "GET",
                url: urlEntregar,
                async: false,
                success: function () {
                    document.location = "/Orders?msg=successEntregada&orderNumber=" + orderNumber;
                }
            });
        };
        confirmationMsg("¿Está seguro que desea marcar como entregada esta orden?", "", okConfirm);
    });

    $(".deleteOrder").click(function () {
        var orderId = $(this).attr("orderId");
        var orderNumber = $(this).attr("name");
        var okConfirm = function () {
            var urlDelete = "/Orders/Delete/" + orderId;
            $.ajax({
                type: "GET",
                url: urlDelete,
                async: false,
                success: function () {
                    document.location = "/OrderNew?msg=successDelete&orderNumber=" + orderNumber;
                }
            });
        };
        getCancelConfirmation(okConfirm);
    });

    $(".cancelOrder").click(function () {
        var orderId = $(this).attr("orderId");
        var orderNumber = $(this).attr("name");
        swal({
            title: "¿Está seguro que desea cancelar esta order?",
            text: "Esta acción no se podrá revertir",
            type: "warning",
            showCancelButton: true,
            confirmButtonText: "Si, Cancelar",
            cancelButtonText: "No",
            closeOnConfirm: false,
            closeOnCancel: true
        }, function (isConfirm) {
            if (isConfirm) {
                swal({
                    title: "¿Desea asignarle el valor pagado como crédito al cliente?",
                    text: "",
                    type: "warning",
                    showCancelButton: true,
                    confirmButtonText: "Si",
                    cancelButtonText: "No",
                    closeOnConfirm: true,
                    closeOnCancel: true,
                }, function (ConfirmCredito) {
                    if (ConfirmCredito) {
                        $.ajax({
                            type: "GET",
                            url: "/Orders/Cancel?id=" + orderId + '&credito=true',
                            async: false,
                            success: function (response) {
                                if (response.success)
                                    document.location = "/OrderNew?msg=successCancell&orderNumber=" + orderNumber;
                                else
                                    toastr.error(response.msg);
                                $.unblockUI()
                            }
                        });
                    }
                    else {
                        $.ajax({
                            type: "GET",
                            url: "/Orders/Cancel?id=" + orderId + '&credito=false',
                            async: false,
                            success: function (response) {
                                if (response.success)
                                    document.location = "/OrderNew?msg=successCancell&orderNumber=" + orderNumber;
                                else
                                    toastr.error(response.msg);
                                $.unblockUI()
                            }
                        });
                    }
                });
            }
        });
    });

    //Para el despacho
    $('#despachar').on('click', function () {
        $('#modalDespachar').modal('show');
        findMayoristas();
    });

    // Para mostrar el boton de despacho al seleccionarse las iniciadas
    $('[name="tab"]').on('click', function () {
        if ($(this).attr('data-type') == "iniciadas/pendientes") {
            $('#despachar').removeClass("hidden");
        }
        else {
            $('#despachar').addClass("hidden");
        }

    });

    $('#tipotramite').select2({
        placeholder: "Trámite a Despachar",
        val: null,
    });

    //**************************************
    var cantSelectMayorista = 0;

    var selectedIdsMayorista;

    $('[name="checkmayorista"]').on("change", function () {
        if ($(this).val() != "all") {
            // Para desabilitar el check todo si se selecciona algun otro check
            $('#checkall').prop('checked', false);

            if ($(this)[0].checked) {
                cantSelectMayorista++;
            } else {
                cantSelectMayorista--;
            }
        }
    });

    // Para seleccionar todo
    $('#checkall').on('change', function () {
        if ($(this)[0].checked) {
            $('[name="checkmayorista"]').prop('checked', true)
            cantSelectMayorista = $('[name="checkmayorista"]').length - 1;

        } else {
            $('[name="checkmayorista"]').prop('checked', false)
            cantSelectMayorista = 0;

        }
    });

    var type = "0";
    $('[name="tipoDespacho"]').on('change', function () {
        type = $(this).val();

        if (type == "0") {
            $('#despachoMayorista').show();
            $('#despachoDistribuidor').hide();
        }
        else {
            $('#despachoMayorista').hide();
            $('#despachoDistribuidor').show();
        }
    })

    $("#creardespacho").click(function () {
        var distributorId = $('#ditribuidorId').val();
        var selectedIdsMayorista = new Array;
        var selectedIds = new Array;
        var rangofecha = $('#fechaReporte').val();
        var ischecked = $('#checkrange').is(':checked');
        var emails = $('#emails').val();

        //Check del mayorista
        $(document).find('[name="checkmayorista"]').each(function (i, e) {
            if (e.checked) {
                if (e.value != "all") {
                    selectedIdsMayorista.push($(e).val());
                }
            }
        });

        //Check de ordenes
        if (cantSelect > 0) {
            $(".order-select").each(function (i, e) {
                if (e.checked) {
                    if (e.value != "all") {
                        selectedIds.push($(e).val());
                    }
                }
            });
        }
        var request = {
            type: type,
            idWholesalers: selectedIdsMayorista,
            idPrincipalDistributor: distributorId,
            idsOrder: selectedIds,
            emails: emails,
            rangoFecha: rangofecha,
            isCheked: ischecked
        };

        if (request.idsOrder.length == 0) {
            toastr.info("Debe seleccionar al menos un trámite para despachar");
            return false;
        }

        if (request.type == "0" && request.idWholesalers.length == 0) {
            toastr.info("Debe seleccionar un mayorista a despachar");
            return false;
        }

        if (request.type == "1" && request.idPrincipalDistributor.length == 0) {
            toastr.info("Debe seleccionar un distribuidor a despachar");
            return false;
        }

        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/json",
            url: "/ordernew/despachar",
            data: JSON.stringify(request),
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
                var aux = response.split('-');
                if (aux[0] == "error") {
                    toastr.error(aux[1]);
                    $.unblockUI();
                }
                else {
                    location.href = "/OrderNew/combos?msg=" + response;
                }
            },
            error: function () {
                toastr.error("No se ha podido despachar", "Error");
                $.unblockUI()
            },
            timeout: 60000,
        });

    });

    $('#checkrange').on('click', function () {
        var ischeked = $(this).is(':checked');
        if (ischeked) {
            $('#fechaReporte').prop('disabled', false);
        }
        else {
            $('#fechaReporte').prop('disabled', true);
        }
    });

    //Para cargar los mayoristas
    function findMayoristas() {
        var idsorders = seleccionarIds();
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/ordernew/getMAyoristasCombos",
            data: {
                ids: idsorders,
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
                console.log(response)

                var aux = response.msg.split('-');
                if (aux[0] == "error") {
                    toastr.error(aux[1]);
                }
                else {
                    // Actualizar tabla de mayoristas
                    $('#addFila').html("");
                    for (var i = 0; i < response.data.length; i++) {
                        var datoMayorista = response.data[i];
                        $('#addFila').append(
                            '<tr>' +
                            '<th style = "padding-bottom:0px" >' +
                            '<label class="custom-control custom-checkbox">' +
                            '<input type="checkbox" name="checkmayorista" class="custom-control-input" value="' + datoMayorista.idmayorista + '" />' +
                            '<span class="custom-control-indicator"></span>' +
                            '<span class="custom-control-description"></span>' +
                            '</label>' +
                            '</th>' +
                            '<td>' + datoMayorista.nombre + '</td>' +
                            '</tr>');
                    }
                }
                $.unblockUI();
            },
            error: function () {
                toastr.error("No se han podido obtener los mayoristas", "Error");
                $.unblockUI()
            },
            timeout: 60000,
        });
    }


    $('#btnShowToggleColumn').on('click', function () {
        if (tab == 1) {
            $('#containerToggleInit').show();
            $('#containerToggleDespachada').hide();
            $('#containerToggleEntregada').hide();
            $('#containerToggleTodas').hide();
        }
        else if (tab == 2) {
            $('#containerToggleInit').hide();
            $('#containerToggleDespachada').show();
            $('#containerToggleEntregada').hide();
            $('#containerToggleTodas').hide();
        }
        else if (tab == 3) {
            $('#containerToggleInit').hide();
            $('#containerToggleDespachada').hide();
            $('#containerToggleEntregada').show();
            $('#containerToggleTodas').hide();
        }
        else if (tab == 4) {
            $('#containerToggleInit').hide();
            $('#containerToggleDespachada').hide();
            $('#containerToggleEntregada').hide();
            $('#containerToggleTodas').show();
        }
        $('#modalToggleColumns').modal('show');
    })
}

//funcion para actualizar la cantidad a despachar
function updatedespacho() {
    if (cantSelect > 0) {
        $('#numerodespachos').html('(' + cantSelect + ')');
        $('#numerodespachos').removeClass("hidden");
        if (nameTable == "tableIniciada") {
            $('#despachar').removeClass("hidden");
        }
    }
    else {
        $('#numerodespachos').addClass("hidden");;
        $('#despachar').addClass("hidden");;
    }
}

function seleccionarIds() {
    selectedIds = new Array;

    $(".order-select").each(function (i, e) {
        if (e.checked) {
            if (e.value != "all") {
                selectedIds.push($(e).val());
            }
        }
    });

    return selectedIds;
}