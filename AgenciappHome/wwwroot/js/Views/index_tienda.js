$(document).ready(function () {


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

    //funcion para actualizar la cantidad a despachar
    function updatedespacho() {
        if (cantSelect > 0) {
            $('#numerodespachos').html('(' + cantSelect + ')');
            $('#numerodespachos').show();
            $('#despachar').show();
        }
        else {
            $('#numerodespachos').hide();
            $('#despachar').hide();
        }
    }

    $(".order-select").on("change", function () {
        if ($(this).val() != "all")
        {
            // Para desabilitar el check todo si se selecciona algun otro check
            $('#checkalltramites').prop('checked', false);

            if ($(this)[0].checked) {
                cantSelect++;
                //selectedIds.push($(this).val());
            } else {
                cantSelect--;
                //selectedIds.splice($.inArray($(this).val(), selectedIds), 1);
            }
            var tabactive = $('[name = "tab"][class = "nav-link active"]');
            if (/*selectedIds.length*/ cantSelect == 0) {
                $("#gen_report").addClass("hidden");
                //Si el tab es completadas
                if (tabactive.attr('data-type') == "iniciadas/pendientes") {
                    $('#despachar').hide();
                }
            } else {
                $("#gen_report").removeClass("hidden");
                //Si el tab es completadas
                if (tabactive.attr('data-type') == "iniciadas/pendientes") {
                    $('#despachar').show();
                }
            }
        }
        updatedespacho(); //Para actualizar la cantidad de tramites a despachar
    });

    $("#gen_report").click(function () {

        selectedIds = new Array;

        $(".order-select").each(function (i, e) {
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

    $(".print_report").click(function () {
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
            if ($(this).attr('data-type') == "iniciadas/pendientes") {
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
        var emails = $('#emails').val();
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/ordernew/despacharTienda",
            data: {
                ids: selectedIds,
                emails: emails,
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
                var aux = response.msg.split('-');
                if (aux[0] == "error") {
                    toastr.error(aux[1]);
                    $.unblockUI();
                }
                else {
                    fileName = 'Despacho Tienda.pdf';
                    var byteCharacters = atob(response.data);
                    var byteNumber = new Array(byteCharacters.length);
                    for (var i = 0; i < byteCharacters.length; i++) {
                        byteNumber[i] = byteCharacters.charCodeAt(i);
                    }
                    var byteArray = new Uint8Array(byteNumber);
                    var blob = new Blob([byteArray], { type: 'application/pdf' });
                    var fileURL = URL.createObjectURL(blob);
                    window.open(fileURL);
                    location.href = "/OrderNew/tiendas?msg=" + response;
                }
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

    /*
    // Para el espacho
    $('#mayorista').select2({
        placeholder: "Mayorista a despachar",
        val: null,
    });
    $('#tipotramite').select2({
        placeholder: "Trámite a Despachar",
        val: null,
    });

    //Muestro los mayoristas según el tipo de trámite
    $('#tipotramite').on('change', function () {
        if ($(this).val() == "Combos") {
            getMayoristas("Combos");
        }
        else {
            getMayoristas("Paquete Aereo");
        }
    });

    function getMayoristas(category) {
        $.ajax({
            type: "POST",
            url: "/OrderNew/getMayoristas",
            data: {
                category: category,
            },
            async: false,
            success: function (data) {
                idMayoristabyTransferencia = "";
                $("#mayorista").val("").trigger("change");
                $("#mayorista").empty();
                $("#mayorista").append(new Option("Mayorista a Despachar", "", true));

                if (data.length != 0) {

                    for (var i = 0; i < data.length; i++) {
                        $("#mayorista").append(new Option(data[i].name, data[i].idWholesaler));
                    }

                }
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    }

   



    $("#creardespacho").click(function () {
        var date = $('#daterangeReport').val();
        var tipotramite = $('#tipotramite').val();
        var mayorista = $('#mayorista').val();
        if (tipotramite != "" && mayorista != "") {
            $.ajax({
                async: true,
                type: "POST",
                contentType: "application/x-www-form-urlencoded",
                url: "/ordernew/despachar",
                data: {
                    date: date,
                    numerodespacho: numerodespacho,
                    idmayorista: mayorista
                },
                success: function (response) {
                    var aux = response.split('-');
                    if (aux[0] == "error") {
                        toastr.error(aux[1]);
                    }
                    else {
                        location.reload();
                    }
                },
                error: function () {
                    toastr.error("No se ha podido despachar", "Error");
                },
                timeout: 10000,
            });
        }
        else {
            toastr.warning("Verifique que los campos tipo de trámite y mayorista no estén vacíos");
        }

    });
    */
});