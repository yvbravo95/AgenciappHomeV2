$(document).ready(function () {
    // Wizard
    $("#wz").steps({
        headerTag: "h6",
        bodyTag: "fieldset",
        transitionEffect: "fade",
        enableCancelButton: true,
        titleTemplate: '<span class="step">#index#</span> #title#',
        labels: {
            previous: "Anterior",
            next: "Siguiente",
            finish: 'Comprar',
            cancel: "Cancelar"
        },
        onStepChanging: function (event, currentIndex, newIndex) {
            // Allways allow previous action even if the current form is not valid!
            if (currentIndex > newIndex) {
                return true;
            }
            //----------------Step1
            var error = false
            if (newIndex == 1) {
                if ($("#ClientId").val() == "") {
                    showWarningMessage("Atención", "El campo de cliente es obligatorio.");
                    error = true
                }
            }
            return !error;
        },
        onFinishing: function (event, currentIndex) {
            var error = false
            //----------------Step2

            if ($("#Import").val() == "" || $("#Import").val() < 0) {
                showWarningMessage("Atención", "El campo de Total debe ser mayor que 0.");
                error = true
            }
            if ($("#Balance").val() < 0) {
                showWarningMessage("Atención", "El valor de la deuda no puede ser menor que 0.");
                error = true
            }
            if ($('#NumberPhone').val() == "") {
                showWarningMessage("Atención", "Debe escribir un teléfono a recargar.");
                error = true
            }
            if (!error) {
                swal({
                    title: "¿Está seguro?",
                    text: "Verifique que todos los datos ingresados se encuentren correctamente. Cuando presione 'ok' se procederá a realizar la compra.",
                    type: "info",
                    showCancelButton: true,
                    closeOnConfirm: false,
                    showLoaderOnConfirm: true,
                }, function (isConfirm) {
                    if (isConfirm) {
                        var form = $('#wz');
                        form.submit()
                    } else {
                        return false;
                    }

                });
            }
            return !error;
        },
        onFinished: function (event, currentIndex) {

        },
        onCanceled: function () {
            window.location = "/Recargue"
        }
    });
    $("[href='#cancel']").addClass('btn-danger');

    // block unblock
    function block($this) {
        var block_ele = $this.closest('.card');
        block_ele.block({
            message: '<div id="load" class="ft-refresh-cw icon-spin font-medium-2"></div>',
            overlayCSS: {
                backgroundColor: '#FFF',
                cursor: 'wait',
            },
            css: {
                border: 0,
                padding: 0,
                backgroundColor: 'none'
            }
        });
    }
    function unblock($this) {
        var block_ele = $this.closest('.card');
        block_ele.unblock()
    }

    $("#inputClientFecha").pickadate({
        labelMonthNext: 'Next month',
        labelMonthPrev: 'Previous month',
        labelMonthSelect: 'Pick a Month',
        selectMonths: true,
        selectYears: 200,
        format: 'dd mmmm yyyy',
        formatSubmit: 'yyyy/dd/mm',
    });

    // cargar estado
    $("select[id=state]").empty()
    $("select[id=state]").append(new Option("Alabama", "Alabama"))
    $("select[id=state]").append(new Option("Alaska", "Alaska"))
    $("select[id=state]").append(new Option("American Samoa", "American Samoa"))
    $("select[id=state]").append(new Option("Arizona", "Arizona"))
    $("select[id=state]").append(new Option("Arkansas", "Arkansas"))
    $("select[id=state]").append(new Option("Armed Forces Americas", "Armed Forces Americas"))
    $("select[id=state]").append(new Option("Armed Forces Europe, Canada, Africa and Middle East", "Armed Forces Europe, Canada, Africa and Middle East"))
    $("select[id=state]").append(new Option("Armed Forces Pacific", "Armed Forces Pacific"))
    $("select[id=state]").append(new Option("California", "California"))
    $("select[id=state]").append(new Option("Colorado", "Colorado"))
    $("select[id=state]").append(new Option("Connecticut", "Connecticut"))
    $("select[id=state]").append(new Option("Delaware", "Delaware"))
    $("select[id=state]").append(new Option("District of Columbia", "District of Columbia"))
    $("select[id=state]").append(new Option("Florida", "Florida"))
    $("select[id=state]").append(new Option("Georgia", "Georgia"))
    $("select[id=state]").append(new Option("Guam", "Guam"))
    $("select[id=state]").append(new Option("Hawaii", "Hawaii"))
    $("select[id=state]").append(new Option("Idaho", "Idaho"))
    $("select[id=state]").append(new Option("Illinois", "Illinois"))
    $("select[id=state]").append(new Option("Indiana", "Indiana"))
    $("select[id=state]").append(new Option("Iowa", "Iowa"))
    $("select[id=state]").append(new Option("Kansas", "Kansas"))
    $("select[id=state]").append(new Option("Kentucky", "Kentucky"))
    $("select[id=state]").append(new Option("Louisiana", "Louisiana"))
    $("select[id=state]").append(new Option("Maine", "Maine"))
    $("select[id=state]").append(new Option("Marshall Islands", "Marshall Islands"))
    $("select[id=state]").append(new Option("Maryland", "Maryland"))
    $("select[id=state]").append(new Option("Massachusetts", "Massachusetts"))
    $("select[id=state]").append(new Option("Michigan", "Michigan"))
    $("select[id=state]").append(new Option("Micronesia", "Micronesia"))
    $("select[id=state]").append(new Option("Minnesota", "Minnesota"))
    $("select[id=state]").append(new Option("Mississippi", "Mississippi"))
    $("select[id=state]").append(new Option("Missouri", "Missouri"))
    $("select[id=state]").append(new Option("Montana", "Montana"))
    $("select[id=state]").append(new Option("Nebraska", "Nebraska"))
    $("select[id=state]").append(new Option("Nevada", "Nevada"))
    $("select[id=state]").append(new Option("New Hampshire", "New Hampshire"))
    $("select[id=state]").append(new Option("New Jersey", "New Jersey"))
    $("select[id=state]").append(new Option("New Mexico", "New Mexico"))
    $("select[id=state]").append(new Option("New York", "New York"))
    $("select[id=state]").append(new Option("North Carolina", "North Carolina"))
    $("select[id=state]").append(new Option("North Dakota", "North Dakota"))
    $("select[id=state]").append(new Option("Northern Mariana Islands", "Northern Mariana Islands"))
    $("select[id=state]").append(new Option("Ohio", "Ohio"))
    $("select[id=state]").append(new Option("Oklahoma", "Oklahoma"))
    $("select[id=state]").append(new Option("Oregon", "Oregon"))
    $("select[id=state]").append(new Option("Palau", "Palau"))
    $("select[id=state]").append(new Option("Pennsylvania", "Pennsylvania"))
    $("select[id=state]").append(new Option("Puerto Rico", "Puerto Rico"))
    $("select[id=state]").append(new Option("Rhode Island", "Rhode Island"))
    $("select[id=state]").append(new Option("South Carolina", "South Carolina"))
    $("select[id=state]").append(new Option("South Dakota", "South Dakota"))
    $("select[id=state]").append(new Option("Tennessee", "Tennessee"))
    $("select[id=state]").append(new Option("Texas", "Texas"))
    $("select[id=state]").append(new Option("Utah", "Utah"))
    $("select[id=state]").append(new Option("Vermont", "Vermont"))
    $("select[id=state]").append(new Option("Virgin Islands", "Virgin Islands"))
    $("select[id=state]").append(new Option("Virginia", "Virginia"))
    $("select[id=state]").append(new Option("Washington", "Washington"))
    $("select[id=state]").append(new Option("West Virginia", "West Virginia"))
    $("select[id=state]").append(new Option("Wisconsin", "Wisconsin"))
    $("select[id=state]").append(new Option("Wyoming", "Wyoming"))

    // para autocomletar cliente
    $(".Sel").select2({
        placeholder: "Buscar cliente por teléfono, nombre o apellido",
        val: null,
        ajax: {
            type: 'POST',
            url: '/Clients/findClient',
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

                        return { id: obj.clientId, text: obj.fullData };
                    })
                };
            }
        }
    });

    // Seleccion de cliente
    $("#ClientId").change(function () {
        $("#editarCliente").show();
        var $this = $(this);
        var clientId = $this.val();
        if (clientId != "") {
            $.ajax({
                async: true,
                type: "POST",
                contentType: "application/x-www-form-urlencoded",
                url: "/ticket/loadclient",
                data: {
                    clientId: clientId,
                },
                beforeSend: function () {
                    block($this)
                },
                success: function (response) {
                    $("#inputClientName").data("pasaporteid", response["passportid"]);
                    $("#inputClientName").val(response["name"]);
                    $("#inputClientLastName").val(response["lastname"]);
                    $("#inputClientMovil").val(response["movile"]);
                    $("#inputClientEmail").val(response["email"]);
                    $("#inputClientAddress").val(response["address"]);
                    $("#state").val(response["state"]);
                    $("#inputClientCity").val(response["city"]);
                    $("#inputClientZip").val(response["zip"]);
                    $("#inputClientTelCuba").val(response["phoneCuba"]);
                    $("#inputClientIdentidad").val(response["id"]);
                    $("#inputClientFecha").pickadate('picker').set('select', new Date(response["fechaNac"]));

                    $('[name = "clienteInfo"]').html(response["name"] + " " + response["lastname"]);
                    $('[name = "telefonoInfo"]').html(response["movile"]);


                    //Datos del Cliente en Authorization Card
                    $('#nameClientCard').html('<strong>Nombre: </strong>' + response["name"] + ' ' + response["lastname"]);
                    $('#phoneClientCard').html('<strong>Teléfono: </strong>' + response["movile"]);
                    $('#emailClientCard').html('<strong>Email: </strong>' + response["email"]);
                    $('#stateClientCard').html('<strong>Estado: </strong>' + response["state"]);
                    $('#cityClientCard').html('<strong>Ciudad: </strong>' + response["city"]);
                    $('#addressClientCard').html('<strong>Dirección: </strong>' + response["address"]);

                    $('#AuthaddressOfSend').val(response["address"]);
                    $('#Authemail').val(response["email"]);
                    $('#Authphone').val(response["movile"]);

                    //Valor del ID en la imagen adjunta
                    $('#TextoIDImg').val(response["id"]);

                    $("#Pasajeros_0__Name").val(response["name"]);
                    $("#Pasajeros_0__LastName").val(response["lastname"]);
                    $("#Pasajeros_0__Phone").val(response["movile"]);

                    if (response.getCredito && response.getCredito != 0) {
                        $("#div_credito").removeAttr('hidden');
                        $("#credito").html(response.getCredito);
                    }
                    else {
                        $("#div_credito").attr('hidden', "hidden");
                    }
                    if (response.conflictivo) {
                        $("#conflictivo").removeAttr("hidden");
                    }
                    else {
                        $("#conflictivo").attr("hidden", "hidden");
                    }
                    $('.actions').show();
                    unblock($this)
                },
                error: function () {
                    $('.actions').hide();
                    alert("Error");
                    unblock($this);
                },
                timeout: 4000,
            });
        }
        else {
        }
    });

    // editar cliente
    $("#editarCliente").click(function () {
        showEdit()
    })

    $("#cancelarCliente").click(function () {
        hideEdit()
    })

    function showEdit() {
        $("#editarCliente").hide()
        $("#guardarCliente").show()
        $("#cancelarCliente").show()
        $("#ClientId").attr("disabled", "disabled")

        $("#inputClientName").removeAttr("disabled")
        $("#inputClientLastName").removeAttr("disabled")
        $("#inputClientMovil").removeAttr("disabled")
        $("#inputClientEmail").removeAttr("disabled")
        $("#inputClientAddress").removeAttr("disabled")
        $("#state").removeAttr("disabled")
        $("#inputClientCity").removeAttr("disabled")
        $("#inputClientZip").removeAttr("disabled")
        $("#inputClientTelCuba").removeAttr("disabled")
        $("#inputClientIdentidad").removeAttr("disabled")
        $("#inputClientFecha").removeAttr("disabled")
    }

    function hideEdit() {
        $("#editarCliente").show()
        $("#guardarCliente").hide()
        $("#cancelarCliente").hide()
        $("#ClientId").removeAttr("disabled")

        $("#inputClientName").attr("disabled", "disabled")
        $("#inputClientLastName").attr("disabled", "disabled")
        $("#inputClientMovil").attr("disabled", "disabled")
        $("#inputClientEmail").attr("disabled", "disabled")
        $("#inputClientAddress").attr("disabled", "disabled")
        $("#state").attr("disabled", "disabled")
        $("#inputClientCity").attr("disabled", "disabled")
        $("#inputClientZip").attr("disabled", "disabled")
        $("#inputClientTelCuba").attr("disabled", "disabled")
        $("#inputClientIdentidad").attr("disabled", "disabled")
        $("#inputClientFecha").attr("disabled", "disabled")
    }

    $("#guardarCliente").click(function () {
        var source = [
            $("#ClientId").val(), // 0
            $('#inputClientName').val(), //1
            $('#inputClientLastName').val(), //2
            $('#inputClientEmail').val(),  //3
            $('#inputClientMovil').val(), //4
            $('#inputClientAddress').val(), //5
            $('#inputClientCity').val(), // 6
            $('#state').val(),  // 7
            $('#inputClientZip').val(), //8
            $('#inputClientTelCuba').val(), //9
            "", //10
            $('#inputClientIdentidad').val(), //11
            $("#inputClientFecha").val(), //12

        ];

        $.ajax({
            type: "POST",
            url: "/clients/EditClient",
            data: JSON.stringify(source),
            dataType: 'json',
            contentType: 'application/json',
            async: false,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.msg);
                }
                else {
                    toastr.error(response.msg);
                }
                hideEdit()
                
                //Datos del Cliente en Authorization Card
                $('#nameClientCard').html('<strong>Nombre: </strong>' + $('#inputClientName').val() + ' ' + $('#inputClientLastName').val());
                $('#phoneClientCard').html('<strong>Teléfono: </strong>' + $('#inputClientMovil').val());
                $('#emailClientCard').html('<strong>Email: </strong>' + $('#inputClientEmail').val());
                $('#stateClientCard').html('<strong>Estado: </strong>' + $('#state').val());
                $('#cityClientCard').html('<strong>Ciudad: </strong>' + $('#inputClientCity').val());
                $('#addressClientCard').html('<strong>Dirección: </strong>' + $('#inputClientAddress').val());

                $('#AuthaddressOfSend').val($('#inputClientAddress').val());
                $('#Authemail').val($('#inputClientEmail').val());
                $('#Authphone').val($('#inputClientMovil').val());

                //Valor del ID en la imagen adjunta
                $('#TextoIDImg').val($('#inputClientIdentidad').val());
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    });


    // Cuando se cree un nuevo cliente al recargarse se muestre
    if (selectedClient != '00000000-0000-0000-0000-000000000000') {
        $.ajax({
            type: "POST",
            url: "/Clients/GetClient",
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(selectedClient),
            async: false,
            success: function (data) {
                var newOption = new Option(data.movil + "-" + data.name + " " + data.lastName, selectedClient, false, false);
                $(".Sel").append(newOption);
                $(".Sel").val(selectedClient).trigger("change").trigger("select2:select");
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            }
        });
    }

    var selectMayorista;
    // variable usada para calcular el porciento en el tipo de recarga celular
    var porcientocelular = 0;
    var tiporecarga = $('[name="tiporecarga"][checked]').val();

    $('#selectMayorista').on('change', function () {
        var id = $(this).val();
        countOld = 1;
            $.ajax({
                type: "POST",
                url: "/Wholesalers/getMayorista",
                data: {
                    id: id
                },
                async: false,
                success: function (data) {
                    selectMayorista = data;
                    if (tiporecarga == "cubacel") {
                        $('#precioventa').val(data.precioCubacel);
                        $('#costo').val(data.costoCubacel);
                        $('#monto').val(20);
                    }
                    else if (tiporecarga == "nauta") {
                        $('#precioventa').val(data.precioNauta);
                        $('#costo').val(data.costoNauta);
                        $('#monto').val(10);

                    }
                    else if (tiporecarga == "celular") {
                        porcientocelular = data.costoCelular;
                        $('#costo').val(0);
                        $('#monto').val(0);
                        $('#precioventa').val(0);

                    }
                    calculate();
                },
                failure: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                },
                error: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                }
            });
        });

    //Si se cambia el tipo de recarga actualizo los valores
    $('[name="tiporecarga"]').on('change', function () {
        tiporecarga = $(this).val();
        countOld = 1;
            if (selectMayorista != null) {
                if (tiporecarga == "cubacel") {
                    $('#precioventa').val(selectMayorista.precioCubacel);
                    $('#costo').val(selectMayorista.costoCubacel);
                    $('#monto').val(20);

                }
                else if (tiporecarga == "nauta") {
                    $('#precioventa').val(selectMayorista.precioNauta);
                    $('#costo').val(selectMayorista.costoNauta);
                    $('#monto').val(10);

                }
                else if (tiporecarga == "celular") {
                    porcientocelular = selectMayorista.costoCelular;
                    $('#precioventa').val(0);
                    $('#costo').val(0);
                    $('#monto').val(0);
                }
            }
            
            calculate();
        });

    //Script para obligar a llenar el autorization card luego de ser entrado el precio
    $('#Price').change(function () {
        var value = $(this).val();
        tipoPagoId = $('#tipoPago').val();
        tipoPago = $('option[value = "' + tipoPagoId + '"]').html();

        if (tipoPago == "Crédito o Débito" && value > 0) {
            $('#AddAuthorization').show();
        }
        else {
            $('#AddAuthorization').hide();
        }
    });
    $('#tipoPago').on('change', function () {
        tipoPagoId = $('#tipoPago').val();
        tipoPago = $('option[value = "' + tipoPagoId + '"]').html();

        if (tipoPago == "Crédito o Débito" && $("#ValorPagado").val() > 0) {
            $('#AddAuthorization').show();
            $('#contfee').show();
        }
        else {
            $('#AddAuthorization').hide();
            $('#contfee').hide();
        }
        calculate(false);
    });

    //pagar con credito consumo
    $("#check_credito").on('click', function () {
        $("#pagoCash").val(0);
        $("#pagoZelle").val(0);
        $("#pagoCheque").val(0);
        $("#pagoCredito").val(0);
        $("#pagoTransf").val(0);
        $("#pagoWeb").val(0);
        if ($("#check_credito").is(" :checked")) {
            $("#inputCredito").val($("#credito").html());
        }
        else {
            $("#inputCredito").val(0);
        }
    });

    //Multiples pagos
    $("#checkpago").on('click', function () {
        if ($("#checkpago").is(" :checked")) {
            $("#untipopago").attr("hidden", 'hidden');
            $(".multipopago").removeAttr("hidden");
            $('#contfee').hide();
            $("#ValorPagado").val(0);
            $("#pagoCash").val(0);
            $("#pagoZelle").val(0);
            $("#pagoCheque").val(0);
            $("#pagoCredito").val(0);
            $("#pagoTransf").val(0);
            $("#pagoWeb").val(0);
        }
        else {
            $(".multipopago").attr("hidden", 'hidden');
            $("#untipopago").removeAttr("hidden")
            $("#pagoCash").val(0);
            $("#pagoZelle").val(0);
            $("#pagoCheque").val(0);
            $("#pagoCredito").val(0);
            $("#pagoTransf").val(0);
            $("#pagoWeb").val(0);
            $("#ValorPagado").val(0);
        }
        calculate()
    });

    $("#monto,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb").keyup(function () {
        calculate();
    })
    $("#monto,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb,#check_credito,#NumberPhone").on('change', function () {
        calculate();
    })
    var modifyPrecioVenta = false;
    $('#precioventa').on('keyup', function () {
        modifyPrecioVenta = true;
        calculate();
    });
    $('#precioventa').on('change', function () {
        modifyPrecioVenta = true;
        calculate();
    });
    $("#ValorPagado").keyup(function () {
        calculatePayment(true)
    })
    $("#ValorPagado").on('change', function () {
        calculatePayment(true)
    })

    var countOld = 1;

    $('#precioventa').on('change', function () {
        tipoPagoId = $('#tipoPago').val();
        tipoPago = $('option[value = "' + tipoPagoId + '"]').html();

        if (tipoPago == "Crédito o Débito" && $(this).val() > 0) {
            $('#AddAuthorization').show();
            $('#contfee').show();
        }
        else {
            $('#AddAuthorization').hide();
            $('#contfee').hide();
        }
    });
    // Si se modifica el precio de venta cuanto el tipo es cubacel 
    function calculatePayment() {
        var pagado = parseFloat($("#ValorPagado").val());
        var precioTotalValue = parseFloat($("#precioventa").val());
        var max = precioTotalValue;

        tipoPagoId = $('#tipoPago').val();
        tipoPago = $('option[value = "' + tipoPagoId + '"]').html();
        if (tipoPago == "Crédito o Débito") {
            var fee = parseFloat($('#fee').html());
            var pagdoReal = pagado / (1 + fee / 100)
            var feeCrDeb = pagado - pagdoReal;
            precioTotalValue = precioTotalValue + feeCrDeb;
            max = max + max * fee / 100;

            //Valor Sale Amount en authorization card
            $('#AuthSaleAmount').val(pagdoReal.toFixed(2));
            $('#TotalCharge').val(pagado);
        }
        precioTotalValue = precioTotalValue.toFixed(2);
        var balanceValue = 0;
        if ($("#check_credito").is(":checked")) {
            var pagoCredito = parseFloat($("#credito").html());
            pagoCredito = pagoCredito > precioTotalValue ? precioTotalValue : pagoCredito;
            balanceValue = precioTotalValue - pagado - pagoCredito;
        }
        else {
            balanceValue = precioTotalValue - pagado;
        }
        $("#Import").val(precioTotalValue);
        $("#valorPagado").val(pagado.toFixed(2));
        $("#Balance").val(balanceValue.toFixed(2));
        if ($("#Balance").val() == '-0.00')
            $("#Balance").val('0.00');
        $("#ValorPagado").attr("max", max.toFixed(2));
    }
    function calculate() {
        var count = ($("#NumberPhone").val()).trim("\r").split("\t").length;
        var mayorista = $('#selectMayorista').val();
        if (tiporecarga == "celular" && mayorista != "" && mayorista != null) {
            var monto = $('#monto').val();
            var porciento = monto * (porcientocelular / 100);
            $('#precioventa').val(monto);
            $('#costo').val(porciento.toFixed(2));
        }
        var total_pagado = 0;

        var precioTotalValue = parseFloat($("#precioventa").val());
        var costo = parseFloat($('#costo').val());
        if (tiporecarga == "celular") {
            precioTotalValue = precioTotalValue * count;
            costo = costo * count;
        }
        else {
            precioTotalValue = precioTotalValue / countOld * count
            costo = costo / countOld * count
            countOld = count;
        }
        $('#costo').val(costo.toFixed(2));
        if (!modifyPrecioVenta) {
            $('#precioventa').val(precioTotalValue.toFixed(2));
        }

        if ($("#checkpago").is(":checked")) {
            var pagoCash = parseFloat($("#pagoCash").val());
            var pagoZelle = parseFloat($("#pagoZelle").val());
            var pagoCheque = parseFloat($("#pagoCheque").val());
            var pagoTransf = parseFloat($("#pagoTransf").val());
            var pagoWeb = parseFloat($("#pagoWeb").val()); var pagoCrDeb = parseFloat($("#pagoCredito").val());

            var pagoCrDeb = parseFloat($("#pagoCredito").val());
            var pagoCrDebReal = parseFloat((parseFloat($("#pagoCredito").val()) / (1 + parseFloat($('#fee').html()) / 100)));
            var feeCrDeb = pagoCrDeb - pagoCrDebReal;

            if (pagoCrDeb > 0) {
                $('#contfee').show();
            }
            else {
                $('#contfee').hide();
            }
            precioTotalValue += feeCrDeb;

            var pagoCredito = 0;
            if ($("#check_credito").is(":checked")) {
                pagoCredito = parseFloat($("#credito").html()).toFixed(2);
                pagoCredito = pagoCredito > precioTotalValue ? precioTotalValue : pagoCredito;
            }

            balanceValue = precioTotalValue - pagoCash - pagoZelle - pagoCheque - pagoCrDeb - pagoTransf - pagoWeb - pagoCredito;
            total_pagado = pagoCash + pagoZelle + pagoCheque + pagoCrDeb + pagoTransf + pagoWeb;
            $("#pagoCash").attr('max', (balanceValue + pagoCash).toFixed(2));
            $("#pagoZelle").attr('max', (balanceValue + pagoZelle).toFixed(2));
            $("#pagoCheque").attr('max', (balanceValue + pagoCheque).toFixed(2));
            $("#pagoTransf").attr('max', (balanceValue + pagoTransf).toFixed(2));
            $("#pagoWeb").attr('max', (balanceValue + pagoWeb).toFixed(2));
            $("#pagoCredito").attr('max', ((balanceValue + pagoCrDebReal) + (balanceValue + pagoCrDebReal) * parseFloat($('#fee').html()) / 100).toFixed(2));
            $("#pagar_credit").html("$" + ((balanceValue + pagoCrDebReal) + (balanceValue + pagoCrDebReal) * parseFloat($('#fee').html()) / 100).toFixed(2) + " (" + ((balanceValue + pagoCrDebReal) * parseFloat($('#fee').html()) / 100).toFixed(2) + " fee)");

            //Valor Sale Amount en authorization card
            $('#AuthSaleAmount').val(pagoCrDebReal.toFixed(2));
            var aconvcharge = parseFloat($('#AuthConvCharge').val());
            var total = pagoCrDebReal + (pagoCrDebReal * aconvcharge / 100);
            $('#TotalCharge').val(total.toFixed(2));
        }
        else {

            tipoPagoId = $('#tipoPago').val();
            tipoPago = $('option[value = "' + tipoPagoId + '"]').html();

            if (tipoPago == "Crédito o Débito") {
                var fee = parseFloat($('#fee').html());
                //Valor Sale Amount en authorization card
                $('#AuthSaleAmount').val(precioTotalValue.toFixed(2));
                precioTotalValue = precioTotalValue + (precioTotalValue * (fee / 100));
                $('#TotalCharge').val(precioTotalValue.toFixed(2));
            }
            var balanceValue = 0;
            if ($("#check_credito").is(":checked")) {
                if (precioTotalValue.toFixed(2) - parseFloat($("#credito").html()) > 0) {
                    $("#ValorPagado").attr('max', precioTotalValue.toFixed(2) - parseFloat($("#credito").html()));
                    $("#ValorPagado").val((precioTotalValue.toFixed(2) - parseFloat($("#credito").html())).toFixed(2));
                }
                else {
                    $("#ValorPagado").attr('max', 0);
                    $("#ValorPagado").val(0);
                }
            }
            else {
                $("#ValorPagado").val(precioTotalValue.toFixed(2));
                $("#ValorPagado").attr('max', precioTotalValue.toFixed(2));
            }
            total_pagado = precioTotalValue;
        }
        $('#Import').val(precioTotalValue.toFixed(2));
        $("#valorPagado").val(total_pagado.toFixed(2));
        $("#Balance").val(balanceValue.toFixed(2));
        if ($("#Balance").val() == '-0.00')
            $("#Balance").val('0.00');

        modifyPrecioVenta = false;
    }

    calculate();   
});