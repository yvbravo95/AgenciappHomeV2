
// Build Table
var tableRecibida;
var tableRecibida2;
var tableRecibida3;
const formatDate = "MM/DD/YY";
const allTables = ["Table1", "Table2", "Table3"]
var currentTab = "base_tabTable1";

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

var rechargeTables = () => {
    tableRecibida.draw();
    tableRecibida2.draw();
    tableRecibida3.draw();
}

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
$.fn.dataTable.ext.errMode = 'none'; //No mostrar warning de alerta de dataTable

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
        case "Province":
            tableRecibida.column("contactAddress:name").search(value).draw();
            tableRecibida2.column("contactAddress:name").search(value).draw();
            tableRecibida3.column("contactAddress:name").search(value).draw();
            break;
        default:
            break;
    }
}, 350);

var searchByTable = debounce(function (value, type, table) {
    switch (type) {
        case "Province":
            table === "Table1" && tableRecibida.column("contactAddress:name").search(value).draw();
            table === "Table2" && tableRecibida2.column("contactAddress:name").search(value).draw();
            table === "Table3" && tableRecibida3.column("contactAddress:name").search(value).draw();
            break;
        default:
            break;
    }
}, 350);

const columnDefsDefault = [
    {
        targets: [0],
        data: "id",
        orderable: false,
        searchable: false,
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
            if (data) {
                const init = new moment(item.createdAt);
                const end = new moment();
                var duration = moment.duration(end.diff(init));
                var html = [];
                html.push(moment(data).format(formatDate));
                html.push(`<div><span class="tag tag-default tag-warning">${duration.days()}d ${duration.hours()}h ${duration.minutes()}m</span></div>`);
                return html.join("");
            }
            return "";
        }
    },
    {
        targets: [2],
        data: "number",
        name: "number",
        render: function (data, type, item) {
            var html = [];
            html.push(`<a href="/orders/details/${item.id}">${data}</a>`);
            if (item.dispatchNumberDistributor) {
                html.push(`<div>${item.dispatchNumberDistributor}</div>`);
            }
            if (item.noOrden) {
                html.push(`<p style="color:red">${item.noOrden}</p>`)
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
        data: "contactFullData",
        name: "contactFullData",
        render: function (data, type, item) {
            return `<div title="${item.contactFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">${item.contactFullData}</div>`
        }
    },
    {
        targets: [4],
        data: "contactPhoneNumber",
        name: "contactPhoneNumber",
        render: function (data, type, item) {
            return `<div title="${data}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">${data}</div>`
        }
    },
    {
        targets: [5],
        data: "contactAddress",
        name: "contactAddress",
        render: function (data, type, item) {
            var html = [];
            html.push(`<div title="${data}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">${item.contactAddressProvince}</div>`);
            html.push(`<div title="${data}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">${item.contactAddressMunicipality}</div>`);
            return html.join("");
        }
    },
    {
        targets: [6],
        data: "distributorFullName",
    },
    {
        targets: [7],
        data: "deliveryFullName",
    },
    {
        targets: [8],
        data: "agencyTransferredName",
        render: function (data, type, item) {
            var html = [];
            if (item.agencyTransferredName) {
                html.push(item.agencyTransferredName);
            }
            else {
                html.push(item.agencyName);
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
            html.push(`<a class="dropdown-item print_report" data-orderid="${data}"><i class="fa fa-file-pdf-o"></i>Imprimir Envío</a>`);
            html.push(`<a class="dropdown-item" href="/Orders/Details?id=${data}"><i class="ft-info"></i>Detalles</a>`);
            if (item.problem) {
                html.push(`<a class="dropdown-item confirm" data-orderid="${data}"><i class="fa fa-check"></i>Confirmar</a>`);
            }
            html.push('</div>');
            html.push('</div>');
            return html.join("");
        },
    },

];

const columnDefsDistribuida = [
    {
        targets: [0],
        data: "id",
        orderable: false,
        searchable: false,
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
            if (data) {
                const init = new moment(item.createdAt);
                const end = new moment();
                var duration = moment.duration(end.diff(init));
                var html = [];
                html.push(moment(data).format(formatDate));
                html.push(`<div><span class="tag tag-default tag-warning">${duration.days()}d ${duration.hours()}h ${duration.minutes()}m</span></div>`);
                return html.join("");
            }
            return "";
        }
    },
    {
        targets: [2],
        data: "distributedDate",
        render: function (data, type, item) {
            if (data) {
                var html = [];
                html.push(moment(data).format(formatDate));
                return html.join("");
            }
            return "";
        }
    },
    {
        targets: [3],
        data: "number",
        name: "number",
        render: function (data, type, item) {
            var html = [];
            html.push(`<a href="/orders/details/${item.id}">${data}</a>`);
            html.push(`<div>${item.dispatchNumberDistributor}</div>`);
            if (item.type == "Tienda" && item.noOrden) {
                html.push(`<p style="color:red">${item.noOrden}</p>`)
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
        targets: [4],
        data: "contactFullData",
        name: "contactFullData",
        render: function (data, type, item) {
            return `<div title="${item.contactFullData}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">${item.contactFullData}</div>`
        }
    },
    {
        targets: [5],
        data: "contactPhoneNumber",
        name: "contactPhoneNumber",
        render: function (data, type, item) {
            return `<div title="${data}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">${data}</div>`
        }
    },
    {
        targets: [6],
        data: "contactAddress",
        name: "contactAddress",
        render: function (data, type, item) {
            var html = [];
            html.push(`<div title="${data}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">${item.contactAddressProvince}</div>`);
            html.push(`<div title="${data}" style="margin-top: 5px;width: 120px;padding: 0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">${item.contactAddressMunicipality}</div>`);
            return html.join("");
        }
    },
    {
        targets: [7],
        data: "distributorFullName",
    },
    {
        targets: [8],
        data: "deliveryFullName",
    },
    {
        targets: [9],
        data: "agencyTransferredName",
        render: function (data, type, item) {
            var html = [];
            if (item.agencyTransferredName) {
                html.push(item.agencyTransferredName);
            }
            else {
                html.push(item.agencyName);
            }
            return html.join("");
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
            html.push(`<a class="dropdown-item print_report" data-orderid="${data}"><i class="fa fa-file-pdf-o"></i>Imprimir Envío</a>`);
            html.push(`<a class="dropdown-item" href="/Orders/Details?id=${data}"><i class="ft-info"></i>Detalles</a>`);
            if (item.problem) {
                html.push(`<a class="dropdown-item confirm" data-orderid="${data}"><i class="fa fa-check"></i>Confirmar</a>`);
            }
            html.push('</div>');
            html.push('</div>');
            return html.join("");
        },
    },

];

$(window).on("load", () => {

    var prevTab = $("#" + currentTab);
    var prevContainer = $(prevTab).data("table");

    var block_ele = $("#" + prevContainer + "_div");
    $(block_ele).block(blockOptions);

    for (var i = 0; i < allTables.length; i++) {
        var tableName = allTables[i];
        const baseTabId = `#base_tab${tableName}`;

        switch (tableName) {
            case "Table1":
                var t = $('#Table1');
                tableRecibida = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: true,
                    lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
                    order: [[1, "desc"]],
                    serverSide: true,
                    processing: true,
                    language: tableLanguage,
                    ajax: {
                        "url": `/empleadocuba/GetData?status=Recibida&typeShow=1`,
                        "type": 'POST',
                        "dataType": "json",
                        "dataFilter": function (data) {
                            var json = jQuery.parseJSON(data);
                            $('#base_tabTable1').html(`Recibidas (${json.recordsTotal})`);
                            return JSON.stringify(json); // return JSON string
                        },
                        "statusCode": {
                            401: function () {
                                location.href = "/Account/Login"
                            }
                        }
                    },
                    columnDefs: columnDefsDefault,
                    createdRow: function (row, data, rowIndex) {
                        InitRow(row);
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                        var select =
                            '<select class="ml-2 form-control input-sm" style="width: 180px" name="selec-province" data-type="Table1">' +
                            '<option value="">Todas las provincias</option>' +
                            '<option value="La Habana">La Habana</option>' +
                            '<option value="Pinar Del Rio">Pinar Del Rio</option>' +
                            '<option value="Artemisa">Artemisa</option>' +
                            '<option value="Mayabeque">Mayabeque</option>' +
                            '<option value="Matanzas">Matanzas</option>' +
                            '<option value="Cienfuegos">Cienfuegos</option>' +
                            '<option value="Villa Clara">Villa Clara</option>' +
                            '<option value="Sancti Spíritus">Sancti Spíritus</option>' +
                            '<option value="Ciego de Ávila">Ciego de Ávila</option>' +
                            '<option value="Las Tunas">Las Tunas</option>' +
                            '<option value="Granma">Granma</option>' +
                            '<option value="Holguín">Holguín</option>' +
                            '<option value="Santiago de Cuba">Santiago de Cuba</option>' +
                            '<option value="Guantánamo">Guantánamo</option>' +
                            '<option value="Isla de la Juventud">Isla de la Juventud</option>' +
                            '</select>';

                        $(`#Table1_length`).append(select)
                    }
                });

                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableRecibida));
                })
                break;
            case "Table2":
                var t = $('#Table2');
                tableRecibida2 = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: true,
                    lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
                    order: [[2, "desc"]],
                    serverSide: true,
                    processing: false,
                    language: tableLanguage,
                    ajax: {
                        "url": `/empleadocuba/GetData?status=Recibida&typeShow=2`,
                        "type": 'POST',
                        "dataType": "json",
                        "dataFilter": function (data) {
                            var json = jQuery.parseJSON(data);
                            $('#base_tabTable2').html(`Distribuidas (${json.recordsTotal})`);
                            return JSON.stringify(json); // return JSON string
                        },
                        "statusCode": {
                            401: function () {
                                location.href = "/Account/Login"
                            }
                        }
                    },
                    columnDefs: columnDefsDistribuida,
                    createdRow: function (row, data, rowIndex) {
                        InitRow(row);
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                        var select =
                            '<select class="ml-2 form-control input-sm" style="width: 180px" name="selec-province" data-type="Table2">' +
                            '<option value="">Todas las provincias</option>' +
                            '<option value="La Habana">La Habana</option>' +
                            '<option value="Pinar Del Rio">Pinar Del Rio</option>' +
                            '<option value="Artemisa">Artemisa</option>' +
                            '<option value="Mayabeque">Mayabeque</option>' +
                            '<option value="Matanzas">Matanzas</option>' +
                            '<option value="Cienfuegos">Cienfuegos</option>' +
                            '<option value="Villa Clara">Villa Clara</option>' +
                            '<option value="Sancti Spíritus">Sancti Spíritus</option>' +
                            '<option value="Ciego de Ávila">Ciego de Ávila</option>' +
                            '<option value="Las Tunas">Las Tunas</option>' +
                            '<option value="Granma">Granma</option>' +
                            '<option value="Holguín">Holguín</option>' +
                            '<option value="Santiago de Cuba">Santiago de Cuba</option>' +
                            '<option value="Guantánamo">Guantánamo</option>' +
                            '<option value="Isla de la Juventud">Isla de la Juventud</option>' +
                            '</select>';

                        $(`#Table2_length`).append(select)
                    }
                });

                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableRecibida2));
                })
                break;
            case "Table3":
                var t = $('#Table3');
                tableRecibida3 = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: true,
                    lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
                    order: [[1, "desc"]],
                    serverSide: true,
                    processing: false,
                    language: tableLanguage,
                    ajax: {
                        "url": `/empleadocuba/GetData?status=Recibida&typeShow=3`,
                        "type": 'POST',
                        "dataType": "json",
                        "dataFilter": function (data) {
                            var json = jQuery.parseJSON(data);
                            $('#base_tabTable3').html(`Repartidas (${json.recordsTotal})`);
                            return JSON.stringify(json); // return JSON string
                        },
                        "statusCode": {
                            401: function () {
                                location.href = "/Account/Login"
                            }
                        }
                    },
                    columnDefs: columnDefsDefault,
                    createdRow: function (row, data, rowIndex) {
                        InitRow(row);
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                        var select =
                            '<select class="ml-2 form-control input-sm" style="width: 180px" name="selec-province data-type="Table3">' +
                            '<option value="">Todas las provincias</option>' +
                            '<option value="La Habana">La Habana</option>' +
                            '<option value="Pinar Del Rio">Pinar Del Rio</option>' +
                            '<option value="Artemisa">Artemisa</option>' +
                            '<option value="Mayabeque">Mayabeque</option>' +
                            '<option value="Matanzas">Matanzas</option>' +
                            '<option value="Cienfuegos">Cienfuegos</option>' +
                            '<option value="Villa Clara">Villa Clara</option>' +
                            '<option value="Sancti Spíritus">Sancti Spíritus</option>' +
                            '<option value="Ciego de Ávila">Ciego de Ávila</option>' +
                            '<option value="Las Tunas">Las Tunas</option>' +
                            '<option value="Granma">Granma</option>' +
                            '<option value="Holguín">Holguín</option>' +
                            '<option value="Santiago de Cuba">Santiago de Cuba</option>' +
                            '<option value="Guantánamo">Guantánamo</option>' +
                            '<option value="Isla de la Juventud">Isla de la Juventud</option>' +
                            '</select>';

                        $(`#Table3_length`).append(select)
                    }
                });

                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableRecibida3));
                })
                break;
            default:
                break;
        }

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

    LoadEvents();
})

