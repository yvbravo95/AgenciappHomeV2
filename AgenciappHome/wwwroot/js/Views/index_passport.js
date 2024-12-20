var wholesalers = null;
var cantSelect = 0;
var number = null;
var linkFedex = "";
const numberPages = 3;

$.fn.dataTable.ext.errMode = 'none';

//var allTables = isDC ? ["Importadas"] : ["Revision","Pendientes", "Iniciadas", "Despachadas", "Recibidas", "Entregadas", "Canceladas"];
var allTables = isDC ? ["Pendientes", "Iniciadas", "Despachadas", "Procesadas", "Importadas", "Recibidas", "Enviadas", "Canceladas"] : ["Revision", "Pendientes", "Iniciadas", "Despachadas", "Importadas", "Procesadas", "Recibidas", "Entregadas", "Canceladas"];

var currentTab = "base_tabPendientes";

var tableRevision;
var tablePendientes;
var tableProcesadas;
var tableImportadas;
var tableIniciadas;
var tableDespachadas;
var tableRecibidas;
var tableEntregadas;
var tableEnviadas;
var tableManifiesto;
var tableCancelada;

var sc = {
    "None": "",
    "PrimerVez": "PASAPORTE 1 VEZ",
    "PrimerVez2": "PASAPORTE 1 VEZ MENOR",
    "Prorroga1": "PRORROGA 1",
    "Prorroga2": "PRORROGA 2",
    "Renovacion": "RENOVAR",
    "Renovacion2": "RENOVAR MENOR",
    "HE11": "HE-11",
    "DVT": "DVT",
}

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

$.fn.editable.defaults.mode = 'inline';

