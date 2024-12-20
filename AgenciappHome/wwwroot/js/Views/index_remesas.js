var allTables = role == "EmpleadoCuba" || role == "DistributorCuba"? [] : ["Iniciada", "Entregada", "Cancelada"]
var currentTab = "base_tabIniciada";
var tableIniciada;
var tableEntregada;
var tableCancelada;
var cantSelect = 0;

$.fn.editable.defaults.mode = 'inline'

const agencyUniversalTravel = "68A559FA-AA00-4D52-93B5-DD833B37ED02".toLowerCase();

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

const searchColumn = debounce(function (e, table) {
    var i = $(e.target).data('column');
    table.column(i).search($(e.target).val()).draw();
}, 350);

const searchTable = debounce(function (e) {
    tableIniciada.search($(e.target).val()).draw();
    tableEntregada.search($(e.target).val()).draw();
}, 350);

const searchTableCancelada = debounce(function (e) {
    tableCancelada.search($(e.target).val()).draw();
}, 350);

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



$(window).on("load", () => {
    var prevTab = $("#" + currentTab);
    var prevContainer = $(prevTab).data("table");

    var block_ele = $("#" + prevContainer + "_div");
    $(block_ele).block(blockOptions);

    for (var i = 0; i < allTables.length; i++) {
        var tableName = allTables[i];
        const baseTabId = `#base_tab${tableName}`;
        var t = null;
        switch (tableName) {
            case "Iniciada":
                t = $('#tableInitPendOrders');
                tableIniciada = t.DataTable({
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
                        "url": "/Remesas/List?status=Iniciada",
                        "type": 'POST',
                        "dataType": "json"
                    },
                    columnDefs: [
                        {
                            targets: [0],
                            data: "remittanceId",
                            render: function (data, type, item) {
                                if (item.agencyTransferida != null) {
                                    if (item.agencyTransferidaId == idAgency) {
                                        return `<label class="custom-control custom-checkbox">
                                                    <input type="checkbox" class="custom-control-input remesa-select" value="${item.number}" />
                                                    <span class="custom-control-indicator"></span>
                                                    <span class="custom-control-description"></span>
                                                </label>`
                                    }
                                    return "";
                                }
                                else {
                                    return `<label class="custom-control custom-checkbox">
                                                <input type="checkbox" class="custom-control-input remesa-select" value="${item.number}" />
                                                <span class="custom-control-indicator"></span>
                                                <span class="custom-control-description"></span>
                                            </label>`
                                }
                            }
                        },
                        {
                            targets: [1],
                            data: "date",
                        },
                        {
                            targets: [2],
                            data: 'number',
                            render: function (data, type, item) {
                                let html = [`<a style="display:inline" href="/Remesas/Details/${item.remittanceId}">${item.number}</a>`]
                                if (item.agencyTransferida != null) {
                                    if (item.agencyTransferidaId == idAgency) {
                                        html.push(`<div style = "display: block !important;" class="tag tag-info" > Transferida de ${item.agency}</div >`)
                                    }
                                    else {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">Transferida a ${item.agencyTransferida}</div>`)
                                    }
                                }
                                if (item.user != null) {
                                    html.push(`<div>${item.user}</div>`)
                                }
                                let moneyType = item.moneyType;
                                if (moneyType == "USD_TARJETA") {
                                    moneyType = "MLC";
                                }

                                html.push(`<span style="display: inherit;" class="tag tag-default tag-warning">${moneyType}</span>`)
                                                                return html.join(" ");
                            }
                        },
                        {
                            targets: [3],
                            data: "client",
                        },
                        {
                            targets: [4],
                            data: "contact",
                        },
                        {
                            targets: [5],
                            data: "ciudad"
                        },
                        {
                            targets: [6],
                            data: "status",
                            render: function (data, type, item) {
                                var html = [];

                                if (item.agencyTransferida != null) {
                                    if (item.agencyTransferidaId != idAgency) {
                                        if (item.status == "Iniciada") {
                                            html.push(`<p style="color:darkorange;margin-top:0px;margin-bottom:0px;display:inline;">${item.status}</p>`);
                                        }
                                        else if (item.status == "No Entregada") {
                                            html.push(`<p style="color:darkred;margin-top:0px;margin-bottom:0px;display:inline;">${item.status}</p>`)
                                        }
                                    }
                                    else {
                                        if (item.status == "Iniciada") {
                                            html.push(`<a href="#" class="status" data-id="${item.remittanceId}" data-type="select" data-value="1" data-pk="1" data-url="/post" data-title="Select status"></a>`)
                                        }
                                        else if (item.status == "No Entregada") {
                                            html.push(`<a href="#" class="status" data-id="${item.remittanceId}" data-type="select" data-value="3" data-pk="3" data-url="/post" data-title="Select status"></a>`)
                                        }
                                    }
                                }
                                else {
                                    if (item.status == "Iniciada") {
                                        html.push(`<a href="#" class="status" data-id="${item.remittanceId}" data-type="select" data-value="1" data-pk="1" data-url="/post" data-title="Select status"></a>`)
                                    }
                                    else if (item.status == "No Entregada") {
                                        html.push(`<a href="#" class="status" data-id="${item.remittanceId}" data-type="select" data-value="3" data-pk="3" data-url="/post" data-title="Select status"></a>`)
                                    }
                                }
                                if (parseFloat(item.balance) != 0) {
                                    html.push('<span style="display: inherit;" class="tag tag-default tag-danger">Pendiente</span>');
                                }

                                return html.join(" ");
                            }
                        },
                        {
                            targets: [7],
                            data: "amount"
                        },
                        {
                            targets: [8],
                            data: "aEntregar"
                        },
                        {
                            targets: [9],
                            data: "remittanceId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(`<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(`<div class="dropdown-menu" aria-label="dropdown88fda939-30e3-417f-a848-956e3b110b8d">`)
                                if (item.balance != 0) {
                                    html.push(` <a class="dropdown-item" href="/Pago/GestionarPago?id=${item.remittanceId}&tipo=Remesa"> <i class="fa fa-usd"></i>Gestionar pago</a>`)
                                }
                                html.push(`
                                                <a class="dropdown-item" href="/Remesas/PaysRemesa/${item.remittanceId}"><i class="icon-wallet"></i>Registro de pagos</a>
                                                <a class="dropdown-item print_report" data-RemittanceId="${item.remittanceId}"><i class="fa fa-file-pdf-o"></i>Imprimir Remesa</a>
                                                <a class="dropdown-item" href="/Remesas/Details/${item.remittanceId}"><i class="ft-info"></i>Detalles</a>`);

                                if (idAgency == agencyUniversalTravel) {
                                    if (role == "Agencia") {
                                        html.push(`<a class="dropdown-item" href="/Remesas/Edit/${item.remittanceId}"><i class="ft-info"></i>Editar</a>`)
                                    }
                                }
                                else {
                                    html.push(`<a class="dropdown-item" href="/Remesas/Edit/${item.remittanceId}"><i class="ft-info"></i>Editar</a>`)
                                }

                                html.push(`<a class="dropdown-item" href="/Remesas/EditarPagos/${item.remittanceId}"><i class="ft-info"></i>Editar Pagos</a>`)

                                if (idAgency == agencyUniversalTravel) {
                                    if (role == "Agencia") {
                                        html.push(`<a class="dropdown-item" href="#" name="cancel" data-type="remesa" data-id="${item.remittanceId}" ><i class="ft-x"></i>Cancelar</a>`);
                                    }
                                }
                                else {
                                    html.push(`<a class="dropdown-item" href="#" name="cancel" data-type="remesa" data-id="${item.remittanceId}" ><i class="ft-x"></i>Cancelar</a>`);
                                }

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
                                            "1": 'Iniciada',
                                            "2": 'Entregada',
                                            "3": 'No Entregada',
                                        },
                                        {
                                            "2": 'Entregada',
                                            "3": 'No Entregada',
                                        },
                                        {
                                            "2": 'Entregada',
                                            "3": 'No Entregada',
                                        },
                                        {
                                            "4": 'Cancelada',
                                        },



                                    ];
                                    return s[$(this).data('value') - 1];

                                },
                                display: function (value, sourceData) {
                                    var colors = { "": "gray", 1: "#ffb019", 2: "green", 3: "red", 4: "red" },
                                        elem = $.grep(sourceData, function (o) { return o.value == value; });

                                    if (elem.length) {
                                        $(this).text(elem[0].text).css("color", colors[value]);
                                    } else {
                                        $(this).empty();
                                    }

                                },
                                ajaxOptions: {
                                    url: '/Remesas/index',
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
                                //showbuttons: false
                            });

                        })
                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($(".remesa-select", row), (i, e) => {
                            $(e).on("change", function () {
                                if ($(this)[0].checked) {
                                    cantSelect++;
                                    //selectedIds.push($(this).val());
                                } else {
                                    cantSelect--;
                                    //selectedIds.splice($.inArray($(this).val(), selectedIds), 1);
                                }

                                if (/*selectedIds.length*/ cantSelect == 0) {
                                    $("#gen_code").addClass("hidden");
                                    $("#gen_report").addClass("hidden");
                                    $("#gen_reporteMayorista").addClass("hidden");
                                } else {
                                    $("#gen_code").removeClass("hidden");
                                    $("#gen_report").removeClass("hidden");
                                    $("#gen_reporteMayorista").removeClass("hidden");
                                }
                            });
                        })
                        $.each($(".print_report", row), (i, e) => {
                            $(e).click(function () {
                                var id = $(this).attr('data-RemittanceId');
                                $.ajax({
                                    async: true,
                                    type: "POST",
                                    contentType: "application/x-www-form-urlencoded",
                                    url: "/Remesas/createOrderComprobante",
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
                        $("#base_tabIniciada").html(`Iniciadas (${json.recordsTotal})`);
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableIniciada));
                })
                $.each($(".order-select", t), (i, e) => {
                    $(e).on("change", function () {
                        if ($(this)[0].checked) {
                            cantSelect++;
                        } else {
                            cantSelect--;
                        }
                        if (cantSelect == 0) {
                            $("#gen_report").addClass("hidden");
                            $("#gen_reporteMayorista").addClass("hidden");
                        } else {
                            $("#gen_report").removeClass("hidden");
                            $("#gen_reporteMayorista").removeClass("hidden");
                        }
                    });
                })
                break;
            case "Entregada":
                t = $('#tableEntregadaOrders');
                tableEntregada = t.DataTable({
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
                        "url": "/Remesas/List?status=Entregada",
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
                            data: 'number',
                            render: function (data, type, item) {
                                let html = [`<a style="display:inline" href="/Remesas/Details/${item.remittanceId}">${item.number}</a>`]
                                if (item.agencyTransferida != null) {
                                    if (item.agencyTransferidaId == idAgency) {
                                        html.push(`<div style = "display: block !important;" class="tag tag-info" > Transferida de ${item.agency}</div >`)
                                    }
                                    else {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">Transferida a ${item.agencyTransferida}</div>`)
                                    }
                                }

                                let moneyType = item.moneyType;
                                if (moneyType == "USD_TARJETA") {
                                    moneyType = "MLC";
                                }

                                html.push(`<span style="display: inherit;" class="tag tag-default tag-warning">${moneyType}</span>`)
                                return html.join(" ");
                            }
                        },
                        {
                            targets: [2],
                            data: "client",
                        },
                        {
                            targets: [3],
                            data: "contact",
                        },
                        {
                            targets: [4],
                            data: "ciudad"
                        },
                        {
                            targets: [5],
                            data: "status",
                            render: function (data, type, item) {
                                var html = [];

                                if (item.agencyTransferida != null) {
                                    if (item.agencyTransferidaId != idAgency) {
                                        if (item.status == "Entregada") {
                                            html.push(`<p style="color:green;margin-top:0px;margin-bottom:0px;display:inline;">${item.status}</p>`);
                                        }
                                        else if (item.status == "Despachada") {
                                            html.push(`<p style="color:green;margin-top:0px;margin-bottom:0px;display:inline;">${item.status}</p>`)
                                        }
                                    }
                                    else {
                                        if (item.status == "Entregada") {
                                            html.push(`<a href="#" class="status" data-id="${item.remittanceId}" data-type="select" data-value="2" data-pk="2" data-url="/post" data-title="Select status"></a>`)
                                        }
                                        else if (item.status == "Despachada") {
                                            html.push(`<p style="color:crimson;margin-top:0px;margin-bottom:0px;display:inline;">${item.status}</p>`)
                                            if (item.despachadaA != null) {
                                                html.push(`<div style="display: block !important;" class="tag tag-danger">${item.despachadaA}</div>`)
                                            }
                                        }
                                    }
                                }
                                else {
                                    if (item.status == "Entregada") {
                                        html.push(`<a href="#" class="status" data-id="${item.remittanceId}" data-type="select" data-value="2" data-pk="2" data-url="/post" data-title="Select status"></a>`)
                                    }
                                    else if (item.status == "Despachada") {
                                        html.push(`<p style="color:crimson;margin-top:0px;margin-bottom:0px;display:inline;">${item.status}</p>`)
                                        if (item.despachadaA != null) {
                                            html.push(`<div style="display: block !important;" class="tag tag-danger">${item.despachadaA}</div>`)
                                        }
                                    }
                                }
                                if (parseFloat(item.balance) != 0) {
                                    html.push('<span style="display: inherit;" class="tag tag-default tag-danger">Pendiente</span>');
                                }

                                return html.join(" ");
                            }
                        },
                        {
                            targets: [6],
                            data: "amount"
                        },
                        {
                            targets: [7],
                            data: "aEntregar"
                        },
                        {
                            targets: [8],
                            data: "remittanceId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(`<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(`<div class="dropdown-menu" aria-label="dropdown88fda939-30e3-417f-a848-956e3b110b8d">`)
                                if (item.balance != 0) {
                                    html.push(` <a class="dropdown-item" href="/Pago/GestionarPago?id=${item.remittanceId}&tipo=Remesa"> <i class="fa fa-usd"></i>Gestionar pago</a>`)
                                }
                                html.push(`
                                                <a class="dropdown-item" href="/Remesas/PaysRemesa/${item.remittanceId}"><i class="icon-wallet"></i>Registro de pagos</a>
                                                <a class="dropdown-item print_report" data-RemittanceId="${item.remittanceId}"><i class="fa fa-file-pdf-o"></i>Imprimir Remesa</a>
                                                <a class="dropdown-item" href="/Remesas/Details/${item.remittanceId}"><i class="ft-info"></i>Detalles</a>
                                `)

                                if (idAgency == agencyUniversalTravel) {
                                    if (role == "Agencia") {
                                        html.push(`<a class="dropdown-item" href="#" name="cancel" data-type="remesa" data-id="${item.remittanceId}" ><i class="ft-x"></i>Cancelar</a>`);
                                    }
                                }
                                else {
                                    html.push(`<a class="dropdown-item" href="#" name="cancel" data-type="remesa" data-id="${item.remittanceId}" ><i class="ft-x"></i>Cancelar</a>`);
                                }

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
                                            "1": 'Iniciada',
                                            "2": 'Entregada',
                                            "3": 'No Entregada',
                                        },
                                        {
                                            "2": 'Entregada',
                                            "3": 'No Entregada',
                                        },
                                        {
                                            "2": 'Entregada',
                                            "3": 'No Entregada',
                                        },
                                        {
                                            "4": 'Cancelada',
                                        },



                                    ];
                                    return s[$(this).data('value') - 1];

                                },
                                display: function (value, sourceData) {
                                    var colors = { "": "gray", 1: "#ffb019", 2: "green", 3: "red", 4: "red" },
                                        elem = $.grep(sourceData, function (o) { return o.value == value; });

                                    if (elem.length) {
                                        $(this).text(elem[0].text).css("color", colors[value]);
                                    } else {
                                        $(this).empty();
                                    }

                                },
                                ajaxOptions: {
                                    url: '/Remesas/index',
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
                                //showbuttons: false
                            });

                        })
                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($(".remesa-select", row), (i, e) => {
                            $(e).on("change", function () {
                                if ($(this)[0].checked) {
                                    cantSelect++;
                                    //selectedIds.push($(this).val());
                                } else {
                                    cantSelect--;
                                    //selectedIds.splice($.inArray($(this).val(), selectedIds), 1);
                                }

                                if (/*selectedIds.length*/ cantSelect == 0) {
                                    $("#gen_code").addClass("hidden");
                                    $("#gen_report").addClass("hidden");
                                    $("#gen_reporteMayorista").addClass("hidden");
                                } else {
                                    $("#gen_code").removeClass("hidden");
                                    $("#gen_report").removeClass("hidden");
                                    $("#gen_reporteMayorista").removeClass("hidden");
                                }
                            });
                        })
                        $.each($(".print_report", row), (i, e) => {
                            $(e).click(function () {
                                var id = $(this).attr('data-RemittanceId');
                                $.ajax({
                                    async: true,
                                    type: "POST",
                                    contentType: "application/x-www-form-urlencoded",
                                    url: "/Remesas/createOrderComprobante",
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
                        $("#base_tabEntregada").html(`Entregadas (${json.recordsTotal})`);
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableEntregada));
                })
                break;
            case "Cancelada":
                t = $('#tableCanceladaOrders');
                tableCancelada = t.DataTable({
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
                        "url": "/Remesas/List?status=Cancelada",
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
                            data: 'number',
                            render: function (data, type, item) {
                                let html = [`<a style="display:inline" href="/Remesas/Details/${item.remittanceId}">${item.number}</a>`]
                                if (item.agencyTransferida != null) {
                                    if (item.agencyTransferidaId == idAgency) {
                                        html.push(`<div style = "display: block !important;" class="tag tag-info" > Transferida de ${item.agency}</div >`)
                                    }
                                    else {
                                        html.push(`<div style="display: block !important;" class="tag tag-info">Transferida a ${item.agencyTransferida}</div>`)
                                    }
                                }
                                let moneyType = item.moneyType;
                                if (moneyType == "USD_TARJETA") {
                                    moneyType = "MLC";
                                }

                                html.push(`<span style="display: inherit;" class="tag tag-default tag-warning">${moneyType}</span>`)
                                return html.join(" ");
                            }
                        },
                        {
                            targets: [2],
                            data: "client",
                        },
                        {
                            targets: [3],
                            data: "ciudad"
                        },
                        {
                            targets: [4],
                            data: "status",
                            render: function (data, type, item) {
                                return `<p style="color:darkred;margin-top:0px;margin-bottom:0px;display:inline;">${item.status}</p>`;
                            }
                        },
                        {
                            targets: [5],
                            data: "amount"
                        },
                        {
                            targets: [6],
                            data: "aEntregar"
                        },
                        {
                            targets: [7],
                            data: "remittanceId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(`<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(`<div class="dropdown-menu" aria-label="dropdown88fda939-30e3-417f-a848-956e3b110b8d">`)
                                html.push(`
                                                <a class="dropdown-item" href="/Remesas/PaysRemesa/${item.remittanceId}"><i class="icon-wallet"></i>Registro de pagos</a>
                                                <a class="dropdown-item" href="/Remesas/Details/${item.remittanceId}"><i class="ft-info"></i>Detalles</a>
                                `)
                                html.push('</div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },

                    ],
                    createdRow: function (row, data, rowIndex) {
                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                    },
                });
                $.each($(".searchcanceladas", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableCancelada));
                })
                break;
            default:
                break
        }


        if (!tableName.includes("Caneladas")) {
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
    }
});

