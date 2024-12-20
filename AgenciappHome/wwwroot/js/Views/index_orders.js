$('#searchServer').hide()
$(document).ready(function () {

    function block() {
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
    }

    /*$("#mostrarTodo").on("click", function () {
        block();
    })*/

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
            showOKMessage("Actualización de Estado", "Estatus actualizado a " + orderNumber , { "timeOut": 0, "closeButton": true });
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
        }
    }

    var tab = 1;
    var nameTable = "tableInitPendOrders";
    var cantSelect = 0;

    $("[data-toggle='tab']").click(function () {
        $("[data-toggle='tab']").removeClass("active");
    });

    $(document).on("change", ".order-select", function () {
        if ($(this)[0].checked) {
            cantSelect++;
            //selectedIds.push($(this).val());
        } else {
            cantSelect--;
            //selectedIds.splice($.inArray($(this).val(), selectedIds), 1);
        }
        var tabactive = $('[name = "tab"][class = "nav-link tablePartial active"]');
        if (/*selectedIds.length*/ cantSelect == 0) {
            $("#gen_report").addClass("hidden");
            $("#btn_transferir").addClass("hidden");
            $("#entregar-orders").addClass("hidden");
            //Si el tab es completadas
            if (tabactive.attr('data-type') == "enviadas") {
                $('#despachar').hide();
            }
        } else {
            $("#gen_report").removeClass("hidden");
            $("#btn_transferir").removeClass("hidden");
            $("#entregar-orders").removeClass("hidden");
            //Si el tab es completadas
            if (tabactive.attr('data-type') == "enviadas") {
                $('#despachar').show();
            }
        }
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

    $(document).on('click', ".print_report",function () {
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
    });

    $(document).on('click',".checkEntregada",function () {
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

    $(document).on('click', ".cancelOrder",function () {
        var orderId = $(this).attr("orderId");
        var orderNumber = $(this).attr("name");
        var okConfirm = function () {
            var urlDelete = "/Orders/Cancel/" + orderId;
            $.ajax({
                type: "GET",
                url: urlDelete,
                async: false,
                success: function () {
                    document.location = "/OrderNew?msg=successCancell&orderNumber=" + orderNumber;
                }
            });
        };
        getCancelConfirmation(okConfirm);
    });

    //Para el despacho
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
                    location.href = "/OrderNew/Index?msg=" + response.msg;
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

    $('#tipotramite').select2({
        placeholder: "Trámite a Despachar",
        val: null,
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

});


var tables = {};
var allTables = ["Iniciada_Pendiente", "Completada", "Enviada", "Despachada", "Revisada", "Entregada", "Cancelada"]
var currentTab = "base_tabIniciada_Pendiente";
var table;
var tableCancelada;

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

    $(".openConfirmar").on("click", function (e) {
        $("#confirmTicket").val($(e.target).data("id"));
        $("#modalConfirmar").modal("show");
    })

    InitStatus(tableName);

    let table = null;
    let tableCancelada = null;
    switch (tableName) {
        case "Iniciada_Pendiente":
            table = $('#tableIniciada_Pendiente').DataTable({
                "searching": true,
                "lengthChange": true,
                "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
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
        case "Completada":
            table = $('#tableCompletada').DataTable({
                "searching": true,
                "lengthChange": true,
                "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
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
        case "Enviada":
            table = $('#tableEnviada').DataTable({
                "searching": true,
                "lengthChange": true,
                "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
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
        case "Despachada":
            table = $('#tableDespachada').DataTable({
                "searching": true,
                "lengthChange": true,
                "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
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
        case "Revisada":
            table = $('#tableRevisada').DataTable({
                "searching": true,
                "lengthChange": true,
                "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
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
        case "Entregada":
            table = $('#tableEntregada').DataTable({
                "searching": true,
                "lengthChange": true,
                "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
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
        case "Cancelada":
            tableCancelada = $('#tableCancelada').DataTable({
                "searching": true,
                "lengthChange": true,
                "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
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
        case "All":
            table = $('#tableAll').DataTable({
                "searching": true,
                "lengthChange": true,
                "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
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
        $(`#table${tableName}_filter`).hide();

    }
    else if (tableCancelada) {
        $(".searchColumnCanceladas").on('keyup', function (e) {
            var i = $(e.target).data('column');
            tableCancelada.column(i).search("(" + $(e.target).val() + ")", true, true).draw();
        });
        $('#searchcanceladas').on('keyup change', function (e) {
            tableCancelada.search($(e.target).val()).draw();
        })
    }
}

function InitStatus(tableName) {
    $(function () {
        $('.status').editable({
            source: function (p) {
                var s = [];
                if (agencyId == agencyRapidM || agencyId == agencyEnvieConFe) {
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
                            "8": "Entregada"
                        },
                        {//Estado Revisada
                            "7": 'Revisada',
                            "8": "Entregada"
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


                return s[$(this).data('value') - 1];

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
    });
}

async function getTableByName(tableName, searchPqtInServer, searchInServer) {
    let search = "";
    if (searchInServer) {
        search = $("#search").val();
    }
    let searchPqt = "";
    if (searchPqtInServer) {
        searchPqt = $("#search_pqt").val();
    }
    const baseTabId = `#base_tab${tableName}`;
    let qty = $(`#months`).val();
    if (tableName != "Cancelada") {
        $(baseTabId).block(blockOptions);
        $(baseTabId).parent().addClass("disabled");
    }

    return await $.get(`/OrderNew/GetTable?type=${tableName}&search_pqt=${searchPqt}&search=${search}&qty=${qty}&typeData=1`, (res) => {
        const tab = $("#div_" + tableName);
        tables[tableName] = res;
        $(tab).html(res);
        initTable(tableName);
        $(baseTabId).unblock();
        $(baseTabId).parent().removeClass("disabled");
    });
}

$(document).ready(function () {
    $('[name="tab"]').on('click', function () {
        $('[name="tab"]').removeClass('active');
        $(this).addClass('active');

    });

    $(document).on('click', '.confirm', function () {
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

    /****Cargar tablas****/
    /*$(".tablePartial").on("click", (e) => {
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
    });*/
});

$(window).on("load", () => {
    //var prevTab = $("#" + currentTab);
    //var prevContainer = $(prevTab).data("table");
    
    //var block_ele = $("#" + prevContainer + "_div");
    //$(block_ele).block(blockOptions);

    /*allTables.map(async tableName => {
        await getTableByName(tableName);
    });*/

    //$(block_ele).unblock();

    $('#months').val(1).trigger('change');
})

/*$('[name="qty"]').on('keyup', function(e){
    if(e.keyCode == 13){
        const tableName = $(this).data("table");
        getTableByName(tableName);
    }
})*/

$('#months').on('change', function(e){
    allTables.map(async tableName => {
        await getTableByName(tableName);
    });
})

$('#searchServer').on('click', function () {
    allTables.map(async tableName => {
        await getTableByName(tableName,false,true);
    });
})

$("#search_pqt").on("keypress", function (e) {
    if (e.which == 13) {
        allTables.map(async tableName => {
            await getTableByName(tableName, true, false);
        });
    }
})

$("#search_pqt_btn").on("click", function () {
    if (e.which == 13) {
        allTables.map(async tableName => {
            await getTableByName(tableName, true, false);
        });
    }
})
