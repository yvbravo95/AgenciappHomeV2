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
            $('#btnDespachar').show();
            if (nameTable == "tableInitPendOrders") {
                $('#despachar').show();
            }
        }
        else {
            $('#numerodespachos').hide();
            $('#despachar').hide();
            $('#btnDespachar').hide();

        }
    }

    $(window).on("load", () => {
        loadTables('false');
    })

    $("#showallbtn").on("click", () => {
        $("#all_div").html("");
        loadTables(true);
    })

    const initTable = () => {
        $('.dropdown-toggle').dropdown();

        var credito = false;
        $.fn.editable.defaults.mode = 'inline';
        $(function () {
            $('.status').editable({
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
                            //"3": 'Cancelada',
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
        });

        $('table tfoot th').each(function () {
            var title = $(this).text();
            if (title != "") {
                $(this).html('<input type="text" class="input-sm" placeholder="' + title + '" />');
            }
        });


        $(".order-select").on("change", function () {
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

        $("#searchField").on("keyup", searchAction);
    }

    const loadTables = (all) => {
        var allTables = ["Despachadas", "Entregadas", "Canceladas"]
        allTables.map(tableName => {
            var block_ele = $("#div_" + tableName);

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

            $.get("/EmpleadoCuba/GetCombos?tableType=" + tableName + '&showAll=' + all, (res) => {
                if (tableName == 'Canceladas') {
                    $('#div_Canceladas').html(res)
                    $(block_ele).unblock();

                    $('.dropdown-toggle').dropdown();

                    var tableCanceladas = tableCanceladas = $('#tableCanceladas').DataTable({
                        "searching": true,
                        "lengthChange": true,
                        "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
                        //"scrollX": true,
                        //"order": [[1, "desc"]],
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
                        initComplete: function () {
                            // Apply the search
                            this.api().columns().every(function () {
                                var that = this;

                                $('input', this.footer()).on('keyup change clear', function () {
                                    if (that.search() !== this.value) {
                                        that
                                            .search(this.value)
                                            .draw();
                                    }
                                });
                            });
                        }
                    });
                    $('#tableCanceladas_filter').hide();

                    $("#searchCanceladas").on('keyup change', function () {
                        tableCanceladas.search($(this).val()).draw();
                    });
                }
                else if (tableName == 'Despachadas') {
                    $('#div_Despachadas').html(res)

                    $(block_ele).unblock();
                    initTable()

                    tableCompletada = $('#tableDespachadaOrders').DataTable({
                        "searching": true,
                        "lengthChange": true,
                        "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
                        //"scrollX": true,
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
                        initComplete: function () {
                            // Apply the search
                            this.api().columns().every(function () {
                                var that = this;

                                $('input', this.footer()).on('keyup change clear', function () {
                                    if (that.search() !== this.value) {
                                        that
                                            .search(this.value)
                                            .draw();
                                    }
                                });
                            });
                        }
                    });
                    $('#tableDespachadaOrders_filter').hide();
                    $('#search').on('keyup change', function () {
                        tableCompletada.search($(this).val()).draw();
                    });

                    $('#containerToggleDespachada .toggle-vis').on('change', function (e) {
                        e.preventDefault();
                        // Get the column API object
                        var column = tableCompletada.column($(this).attr('data-column'));
                        // Toggle the visibility
                        column.visible($(this).prop('checked'));
                    });

                    updateColumnsTableDespachada();
                    function updateColumnsTableDespachada() {
                        $('#containerToggleDespachada .toggle-vis').each(function (index, element) {
                            // Get the column API object
                            var column = tableCompletada.column($(element).attr('data-column'));

                            // Toggle the visibility
                            column.visible($(element).prop('checked'));
                        });
                    }
                }
                else if (tableName == "Entregadas") {
                    $('#div_Entregadas').html(res)
                    $(block_ele).unblock();
                    initTable()

                    tableEntregada = $('#tableEntregadaOrders').DataTable({
                        "searching": true,
                        "lengthChange": true,
                        "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
                        //"scrollX": true,
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
                        initComplete: function () {
                            // Apply the search
                            this.api().columns().every(function () {
                                var that = this;

                                $('input', this.footer()).on('keyup change clear', function () {
                                    if (that.search() !== this.value) {
                                        that
                                            .search(this.value)
                                            .draw();
                                    }
                                });
                            });
                        }
                    });
                    $('#tableEntregadaOrders_filter').hide();
                    $('#search').on('keyup change', function () {
                        tableEntregada.search($(this).val()).draw();
                    });

                    $('#containerToggleEntregada .toggle-vis').on('change', function (e) {
                        e.preventDefault();
                        // Get the column API object
                        var column = tableEntregada.column($(this).attr('data-column'));
                        // Toggle the visibility
                        column.visible($(this).prop('checked'));
                    });

                    updateColumnsTableEntregada();
                    function updateColumnsTableEntregada() {
                        $('#containerToggleEntregada .toggle-vis').each(function (index, element) {
                            // Get the column API object
                            var column = tableEntregada.column($(element).attr('data-column'));

                            // Toggle the visibility
                            column.visible($(element).prop('checked'));
                        });
                    }
                }
            });
        })
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
        nameTable = "tableDespachadaOrders";
        $("#tableDespachadaOrders tr").removeClass("hidden");

        if ($("#tableDespachadaOrders tr").length == 1)
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
        nameTable = "tableEntregadaOrders";
        $("#tableEntregadaOrders tr").removeClass("hidden");

        if ($("#tableEntregadaOrders tr").length == 1)
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


    //Para el despacho
    $('#despachar').on('click', function () {
        $('#modalDespachar').modal('show');
        findMayoristas();
    });

    // Para mostrar el boton de despacho al seleccionarse las iniciadas
    $('[name="tab"]').on('click', function () {
        if ($(this).attr('data-type') == "iniciadas/pendientes") {
            $('#despachar').show();
        }
        else {
            $('#despachar').hide();
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

    $("#creardespacho").click(function () {
        //Check del mayorista
        selectedIdsMayorista = new Array;
        $(document).find('[name="checkmayorista"]').each(function (i, e) {
            if (e.checked) {
                if (e.value != "all") {
                    selectedIdsMayorista.push($(e).val());
                }
            }
        });

        //Check de ordenes
        selectedIds = new Array;
        if (cantSelect > 0) {
            $(".order-select").each(function (i, e) {
                if (e.checked) {
                    if (e.value != "all") {
                        selectedIds.push($(e).val());
                    }
                }
            });
        }

        var rangofecha = $('#fechaReporte').val();
        var ischecked = $('#checkrange').is(':checked');
        var emails = $('#emails').val();
        if (selectedIdsMayorista.length != 0) {
            $.ajax({
                async: true,
                type: "POST",
                contentType: "application/x-www-form-urlencoded",
                url: "/ordernew/despachar",
                data: {
                    ids: selectedIdsMayorista,
                    idsorder: selectedIds,
                    emails: emails,
                    rangofecha: rangofecha,
                    ischeked: ischecked
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
        }
        else {
            toastr.warning("No ha elegido mayoristas a despachar");
        }

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

    $("#btnDespachar").on('click', function () {
        $('#modalDespacho').modal('show');
    });
    $('#btnModalDespacho').on('click', function () {
        var distributor = $('#selectDistributor').val();
        var selectedIds = seleccionarIds();
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
                    location.href = "/EmpleadoCuba/Combos?msg=" + response.msg;
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

});