function ChangeStatus(id, linkFedex, value) {
    $.ajax({
        async: true,
        type: "POST",
        data: {
            value: value,
            id: id,
            linkFedex: linkFedex
        },
        beforeSend: function () {
            $.blockUI()
        },
        success: function (response) {
            location.reload();
            $.unblockUI();
        },
        error: function (e) {
            $.unblockUI();
        }
    })
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

var searchTable = debounce(function (value) {
    if(tableRevision) tableRevision.search(value).draw();
    if (tablePendientes) tablePendientes.search(value).draw();
    if (tableIniciadas) tableIniciadas.search(value).draw();
    if (tableRecibidas) tableRecibidas.search(value).draw();
    if (tableProcesadas) tableProcesadas.search(value).draw();
    if (tableImportadas) tableImportadas.search(value).draw();
    if (tableManifiesto) tableManifiesto.search(value).draw();
    if (tableEnviadas) tableEnviadas.search(value).draw();
    if (tableDespachadas) tableDespachadas.search(value).draw();
    if (tableEntregadas) tableEntregadas.search(value).draw();


}, 350);

var searchCliente = debounce(function (value) {
    if(tableRevision) tableRevision.column("clientFullData:name").search(value).draw();
    if (tablePendientes) tablePendientes.column("clientFullData:name").search(value).draw();
    if (tableIniciadas) tableIniciadas.column("clientFullData:name").search(value).draw();
    if (tableRecibidas) tableRecibidas.column("clientFullData:name").search(value).draw();
    if (tableProcesadas) tableProcesadas.column("clientFullData:name").search(value).draw();
    if (tableImportadas) tableImportadas.column("clientFullData:name").search(value).draw();
    if (tableManifiesto) tableManifiesto.column("clientFullData:name").search(value).draw();
    if (tableEnviadas) tableEnviadas.column("clientFullData:name").search(value).draw();
    if (tableDespachadas) tableDespachadas.column("clientFullData:name").search(value).draw();
    if (tableEntregadas) tableEntregadas.column("clientFullData:name").search(value).draw();


}, 350);

var searchOrder = debounce(function (value) {
    if (tableRevision) tableRevision.column("orderNumber:name").search(value).draw();
    if (tablePendientes) tablePendientes.column("orderNumber:name").search(value).draw();
    if (tableIniciadas) tableIniciadas.column("orderNumber:name").search(value).draw();
    if (tableRecibidas) tableRecibidas.column("orderNumber:name").search(value).draw();
    if (tableProcesadas) tableProcesadas.column("orderNumber:name").search(value).draw();
    if (tableImportadas) tableImportadas.column("orderNumber:name").search(value).draw();
    if (tableManifiesto) tableManifiesto.column("orderNumber:name").search(value).draw();
    if (tableEnviadas) tableEnviadas.column("orderNumber:name").search(value).draw();
    if (tableDespachadas) tableDespachadas.column("orderNumber:name").search(value).draw();
    if (tableEntregadas) tableEntregadas.column("orderNumber:name").search(value).draw();
}, 350);

var searchCancelada = debounce(function (value) {
    tableCancelada.search(value).draw();
}, 350);

var ClearFiltersTables = debounce(function () {
    $("#search_order").val("");
    $("#search_client").val("");
    $("#search").val("");

    if (tableRevision) {
        tableRevision.columns().search("");
        tableRevision.search("");
        tableRevision.draw();
    } 

    if (tablePendientes) {
        tablePendientes.columns().search("");
        tablePendientes.search("");
        tablePendientes.draw();
    }

    if (tableIniciadas) {
        tableIniciadas.columns().search("");
        tableIniciadas.search("");
        tableIniciadas.draw();
    }

    if (tableRecibidas) {
        tableRecibidas.columns().search("");
        tableRecibidas.search("");
        tableRecibidas.draw();
    }
   
    if (tableProcesadas) {
        tableProcesadas.columns().search("");
        tableProcesadas.search("");
        tableProcesadas.draw();
    }

    if (tableImportadas) {
        tableImportadas.columns().search("");
        tableImportadas.search("");
        tableImportadas.draw();
    }

    if (tableManifiesto) {
        tableManifiesto.columns().search("");
        tableManifiesto.search("");
        tableManifiesto.draw();
    }

    if (tableEnviadas) {
        tableEnviadas.columns().search("");
        tableEnviadas.search("");
        tableEnviadas.draw();
    }

    if (tableDespachadas) {
        tableDespachadas.columns().search("");
        tableDespachadas.search("");
        tableDespachadas.draw();
    }

    if (tableEntregadas) {
        tableEntregadas.columns().search("");
        tableEntregadas.search("");
        tableEntregadas.draw();
    }

}, 350);

function Precessing(e, settings, processing, baseTabId) {
    if (processing) {
        $(baseTabId).block(blockOptions);
        $(baseTabId).parent().addClass("disabled");
    }
    else {
        $(baseTabId).unblock();
        $(baseTabId).parent().removeClass("disabled");
    }
}

function UpdateLength(e, settings, currentTab, text) {
    $(currentTab).html(`${text} (${settings.fnRecordsTotal() })`);
}

$(document).ready(function () {
    $('#exportExcelAccept').attr('href', '/passport/ExportExcel/?date=' + document.getElementById("daterangeexcel").value);

    $(document).on('change', '#daterangeexcel', function () {
        const date = document.getElementById("daterangeexcel").value;
        $('#exportExcelAccept').attr('href', '/passport/ExportExcel/?date=' + date)
    });

    $('[name="checkalltramites"]').on("change", function () {
        var paneActive = $('[class="tab-pane active"]');
        var table = paneActive.find("table");
        var type = $('[class="nav-link active"]').attr("data-type");

        if ($(this)[0].checked) {
            table.find(".order-select").prop("checked", true);
            cantSelect = table.find(".order-select").length;
            $(".code").removeClass("hidden");
            if (type == "iniciadas") {
                $("#gen_xml").removeClass("hidden");
                $("#crear_guia").removeClass("hidden");
                $("#despachar").removeClass("hidden");
                $("#btn-statusdespachar").removeClass("hidden");
            } else if (type == "pendientes") {
                $("#iniciar").removeClass("hidden");
                $("#despachar").removeClass("hidden");

            }
            else if (type == "despachadas") {
                if (isDC) {
                    $("#procesar").removeClass("hidden");
                }
            }
        } else {
            table.find(".order-select").prop("checked", false);
            cantSelect = 0;
            $(".code").addClass("hidden");
            if (type == "iniciadas") {
                $("#gen_xml").addClass("hidden");
                $("#crear_guia").addClass("hidden");
                $("#despachar").addClass("hidden");
                $("#btn-statusdespachar").addClass("hidden");
            } else if (type == "pendientes") {
                $("#iniciar").addClass("hidden");
                $("#despachar").addClass("hidden");

            }
            else if (type == "despachadas") {
                if (isDC) {
                    $("#procesar").addClass("hidden");
                }
            }
        }
    });

    $("#gen_code").click(function () {
        selectedIds = [];
        if (cantSelect > 0) {
            $(".order-select").each(function (i, e) {
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
            url: "/Passport/GenCode",
            data: {
                ids: selectedIds,
            },
            beforeSend: function () {
                $.blockUI({
                    message: '<div class="ft-refresh-cw icon-spin font-medium-2"></div>',
                    timeout: 60000,
                    overlayCSS: {
                        backgroundColor: "#FFF",
                        opacity: 0.8,
                        cursor: "wait",
                    },
                    css: {
                        border: 0,
                        padding: 0,
                        backgroundColor: "transparent",
                    },
                });
            },
            success: function (response) {
                if (!response.success) {
                    toastr.error(response.msg);
                    $.unblockUI();
                } else {
                    $.unblockUI();
                    var w = window.open("", "_blank");
                    w.document.write(response.text);
                }
            },
            error: function () {
                toastr.error("No se ha podido crear el codigo", "Error");
                $.unblockUI();
            },
            timeout: 60000,
        });
    });

    $("#cop_code").click(function () {
        selectedIds = [];
        if (cantSelect > 0) {
            $(".order-select").each(function (i, e) {
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
            url: "/Passport/GenCode",
            data: {
                ids: selectedIds,
            },
            beforeSend: function () {
                $.blockUI({
                    message: '<div class="ft-refresh-cw icon-spin font-medium-2"></div>',
                    timeout: 60000,
                    overlayCSS: {
                        backgroundColor: "#FFF",
                        opacity: 0.8,
                        cursor: "wait",
                    },
                    css: {
                        border: 0,
                        padding: 0,
                        backgroundColor: "transparent",
                    },
                });
            },
            success: function (response) {
                if (!response.success) {
                    toastr.error(response.msg);
                    $.unblockUI();
                } else {
                    $.unblockUI();
                    navigator.clipboard.writeText(response.text);
                    toastr.info("El codigo del pasaporte ha sido copiado");
                }
            },
            error: function () {
                toastr.error("No se ha podido copiar el codigo", "Error");
                $.unblockUI();
            },
            timeout: 60000,
        });
    });

    $("#gen_xml").click(function () {
        var selectedIds = [];
        var getXml = function () {
            $.ajax({
                async: true,
                type: "POST",
                data: { ids: selectedIds },
                url: "/Passport/GetXml",
                contentType: "application/x-www-form-urlencoded",
                success: function (resp) {
                    if (resp) {
                        window.open("/Passport/GetGuiaZip/" + resp);
                        $.unblockUI();
                        window.location = "/passport/";
                    }
                    else {
                        toastr.error("No se ha podido crear el codigo, ya hay pasaportes en manifiestos", "Error");

                    }
                },
                error: function () {
                    $.unblockUI();
                    toastr.error("No se ha podido crear el codigo", "Error");
                },
            });
        };
        var agregarAGuia = function (m) {
            $.ajax({
                async: true,
                type: "POST",
                data: { ids: selectedIds },
                url: "/Passport/agregarAGuia",
                contentType: "application/x-www-form-urlencoded",
                success: function (resp) {
                    $.unblockUI();
                    if (resp) {
                        window.location =
                            "/passport/index?msg=Se agregaron " +
                            selectedIds.length +
                            " pasaporetes al manifiesto " +
                            m;
                    }
                    else {
                        toastr.error(
                            "Error al agregar los pasaportes al manifiesto, ya hay pasaportes en manifiestos",
                            "Error"
                        );
                    }
                },
                error: function () {
                    $.unblockUI();
                    toastr.error(
                        "Error al agregar los pasaportes al manifiesto",
                        "Error"
                    );
                },
            });
        };
        if (cantSelect > 0) {
            $(".order-select").each(function (i, e) {
                if (e.checked) {
                    if (e.value != "all") {
                        selectedIds.push($(e).val());
                    }
                }
            });
        }

        if (selectedIds.length > 30) {
            toastr.error("Hay mas de 30 pasaportes seleccionados")
        }
        else {
            $.ajax({
                async: true,
                type: "POST",
                contentType: "application/x-www-form-urlencoded",
                url: "/Passport/FindGuia",
                data: {
                    ids: selectedIds,
                },
                beforeSend: function () {
                    $.blockUI({
                        message: '<div class="ft-refresh-cw icon-spin font-medium-2"></div>',
                        timeout: 60000,
                        overlayCSS: {
                            backgroundColor: "#FFF",
                            opacity: 0.8,
                            cursor: "wait",
                        },
                        css: {
                            border: 0,
                            padding: 0,
                            backgroundColor: "transparent",
                        },
                    });
                },
                success: function (response) {
                    if (response) {
                        if (response == "error") {
                            toastr.error("Los pasaportes no son del mismo tipo")
                            $.unblockUI();
                        }
                        else {
                            swal(
                                {
                                    title: "Atención",
                                    text: "Desea agregar los pasaportes al manifisto " + response,
                                    type: "warning",
                                    showCancelButton: true,
                                    confirmButtonText: "Si, Agregar",
                                    cancelButtonText: "No",
                                    closeOnConfirm: true,
                                    closeOnCancel: true,
                                },
                                function (isConfirm) {
                                    if (isConfirm) {
                                        agregarAGuia(response);
                                    } else {
                                        getXml();
                                    }
                                }
                            );
                        }
                    } else {
                        getXml();
                    }
                },
                error: function () {
                    toastr.error("No se ha podido crear el codigo", "Error");
                    $.unblockUI();
                },
                timeout: 60000,
            });
        }
    });

    $("#crear_guia").click(function () {
        var selectedIds = [];
        var crearGuia = function () {
            $.ajax({
                async: true,
                type: "POST",
                data: { ids: selectedIds },
                url: "/Passport/CrearGuia",
                contentType: "application/x-www-form-urlencoded",
                success: function (resp) {
                    $.unblockUI();
                    if (resp) {
                        window.location = "/passport/";
                    }
                    else {
                        toastr.error("No se ha podido crear la guia, ya hay pasaportes en manifiestos", "Error");
                    }
                },
                error: function () {
                    $.unblockUI();
                    toastr.error("No se ha podido crear la guia", "Error");
                },
            });
        };

        var agregarAGuia = function (m) {
            $.ajax({
                async: true,
                type: "POST",
                data: { ids: selectedIds },
                url: "/Passport/agregarAGuia",
                contentType: "application/x-www-form-urlencoded",
                success: function (resp) {
                    $.unblockUI();
                    if (resp) {
                        window.location =
                            "/passport/index?msg=Se agregaron " +
                            selectedIds.length +
                            " pasaporetes al manifiesto " +
                            m;
                    }
                    else {
                        toastr.error(
                            "Error al agregar los pasaportes al manifiesto, ya hay pasaportes en manifiestos",
                            "Error"
                        );
                    }

                },
                error: function () {
                    $.unblockUI();
                    toastr.error(
                        "Error al agregar los pasaportes al manifiesto",
                        "Error"
                    );
                },
            });
        };

        if (cantSelect > 0) {
            $(".order-select").each(function (i, e) {
                if (e.checked) {
                    if (e.value != "all") {
                        selectedIds.push($(e).val());
                    }
                }
            });
        }

        if (selectedIds.length > 30) {
            toastr.error("Hay mas de 30 pasaportes seleccionados")
        }
        else {
            $.ajax({
                async: true,
                type: "POST",
                contentType: "application/x-www-form-urlencoded",
                url: "/Passport/FindGuia",
                data: {
                    ids: selectedIds,
                },
                beforeSend: function () {
                    $.blockUI({
                        message: '<div class="ft-refresh-cw icon-spin font-medium-2"></div>',
                        timeout: 60000,
                        overlayCSS: {
                            backgroundColor: "#FFF",
                            opacity: 0.8,
                            cursor: "wait",
                        },
                        css: {
                            border: 0,
                            padding: 0,
                            backgroundColor: "transparent",
                        },
                    });
                },
                success: function (response) {
                    if (response) {
                        if (response == "error") {
                            toastr.error("Los pasaportes no son del mismo tipo")
                            $.unblockUI();
                        }
                        else
                            swal(
                                {
                                    title: "Atención",
                                    text: "Desea agregar los pasaportes al manifisto " + response,
                                    type: "warning",
                                    showCancelButton: true,
                                    confirmButtonText: "Si, Agregar",
                                    cancelButtonText: "No",
                                    closeOnConfirm: true,
                                    closeOnCancel: true,
                                },
                                function (isConfirm) {
                                    if (isConfirm) {
                                        agregarAGuia(response);
                                    } else {
                                        crearGuia();
                                    }
                                }
                            );
                    } else {
                        crearGuia();
                    }
                },
                error: function () {
                    toastr.error("No se ha podido crear el codigo", "Error");
                    $.unblockUI();
                },
                timeout: 60000,
            });
        }
    });

    $("#iniciar").click(function () {
        var selectedIds = [];
        if (cantSelect > 0) {
            var c = $(".order-select").toArray();
            c = c.filter(x => x.checked && (isDC || $(x).data("valid") == "True"));
            for (var i = 0; i < c.length; i++) {
                if (c[i].value != "all") {
                    selectedIds.push($(c[i]).val());
                }
            }
        }

        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/Passport/Iniciar",
            data: {
                ids: selectedIds,
            },
            beforeSend: function () {
                $.blockUI({
                    message: '<div class="ft-refresh-cw icon-spin font-medium-2"></div>',
                    timeout: 60000,
                    overlayCSS: {
                        backgroundColor: "#FFF",
                        opacity: 0.8,
                        cursor: "wait",
                    },
                    css: {
                        border: 0,
                        padding: 0,
                        backgroundColor: "transparent",
                    },
                });
            },
            success: function (response) {
                if (response.success) {
                    location.reload();
                } else {
                    toastr.error("No se ha podido iniciar los pasaportes", "Error");
                    $.unblockUI();
                }
            },
            error: function () {
                toastr.error("No se ha podido iniciar los pasaportes", "Error");
                $.unblockUI();
            },
            timeout: 60000,
        });
    });

    $("#procesar").click(function () {
        var selectedIds = [];
        if (cantSelect > 0) {
            var c = $(".order-select").toArray();
            c = c.filter(x => x.checked);
            for (var i = 0; i < c.length; i++) {
                if (c[i].value != "all") {
                    selectedIds.push($(c[i]).val());
                }
            }
        }

        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/Passport/Procesar",
            data: {
                ids: selectedIds,
            },
            beforeSend: function () {
                $.blockUI({
                    message: '<div class="ft-refresh-cw icon-spin font-medium-2"></div>',
                    timeout: 60000,
                    overlayCSS: {
                        backgroundColor: "#FFF",
                        opacity: 0.8,
                        cursor: "wait",
                    },
                    css: {
                        border: 0,
                        padding: 0,
                        backgroundColor: "transparent",
                    },
                });
            },
            success: function (response) {
                if (response.success) {
                    location.reload();
                } else {
                    toastr.error("No se ha podid procesar los pasaportes", "Error");
                    $.unblockUI();
                }
            },
            error: function () {
                toastr.error("No se ha podido procesar los pasaportes", "Error");
                $.unblockUI();
            },
            timeout: 60000,
        });
    });

    format = function date2str(x, y) {
        var z = {
            M: x.getMonth() + 1,
            d: x.getDate(),
            h: x.getHours(),
            m: x.getMinutes(),
            s: x.getSeconds(),
        };
        y = y.replace(/(M+|d+|h+|m+|s+)/g, function (v) {
            return ((v.length > 1 ? "0" : "") + eval("z." + v.slice(-1))).slice(-2);
        });

        return y.replace(/(y+)/g, function (v) {
            return x.getFullYear().toString().slice(-v.length);
        });
    };

    $("#despachar").on("click", function () {
        var x = format(new Date(), "yyMMddhhmm");
        $("#numeroDespacho").val(x);
    });

    $('#btn-statusdespachar').on('click', function () {
        selectedIds = [];
        if (cantSelect > 0) {
            $(".order-select").each(function (i, e) {
                if (e.checked) {
                    if (e.value != "all") {
                        selectedIds.push($(e).val());
                    }
                }
            });
        }

        if (selectedIds.length > 0) {
            // swal confirm
            swal(
                {
                    title: "Atención",
                    text: "¿Desea despachar los pasaportes seleccionados?",
                    type: "warning",
                    showCancelButton: true,
                    confirmButtonText: "Si, Despachar",
                    cancelButtonText: "No",
                    closeOnConfirm: true,
                    closeOnCancel: true,
                },function (isConfirm) {
                    if (isConfirm) {
                        $.ajax({
                            async: true,
                            type: "POST",
                            contentType: "application/x-www-form-urlencoded",
                            url: "/Passport/despachar",
                            data: {
                                email: '',
                                numero: format(new Date(), "yyMMddhhmm"),
                                ids: selectedIds
                            },
                            beforeSend: function () {
                                $.blockUI({
                                    message:
                                        '<div class="ft-refresh-cw icon-spin font-medium-2"></div>',
                                    timeout: 60000,
                                    overlayCSS: {
                                        backgroundColor: "#FFF",
                                        opacity: 0.8,
                                        cursor: "wait",
                                    },
                                    css: {
                                        border: 0,
                                        padding: 0,
                                        backgroundColor: "transparent",
                                    },
                                });
                            },
                            success: function (response) {
                                $.unblockUI();
                                if (!response.success) {
                                    toastr.error(response.message);
                                } else {
                                    toastr.success("Los trámites han sido despachados");
                                    location.href = "/Passport/Index?msg=Los trámites han sido despachados";
                                }
                            },
                            error: function () {
                                toastr.error("No se ha podido realizar el despacho", "Error");
                                $.unblockUI();
                            },
                            timeout: 60000,
                        }); 
                    }
                }
            );

        }
    })

    $("#btnDespachar").on("click", function () {
        var wholesaler = $("#selectWholesaler").val();
        selectedIds = [];
        if (cantSelect > 0) {
            $(".order-select").each(function (i, e) {
                if (e.checked) {
                    if (e.value != "all") {
                        selectedIds.push($(e).val());
                    }
                }
            });
        }
        var email = $("#emailDespacho").val();
        var numdespacho = $("#numeroDespacho").val();
        if (email != "" && numdespacho != "" && selectedIds.length > 0) {
            $.ajax({
                async: true,
                type: "POST",
                contentType: "application/x-www-form-urlencoded",
                url: "/Passport/despachar",
                data: {
                    email: email,
                    numero: numdespacho,
                    ids: selectedIds,
                    wholesalerId: wholesaler,
                },
                beforeSend: function () {
                    $.blockUI({
                        message:
                            '<div class="ft-refresh-cw icon-spin font-medium-2"></div>',
                        timeout: 60000,
                        overlayCSS: {
                            backgroundColor: "#FFF",
                            opacity: 0.8,
                            cursor: "wait",
                        },
                        css: {
                            border: 0,
                            padding: 0,
                            backgroundColor: "transparent",
                        },
                    });
                },
                success: function (response) {
                    if (!response.success) {
                        $.unblockUI();
                        toastr.error(response.msg);
                        console.log(response.exception);
                    } else {
                        toastr.success(response.msg);
                        location.href = "/Passport/Index?msg=" + response.msg;
                    }
                },
                error: function () {
                    toastr.error("No se ha podido realizar el despacho", "Error");
                    $.unblockUI();
                },
                timeout: 60000,
            });
        } else {
            toastr.error("Los datos no son correctos.");
        }
    });

    $.ajax({
        async: true,
        type: "POST",
        url: "/Passport/getWholesalerDispatch",
        success: function (response) {
            if (!response.success) {
                toastr.error(response.msg);
            } else {
                wholesalers = response.data;
                loadSelectWholesaler();
            }
        },
        error: function (e) {
            toastr.error("No se han podido cargar los mayoristas para el despacho");
        },
    });

    function loadSelectWholesaler() {
        if (wholesalers != null) {
            for (var i = 0; i < wholesalers.length; i++) {
                var newOption = new Option(
                    wholesalers[i].name,
                    wholesalers[i].idWholesaler,
                    false,
                    false
                );
                $("#selectWholesaler").append(newOption);
            }
            if (wholesalers.length > 0) {
                $("#emailDespacho").val(wholesalers[0].email);
            }
        }
    }

    $("#selectWholesaler").on("change", function () {
        $("#emailDespacho").val("");

        var id = $(this).val();
        for (var i = 0; i < wholesalers.length; i++) {
            if (wholesalers[i].idWholesaler == id) {
                $("#emailDespacho").val(wholesalers[i].email);
            }
        }
    });

    $('[name="addOtorgamientoMinoristas"]').on("click", function () {
        var passportId = $(this).attr("data-passport");
        var manifiestoId = $(this).attr("data-manifiesto");
        var inputNumber = prompt("Número del cheque");
        if (inputNumber != null) {
            var number = parseInt(inputNumber);
            if (isNaN(number)) {
                toastr.error("Debe escribir un número entero.");
                return false;
            }
            var model = {
                manifiestoId: manifiestoId,
                passportId: passportId,
                number: number,
            };
            $.ajax({
                async: true,
                type: "POST",
                url: "/Passport/AsignarOtorgamiento",
                data: model,
                beforeSend: function () { },
                success: function (response) {
                    if (response.success) {
                        window.open(
                            "/Print/ChequeOtorgamiento?manifiestoId=" +
                            manifiestoId +
                            "&passportId=" +
                            passportId,
                            "_blank"
                        );
                        location.href = "/Passport/IndexImportados?msg=" + response.msg;
                    } else {
                        toastr.error(response.msg, "Error");
                    }
                },
                error: function (e) {
                    toastr.error("No se ha podido crear el cheque.", "Error");
                },
            });
        }
    });

    $('#btnConfirmUpdateStatus').on('click', function () {
        var passportNumber = $('#inputPassportNumber').val();
        var passportExpireDate = $('#inputPassportExpireDate').val();
        var passportId = $('#inputPassportId').val();
        var inputLinkFedex = $('#inputLinkFedex').val();
        $.ajax({
            async: true,
            type: "POST",
            url: '/Passport/index',
            data: {
                value: 4,
                id: passportId,
                passportNumber: passportNumber,
                passportExpireDate: passportExpireDate,
                linkFedex: inputLinkFedex
            },
            beforeSend: function () {
                $.blockUI()
            },
            success: function (response) {
                location.reload();
                $.unblockUI();
            },
            error: function (e) {
                $.unblockUI();
            }
        })
    })

    $('[name="tab"]').on('click', function () {
        $('[name="tab"]').removeClass('active');
        $(this).addClass('active');

        //Desmarco los check
        $('.order-select').prop('checked', false)
        $('[name="checkalltramites"]').prop('checked', false)

        $(".searchColumn").map((i, e) => {
            if ($(e).val() != "") {
                $(e).val("").trigger("change");
            }
        })

        cantSelect = 0;
    });

    $('#status').select2();

    $('.daterange').daterangepicker();

    $("#selectNumDespacho").select2({
        placeholder: "Buscar número de despacho",
        dropdownParent: $("#reportNumDespacho"),
        val: null,
        ajax: {
            type: 'POST',
            url: '/Passport/findDispatch',
            data: function (params) {
                var query = {
                    search: params.term,
                }

                // Query parameters will be ?search=[term]&type=public
                return query;
            },
            processResults: function (data) {
                // Transforms the top-level key of the response object from 'items' to 'results'
                return {
                    results: $.map(data, function (obj) {
                        return { id: obj, text: obj };
                    })
                };
            }
        }
    });

    $('#selectNumDespacho').on('change', function () {
        number = $(this).val();
    })

    $('#btnDespacho').click(function () {
        if (number != null && number != "") {
            window.open('/Print/ReportDispatchPassport?NoDispatch=' + number, 'Nombre Ventana');
        }
        else {
            toastr.error("Debe elegir un número de despacho")
        }
        return false;
    })

    var updateUrlCost = function () {
        var btnAccept = $('#btnReportCost');
        var status = $('#status').val();
        var date = $('#daterange').val();

        btnAccept.attr('href', '/Passport/ReportCostExcel?status=' + status + '&date=' + date);
    }

    $('#btnShowModalReport').on('click', updateUrlCost);

    $('#daterange').on('change', updateUrlCost);

    $('#status').on('change', updateUrlCost);

    $('#btnSearchOrder').on('click', () => searchOrder($("#search_order").val()));

    $('#btnSearchClient').on('click', () => searchCliente($("#search_client").val()));

    $('#btnSearch').on('click', () => searchTable($("#search").val()));

    $('#btnSearchCanceladas').on('click', () => searchCancelada($("#searchCanceladas").val()));

    $('#clearFilters').on('click', ClearFiltersTables)

    $('#btnReportProrroga').on('click', function () {
        const dateRange = $('#daterange-report').val();
        const arrDate = dateRange.split(" - ")
        const dateIni = moment(arrDate[0]).format('MM-DD-YYYY');
        const dateEnd = moment(arrDate[1]).format('MM-DD-YYYY');
        $.ajax({
            async: true,
            type: "GET",
            url: `/Passport/ExportPdfProrrogas?dateInit=${dateIni}&dateEnd=${dateEnd}`,
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
            error: function (e) {
                toastr.error("No se ha podido obtener el reporte");
            },
        });
    })
});