function InitRow(row) {
    $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); });

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
}

function LoadEvents() {
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

    $(document).on('change', '[name="selec-province"]', function () {
        searchByTable($(this).val(), "Province", $(this).data("type"));
    })

    var tableName = "";
    $("#btnDespachar").on('click', function () {
        var tabActive = $('[class="nav-link tablePartial active"]');
        tableName = $(tabActive).data('table');
        if (tableName === "Table3") {
            $('#divrepartidor').removeClass('hidden')
            $('#divdistribuidor').addClass('hidden')
        }
        else {
            $('#divrepartidor').addClass('hidden')
            $('#divdistribuidor').removeClass('hidden')
        }

        $('#modalDespacho').modal('show');
    });

    $('#btnModalDespacho').on('click', function () {
        var distributor = $('#selectDistributor').val();
        if (tableName === "Table3") {
            distributor = $('#selectRepartidor').val();
        }

        if (!distributor) {
            toastr.error("Debe seleccionar un distribuidor");
            return false;
        }

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
                    toastr.success(response.msg);
                    rechargeTables();
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

    $('#exportExcelAccept').attr('href', '/OrderNew/ExportExcel/?date=' + $('#daterange').val());

    $('#daterange').on('change', function () {
        $('#exportExcelAccept').attr('href', '/OrderNew/ExportExcel/?date=' + $('#daterange').val())
    });

    $('#exportExcelAccept').on('click', function () {
        $('#modalExport').modal('hide');
    })

    $('#exportExcel').on('click', function () {
        $('#modalExport').modal('show');
    });

    $("#exportReporteAccept").click(function () {
        var date = $('#daterangeReport').val();
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/OrderNew/ExportReporte",
            data: {
                date: date
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

    $('#gen_reportFecha').on('click', function () {
        $('#modalReporte').modal('show');
    });
    $('#exportReporteAccept').on('click', function () {
        $('#modalExport').modal('hide');
    })
}


$('.nav-link').on('click', function () {
    $('.nav-link').removeClass('active')
    $(this).addClass('active')

    $(".order-select").prop("checked", false).trigger('change');
    $('.select-all').prop("checked", false).trigger('change');
})

$('.select-all').on('change', function () {
    var paneActive = $('[class="tab-pane active"]');
    var table = paneActive.find("table");

    if ($(this)[0].checked) {
        table.find(".order-select").prop("checked", true).trigger('change');
    }
    else {
        table.find(".order-select").prop("checked", false).trigger('change');
    }

});