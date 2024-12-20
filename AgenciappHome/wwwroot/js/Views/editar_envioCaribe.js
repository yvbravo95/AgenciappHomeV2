$(document).ready(function () {
    var dataPrecios = null;
    var precio = 0;

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
    };

    function unblock($this) {
        var block_ele = $this.closest('.card');
        block_ele.unblock()
    };

    function getMayorista() {
        var ContactId = $('[name="ContactId"]').val();
        var idTramite = $('#EnvioCaribeId').val();
        $.ajax({
            url: "/EnviosCaribe/getWholesaler",
            type: "POST",
            data: {
                IdContact: ContactId,
                idTramite, idTramite
            },
            success: function (response) {
                if (response.success) {
                    var data = response.data;
                    dataPrecios = data;

                    var servicio = $('#dataPrecios.precio').val();
                    var provincia = $('#provincia').val();
                    var tabla = $('#tblpaquetes').find('tbody')[0].rows;
                    if (tabla.length > 0) {
                        var row = tabla.item(0);
                        var descripcion = row.cells[3].innerHTML;
                        var peso = $(row.cells[2]).find('input').val();
                        if (descripcion == "Miscelaneos" && peso == "3.30") {
                            precio = pesoTotal * parseFloat(dataPrecios.precio);
                        }
                        else {
                            if (servicio == "Aerovaradero- Recogida") {
                                precio = pesoTotal * 5;
                            }
                            else {
                                if (provincia == "La Habana") {
                                    precio = pesoTotal * 6;
                                }
                                else {
                                    precio = pesoTotal * 7;
                                }
                            }
                            
                        }
                    }

                    costo = pesoTotal * parseFloat(dataPrecios.costo);

                    $('#costo').val(costo);
                    $('[name = "Precio"]').val(precio.toFixed(2));
                    calcular();
                }
                else {
                    dataPrecios = null;
                    precio = 0;
                    $('[name = "wholesalerId"]').val("");
                    $('[name = "Precio"]').val(0);
                    toastr.error(response.msg);
                    console.log(response.exception);
                }
            }
        })
    }

    var checkPago = function () {
        if ($("#checkpago").is(" :checked")) {

            $("#untipopago").attr("hidden", 'hidden');
            $(".multipopago").removeAttr("hidden");
            $('#contfee').hide();
            $("#ValorPagado").val(0);
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
        }
        calcular()
    }

    $("#checkpago").on('click', checkPago);
    checkPago();

    $(document).on('click', '#checkpago', function () {
        if ($("#checkpago").is(" :checked")) {

            $("#untipopago").attr("hidden", 'hidden');
            $(".multipopago").removeAttr("hidden");
            $('#contfee').hide();
            $("#ValorPagado").val(0);
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
        }
        calcular()
    });
        

    function calcular() {
        cservicio = parseFloat($('[name = "OtrosCostos"]').val());
        var balanceValue = 0;
        var precio = parseFloat($('[name = "Precio"]').val());
        var addprecio = parseFloat($('[name = "addPrecio"]').val());
        var amount = precio + cservicio + addprecio;
        var fee = parseFloat($('#AuthConvCharge').val());

        tipopago = $('[value="' + $('#tipoPago').val() + '"]').html();

        if ($("#checkpago").is(":checked")) {
            var pagoCash = parseFloat($("#pagoCash").val());
            var pagoZelle = parseFloat($("#pagoZelle").val());
            var pagoCheque = parseFloat($("#pagoCheque").val());
            var pagoTransf = parseFloat($("#pagoTransf").val());
            var pagoWeb = parseFloat($("#pagoWeb").val());

            var pagoCrDeb = parseFloat($("#pagoCredito").val());
            var pagoCrDebReal = parseFloat((parseFloat($("#pagoCredito").val()) / (1 + fee / 100)));
            var feereal = pagoCrDeb - pagoCrDebReal;
            //Aumento al total el fee
            if (pagoCrDeb > 0) {
                $('#fee').show();
            }
            else {
                $('#fee').hide();
            }

            amount = amount + feereal;

            var pagoCredito = 0;
            if ($("#check_credito").is(":checked")) {
                pagoCredito = parseFloat($("#cred").html()).toFixed(2);
                pagoCredito = pagoCredito > amount ? amount : pagoCredito;
                $("#Credito").val(pagoCredito);
            }
            else {
                $("#Credito").val(0);
            }

            balanceValue = amount - pagoCash - pagoZelle - pagoCheque - pagoCrDeb - pagoTransf - pagoWeb - pagoCredito;

            $("#pagoCash").attr('max', (balanceValue + pagoCash).toFixed(2));
            $("#pagoZelle").attr('max', (balanceValue + pagoZelle).toFixed(2));
            $("#pagoCheque").attr('max', (balanceValue + pagoCheque).toFixed(2));
            $("#pagoTransf").attr('max', (balanceValue + pagoTransf).toFixed(2));
            $("#pagoWeb").attr('max', (balanceValue + pagoWeb).toFixed(2));
            $("#pagoCredito").attr('max', ((balanceValue + pagoCrDebReal) + (balanceValue + pagoCrDebReal) * fee / 100).toFixed(2));
            $("#pagar_credit").html("$" + ((balanceValue + pagoCrDebReal) + (balanceValue + pagoCrDebReal) * fee / 100).toFixed(2) + " (" + ((balanceValue + pagoCrDebReal) * fee / 100).toFixed(2) + " fee)");

            //Valor Sale Amount en authorization card
            $('#AuthSaleAmount').val(pagoCrDebReal.toFixed(2));
            var aconvcharge = parseFloat($('#AuthConvCharge').val());
            var total = pagoCrDebReal + (pagoCrDebReal * aconvcharge / 100);
            $('#TotalCharge').val(total.toFixed(2));
        }
        else {
            if (tipopago == "Crédito o Débito") {
                var pagado = parseFloat($("#ValorPagado").val());
                $('#fee').show();
                var pagdoReal = pagado / (1 + fee / 100)
                var feeCrDeb = pagado - pagdoReal;
                var auxamount = amount;
                total = amount + (amount * (fee / 100));
                amount = amount + feeCrDeb;
                $('#AuthSaleAmount').val(auxamount.toFixed(2));
                $('#TotalCharge').val(amount.toFixed(2));
            }
            else {
                $('#fee').hide();
            }

            if ($("#check_credito").is(":checked")) {
                if (parseFloat((amount - parseFloat($("#cred").html())).toFixed(2)) > 0) {
                    $("#ValorPagado").attr('max', (amount - parseFloat($("#cred").html())).toFixed(2));
                    $("#ValorPagado").val((amount - parseFloat($("#cred").html())).toFixed(2));
                    $("#Credito").val(parseFloat($("#cred").html()));
                }
                else {
                    $("#ValorPagado").attr('max', 0);
                    $("#ValorPagado").val(0);
                    $("#Credito").val(amount);

                }
            }
            else {
                $("#ValorPagado").val((amount).toFixed(2));
                $("#ValorPagado").attr('max', (amount).toFixed(2));
                $("#Credito").val(0);
            }
            balanceValue = amount - parseFloat($("#ValorPagado").val()) - $("#Credito").val();

        }

        $('[name="Amount"]').val(amount.toFixed(2));
        $('[name="Balance"]').val(balanceValue.toFixed(2));
        $('#balanceValue').html(balanceValue.toFixed(2));

        if ($('[name="Balance"]').val() == '-0.00') {
            $('[name="Balance"]').val('0.00');
            $('#balanceValue').html("0.00");

        }

        $('#precioTotalValue').html(amount.toFixed(2));

    };

    function calcularValorPagado() {
        cservicio = parseFloat($('[name = "OtrosCostos"]').val());
        var balanceValue = 0;
        var precio = parseFloat($('[name = "Precio"]').val());
        var addprecio = parseFloat($('[name = "addPrecio"]').val());
        var fee = parseFloat($('#AuthConvCharge').val());

        var amount = precio + cservicio + addprecio;
        var max = amount;

        tipopago = $('[value="' + $('#tipoPago').val() + '"]').html();

        if ($("#checkpago").is(":checked")) {
            var pagoCash = parseFloat($("#pagoCash").val());
            var pagoZelle = parseFloat($("#pagoZelle").val());
            var pagoCheque = parseFloat($("#pagoCheque").val());
            var pagoTransf = parseFloat($("#pagoTransf").val());
            var pagoWeb = parseFloat($("#pagoWeb").val());

            var pagoCrDeb = parseFloat($("#pagoCredito").val());
            var pagoCrDebReal = parseFloat((parseFloat($("#pagoCredito").val()) / (1 + fee / 100)));
            var feereal = pagoCrDeb - pagoCrDebReal;
            //Aumento al total el fee
            amount = amount + feereal;

            if (pagoCrDeb > 0) {
                $('#fee').show();
            }
            else {
                $('#fee').hide();
            }
            var pagoCredito = 0;
            if ($("#check_credito").is(":checked")) {
                pagoCredito = parseFloat($("#cred").html()).toFixed(2);
                pagoCredito = pagoCredito > amount ? amount : pagoCredito;
            }
            balanceValue = amount - pagoCash - pagoZelle - pagoCheque - pagoCrDeb - pagoTransf - pagoWeb - pagoCredito;
            $("#pagoCash").attr('max', (balanceValue + pagoCash).toFixed(2));
            $("#pagoZelle").attr('max', (balanceValue + pagoZelle).toFixed(2));
            $("#pagoCheque").attr('max', (balanceValue + pagoCheque).toFixed(2));
            $("#pagoTransf").attr('max', (balanceValue + pagoTransf).toFixed(2));
            $("#pagoWeb").attr('max', (balanceValue + pagoWeb).toFixed(2));
            $("#pagoCredito").attr('max', ((balanceValue + pagoCrDebReal) + (balanceValue + pagoCrDebReal) * fee / 100).toFixed(2));
            $("#pagar_credit").html("$" + ((balanceValue + pagoCrDebReal) + (balanceValue + pagoCrDebReal) * fee / 100).toFixed(2) + " (" + ((balanceValue + pagoCrDebReal) * fee / 100).toFixed(2) + " fee)");

            //Valor Sale Amount en authorization card
            $('#AuthSaleAmount').val(pagoCrDebReal.toFixed(2));
            var aconvcharge = parseFloat($('#AuthConvCharge').val());
            var total = pagoCrDebReal + (pagoCrDebReal * aconvcharge / 100);
            $('#TotalCharge').val(total.toFixed(2));
        }
        else {
            if (tipopago == "Crédito o Débito") {
                $('#fee').show();
                var pagado = parseFloat($("#ValorPagado").val());
                var pagdoReal = pagado / (1 + fee / 100)
                var feeCrDeb = pagado - pagdoReal;
                max = amount + (amount * (fee / 100));
                amount = amount + feeCrDeb;
                $('#AuthSaleAmount').val(pagdoReal.toFixed(2));
                $('#TotalCharge').val(pagado);
            }
            else {
                $('#fee').hide();
            }

            if ($("#check_credito").is(":checked")) {
                if (parseFloat((amount - parseFloat($("#cred").html())).toFixed(2)) > 0) {
                    $("#ValorPagado").attr('max', parseFloat((amount - parseFloat($("#cred").html())).toFixed(2)));
                }
                else {
                    $("#ValorPagado").attr('max', 0);
                    $("#ValorPagado").val(0);
                }
            }
            else {
                $("#ValorPagado").attr('max', (max).toFixed(2));
            }
            balanceValue = amount - parseFloat($("#ValorPagado").val()) - $("#Credito").val();
        }

        $('[name="Amount"]').val(amount.toFixed(2));
        $('[name="Balance"]').val(balanceValue.toFixed(2));
        $('#balanceValue').html(balanceValue.toFixed(2));

        if ($('[name="Balance"]').val() == '-0.00') {
            $('[name="Balance"]').val('0.00');
            $('#balanceValue').html("0.00");
        }
        $('#precioTotalValue').html(amount.toFixed(2));
    };

    $(document).on('click', '#btnprecioscosto', function () {
        $('[name = "addPrecio"]').val($('#addprecio').val());
        calcular();
    });

    //Cuando se cambie el valor pagado se calcule el balance
    function changeBalance() {
        var valorpagado = parseFloat($('[name="ValorPagado"]').val());
        var amount = parseFloat($('[name="Amount"]').val());
        var balance = amount - valorpagado - $("#Credito").val();
        $('[name="Balance"]').val(balance.toFixed(2));

    }

    var validarInputVacios = function () {
        if ($("#pagoCash").val() == "")
            $("#pagoCash").val(0);
        if ($("#pagoZelle").val() == "")
            $("#pagoZelle").val(0);
        if ($("#pagoCheque").val() == "")
            $("#pagoCheque").val(0);
        if ($("#pagoCredito").val() == "")
            $("#pagoCredito").val(0);
        if ($("#pagoTransf").val() == "")
            $("#pagoTransf").val(0);
        if ($("#pagoWeb").val() == "")
            $("#pagoWeb").val(0);
        if ($("#ValorPagado").val() == "")
            $("#ValorPagado").val(0);
        calcular();
    }

    $(document).on("keyup", "#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb", calcular);
    $(document).on("change", "#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb", calcular);
    $(document).on("keyup", "#ValorPagado", calcularValorPagado);
    $(document).on("change", "#ValorPagado", calcularValorPagado);

    $(document).on("change", '#tipoPago', calcular);
    $(document).on("keyup", '[name="OtrosCostos"]', calcular).on("change", '[name="OtrosCostos"]', calcular);
    $(document).on("keyup", '[name="Precio"]', calcular).on("change", '[name="Precio"]', calcular);


    /**********************************
         *   Form Wizard Step Icon
    **********************************/
    $('.step-icon').each(function () {
        var $this = $(this);
        if ($this.siblings('span.step').length > 0) {
            $this.siblings('span.step').empty();
            $(this).appendTo($(this).siblings('span.step'));
        }
    });

    $("#zc").steps({
        headerTag: "h6",
        bodyTag: "fieldset",
        transitionEffect: "fade",
        titleTemplate: '<span class="step">#index#</span> #title#',
        labels: {
            previous: "Anterior",
            next: "Siguiente",
            finish: 'Enviar'
        },
        onStepChanging: function (event, currentIndex, newIndex) {
            // Allways allow previous action even if the current form is not valid!
            if (currentIndex > newIndex) {
                return true;
            }

            //----------------Step1
            var error = false

            if (newIndex == 0) {

            }

            if (newIndex == 1) {
                if ($("#single-placeholder-2").val() == "" || $("#single-placeholder-3").val() == "") {
                    return false;
                }

                if ($("#inputClientMovil").val() == "") {
                    showWarningMessage("Atención", "El campo de teléfono del cliente es obligatorio.");
                    return false;
                }
                if ($("#provincia").val() == null) {
                    showWarningMessage("Atención", "El campo de provincia del contacto es obligatorio.");
                    return false;
                }

                //Si esta en estado iniciada no recalcular
                if (provinciaActual != $('#provincia').val() || $('#Status').val() == "Pendiente") {
                    getMayorista();
                }
                else {
                    precio = $('[name = "Precio"]').val();
                }
            }

            //----------------Step2

            if (newIndex == 2) {

            }
            return true;
        },
        onFinishing: function (event, currentIndex) {
            var id = $('#tipoPago').val();
            var value = $('option[value = "' + id + '"]').html();
            var pagocredito = parseFloat($('#pagoCredito').val());
            var multiplespagos = $('#checkpago').is(':checked');
            if ((multiplespagos == false && value != "Crédito o Débito") || (multiplespagos == true && pagocredito == 0)) {
                $('[data-tipo="authcard"]').prop('disabled', true);
            }
            
            return $('#zc').submit();
        },
        onFinished: function (event, currentIndex) {


        }
    });
    $('ul[aria-label="Pagination"]').prepend(
        '<li class="" aria-disabled="false"><a id="cancel" style="background-color:#ff7e7e;" href="/EnviosCaribe/index/" role="menuitem">Cancelar</a></li>'
    );

    $("a[href='#next']").addClass("hidden");
    $('#cancel').hide();

    $("a[href=#previous]").hide();
    $("a[href=#previous]").click(function () {
        $("#showAllContacts").removeAttr('disabled')
        $("a[href=#previous]").hide();
    })
    $("a[href=#next]").click(function () {
        $("#showAllContacts").attr('disabled', 'disabled')

        $("a[href=#previous]").show();
    })

/*********************************************/


    //placeholder cliente y contacto

    $("#provincia").on("change", () => selectMunicipios());

    var selectMunicipios = function () {
        var provincia = $("#provincia").val();
        if (!provincia)
            return;

        $.ajax({
            url: "/Provincias/Municipios?nombre=" + provincia,
            type: "POST",
            dataType: "json",
            success: function (response) {
                var municipios = $("#municipio");
                municipios.empty();
                municipios.append(new Option())
                for (var i in response) {
                    var m = response[i];
                    municipios.append(new Option(m, m))
                }
                municipios.val(selected_municipio).trigger("change");
            }
        })
    }
    $("#municipio").select2({
        placeholder: "Municipio"
    });

    $(".select2-placeholder-selectClient").select2({
        placeholder: "Buscar cliente por teléfono, nombre o apellido",
        val: null,
        ajax: {
            type: 'POST',
            dataType: "json",
            delay: 500,
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

    $(".hide-search-clientState").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Estado",
    });

    $(".select2-placeholder-selectContact").select2({
        placeholder: "Buscar contacto por teléfono, nombre o apellido",
        text: " ",
        ajax: {
            type: 'POST',
            url: '/Contacts/findContacts',
            data: function (params) {
                var query = {
                    search: params.term,
                    idClient: $('.select2-placeholder-selectClient').val()
                }

                // Query parameters will be ?search=[term]&type=public
                return query;
            },
            processResults: function (data) {

                // Transforms the top-level key of the response object from 'items' to 'results'
                return {
                    results: $.map(data, function (obj) {

                        return { id: obj.contactId, text: obj.phone1 + "-" + obj.name + " " + obj.lastName };
                    })
                };
            }
        }
    });

    $(".hide-search-contactCity2").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Provincia",
    });

    $('#inputClientMovil').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });

    $('#inputContactPhoneMovil').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });
    $('#contactCI').mask('00000000000', {
        placeholder: "Carnet de Identidad"
    });

    $('#inputContactPhoneHome').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });


    //placeholder orden

    $(".hide-search-pago").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Tipo de Pago",
    });

    $(".select2-placeholder-valor").select2({
        placeholder: "Valor Aduanal",

    });

    $(".select2-placeholder-selectProduct").select2({
        placeholder: "Buscar producto por tipo, color, talla o marca",
    });
    /**********Clientes y Contactos ***********/

    var showAllClients = function () {
        $.ajax({
            type: "POST",
            url: "/OrderNew/GetAllClients",
            dataType: 'json',
            contentType: 'application/json',
            async: false,
            success: function (data) {
                $("[name='ClientId']").empty();
                $("[name='ClientId']").append(new Option());
                if (data.length == 0) {
                    $('#inputClientName').val("");
                    $('#inputClientLastName').val("");
                    $('#inputClientMovil').val("");
                    $('#inputClientEmail').val("");
                    $('#inputClientAddress').val("");
                    $('#inputClientCity').val("");
                    $('#inputClientZip').val("");
                    $('#inputClientState').val("").trigger("change");
                } 
                else {
                    for (var i = 0; i < data.length; i++) {
                        var newOption = new Option(data[i].fullData, data[i].clientId, false, false);
                        $("[name='ClientId']").append(newOption);
                    }

                    if (selectedClient != '00000000-0000-0000-0000-000000000000') {
                        $(".select2-placeholder-selectClient").val(selectedClient).trigger("change").trigger("select2:select");
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
    };

    var showClient = function () {
        var value = $(".select2-placeholder-selectClient").val();
        selectedClient = value;
        $.ajax({
            type: "POST",
            url: "/OrderNew/GetClient",
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(value),
            async: false,
            success: function (data) {
                //Datos del Cliente en Authorization Card
                $('#nameClientCard').html('<strong>Nombre: </strong>' + data.name + ' ' + data.lastName);
                $('#phoneClientCard').html('<strong>Teléfono: </strong>' + data.movil);
                $('#emailClientCard').html('<strong>Email: </strong>' + data.email);
                $('#countryClientCard').html('<strong>País: </strong>' + data.country);
                $('#cityClientCard').html('<strong>Ciudad: </strong>' + data.city);
                $('#addressClientCard').html('<strong>Dirección: </strong>' + data.calle);
                $('#AuthaddressOfSend').val(data.calle);
                $('#Authemail').val(data.email);
                $('#Authphone').val(data.movil);

                //Datos del Cliente en Step 1
                $('#inputClientName').val(data.name);
                $('#inputClientLastName').val(data.lastName);
                $('#inputClientMovil').val(data.movil);
                $('#inputClientEmail').val(data.email);
                $('#inputClientAddress').val(data.calle);
                $('#inputClientCity').val(data.city);
                $('#inputClientZip').val(data.zip);
                $('#inputClientState').val(data.state).trigger("change");

                $('#remitente').html(data.name + " " + data.lastName);

                if (data.getCredito && data.getCredito != 0) {
                    $("#div_credito").removeAttr('hidden');
                    $("#cred").html(data.getCredito);
                }
                else {
                    $("#div_credito").attr('hidden', "hidden");
                }
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            }
        });
    }

    var showAllContact = function () {
        $.ajax({
            type: "POST",
            url: "/OrderNew/GetAllContacts",
            dataType: 'json',
            contentType: 'application/json',
            async: false,
            success: function (data) {
                // Contactos del cliente
                $("[name='ContactId']").empty();
                $("[name='ContactId']").append(new Option());

                var contactData = "";
                for (var i = 0; i < data.length; i++) {
                    if (data[i].phones1 != "")
                        contactData = data[i].phone1 + " - " + data[i].name + " " + data[i].lastName;
                    else
                        contactData = data[i].phone2 + " - " + data[i].name + " " + data[i].lastName;
                    $("[name='ContactId']").append(new Option(contactData, data[i].contactId));
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
    var showContact = function () {
        var idContacto = $('.select2-placeholder-selectContact').val();
        if (idContacto != null) {
            $.ajax({
                type: "POST",
                url: "/OrderNew/GetContact",
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(idContacto),
                async: false,
                success: function (data) {
                    $('#inputContactName').val(data.name);
                    $('#inputContactLastName').val(data.lastName);
                    $('#inputContactPhoneMovil').val(data.movilPhone);
                    $('#inputContactPhoneHome').val(data.casaPhone);
                    $('#contactDireccion').val(data.direccion);
                    $('#provincia').val(data.city).trigger("change");
                    $('#municipio').val(data.municipio);
                    $('#reparto').val(data.reparto);
                    $('#contactCI').val(data.ci);


                    $('#destinatario').html(data.name + " " + data.lastName);

                    $("a[href='#next']").removeClass("hidden");
                    $('#cancel').show();

                    //getprecioxlibra();
                    selected_municipio = data.municipio;
                },
                failure: function (response) {
                    showErrorMessage("ERROR", response.statusText);
                },
                error: function (response) {
                    showErrorMessage("ERROR", response.statusText);
                }
            });
        }
    }

    var showContactsOfAClient = function () {
        var idClient = $(".select2-placeholder-selectClient").val();
        $('#inputContactName').val("");
        $('#inputContactLastName').val("");
        $('#inputContactPhoneMovil').val("");
        $('#inputContactPhoneHome').val("");
        $('#contactDireccion').val("");
        $('#provincia').val("").trigger("change");
        $('#municipio').val("");
        $('#reparto').val("");
        $('#contactCI').val("");

        $.ajax({
            type: "GET",
            url: "/Contacts/GetContactsOfAClient?id=" + idClient,
            dataType: 'json',
            contentType: 'application/json',
            async: true,
            success: function (data) {
                $("[name='ContactId']").empty();
                $("[name='ContactId']").append(new Option());

                if (data.length == 0) {
                    //showAllContact();
                }
                else {
                    for (var i = 0; i < data.length; i++) {
                        var contactData;
                        if (data[i].phone1 != "")
                            contactData = data[i].phone1 + " - " + data[i].name + " " + data[i].lastName;
                        else
                            contactData = data[i].phone2 + " - " + data[i].name + " " + data[i].lastName;;
                        $("[name='ContactId']").append(new Option(contactData, data[i].contactId));
                    }
                    var last = data[data.length - 1];
                    if (selectedContact != null && selectedContact != "00000000-0000-0000-0000-000000000000") {
                        $("[name='ContactId']").val(selectedContact).trigger("change");
                        selectedContact = "00000000-0000-0000-0000-000000000000";
                    }
                    else {
                        $("[name='ContactId']").val(last.contactId).trigger("change");

                    }
                    showContact();
                    $("#editarContacto, a[href='#next']").removeClass("hidden");
                    $('#cancel').show();

                }
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    };



    var mostrarEstados = function () {
        $("#inputClientState").empty()
        $("#inputClientState").append(new Option())
        $("#inputClientState").append(new Option("Alabama", "Alabama"))
        $("#inputClientState").append(new Option("Alaska", "Alaska"))
        $("#inputClientState").append(new Option("American Samoa", "American Samoa"))
        $("#inputClientState").append(new Option("Arizona", "Arizona"))
        $("#inputClientState").append(new Option("Arkansas", "Arkansas"))
        $("#inputClientState").append(new Option("Armed Forces Americas", "Armed Forces Americas"))
        $("#inputClientState").append(new Option("Armed Forces Europe, Canada, Africa and Middle East", "Armed Forces Europe, Canada, Africa and Middle East"))
        $("#inputClientState").append(new Option("Armed Forces Pacific", "Armed Forces Pacific"))
        $("#inputClientState").append(new Option("California", "California"))
        $("#inputClientState").append(new Option("Colorado", "Colorado"))
        $("#inputClientState").append(new Option("Connecticut", "Connecticut"))
        $("#inputClientState").append(new Option("Delaware", "Delaware"))
        $("#inputClientState").append(new Option("District of Columbia", "District of Columbia"))
        $("#inputClientState").append(new Option("Florida", "Florida"))
        $("#inputClientState").append(new Option("Georgia", "Georgia"))
        $("#inputClientState").append(new Option("Guam", "Guam"))
        $("#inputClientState").append(new Option("Hawaii", "Hawaii"))
        $("#inputClientState").append(new Option("Idaho", "Idaho"))
        $("#inputClientState").append(new Option("Illinois", "Illinois"))
        $("#inputClientState").append(new Option("Indiana", "Indiana"))
        $("#inputClientState").append(new Option("Iowa", "Iowa"))
        $("#inputClientState").append(new Option("Kansas", "Kansas"))
        $("#inputClientState").append(new Option("Kentucky", "Kentucky"))
        $("#inputClientState").append(new Option("Louisiana", "Louisiana"))
        $("#inputClientState").append(new Option("Maine", "Maine"))
        $("#inputClientState").append(new Option("Marshall Islands", "Marshall Islands"))
        $("#inputClientState").append(new Option("Maryland", "Maryland"))
        $("#inputClientState").append(new Option("Massachusetts", "Massachusetts"))
        $("#inputClientState").append(new Option("Michigan", "Michigan"))
        $("#inputClientState").append(new Option("Micronesia", "Micronesia"))
        $("#inputClientState").append(new Option("Minnesota", "Minnesota"))
        $("#inputClientState").append(new Option("Mississippi", "Mississippi"))
        $("#inputClientState").append(new Option("Missouri", "Missouri"))
        $("#inputClientState").append(new Option("Montana", "Montana"))
        $("#inputClientState").append(new Option("Nebraska", "Nebraska"))
        $("#inputClientState").append(new Option("Nevada", "Nevada"))
        $("#inputClientState").append(new Option("New Hampshire", "New Hampshire"))
        $("#inputClientState").append(new Option("New Jersey", "New Jersey"))
        $("#inputClientState").append(new Option("New Mexico", "New Mexico"))
        $("#inputClientState").append(new Option("New York", "New York"))
        $("#inputClientState").append(new Option("North Carolina", "North Carolina"))
        $("#inputClientState").append(new Option("North Dakota", "North Dakota"))
        $("#inputClientState").append(new Option("Northern Mariana Islands", "Northern Mariana Islands"))
        $("#inputClientState").append(new Option("Ohio", "Ohio"))
        $("#inputClientState").append(new Option("Oklahoma", "Oklahoma"))
        $("#inputClientState").append(new Option("Oregon", "Oregon"))
        $("#inputClientState").append(new Option("Palau", "Palau"))
        $("#inputClientState").append(new Option("Pennsylvania", "Pennsylvania"))
        $("#inputClientState").append(new Option("Puerto Rico", "Puerto Rico"))
        $("#inputClientState").append(new Option("Rhode Island", "Rhode Island"))
        $("#inputClientState").append(new Option("South Carolina", "South Carolina"))
        $("#inputClientState").append(new Option("South Dakota", "South Dakota"))
        $("#inputClientState").append(new Option("Tennessee", "Tennessee"))
        $("#inputClientState").append(new Option("Texas", "Texas"))
        $("#inputClientState").append(new Option("Utah", "Utah"))
        $("#inputClientState").append(new Option("Vermont", "Vermont"))
        $("#inputClientState").append(new Option("Virgin Islands", "Virgin Islands"))
        $("#inputClientState").append(new Option("Virginia", "Virginia"))
        $("#inputClientState").append(new Option("Washington", "Washington"))
        $("#inputClientState").append(new Option("West Virginia", "West Virginia"))
        $("#inputClientState").append(new Option("Wisconsin", "Wisconsin"))
        $("#inputClientState").append(new Option("Wyoming", "Wyoming"))
    }

    var validateEditarCliente = function () {
        if ($("#inputClientName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($("#inputClientLastName").val() == "") {
            showWarningMessage("Atención", "El campo Apellidos no puede estar vacío.");
            return false;
        } else if ($("#inputClientMovil").val() == "") {
            showWarningMessage("Atención", "El campo Teléfono no puede estar vacío.");
            return false;
        }

        if ($("#inputClientEmail").val() != "") {
            var regexEmail = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
            if (!regexEmail.test($("#inputClientEmail").val())) {
                showWarningMessage("Atención", "El campo Email no tiene el formato correcto.");
                return false;
            }
        }

        return true;
    };

    var desactClientForm = function () {
        $('#nuevoCliente').removeAttr("disabled");
        $(".select2-placeholder-selectClient").removeAttr("disabled");
        $('#nuevoContacto').removeAttr("disabled");
        $('.select2-placeholder-selectContact').removeAttr("disabled");
        $('#editarContacto').removeAttr("disabled");

        $('#inputClientName').attr("disabled", "disabled");
        $('#inputClientLastName').attr("disabled", "disabled");
        $('#inputClientMovil').attr("disabled", "disabled");
        $('#inputClientEmail').attr("disabled", "disabled");
        $('#inputClientAddress').attr("disabled", "disabled");
        $('#inputClientCity').attr("disabled", "disabled");
        $('#inputClientState').attr("disabled", "disabled");
        $('#inputClientZip').attr("disabled", "disabled");

        $('#editarCliente').removeClass("hidden");
        $("#cancelarCliente").addClass("hidden");
        $("#guardarCliente").addClass("hidden");

        if ($("#inputContactName").val() != null) {
            $("a[href='#next']").removeClass("hidden");
            $('#cancel').show();

        }
        $("#showAllContacts").removeAttr('disabled');

    }

    var cancelClientForm = function () {
        $('#inputClientName').val($('#inputClientName').data("prevVal"));
        $('#inputClientLastName').val($('#inputClientLastName').data("prevVal"));
        $('#inputClientMovil').val($('#inputClientMovil').data("prevVal"));
        $('#inputClientEmail').val($('#inputClientEmail').data("prevVal"));
        $('#inputClientAddress').val($('#inputClientAddress').data("prevVal"));
        $('#inputClientCity').val($('#inputClientCity').data("prevVal"));
        $('#inputClientState').val($('#inputClientState').data("prevVal")).trigger("change");
        $('#inputClientZip').val($('#inputClientZip').data("prevVal"));
        desactClientForm();
    }

    var validateEditarContacto = function () {
        if ($("#inputContactName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($("#inputContactLastName").val() == "") {
            showWarningMessage("Atención", "El campo Apellidos no puede estar vacío.");
            return false;
        } else if ($("#inputContactPhoneMovil").val() == "" && $("#inputContactPhoneHome").val() == "") {
            showWarningMessage("Atención", "Debe introducir al menos un teléfono de contacto.");
            return false;
        } else if ($('#contactCI').val().length > 0) {
            if ($('#contactCI').val().length != 11) {
                showWarningMessage("Atención", "El carnet de identidad debe tener 11 dígitos");
                return false;
            }
        }

        else if ($("#contactDireccion").val() == "") {
            showWarningMessage("Atención", "El campo Dirección no puede estar vacío");
            return false;
        }
        else if ($("#provincia").val() == "") {
            showWarningMessage("Atención", "El campo Provincia no puede estar vacío.");
            return false;
        } else if ($("#municipio").val() == "") {
            showWarningMessage("Atención", "El campo Municipio no puede estar vacío.");
            return false;
        }
        return true;
    };

    var desactContactForm = function () {
        $('#nuevoCliente').removeAttr("disabled");
        $('.select2-placeholder-selectClient').removeAttr("disabled");
        $('#nuevoContacto').removeAttr("disabled");
        $('.select2-placeholder-selectContact').removeAttr("disabled");
        $('#showAllContacts').removeAttr("disabled");
        $('#editarCliente').removeAttr("disabled");

        $('#inputContactName').attr("disabled", "disabled");
        $('#inputContactLastName').attr("disabled", "disabled");
        $('#inputContactPhoneMovil').attr("disabled", "disabled");
        $('#inputContactPhoneHome').attr("disabled", "disabled");
        $('#contactDireccion').attr("disabled", "disabled");
        $('#provincia').attr("disabled", "disabled");
        $('#municipio').attr("disabled", "disabled");
        $('#reparto').attr("disabled", "disabled");
        $('#contactCI').attr("disabled", "disabled");

        $('#editarContacto').removeClass("hidden");
        $("#cancelarContacto").addClass("hidden");
        $("#guardarContacto").addClass("hidden");

        $("a[href='#next']").removeClass("hidden");
        $('#cancel').show();

        $("#showAllContacts").attr('disabled', 'disabled')

    }

    var cancelarContactForm = function () {
        $('#inputContactName').val($('#inputContactName').data("prevVal"));
        $('#inputContactLastName').val($('#inputContactLastName').data("prevVal"));
        $('#inputContactPhoneMovil').val($('#inputContactPhoneMovil').data("prevVal"));
        $('#inputContactPhoneHome').val($('#inputContactPhoneHome').data("prevVal"));
        $('#contactDireccion').val($('#contactDireccion').data("prevVal"));
        $('#provincia').val($('#provincia').data("prevVal")).trigger("change");
        $('#reparto').val($('#reparto').data("prevVal"));
        $('#municipio').val($('#municipio').data("prevVal"));
        $('#contactCI').val($('#contactCI').data("prevVal"));

        desactContactForm();
    }

    $("#check_credito").on('click', function () {
        $("#pagoCash").val(0);
        $("#pagoZelle").val(0);
        $("#pagoCheque").val(0);
        $("#pagoCredito").val(0);
        $("#pagoTransf").val(0);
        $("#pagoWeb").val(0);
        calcular();
    });

    $(".select2-placeholder-selectClient").on("select2:select", function (a, b) {
        $('.select2-placeholder-selectContact').removeAttr("disabled");
        $('#nuevoContacto').removeAttr("disabled");
        $('#showAllContacts').removeAttr("disabled");
        $('#editarCliente').removeClass("hidden");

        $("a[href='#next']").addClass("hidden");
        $('#cancel').hide();

        showClient();

        showContactsOfAClient();
    }); 

    $('.select2-placeholder-selectContact').on("select2:select", function () {
        $('#editarContacto').removeClass("hidden hide-search-contactCity");
        showContact();
        //getprecioxlibra();
    });

    $("#showAllContacts").on('click', function (e) {
        e.preventDefault();
        //showAllContact();
    });

    //se activan los campos desactivados y se muestran los botones guardar y cancelar
    $('#editarCliente').on('click', function () {
        // para que no pueda crear nuevo cliente mientras edita cliente
        $('#nuevoCliente').attr("disabled", "disabled");

        // para que no pueda cambiar de cliente mientras edita cliente
        $('.select2-placeholder-selectClient').attr("disabled", "disabled");

        // para que no pueda crear nuevo contacto mientras edita cliente
        $('#nuevoContacto').attr("disabled", "disabled");

        // para que no pueda cambiar de contacto mientras edita cliente
        $('.select2-placeholder-selectContact').attr("disabled", "disabled");

        // para que no pueda editar contacto mientras edita cliente
        $('#editarContacto').attr("disabled", "disabled");

        $("a[href='#next']").addClass("hidden");
        $('#cancel').hide();


        $("#showAllContacts").attr('disabled', 'disabled');


        $('#inputClientName').removeAttr("disabled").data("prevVal", $('#inputClientName').val());
        $('#inputClientLastName').removeAttr("disabled").data("prevVal", $('#inputClientLastName').val());
        $('#inputClientMovil').removeAttr("disabled").data("prevVal", $('#inputClientMovil').val());
        $('#inputClientEmail').removeAttr("disabled").data("prevVal", $('#inputClientEmail').val());
        $('#inputClientAddress').removeAttr("disabled").data("prevVal", $('#inputClientAddress').val());
        $('#inputClientCity').removeAttr("disabled").data("prevVal", $('inputClientCity').val());
        $('#inputClientState').removeAttr("disabled").data("prevVal", $('#inputClientState').val());
        $('#inputClientZip').removeAttr("disabled").data("prevVal", $('#inputClientZip').val());

        $('#editarCliente').addClass("hidden");
        $("#cancelarCliente").removeClass("hidden");
        $("#guardarCliente").removeClass("hidden");
    });

    $("#cancelarCliente").click(cancelClientForm);

    $('#guardarCliente').on('click', function () {
        if (validateEditarCliente()) {
            var source = [
                $(".select2-placeholder-selectClient").val(),
                $('#inputClientName').val(),
                $('#inputClientLastName').val(),
                $('#inputClientEmail').val(),
                $('#inputClientMovil').val(),
                $('#inputClientAddress').val(),
                $('#inputClientCity').val(),
                $('#inputClientState').val(),
                $('#inputClientZip').val(),
                "",
                "",
                ""
            ];
            $.ajax({
                type: "POST",
                url: "/Clients/EditClient",
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
                    showClient();
                },
                failure: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                },
                error: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                }
            });

            desactClientForm();
        }
    });

    //se activan los campos desactivados y se muestran los botones guardar y cancelar
    $('#editarContacto').on('click', function () {
        // para que no pueda crear nuevo cliente mientras edita contacto
        $('#nuevoCliente').attr("disabled", "disabled");

        // para que no pueda cambiar de cliente mientras edita contacto
        $('.select2-placeholder-selectClient').attr("disabled", "disabled");

        // para que no pueda crear nuevo contacto mientras edita contacto
        $('#nuevoContacto').attr("disabled", "disabled");

        // para que no pueda cambiar de contacto mientras edita contacto
        $('.select2-placeholder-selectContact').attr("disabled", "disabled");

        // para que no pueda cambiar de contacto mientras edita contacto
        $('#showAllContacts').attr("disabled", "disabled");

        // para que no pueda editar cliente mientras edita contacto
        $('#editarCliente').attr("disabled", "disabled");

        // para que no pueda avanzar a la otra parte del formulario
        $("a[href='#next']").addClass("hidden");
        $('#cancel').hide();


        $('#inputContactName').removeAttr("disabled").data("prevVal", $('#inputContactName').val());
        $('#inputContactLastName').removeAttr("disabled").data("prevVal", $('#inputContactLastName').val());
        $('#inputContactPhoneMovil').removeAttr("disabled").data("prevVal", $('#inputContactPhoneMovil').val());
        $('#inputContactPhoneHome').removeAttr("disabled").data("prevVal", $('#inputContactPhoneHome').val());
        $('#contactDireccion').removeAttr("disabled").data("prevVal", $('#contactDireccion').val());
        $('#provincia').removeAttr("disabled").data("prevVal", $('#provincia').val());
        $('#municipio').removeAttr("disabled").data("prevVal", $('#municipio').val());
        $('#reparto').removeAttr("disabled").data("prevVal", $('#reparto').val());
        $('#contactCI').removeAttr("disabled").data("prevVal", $('#contactCI').val());

        $('#editarContacto').addClass("hidden");
        $("#cancelarContacto").removeClass("hidden");
        $("#guardarContacto").removeClass("hidden");
    });

    $("#cancelarContacto").click(cancelarContactForm);

    $('#guardarContacto').on('click', function () {
        selectedContact = $('.select2-placeholder-selectContact').val();
        if (validateEditarContacto()) {
            var source = [
                $('.select2-placeholder-selectContact').val(),
                $('#inputContactName').val(),
                $('#inputContactLastName').val(),
                $('#inputContactPhoneMovil').val(),
                $('#inputContactPhoneHome').val(),
                $('#contactDireccion').val(),
                $('#provincia').val(),
                $('#municipio').val(),
                $('#reparto').val(),
                $('#contactCI').val(),
                $(".select2-placeholder-selectClient").val(),

            ];

            $.ajax({
                type: "POST",
                url: "/Contacts/EditContact",
                data: JSON.stringify(source),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function () {
                    showOKMessage("Editar Contacto", "Contacto editado con éxito");

                    showContactsOfAClient();
                },
                failure: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                },
                error: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                }

            });

            desactContactForm();
        }
    });

    $("#nuevoContacto").attr('disabled', 'disabled');

    //showAllClients();

    mostrarEstados();

    /***********DATOS DE LA ORDEN *************/

    var tipo_orden_sufij = "EM";

    var setNoOrden = function () {
        var time = $("#time").html();
        $('#no_orden').html(tipo_orden_sufij + time);
    };

    var addProductToTable = function (cant, descripcion) {
        //Get the reference of the Table's TBODY element.
        var tBody = $("#tblProductos > TBODY")[0];

        var index = $(tBody).children().length - 1;

        ////Add Row.
        var row = tBody.insertRow(index);


        var cell1 = $(row.insertCell(-1));
        cell1.append(cant);

        var cell5 = $(row.insertCell(-1));
        cell5.append(descripcion);

        ////Add Button cell.
        var cell6 = $(row.insertCell(-1));
        var btnEdit = $("<button type='button' class='btn btn-warning' title='Editar' style='font-size: 10px'><i class='fa fa-pencil'></i></button>");
        var btnRemove = $("<button type='button' class='btn btn-danger pull-right' title='Eliminar' style='font-size: 10px'><i class=' fa fa-close'></button>");
        var btnConfirm = $("<button type='button' class='btn btn-success hidden' title='Confirmar' style='font-size: 10px'><i class='fa fa-check'></button>");
        btnEdit.on("click", function () {
            cell1.html("<input type='number' name='cell1' class='form-control' value='" + cell1.html() + "'/>");
            cell5.html("<input name='cell5' class='form-control' value='" + cell5.html() + "'/>");

            btnConfirm.removeClass("hidden");
            btnEdit.addClass("hidden");
            btnRemove.addClass("hidden");
        });
        btnRemove.on("click", function () {
            row.remove();
        });
        btnConfirm.on("click", function () {
            if (validateEditProduct()) {
                cell1.html($("[name='cell1']").val());
                cell5.html($("[name='cell5']").val());

                btnConfirm.addClass("hidden");
                btnEdit.removeClass("hidden");
                btnRemove.removeClass("hidden");
            }
        });
        cell6.append(btnEdit.add(btnRemove).add(btnConfirm));
    };

    var validateAddProduct = function () {
        if ($("#newCant").val() == "") {
            showWarningMessage("Atención", "El campo Cantidad no puede estar vacío.");
            return false;
        }
        if ($("#newDescrip").val() == "") {
            showWarningMessage("Atención", "El campo Tipo no puede estar vacío.");
            return false;
        }

        return true;
    };

    var validateEditProduct = function () {
        if ($("[name='cell1']").val() == "") {
            showWarningMessage("Atención", "El campo Cantidad no puede estar vacío.");
            return false;
        } else if ($("[name='cell5']").val() == "") {
            showWarningMessage("Atención", "El campo Tipo no puede estar vacío.");
            return false;
        } 
        return true;
    }


    function salvarImagesClient() {
        var fileID = $('#IdImg')[0].files[0];
        var textoId = $('#TextoIDImg').val();

        var filePasaporte = $('#pasaporteImg')[0].files[0];
        var fechapasaporte = $('#FechaPasaporteImg').val();
        var clientId = $('[name="ClientId"]').val();
        var formData = new FormData();
        formData.append('fileId', fileID);
        formData.append('filePasaporte', filePasaporte);
        formData.append('textoId', textoId);
        formData.append('fechapasaporte', fechapasaporte);
        formData.append('clientId', clientId);

        $.ajax({
            url: "/EnvioMaritimo/SaveImage",
            type: 'post',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
            }
        });
    }

    var embolsar = function() {

        var listProduct = new Array;

        var tBody = $("#tblProductos > TBODY")[0];
        var tr = "";
        var codigoBolsa = ""
        var countFilas = tBody.rows.length - 1;
        for (var i = 0; i < countFilas; i++) {
            var fila = tBody.rows[0];
            listProduct[i] = new Array;

            //Creando codigo de bolsa
            var date = new Date(Date.now())
            codigoBolsa = "BL" + date.getFullYear() + "" + (date.getUTCMonth() + 1) + "" + date.getUTCDate() + "" + date.getMinutes() + "" + date.getUTCMilliseconds()
            tr += "<tr><td>" + $(fila.children[0]).html() + "</td><td>" + $(fila.children[1]).html() + "</td></tr>"
            $(fila).remove();
        }


        if (tr != "") {
            var bolsaHtml = '<div class="col-md-6" style="margin-bottom: 15px">\n' +
                '                            <h4 class="form-section" style="border-bottom: 1px solid silver;border-top: 1px solid silver;">' +
                '                                <div class="btn btn-default btnRemoveBolsa" style="padding: 5px;" data-toggle="tooltip" data-placement="top" data-original-title="Eliminar bolsa"><i style="font-size: 12px" class="fa fa-remove"></i></div> ' +
                //'                                <div class="btn btn-default btnEditBolsa" style="padding: 5px;" data-toggle="tooltip" data-placement="top" data-original-title="Eliminar bolsa"><i style="font-size: 12px" class="fa fa-edit"></i></div> ' +
                '                                Bolsa: ' + codigoBolsa +
                '                                <div class="btn btn-default btnDownUp" style="padding: 5px;float:right; margin-top: 8px;" data-toggle="tooltip" data-placement="top" data-original-title="Detalles"><i style="font-size: 12px" class="fa fa-angle-up"></i></div> ' +
                '                            </h4>\n' +
                '                            <div  style="display: none;"class="table-responsive">\n' +
                '                                <table class="table productsBag" data-codigobolsa="' + codigoBolsa + '">\n' +
                '                                    <thead>\n' +
                '                                    <tr>\n' +
                '                                        <th>Cantidad</th>\n' +
                '                                        <th>Descripción</th>\n' +
                '                                    </tr>\n' +
                '                                    </thead>\n' +
                '                                    <tbody>\n' +
                '                                    <tr>\n' +
                tr
            '                                    </tr>\n' +
                '                                    </tbody>\n' +
                '                                </table>\n' +
                '                            </div>\n' +
                '                        </div>';
            '                        </div>';

            $("#bolsasContainer").html($("#bolsasContainer").html() + bolsaHtml)
            $("#noBolsas").hide()
            functionClickRemoveBag();
            functionClickbtnDownUp();
        }

    }

    var functionClickRemoveBag = function () {
        $(".btnRemoveBolsa").click(function () {
            var $this = $(this);
            confirmationMsg("Eliminar bolsa", "¿Desea eliminar la bolsa selecta?", function () {
                var html = $($this.parent().parent().parent())
                $($this.parent().parent()).remove()
                if (html.find("div").length === 0) {
                    $("#noBolsas").show();
                }

            });

        })
    }

    var functionClickRemoveBag = function () {
        $(".btnRemoveBolsa").click(function () {
            var $this = $(this);
            confirmationMsg("Eliminar bolsa", "¿Desea eliminar la bolsa selecta?", function () {
                var html = $($this.parent().parent().parent())
                $($this.parent().parent()).remove()
                if (html.find("div").length === 0) {
                    $("#noBolsas").show();
                }

            });

        })
    }

    var functionClickbtnDownUp = function () {
        $(".btnDownUp").click(function () {
            var table = $($(this).parent().parent()).find("div[class=table-responsive]")
            if (table.attr("style") == "display: none;") {
                $(this).html('<i style="font-size: 12px" class="fa fa-angle-down"></i>')
                table.show("fast")
            }
            else {
                $(this).html('<i style="font-size: 12px" class="fa fa-angle-up"></i>')
                table.hide("fast")
            }

        });
    }

    var validateCreateOrder = function () {
        var tBody = $("#tblProductos > TBODY")[0];
        if ($("#CantLb").val() == "") {
            showWarningMessage("Atención", "El campo Peso(Lb) no puede estar vacío.");
            return false;
        } else if ($("#precioxlibra").val() == "") {
            showWarningMessage("Atención", "El campo Precio no puede estar vacío.");
            return false;
        } else if ($("#OtrosCostos").val() == "") {
            showWarningMessage("Atención", "El campo Otros Cargos no puede estar vacío.");
            return false;
        } else if ($("#ValorPagado").val() == "") {
            showWarningMessage("Atención", "El campo Importe Pagado no puede estar vacío.");
            return false;
        } else if ($("#balanceValue").html().includes("-")) {
            showWarningMessage("Atención", "El Importe Pagado no puede ser superior al Precio Total.");
            return false;
        } else if ($("#CantLb").val() <= 0) {
            showWarningMessage("Atención", "El campo Peso(Lb) debe ser mayor que 0.");
            return false;
        } else if ($("#PriceLb").val() <= 0 ) {
            showWarningMessage("Atención", "El campo Precio debe ser mayor que 0.");
            return false;
        } else if ($("#OtrosCostos").val() < 0) {
            showWarningMessage("Atención", "El campo Otros Cargos debe ser mayor o igual que 0.");
            return false;
        } else if ($("#ValorPagado").val() < 0) {
            showWarningMessage("Atención", "El campo Importe Pagado debe ser mayor o igual que 0.");
            return false;
        }
       
        else if (tBody.rows.length == 1) {
            showWarningMessage("Atención", "Debe adicionar al menos un producto.");
            return false;
        }

        return true;
    }

   

    //Mostrar y ocultar el credit card fee
    $('[name = "TipoPago"]').on('change', function () {
        var Id = $(this).val();
        tipoPago = $('option[value = "' + Id + '"]').html();
        if (tipoPago == "Crédito o Débito") {
            $('#contfee').show();
        }
        else {
            $('#contfee').hide();
        }
    })

    /**************AuthCard************************************/
    $('#tipoPago').on('change', updateAuthCard);

    function updateAuthCard() {
        var id = $('#tipoPago').val();
        var value = $('option[value = "' + id + '"]').html();
        if (value == "Crédito o Débito") {
            $('#AddAuthorization').show();
        }
        else {
            $('#AddAuthorization').hide();
        }
    }
    updateAuthCard();

    $('[name = "AuthConvCharge"]').on('change', function () {
        calcAuthCard();
    })

    $('#ConfirmAuth').click(function () {
        var error = false;
        if ($('[name="AuthTypeCard"]').val() == "") {
            error = true;
        }

        if ($('#AuthCardCreditEnding').val() == "") {
            error = true;
        }

        if ($('#AuthExpDate').val() == "") {
            error = true;
        }

        if ($('#AuthCCV').val() == "") {
            error = true;
        }

        if ($('#AuthaddressOfSend').val() == "") {
            error = true;
        }

        if ($('#AuthOwnerAddressDiferent').val() == "") {
            error = true;
        }

        if ($('#Authemail').val() == "") {
            error = true;
        }

        if ($('#Authphone').val() == "") {
            error = true;
        }

        if ($('#AuthConvCharge').val() == "") {
            error = true;
        }

        if ($('#TotalCharge').val() == "") {
            error = true;
        }

        if ($('#AuthSaleAmount').val() == "") {
            error = true;
        }

        $('#AuthorizationCard').modal("hide");
        $('body').removeClass('modal-open');
        $('.modal-backdrop').remove();
        if (!error) {

            $('#IcoAuthorizationAdd').attr('class', 'fa fa-check-circle')
            $('#AddAuthorization').attr('class', 'btn mr-1 mb-1 btn-success')
        }
        else {
            $('#IcoAuthorizationAdd').attr('class', 'fa fa-plus-circle')
            $('#AddAuthorization').attr('class', 'btn mr-1 mb-1 btn-secondary')
        }
    });

    function calcAuthCard() {
        var saleamount = parseFloat($('#AuthSaleAmount').val());
        var aconvcharge = parseFloat($('#AuthConvCharge').val());
        var total = saleamount + aconvcharge;
        $('#TotalCharge').val(total.toFixed(2));
    }

    $(document).ready(function () {
        $(".select2-container--default").attr("style", "width: 100%;");
    })

    // Cuando se cree un nuevo cliente al recargarse se muestre
    if (selectedClient != '00000000-0000-0000-0000-000000000000') {
        $.ajax({
            type: "POST",
            url: "/OrderNew/GetClient",
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(selectedClient),
            async: false,
            success: function (data) {
                var newOption = new Option(data.movil + "-" + data.name + " " + data.lastName, selectedClient, false, false);
                $("[name='ClientId']").append(newOption);
                $(".select2-placeholder-selectClient").val(selectedClient).trigger("change").trigger("select2:select");

                //Datos del Cliente en Step 1
                $('#inputClientName').val(data.name);
                $('#inputClientLastName').val(data.lastName);
                $('#inputClientMovil').val(data.movil);
                $('#inputClientEmail').val(data.email);
                $('#inputClientAddress').val(data.calle);
                $('#inputClientCity').val(data.city);
                $('#inputClientZip').val(data.zip);
                $('#inputClientState').val(data.state).trigger("change");

                $('#remitente').html(data.name + " " + data.lastName);
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            }
        });
    }

    var provinciaActual = $('#provincia').val(); //Para saber si se modifica la provincia y recalcular el precio segun el valor del mayorista

    $('#AuthExpDate').pickadate({
        min: new Date(),
    });
});