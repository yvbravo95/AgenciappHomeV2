$(document).ready(function () {

    var urlGetNumber = "/Passport/GetSelectPassport";
    var typeServCheckedReport = "pasaporte";

    const initSelectOrder = () => {
        $(".selectOrderNumber").select2({
            placeholder: "Buscar trámite",
            dropdownParent: $('#modalUtilidadDCuba'),
            val: null,
            ajax: {
                type: 'POST',
                url: urlGetNumber,
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
                            return { id: obj.orderNumber, text: obj.orderNumber };
                        })
                    };
                }
            }
        });
    }
    initSelectOrder();

    function showDataUtilityAndSales(data, today) {
        let totalSales = 0;
        let tabSales = $('#tab1');
        if (!today)
            tabSales = $('#tab2')
        for (var i = 0; i < data.sales.length; i++) {
            var aux = data.sales[i];
            $(tabSales).append(
                '<a style="padding: 5px;" href="#" class="list-group-item list-group-item-action media no-border">' +
                '<div class= "media-body" >' +
                '<h6 class="list-group-item-heading">' + aux.elem1 + '<span class="font-medium-4 float-xs-right ">$ ' + aux.elem2.toFixed(2) + '</span></h6>' +
                '</div >' +
                '</a >'
            );
            totalSales += parseFloat(aux.elem2);
        }
        $(tabSales).append(
            '<a style="padding: 5px;" href="#" class="list-group-item list-group-item-action media no-border">' +
            '<div class="media-body">' +
            '<h6 class="list-group-item-heading"> <b>Total:</b> <span class="font-medium-4 float-xs-right "><b>$ ' + totalSales.toFixed(2) + '</b></span></h6>' +
            '</div>' +
            '</a>'
        );

        //Utilidad 
        let totalUtility = 0;
        let tabUtility = $('#tab1utilidad');
        if (!today)
            tabUtility = $('#tab2utilidad')
        for (var i = 0; i < data.utility.length; i++) {
            var aux = data.utility[i];
            $(tabUtility).append(
                '<a style="padding: 5px;" href="#" class="list-group-item list-group-item-action media no-border">' +
                '<div class= "media-body" >' +
                '<h6 class="list-group-item-heading">' + aux.elem1 + '<span class="font-medium-4 float-xs-right ">$ ' + aux.elem2.toFixed(2) + '</span></h6>' +
                '</div >' +
                '</a >'
            );

            totalUtility += parseFloat(aux.elem2);
        }
        //Total Ventas Hoy
        $(tabUtility).append(
            '<a style="padding: 5px;" href="#" class="list-group-item list-group-item-action media no-border">' +
            '<div class="media-body">' +
            '<h6 class="list-group-item-heading"> <b>Total:</b> <span class="font-medium-4 float-xs-right "><b>$ ' + totalUtility.toFixed(2) + '</b></span></h6>' +
            '</div>' +
            '</a>'
        );
    }

    function showDataSettlement(data, today) {
        let total = 0;
        let tab = $('#tab1empleado');
        let type = "hoy";
        if (!today)
        {
            tab = $('#tab2empleado');
            type = "ayer";
        }
        //Liquidacion
        for (var i = 0; i < data.length; i++) {
            var aux = data[i];
            $(tab).append(
                '<a style="padding: 5px;" href="#" name="exportempleado" data-type="' + type + '" data-id="' + aux.elem1.elem1 + '" class="list-group-item list-group-item-action media no-border">' +
                '<div class="media-body">' +
                '<h6 class="list-group-item-heading"> ' + aux.elem1.elem2 + ' <span class="font-medium-4 float-xs-right ">$ ' + aux.elem2.toFixed(2) + '</span></h6>' +
                '</div>' +
                '</a>'
            );

            total += parseFloat(aux.elem2);
        }
        $(tab).append(
            '<a style="padding: 5px;" href="#" class="list-group-item list-group-item-action media no-border">' +
            '<div class="media-body">' +
            '<h6 class="list-group-item-heading"> <b>Total:</b> <span class="font-medium-4 float-xs-right "><b>$ ' + total.toFixed(2) + '</b></span></h6>' +
            '</div>' +
            '</a>'
        );
    }

    var urlSettlement = domainReports + "api/v1/report/SettlementSummary?userId=" + userId;
    var urlSales = domainReports + "api/v1/report/SalesAndUtilitySummary?userId=" + userId;

    function LoadDataSettlement(today) {

        $.ajax({
            async: true,
            type: "GET",
            contentType: "application/x-www-form-urlencoded",
            url: today ? urlSettlement + "&today=true" : urlSettlement + "&today=false",
            data: {
            },
            beforeSend: function () {

            },
            success: function (response) {
                showDataSettlement(response, today);
            },
            error: function () {
                toastr.error("No se han podido obtener los datos", "Error");
            }
        });
    }

    function LoadDataUtilityAndSales(today) {

        $.ajax({
            async: true,
            type: "GET",
            contentType: "application/x-www-form-urlencoded",
            url: today ? urlSales + "&isMonth=false" : urlSales + "&isMonth=true",
            data: {
            },
            beforeSend: function () {

            },
            success: function (response) {
                showDataUtilityAndSales(response, today);
            },
            error: function () {
                toastr.error("No se han podido obtener los datos", "Error");
            }
        });
    }

    LoadDataSettlement(true);
    LoadDataUtilityAndSales(true);

    let isLoadSalesAndUtility = false;
    $('#base-tab2utilidad, #base-tab2').on('click', function () {
        if (!isLoadSalesAndUtility) {
            LoadDataUtilityAndSales(false);
        }
        isLoadSalesAndUtility = true;
    })
    let isLoadSettlement = false;
    $('#base-tab2empleado').on('click', function () {
        if (!isLoadSettlement) {
            LoadDataSettlement(false);
        }
        isLoadSettlement = true;
    })

    $('#dateLiquidacion').on('change', function () {
        var date = $(this).val();

        var url = "/Home/getLiquidacionByDate?date="
        /*if (agencyId == agencyRapidM) {
            url = "/Home/getLiquidacionByDateRapid?date=";
        }*/

        $.ajax({
            async: true,
            type: "GET",
            contentType: "application/x-www-form-urlencoded",
            url: url + date,
            beforeSend: function () {
                $('#data_tab3empleado').html("");
                $('#tab3empleado').block({
                    message: '<div class="ft-refresh-cw icon-spin font-medium-2"></div>',
                    timeout: null, //unblock after 2 seconds
                    overlayCSS: {
                        backgroundColor: '#fff',
                        opacity: 0.8,
                        cursor: 'no-drop'
                    },
                    css: {
                        border: 0,
                        padding: 0,
                        backgroundColor: 'transparent'
                    }
                });
            },
            success: function (data) {
                for (var i = 0; i < data.liquidacionHoy.length; i++) {
                    var aux = data.liquidacionHoy[i];
                    $('#data_tab3empleado').append(
                        '<a style="padding: 5px;" href="#" name="exportempleado" data-type="fecha" data-id="' + aux.elem1.elem1 + '" class="list-group-item list-group-item-action media no-border">' +
                        '<div class="media-body">' +
                        '<h6 class="list-group-item-heading"> ' + aux.elem1.elem2 + ' <span class="font-medium-4 float-xs-right ">$ ' + aux.elem2 + '</span></h6>' +
                        '</div>' +
                        '</a>'
                    );
                }
                $('#data_tab3empleado').append(
                    '<a style="padding: 5px;" href="#" class="list-group-item list-group-item-action media no-border">' +
                    '<div class="media-body">' +
                    '<h6 class="list-group-item-heading"> <b>Total:</b> <span class="font-medium-4 float-xs-right "><b>$ ' + data.totalLiquidacionHoy + '</b></span></h6>' +
                    '</div>' +
                    '</a>'
                );

                $('#tab3empleado').unblock();
            },
            error: function () {
                toastr.error("No se han podido obtener los datos", "Error");
                $('#tab3empleado').unblock();
            },
            timeout: 60000,
        });
    })

    $('#reporteOrderNumber').on('click', function () {
        $('#modalUtilidadDCuba').modal('show');
    })

    $('[name="checkExportByNumber"]').on('click', function () {
        $('#orderNumber1').val(null).trigger('change');
        $('#orderNumber2').val(null).trigger('change');

        typeServCheckedReport = $(this).val();
        if (typeServCheckedReport == "pasaporte")
            urlGetNumber = "/Passport/GetSelectPassport";
        else if (typeServCheckedReport == "servicios")
            urlGetNumber = "/Servicios/GetSelectServicio";

        initSelectOrder();
    })

    $('#btnReportOrderNumber').on('click', function () {
        var number1 = $('#orderNumber1').val();
        var number2 = $('#orderNumber2').val();
        if (number1 == null || number2 == null) {
            toastr.warning("Debe especificar el valor inicial y final");
            return false;
        }
        var url = "/Home/UtilityDCubaByNumber";
        if (typeServCheckedReport == "servicios")
            url = "/Home/UtilityOtrosServiciosByNumber";

        const onlyClientsAgency = $('#onlyClientsAgency').is(':checked');

        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: url,
            data: {
                number1: number1,
                number2: number2,
                onlyClientsAgency: onlyClientsAgency
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
                    fileName = 'document.pdf';
                    var byteCharacters = atob(response.data);
                    var byteNumber = new Array(byteCharacters.length);
                    for (var i = 0; i < byteCharacters.length; i++) {
                        byteNumber[i] = byteCharacters.charCodeAt(i);
                    }
                    var byteArray = new Uint8Array(byteNumber);
                    var blob = new Blob([byteArray], { type: 'application/pdf' });
                    var fileURL = URL.createObjectURL(blob);
                    window.open(fileURL);
                }
                else {
                    toastr.error(response.msg);
                }

                $.unblockUI();
            },
            error: function () {
                toastr.error("No se ha podido exportar", "Error");
                $.unblockUI();
            },
        });
    })

    var dCubaWashington = "4752B08A-7684-42B3-930D-FF86F496DF2F";
    var dCubaHouston = "4E50B1EF-0EC1-4383-BCEE-3098744CDDD0";
    var dCubaDallas = "4F1DDEF5-0592-46AD-BEEE-3316CB84385B";
    var dataBox = async () => {
        var data = await $.get("/Home/GetDataBoxDCuba");
        if (data.success) {
            $('#utilityProrroga').html(Math.round(data.data.utilityProrrogas, 2));
            $('#utilityRenovar').html(Math.round(data.data.utilityRenovar, 2));
            $('#utilityPrimerVez').html(Math.round(data.data.utilityPrimerVez, 2));
            $('#utilityTotal').html(Math.round(data.data.utilityProrrogas + data.data.utilityRenovar + data.data.utilityPrimerVez, 2));

            $('#totalPrimerVez').html(Math.round(data.data.primerVezThisMonth, 2));
            if (data.data.primerVezDiffLastMonth >= 0)
                $('#difPrimerVez').html('<i style="font-size:14px;margin-left:10px;" class="ft-plus">' + Math.round(data.data.primerVezDiffLastMonth, 2) + '</i>');
            else
                $('#difPrimerVez').html('<i style="font-size:14px;margin-left:10px;" class="ft-minus">' + Math.abs(Math.round(data.data.primerVezDiffLastMonth, 2)) + '</i>');

            $('#totalProrroga').html(Math.round(data.data.prorrogasThisMonth, 2));
            if (data.data.prorrogasDiffLastMonth >= 0)
                $('#difProrroga').html('<i style="font-size:14px;margin-left:10px;" class="ft-plus">' + Math.round(data.data.prorrogasDiffLastMonth, 2) + '</i>');
            else
                $('#difProrroga').html('<i style="font-size:14px;margin-left:10px;" class="ft-minus">' + Math.abs(Math.round(data.data.prorrogasDiffLastMonth, 2)) + '</i>');

            $('#totalRenovar').html(data.data.renovarThisMonth);
            if (data.data.renovarDiffLastMonth >= 0)
                $('#difRenovar').html('<i style="font-size:14px;margin-left:10px;" class="ft-plus">' + Math.round(data.data.renovarDiffLastMonth, 2) + '</i>');
            else
                $('#difRenovar').html('<i style="font-size:14px;margin-left:10px;" class="ft-minus">' + Math.abs(Math.round(data.data.renovarDiffLastMonth, 2)) + '</i>');

            $('#totalClientes').html(data.data.clientsTotal);
            $('#totalClientesMes').html(data.data.clientsThisMonth);
        }
        else {
            toast.error("No se han podido obtener los datos");
        }
    }

    var agencyCubiq = "30C11DB5-210F-4D87-9C94-F53864BD8240";
    if (agencyId == dCubaHouston.toLowerCase() || agencyId == dCubaWashington.toLowerCase() || dCubaDallas.toLowerCase())
        dataBox();

    if (agencyId == agencyCubiq.toLocaleLowerCase()) {
        const updateBoxCubiq = async () => {
            var data = await $.get("/Home/GetGuiasCubiqOpen");
            if (data.success) {
                if (data.dataGuias != null) {
                    var elements = $('[name="element-guia"]');
                    data.dataGuias.forEach(element => {
                        for (let index = 0; index < elements.length; index++) {
                            var elemDom = elements[index];
                            if ($(elemDom).attr('id') == element.guiaAereaId) {
                                var media = $(elemDom).find('.media');
                                $($(media).children()[0]).html(element.noGuia + " (" + element.type + ")");
                                $($($(media).children()[1]).children()[0]).html(element.bultos);
                                $($($(media).children()[2]).children()[0]).html(element.pesoKg);
                            }
                        }
                    });
                }
                //if (data.data != null) {
                //    $('#cantPaqutesCubiq').html(data.data.cantPaquetes);
                //    $('#pesoKgCubiq').html(data.data.cantKg);
                //}

                setTimeout(updateBoxCubiq, 60000);
            }
        }

        updateBoxCubiq();
    }

    $('#reporterangofecha').on('click', function () {
        $('#modalreporte').modal('show');
    });
    $('#reporterangofechaRapid').on('click', function () {
        $('#modalreporteRapid').modal('show');
    });
    $('#reporterangofechautilidad').on('click', function () {
        $('#modalreporteutilidad').modal('show');
    });
    $('#reporterangofechautilidadRapid').on('click', function () {
        $('#modalreporteutilidadRapid').modal('show');
    });
    $('#rpteByDateUtilityExcel').on('click', function () {
        $('#modalreporteutilidadExcel').modal('show');
    });
    $('#rpteByDateUtilityExcel2').on('click', function () {
        $('#modalreporteutilidadExcel2').modal('show');
    });
    $('#reporterangofechaempleado').on('click', function () {
        $('#modalreporteempleado').modal('show');
    });

    $('#reporteutilidadMPS').on('click', function () {
        $('#modalreporteutilidadExcelMPS').modal('show');
    });
    $('#reporteutilidadMPS2').on('click', function () {
        $('#modalreporteutilidadExcelMPS2').modal('show');
    });

    $('[name="tablink"]').on('click', function () {
        $('[name="tablink"]').removeClass('active');
        $(this).addClass('active');

    });
    $('[name="tablinkutilidad"]').on('click', function () {
        $('[name="tablinkutilidad"]').removeClass('active');
        $(this).addClass('active');

    });
    $('[name="tablinkempleado"]').on('click', function () {
        $('[name="tablinkempleado"]').removeClass('active');
        $(this).addClass('active');

    });

    $("#gen_report").click(function () {
        var id = $('[name="tabpaneventas"][class="tab-pane active"]').attr('id');
        console.log(id);
        var date = "";
        var hoy = new Date();

        if (id == "tab1") {
            //Fecha hoy
            var dd = hoy.getDate();
            var mm = hoy.getMonth() + 1;
            var yyyy = hoy.getFullYear();
            date = addZero(mm) + "/" + addZero(dd) + "/" + yyyy + " - " + addZero(mm) + "/" + addZero(dd) + "/" + yyyy;
        }
        else {
            var ddFin = hoy.getDate();
            var ddInit = 1;
            var mm = hoy.getMonth() + 1;
            var yyyy = hoy.getFullYear();
            date = addZero(mm) + "/" + addZero(ddInit) + "/" + yyyy + " - " + addZero(mm) + "/" + addZero(ddFin) + "/" + yyyy;
            //Fecha ayer
        }
        var url = "/Home/ExportVentas";
        /*if (agencyId === agencyRapidM)
            url = "/Home/ExportVentasRapid";*/

        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: url,
            data: {
                strdate: date
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
            timeout: 240000,
        });
    });
    // Reporteventas por rango de fecha
    $('#reportefecha').on('click', function () {
        var date = $('#daterangeReport').val();
        var url = "/Home/ExportVentas";
        /*if (agencyId === agencyRapidM) {
            url = "/Home/ExportVentasRapid";
        }*/

        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: url,
            data: {
                strdate: date
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
            timeout: 240000,
        });

    })

    $('#reportefechaRapid').on('click', function () {
        var date = $('#daterangeReportRapid').val();
        var url = "/Home/ExportVentas";
        if (agencyId === agencyRapidM) {
            url = "/Home/ExportVentasRapid";
        }

        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: url,
            data: {
                strdate: date
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
            timeout: 240000,
        });

    })


    $("#gen_reportutilidad").click(function () {
        var id = $('[name="tabpaneutilidad"][class="tab-pane active"]').attr('id');
        var date = "";
        var hoy = new Date();
        const onlyClientsAgency = $('#onlyClientsAgency').is(':checked');

        if (id == "tab1utilidad") {
            //Fecha hoy
            var dd = hoy.getDate();
            var mm = hoy.getMonth() + 1;
            var yyyy = hoy.getFullYear();
            date = addZero(mm) + "/" + addZero(dd) + "/" + yyyy + " - " + addZero(mm) + "/" + addZero(dd) + "/" + yyyy;
        }
        else {
            var dd = hoy.getDate();
            var ddInit = 1;
            var mm = hoy.getMonth() + 1;
            var yyyy = hoy.getFullYear();
            date = addZero(mm) + "/" + addZero(ddInit) + "/" + yyyy + " - " + addZero(mm) + "/" + addZero(dd) + "/" + yyyy;
            //Fecha ayer
        }
        var url = "/Home/ExportUtilidad";
        /*if (agencyId === agencyRapidM) {
            url = "/Home/ExportUtilidadRapid";
        }*/

        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: url,
            data: {
                strdate: date,
                onlyClientsAgency: onlyClientsAgency
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
            timeout: 240000,
        });
    });
    // Reporteventas por rango de fecha utilidad
    $('#reportefechaUtilidad').on('click', function () {
        var date = $('#daterangeReportUtilidad').val();
        const onlyClientsAgency = $('#onlyClientsAgency').is(':checked');

        var url = "/Home/ExportUtilidad";
        /*if (agencyId === agencyRapidM) {
            url = "/Home/ExportUtilidadRapid";
        }*/

        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: url,
            data: {
                strdate: date,
                onlyClientsAgency: onlyClientsAgency
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
            timeout: 240000,
        });

    })

    $('#reportefechaUtilidadRapid').on('click', function () {
        var date = $('#daterangeReportUtilidadRapid').val();
        const onlyClientsAgency = $('#onlyClientsAgencyRapid').is(':checked');

        var url = "/Home/ExportUtilidad";
        if (agencyId === agencyRapidM) {
            url = "/Home/ExportUtilidadRapid";
        }

        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: url,
            data: {
                strdate: date,
                onlyClientsAgency: onlyClientsAgency
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
            timeout: 240000,
        });

    })

    $('#reportefechaUtilidadExcel').on('click', function () {
        var date = $('#daterangeReportUtilidadExcel').val();
        const onlyClientsAgency = $('#onlyClientsAgency').is(':checked');

        window.open("/Home/ExportUtilidadExcel?strdate=" + date + "&onlyClientsAgency=" + onlyClientsAgency, 'Utilidad');
    })

    $('#reportefechaUtilidadExcelMPS').on('click', function () {
        var date = $('#daterangeReportUtilidadExcelMPS').val();
        window.open("/Home/ExportUtilidadExcelMPS?strdate=" + date + "&isDC=False", 'Utilidad');
    })
    $('#reportefechaUtilidadExcelMPS2').on('click', function () {
        var date = $('#daterangeReportUtilidadExcelMPS2').val();
        window.open("/Home/ExportUtilidadExcelMPS?strdate=" + date + "&isDC=True", 'Utilidad');
    })

    $('#reportefechaUtilidadExcel2').on('click', function () {
        var date = $('#daterangeReportUtilidadExcel2').val();
        const onlyClientsAgency = $('#onlyClientsAgency').is(':checked');

        window.open("/Home/ExportUtilidadExcel2?strdate=" + date + "&onlyClientsAgency=" + onlyClientsAgency, 'Utilidad');
    })


    //Reporte de venta de un empleado
    $(document).on('click', '[name="exportempleado"]', async function () {
        var idempleado = $(this).attr('data-id');
        var datatype = $(this).attr('data-type');

        var date = "";
        var hoy = new Date();
        if (datatype == "fecha") {
            var aux = $('#dateLiquidacion').val().replace("-", "/").replace("-", "/").replace("-", "/");
            date = aux + " - " + aux;
        }
        else if (datatype == "hoy") {
            //Fecha hoy
            var dd = hoy.getDate();
            var mm = hoy.getMonth() + 1;
            var yyyy = hoy.getFullYear();
            date = addZero(mm) + "/" + addZero(dd) + "/" + yyyy + " - " + addZero(mm) + "/" + addZero(dd) + "/" + yyyy;
        }
        else {
            hoy.setDate(hoy.getDate() - 1);
            var dd = hoy.getDate();
            var mm = hoy.getMonth() + 1;
            var yyyy = hoy.getFullYear();
            date = addZero(mm) + "/" + addZero(dd) + "/" + yyyy + " - " + addZero(mm) + "/" + addZero(dd) + "/" + yyyy;
            //Fecha ayer
        }

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

        const data = await $.get(domainReports + "api/v1/report/liquidacion/" + idempleado + "?rangeDate=" + date);

        if (data != "") {
            fileName = 'document.pdf';
            var byteCharacters = atob(data);
            var byteNumber = new Array(byteCharacters.length);
            for (var i = 0; i < byteCharacters.length; i++) {
                byteNumber[i] = byteCharacters.charCodeAt(i);
            }
            var byteArray = new Uint8Array(byteNumber);
            var blob = new Blob([byteArray], { type: 'application/pdf' });
            var fileURL = URL.createObjectURL(blob);
            window.open(fileURL);
        }

        $.unblockUI();

        //var url = "/Home/ExportVentasEmpleado";
        /*if (agencyId === agencyRapidM) {
            url = "/Home/ExportVentasEmpleadoRapid";
        }*/

        /*$.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: url,
            data: {
                strdate: date,
                idempleado: idempleado
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
            timeout: 540000,
        });*/
    });

    // Reporte de todos los empleados
    $("#gen_reportempleado").click(function () {
        var id = $('[name="tabpaneempleado"][class="tab-pane active"]').attr('id');
        var date = "";
        var hoy = new Date();
        if (id == "tab1empleado") {
            //Fecha hoy
            var dd = hoy.getDate();
            var mm = hoy.getMonth() + 1;
            var yyyy = hoy.getFullYear();
            date = addZero(mm) + "/" + addZero(dd) + "/" + yyyy + " - " + addZero(mm) + "/" + addZero(dd) + "/" + yyyy;
        }
        else {
            hoy.setDate(hoy.getDate() - 1);
            var dd = hoy.getDate();
            var mm = hoy.getMonth() + 1;
            var yyyy = hoy.getFullYear();
            date = addZero(mm) + "/" + addZero(dd) + "/" + yyyy + " - " + addZero(mm) + "/" + addZero(dd) + "/" + yyyy;
            //Fecha ayer
        }

        var url = "/Home/PDFVentasporempleado";
        /*if (agencyId === agencyRapidM) {
            url = "/Home/PDFVentasporempleadoRapid";
        }*/

        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: url,
            data: {
                strdate: date
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
            timeout: 240000,
        });
    });
    // Reporteventas por rango de fecha utilidad
    $('#reportefechaEmpleado').on('click', function () {
        var date = $('#daterangeReportEmpleado').val();

        var url = "/Home/PDFVentasporempleado";
        /*if (agencyId === agencyRapidM) {
            url = "/Home/PDFVentasporempleadoRapid";
        }*/

        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: url,
            data: {
                strdate: date
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
            timeout: 240000,
        });

    })

    $('#reportePendientes').on('click', function () {
        $('#modalreportependientes').modal('show');
    })

    $('#btnreportefechapendientes').on('click', async function () {
        const date = $('#fechareportependientes').val();
        const idempleado = $(this).attr('data-id');
        $.blockUI()
        const data = await $.get(domainReports + "api/v1/report/pendings/" + userId + "?rangeDate=" + date);

        if (data) {
            fileName = 'pendientes.pdf';
            var byteCharacters = atob(data);
            var byteNumber = new Array(byteCharacters.length);
            for (var i = 0; i < byteCharacters.length; i++) {
                byteNumber[i] = byteCharacters.charCodeAt(i);
            }
            var byteArray = new Uint8Array(byteNumber);
            var blob = new Blob([byteArray], { type: 'application/pdf' });
            var fileURL = URL.createObjectURL(blob);
            window.open(fileURL);
        }
        $.unblockUI();
    })

    function addZero(i) {
        if (i < 10) {
            i = '0' + i;
        }
        return i;
    }
});