const initStatusTable = (rows) => {
    $.each($('.status', rows), (i, e) => {
        $(e).editable({
            source: function (p) {
                var s = [];
                if (isDC) {
                    s = [
                        {
                            "1": 'Iniciada',
                        },
                        {
                            "1": 'Iniciada',
                            "2": 'Pendiente',
                        },
                        {
                            "1": 'Iniciada',
                            "2": 'Pendiente',
                            "3": 'Despachada',
                            "4": 'Recibida',
                            "9": 'Enviada',
                        },
                        {
                            "1": 'Iniciada',
                            "2": 'Pendiente',
                            "3": 'Despachada',
                            "4": 'Recibida',
                            "9": 'Enviada',
                        },
                        {
                            "1": 'Iniciada',
                            "2": 'Pendiente',
                            "3": 'Despachada',
                            "4": 'Recibida',
                            "9": 'Enviada',
                        },
                        {
                            "6": 'Cancelada',
                        },
                        {
                            "7": "Manifiesto",
                            "4": 'Recibida',
                            "1": 'Iniciada',
                        },
                        {
                            "8": 'Pendiente',
                        },
                        {
                            "9": 'Enviada',
                            "6": 'Cancelada',
                        },
                        {
                            "10": "Importada",
                            "4": 'Recibida',
                            "9": 'Enviada',
                        }
                    ];
                }
                else {
                    s = [
                        {
                            "1": 'Iniciada',
                        },
                        {
                            "1": 'Iniciada',
                            "2": 'Pendiente',
                        },
                        {
                            "1": 'Iniciada',
                            "2": 'Pendiente',
                            "3": 'Despachada',
                            "4": 'Recibida',
                            "5": 'Entregada',
                        },
                        {
                            "1": 'Iniciada',
                            "2": 'Pendiente',
                            "3": 'Despachada',
                            "4": 'Recibida',
                            "5": 'Entregada',
                        },
                        {
                            "1": 'Iniciada',
                            "2": 'Pendiente',
                            "3": 'Despachada',
                            "4": 'Recibida',
                            "5": 'Entregada',
                        },
                        {
                            "6": 'Cancelada',
                        },
                        {
                            "7": "Manifiesto",
                            "4": 'Recibida',
                            "1": 'Iniciada',
                        },
                        {
                            "8": 'Pendiente',
                        },
                        {
                            "9": 'Enviada',
                            "6": 'Cancelada',
                        },
                        {
                            "10": "Consulado",
                            "4": 'Recibida',
                        }
                    ];
                }
                return s[$(this).data('value') - 1];
            },
            validate: function (x) {
                if ($(this).data('value') == 7 && x == 1) {
                    if (!confirm("Recuerde que al cambiar el trámite a estado iniciado este perderá su número de manifiesto actual, ¿Desea continuar?"))
                        return 'invalid';
                }
                if (x == 4) {
                    var type = $(this).attr('data-consularService');
                    if (type == "PrimerVez" || type == "PrimerVez2" || type == "Renovacion" || type == "Renovacion2") {
                        $('#modalStatusRecibida').modal('show');
                        $('#inputPassportId').val($(this).data("id"));
                        return 'validando...';
                    }
                }
                if (isDC && (x == 4 || x == 9)) {
                    linkFedex = prompt("Escriba el link de Fedex:");
                    ChangeStatus($(this).data("id"), linkFedex, x);
                    return 'invalid';
                }
            },
            display: function (value, sourceData) {
                var colors = { "": "gray", 1: "green", 2: "green", 8: "red", 3: "#ffb019", 4: "gray", 5: "blue", 6: "red", 9: "blue" },
                    elem = $.grep(sourceData, function (o) { return o.value == value; });
                if (elem.length) {
                    $(this).text(elem[0].text).css("color", colors[value]);
                } else {
                    $(this).empty();
                }
            },
            ajaxOptions: {
                url: '/Passport/index',
                type: 'post',
                dataType: 'json',
            },
            params: function (params) {
                params.id = $(this).data("id");
                params.linkFedex = linkFedex;
                return params;
            },
            success: function (response, newValue) {
                location.reload();
            },
            error: function (e) {
                linkFedex = "";
            }
        });
    })

}

