// Build Table
var tableDespachada;
const formatDate = "MM/DD/YY";

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

$(window).on("load", () => {

    var divContainer = $('#tblContainer');
    $(divContainer).block(blockOptions);

    var t = $('#tableDespachada');
    tableDespachada = t.DataTable({
        searching: true,
        dom: "ltip",
        lengthChange: false,
        order: [[1, "desc"]],
        serverSide: true,
        processing: false,
        language: tableLanguage,
        ajax: {
            "url": `/empleadocuba/GetData?status=Recibiendo`,
            "type": 'POST',
            "dataType": "json",
            "dataFilter": function (data) {
                var json = jQuery.parseJSON(data);
                $('#base_tabDespachada').html(`Despachadas (${json.recordsTotal})`);
                return JSON.stringify(json); // return JSON string
            },
            "statusCode": {
                401: function () {
                    location.href = "/Account/Login"
                }
            }
        },
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
                    html.push(`<a class="dropdown-item confirmToReceived" data-orderid="${data}"><i class="fa fa-check"></i>Confirmar a Recibida</a>`);
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
            $(divContainer).unblock();
        }
    });

    $.each($(".searchColumn", t), (i, e) => {
        $(e).on('keyup', (e) => searchColumn(e, tableDespachada));
    })

    $(t).on('processing.dt', function (e, settings, processing) {
        if (processing) {
            $(divContainer).block(blockOptions);
        }
        else {
            $(divContainer).unblock();
        }
    })

    LoadEvents();
})


function InitRow(row) {
    $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); });
    $.each($('.status', row), (i, e) => {
        $(e).editable({
            source: function (p) {
                s = [
                    {
                        "1": 'Iniciada',
                        "2": 'Pendiente',
                        "3": 'Cancelada',
                        "4": 'Completada'
                    },
                    {
                        "1": 'Iniciada',
                        "2": 'Pendiente',
                        "3": 'Cancelada',
                        "4": 'Completada'
                    },
                    {
                        "3": 'Cancelada',
                    },
                    {//Estado Completada
                        "3": 'Cancelada',
                        "4": 'Completada',
                        "5": "Enviada"
                    },
                    {//Estado Enviada
                        "3": 'Cancelada',
                        "5": "Enviada",
                        "6": "Revisada"
                    },
                    {//Estado Revisada
                        "3": 'Cancelada',
                        "6": 'Revisada',
                        "7": "Entregada"
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
                url: 'OrderNew/index',
                type: 'post',
                dataType: 'json'
            },
            params: function (params) {
                params.id = $(this).data("id");
                return params;
            },
            success: function (response, newValue) {
                location.reload();
                //showOKMessage("Cambio de estatus", "Se ha cambiado satifactoriamente el estatus");
            },
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
                        location.reload();
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

    $.each($('.confirmToReceived', row), (i, e) => {
        $(e).click(function () {
            var orderid = $(this).data("orderid")
            $.ajax({
                async: true,
                type: "GET",
                contentType: "application/x-www-form-urlencoded",
                url: "/ordernew/ConfirmToReceivedOrder?orderid=" + orderid,
                beforeSend: function () {
                    $.blockUI();
                },
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        location.reload();
                    }
                    else
                        toastr.error(response.message);

                    $.unblockUI();
                },
                error: function () {
                    toastr.error("Ha ocurrido un error", "Error");
                    $.unblockUI();
                },
                timeout: 60000,
            });
        });
    });
}

function LoadEvents()
{
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

    $("#btnDespachar").on('click', function () {
        $('#modalDespacho').modal('show');
    });
    $('#btnModalDespacho').on('click', function () {
        var distributor = $('#selectDistributor').val();

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
                    location.href = "/EmpleadoCuba/OrdenesDespachadas?msg=" + response.msg;
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
}
