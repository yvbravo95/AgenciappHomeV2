//variables
var tab = 1;
var nameTable = "tableInit";
var tabName = "";
var guid_empty = "00000000-0000-0000-0000-000000000000";
var cantSelect = 0;
var cantSelectEnviados = 0;
var tableIniciado;
var tableEnviado;
var tableRecibida;
var tableConfirmado;
var tableCancelado;
var bags = null;
var idPackageReview = null;
var btnReview = null;
var idBag = null;

//funciones
function changeStatusBag(status, idBag, nota) {
    $('#cointainerNotas').html("");
    if (bags != null) {
        for (var i = 0; i < bags.length; i++) {
            var bag = bags[i];
            if (bag.bagId == idBag) {
                bag.isComplete = status;
                bag.comment = nota;
            }
            if (!bag.isComplete) {
                $('#cointainerNotas').append(
                    '<p>- <b>' + bag.bagNumber + ':</b> ' + bag.comment + ' </p> \n'
                );

            }
        }
    }
}

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

var searchEquipaje = debounce(function (e) {
    tableIniciado.column("number:name").search($(e.target).val()).draw();
    tableEnviado.column("number:name").search($(e.target).val()).draw();
    tableRecibida.column("number:name").search($(e.target).val()).draw();
    tableConfirmado.column("number:name").search($(e.target).val()).draw();
    tableCancelado.column("number:name").search($(e.target).val()).draw();
}, 350);

var searchBolsa = debounce(function (e) {
    tableIniciado.column("bags:name").search($(e.target).val()).draw();
    tableEnviado.column("bags:name").search($(e.target).val()).draw();
    tableRecibida.column("bags:name").search($(e.target).val()).draw();
    tableConfirmado.column("bags:name").search($(e.target).val()).draw();
    tableCancelado.column("bags:name").search($(e.target).val()).draw();
}, 350);

var searchPaquete = debounce(function (e) {
    tableIniciado.column("packageNumbers:name").search($(e.target).val()).draw();
    tableEnviado.column("packageNumbers:name").search($(e.target).val()).draw();
    tableRecibida.column("packageNumbers:name").search($(e.target).val()).draw();
    tableConfirmado.column("packageNumbers:name").search($(e.target).val()).draw();
    tableCancelado.column("packageNumbers:name").search($(e.target).val()).draw();
}, 350);

var searchProducto = debounce(function (e) {
    tableIniciado.column("product:name").search($(e.target).val()).draw();
    tableEnviado.column("product:name").search($(e.target).val()).draw();
    tableRecibida.column("product:name").search($(e.target).val()).draw();
    tableConfirmado.column("product:name").search($(e.target).val()).draw();
    tableCancelado.column("product:name").search($(e.target).val()).draw();
}, 350);

var allTables = ["Iniciado", "Enviado", "Confirmado", "Recibida", "Cancelado"]

$.fn.editable.defaults.mode = 'inline';