$(window).on("load", () => {
    var prevTab = $("#" + currentTab);
    var prevContainer = $(prevTab).data("table");

    var block_ele = $("#" + prevContainer + "_div");
    $(block_ele).block(blockOptions);

    for (var i = 0; i < allTables.length; i++) {
        var tableName = allTables[i];
        const baseTabId = `#base_tab${tableName}`;
        $(baseTabId).block(blockOptions);
        $(baseTabId).parent().addClass("disabled");
        var t = null;
        switch (tableName) {
            case "Revision":
                t = $('#tblrevision');
                tableRevision = t.DataTable({
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
                    ajax: $.fn.dataTable.pipeline({
                        url: "/Passport/List?type=Revision",
                        pages: numberPages, // number of pages to cache,
                    }),
                    columnDefs: [
                        {
                            targets: [0],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<label class="custom-control custom-checkbox">`)
                                html.push(` <input type="checkbox" data-valid="${item.debe == 0 && item.isValid}" class="custom-control-input order-select" value="${item.passportId}" />`)
                                html.push(` <span class="custom-control-indicator"></span>`)
                                html.push(` <span class="custom-control-description"></span>`)
                                html.push(`</label>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [1],
                            data: "fechaSolicitud",
                        },
                        {
                            targets: [2],
                            data: "orderNumber",
                            name: "orderNumber",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div><a href="/Passport/Details/${item.passportId}">${item.orderNumber}</a></div>`)

                                if (item.agencyTransferida != null) {
                                    if (item.agencyTransferidaId == idAgency) {
                                        html.push(`<div class="tag tag-info">T. de ${item.agency}</div>`)
                                    }
                                    else {
                                        html.push(`<div class="tag tag-info">${item.agencyTransferida}</div>`)
                                    }
                                }
                                if (item.appMovil) {
                                    html.push(`<div class="tag tag-success">App </div>`)
                                }
                                if (item.express) {
                                    html.push(`<div class="tag tag-danger">Express</div>`)
                                }
                                if (item.agencyPassport != null && item.agencyPassport != "") {
                                    html.push(`<div class="tag tag-default">${item.agencyPassport}</div>`)
                                }
                                if (item.prorrogaNumber) {
                                    html.push(`<div class="tag tag-primary">${item.prorrogaNumber}</div>`)
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [3],
                            data: "clientFullData",
                            name: "clientFullData",
                            render: function (data, type, item) {
                                return `<a target="_blank" href="/clients/tramites?id=${item.clientId}">${item.clientFullData}</a>`;
                            },
                        },
                        {
                            targets: [4],
                            data: "servicioConsular",
                            render: function (data, type, item) {
                                return sc[data];
                            },
                        },
                        {
                            targets: [5],
                            data: "pagado",
                            orderable: false,
                            render: function (data, type, item) {
                                return `${item.pagado} | ${item.debe}`
                            }
                        },
                        {
                            targets: [6],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(` <i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(` <div class="dropdown-menu">`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Details/${data}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`     <a class="dropdown-item initProcedure" href="#" data-id="${data}"><i class="ft-plus" ></i>Iniciar Tramite</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/TrackingOrden/${data}"><i class="ft-info"></i>Tracking</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Edit/${data}"><i class="ft-edit"></i>Editar</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/PagarOrden/${data}"><i class="fa fa-money"></i>Gestionar Pago</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Pays/${data}"><i class="fa fa-money"></i>Registro Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/EditarPagos/${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="#" name="cancel" data-type="passport" data-id="${data}"><i class="ft-x"></i>Cancelar</a>`)
                                html.push(' </div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },

                    ],
                    createdRow: function (row, data, rowIndex) {
                        initStatusTable(row);

                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($('.order-select', row), (i, e) => {
                            $(e).on("change", function () {
                                $('[name="checkalltramites"]').prop("checked", false);

                                if ($(this)[0].checked) {
                                    cantSelect++;
                                } else {
                                    cantSelect--;
                                }
                                var type = $('[class="nav-link active"]').attr("data-type");

                                if (cantSelect == 0) {
                                    $(".code").addClass("hidden");

                                    if (type == "iniciadas") {
                                        $("#despachar").addClass("hidden");
                                        $("#gen_xml").addClass("hidden");
                                        $("#crear_guia").addClass("hidden");
                                    } else if (type == "pendientes") {
                                        $("#iniciar").addClass("hidden");
                                    }
                                } else {
                                    $(".code").removeClass("hidden");

                                    if (type == "iniciadas") {
                                        $("#despachar").removeClass("hidden");
                                        $("#gen_xml").removeClass("hidden");
                                        $("#crear_guia").removeClass("hidden");
                                    } else if (type == "pendientes") {
                                        $("#iniciar").removeClass("hidden");
                                    }
                                }
                            });
                        })

                        $.each($('.initProcedure', row), (i, e) => {
                            $(e).on("click", function () {
                                const id = $(this).data('id')
                                $.ajax({
                                    async: true,
                                    type: "GET",
                                    contentType: "application/x-www-form-urlencoded",
                                    url: "/Passport/StartProcess",
                                    data: {
                                        id: id
                                    },
                                    success: function (response) {
                                        if (response.success)
                                            location.reload();
                                        else
                                            toastr.error(response.message, "Error");
                                    },
                                    error: function () {
                                        toastr.error("No se ha podido iniciar el trámite", "Error");
                                    },
                                });
                            });
                        })
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableRevision));
                })
                $(".column_visible").on("change", function (e) {
                    var i = $(e.target).data('column');
                    tableRevision.column(i).visible($(e.target).is(':checked'));
                    $.each($(".searchColumn", tableRevision.column(i)), (i, e) => {
                        $(e).on('keyup', (e) => searchColumn(e, tableRevision));
                    })
                });
                tableRevision.on("draw", (e, setting) => UpdateLength(e, setting, baseTabId, 'Revision'));
                tableRevision.on("processing", (e, settings, processing) => Precessing(e, settings, processing, baseTabId));
                break;

            case "Pendientes":
                t = $('#tblpendientes');
                tablePendientes = t.DataTable({
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
                    ajax: $.fn.dataTable.pipeline({
                        url: "/Passport/List?type=Pendiente",
                        pages: numberPages, // number of pages to cache,
                    }),
                    columnDefs: [
                        {
                            targets: [0],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<label class="custom-control custom-checkbox">`)
                                html.push(` <input type="checkbox" data-valid="${item.debe == 0 && item.isValid}" class="custom-control-input order-select" value="${item.passportId}" />`)
                                html.push(` <span class="custom-control-indicator"></span>`)
                                html.push(` <span class="custom-control-description"></span>`)
                                html.push(`</label>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [1],
                            data: "fechaSolicitud",
                        },
                        {
                            targets: [2],
                            data: "orderNumber",
                            name: "orderNumber",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div><a href="/Passport/Details/${item.passportId}">${item.orderNumber}</a></div>`)

                                if (item.agencyTransferida != null) {
                                    if (item.agencyTransferidaId == idAgency) {
                                        html.push(`<div class="tag tag-info">T. de ${item.agency}</div>`)
                                    }
                                    else {
                                        html.push(`<div class="tag tag-info">${item.agencyTransferida}</div>`)
                                    }
                                }
                                if (item.appMovil) {
                                    html.push(`<div class="tag tag-success">App </div>`)
                                }
                                if (item.express) {
                                    html.push(`<div class="tag tag-danger">Express</div>`)
                                }
                                if (item.agencyPassport != null && item.agencyPassport != "") {
                                    html.push(`<div class="tag tag-default">${item.agencyPassport}</div>`)
                                }
                                if (item.prorrogaNumber) {
                                    html.push(`<div class="tag tag-primary">${item.prorrogaNumber}</div>`)
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [3],
                            data: "clientFullData",
                            name: "clientFullData",
                            render: function (data, type, item) {
                                return `<a target="_blank" href="/clients/tramites?id=${item.clientId}">${item.clientFullData}</a>`;
                            },
                        },
                        {
                            targets: [4],
                            data: "servicioConsular",
                            render: function (data, type, item) {
                                return sc[data];
                            },
                        },
                        {
                            targets: [5],
                            data: "status",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = [];
                                if (isDC) {
                                    if (parseFloat(item.debe) > 0 || !item.isValid) {
                                        html.push(`<a href="#" class="status" data-id="${item.passportId}" data-consularService="${item.servicioConsular}" data-type="select" data-value="8" data-pk="2" data-url="/post" data-title="Select status"></a>`)
                                    }
                                    else {
                                        html.push(`<a href="#" class="status" data-id="${item.passportId}" data-consularService="${item.servicioConsular}" data-type="select" data-value="2" data-pk="2" data-url="/post" data-title="Select status"></a>`)
                                    }
                                }
                                else {
                                    html.push(`<a href="#" class="status" data-id="${item.passportId} data-consularService="${item.servicioConsular}" data-type="select" data-value="2" data-pk="2" data-url="/post" data-title="Select status"></a>`)
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [6],
                            data: "pagado",
                            orderable: false,
                            render: function (data, type, item) {
                                return `${item.pagado} | ${item.debe}`
                            }
                        },
                        {
                            targets: [7],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(` <i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(` <div class="dropdown-menu">`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Details/${data}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/TrackingOrden/${data}"><i class="ft-info"></i>Tracking</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Edit/${data}"><i class="ft-edit"></i>Editar</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/PagarOrden/${data}"><i class="fa fa-money"></i>Gestionar Pago</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Pays/${data}"><i class="fa fa-money"></i>Registro Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/EditarPagos/${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="#" name="cancel" data-type="passport" data-id="${data}"><i class="ft-x"></i>Cancelar</a>`)
                                html.push(' </div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },

                    ],
                    createdRow: function (row, data, rowIndex) {
                        initStatusTable(row);

                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($('.order-select', row), (i, e) => {
                            $(e).on("change", function () {
                                $('[name="checkalltramites"]').prop("checked", false);

                                if ($(this)[0].checked) {
                                    cantSelect++;
                                } else {
                                    cantSelect--;
                                }
                                var type = $('[class="nav-link active"]').attr("data-type");

                                if (cantSelect == 0) {
                                    $(".code").addClass("hidden");
                                    if (type == "pendientes") {
                                        $("#iniciar").addClass("hidden");
                                    }
                                } else {
                                    $(".code").removeClass("hidden");
                                    if (type == "pendientes") {
                                        $("#iniciar").removeClass("hidden");
                                    }
                                }
                            });
                        })
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tablePendientes));
                })
                $(".column_visible").on("change", function (e) {
                    var i = $(e.target).data('column');
                    tablePendientes.column(i).visible($(e.target).is(':checked'));
                    $.each($(".searchColumn", tablePendientes.column(i)), (i, e) => {
                        $(e).on('keyup', (e) => searchColumn(e, tablePendientes));
                    })
                });
                tablePendientes.on("draw", (e, setting) => UpdateLength(e, setting, baseTabId, 'Pendientes'));
                tablePendientes.on("processing", (e, settings, processing) => Precessing(e, settings, processing, baseTabId));
                break;

            case "Iniciadas":
                t = $('#tbliniciadas');
                tableIniciadas = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: true,
                    lengthMenu: [[10, 1000], [10,"Todas"]],
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
                    ajax: $.fn.dataTable.pipeline({
                        url: "/Passport/List?type=Iniciada",
                        pages: numberPages, // number of pages to cache,
                    }),
                    columnDefs: [
                        {
                            targets: [0],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<label class="custom-control custom-checkbox">`)
                                if (idAgency == agencyDCubaId || item.agencyTransferidaId == agencyDCubaId) {
                                    html.push(` <input type="checkbox" class="custom-control-input order-select" value="${item.passportId}" data-showdespachodc="true" />`)
                                }
                                else {
                                    html.push(` <input type="checkbox" class="custom-control-input order-select" value="${item.passportId}" data-showdespachodc="false" />`)
                                }
                                html.push(` <span class="custom-control-indicator"></span>`)
                                html.push(` <span class="custom-control-description"></span>`)
                                html.push(`</label>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [1],
                            data: "fechaSolicitud",
                        },
                        {
                            targets: [2],
                            data: "orderNumber",
                            name: "orderNumber",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div><a href="/Passport/Details/${item.passportId}">${item.orderNumber}</a></div>`)

                                if (item.agencyTransferida != null) {
                                    if (item.agencyTransferidaId == idAgency) {
                                        html.push(`<div class="tag tag-info">T. de ${item.agency}</div>`)
                                    }
                                    else {
                                        html.push(`<div class="tag tag-info">${item.agencyTransferida}</div>`)
                                    }
                                }
                                if (item.appMovil) {
                                    html.push(`<div class="tag tag-success">App </div>`)
                                }
                                if (item.express) {
                                    html.push(`<div class="tag tag-danger">Express</div>`)
                                }
                                if (item.agencyPassport != null && item.agencyPassport != "") {
                                    html.push(`<div class="tag tag-default">${item.agencyPassport}</div>`)
                                }
                                if (item.prorrogaNumber) {
                                    html.push(`<div class="tag tag-primary">${item.prorrogaNumber}</div>`)
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [3],
                            data: "clientFullData",
                            name: "clientFullData",
                            render: function (data, type, item) {
                                return `<a target="_blank" href="/clients/tramites?id=${item.clientId}">${item.clientFullData}</a>`;
                            },
                        },
                        {
                            targets: [4],
                            data: "servicioConsular",
                            render: function (data, type, item) {
                                return sc[data];
                            },
                        },
                        {
                            targets: [5],
                            data: "status",
                            render: function (date, typr, item) {
                                return `<a href="#" class="status" data-id="${item.passportId}" data-consularService="${item.servicioConsular}" data-type="select" data-value="1" data-pk="1" data-url="/post" data-title="Select status"></a>`
                            }
                        },
                        {
                            targets: [6],
                            data: "pagado",
                            orderable: false,
                            render: function (data, type, item) {
                                return `${item.pagado} | ${item.debe}`
                            }
                        },
                        {
                            targets: [7],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(` <i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(` <div class="dropdown-menu">`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Details/${data}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/TrackingOrden/${data}"><i class="ft-info"></i>Tracking</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Edit/${data}"><i class="ft-edit"></i>Editar</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Pays/${data}"><i class="fa fa-money"></i>Registro Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/EditarPagos/${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="#" name="cancel" data-type="passport" data-id="${data}"><i class="ft-x"></i>Cancelar</a>`)
                                html.push(' </div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },

                    ],
                    createdRow: function (row, data, rowIndex) {
                        initStatusTable(row);
                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($('.order-select', row), (i, e) => {
                            $(e).on("change", function () {
                                $('[name="checkalltramites"]').prop("checked", false);

                                if ($(this)[0].checked) {
                                    cantSelect++;
                                } else {
                                    cantSelect--;
                                }
                                var type = $('[class="nav-link active"]').attr("data-type");

                                //obtener las marcadas con showDespachoDc
                                let countShowDespacho = 0;
                                $(".order-select").each(function (i, e) {
                                    if (e.checked) {
                                        if (e.value != "all" && $(e).data('showdespachodc') == true) {
                                            countShowDespacho++;
                                        }
                                    }
                                });

                                if (cantSelect == 0) {
                                    $(".code").addClass("hidden");

                                    if (type == "iniciadas") {
                                        if (!isDC) {
                                            $("#despachar").addClass("hidden");
                                        }
                                        $("#btn-statusdespachar").addClass("hidden");
                                    }
                                    else if (type == "despachadas") {
                                        if (isDC) {
                                            $("#procesar").addClass("hidden");
                                        }
                                    }
                                } else {
                                    $(".code").removeClass("hidden");

                                    if (type == "iniciadas") {
                                        if (!isDC && countShowDespacho != cantSelect) {
                                            $("#despachar").removeClass("hidden");
                                        }
                                        else $("#despachar").addClass("hidden");

                                        if (countShowDespacho == cantSelect) {
                                            $("#btn-statusdespachar").removeClass("hidden");
                                        }
                                        else $("#btn-statusdespachar").addClass("hidden");
                                    }
                                    else if (type == "despachadas") {
                                        if (isDC) {
                                            $("#procesar").removeClass("hidden");
                                        }
                                    }
                                }
                            });
                        })
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableIniciadas));
                })
                tableIniciadas.on("draw", (e, setting) => UpdateLength(e, setting, baseTabId, 'Iniciadas'));
                tableIniciadas.on("processing", (e, settings, processing) => Precessing(e, settings, processing, baseTabId));
                break;
            case "Despachadas":
                t = $('#tbldespachadas');
                tableDespachadas = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: true,
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
                    ajax: $.fn.dataTable.pipeline({
                        url: "/Passport/List?type=Despachada",
                        pages: numberPages, // number of pages to cache,
                    }),
                    columnDefs: [
                        {
                            targets: [0],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<label class="custom-control custom-checkbox">`)
                                html.push(` <input type="checkbox" class="custom-control-input order-select" value="${item.passportId}" />`)
                                html.push(` <span class="custom-control-indicator"></span>`)
                                html.push(` <span class="custom-control-description"></span>`)
                                html.push(`</label>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [1],
                            data: "fechaDespacho",
                        },
                        {
                            targets: [2],
                            data: "orderNumber",
                            name: "orderNumber",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div><a href="/Passport/Details/${item.passportId}">${item.orderNumber}</a></div>`)

                                if (item.agencyTransferida != null) {
                                    if (item.agencyTransferidaId == idAgency) {
                                        html.push(`<div class="tag tag-info">T. de ${item.agency}</div>`)
                                    }
                                    else {
                                        html.push(`<div class="tag tag-info">${item.agencyTransferida}</div>`)
                                    }
                                }
                                if (item.appMovil) {
                                    html.push(`<div class="tag tag-success">App </div>`)
                                }
                                if (item.express) {
                                    html.push(`<div class="tag tag-danger">Express</div>`)
                                }
                                if (item.agencyPassport != null && item.agencyPassport != "") {
                                    html.push(`<div class="tag tag-default">${item.agencyPassport}</div>`)
                                }
                                if (item.prorrogaNumber) {
                                    html.push(`<div class="tag tag-primary">${item.prorrogaNumber}</div>`)
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [3],
                            data: "numDespacho",
                        },
                        {
                            targets: [4],
                            data: "wholeslerDespacho",
                            render: function (data, type, item) {
                                if (data) {
                                    return data
                                }
                                return "-"
                            }
                        },
                        {
                            targets: [5],
                            data: "fechaImportacion",
                            render: function (data, type, item) {
                                if (data) {
                                    return data
                                }
                                return "-"
                            }
                        },
                        {
                            targets: [6],
                            data: "clientFullData",
                            name: "clientFullData",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a target="_blank" href="/clients/tramites?id=${item.clientId}">${item.clientFullData}</a>`)
                                if (item.chequeOtorgamientoId) {
                                    html.push('<div class="tag tag-info">Otorgamiento</div>')
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [7],
                            data: 'servicioConsular',
                            render: function (data, type, item) {
                                return sc[data];
                            },
                        },
                        {
                            targets: [8],
                            data: "status",
                            render: function (data, type, item) {
                                return `<a href="#" class="status" data-id="${item.passportId}" data-consularService="${item.servicioConsular}" data-type="select" data-value="3" data-pk="3" data-url="/post" data-title="Select status"></a>`
                            }
                        },
                        {
                            targets: [9],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(` <i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(` <div class="dropdown-menu">`)
                                if (item.chequeOtorgamientoId && isDC && (item.servicioConsular == "PrimerVez" || item.servicioConsular == "PrimerVez2" || item.servicioConsular == "HE11")) {
                                        html.push(`<a target="_blank" class="dropdown-item" href="/Print/ChequeOtorgamiento?manifiestoId=${item.manifiestoPasaporteId}&passportId=${item.passportId}"><i class="fa fa-eye"></i>Cheque Otorgamiento</a>`)
                                }
                                html.push(`     <a class="dropdown-item" href="/Passport/Details/${data}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/TrackingOrden/${data}"><i class="ft-info"></i>Tracking</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Pays/${data}"><i class="fa fa-money"></i>Registro Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/EditarPagos/${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="#" name="cancel" data-type="passport" data-id="${data}"><i class="ft-x"></i>Cancelar</a>`)
                                html.push(' </div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },
                    ],
                    createdRow: function (row, data, rowIndex) {
                        initStatusTable(row);

                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($('.order-select', row), (i, e) => {
                            $(e).on("change", function () {
                                $('[name="checkalltramites"]').prop("checked", false);

                                if ($(this)[0].checked) {
                                    cantSelect++;
                                } else {
                                    cantSelect--;
                                }
                                var type = $('[class="nav-link active"]').attr("data-type");

                                if (cantSelect == 0) {
                                    $(".code").addClass("hidden");

                                    if (type == "iniciadas") {
                                        $("#despachar").addClass("hidden");
                                        $("#gen_xml").addClass("hidden");
                                        $("#crear_guia").addClass("hidden");
                                    } else if (type == "pendientes") {
                                        $("#iniciar").addClass("hidden");
                                    }
                                    else if (type == "despachadas") {
                                        if (isDC) {
                                            $("#procesar").addClass("hidden");
                                        }
                                    }
                                } else {
                                    $(".code").removeClass("hidden");

                                    if (type == "iniciadas") {
                                        $("#despachar").removeClass("hidden");
                                        $("#gen_xml").removeClass("hidden");
                                        $("#crear_guia").removeClass("hidden");
                                    } else if (type == "pendientes") {
                                        $("#iniciar").removeClass("hidden");
                                    }
                                    else if (type == "despachadas") {
                                        if (isDC) {
                                            $("#procesar").removeClass("hidden");
                                        }
                                    }
                                }
                            });
                        })
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableDespachadas));
                })
                tableDespachadas.on("draw", (e, setting) => UpdateLength(e, setting, baseTabId, 'Despachadas'));
                tableDespachadas.on("processing", (e, settings, processing) => Precessing(e, settings, processing, baseTabId));
                break;
            case "Recibidas":
                t = $('#tblrecibidas');
                var columnDefs;
                if (isDC) {
                    columnDefs = [
                        {
                            targets: [0],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<label class="custom-control custom-checkbox">`)
                                html.push(` <input type="checkbox" class="custom-control-input order-select" value="${item.passportId}" />`)
                                html.push(` <span class="custom-control-indicator"></span>`)
                                html.push(` <span class="custom-control-description"></span>`)
                                html.push(`</label>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [1],
                            data: "fechaSolicitud",
                        },
                        {
                            targets: [2],
                            data: "fechaRecibido",
                        },
                        {
                            targets: [3],
                            data: "orderNumber",
                            name: "orderNumber",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div><a href="/Passport/Details/${item.passportId}">${item.orderNumber}</a></div>`)

                                if (item.agencyTransferida != null) {
                                    if (item.agencyTransferidaId == idAgency) {
                                        html.push(`<div class="tag tag-info">T. de ${item.agency}</div>`)
                                    }
                                    else {
                                        html.push(`<div class="tag tag-info">${item.agencyTransferida}</div>`)
                                    }
                                }
                                if (item.appMovil) {
                                    html.push(`<div class="tag tag-success">App </div>`)
                                }
                                if (item.express) {
                                    html.push(`<div class="tag tag-danger">Express</div>`)
                                }
                                if (item.agencyPassport != null && item.agencyPassport != "") {
                                    html.push(`<div class="tag tag-default">${item.agencyPassport}</div>`)
                                }
                                if (item.prorrogaNumber) {
                                    html.push(`<div class="tag tag-primary">${item.prorrogaNumber}</div>`)
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [4],
                            data: "clientFullData",
                            name: "clientFullData",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a target="_blank" href="/clients/tramites?id=${item.clientId}">${item.clientFullData}</a>`)
                                if (item.chequeOtorgamientoId) {
                                    html.push('<div class="tag tag-info">Otorgamiento</div>')
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [5],
                            data: "servicioConsular",
                            render: function (data, type, item) {
                                return sc[data];
                            },
                        },
                        {
                            targets: [6],
                            data: "manifiestoPasaporte",
                            render: function (data, type, item) {
                                return `<a href="/Passport/DetailsGuia/${item.guiaPasaporteGuiaId}">${item.manifiestoPasaporte}</a>`;
                            },
                        },
                        {
                            targets: [7],
                            data: "status",
                            render: function (date, typr, item) {
                                return `<a href="#" class="status" data-id="${item.passportId}" data-consularService="${item.servicioConsular}" data-type="select" data-value="4" data-pk="4" data-url="/post" data-title="Select status"></a>`
                            }
                        },
                        {
                            targets: [8],
                            data: "pagado",
                            orderable: false,
                            render: function (data, type, item) {
                                return `${item.pagado} | ${item.debe}`
                            }
                        },
                        {
                            targets: [9],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(` <i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(` <div class="dropdown-menu">`)
                                if (item.chequeOtorgamientoId && isDC && (item.servicioConsular == "PrimerVez" || item.servicioConsular == "PrimerVez2" || item.servicioConsular == "HE11")) {
                                        html.push(`<a target="_blank" class="dropdown-item" href="/Print/ChequeOtorgamiento?manifiestoId=${item.manifiestoPasaporteId}&passportId=${item.passportId}"><i class="fa fa-eye"></i>Cheque Otorgamiento</a>`)
                                }
                                html.push(`     <a class="dropdown-item" href="/Passport/Details/${data}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/TrackingOrden/${data}"><i class="ft-info"></i>Tracking</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Pays/${data}"><i class="fa fa-money"></i>Registro Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/EditarPagos/${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="#" name="cancel" data-type="passport" data-id="${data}"><i class="ft-x"></i>Cancelar</a>`)
                                html.push(' </div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },
                    ]
                }
                else {
                    columnDefs = [
                        {
                            targets: [0],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<label class="custom-control custom-checkbox">`)
                                html.push(` <input type="checkbox" class="custom-control-input order-select" value="${item.passportId}" />`)
                                html.push(` <span class="custom-control-indicator"></span>`)
                                html.push(` <span class="custom-control-description"></span>`)
                                html.push(`</label>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [1],
                            data: "fechaSolicitud",
                        },
                        {
                            targets: [2],
                            data: "fechaRecibido",
                        },
                        {
                            targets: [3],
                            data: "orderNumber",
                            name: "orderNumber",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div><a href="/Passport/Details/${item.passportId}">${item.orderNumber}</a></div>`)

                                if (item.agencyTransferida != null) {
                                    if (item.agencyTransferidaId == idAgency) {
                                        html.push(`<div class="tag tag-info">T. de ${item.agency}</div>`)
                                    }
                                    else {
                                        html.push(`<div" class="tag tag-info">${item.agencyTransferida}</div>`)
                                    }
                                }
                                if (item.appMovil) {
                                    html.push(`<div class="tag tag-success">App </div>`)
                                }
                                if (item.express) {
                                    html.push(`<div class="tag tag-danger">Express</div>`)
                                }
                                if (item.agencyPassport != null && item.agencyPassport != "") {
                                    html.push(`<div class="tag tag-default">${item.agencyPassport}</div>`)
                                }
                                if (item.prorrogaNumber) {
                                    html.push(`<div class="tag tag-primary">${item.prorrogaNumber}</div>`)
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [4],
                            data: "clientFullData",
                            name: "clientFullData",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a target="_blank" href="/clients/tramites?id=${item.clientId}">${item.clientFullData}</a>`)
                                if (item.chequeOtorgamientoId) {
                                    html.push('<div class="tag tag-info">Otorgamiento</div>')
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [5],
                            data: "servicioConsular",
                            render: function (data, type, item) {
                                return sc[data];
                            },
                        },
                        {
                            targets: [6],
                            data: "status",
                            render: function (date, typr, item) {
                                return `<a href="#" class="status" data-id="${item.passportId}" data-consularService="${item.servicioConsular}" data-type="select" data-value="4" data-pk="4" data-url="/post" data-title="Select status"></a>`
                            }
                        },
                        {
                            targets: [7],
                            data: "pagado",
                            orderable: false,
                            render: function (data, type, item) {
                                return `${item.pagado} | ${item.debe}`
                            }
                        },
                        {
                            targets: [8],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(` <i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(` <div class="dropdown-menu">`)
                                if (item.chequeOtorgamientoId && isDC && (item.servicioConsular == "PrimerVez" || item.servicioConsular == "PrimerVez2" || item.servicioConsular == "HE11")) {
                                        html.push(`<a target="_blank" class="dropdown-item" href="/Print/ChequeOtorgamiento?manifiestoId=${item.manifiestoPasaporteId}&passportId=${item.passportId}"><i class="fa fa-eye"></i>Cheque Otorgamiento</a>`)
                                }
                                html.push(`     <a class="dropdown-item" href="/Passport/Details/${data}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/TrackingOrden/${data}"><i class="ft-info"></i>Tracking</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Pays/${data}"><i class="fa fa-money"></i>Registro Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/EditarPagos/${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="#" name="cancel" data-type="passport" data-id="${data}"><i class="ft-x"></i>Cancelar</a>`)
                                html.push(' </div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },
                    ]
                }
                tableRecibidas = t.DataTable({
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
                    ajax: $.fn.dataTable.pipeline({
                        url: "/Passport/List?type=Recibida",
                        pages: numberPages, // number of pages to cache,
                    }),
                    columnDefs: columnDefs,
                    createdRow: function (row, data, rowIndex) {
                        initStatusTable(row);

                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($('.order-select', row), (i, e) => {
                            $(e).on("change", function () {
                                $('[name="checkalltramites"]').prop("checked", false);

                                if ($(this)[0].checked) {
                                    cantSelect++;
                                } else {
                                    cantSelect--;
                                }
                                var type = $('[class="nav-link active"]').attr("data-type");

                                if (cantSelect == 0) {
                                    $(".code").addClass("hidden");

                                    if (type == "iniciadas") {
                                        $("#despachar").addClass("hidden");
                                        $("#gen_xml").addClass("hidden");
                                        $("#crear_guia").addClass("hidden");
                                    } else if (type == "pendientes") {
                                        $("#iniciar").addClass("hidden");
                                    }
                                } else {
                                    $(".code").removeClass("hidden");

                                    if (type == "iniciadas") {
                                        $("#despachar").removeClass("hidden");
                                        $("#gen_xml").removeClass("hidden");
                                        $("#crear_guia").removeClass("hidden");
                                    } else if (type == "pendientes") {
                                        $("#iniciar").removeClass("hidden");
                                    }
                                }
                            });
                        })
                    },
                    initComplete: function (settings, json) {
                        $("#base_tabRecibidas").unblock();
                        $("#base_tabRecibidas").parent().removeClass("disabled");
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableRecibidas));
                })
                tableRecibidas.on("draw", (e, setting) => UpdateLength(e, setting, baseTabId, 'Recibidas'));
                tableRecibidas.on("processing", (e, settings, processing) => Precessing(e, settings, processing, baseTabId));
                break;
            case "Entregadas":
                t = $('#tblentregadas');
                tableEntregadas = t.DataTable({
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
                    ajax: $.fn.dataTable.pipeline({
                        url: "/Passport/List?type=Entregada",
                        pages: numberPages, // number of pages to cache,
                    }),
                    columnDefs: [
                        {
                            targets: [0],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<label class="custom-control custom-checkbox">`)
                                html.push(` <input type="checkbox" class="custom-control-input order-select" value="${item.passportId}" />`)
                                html.push(` <span class="custom-control-indicator"></span>`)
                                html.push(` <span class="custom-control-description"></span>`)
                                html.push(`</label>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [1],
                            data: "fechaSolicitud",
                        },
                        {
                            targets: [2],
                            data: "orderNumber",
                            name: "orderNumber",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div><a href="/Passport/Details/${item.passportId}">${item.orderNumber}</a></div>`)

                                if (item.agencyTransferida != null) {
                                    if (item.agencyTransferidaId == idAgency) {
                                        html.push(`<div class="tag tag-info">T. de ${item.agency}</div>`)
                                    }
                                    else {
                                        html.push(`<div class="tag tag-info">${item.agencyTransferida}</div>`)
                                    }
                                }
                                if (item.appMovil) {
                                    html.push(`<div class="tag tag-success">App </div>`)
                                }
                                if (item.express) {
                                    html.push(`<div class="tag tag-danger">Express</div>`)
                                }
                                if (item.agencyPassport != null && item.agencyPassport != "") {
                                    html.push(`<div class="tag tag-default">${item.agencyPassport}</div>`)
                                }
                                if (item.prorrogaNumber) {
                                    html.push(`<div class="tag tag-primary">${item.prorrogaNumber}</div>`)
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [3],
                            data: "clientFullData",
                            name: "clientFullData",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a target="_blank" href="/clients/tramites?id=${item.clientId}">${item.clientFullData}</a>`)
                                if (item.chequeOtorgamientoId) {
                                    html.push('<div class="tag tag-info">Otorgamiento</div>')
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [4],
                            data: "servicioConsular",
                            render: function (data, type, item) {
                                return sc[data];
                            },
                        },
                        {
                            targets: [5],
                            data: "status",
                            render: function (date, typr, item) {
                                return `<a href="#" class="status" data-id="${item.passportId}" data-consularService="${item.servicioConsular}" data-type="select" data-value="5" data-pk="5" data-url="/post" data-title="Select status"></a>`
                            }
                        },
                        {
                            targets: [6],
                            data: "pagado",
                            orderable: false,
                            render: function (data, type, item) {
                                return `${item.pagado} | ${item.debe}`
                            }
                        },
                        {
                            targets: [7],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(` <i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(` <div class="dropdown-menu">`)
                                if (item.chequeOtorgamientoId && isDC && (item.servicioConsular == "PrimerVez" || item.servicioConsular == "PrimerVez2" || item.servicioConsular == "HE11")) {
                                        html.push(`<a target="_blank" class="dropdown-item" href="/Print/ChequeOtorgamiento?manifiestoId=${item.manifiestoPasaporteId}&passportId=${item.passportId}"><i class="fa fa-eye"></i>Cheque Otorgamiento</a>`)
                                }
                                html.push(`     <a class="dropdown-item" href="/Passport/Details/${data}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`     <a class="dropdown-item btnReciboEntrega" data-id="${item.passportId}"><i class="ft-info"></i>Recibo Entrega</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/TrackingOrden/${data}"><i class="ft-info"></i>Tracking</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Pays/${data}"><i class="fa fa-money"></i>Registro Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/EditarPagos/${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="#" name="cancel" data-type="passport" data-id="${data}"><i class="ft-x"></i>Cancelar</a>`)
                                html.push(' </div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },
                    ],
                    createdRow: function (row, data, rowIndex) {
                        initStatusTable(row);

                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($('.order-select', row), (i, e) => {
                            $(e).on("change", function () {
                                $('[name="checkalltramites"]').prop("checked", false);

                                if ($(this)[0].checked) {
                                    cantSelect++;
                                } else {
                                    cantSelect--;
                                }
                                var type = $('[class="nav-link active"]').attr("data-type");

                                if (cantSelect == 0) {
                                    $(".code").addClass("hidden");

                                    if (type == "iniciadas") {
                                        $("#despachar").addClass("hidden");
                                        $("#gen_xml").addClass("hidden");
                                        $("#crear_guia").addClass("hidden");
                                    } else if (type == "pendientes") {
                                        $("#iniciar").addClass("hidden");
                                    }
                                } else {
                                    $(".code").removeClass("hidden");

                                    if (type == "iniciadas") {
                                        $("#despachar").removeClass("hidden");
                                        $("#gen_xml").removeClass("hidden");
                                        $("#crear_guia").removeClass("hidden");
                                    } else if (type == "pendientes") {
                                        $("#iniciar").removeClass("hidden");
                                    }
                                }
                            });
                        })
                        $.each($('.btnReciboEntrega', row), (i, e) => {
                            $(e).on('click', function () {
                                const id = $(this).attr('data-id');
                                $.ajax({
                                    async: true,
                                    type: "POST",
                                    contentType: "application/x-www-form-urlencoded",
                                    url: "/Passport/createReciboEntrega",
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
                                });
                            })
                        })
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableEntregadas));
                })
                tableEntregadas.on("draw", (e, setting) => UpdateLength(e, setting, baseTabId, 'Entregadas'));
                tableEntregadas.on("processing", (e, settings, processing) => Precessing(e, settings, processing, baseTabId));
                break;
            case "Manifiesto":
                t = $('#tblmanifiesto');
                tableManifiesto = t.DataTable({
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
                    ajax: $.fn.dataTable.pipeline({
                        url: "/Passport/List?type=Manifiesto",
                        pages: numberPages, // number of pages to cache,
                    }),
                    columnDefs: [
                        {
                            targets: [0],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<label class="custom-control custom-checkbox">`)
                                html.push(` <input type="checkbox" class="custom-control-input order-select" value="${item.passportId}" />`)
                                html.push(` <span class="custom-control-indicator"></span>`)
                                html.push(` <span class="custom-control-description"></span>`)
                                html.push(`</label>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [1],
                            data: "fechaSolicitud",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(item.fechaSolicitud)
                                if (item.chequeOtorgamientoId && item.fechaOtorgamiento) {
                                    html.push(`<div class="tag tag-info">${item.fechaOtorgamiento}</div>`)
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [2],
                            data: "orderNumber",
                            name: "orderNumber",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div><a href="/Passport/Details/${item.passportId}">${item.orderNumber}</a></div>`)

                                if (item.agencyTransferida != null) {
                                    if (item.agencyTransferidaId == idAgency) {
                                        html.push(`<div class="tag tag-info">T. de ${item.agency}</div>`)
                                    }
                                    else {
                                        html.push(`<div class="tag tag-info">${item.agencyTransferida}</div>`)
                                    }
                                }
                                if (item.appMovil) {
                                    html.push(`<div class="tag tag-success">App </div>`)
                                }
                                if (item.express) {
                                    html.push(`<div class="tag tag-danger">Express</div>`)
                                }
                                if (item.agencyPassport != null && item.agencyPassport != "") {
                                    html.push(`<div class="tag tag-default">${item.agencyPassport}</div>`)
                                }
                                if (item.prorrogaNumber) {
                                    html.push(`<div class="tag tag-primary">${item.prorrogaNumber}</div>`)
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [3],
                            data: "clientFullData",
                            name: "clientFullData",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a target="_blank" href="/clients/tramites?id=${item.clientId}">${item.clientFullData}</a>`)
                                if (item.chequeOtorgamientoId) {
                                    html.push('<div class="tag tag-info">Otorgamiento</div>')
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [4],
                            data: "servicioConsular",
                            render: function (data, type, item) {
                                return sc[data];
                            },
                        },
                        {
                            targets: [5],
                            data: "status",
                            render: function (date, typr, item) {
                                return `<a href="#" class="status" data-id="${item.passportId}" data-consularService="${item.servicioConsular}" data-type="select" data-value="7" data-pk="6" data-url="/post" data-title="Select status"></a>`
                            }
                        },
                        {
                            targets: [6],
                            data: "manifiestoPasaporte",
                            orderable: false,
                            render: function (data, type, item) {
                                return `<a href="/Passport/DetailsGuia/${item.guiaPasaporteGuiaId}">${item.manifiestoPasaporte}</a>`
                            }
                        },
                        {
                            targets: [7],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(` <i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(` <div class="dropdown-menu">`)
                                if (isDC && (item.servicioConsular == "PrimerVez" || item.servicioConsular == "PrimerVez2" || item.servicioConsular == "HE11")) {
                                    if (item.chequeOtorgamientoId) {
                                        html.push(`<a target="_blank" class="dropdown-item" href="/Print/ChequeOtorgamiento?manifiestoId=${item.manifiestoPasaporteId}&passportId=${item.passportId}"><i class="fa fa-eye"></i>Cheque Otorgamiento</a>`)
                                    }
                                    else {
                                        html.push(`<a class="dropdown-item addChequeOtorgamiento" href="#" data-manifiesto="${item.manifiestoPasaporteId}" data-passport="${item.passportId}"><i class="fa fa-plus"></i>Otorgamiento</a>`)
                                    }
                                }
                                html.push(`     <a class="dropdown-item" href="/Passport/Details/${data}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Pays/${data}"><i class="fa fa-money"></i>Registro Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Edit/${data}"><i class="ft-edit"></i>Editar</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/TrackingOrden/${data}"><i class="ft-info"></i>Tracking</a>`)
                                html.push(' </div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },
                    ],
                    createdRow: function (row, data, rowIndex) {
                        initStatusTable(row);

                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($('.order-select', row), (i, e) => {
                            $(e).on("change", function () {
                                $('[name="checkalltramites"]').prop("checked", false);

                                if ($(this)[0].checked) {
                                    cantSelect++;
                                } else {
                                    cantSelect--;
                                }
                                var type = $('[class="nav-link active"]').attr("data-type");
                                
                                if (cantSelect == 0) {
                                    $(".code").addClass("hidden");

                                    if (type == "iniciadas") {
                                        $("#despachar").addClass("hidden");
                                        $("#gen_xml").addClass("hidden");
                                        $("#crear_guia").addClass("hidden");
                                    } else if (type == "pendientes") {
                                        $("#iniciar").addClass("hidden");
                                    }
                                } else {
                                    $(".code").removeClass("hidden");

                                    if (type == "iniciadas") {
                                        $("#despachar").removeClass("hidden");
                                        $("#gen_xml").removeClass("hidden");
                                        $("#crear_guia").removeClass("hidden");
                                    } else if (type == "pendientes") {
                                        $("#iniciar").removeClass("hidden");
                                    }
                                }
                            });
                        })
                        $.each($('.addChequeOtorgamiento', row), (i, e) => {
                            $(e).on("click", function () {
                                var passportId = $(this).attr("data-passport");
                                var manifiestoId = $(this).attr("data-manifiesto");
                                var inputNumber = prompt("Número del cheque");
                                if (inputNumber != null) {
                                    var number = parseInt(inputNumber);
                                    if (isNaN(number)) {
                                        toastr.error("Debe escribir un número entero.");
                                        return false;
                                    }
                                    var model = {
                                        manifiestoId: manifiestoId,
                                        passportId: passportId,
                                        number: number,
                                    };
                                    $.ajax({
                                        async: true,
                                        type: "POST",
                                        url: "/Passport/AsignarOtorgamiento",
                                        data: model,
                                        beforeSend: function () { },
                                        success: function (response) {
                                            if (response.success) {
                                                window.open(
                                                    "/Print/ChequeOtorgamiento?manifiestoId=" +
                                                    manifiestoId +
                                                    "&passportId=" +
                                                    passportId,
                                                    "_blank"
                                                );
                                                location.href = "/Passport/Index?msg=" + response.msg;
                                            } else {
                                                toastr.error(response.msg, "Error");
                                            }
                                        },
                                        error: function (e) {
                                            toastr.error("No se ha podido crear el cheque.", "Error");
                                        },
                                    });
                                }
                            })
                        })
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableManifiesto));
                })
                tableManifiesto.on("draw", (e, setting) => UpdateLength(e, setting, baseTabId, 'Manifiesto'));
                tableManifiesto.on("processing", (e, settings, processing) => Precessing(e, settings, processing, baseTabId));
                break;
            case "Enviadas":
                t = $('#tblenviadas');
                tableEnviadas = t.DataTable({
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
                    ajax: $.fn.dataTable.pipeline({
                        url: "/Passport/List?type=Enviada",
                        pages: numberPages, // number of pages to cache,
                    }),
                    columnDefs: [
                        {
                            targets: [0],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<label class="custom-control custom-checkbox">`)
                                html.push(` <input type="checkbox" class="custom-control-input order-select" value="${item.passportId}" />`)
                                html.push(` <span class="custom-control-indicator"></span>`)
                                html.push(` <span class="custom-control-description"></span>`)
                                html.push(`</label>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [1],
                            data: "fechaSolicitud"
                        },
                        {
                            targets: [2],
                            data: "orderNumber",
                            name: "orderNumber",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div><a href="/Passport/Details/${item.passportId}">${item.orderNumber}</a></div>`)

                                if (item.agencyTransferida != null) {
                                    if (item.agencyTransferidaId == idAgency) {
                                        html.push(`<div class="tag tag-info">T. de ${item.agency}</div>`)
                                    }
                                    else {
                                        html.push(`<div class="tag tag-info">${item.agencyTransferida}</div>`)
                                    }
                                }
                                if (item.appMovil) {
                                    html.push(`<div class="tag tag-success">App </div>`)
                                }
                                if (item.express) {
                                    html.push(`<div class="tag tag-danger">Express</div>`)
                                }
                                if (item.agencyPassport != null && item.agencyPassport != "") {
                                    html.push(`<div class="tag tag-default">${item.agencyPassport}</div>`)
                                }
                                if (item.prorrogaNumber) {
                                    html.push(`<div class="tag tag-primary">${item.prorrogaNumber}</div>`)
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [3],
                            data: "clientFullData",
                            name: "clientFullData",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a target="_blank" href="/clients/tramites?id=${item.clientId}">${item.clientFullData}</a>`)
                                if (item.chequeOtorgamientoId) {
                                    html.push('<div class="tag tag-info">Otorgamiento</div>')
                                }
                                return html.join("");
                            },
                        },
                        {
                            targets: [4],
                            data: "servicioConsular",
                            render: function (data, type, item) {
                                return sc[data];
                            },
                        },
                        {
                            targets: [5],
                            data: "manifiestoPasaporte",
                            orderable: false,
                            render: function (data, type, item) {
                                return `<a href="/Passport/DetailsGuia/${item.guiaPasaporteGuiaId}">${item.manifiestoPasaporte}</a>`
                            }
                        },
                        {
                            targets: [6],
                            data: "status",
                            render: function (date, typr, item) {
                                return `<a href="#" class="status" data-id="${item.passportId}" data-consularService="${item.servicioConsular}" data-type="select" data-value="9" data-pk="9" data-url="/post" data-title="Select status"></a>`
                            }
                        },
                        {
                            targets: [7],
                            data: "pagado",
                            orderable: false,
                            render: function (data, type, item) {
                                return `${item.pagado} | ${item.debe}`
                            }
                        },
                        {
                            targets: [8],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(` <i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(` <div class="dropdown-menu">`)
                                if (item.chequeOtorgamientoId && isDC && (item.servicioConsular == "PrimerVez" || item.servicioConsular == "PrimerVez2" || item.servicioConsular == "HE11")) {
                                        html.push(`<a target="_blank" class="dropdown-item" href="/Print/ChequeOtorgamiento?manifiestoId=${item.manifiestoPasaporteId}&passportId=${item.passportId}"><i class="fa fa-eye"></i>Cheque Otorgamiento</a>`)
                                }
                                html.push(`     <a class="dropdown-item" href="/Passport/Details/${data}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/TrackingOrden/${data}"><i class="ft-info"></i>Tracking</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Pays/${data}"><i class="fa fa-money"></i>Registro Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/EditarPagos/${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="#" name="cancel" data-type="passport" data-id="${data}"><i class="ft-x"></i>Cancelar</a>`)
                                html.push(' </div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },
                    ],
                    createdRow: function (row, data, rowIndex) {
                        initStatusTable(row);

                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($('.order-select', row), (i, e) => {
                            $(e).on("change", function () {
                                $('[name="checkalltramites"]').prop("checked", false);

                                if ($(this)[0].checked) {
                                    cantSelect++;
                                } else {
                                    cantSelect--;
                                }
                                var type = $('[class="nav-link active"]').attr("data-type");
                                
                                if (cantSelect == 0) {
                                    $(".code").addClass("hidden");

                                    if (type == "iniciadas") {
                                        $("#despachar").addClass("hidden");
                                        $("#gen_xml").addClass("hidden");
                                        $("#crear_guia").addClass("hidden");
                                    } else if (type == "pendientes") {
                                        $("#iniciar").addClass("hidden");
                                    }
                                } else {
                                    $(".code").removeClass("hidden");

                                    if (type == "iniciadas") {
                                        $("#despachar").removeClass("hidden");
                                        $("#gen_xml").removeClass("hidden");
                                        $("#crear_guia").removeClass("hidden");
                                    } else if (type == "pendientes") {
                                        $("#iniciar").removeClass("hidden");
                                    }
                                }
                            });
                        })
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableEnviadas));
                })
                tableEnviadas.on("draw", (e, setting) => UpdateLength(e, setting, baseTabId, 'Enviadas'));
                tableEnviadas.on("processing", (e, setting) => Precessing(e, setting, baseTabId));
                break;
            case "Procesadas":
                t = $('#tblprocesadas');
                tableProcesadas = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: true,
                    lengthMenu: [[10, 1000], [10, "Todas"]],
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
                    ajax: $.fn.dataTable.pipeline({
                        url: "/Passport/List?type=Procesada",
                        pages: numberPages, // number of pages to cache,
                    }),
                    columnDefs: [
                        {
                            targets: [0],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<label class="custom-control custom-checkbox">`)
                                html.push(` <input type="checkbox" class="custom-control-input order-select" value="${item.passportId}" />`)
                                html.push(` <span class="custom-control-indicator"></span>`)
                                html.push(` <span class="custom-control-description"></span>`)
                                html.push(`</label>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [1],
                            data: "fechaSolicitud",
                        },
                        {
                            targets: [2],
                            data: "orderNumber",
                            name: "orderNumber",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div><a href="/Passport/Details/${item.passportId}">${item.orderNumber}</a></div>`)

                                if (item.agencyTransferida != null) {
                                    if (item.agencyTransferidaId == idAgency) {
                                        html.push(`<div class="tag tag-info">T. de ${item.agency}</div>`)
                                    }
                                    else {
                                        html.push(`<div class="tag tag-info">${item.agencyTransferida}</div>`)
                                    }
                                }
                                if (item.appMovil) {
                                    html.push(`<div class="tag tag-success">App </div>`)
                                }
                                if (item.express) {
                                    html.push(`<div class="tag tag-danger">Express</div>`)
                                }
                                if (item.agencyPassport != null && item.agencyPassport != "") {
                                    html.push(`<div class="tag tag-default">${item.agencyPassport}</div>`)
                                }
                                if (item.prorrogaNumber) {
                                    html.push(`<div class="tag tag-primary">${item.prorrogaNumber}</div>`)
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [3],
                            data: "numDespacho",
                        },
                        {
                            targets: [4],
                            data: "clientFullData",
                            name: "clientFullData",
                            render: function (data, type, item) {
                                return `<a target="_blank" href="/clients/tramites?id=${item.clientId}">${item.clientFullData}</a>`;
                            },
                        },
                        {
                            targets: [5],
                            data: "servicioConsular",
                            render: function (data, type, item) {
                                return sc[data];
                            },
                        },
                        {
                            targets: [6],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(` <i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(` <div class="dropdown-menu">`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Details/${data}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/TrackingOrden/${data}"><i class="ft-info"></i>Tracking</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Edit/${data}"><i class="ft-edit"></i>Editar</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Pays/${data}"><i class="fa fa-money"></i>Registro Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/EditarPagos/${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="#" name="cancel" data-type="passport" data-id="${data}"><i class="ft-x"></i>Cancelar</a>`)
                                html.push(' </div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },

                    ],
                    createdRow: function (row, data, rowIndex) {
                        initStatusTable(row);
                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($('.order-select', row), (i, e) => {
                            $(e).on("change", function () {
                                $('[name="checkalltramites"]').prop("checked", false);

                                if ($(this)[0].checked) {
                                    cantSelect++;
                                } else {
                                    cantSelect--;
                                }
                                var type = $('[class="nav-link active"]').attr("data-type");

                                if (cantSelect == 0) {
                                    $(".code").addClass("hidden");

                                    if (type == "iniciadas") {
                                        if (!isDC) {
                                            $("#despachar").addClass("hidden");
                                        }
                                        $("#gen_xml").addClass("hidden");
                                        $("#crear_guia").addClass("hidden");
                                    }
                                    else if (type == "despachadas") {
                                        if (isDC) {
                                            $("#procesar").addClass("hidden");
                                        }
                                    }
                                    else if (type == "pendientes") {
                                        $("#iniciar").addClass("hidden");
                                    }
                                    else if (type == "procesadas") {
                                        $("#despachar").addClass("hidden");
                                    }
                                } else {
                                    $(".code").removeClass("hidden");

                                    if (type == "iniciadas") {
                                        if (!isDC) {
                                            $("#despachar").removeClass("hidden");
                                        }
                                        $("#despachar").removeClass("hidden");
                                        $("#gen_xml").removeClass("hidden");
                                        $("#crear_guia").removeClass("hidden");
                                    }
                                    else if (type == "despachadas") {
                                        if (isDC) {
                                            $("#procesar").removeClass("hidden");
                                        }
                                    }
                                    else if (type == "pendientes") {
                                        $("#iniciar").removeClass("hidden");
                                    }
                                    else if (type == "procesadas") {
                                        $("#despachar").removeClass("hidden");
                                    }
                                }
                            });
                        })
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableProcesadas));
                })
                tableProcesadas.on("draw", (e, setting) => UpdateLength(e, setting, baseTabId, 'Revisión'));
                tableProcesadas.on("processing", (e, settings, processing) => Precessing(e, settings, processing, baseTabId));
                break;
            case "Importadas":
                t = $('#tblimportadas');
                tableImportadas = t.DataTable({
                    searching: true,
                    dom: "ltip",
                    lengthChange: true,
                    lengthMenu: [[10, 1000], [10, "Todas"]],
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
                    ajax: $.fn.dataTable.pipeline({
                        url: "/Passport/List?type=Importada",
                        pages: numberPages, // number of pages to cache,
                    }),
                    columnDefs: [
                        {
                            targets: [0],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<label class="custom-control custom-checkbox">`)
                                html.push(` <input type="checkbox" class="custom-control-input order-select" value="${item.passportId}" />`)
                                html.push(` <span class="custom-control-indicator"></span>`)
                                html.push(` <span class="custom-control-description"></span>`)
                                html.push(`</label>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [1],
                            data: "fechaDespachoImportada",
                        },
                        {
                            targets: [2],
                            data: "orderNumber",
                            name: "orderNumber",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<div><a href="/Passport/Details/${item.passportId}">${item.orderNumber}</a></div>`)

                                if (item.agencyTransferida != null) {
                                    if (item.agencyTransferidaId == idAgency) {
                                        html.push(`<div class="tag tag-info">T. de ${item.agency}</div>`)
                                    }
                                    else {
                                        html.push(`<div class="tag tag-info">${item.agencyTransferida}</div>`)
                                    }
                                }
                                if (item.appMovil) {
                                    html.push(`<div class="tag tag-success">App </div>`)
                                }
                                if (item.express) {
                                    html.push(`<div class="tag tag-danger">Express</div>`)
                                }
                                if (item.agencyPassport != null && item.agencyPassport != "") {
                                    html.push(`<div class="tag tag-default">${item.agencyPassport}</div>`)
                                }
                                if (item.prorrogaNumber) {
                                    html.push(`<div class="tag tag-primary">${item.prorrogaNumber}</div>`)
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [3],
                            data: "numDespachoImportada",
                        },
                        {
                            targets: [4],
                            data: "numDespacho",
                        },
                        {
                            targets: [5],
                            data: "wholeslerDespacho",
                            render: function (data, type, item) {
                                if (data) {
                                    return data
                                }
                                return "-"
                            }
                        },
                        {
                            targets: [6],
                            data: "fechaImportacion",
                            render: function (data, type, item) {
                                if (data) {
                                    return data
                                }
                                return "-"
                            }
                        },
                        {
                            targets: [7],
                            data: "clientFullData",
                            name: "clientFullData",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a target="_blank" href="/clients/tramites?id=${item.clientId}">${item.clientFullData}</a>`)
                                return html.join("");
                            },
                        },
                        {
                            targets: [8],
                            data: 'servicioConsular',
                            render: function (data, type, item) {
                                return sc[data];
                            },
                        },
                        {
                            targets: [9],
                            data: "status",
                            render: function (data, type, item) {
                                //if (isRetail == "True") {
                                //    return data;
                                //}
                                return `<a href="#" class="status" data-id="${item.passportId}" data-consularService="${item.servicioConsular}" data-type="select" data-value="10" data-pk="10" data-url="/post" data-title="Select status"></a>`
                            }
                        },
                        {
                            targets: [10],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(` <i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(` <div class="dropdown-menu">`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Details/${data}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/TrackingOrden/${data}"><i class="ft-info"></i>Tracking</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Edit/${data}"><i class="ft-edit"></i>Editar</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Pays/${data}"><i class="fa fa-money"></i>Registro Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/EditarPagos/${data}"><i class="fa fa-usd"></i>Editar Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="#" name="cancel" data-type="passport" data-id="${data}"><i class="ft-x"></i>Cancelar</a>`)
                                html.push(' </div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },

                    ],
                    createdRow: function (row, data, rowIndex) {
                        initStatusTable(row);
                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($('.order-select', row), (i, e) => {
                            $(e).on("change", function () {
                                $('[name="checkalltramites"]').prop("checked", false);

                                if ($(this)[0].checked) {
                                    cantSelect++;
                                } else {
                                    cantSelect--;
                                }
                                var type = $('[class="nav-link active"]').attr("data-type");

                                if (cantSelect == 0) {
                                    $(".code").addClass("hidden");

                                    if (type == "iniciadas") {
                                        if (!isDC) {
                                            $("#despachar").addClass("hidden");
                                        }
                                        $("#gen_xml").addClass("hidden");
                                        $("#crear_guia").addClass("hidden");
                                        $("#btn-statusdespachar").addClass("hidden");
                                    }
                                    else if (type == "despachadas") {
                                        if (isDC) {
                                            $("#procesar").addClass("hidden");
                                        }
                                    }
                                    else if (type == "pendientes") {
                                        $("#iniciar").addClass("hidden");
                                    }
                                    else if (type == "procesadas") {
                                        $("#despachar").addClass("hidden");
                                    }
                                } else {
                                    $(".code").removeClass("hidden");

                                    if (type == "iniciadas") {
                                        if (!isDC) {
                                            $("#despachar").removeClass("hidden");
                                        }
                                        $("#despachar").removeClass("hidden");
                                        $("#gen_xml").removeClass("hidden");
                                        $("#crear_guia").removeClass("hidden");
                                        $("#btn-statusdespachar").removeClass("hidden");
                                    }
                                    else if (type == "despachadas") {
                                        if (isDC) {
                                            $("#procesar").removeClass("hidden");
                                        }
                                    }
                                    else if (type == "pendientes") {
                                        $("#iniciar").removeClass("hidden");
                                    }
                                    else if (type == "procesadas") {
                                        $("#despachar").removeClass("hidden");
                                    }
                                }
                            });
                        })
                    },
                    initComplete: function (settings, json) {
                        $(baseTabId).unblock();
                        $(baseTabId).parent().removeClass("disabled");
                    }
                });
                $.each($(".searchColumn", t), (i, e) => {
                    $(e).on('keyup', (e) => searchColumn(e, tableImportadas));
                })

                if (isRetail == "True") {
                    tableImportadas.on("draw", (e, setting) => UpdateLength(e, setting, baseTabId, 'Consulado'));

                }
                else {
                    tableImportadas.on("draw", (e, setting) => UpdateLength(e, setting, baseTabId, 'Imp-TramiPro'));
                }
                tableImportadas.on("processing", (e, settings, processing) => Precessing(e, settings, processing, baseTabId));
                break;
            default:
                t = $('#tblcanceladas');
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
                        "url": "/Passport/List?type=Cancelada",
                        "type": 'POST',
                        "dataType": "json"
                    },
                    columnDefs: [
                        {
                            targets: [0],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<label class="custom-control custom-checkbox">`)
                                html.push(` <input type="checkbox" class="custom-control-input order-select" value="${item.passportId}" />`)
                                html.push(` <span class="custom-control-indicator"></span>`)
                                html.push(` <span class="custom-control-description"></span>`)
                                html.push(`</label>`)
                                return html.join("");
                            }
                        },
                        {
                            targets: [1],
                            data: "fechaSolicitud"
                        },
                        {
                            targets: [2],
                            data: "orderNumber",
                            render: function (data, type, item) {
                                var html = [];
                                html.push(`<a href="/Passport/Details/${item.passportId}">${item.orderNumber}</a>`)

                                if (item.agencyTransferida != null) {
                                    if (item.agencyTransferidaId == idAgency) {
                                        html.push(`<div class="tag tag-info">T. de ${item.agency}</div>`)
                                    }
                                    else {
                                        html.push(`<div class="tag tag-info">${item.agencyTransferida}</div>`)
                                    }
                                }
                                if (item.express) {
                                    html.push(`<div class="tag tag-danger">Express</div>`)
                                }
                                if (item.prorrogaNumber) {
                                    html.push(`<div class="tag tag-primary">${item.prorrogaNumber}</div>`)
                                }
                                return html.join("");
                            }
                        },
                        {
                            targets: [3],
                            data: "clientFullData",
                            render: function (data, type, item) {
                                return `<a target="_blank" href="/clients/tramites?id=${item.clientId}">${item.clientFullData}</a>`;                                
                            },
                        },
                        {
                            targets: [4],
                            data: "servicioConsular",
                            render: function (data, type, item) {
                                return sc[data];
                            },
                        },                        
                        {
                            targets: [5],
                            data: "status",
                            render: function (date, typr, item) {
                                return `<a href="#" class="status" data-id="${item.passportId}" data-consularService="${item.servicioConsular}" data-type="select"  data-value="6" data-pk="6" data-url="/post" data-title="Select status"></a>`
                            }
                        },
                        {
                            targets: [6],
                            data: "pagado",
                            orderable: false,
                            render: function (data, type, item) {
                                return `${item.pagado} | ${item.debe}`
                            }
                        },
                        {
                            targets: [7],
                            data: "passportId",
                            orderable: false,
                            render: function (data, type, item) {
                                var html = []
                                html.push(`<div class="dropdown">`)
                                html.push(` <i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>`)
                                html.push(` <div class="dropdown-menu">`)                               
                                html.push(`     <a class="dropdown-item" href="/Passport/Details/${data}"><i class="ft-info"></i>Detalles</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/Pays/${data}"><i class="fa fa-money"></i>Registro Pagos</a>`)
                                html.push(`     <a class="dropdown-item" href="/Passport/TrackingOrden/${data}"><i class="ft-info"></i>Tracking</a>`)
                                html.push(' </div>')
                                html.push('</div>')
                                return html.join("");
                            }
                        },
                    ],
                    createdRow: function (row, data, rowIndex) {
                        initStatusTable(row);

                        $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); })
                        $.each($('.order-select', row), (i, e) => {
                            $(e).on("change", function () {
                                $('[name="checkalltramites"]').prop("checked", false);

                                if ($(this)[0].checked) {
                                    cantSelect++;
                                } else {
                                    cantSelect--;
                                }
                                var type = $('[class="nav-link active"]').attr("data-type");
                                
                                if (cantSelect == 0) {
                                    $(".code").addClass("hidden");

                                    if (type == "iniciadas") {
                                        $("#despachar").addClass("hidden");
                                        $("#gen_xml").addClass("hidden");
                                        $("#crear_guia").addClass("hidden");
                                    } else if (type == "pendientes") {
                                        $("#iniciar").addClass("hidden");
                                    }
                                } else {
                                    $(".code").removeClass("hidden");

                                    if (type == "iniciadas") {
                                        $("#despachar").removeClass("hidden");
                                        $("#gen_xml").removeClass("hidden");
                                        $("#crear_guia").removeClass("hidden");
                                    } else if (type == "pendientes") {
                                        $("#iniciar").removeClass("hidden");
                                    }
                                }
                            });
                        })
                    },
                });
                break
        }
    };

    $(block_ele).unblock();
})