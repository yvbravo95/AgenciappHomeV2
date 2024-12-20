var selected_municipio;
let addPrecio = 0;

$(document).ready(function () {
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
            finish: 'Enviar remesa'
        },
        onStepChanging: function (event, currentIndex, newIndex) {
            // Allways allow previous action even if the current form is not valid!
            if (currentIndex > newIndex) {
                return true;
            }

            //----------------Step1
            var error = false

            if (newIndex == 0) {
                alert("df   ")
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
                // Si no es viaje pronto entonces actualizo el check segun el monto 
                if (agencyLegalName != "Viaje Pronto Inc") {
                    changeTarifa();
                }
                cargarValorPrecioVenta();
            }

            //----------------Step2

            if (newIndex == 2) {


            }
            return true;
        },
        onFinishing: function (event, currentIndex) {
            return sendReserva();
        },
        onFinished: function (event, currentIndex) {


        }
    });

    var isSend = false; //Para controlar que se envie una unica peticion
    var sendReserva = function () {
        if (!isSend && validateCreateOrder()) {
            isSend = true;

            var valorespagado = [];
            var tipopagos = [];
            var notas = [];
            if ($("#checkpago").is(":checked")) {
                valorespagado = [
                    $("#pagoCash").val().replace(",", "."),
                    $("#pagoZelle").val().replace(",", "."),
                    $("#pagoCheque").val().replace(",", "."),
                    $("#pagoCredito").val().replace(",", "."),
                    $("#pagoTransf").val().replace(",", "."),
                    $("#pagoWeb").val().replace(",", ".")
                ];
                notas = [
                    "",
                    $("#NotaZelle").val(),
                    $("#NotaCheque").val(),
                    "",
                    $("#NotaTransf").val(),
                    ""
                ];
                tipopagos = ['Cash', 'Zelle', 'Cheque', 'Crédito o Débito', 'Transferencia Bancaria', 'Web'];
            }
            else {
                valorespagado = [$("#ValorPagado").val().replace(",", ".")];
                tipopagos = [$('#tipoPago > option[value="' + $("#tipoPago").val() + '"]').html()];
                notas = [$('#NotaPago').val()];
            }
            var total = parseFloat($("#Amount").val()) - parseFloat($("#Debe").val());


            var datosRemesa = [
                tipopagos,// 0 Tipo de pago 
                $(".select2-placeholder-selectClient").val(),// 1 Cliente 
                $(".select2-placeholder-selectContact").val(),// 2  Contacto
                $("#Monto").val(), // 3 Monto a enviar
                porciento, // 4 fee de cobro
                $('#mayorista').val(), // 5
                tarifa, // 6 tipo de tarifa
                total, // 7 Total Pagado
                parseFloat($('#Amount').val()), // 8 Valor a pagar
                $('#provincia').val(), // 9 provincia del contacto
                $('[name="Card.RecipientCardNumber"]').val(),//10
                $('[name="Card.NotaTarjeta"]').val(), // 11
                getMoneyType(), // 12
                $('[name="Card.SenderName"]').val(), // 13
                $('[name="Card.SenderSurName"]').val(), //14
                $('[name="Card.SenderSecondSurName"]').val(), //15
                $('[name="Card.SenderDocumentType"]').val(), //16
                $('[name="Card.SenderNumberIdentityDocument"]').val(), //17
                $('[name="Card.SenderExpirationDateDocument"]').val(), //18
                $('[name="Card.SenderCountryBirth"').val(), //19
                $('[name="Card.SenderDateBirth"]').val(), //20
                $('[name="Card.SenderAmountRechargueCard"]').val(),//21
                $('[name="Card.SenderAddressEEUU"]').val(),//22
                $('[name="Card.SenderPhoneEEUU"]').val(),//23
                $('[name="Card.RecipientName"]').val(),//24
                $('[name="Card.RecipientSurname"]').val(),//25
                $('[name="Card.RecipientSecondSurname"]').val(),//26
                $('[name="Card.RecipientNumberCI"]').val(),//27
                $('[name="Card.RecipientCardType"]').val(),//28
                $('[name="Card.RecipientAddressCountry"]').val(),//20
                $('[name="Card.RecipientCity"]').val(),//30
                $('[name="Card.RecipientProvince"]').val(),//31
                $('[name="Card.RecipientPhone"]').val(),//32
                $('#isRemittanceCard').val(),//33
                $('#AuthTypeCard').val(),//34
                $('#AuthCardCreditEnding').val(),//35
                $('#AuthExpDate').val(),//36
                $('#AuthCCV').val(),//37
                $('#AuthaddressOfSend').val(),//38
                $('#AuthOwnerAddressDiferent').val(),//39
                $('#Authemail').val(),//40
                $('#Authphone').val(),//41
                $('#AuthSaleAmount').val().replace(",", "."),//42
                $('#AuthConvCharge').val().replace(",", "."),//43
                $('#TotalCharge').val().replace(",", "."),//44
                $('[name="ExchangeRate"]').val(), //45
                valorespagado, //46
                notas, //47
                ($('#check_credito').is(':checked')) ? $("#credito").html().replace(",", ".") : 0,// 48
                $('#NotaRemesa').val(), //49
                addPrecio, //50
                $('#CardNumber').val(), //51
            ];

            $.ajax({
                type: "POST",
                url: "/Remesas/CreateRemesa",
                data: JSON.stringify(datosRemesa),
                dataType: 'json',
                contentType: 'application/json',
                async: true,
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
                success: function (data) {
                    window.location = "/Remesas/Details/" + data.id;
                }
            });
        }

        return true;
    }

    var validateCreateOrder = function () {
        if (parseFloat($('#Monto').val()) <= 0) {
            showWarningMessage("Atención", 'El campo "Monto a Enviar" no puede ser 0.');
            return false;
        }
        if (parseFloat($('#Debe').val()) < 0) {
            showWarningMessage("Atención", 'El Total pagado no puede ser mayor que el Valor a Pagar');
            return false;
        }
        return true;
    }

    var isCard = false;
    $('#btnAcceptCard').click(function () {
        $('#div_cardNumber').hide();
        if (!isCard) {
            isCard = true;

            $('[name="MoneyType"]').prop('checked', false);
            $('[name="MoneyType"][value="USD_TARJETA"]').prop('checked', true);

            if (idWholesaerCard && $('#mayorista').val() == "sinmayorista") {
                $('#mayorista').val(idWholesaerCard).trigger('change');
            }

            $('#isRemittanceCard').val(true).trigger('change');

            changeTarifa();
            
            cargarValorPrecioVenta();
            calc();
        }

    })
    $('#btnCancelCard').click(function () {
        if (isCard == true) {
            $('[name="MoneyType"]').prop('checked', false);
            $('[name="MoneyType"][value="CUP"]').prop('checked', true);
            //$('#isUsd').prop('checked', false);
            //$('#mayorista').prop('disabled', false);
            //$('#mayorista').val("sinmayorista").trigger('change');
            isCard = false;
            $('#isRemittanceCard').val(false).trigger('change');
            //$('#tarifa').prop('disabled', false)
            changeTarifa();
        }
    })

    $('#isRemittanceCard').on('change', function () {
        const active = $(this).val();
        if (active == "true")
            $('#labelTarjeta').show();
        else
            $('#labelTarjeta').hide();
    })

    $('ul[aria-label="Pagination"]').prepend(
        '<li class="" aria-disabled="false"><a id="cancel" style="background-color:#ff7e7e;" href="/Remesas/index/" role="menuitem">Cancelar</a></li>'
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

    //placeholder cliente y contacto
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

                if (Array.isArray(data)) {
                    return {
                        results: $.map(data, function (obj) {

                            return { id: obj.clientId, text: obj.fullData };
                        })
                    };
                }
                else {
                    return {
                        results: []
                    };
                }
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


    /**********Clientes y Contactos ***********/

    var showAllClients = function () {
        $.ajax({
            type: "POST",
            url: "/OrderNew/GetAllClients",
            dataType: 'json',
            contentType: 'application/json',
            async: false,
            success: function (data) {
                $("[name='selectClient']").empty();
                $("[name='selectClient']").append(new Option());
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
                        $("[name='selectClient']").append(newOption);
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

                //Card
                $('[name="Card.SenderName"]').val(data.name);
                if (data.lastName != null) {
                    lastname = data.lastName.split(" ");
                    if (lastname.length == 1) {
                        $('[name="Card.SenderSurName"]').val(lastname[0]);
                    }
                    else if (lastname.length > 1) {
                        $('[name="Card.SenderSurName"]').val(lastname[0]);
                        $('[name="Card.SenderSecondSurName"]').val(lastname[1]);
                    }
                }
                $('[name="Card.SenderSurName"]').val(data.lastName);
                $('[name="Card.SenderAddressEEUU"]').val(data.fullAddress);
                $('[name="Card.SenderPhoneEEUU"]').val(data.movil);
                if (data.conflictivo) {
                    $("#conflictivo").removeAttr("hidden");
                }
                else {
                    $("#conflictivo").attr("hidden", "hidden");
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

    async function loadLastCard(contactId) {
        var data = await $.get(`/Remesas/GetLastCardOfContact?contactId=${contactId}`);
        console.log(data);
        if (data != null) {
            $('[name="Card.RecipientCardNumber"]').val(data.recipientCardNumber);

        }
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
                $("[name='selectContact']").empty();
                $("[name='selectContact']").append(new Option());

                var contactData = "";
                for (var i = 0; i < data.length; i++) {
                    if (data[i].phones1 != "")
                        contactData = data[i].phone1 + " - " + data[i].name + " " + data[i].lastName;
                    else
                        contactData = data[i].phone2 + " - " + data[i].name + " " + data[i].lastName;
                    $("[name='selectContact']").append(new Option(contactData, data[i].contactId));
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

                    selected_municipio = data.municipio;

                    //Card
                    $('[name="Card.RecipientName"]').val(data.name);
                    if (data.lastName != null) {
                        var lastname = (data.lastName).split(" ");
                        if (lastname.length == 1) {
                            $('[name="Card.RecipientSurname"]').val(lastname[0]);
                        }
                        else if (lastname.length > 1) {
                            $('[name="Card.RecipientSurname"]').val(lastname[0]);
                            $('[name="Card.RecipientSecondSurname"]').val(lastname[1]);
                        }
                    }
                    $('[name="Card.RecipientNumberCI"]').val(data.ci);
                    $('[name="Card.RecipientAddressCountry"]').val(data.fullAddress);
                    $('[name="Card.RecipientProvince"]').val(data.city);
                    $('[name="Card.RecipientPhone"]').val(data.movilPhone);

                    loadLastCard(idContacto);

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

    var porciento = 15;
    var tarifa = $('[name="tarifa"][checked]')[0].value;
    // Al seleccionarse un contacto se carga el valor fee Cobro
    function cargarValorPrecioVenta() {
        var idContacto = $('.select2-placeholder-selectContact').val();
        $.ajax({
            type: "POST",
            url: "/Remesas/getFeeCobro",
            data: {
                idContacto: idContacto,
                tarifa: tarifa,
                MoneyType: getMoneyType()
            },
            async: false,
            success: function (data) {
                if (data != "") {
                    porciento = parseFloat(data);
                    // Slider Porciento de Cobro
                    $('#sliderContent').html('<div id="slider-handles" class="my-1"></div>');

                    var handlesSlider = document.getElementById('slider-handles');

                    noUiSlider.create(handlesSlider, {
                        start: [porciento],
                        range: {
                            'min': [0],
                            'max': [80]
                        },
                        tooltips: true,
                        format: wNumb({
                            decimals: 0
                        })
                    }).on("change", function (e) {
                        porciento = parseFloat(e);
                        calc()
                        $("#Cobro").val(e)
                    });
                    calc();
                }
                else {
                    showErrorMessage("ERROR", 'No se ha podido cargar el valor del campo "Fee de Cobro".')
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

    $('[name="MoneyType"]').on('change', function () {
        if ($('[name="selectContact"]').val() != "") {
            cargarValorPrecioVenta();
        }
        var option = getMoneyType();
        if (option == "CUP") {
            $('#divTasaCambio').show()
            $('#div_cardNumber').show()
        }
        else {
            $('#divTasaCambio').hide()
            $('#div_cardNumber').hide()
        }

        if (isCard) {
            $('#btnCancelCard').trigger('click');
        }

    });

    // Si se cambia la tarifa de cobro se carga el valor de precio de venta
    $('[name = "tarifa"]').on('click', function () {
        tarifa = $(this).val();
        $(this).prop('checked', true);
        /*if (!isCard) {
            cargarValorPrecioVenta();
        }*/
        cargarValorPrecioVenta();
    });

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
                $("[name='selectContact']").empty();
                $("[name='selectContact']").append(new Option());

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
                        $("[name='selectContact']").append(new Option(contactData, data[i].contactId));
                    }
                    var last = data[data.length - 1];
                    if (selectedContact != null && selectedContact != "00000000-0000-0000-0000-000000000000") {
                        $("[name='selectContact']").val(selectedContact).trigger("change");
                        selectedContact = "00000000-0000-0000-0000-000000000000";

                    }
                    else {
                        $("[name='selectContact']").val(last.contactId).trigger("change");

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
        } else if ($("#contactDireccion").val() == "") {
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

    mostrarEstados();

    /***********DATOS DE LA RESERVA *************/

    $("#Monto").on("change", function () {
        changeTarifa();
        $('[name="Card.SenderAmountRechargueCard"]').val($(this).val());
    });

    $("#Monto").on("keyup", function () {
        changeTarifa();
        $('[name="Card.SenderAmountRechargueCard"]').val($(this).val());
    })

    $("#Monto,#ExchangeRate,#check_credito,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb").keyup(function () {
        calc();
    });
    $("#Monto,#ExchangeRate,#check_credito,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb").change(function () {
        calc();
    });

    $("#btnprecioscosto").on("click", function () {
        addPrecio = parseFloat($("#addprecio").val());
        calc();
    });


    //funcion para cambiar la tarifa si el monto es de 0 a 99 ó mayor que 99
    function changeTarifa() {
        const moneyType = getMoneyType();
        if (isCubatex == "True" && moneyType == "USD_TARJETA") {
            // cambio la tarifa a porciento
            tarifa = "porciento";
            $('[name="tarifa"]').prop('checked', false);
            $('[name="tarifa"][value = "porciento"]').prop('checked', true);
            cargarValorPrecioVenta();
        }
        else if (agencyLegalName != "Viaje Pronto Inc") {
            var monto = parseFloat($('#Monto').val());

            if (monto <= 99 && tarifa == "porciento") {
                //cambio la tarifa a fijo
                tarifa = "fijo";
                $('[name="tarifa"]').prop('checked', false);
                $('[name="tarifa"][value = "fijo"]').prop('checked', true);
                cargarValorPrecioVenta();

            }
            else if (monto > 99 && tarifa == "fijo") {
                // cambio la tarifa a porciento
                tarifa = "porciento";
                $('[name="tarifa"]').prop('checked', false);
                $('[name="tarifa"][value = "porciento"]').prop('checked', true);
                cargarValorPrecioVenta();

            }
        }
        //}
    }

    function calc() {
        var monto = parseFloat($("#Monto").val());
        var total = addPrecio;
        if (tarifa == "fijo") {
            total += monto + porciento;
        }
        else if (tarifa == "porciento") {
            total += monto + (monto * (porciento / 100));
        }

        if ($("#checkpago").is(":checked")) {
            var pagoCash = parseFloat($("#pagoCash").val());
            var pagoZelle = parseFloat($("#pagoZelle").val());
            var pagoCheque = parseFloat($("#pagoCheque").val());
            var pagoTransf = parseFloat($("#pagoTransf").val());
            var pagoWeb = parseFloat($("#pagoWeb").val());

            var pagoCrDeb = parseFloat($("#pagoCredito").val());
            var pagoCrDebReal = parseFloat($("#pagoCredito").val()) / (1 + parseFloat($('#fee').html()) / 100);
            var feeCrDeb = pagoCrDeb - pagoCrDebReal;
            if (pagoCrDeb > 0) {
                $('#contfee').show();
            }
            else {
                $('#contfee').hide();
            }
            total += feeCrDeb;

            var pagoCredito = 0;
            if ($("#check_credito").is(":checked")) {
                pagoCredito = parseFloat($("#credito").html()).toFixed(2);
                pagoCredito = pagoCredito > total ? total : pagoCredito;
            }

            var balanceValue = total - pagoCash - pagoZelle - pagoCheque - pagoCrDeb - pagoTransf - pagoWeb - pagoCredito;

            $("#pagoCash").attr('max', (balanceValue + pagoCash).toFixed(2));
            $("#pagoZelle").attr('max', (balanceValue + pagoZelle).toFixed(2));
            $("#pagoCheque").attr('max', (balanceValue + pagoCheque).toFixed(2));
            $("#pagoTransf").attr('max', (balanceValue + pagoTransf).toFixed(2));
            $("#pagoWeb").attr('max', (balanceValue + pagoWeb).toFixed(2));
            $("#pagoCredito").attr('max', ((balanceValue + pagoCrDebReal) + (balanceValue + pagoCrDebReal) * parseFloat($('#fee').html()) / 100).toFixed(2));
            $("#pagar_credit").html("$" + ((balanceValue + pagoCrDebReal) + (balanceValue + pagoCrDebReal) * parseFloat($('#fee').html()) / 100).toFixed(2) + " (" + ((balanceValue + pagoCrDebReal) * parseFloat($('#fee').html()) / 100).toFixed(2) + " fee)");

            //Valor Sale Amount en authorization card
            $('[name = "AuthSaleAmount"]').val(pagoCrDebReal.toFixed(2));
            var aconvcharge = parseFloat($('[name = "AuthConvCharge"]').val());
            var totalCharge = pagoCrDebReal + (pagoCrDebReal * aconvcharge / 100);
            $('[name = "TotalCharge"]').val(totalCharge.toFixed(2));
        }
        else {
            tipoPagoId = $('[name = "TipoPago"]').val();
            tipoPago = $('option[value = "' + tipoPagoId + '"]').html();
            if (tipoPago == "Crédito o Débito") {
                var fee = parseFloat($('#fee').html());
                $('[name = "AuthSaleAmount"]').val(total.toFixed(2));
                total = total + (total * (fee / 100));
                $('[name = "TotalCharge"]').val(total.toFixed(2));
            }
            var balanceValue = 0;
            if ($("#check_credito").is(":checked")) {
                if (total.toFixed(2) - parseFloat($("#credito").html()) > 0) {
                    $("#ValorPagado").attr('max', total.toFixed(2) - parseFloat($("#credito").html()));
                    $("#ValorPagado").val((total.toFixed(2) - parseFloat($("#credito").html())).toFixed(2));
                }
                else {
                    $("#ValorPagado").attr('max', 0);
                    $("#ValorPagado").val(0);
                }
            }
            else {
                $("#ValorPagado").val(total.toFixed(2));
                $("#ValorPagado").attr('max', total.toFixed(2));
            }
        }

        $("#Amount").val(total.toFixed(2));
        $("#Debe").val(balanceValue.toFixed(2));
        if ($("#Debe").val() == '-0.00')
            $("#Debe").val('0.00');

        showConversion();
    }

    function calcPayment() {
        var monto = parseFloat($("#Monto").val());
        var total = 0;
        if (tarifa == "fijo") {
            total = monto + porciento;
        }
        else if (tarifa == "porciento") {
            total = monto + (monto * (porciento / 100));
        }
        var max = total;
        var pagado = $("#ValorPagado").val();

        tipoPagoId = $('[name = "TipoPago"]').val();
        tipoPago = $('option[value = "' + tipoPagoId + '"]').html();
        if (tipoPago == "Crédito o Débito") {
            var fee = parseFloat($('#fee').html());
            var pagdoReal = pagado / (1 + fee / 100)
            var feeCrDeb = pagado - pagdoReal;
            total = total + feeCrDeb;
            max = max + max * fee / 100;

            //Valor Sale Amount en authorization card
            $('[name = "AuthSaleAmount"]').val(pagdoReal.toFixed(2));
            $('[name = "TotalCharge"]').val(pagado);
        }

        total = total.toFixed(2);
        var balanceValue = 0;
        if ($("#check_credito").is(":checked")) {
            var pagoCredito = parseFloat($("#credito").html()).toFixed(2);
            pagoCredito = pagoCredito > total ? total : pagoCredito;
            balanceValue = total - pagado - pagoCredito;
        }
        else {
            balanceValue = total - pagado;
        }

        $("#Amount").val(total);
        $("#Debe").val(balanceValue.toFixed(2));
        if ($("#Debe").val() == '-0.00')
            $("#Debe").val('0.00');

        $("#ValorPagado").attr("max", max.toFixed(2));
        showConversion();
    };

    $("#checkpago").on('click', function () {
        if ($("#checkpago").is(" :checked")) {
            $("#untipopago").attr("hidden", 'hidden');
            $("#ValorPagado").val(0);
            $(".multipopago").removeAttr("hidden");
            $('#contfee').hide();
        }
        else {
            $(".multipopago").attr("hidden", 'hidden');
            $("#untipopago").removeAttr("hidden");
            $("#pagoCash").val(0);
            $("#pagoZelle").val(0);
            $("#pagoCheque").val(0);
            $("#pagoCredito").val(0);
            $("#pagoTransf").val(0);
            $("#pagoWeb").val(0);
        }
        calc()
    });

    $("#check_credito").on('click', function () {
        $("#pagoCash").val(0);
        $("#pagoZelle").val(0);
        $("#pagoCheque").val(0);
        $("#pagoCredito").val(0);
        $("#pagoTransf").val(0);
        $("#pagoWeb").val(0);
    });

    $("#ValorPagado").on("keyup", calcPayment).on("change", calcPayment);

    $("#Monto,#check_credito,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb,#ValorPagado").on("change", validarInputVacios);

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
        if ($("#Monto").val() == "")
            $("#Monto").val(0);
        if ($("#ValorPagado").val() == "")
            $("#ValorPagado").val(0);
    }


    //Mostrar y ocultar el credit card fee
    $('[name = "TipoPago"]').on('change', function () {
        var Id = $(this).val();
        tipoPago = $('option[value = "' + Id + '"]').html();
        if (tipoPago == "Crédito o Débito") {
            $('#contfee').show();
            $('#AddAuthorization').show();
            calc()
        }
        else {
            $('#contfee').hide();
            $('#AddAuthorization').hide();
            calc();
        }
        if (tipoPago == "Zelle" || tipoPago == "Cheque" || tipoPago == "Transferencia Bancaria") {
            $('#contNotaPago').show();
        }
        else {
            $('#contNotaPago').hide();
        }
    })

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
                $("[name='selectClient']").append(newOption);
                $(".select2-placeholder-selectClient").val(selectedClient).trigger("change").trigger("select2:select");

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
                showContactsOfAClient();
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            }
        });
    }

    var idMayoristabyTransferencia = $('#idMayoristabyTransf').html();
    $('#mayorista').change(function () {
        updateExchangeRate($(this).val());
        var nombre = $('option[value="' + $(this).val() + '"]').html();
        // Si existe un mayorista por transferencia envio una alerta
        if (idMayoristabyTransferencia != "") {
            swal({
                title: "Está usted seguro",
                text: "Esta seguro que desea cambiar al mayorista por transferencia a " + nombre,
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DA4453",
                confirmButtonText: "Si, cambiar",
                cancelButtonText: "No, mantener",
                closeOnConfirm: false,
                closeOnCancel: false
            }, function (isConfirm) {
                if (isConfirm) {
                    idMayoristabyTransferencia = "";
                    swal.close();
                } else {
                    $('#mayorista').val(idMayoristabyTransferencia).trigger("change");

                    swal.close();
                }
            });
        }


    });

    function getMoneyType() {
        var ele = document.getElementsByName('MoneyType');

        for (i = 0; i < ele.length; i++) {
            if (ele[i].checked)
                return ele[i].value;
        }

        return "";
    }

    function updateExchangeRate(wholesalerId) {
        $.ajax({
            async: true,
            type: "GET",
            url: `/remesas/GetEchangeRate?id=${wholesalerId}`,
            success: function (response) {
                $("[name='ExchangeRate']").val(response).trigger("change");
            },
            error: function (error) {
                console.log(error);
                $("[name='ExchangeRate']").val("0.00").trigger("change");
            }
        })
    }

    function showConversion() {
        var exchangeRate = parseFloat($('[name="ExchangeRate"]').val());
        var moneyType = getMoneyType();
        var convertValue = parseFloat($('[name="Monto"]').val());
        if (moneyType == "CUP") {
            convertValue = convertValue * exchangeRate;
        }
        else if (moneyType == "USD_TARJETA") {
            moneyType = "MLC"
        }
        $('#convert').html(`${convertValue} ${moneyType}`);
    }
});

