$('#despachar').hide();
$('#entregar').hide();
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
            showOKMessage("Actualización de Estado", "Estatus actualizado a " + orderNumber, { "timeOut": 0, "closeButton": true });
        } else if (msg == "successEdit") {
            var orderNumber = params[1].split("=")[1];
            showOKMessage("Editar Orden", "Orden " + orderNumber + " editada con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successDelete") {
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

    $('#mayorista').select2({
        placeholder: "Mayorista a despachar",
        val: null,
    });

    $('[name="tab"]').on('click', function () {
        $('#despachar').hide();
        $('#entregar').hide();
        if ($(this).attr('data-type') == "iniciadas") {
            $('#despachar').show();
        }
        else if ($(this).attr('data-type') == "despachadas") {
            $('#entregar').show();
        }
    });

    $('#despachar').on('click', function () {
        $('#modalDespachar').modal('show');
    });

    $("#creardespacho").click(function () {
        // ontener id de ordenes seleccionadas en order-select
        var selectedIds = new Array;
        $(".order-select").each(function (i, e) {
            if (e.checked) {
                selectedIds.push($(e).val());
            }
        });

        var date = $('#daterangeReport1').val();
        var numerodespacho = $('#numerodespacho').val();
        var mayorista = $('#mayorista').val();
        if (numerodespacho != "" && mayorista != "") {
            $.ajax({
                async: true,
                type: "POST",
                contentType: "application/x-www-form-urlencoded",
                url: "/EnvioMaritimo/despachar",
                data: {
                    date: date,
                    numerodespacho: numerodespacho,
                    idmayorista: mayorista,
                    ids: selectedIds
                },
                success: function (response) {
                    var aux = response.split('-');
                    if (aux[0] == "error") {
                        toastr.error(aux[1]);
                    }
                    else {
                        window.location = "/EnvioMaritimo";
                    }
                },
                error: function () {
                    toastr.error("No se ha podido despachar", "Error");
                },
                timeout: 10000,
            });
        }
        else {
            toastr.warning("Verifique que los campos número de despacho y mayorista no estén vacíos");

        }

    });

    $('#entregar').on('click', function () {
        // alerta de confirmación
        var selectedIds = new Array;
        $(".order-select").each(function (i, e) {
            if (e.checked) {
                selectedIds.push($(e).val());
            }
        }
        );
        if (selectedIds.length > 0) {
            var okConfirm = function () {
                $.ajax({
                    async: true,
                    type: "POST",
                    contentType: "application/x-www-form-urlencoded",
                    url: "/EnvioMaritimo/EntregarOrdenes",
                    data: {
                        numbers: selectedIds
                    },
                    success: function (response) {
                        if (response == "error") {
                            toastr.error("No se ha podido entregar", "Error");
                        } else {
                            window.location = "/EnvioMaritimo";
                        }
                    },
                    error: function () {
                        toastr.error("No se ha podido entregar", "Error");
                    },
                    timeout: 10000,
                });
            };

            confirmationMsg("¿Está seguro que desea marcar como entregado los trámites?", "", okConfirm);
        }
        else {
            toastr.warning("Seleccione al menos una orden para entregar");
        }
    });



    var tab = 1;
    var nameTable = "tableInitPendOrders";
    var cantSelect = 0;

    $("[data-toggle='tab']").click(function () {
        $("[data-toggle='tab']").removeClass("active");
    });

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
            $('#daterangeReport1').prop('disabled', false);
        } else {
            $('#daterangeReport1').prop('disabled', true);
            $("#gen_report").removeClass("hidden");
        }
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
            url: "/EnvioMaritimo/Report",
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

    $(".print_report").click(function () {
        var orderid = $(this).attr('data-Id');
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/EnvioMaritimo/createFileEnvio",
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
            var urlEntregar = "/EnvioMaritimo/CheckEntregada/" + orderId;
            $.ajax({
                type: "GET",
                url: urlEntregar,
                async: false,
                success: function () {
                    document.location = "/EnvioMaritimo?msg=successEntregada&orderNumber=" + orderNumber;
                }
            });
        };
        confirmationMsg("¿Está seguro que desea marcar como entregado este envío?", "", okConfirm);
    });



    $(".cancelOrder").click(function () {
        var orderId = $(this).attr("Id");
        var orderNumber = $(this).attr("name");
        var okConfirm = function () {
            var urlDelete = "/EnvioMaritimo/Cancel/" + orderId;
            $.ajax({
                type: "GET",
                url: urlDelete,
                async: false,
                success: function () {
                    document.location = "/EnvioMaritimo?msg=successDelete&orderNumber=" + orderNumber;
                }
            });
        };
        getCancelConfirmation(okConfirm);
    });

});