$(document).ready(function () {
    //code here
    var date = new Date();
    var dd = String(date.getDate()).padStart(2, '0');
    var mm = String(date.getMonth() + 1).padStart(2, '0'); //January is 0!
    var yyyy = date.getFullYear();
    date = mm + '/' + dd + '/' + yyyy;

    var dataAgency = null;
    var dataBanco = null;

    //Actualizar el select meses al mes actual
    function updateMonth(month) {
        $('#selectMonth').val(month).trigger('change');
        getDataAgency();
    }
    updateMonth(mm);

    $('#selectMonth').on('change', function () { getDataAgency(); });

    $(document).on('click', '[name="enlace"]', function () {
        var elem = $(this);
        var subitems = elem.parent().parent().find('[name="subitems"]');
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

    $(document).on('click', '[name="elem_url"]', function () {
        var dataurl = $(this).attr('data-url');
        location.href = dataurl;
    });

    function getDataAgency() {
        mm = $('#selectMonth').val();
        date = mm + '/' + 01 + '/' + yyyy;
        $.ajax({
            type: "POST",
            url: "/contabilidad/getDataAgency",
            data: {
                date: date
            },
            async: false,
            beforeSend: function () {
                $.blockUI();
                $('#cont1').html('');
                $('#cont2').html('');
                $('#cont3').html('');
            },
            success: function (data) {
                if (data.success) {
                    console.log(data.data);
                    dataAgency = data.data;
                    var total = 0;
                    var cantidad = 0;
                    for (var i = 0; i < data.data.length; i++) {
                        var elem = data.data[i];
                        var credito = ""; 
                        preciocredito = 0;
                        datecredito = 0;
                        for (var j = 0; j < elem.length; j++) {
                            var date = elem[j].date.split('T');
                            var cliente = elem[j].cliente;
                            var tipopago = elem[j].tipopago;
                            var monto = parseFloat(elem[j].monto);
                            var details = elem[j].details;

                            total += monto;
                            cantidad++;
                            
                            if (elem[j].tipopago == "Crédito o Débito") {
                                preciocredito += monto;
                                datecredito = date[0];
                                credito +=
                                    '<div class="item" style="display:flex" >' +
                                    '<label style="margin:0px;margin-left:4px;margin-top:1px;font-weight:bold;">' + cantidad + '</label>' +
                                '<input type="checkbox" style="display:inline-block;margin-left:10px;margin-top:5px;" name="checkAgency" data-id="" value="" />' +
                                '<div name="elem_url" data-url="' + details + '" class="row" style="margin-left: 0px;margin-right: 0px;width:100%">' +
                                    '<div class="col-md-2 col-xs-5" style="margin-top:5px;font-size:11px;padding-left:5px;padding-right:5px;">' + date[0] + '</div>' +
                                '<div class="col-md-4 col-xs-5" style="margin-top:5px;font-size:11px;padding:0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;" title="' + cliente +'">' + cliente + '</div>' +
                                    '<div class="col-md-4 col-xs-6" style="margin-top:5px;font-size:11px;">' + tipopago + '</div>' +
                                    '<div class="col-md-2 col-xs-4" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + monto.toFixed(2) + '</span></div>' +
                                    '</div>' +
                                    '</div>'
                            }
                            else {
                                $('#cont1').append(
                                    '<div class="item" style="display:flex" >' +
                                    '<label style="margin:0px;margin-left:4px;margin-top:1px;font-weight:bold;">' + cantidad + '</label>' +
                                    '<input type="checkbox" style="display:inline-block;margin-left:10px;margin-top:5px;" name="checkAgency" data-id="" value="" />' +
                                    '<div name="elem_url" data-url="' + details + '" class="row" style="margin-left: 0px;margin-right: 0px;width:100%">'+
                                    '<div class="col-md-2 col-xs-5" style="margin-top:5px;font-size:11px;padding-left:5px;padding-right:5px;">' + date[0] + '</div>' +
                                    '<div class="col-md-4 col-xs-5" style="margin-top:5px;font-size:11px;padding:0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;" title="'+cliente+'">' + cliente + '</div>' +
                                    '<div class="col-md-4 col-xs-6" style="margin-top:5px;font-size:11px;">' + tipopago + '</div>' +
                                    '<div class="col-md-2 col-xs-4" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + monto.toFixed(2) + '</span></div>' +
                                        '</div>'+
                                    '</div>'
                                );
                            }
                        }

                        if (credito != "") {
                            $('#cont1').append(
                                '<div class="item" >' +
                                '<div style="display:flex">' +
                                '<input type="checkbox" style="display:inline-block;margin-left:10px;margin-top:5px;" name="checkAgency" data-id="" value="" />' +
                                '<div class="row" data-status="inactivo" name="enlace" style="width:100%; margin-left: 0px;margin-right: 0px;cursor:pointer">' +
                                '<div class="col-md-6" style="margin-top:5px;font-size:11px;padding-left:5px;">' + datecredito + '</div>' +
                                '<div class="col-md-4" style="margin-top:5px;font-size:11px">Crédito o Débito</div>' +
                                '<div class="col-md-2" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + preciocredito.toFixed(2) + '</span></div>' +
                                '</div>' +
                                '</div>' +
                                '<div name="subitems" data-status="inactivo" style="display:none">' + credito + '</div>' +
                                '</div>'
                            );
                        }
                    }

                    $('#pTotalAgency').html(total.toFixed(2));
                    $('#cantTotalAgency').html(" (" + cantidad + ")");
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

    $('#file-upload').on('change', function () {
        if (validateFile()) {
            var fdata = new FormData();
            var fileUpload = $("#file-upload").get(0);
            var files = fileUpload.files;
            fdata.append(files[0].name, files[0]);
            $.ajax({
                type: "POST",
                url: "/Contabilidad/ReadExcel",
                beforeSend: function (xhr) {
                    $('#contB1').html("");
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
                data: fdata,
                contentType: false,
                processData: false,
                async: false,
                success: function (response) {
                    if (response.success) {
                        dataBanco = response.data;
                        toastr.success("Success");
                        console.log(response.data);
                        var total = 0;
                        var canttotal = response.data.length;
                        for (var i = 0; i < response.data.length; i++) {
                            var elem = response.data[i];
                            var date = elem.date.split('T');
                            var amount = parseFloat(elem.amount);
                            var cliente = response.data[i].cliente;
                            total += amount;
                            var description = elem.description;

                            $('#contB1').append(
                                '<div class="item" style="display:flex">'+
                                    '<label style="margin:0px;margin-left:4px;margin-top:1px;font-weight:bold;">'+(i+1)+'</label>'+
                                    '<input type="checkbox" style="display:inline-block;margin-left:2px;margin-top:5px;" name="checkAgency" data-id="" value="" />'+
                                    '<div class="row" style="margin-left: 0px;margin-right: 0px;width:100%">'+
                                '<div class="col-md-2" style="margin-top:5px;font-size:11px;padding-left:5px;padding-right:0px;">' + date[0] + '</div>'+
                                        '<div class="col-md-4" style="margin-top:5px;font-size:11px;padding:0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">'+cliente+'</div>'+
                                '<div class="col-md-4" style="margin-top:5px;font-size:11px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;" title="' + description + '">' + description + '</div>'+
                                '<div class="col-md-2" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + amount.toFixed(2) + '</span></div>'+
                                    '</div>'+
                                '</div>'
                            );
                        }
                        $('#pTotalBanco').html(total.toFixed(2));
                        $('#cantTotalBanco').html(" (" + canttotal + ")");
                    }
                    else {
                        toastr.error(response.msg);
                    }
                    $.unblockUI();
                },
                error: function (e) {
                    showErrorMessage("ERROR", e.responseText);
                    $("#btnUpload").addClass("hidden");
                }
            });
        }
    });

    var validateFile = function () {
        var fileExtension = ['xls', 'xlsx'];
        var filename = $('#file-upload').val();
        if (filename.length == 0) {
            showWarningMessage("Atención", "Por favor, seleccione un archivo.");
            return false;
        } else {
            var extension = filename.replace(/^.*\./, '');
            if ($.inArray(extension, fileExtension) == -1) {
                showWarningMessage("Atención", "Por favor, seleccione solamente archivos excel.");
                return false;
            }
        }

        return true;
    }

    $('#addGasto').click(function () {
        var gastoValor = $('#gastoValor').val();
        var gastoDescripcion = $('#gastoDescripcion').val();
        var total = parseFloat($('#pTotalAgency').html());
        $('#pTotalAgency').html((total - gastoValor).toFixed(2));

        $('#cont1').prepend(
            '<div class="item">' +
            '<input type="checkbox" style="display:inline-block;margin-left:10px;" name="checkAgency" data-id="" value="" />' +
            '<div class="row" style="height: 20px;margin-left: 0px;margin-right: 0px;width:93%;display: inline-block;">' +
            '<div class="col-md-10 col-xs-7" title="' + gastoDescripcion + '" style="padding-left:5px;margin-top:5px;font-size:11px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">' + gastoDescripcion + '</div>' +
            '<div class="col-md-2 col-xs-4" style="margin-top:5px;font-size:12px;padding:0px;padding-left:10px"><span style="font-weight:bold;color:red">$ -' + gastoValor + '</span></div>' +
            '</div>' +
            '</div>'
        );
    });

    $('#match').on('click', Match);

    function Match() { //Para hacer coincidir los elementos de la agencia con el documento del banco
        $(this).prop('disabled', true);
        if (dataAgency != null && dataBanco != null && dataAgency.length != 0 && dataBanco.length != 0) {
            $('#cont1').html('');
            $('#cont2').html('');
            $('#cont3').html('');
            $('#contB1').html('');
            var countAgency = 1;
            var countBanco = 1;
            var auxDataAgency = dataAgency;
            var auxDataBanco = dataBanco;
            // Para los que coinciden
            for (var i = 0; i < auxDataAgency.length; i++) {
                var elemAgencyDay = dataAgency[i]; //Elementos de un día
                for (var j = 0; j < elemAgencyDay.length; j++) {
                    var elemAgency = elemAgencyDay[j];
                    if (elemAgency.tipopago == "Zelle") {
                        //Busco si el elemento coincide con algun elemento del banco
                        var elembanco = auxDataBanco.find(x => x.date.split('T')[0] == elemAgency.date.split('T')[0] && x.amount == elemAgency.monto && x.cliente.trim().toLowerCase() == elemAgency.cliente.trim().toLowerCase() && x.tipopago == elemAgency.tipopago);
                        if (elembanco != null) {
                            //Lo agrego al visual
                            //Agencia 
                            $('#cont1').append(
                                '<div class="item" style="display:flex; color:green" >' +
                                '<label style="margin:0px;margin-left:4px;margin-top:1px;font-weight:bold;">' + countAgency + '</label>' +
                                '<input type="checkbox" style="display:inline-block;margin-left:10px;margin-top:5px;" name="checkAgency" data-id="" value="" />' +
                                '<div name="elem_url" data-url="' + elemAgency.details + '" class="row" style="margin-left: 0px;margin-right: 0px;width:100%">' +
                                '<div class="col-md-2 col-xs-5" style="margin-top:5px;font-size:11px;padding-left:5px;padding-right:5px;">' + elemAgency.date.split('T')[0] + '</div>' +
                                '<div class="col-md-4 col-xs-5" style="margin-top:5px;font-size:11px;padding:0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;" title="' + elemAgency.cliente + '">' + elemAgency.cliente + '</div>' +
                                '<div class="col-md-4 col-xs-6" style="margin-top:5px;font-size:11px;">' + elemAgency.tipopago + '</div>' +
                                '<div class="col-md-2 col-xs-4" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + elemAgency.monto.toFixed(2) + '</span></div>' +
                                '</div>' +
                                '</div>'
                            );
                            //Banco
                            $('#contB1').append(
                                '<div class="item" style="display:flex;color:green">' +
                                '<label style="margin:0px;margin-left:4px;margin-top:1px;font-weight:bold;">' + countBanco + '</label>' +
                                '<input type="checkbox" style="display:inline-block;margin-left:2px;margin-top:5px;" name="checkAgency" data-id="" value="" />' +
                                '<div class="row" style="margin-left: 0px;margin-right: 0px;width:100%">' +
                                '<div class="col-md-2" style="margin-top:5px;font-size:11px;padding-left:5px;padding-right:0px;">' + elembanco.date.split('T')[0] + '</div>' +
                                '<div class="col-md-4" style="margin-top:5px;font-size:11px;padding:0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">' + elembanco.cliente + '</div>' +
                                '<div class="col-md-4" style="margin-top:5px;font-size:11px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;" title="' + elembanco.description + '">' + elembanco.description + '</div>' +
                                '<div class="col-md-2" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + elembanco.amount.toFixed(2) + '</span></div>' +
                                '</div>' +
                                '</div>'
                            );

                            var indiceBanco = auxDataBanco.indexOf(elembanco);
                            var indiceAgencia = elemAgencyDay.indexOf(elemAgency);

                            auxDataBanco.splice(indiceBanco, 1); //Lo elimino del listado
                            elemAgencyDay.splice(indiceAgencia, 1); //Lo elimino del listado

                            countAgency++;
                            countBanco++;
                        }
                    }
                    else {//Si no es Zell el primer elemento termino de iterar ya que lostipos de pagos estan agrupados por día
                        break;
                    }
                }
            }
            // Para los que tienen alguna coincidencia (fecha, )
            for (var i = 0; i < auxDataAgency.length; i++) {
                var elemAgencyDay = dataAgency[i]; //Elementos de un día
                var credito = "";
                preciocredito = 0;
                datecredito = "";
                var cantidad = 0; //Auxiliar para cuando es credito
                for (var j = 0; j < elemAgencyDay.length; j++) {
                    var elemAgency = elemAgencyDay[j];
                    if (elemAgency.tipopago == "Crédito o Débito") {
                        cantidad++;
                        preciocredito += elemAgency.monto;
                        datecredito = elemAgency.date.split('T')[0];
                        credito +=
                            '<div class="item" style="display:flex" >' +
                            '<label style="margin:0px;margin-left:4px;margin-top:1px;font-weight:bold;">' + countAgency + '</label>' +
                            '<input type="checkbox" style="display:inline-block;margin-left:10px;margin-top:5px;" name="checkAgency" data-id="" value="" />' +
                            '<div name="elem_url" data-url="' + elemAgency.details + '" class="row" style="margin-left: 0px;margin-right: 0px;width:100%">' +
                            '<div class="col-md-2 col-xs-5" style="margin-top:5px;font-size:11px;padding-left:5px;padding-right:5px;">' + elemAgency.date.split('T')[0] + '</div>' +
                            '<div class="col-md-4 col-xs-5" style="margin-top:5px;font-size:11px;padding:0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;" title="' + elemAgency.cliente + '">' + elemAgency.cliente + '</div>' +
                            '<div class="col-md-4 col-xs-6" style="margin-top:5px;font-size:11px;">' + elemAgency.tipopago + '</div>' +
                            '<div class="col-md-2 col-xs-4" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + elemAgency.monto.toFixed(2) + '</span></div>' +
                            '</div>' +
                            '</div>';
                        countAgency++;
                    }
                    else {
                        //Busco si el elemento tiene alguna coincidencia con algun elemento del banco
                        var elembanco = auxDataBanco.find(x => x.date.split('T')[0] == elemAgency.date.split('T')[0] && x.amount == elemAgency.monto && x.cliente.trim().toLowerCase() != elemAgency.cliente.trim().toLowerCase() && x.tipopago == elemAgency.tipopago);
                        if (elembanco != null) {
                            //Lo agrego al visual
                            //Agencia 
                            $('#cont2').append(
                                '<div class="item" style="display:flex; color:#e48b05" >' +
                                '<label style="margin:0px;margin-left:4px;margin-top:1px;font-weight:bold;">' + countAgency + '</label>' +
                                '<input type="checkbox" style="display:inline-block;margin-left:10px;margin-top:5px;" name="checkAgency" data-id="" value="" />' +
                                '<div name="elem_url" data-url="' + elemAgency.details + '" class="row" style="margin-left: 0px;margin-right: 0px;width:100%">' +
                                '<div class="col-md-2 col-xs-5" style="margin-top:5px;font-size:11px;padding-left:5px;padding-right:5px;">' + elemAgency.date.split('T')[0] + '</div>' +
                                '<div class="col-md-4 col-xs-5" style="margin-top:5px;font-size:11px;padding:0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;" title="' + elemAgency.cliente + '">' + elemAgency.cliente + '</div>' +
                                '<div class="col-md-4 col-xs-6" style="margin-top:5px;font-size:11px;">' + elemAgency.tipopago + '</div>' +
                                '<div class="col-md-2 col-xs-4" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + elemAgency.monto.toFixed(2) + '</span></div>' +
                                '</div>' +
                                '</div>'
                            );
                            //Banco
                            $('#contB2').append(
                                '<div class="item" style="display:flex;color:#e48b05">' +
                                '<label style="margin:0px;margin-left:4px;margin-top:1px;font-weight:bold;">' + countBanco + '</label>' +
                                '<input type="checkbox" style="display:inline-block;margin-left:2px;margin-top:5px;" name="checkAgency" data-id="" value="" />' +
                                '<div class="row" style="margin-left: 0px;margin-right: 0px;width:100%">' +
                                '<div class="col-md-2" style="margin-top:5px;font-size:11px;padding-left:5px;padding-right:0px;">' + elembanco.date.split('T')[0] + '</div>' +
                                '<div class="col-md-4" style="margin-top:5px;font-size:11px;padding:0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">' + elembanco.cliente + '</div>' +
                                '<div class="col-md-4" style="margin-top:5px;font-size:11px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;" title="' + elembanco.description + '">' + elembanco.description + '</div>' +
                                '<div class="col-md-2" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + elembanco.amount.toFixed(2) + '</span></div>' +
                                '</div>' +
                                '</div>'
                            );
                            countBanco++;
                            countAgency++;
                            var indiceBanco = auxDataBanco.indexOf(elembanco);
                            auxDataBanco.splice(indiceBanco, 1); //Lo elimino del listado
                            var indiceAgencia = elemAgencyDay.indexOf(elemAgency);
                            elemAgencyDay.splice(indiceAgencia, 1); //Lo elimino del listado
                    }
                    
                    }

                }

                if (credito != "") {
                    var elembanco = auxDataBanco.find(x => x.date.split('T')[0] == datecredito && x.amount == preciocredito );
                    if (elembanco != null) {
                        $('#cont2').append(
                            '<div class="item" >' +
                            '<div style="display:flex;color:#e48b05"">' +
                            '<input type="checkbox" style="display:inline-block;margin-left:10px;margin-top:5px;" name="checkAgency" data-id="" value="" />' +
                            '<div class="row" data-status="inactivo" name="enlace" style="width:100%; margin-left: 0px;margin-right: 0px;cursor:pointer">' +
                            '<div class="col-md-6" style="margin-top:5px;font-size:11px;padding-left:5px;">' + datecredito + '</div>' +
                            '<div class="col-md-4" style="margin-top:5px;font-size:11px">Crédito o Débito</div>' +
                            '<div class="col-md-2" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + preciocredito.toFixed(2) + '</span></div>' +
                            '</div>' +
                            '</div>' +
                            '<div name="subitems" data-status="inactivo" style="display:none">' + credito + '</div>' +
                            '</div>'
                        );

                        //Banco
                        $('#contB2').append(
                            '<div class="item" style="display:flex;color:#e48b05">' +
                            '<label style="margin:0px;margin-left:4px;margin-top:1px;font-weight:bold;">' + countBanco + '</label>' +
                            '<input type="checkbox" style="display:inline-block;margin-left:2px;margin-top:5px;" name="checkAgency" data-id="" value="" />' +
                            '<div class="row" style="margin-left: 0px;margin-right: 0px;width:100%">' +
                            '<div class="col-md-2" style="margin-top:5px;font-size:11px;padding-left:5px;padding-right:0px;">' + elembanco.date.split('T')[0] + '</div>' +
                            '<div class="col-md-4" style="margin-top:5px;font-size:11px;padding:0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">' + elembanco.cliente + '</div>' +
                            '<div class="col-md-4" style="margin-top:5px;font-size:11px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;" title="' + elembanco.description + '">' + elembanco.description + '</div>' +
                            '<div class="col-md-2" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + elembanco.amount.toFixed(2) + '</span></div>' +
                            '</div>' +
                            '</div>'
                        );

                        var indiceBanco = auxDataBanco.indexOf(elembanco);
                        auxDataBanco.splice(indiceBanco, 1); //Lo elimino del listado
                        var indiceAgencia = auxDataAgency.indexOf(elemAgencyDay);
                        auxDataAgency.splice(indiceAgencia, 1); //Lo elimino del listado
                    }
                    else {
                        countAgency = countAgency - cantidad; //Para restarle la cantidad usada en credito
                    }
              
                }
            }
            //Para el resto Agencia
            for (var i = 0; i < auxDataAgency.length; i++) {
                var elemAgencyDay = dataAgency[i]; //Elementos de un día
                var credito = "";
                preciocredito = 0;
                datecredito = "";
                for (var j = 0; j < elemAgencyDay.length; j++) {
                    var elemAgency = elemAgencyDay[j];
                    if (elemAgency.tipopago == "Crédito o Débito") {
                        preciocredito += elemAgency.monto;
                        datecredito = elemAgency.date.split('T')[0];
                        credito +=
                            '<div class="item" style="display:flex" >' +
                            '<label style="margin:0px;margin-left:4px;margin-top:1px;font-weight:bold;">' + countAgency + '</label>' +
                            '<input type="checkbox" style="display:inline-block;margin-left:10px;margin-top:5px;" name="checkAgency" data-id="" value="" />' +
                            '<div name="elem_url" data-url="' + elemAgency.details + '" class="row" style="margin-left: 0px;margin-right: 0px;width:100%">' +
                            '<div class="col-md-2 col-xs-5" style="margin-top:5px;font-size:11px;padding-left:5px;padding-right:5px;">' + elemAgency.date.split('T')[0] + '</div>' +
                            '<div class="col-md-4 col-xs-5" style="margin-top:5px;font-size:11px;padding:0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;" title="' + elemAgency.cliente + '">' + elemAgency.cliente + '</div>' +
                            '<div class="col-md-4 col-xs-6" style="margin-top:5px;font-size:11px;">' + elemAgency.tipopago + '</div>' +
                            '<div class="col-md-2 col-xs-4" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + elemAgency.monto.toFixed(2) + '</span></div>' +
                            '</div>' +
                            '</div>';
                        countAgency++;
                    }
                    else {
                        //Lo agrego al visual
                        //Agencia 
                        $('#cont3').append(
                            '<div class="item" style="display:flex; color:red" >' +
                            '<label style="margin:0px;margin-left:4px;margin-top:1px;font-weight:bold;">' + countAgency + '</label>' +
                            '<input type="checkbox" style="display:inline-block;margin-left:10px;margin-top:5px;" name="checkAgency" data-id="" value="" />' +
                            '<div name="elem_url" data-url="' + elemAgency.details + '" class="row" style="margin-left: 0px;margin-right: 0px;width:100%">' +
                            '<div class="col-md-2 col-xs-5" style="margin-top:5px;font-size:11px;padding-left:5px;padding-right:5px;">' + elemAgency.date.split('T')[0] + '</div>' +
                            '<div class="col-md-4 col-xs-5" style="margin-top:5px;font-size:11px;padding:0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;" title="' + elemAgency.cliente + '">' + elemAgency.cliente + '</div>' +
                            '<div class="col-md-4 col-xs-6" style="margin-top:5px;font-size:11px;">' + elemAgency.tipopago + '</div>' +
                            '<div class="col-md-2 col-xs-4" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + elemAgency.monto.toFixed(2) + '</span></div>' +
                            '</div>' +
                            '</div>'
                        );
                   
                        countAgency++;
                        var indiceAgencia = elemAgencyDay.indexOf(elemAgency);
                        elemAgencyDay.splice(indiceAgencia, 1); //Lo elimino del listado
                    }

                }

                if (credito != "") {
                    $('#cont3').append(
                        '<div class="item" >' +
                        '<div style="display:flex;color:red">' +
                        '<input type="checkbox" style="display:inline-block;margin-left:10px;margin-top:5px;" name="checkAgency" data-id="" value="" />' +
                        '<div class="row" data-status="inactivo" name="enlace" style="width:100%; margin-left: 0px;margin-right: 0px;cursor:pointer">' +
                        '<div class="col-md-6" style="margin-top:5px;font-size:11px;padding-left:5px;">' + datecredito + '</div>' +
                        '<div class="col-md-4" style="margin-top:5px;font-size:11px">Crédito o Débito</div>' +
                        '<div class="col-md-2" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + preciocredito.toFixed(2) + '</span></div>' +
                        '</div>' +
                        '</div>' +
                        '<div name="subitems" data-status="inactivo" style="display:none">' + credito + '</div>' +
                        '</div>'
                    );
                    var indiceAgencia = auxDataAgency.indexOf(elemAgencyDay);
                    auxDataAgency.splice(indiceAgencia, 1); //Lo elimino del listado

                }
            }
            //Para el resto Banco
            for (var i = 0; i < auxDataBanco.length; i++) {
                var elembanco = auxDataBanco[i];
                $('#contB3').append(
                    '<div class="item" style="display:flex;color:red">' +
                    '<label style="margin:0px;margin-left:4px;margin-top:1px;font-weight:bold;">' + countBanco + '</label>' +
                    '<input type="checkbox" style="display:inline-block;margin-left:2px;margin-top:5px;" name="checkAgency" data-id="" value="" />' +
                    '<div class="row" style="margin-left: 0px;margin-right: 0px;width:100%">' +
                    '<div class="col-md-2" style="margin-top:5px;font-size:11px;padding-left:5px;padding-right:0px;">' + elembanco.date.split('T')[0] + '</div>' +
                    '<div class="col-md-4" style="margin-top:5px;font-size:11px;padding:0px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;">' + elembanco.cliente + '</div>' +
                    '<div class="col-md-4" style="margin-top:5px;font-size:11px;white-space: nowrap;text-overflow: ellipsis;overflow: hidden;" title="' + elembanco.description + '">' + elembanco.description + '</div>' +
                    '<div class="col-md-2" style="margin-top:5px;font-size:12px;padding:0px;"><span style="font-weight:bold">$ ' + elembanco.amount.toFixed(2) + '</span></div>' +
                    '</div>' +
                    '</div>'
                );
                countBanco++;
                var indiceBanco = auxDataBanco.indexOf(elembanco);
                auxDataBanco.splice(indiceBanco, 1); //Lo elimino del listado
            }
        }
        else {
            toastr.error("Datos insuficientes.")
            $('#match').prop('disabled', false);
        }
    };




});