$(document).ready(function () {

    $('#search').on('keyup change', searchTable);

    var url = decodeURIComponent(window.location);
    var params = url.split("?")[1];
    if (params != null) {
        var params = params.split("&");
        var msg = params[0].split("=")[1];
        if (msg == "success") {
            var orderNumber = params[1].split("=")[1];
            showOKMessage("Nueva Remesa", "Remesa " + orderNumber + " creada con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "updateStatusOrden") {
            var orderNumber = params[1].split("=")[1];
            showOKMessage("Actualización de Estado", "Estatus actualizado a " + orderNumber , { "timeOut": 0, "closeButton": true });
        } else if (msg == "successEdit") {
            var orderNumber = params[1].split("=")[1];
            showOKMessage("Editar Remesa", "Remesa " + orderNumber + " editada con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successDelete") {
            var orderNumber = params[1].split("=")[1];
            showOKMessage("Eliminar Remesa", "Remesa " + orderNumber + " cancelada con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successPago") {
            var pagoNumber = params[1].split("=")[1];
            showOKMessage("Registro de Pago", "Se ha efectuado el pago de la remesa " + pagoNumber + " con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successEntregada") {
            var orderNumber = params[1].split("=")[1];
            showOKMessage("Remesa Entregada", "Remesa " + ordenNumber + " marcada como entregada con éxito", { "timeOut": 0, "closeButton": true });
        }
    }

    $("#selectMayorista").select2({
        placeholder: "Seleccione un mayorista",
        val: null
    });

    //************* Reporte por check *******************

    $('#gen_reporteMayorista').on('click', function () {
        $('#modalsenMayorista').modal('show');
    });

    $('#sendMayorista').click(function () {
        $('#modalsenMayorista').modal('hide');

        selectedIds = new Array;
        $(".remesa-select").each(function (i, e) {
            if (e.checked) {
                selectedIds.push($(e).val());
            }
        });
        $.ajax({
            async: true,
            type: "POST",
            url: "/Remesas/enviarReporteMayorista",
            data: {
                ids: selectedIds,
                idmayorista: $('#selectMayorista').val()
            },
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                var aux = response.split('-');
                if (aux[0] == "success") {
                    toastr.success(aux[1]);
                    location.reload();
                }
                else {
                    toastr.error(aux[1]);
                }
                $.unblockUI();
            },
            error: function () {
                $.unblockUI();
                toastr.error("No se ha podido exportar", "Error");
            },
            timeout: 15000,
        });
    });

    $("#gen_report").click(function () {

        selectedIds = new Array;

        $(".remesa-select").each(function (i, e) {
            if (e.checked) {
                selectedIds.push($(e).val());
            }
        });
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/Remesas/Report",
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
            timeout: 10000,
        });
    });
    $('#gen_reportFecha').on('click', function () {
        $('#modalReporte').modal('show');
    });
    $("#gen_code").click(function () {
        selectedIds = [];
        if (cantSelect > 0) {
            $(".remesa-select").each(function (i, e) {
                if (e.checked) {
                    if (e.value != "all") {
                        selectedIds.push($(e).val());
                    }
                }
            });
        }

        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/Remesas/GenCode",
            data: {
                ids: selectedIds,
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
                if (!response.success) {
                    toastr.error(response.msg);
                    $.unblockUI();
                }
                else {
                    $.unblockUI();
                    var w = window.open("", "_blank");
                    w.document.write(response.text);
                }
            },
            error: function () {
                toastr.error("No se ha podido crear el codigo", "Error");
                $.unblockUI()
            },
            timeout: 60000,
        });
    });

    //************* Fin Reporte por check *******************

    var tab = 1;
    var nameTable = "tableInitPendOrders";

    $("[data-toggle='tab']").click(function () {
        $("[data-toggle='tab']").removeClass("active");
    });

    if (role == "EmpleadoCuba" || role == "DistributorCuba") {
        $(".order-select").on("change", function () {
            if ($(this)[0].checked) {
                cantSelect++;
                //selectedIds.push($(this).val());
            } else {
                cantSelect--;
                //selectedIds.splice($.inArray($(this).val(), selectedIds), 1);
            }

            if (/*selectedIds.length*/ cantSelect == 0) {
                $("#gen_report").addClass("hidden");
                $("#gen_reporteMayorista").addClass("hidden");
            } else {
                $("#gen_report").removeClass("hidden");
                $("#gen_reporteMayorista").removeClass("hidden");
            }
        });
    }
   

    $("#base-tab1").click(function () {
        tab = 1;

        $("#searchBtn").val("");
        nameTable = "tableInitPendOrders";
        $("#tableInitPendOrders tr").removeClass("hidden");

        if ($("#tableInitPendOrders tr").length == 1)
            $("#no_result").removeClass("hidden");
        else
            $("#no_result").addClass("hidden");

        selectedIds = new Array;
        $(".order-select").attr("checked", false);
        $("#gen_report").addClass("hidden");
        $("#gen_reporteMayorista").addClass("hidden");
    });

    $("#base-tab2").click(function () {
        tab = 2;

        $("#searchBtn").val("");
        nameTable = "tableCompleteOrders";
        $("#tableCompleteOrders tr").removeClass("hidden");

        if ($("#tableCompleteOrders tr").length == 1)
            $("#no_result").removeClass("hidden");
        else
            $("#no_result").addClass("hidden");

        selectedIds = new Array;
        $(".order-select").attr("checked", false);
        $("#gen_report").addClass("hidden");
        $("#gen_reporteMayorista").addClass("hidden");
    });

    $("#base-tab3").click(function () {
        tab = 3;

        $("#searchBtn").val("");
        nameTable = "tableEntregOrders";
        $("#tableEntregOrders tr").removeClass("hidden");

        if ($("#tableEntregOrders tr").length == 1)
            $("#no_result").removeClass("hidden");
        else
            $("#no_result").addClass("hidden");

        selectedIds = new Array;
        $(".order-select").attr("checked", false);
        $("#gen_report").addClass("hidden");
        $("#gen_reporteMayorista").addClass("hidden");
    });

    $("#base-tab4").click(function () {
        tab = 4;

        $("#searchBtn").val("");
        nameTable = "tableAllOrders";
        $("#tableAllOrders tr").removeClass("hidden");

        if ($("#tableAllOrders tr").length == 1)
            $("#no_result").removeClass("hidden");
        else
            $("#no_result").addClass("hidden");

        selectedIds = new Array;
        $(".order-select").attr("checked", false);
        $("#gen_report").addClass("hidden");
        $("#gen_reporteMayorista").addClass("hidden");
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
        var okConfirm = function () {
            var urlDelete = "/Remesas/Cancel/" + orderId;
            $.ajax({
                type: "GET",
                url: urlDelete,
                async: false,
                success: function () {
                    document.location = "/Remesas?msg=successDelete&orderNumber=" + orderNumber;
                }
            });
        };
        getCancelConfirmation(okConfirm);
    });

    $('#searchcanceladas').on('keyup change', searchTableCancelada)
});