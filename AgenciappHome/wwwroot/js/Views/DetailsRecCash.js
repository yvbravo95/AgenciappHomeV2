$(document).ready(function () {

    var cantrigth = 0;
    var amountRight = 0;
    var dataAgency = [];
    let ListCash = [];

    $(document).on('click', '[name="enlace"]', function () {
        var elem = $(this);
        var subitems = elem.parent().find('[name="subitems"]');
        var status = elem.attr('data-status');
        var row = elem.parent().children()[0];
        if (status == "inactivo") {
            subitems.show();
            elem.attr('data-status', "activo");
            $(row).addClass('selected');
        }
        else {
            subitems.hide();
            elem.attr('data-status', "inactivo");
            $(row).removeClass('selected');
        }
    });

    function getDataAgency() {

        $.ajax({
            type: "POST",
            url: "/contabilidad/getDataCashItem",
            data: {
                Id: cashId
            },
            async: false,
            beforeSend: function () {
                $.blockUI();
                $('#contAgency').html("");
            },
            success: function (data) {
                if (data.success) {
                    //Left
                    var total = 0;
                    var cantidad = 0;
                    for (var i = 0; i < data.dataLeft.length; i++) {
                        var elem = data.dataLeft[i];
                        var credito = ""; 
                        preciocredito = 0;
                        datecredito = 0;
                        for (var j = 0; j < elem.length; j++) {
                            dataAgency.push(elem[j]);
                            var date = elem[j].date.split('T');
                            var number = elem[j].number;
                            var tipopago = elem[j].tipopago;
                            var monto = parseFloat(elem[j].monto);
                            var details = elem[j].details;

                            total += monto;
                            cantidad++;
                            
                            if (elem[j].tipopago == "Crédito o Débito") {
                                preciocredito += monto;
                                datecredito = date[0];
                                credito +=
                                    '<div class="item">' +
                                    '<input type="checkbox" style="display:inline-block;margin-left:10px;" name="checkAgency" data-id="" value="" />'+
                                    '<div class="row" style="height: 20px;margin-left: 0px;margin-right: 0px;width:93%;display: inline-block;">' +
                                    '<div class="col-md-2 col-xs-5" style="margin-top:5px;font-size:11px;margin-left:5px;padding-left: 0px;padding-right:0px;">' + date[0] + '</div>' +
                                    '<div class="col-md-3 col-xs-5" style="margin-top:5px;font-size:11px;padding:0px;"><a href="' + details + '" class="linknumber">' + number + '</a></div>' +
                                '<div class="col-md-4 col-xs-6" style="margin-top:5px;font-size:11px;">' + tipopago + '</div>' +
                                '<div class="col-md-2 col-xs-4" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + monto.toFixed(2) + '</span></div>' +
                                    '</div>' +
                                    '</div>'
                            }
                            else {
                                $('#contAgency').append(
                                    '<div class="item">' +
                                    '<input type="checkbox" style="display:inline-block;margin-left:10px;" name="checkAgency" data-id="" value="" />' +
                                    '<div class="row" style="height: 20px;margin-left: 0px;margin-right: 0px;width:93%;display: inline-block;">' +
                                    '<div class="col-md-2 col-xs-5" style="margin-top:5px;font-size:11px;margin-left:5px;padding-left: 0px;padding-right:0px;">' + date[0] + '</div>' +
                                    '<div class="col-md-3 col-xs-5" style="margin-top:5px;font-size:11px;padding:0px;"><a href="' + details + '" class="linknumber">' + number + '</a></div>' +
                                    '<div class="col-md-4 col-xs-6" style="margin-top:5px;font-size:11px;">' + tipopago + '</div>' +
                                    '<div class="col-md-2 col-xs-4" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + monto.toFixed(2) + '</span></div>' +
                                    '</div>'+
                                    '</div>'
                                );
                            }
                        }
                        if (credito != "") {
                            $('#contAgency').append(
                                '<div class="item">' +
                                '<input type="checkbox" style="display:inline-block;margin-left:10px;" name="checkAgency" data-id="" value="" />' +
                                '<div class="row" data-status="inactivo" name="enlace" class="enlace" style="height: 20px;margin-left: 0px;margin-right: 0px;width:93%;display: inline-block;">' +
                                '<div class="col-md-5" style="margin-top:5px;font-size:11px;margin-left:5px;padding-left:0px;padding-right:0px;">' + datecredito + '</div>' +
                                '<div class="col-md-4" style="margin-top:5px;font-size:11px">Crédito o Débito</div>' +
                                '<div class="col-md-2" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + preciocredito.toFixed(2) + '</span></div>' +
                                    '</div>'+
                                    '<div name="subitems" style="display:none">'+
                                       credito +
                                    '</div>'+
                                '</div>'
                            );
                        }
                    }
                    $('#pTotalAgency').html(total.toFixed(2));
                    $('#cantTotalAgency').html(" (" + cantidad + ")");

                    //Right
                    total = 0;
                    var canttotal = data.dataRight.length;
                    for (var i = 0; i < data.dataRight.length; i++) {
                        var descripcion = data.dataRight[i].description;
                        var monto = data.dataRight[i].amount;
                        cantrigth++;
                        amountRight += parseFloat(monto);

                        var cash = new Object();
                        cash.TypeAdjustment = data.dataRight[i].typeAdjustment;
                        cash.Description = descripcion;
                        cash.Amount = monto;

                        ListCash.push(cash);

                        if (data.dataRight[i].typeAdjustment == "Cash Físico") {
                            $('#elementsRight').append(
                                '<div class= "container" id ="contBanco" >' +
                                '<div class="item" style="display:flex;background-color: #b1ffd7;">' +
                                '<div class="row" style="margin-left: 0px;margin-right: 0px;width:100%">' +
                                '<div class="col-md-2" style="margin-top:5px;font-size:11px;padding-left:5px;padding-right:0px;">Cash Físico</div>' +
                                '<div class="col-md-8" style="margin-top:5px;font-size:11px;padding:0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">' + descripcion + '</div>' +
                                '<div class="col-md-2" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + monto + '</span></div>' +
                                '</div>' +
                                '</div>' +
                                '</div >'
                            );
                        }
                        else if (data.dataRight[i].typeAdjustment == "Gasto") {
                            $('#elementsRight').append(
                                '<div class= "container" id ="contBanco" >' +
                                '<div class="item" style="display:flex;background-color: #f9000026;">' +
                                '<div class="row" style="margin-left: 0px;margin-right: 0px;width:100%">' +
                                '<div class="col-md-2" style="margin-top:5px;font-size:11px;padding-left:5px;padding-right:0px;">Gasto</div>' +
                                '<div class="col-md-8" style="margin-top:5px;font-size:11px;padding:0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">' + descripcion + '</div>' +
                                '<div class="col-md-2" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + monto + '</span></div>' +
                                '</div>' +
                                '</div>' +
                                '</div >'
                            );
                        }
                        
                    }
                    $('#pTotalBanco').html(amountRight);
                    $('#cantTotalBanco').html("(" + cantrigth + ")");
                }
                else {
                    toastr.error(data.msg);
                }
                $.unblockUI();
            },
            failure: function (response) {
                $.unblockUI();
                showErrorMessage("ERROR", "No se ha podido obtener los datos de la agencia");
            },
            error: function (response) {
                $.unblockUI();
                showErrorMessage("ERROR", "No se ha podido obtener los datos de la agencia");
            }
        });
    };
    getDataAgency();

  

    $('#confirmCash').on('click', function () {
        var descripcion = $('#descripcionCash').val();
        var monto = $('#montoCash').val();
        cantrigth++;
        amountRight += parseFloat(monto);

        var cash = new Object();
        cash.TypeAdjustment = "Cash Físico";
        cash.Description = descripcion;
        cash.Amount = monto;

        ListCash.push(cash);

        $('#pTotalBanco').html(amountRight);
        $('#cantTotalBanco').html("(" + cantrigth + ")");
        $('#elementsRight').append(
            '<div class= "container" id ="contBanco" >'+
            '<div class="item" style="display:flex;background-color: #b1ffd7;">' +
                '<div class="row" style="margin-left: 0px;margin-right: 0px;width:100%">'+
                '<div class="col-md-2" style="margin-top:5px;font-size:11px;padding-left:5px;padding-right:0px;">Cash Físico</div>' +
                '<div class="col-md-8" style="margin-top:5px;font-size:11px;padding:0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">' + descripcion + '</div>' +
                '<div class="col-md-2" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ '+ monto +'</span></div>'+
                '</div>'+
            '</div>'+
                '</div >'
            );

    });

    $('#confirmGasto').on('click', function () {
        var descripcion = $('#conceptoGasto').val();
        var monto = $('#montoGasto').val();
        cantrigth++;
        amountRight += parseFloat(monto);

        var cash = new Object();
        cash.TypeAdjustment = "Gasto";
        cash.Description = descripcion;
        cash.Amount = monto;

        ListCash.push(cash);

        $('#pTotalBanco').html(amountRight);
        $('#cantTotalBanco').html("(" + cantrigth + ")");
        $('#elementsRight').append(
            '<div class= "container" id ="contBanco" >' +
            '<div class="item" style="display:flex;background-color: #f9000026;">' +
            '<div class="row" style="margin-left: 0px;margin-right: 0px;width:100%">' +
            '<div class="col-md-2" style="margin-top:5px;font-size:11px;padding-left:5px;padding-right:0px;">Gasto</div>' +
            '<div class="col-md-8" style="margin-top:5px;font-size:11px;padding:0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">' + descripcion + '</div>' +
            '<div class="col-md-2" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + monto + '</span></div>' +
            '</div>' +
            '</div>' +
            '</div >'
        );

    });

    $('#selectStatus').on('change', function () {
        if ($(this).val() == "Confirmada") $('#fieldBox').show();
        else $('#fieldBox').hide();
    });

    $('#modalEditConfirm').on('click', function () {
        var status = $('#selectStatus').val();
        var box = $('#selectBox').val();
        $.ajax({
            type: "POST",
            url: "/Contabilidad/CashItemEdit",
            data: {
                Id: cashId,
                dataCash: ListCash,
                status: status,
                box: box
            },
            async: false,
            beforeSend: function () {
                //Bloqueo la pantalla del usuario
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
                    window.location = "/Contabilidad/CashIndex?msg=" + response.msg ;
                }
                else {
                    console.log(response.exception);
                    toastr.error(response.msg);
                }
            },
            failure: function (response) {
                showErrorMessage("FAILURE", "No se han podido guardar los datos");
                $.unblockUI();
                isSend = false;
            },
            error: function (response) {
                showErrorMessage("ERROR", "No se han podido guardar los datos");
                $.unblockUI();
                isSend = false;
            }
        });
    });
});