$(window).on("load", () => {
    var prevTab = $("#" + tabActive);
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
            case "Iniciado":
                var t = $('#tableShippings');
                tableIniciado = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: false,
                    order: [[2, "desc"]],
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
                        "url": "/Shippings/List?status=Iniciado",
                        "type": 'POST',
                        "dataType": "json"
                    },
                    columnDefs: [
                        {
                            targets: [0],
                            data: "product",
                            name: "product",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<label class="custom-control custom-checkbox">`);
                                html.push(`     <input type="checkbox" class="custom-control-input order-select" value="${item.packingId}" />`);
                                html.push(`     <span class="custom-control-indicator"></span>`);
                                html.push(`     <span class="custom-control-description"></span>`);
                                html.push(`</label>`);
                                return html.join("");
                            }
                        },
                        {
                            targets: [1],
                            name: "number",
                            data: "number",
                            render: function (data, type, item) {                             
                                var html = [];
                                html.push(`<a href="/Shippings/Details/${item.packingId}">${data}</a>`)
                                if (role == "PrincipalDistributor") {
                                    html.push(`<div style="display: block !important;" class="tag tag-info">${item.agencyName}</div>`)
                                }
                                else if (item.principalDistributorFullName) {
                                    html.push(`<div style="display: block !important;" class="tag tag-info">${item.principalDistributorFullName}</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [2],
                            data: "date",
                            name: "date",
                        },
                        {
                            targets: [3],
                            data: "status",
                            name: "status",
                            render: function (data, type, item) {
                                if (role != "EmpleadoCuba") {
                                    return `<a href="#" class="status" data-id="${item.packingId}" data-type="select" data-value="1" data-pk="1" data-url="/post" data-title="Select status"></a>`;
                                }
                                else {
                                    return `${data}`;
                                }
                            },
                        },
                        {
                            targets: [4],
                            data: "bags",
                            name: "bags",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    var bag = data[i].number;
                                    html.push(`<div>${bag}</div>`);
                                }  
                                return html.join("");
                            }

                        },
                        {
                            targets: [5],
                            data: "packageNumbers",
                            name: "packageNumbers",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < item.provinces.length; i++) {
                                    var pk = item.provinces[i];
                                    html.push(`<div>${pk.orderNumber} (${pk.contactProvince})</div>`);
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [6],
                            data: "packingId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(`     <i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(`     <div class="dropdown-menu" aria-label="dropdown${item.number}">`)
                                html.push(`         <a class="dropdown-item" href="/Shippings/SetCarrier/${item.packingId}"><i class="fa fa-street-view"></i>Enviar Equipaje</a>`)
                                html.push(`         <a class="dropdown-item" href="/Shippings/ViewQR/${item.packingId}"><i class="fa fa-qrcode"></i>Ver QR</a>`)
                                html.push(`         <a class="dropdown-item print_report" data-shippingidid="${item.packingId}"><i class="fa fa-file-pdf-o"></i>Imprimir</a>`)
                                html.push(`         <a class="dropdown-item" href="/Shippings/Details/${item.packingId}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`         <a class="dropdown-item" href="/Shippings/Edit/${item.packingId}"><i class="ft-edit"></i>Editar</a>`)
                                html.push(`         <a class="dropdown-item" name="entregar_ordenes" href="#" data-id="${item.packingId}"><i class="ft-check"></i>Entregar Paquetes</a>`)
                                if (role != "EmpleadoCuba") {
                                    html.push(`     <a class="dropdown-item cancelShipp" data-number="${item.number}" data-id="${item.packingId}"><i class="ft-delete"></i>Cancelar</a>`)
                                }
                                html.push('     </div>')
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
                                            "1": 'Iniciada',
                                            "2": 'Cancelada'
                                        },
                                    ];
                                    return s[$(this).data('value') - 1];
                                },
                                display: function (value, sourceData) {
                                    var colors = { "": "gray", 1: "#ffb019", 2: "red", 3: "red", 4: "green" },
                                        elem = $.grep(sourceData, function (o) { return o.value == value; });

                                    if (elem.length) {
                                        $(this).text(elem[0].text).css("color", colors[value]);
                                    } else {
                                        $(this).empty();
                                    }

                                },
                                ajaxOptions: {
                                    url: '/Shippings/Index',
                                    type: 'post',
                                    dataType: 'json'
                                },
                                params: function (params) {
                                    params.id = $(this).data("id");
                                    return params;
                                },
                                success: function (response, newValue) {
                                    location.reload();
                                },
                            });
                        })
                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($(".print_report", row), (i, e) => {
                            $(e).on('click', function () {
                                var shippingid = $(this).data("shippingidid")
                                $.ajax({
                                    async: true,
                                    type: "POST",
                                    contentType: "application/x-www-form-urlencoded",
                                    url: "/Shippings/createFileShipping",
                                    data: {
                                        id: shippingid
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
                        $.each($('[name="entregar_ordenes"]', row), (i, e) => {
                            $(e).on('click', function () {
                                $.ajax({
                                    method: "GET",
                                    url: "/Shippings/EntregarPaquetes?packingId=" + $(this).attr("data-id"),
                                    success: function (data) {
                                        document.location = "/Shippings?msg=Los tramites han sido actualizados"
                                    },
                                    error: function (e) {
                                        toastr.error("Los tramites no han podido ser actualizados")
                                    }
                                });
                            })

                        })
                        if (role != "EmpleadoCuba") {
                            $.each($(".cancelShipp", row), (i, e) => {
                                $(e).click(function () {
                                    var Id = $(this).data("id");
                                    var Number = $(this).data("number");
                                    var okConfirm = function () {
                                        var urlDelete = "/Shippings/Cancel/" + Id;
                                        $.ajax({
                                            type: "POST",
                                            url: urlDelete,
                                            async: false,
                                            success: function () {
                                                document.location = "/Shippings?msg=Cancel&noEquipaje=" + Number;
                                            }
                                        });
                                    };
                                    getCancelConfirmation(okConfirm);
                                });
                            })
                        }    
                        $.each($(".order-select", row), (i, e) => {
                            $(e).on("change", function () {
                                if ($(this)[0].checked) {
                                    cantSelect++;
                                } else {
                                    cantSelect--;
                                }
                                if (cantSelect == 0) {
                                    $('#sendShippings').hide();
                                } else {
                                    if (tabActive.toLowerCase() == 'Iniciados'.toLowerCase()) {
                                        $('#sendShippings').show();
                                    }
                                }
                            });
                        })                      
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                        $("#base_tabIniciado").html(`Iniciados (${json.recordsTotal})`);
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableIniciado));
                })               
                break;
            case "Enviado":
                var t = $('#tableShippings1');
                tableEnviado = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: false,
                    order: [[3, "desc"]],
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
                        "url": "/Shippings/List?status=Enviado",
                        "type": 'POST',
                        "dataType": "json"
                    },
                    columnDefs: [
                        {
                            targets: [0],
                            data: "product",
                            name: "product",
                            visible: false
                        },
                        {
                            targets: [1],
                            data: "packingId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<label class="custom-control custom-checkbox">`);
                                html.push(`     <input type="checkbox" class="custom-control-input order-select" value="${data}" />`);
                                html.push(`     <span class="custom-control-indicator"></span>`);
                                html.push(`     <span class="custom-control-description"></span>`);
                                html.push(`</label>`);
                                return html.join("");
                            },
                        },
                        {
                            targets: [2],
                            data: "number",
                            name: "number",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a href="/Shippings/Details/${item.packingId}">${data}</a>`)
                                if (role == "PrincipalDistributor") {
                                    html.push(`<div style="display: block !important;" class="tag tag-info">${item.agencyName}</div>`)
                                }
                                else if (item.principalDistributorFullName) {
                                    html.push(`<div style="display: block !important;" class="tag tag-info">${item.principalDistributorFullName}</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [3],
                            data: "date",
                            name: "date"
                        },
                        {
                            targets: [4],
                            data: "shippingDate",
                            name: "shippingDate",
                        },
                        {
                            targets: [5],
                            data: "carrierName",
                            name: "carrierName"
                        },
                        {
                            targets: [6],
                            data: "dateSend",
                            name: "dateSend"
                        },
                        {
                            targets: [7],
                            data: "numeroVuelo",
                            name: "numeroVuelo"
                        },
                        {
                            targets: [8],
                            data: "bags",
                            name: "bags",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    var bag = data[i].number;
                                    html.push(`<div>${bag}</div>`);
                                }
                                return html.join("");
                            }

                        },
                        {
                            targets: [9],
                            data: "packageNumbers",
                            name: "packageNumbers",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    var pk = data[i];
                                    html.push(`<div>${pk}</div>`);
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [10],
                            data: "packingId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(`     <i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(`     <div class="dropdown-menu" aria-label="dropdown${item.number}">`)
                                html.push(`         <a class="dropdown-item check" data-id="${item.packingId}"><i class="ft-check"></i>Confirmar</a>`)
                                html.push(`         <a class="dropdown-item" href="/Shippings/ViewQR/${item.packingId}"><i class="fa fa-qrcode"></i>Ver QR</a>`)
                                html.push(`         <a class="dropdown-item print_report" data-shippingidid="${item.packingId}"><i class="fa fa-file-pdf-o"></i>Imprimir</a>`)
                                html.push(`         <a class="dropdown-item" href="/Shippings/Details/${item.packingId}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`         <a class="dropdown-item" href="/Shippings/Edit/${item.packingId}"><i class="ft-edit"></i>Editar</a>`)
                                html.push(`         <a class="dropdown-item" name="entregar_ordenes" href="#" data-id="${item.packingId}"><i class="ft-check"></i>Entregar Paquetes</a>`)
                                html.push('     </div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },
                    ],
                    createdRow: function (row, data, rowIndex) {
                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($(".print_report", row), (i, e) => {
                            $(e).on('click', function () {
                                var shippingid = $(this).data("shippingidid")
                                $.ajax({
                                    async: true,
                                    type: "POST",
                                    contentType: "application/x-www-form-urlencoded",
                                    url: "/Shippings/createFileShipping",
                                    data: {
                                        id: shippingid
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
                        $.each($('[name="entregar_ordenes"]', row), (i, e) => {
                            $(e).on('click', function () {
                                $.ajax({
                                    method: "GET",
                                    url: "/Shippings/EntregarPaquetes?packingId=" + $(this).attr("data-id"),
                                    success: function (data) {
                                        document.location = "/Shippings?msg=Los tramites han sido actualizados"
                                    },
                                    error: function (e) {
                                        toastr.error("Los tramites no han podido ser actualizados")
                                    }
                                });
                            })

                        })
                        $.each($(".check", row), (i, e) => {
                            $(e).on("click", function () {
                                $.ajax({
                                    method: "POST",
                                    url: "/Shippings/Confirmar/" + $(this).data("id"),
                                    success: function (data) {
                                        if (data.success)
                                            document.location = "/Shippings?msg=Confirmado&noEquipaje=" + data.noequipaje;
                                        else
                                            showErrorMessage("ERROR", "No se pudo confirmar el equipaje");
                                    }
                                });
                            });
                        })
                        $.each($(".order-select", row), (i, e) => {
                            $(e).on("change", function () {
                                if ($(this)[0].checked) {
                                    cantSelectEnviados++;
                                } else {
                                    cantSelectEnviados--;
                                }
                                if (cantSelectEnviados == 0) {
                                    $('#btnStatusRecibido').hide();
                                    $('#despachar').hide();
                                } else {
                                    if (tabActive == 'enviados') {
                                        $('#btnStatusRecibido').show();
                                        $('#despachar').show();
                                    }
                                }
                            });
                        });

                        $('#btnStatusRecibido').on('click', function () {
                            var url = '/Shippings/checkequipaje?';
                            var items = new Array;
                            var count = 0;
                            $.each($(".order-select", "#tableShippings1"), (i, e) => {
                                if (e.checked) {
                                    const value = $(e).val();
                                    items.push(`ids[${count++}]=${value}`);
                                }
                            });
                            location.href = url + items.join("&");
                        });
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                        $("#base_tabEnviado").html(`Enviados (${json.recordsTotal})`);
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableEnviado));
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
                        "url": "/Shippings/List?status=Recibido",
                        "type": 'POST',
                        "dataType": "json"
                    },
                    columnDefs: [
                        {
                            targets: [0],
                            data: "product",
                            name: "product",
                            visible: false
                        },
                        {
                            targets: [1],
                            data: "number",
                            name: "number",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a href="/Shippings/Details/${item.packingId}">${data}</a>`)
                                if (role == "PrincipalDistributor") {
                                    html.push(`<div style="display: block !important;" class="tag tag-info">${item.agencyName}</div>`)
                                }
                                else if (item.principalDistributorFullName) {
                                    html.push(`<div style="display: block !important;" class="tag tag-info">${item.principalDistributorFullName}</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [2],
                            data: "date",
                            name: "date"
                        },
                        {
                            targets: [3],
                            data: "shippingDate",
                            name: "shippingDate",
                        },
                        {
                            targets: [4],
                            data: "carrierName",
                            name: "carrierName"
                        },
                        {
                            targets: [5],
                            data: "dateSend",
                            name: "dateSend"
                        },
                        {
                            targets: [6],
                            data: "numeroVuelo",
                            name: "numeroVuelo"
                        },
                        {
                            targets: [7],
                            data: "bags",
                            name: "bags",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    var bag = data[i].number;
                                    html.push(`<div>${bag}</div>`);
                                }
                                return html.join("");
                            }

                        },
                        {
                            targets: [8],
                            data: "packageNumbers",
                            name: "packageNumbers",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    var pk = data[i];
                                    html.push(`<div>${pk}</div>`);
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [9],
                            data: "packingId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(`     <i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(`     <div class="dropdown-menu" aria-label="dropdown${item.number}">`)
                                if (role != "PrincipalDistributor") {
                                    html.push(`         <a class="dropdown-item check" data-id="${item.packingId}"><i class="ft-check"></i>Confirmar</a>`)
                                }
                                html.push(`         <a class="dropdown-item" href="/Shippings/ViewQR/${item.packingId}"><i class="fa fa-qrcode"></i>Ver QR</a>`)
                                html.push(`         <a class="dropdown-item print_report" data-shippingidid="${item.packingId}"><i class="fa fa-file-pdf-o"></i>Imprimir</a>`)
                                html.push(`         <a class="dropdown-item" href="/Shippings/Details/${item.packingId}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`         <a class="dropdown-item" name="entregar_ordenes" href="#" data-id="${item.packingId}"><i class="ft-check"></i>Entregar Paquetes</a>`)
                                html.push('     </div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },
                    ],
                    createdRow: function (row, data, rowIndex) {
                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($(".print_report", row), (i, e) => {
                            $(e).on('click', function () {
                                var shippingid = $(this).data("shippingidid")
                                $.ajax({
                                    async: true,
                                    type: "POST",
                                    contentType: "application/x-www-form-urlencoded",
                                    url: "/Shippings/createFileShipping",
                                    data: {
                                        id: shippingid
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
                        $.each($('[name="entregar_ordenes"]', row), (i, e) => {
                            $(e).on('click', function () {
                                $.ajax({
                                    method: "GET",
                                    url: "/Shippings/EntregarPaquetes?packingId=" + $(this).attr("data-id"),
                                    success: function (data) {
                                        document.location = "/Shippings?msg=Los tramites han sido actualizados"
                                    },
                                    error: function (e) {
                                        toastr.error("Los tramites no han podido ser actualizados")
                                    }
                                });
                            })

                        })
                        $.each($(".check", row), (i, e) => {
                            $(e).on("click", function () {
                                $.ajax({
                                    method: "POST",
                                    url: "/Shippings/Confirmar/" + $(this).data("id"),
                                    success: function (data) {
                                        if (data.success)
                                            document.location = "/Shippings?msg=Confirmado&noEquipaje=" + data.noequipaje;
                                        else
                                            showErrorMessage("ERROR", "No se pudo confirmar el equipaje");
                                    }
                                });
                            });
                        })
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                        $("#base_tabRecibida").html(`Recibidos (${json.recordsTotal})`);
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableRecibida));
                })
                break;
            case "Confirmado":
                var t = $('#tableShippings2');
                tableConfirmado = t.DataTable({
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
                        "url": "/Shippings/List?status=Confirmado",
                        "type": 'POST',
                        "dataType": "json"
                    },
                    columnDefs: [
                        {
                            targets: [0],
                            data: "product",
                            name: "product",
                            visible: false
                        },
                        {
                            targets: [1],
                            data: "number",
                            name: "number",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a href="/Shippings/Details/${item.packingId}">${data}</a>`)
                                if (role == "PrincipalDistributor") {
                                    html.push(`<div style="display: block !important;" class="tag tag-info">${item.agencyName}</div>`)
                                }
                                else if (item.principalDistributorFullName) {
                                    html.push(`<div style="display: block !important;" class="tag tag-info">${item.principalDistributorFullName}</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [2],
                            data: "date",
                            name: "date"
                        },
                        {
                            targets: [3],
                            data: "carrierName",
                            name: "carrierName"
                        },
                        {
                            targets: [4],
                            data: "dateSend",
                            name: "dateSend"
                        },
                        {
                            targets: [5],
                            data: "numeroVuelo",
                            name: "numeroVuelo"
                        },
                        {
                            targets: [6],
                            data: "bags",
                            name: "bags",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    var bag = data[i].number;
                                    html.push(`<div>${bag}</div>`);
                                }
                                return html.join("");
                            }

                        },
                        {
                            targets: [7],
                            data: "packageNumbers",
                            name: "packageNumbers",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    var pk = data[i];
                                    html.push(`<div>${pk}</div>`);
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [8],
                            data: "packingId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(`     <i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(`     <div class="dropdown-menu" aria-label="dropdown${item.number}">`)
                                if (!item.isReview) {
                                    html.push(` <a class="dropdown-item" href="#" name="review" data-id="${item.packingId}"><i class="fa fa-check"></i>Revisar</a>`)
                                }
                                html.push(`         <a class="dropdown-item" href="/Shippings/ViewQR/${item.packingId}"><i class="fa fa-qrcode"></i>Ver QR</a>`)
                                html.push(`         <a class="dropdown-item print_report" data-shippingidid="${item.packingId}"><i class="fa fa-file-pdf-o"></i>Imprimir</a>`)
                                html.push(`         <a class="dropdown-item" href="/Shippings/Details/${item.packingId}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`         <a class="dropdown-item" name="entregar_ordenes" href="#" data-id="${item.packingId}"><i class="ft-check"></i>Entregar Paquetes</a>`)
                                html.push('     </div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },
                    ],
                    rowCallback: function (row, data, index) {
                        if (data["isReview"]) {
                            $(row).find('td').css('background-color', '#ffff0040');
                        }
                    },
                    createdRow: function (row, data, rowIndex) {
                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($(".print_report", row), (i, e) => {
                            $(e).on('click', function () {
                                var shippingid = $(this).data("shippingidid")
                                $.ajax({
                                    async: true,
                                    type: "POST",
                                    contentType: "application/x-www-form-urlencoded",
                                    url: "/Shippings/createFileShipping",
                                    data: {
                                        id: shippingid
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
                        $.each($('[name="entregar_ordenes"]', row), (i, e) => {
                            $(e).on('click', function () {
                                $.ajax({
                                    method: "GET",
                                    url: "/Shippings/EntregarPaquetes?packingId=" + $(this).attr("data-id"),
                                    success: function (data) {
                                        document.location = "/Shippings?msg=Los tramites han sido actualizados"
                                    },
                                    error: function (e) {
                                        toastr.error("Los tramites no han podido ser actualizados")
                                    }
                                });
                            })

                        })
                        $.each($('[name="review"]', row), (i, e) => {
                            $(e).on('click', function (e) {
                                btnReview = $(e.target);
                                idPackageReview = $(e.target).data('id');
                                $.ajax({
                                    type: "Post",
                                    url: "/Shippings/GetBagReview",
                                    data: {
                                        Id: idPackageReview
                                    },
                                    beforeSend: function () {
                                        $('#containerBag').html("");
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
                                        if (response.success) {
                                            bags = response.data;
                                            for (var i = 0; i < bags.length; i++) {
                                                $('#containerBag').append(
                                                    '<div class="col-md-6">' +
                                                    '<div class="float-xs-left" style="margin-right:10px;">' +
                                                    '<input checked type="checkbox" class="switch" name="checkComplete" data-id="' + bags[i].bagId + '" data-group-cls="btn-group-sm" data-off-label="No" data-on-label="Sí" />' +
                                                    '</div>' +
                                                    '<h5 style="font-size:20px;">' + bags[i].bagNumber + '</h5>' +
                                                    '</div>'
                                                );
                                            }
                                            $('[name="checkComplete"]').checkboxpicker();
                                            $('#ReviewPackage').modal('show');
                                        }
                                        else {
                                            toastr.error(response.msg);
                                            idPackageReview = null;
                                        }
                                        $.unblockUI();
                                    },
                                    error: function (response) {
                                        response.error("No se han podido obtener las bolsas");
                                        $.unblockUI();
                                    }
                                });
                            });
                        })
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                        $("#base_tabConfirmado").html(`Confirmado (${json.recordsTotal})`);
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableConfirmado));
                })    
                break;
            case "Cancelado":
                var t = $('#tableShippings3');
                tableCancelado = t.DataTable({
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
                        "url": "/Shippings/List?status=Cancelado",
                        "type": 'POST',
                        "dataType": "json"
                    },
                    columnDefs: [
                        {
                            targets: [0],
                            data: "product",
                            name: "product",
                            visible: false
                        },
                        {
                            targets: [1],
                            data: "number",
                            name: "number",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a href="/Shippings/Details/${item.packingId}">${data}</a>`)
                                if (role == "PrincipalDistributor") {
                                    html.push(`<div style="display: block !important;" class="tag tag-info">${item.agencyName}</div>`)
                                }
                                else if (item.principalDistributorFullName) {
                                    html.push(`<div style="display: block !important;" class="tag tag-info">${item.principalDistributorFullName}</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [2],
                            data: "date",
                            name: "date"
                        },
                        {
                            targets: [3],
                            render: function (data, type, item) {
                                return `<label style="color:red;">Cancelada</label>`;
                            }
                        },
                        {
                            targets: [4],
                            data: "bags",
                            name: "bags",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    var bag = data[i];
                                    html.push(`<div>${bag}</div>`);
                                }
                                return html.join("");
                            }

                        },
                        {
                            targets: [5],
                            data: "packageNumbers",
                            name: "packageNumbers",
                            render: function (data, type, item) {
                                var html = [];
                                for (var i = 0; i < data.length; i++) {
                                    var pk = data[i];
                                    html.push(`<div>${pk}</div>`);
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [6],
                            data: "packingId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(`     <i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(`     <div class="dropdown-menu" aria-label="dropdown${item.number}">`)
                                html.push(`         <a class="dropdown-item" href="/Shippings/ViewQR/${item.packingId}"><i class="fa fa-qrcode"></i>Ver QR</a>`)
                                html.push(`         <a class="dropdown-item print_report" data-shippingidid="${item.packingId}"><i class="fa fa-file-pdf-o"></i>Imprimir</a>`)
                                html.push(`         <a class="dropdown-item" href="/Shippings/Details/${item.packingId}"><i class="ft-info"></i>Detalles</a>`)
                                html.push('</div>')
                                return html.join("");
                            }
                        },
                    ],
                    createdRow: function (row, data, rowIndex) {
                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($(".print_report", row), (i, e) => {
                            $(e).on('click', function () {
                                var shippingid = $(this).data("shippingidid")
                                $.ajax({
                                    async: true,
                                    type: "POST",
                                    contentType: "application/x-www-form-urlencoded",
                                    url: "/Shippings/createFileShipping",
                                    data: {
                                        id: shippingid
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
                        $("#base_tabCancelado").html(`Cancelado (${json.recordsTotal})`);
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableCancelado));
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
    };

    $(block_ele).unblock();
})

$(document).ready(function () {
    var url = decodeURIComponent(window.location);
    var params = url.split("?")[1];
    if (params != null) {
        params = params.split("&");
        var msg = params[0].split("=")[1];
        if (params.length > 1) {
            var noEquipaje = params[1].split("=")[1];
            if (msg == "success") {
                showOKMessage("Nuevo Equipaje", "Equipaje " + noEquipaje + " se ha creado con éxito", { "timeOut": 0, "closeButton": true });
            } else if (msg == "successCarrier") {
                showOKMessage("Asignar Carrier", "Al equipaje " + noEquipaje + " se le ha asignado el carrier con éxito", { "timeOut": 0, "closeButton": true });
            }
            else if (msg == "Confirmado") {
                showOKMessage("Confirmar Equipaje", "El equipaje " + noEquipaje + " se confirmado con éxito", { "timeOut": 3000 });
            }
            else if (msg == "Cancel") {
                showOKMessage("Cancelar Equipaje", "El equipaje " + noEquipaje + " se cancelo con éxito", { "timeOut": 3000 });
            }
        }
        //else {
        //    toastr.info(msg);
        //}
    }

    $('#despachar').on('click', function () {
        $('#modalDespachar').modal('show');
    });

    $('#creardespacho').on('click', function () {
        var distributorId = $('#ditribuidorId').val();
        var ids = new Array;
        $.each($(".order-select", "#tableShippings1"), (i, e) => {
            if (e.checked) {
                ids.push($(e).val())
            }
        });
        var urlEntregar = "/Shippings/DespacharEquipajeDistribuidor";
        $.ajax({
            type: "POST",
            async: true,
            url: urlEntregar,
            data: {
                "shippingIds": ids,
                "userId": distributorId
            },
            beforeSend: function () {
                $.blockUI();
            },
            success: function (data) {
                if (data.success)
                    location.reload();
                else
                    toastr.error(data.message);

                $.unblockUI();
            },
            error: function (e) {
                console.log(e);
                $.unblockUI();
            }
        });
    })

    $(document).on('change', '[name="checkComplete"]', function () {
        idBag = $(this).attr('data-id');
        if (!($(this).is(':checked'))) {
            $('#containerNota').show();
            changeStatusBag(false, idBag, "")
        }
        else {
            $('#containerNota').hide();
            changeStatusBag(true, idBag, "")
        }
    });

    $('#confirmNota').click(function () {
        var nota = $('#writeNota').val();
        changeStatusBag(false, idBag, nota)

        $('#containerNota').hide();
        $('#writeNota').val("");

    });
    $('#cancelNota').click(function () {
        $('#containerNota').hide();
    });

    $('#btnReview').on('click', function () {
        $.ajax({
            type: "POST",
            url: "/Shippings/ReviewEquipaje",
            async: true,
            data: {
                Id: idPackageReview,
                bags: bags
            },
            beforeSend: function () {
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
                if (response.success) {
                    toastr.success(response.msg);
                    var rowParent = $(btnReview).parents('tr')[0];
                    $(rowParent).css('background-color', '#ffff0040');
                    $(btnReview).remove()
                }
                else {
                    toastr.error(response.msg);
                }
                $.unblockUI();
            },
            error: function (response) {
                console.log(response);
                $.unblockUI();

            }
        })
    });

    $(document).on('click', '[name="tab"]', function (e) {
        $('[name = "tab"]').removeClass('active');
        $(e.target).addClass('active');
        tabActive = $(e.target).attr('data-type');
        $('.order-select').prop('checked', false);
        if (tabActive == "Enviados") {
            $('#ReportByCarrier').show();
        }
    });

    $("#search").on("change", function (e) {
        searchEquipaje(e);
    })

    $("#search_pqt").on("change", function (e) {
        searchPaquete(e);
    })

    $("#search_bag").on("change", function (e) {
        searchBolsa(e);
    })

    $("#search_pro").on("change", function (e) {
        searchProducto(e);
    })

    /*********************** Check tables **************************/

    $('#sendShippings').on('click', function () {
        if (tabActive = 'Iniciados') {
            var url = "/Shippings/SetCarrierShippings?"
            var count = 0;
            $(".order-select").each(function (i, e) {
                if (e.checked) {
                    if (count == 0)
                        url += "ids[" + count + "]=" + $(e).val();
                    else
                        url += "&ids[" + count + "]=" + $(e).val();
                    count++;
                }
            });
            location.href = url;
        }
    });

    $('[name="Review"]').on('click', function () {
        var id = $(this).attr('data-id');
        $.ajax({
            method: "POST",
            url: "/Shippings/ReviewEquipaje/" + $(this).data("id"),
            success: function (data) {
                if (data.success)
                    toastr.success(data.msg);
                else
                    toastr.error("No se pudo confirmar el equipaje");
            }
        });
    });

    $('[name="checkalltramites"]').on("change", function () {
        var paneActive = $('[class="tab-pane active"]');
        var table = paneActive.find("table");
        var type = $('[class="nav-link active"]').attr("data-type");

        if ($(this)[0].checked) {
            table.find(".order-select").prop("checked", true);
            cantSelect = table.find(".order-select").length;
            if (type == "iniciados") {
                $('#sendShippings').show();
            }
            else if (type == "enviados")
            {
                $('#btnStatusRecibido').show();
                $('#despachar').show();
            }
        } else {
            table.find(".order-select").prop("checked", false);
            cantSelect = 0;
            if (type == "iniciados") {
                $('#sendShippings').hide();
            }
            else if (type == "enviados") {
                $('#btnStatusRecibido').hide();
                $('#despachar').hide();
            }
        }
    });

    $('.nav-link').on('click', function () {
        $('[name="checkalltramites"]').prop('checked', false);
        cantSelect = 0;
        $('#sendShippings').hide();
        $('#btnStatusRecibido').hide();
        $('#despachar').hide();
    })

});


