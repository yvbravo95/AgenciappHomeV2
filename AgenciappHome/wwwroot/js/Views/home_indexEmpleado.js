$('#reporterangofecha').on('click', function () {
    $('#modalreporte').modal('show');
});

$('[name="tablink"]').on('click', function () {
    $('[name="tablink"]').removeClass('active');
    $(this).addClass('active');

});

$("#gen_report").click(function () {
    var id = $('[class="tab-pane active"]').attr('id');
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
        hoy.setDate(hoy.getDate() - 1);
        var dd = hoy.getDate();
        var mm = hoy.getMonth() + 1;
        var yyyy = hoy.getFullYear();
        date = addZero(mm) + "/" + addZero(dd) + "/" + yyyy + " - " + addZero(mm) + "/" + addZero(dd) + "/" + yyyy;
        //Fecha ayer
    }

    $.ajax({
        async: true,
        type: "GET",
        contentType: "application/x-www-form-urlencoded",
        url: domainReports + "api/v1/report/liquidacion/" + idempleado + "?rangeDate=" + date,
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
    $.ajax({
        async: true,
        type: "GET",
        contentType: "application/x-www-form-urlencoded",
        url: domainReports + "api/v1/report/liquidacion/" + idempleado + "?rangeDate=" + date,
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

function addZero(i) {
    if (i < 10) {
        i = '0' + i;
    }
    return i;
}

    $('#recent-buyers').perfectScrollbar({
        wheelPropagation: true
    });

if (role == "PrincipalDistributor") {

    $.ajax({
        async: true,
        type: "GET",
        contentType: "application/x-www-form-urlencoded",
        url: "/Home/GetDataOrdersByAgency",
        data: {
        },
        beforeSend: function () {
            $('.container-order-by-agency').block({
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
        success: function (response) {
            console.log(response);
            ShowDataBagsByAgency(response);
            $('.container-order-by-agency').unblock();
        },
        error: function () {
            toastr.error("No se han podido obtener los datos", "Error");
            $('.container-order-by-agency').unblock();
        },
        timeout: 60000,
    });

    $.ajax({
        async: true,
        type: "GET",
        contentType: "application/x-www-form-urlencoded",
        url: "/Home/GetDataOrdersByProvince",
        beforeSend: function () {
            $('.containerData').block({
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
        success: function (response) {
            console.log(response);
            ShowDataBagsByProvince(response);
            $('.containerData').unblock();
        },
        error: function () {
            toastr.error("No se han podido obtener los datos", "Error");
            $('.containerData').unblock();
        },
        timeout: 60000,
    });
}

function ShowDataBagsByAgency(data) {
    BuildAddDataByProvince($('#tab1'), data.today);
    BuildAddDataByProvince($('#tab2'), data.month);
}

function ShowDataBagsByProvince(data) {
    BuildAddDataByProvince($('#despachada1'), data.today.dispatched);
    BuildAddDataByProvince($('#despachada2'), data.month.dispatched);

    BuildAddDataByProvince($('#recibida1'), data.today.received);
    BuildAddDataByProvince($('#recibida2'), data.month.received);

    BuildAddDataByProvince($('#entregada1'), data.today.delivered);
    BuildAddDataByProvince($('#entregada2'), data.month.delivered);
}

function BuildAddDataByProvince(element, data) {
    var total = 0;
    for (var i = 0; i < data.length; i++) {
        const item = data[i];
        total += item.obj2;
        $(element).append(
            '<a style="padding: 5px;" href="#" class="list-group-item list-group-item-action media no-border">' +
            '<div class= "media-body" >' +
            '<h6 class="list-group-item-heading">' + item.obj1 + '<span class="font-medium-4 float-xs-right "> ' + item.obj2 + '</span></h6>' +
            '</div >' +
            '</a >'
        );
    }
    $(element).append(
        '<a style="padding: 5px;" href="#" class="list-group-item list-group-item-action media no-border">' +
        '<div class="media-body">' +
        '<h6 class="list-group-item-heading"> <b>Total:</b> <span class="font-medium-4 float-xs-right "><b> ' + total + '</b></span></h6>' +
        '</div>' +
        '</a>'
    );
}

$("#filtro-estados").select2({
    placeholder: "Filtro Estados",
});
$("#filtro-agencias").select2({
    placeholder: "Filtro Agencias",
});
$("#filtro-agencias-factura").select2({
    placeholder: "Filtro Agencias",
});

$("#filtro-minoristas-factura").select2({
    placeholder: "Filtro Minoristas",
});

$('#daterange, #filtro-estados, #filtro-agencias').on('change', buildUrlExport);

function buildUrlExport() {
    var date = $('#daterange').val();
    var status = $('#filtro-estados').val()
    var agencies = $('#filtro-agencias').val()
    $('#exportExcelAccept').attr('href', `/home/ExportExcelPrincipalDistributor/?date=${date}&status=${status}&agencies=${agencies}`)
}

buildUrlExport();

$('#daterange-factura, #filtro-estados-factura, #filtro-agencias-factura, #filtro-minoristas-factura').on('change', buildUrlExportFactura);

function buildUrlExportFactura() {
    var date = $('#daterange-factura').val();
    var agencies = $('#filtro-agencias-factura').val()
    var retail = $('#filtro-minoristas-factura').val()
    $('#exportExcelFacturaAccept').attr('href', `/home/ExportExcelFacturaPrincipalDistributor/?date=${date}&agencies=${agencies}&retails=${retail}`)
}
buildUrlExportFactura()

$('#exportExcelAccept').on('click', function () {
    $('#modalExport').modal('hide');
})

$('#exportExcelFacturaAccept').on('click', function () {
    $('#modalExportFactura').modal('hide');
    $('#filtro-agencias-factura').val("").trigger('change')
    $('#filtro-minoristas-factura').val("").trigger('change')
})

$('#exportExcel').on('click', function () {
    $('#modalExport').modal('show');
});
$('#exportFacturaExcel').on('click', function () {
    $('#modalExportFactura').modal('show');
});
