var agencyDCubaId = "4752B08A-7684-42B3-930D-FF86F496DF2F";
//var agencyDCubaId ="2F7B03FB-4BE1-474D-8C95-3EE8C6EAEAC1";
var municipalities = {};
var cities = {};

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

let isLoadMunic = false;
async function loadMunicipios(provincia, municipios) {
    if (!provincia)
        return;

    while (isLoadMunic) {
        await sleep(500);
    }
    isLoadMunic = true;
    await loadMunicipiosAux(provincia, municipios);
    isLoadMunic = false;
}

async function loadMunicipiosAux(provincia, municipios) {
    if (provincia.val() != "" && provincia.val() != null) {
        var response = municipalities[provincia.val()];
        if (response) {
            municipios.empty();
            municipios.append(new Option())
            for (var i in response) {
                var m = response[i];
                municipios.append(new Option(m, m))
                if ($(municipios).data("value") != "" && $(municipios).data("value") != null) {
                    $(municipios).val($(municipios).data("value")).trigger("change")
                }
            }
        }
        else {
            await $.ajax({
                url: "/Provincias/Municipios?nombre=" + provincia.val(),
                type: "POST",
                dataType: "json",
                async: true,
                success: function (response) {
                    municipalities[provincia.val()] = response;
                    municipios.empty();
                    municipios.append(new Option())
                    for (var i in response) {
                        var m = response[i];
                        municipios.append(new Option(m, m))
                        if ($(municipios).data("value") != "" && $(municipios).data("value") != null) {
                            $(municipios).val($(municipios).data("value")).trigger("change")
                        }
                    }
                }
            })
        }
    }
}

let isLoadCity = false;
async function loadCiudad(estado, ciudad) {
    if (!estado)
        return;

    while (isLoadCity) {
        await sleep(500);
    }
    isLoadCity = true;
    await loadCiudadAux(estado, ciudad);
    isLoadCity = false;
}

async function loadCiudadAux(estado, ciudad) {
    if (estado.val() != "" && estado.val() != null) {
        var response = cities[estado.val()];
        if (response) {
            ciudad.empty();
            ciudad.append(new Option())
            for (var i in response) {
                var m = response[i];
                ciudad.append(new Option(m, m))
                if ($(ciudad).data("value") != "" && $(ciudad).data("value") != null) {
                    $(ciudad).val($(ciudad).data("value")).trigger("change")
                }
            }
        }
        else {
            await $.ajax({
                url: "/Provincias/Ciudades?nombre=" + estado.val(),
                type: "POST",
                dataType: "json",
                async: true,
                success: function (response) {
                    cities[estado.val()] = response;
                    ciudad.empty();
                    ciudad.append(new Option())
                    for (var i in response) {
                        var m = response[i];
                        ciudad.append(new Option(m, m))
                        if ($(ciudad).data("value") != "" && $(ciudad).data("value") != null) {
                            $(ciudad).val($(ciudad).data("value")).trigger("change")
                        }
                    }
                }
            })
        }
    }
}

$(document).on("ready", function () {
    var showClient = function () {
        selectedClient = $("#ClientId").val();
        $.ajax({
            type: "POST",
            url: "/Clients/GetClient",
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(selectedClient),
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
                $('#inputClientName2').val(data.name2);
                $('#inputClientLastName2').val(data.lastName2);
                $('#inputClientMovil').val(data.movil);
                $('#inputClientEmail').val(data.email);
                $('#inputClientAddress').val(data.calle);
                $('#inputClientCity').data("value",data.city);
                $('#inputClientZip').val(data.zip);
                $('#inputClientState').val(data.state).trigger("change");
                $('#inputClientFNaci').val(parseDate(data.fechaNac.slice(0, 10)));
                $('#inputClientID').val(data.id);

                if (data.getCredito && data.getCredito != 0) {
                    $("#div_credito").removeAttr('hidden');
                    $("#credito").html(data.getCredito);
                }
                else {
                    $("#div_credito").attr('hidden', "hidden");
                }


                $("#PaisResidencia").val(data.country);
                $("#PaisActual").val(data.country);
                $("#Estado").val(data.state).trigger("change");
                $("#PrimerNombre").val(data.name);
                $("#PrimerApellidos").val(data.lastName);
                $("#SegundoNombre").val(data.name2);
                $("#SegundoApellidos").val(data.lastName2);
                $("#DireccionActual").val(data.calle);
                $("#ProvinciaActual").data("value", data.city);;
                $("#EstadoActual").val(data.state).trigger("change");
                $("#CodigoPostalActual").val(data.zip);
                $("#Telefono").val(data.movil);
                $("#EmailActual").val(data.email);
                if (data.fechaNac.slice(0, 10) != "0001-01-01") {
                    $("#DateBirth").val(data.fechaNac.slice(0, 10));
                    $("div[data-input='DateBirth']").find("input:first").val(parseDate(data.fechaNac.slice(0, 10)))
                }
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
    }

    var desactClientForm = function () {
        $('#nuevoCliente').removeAttr("disabled");
        $(".select2-placeholder-selectClient").removeAttr("disabled");

        $('#inputClientName').attr("disabled", "disabled");
        $('#inputClientLastName').attr("disabled", "disabled");
        $('#inputClientName2').attr("disabled", "disabled");
        $('#inputClientLastName2').attr("disabled", "disabled");
        $('#inputClientMovil').attr("disabled", "disabled");
        $('#inputClientEmail').attr("disabled", "disabled");
        $('#inputClientAddress').attr("disabled", "disabled");
        $('#inputClientCity').attr("disabled", "disabled");
        $('#inputClientState').attr("disabled", "disabled");
        $('#inputClientZip').attr("disabled", "disabled");
        $('#inputClientFNaci').attr("disabled", "disabled");
        $('#inputClientID').attr("disabled", "disabled");

        $('#editarCliente').removeClass("hidden");
        $("#cancelarCliente").addClass("hidden");
        $("#guardarCliente").addClass("hidden");

        $("a[href='#next']").removeClass("hidden");
    }

    var cancelClientForm = function () {
        $('#inputClientName').val($('#inputClientName').data("prevVal"));
        $('#inputClientLastName').val($('#inputClientLastName').data("prevVal"));
        $('#inputClientName2').val($('#inputClientName2').data("prevVal"));
        $('#inputClientLastName2').val($('#inputClientLastName2').data("prevVal"));
        $('#inputClientMovil').val($('#inputClientMovil').data("prevVal"));
        $('#inputClientEmail').val($('#inputClientEmail').data("prevVal"));
        $('#inputClientAddress').val($('#inputClientAddress').data("prevVal"));
        $('#inputClientCity').val($('#inputClientCity').data("prevVal"));
        $('#inputClientState').val($('#inputClientState').data("prevVal")).trigger("change");
        $('#inputClientZip').val($('#inputClientZip').data("prevVal"));
        $('#inputClientID').val($('#inputClientID').data("prevVal"));
        $('#inputClientFNaci').val($('#inputClientFNaci').data("prevVal"));

        desactClientForm();
    }

    var calcMoney = function () {
        var precio = $("#Precio").val();
        var oCargos = $("#OtrosCostos").val();
        var comision = $("#Comision").val();
        var descuento = $("#Descuento").val();
        var priceExpress = $('#priceExpress').val();
        var precioTotalValue = parseFloat(precio) + parseFloat(oCargos) - parseFloat(descuento) + parseFloat(comision);
        if ($('#Express').is(':checked')) {
            precioTotalValue += parseFloat(priceExpress);
            $('#labelPriceExpress').show();
            $('#spanPriceExpress').html(priceExpress);
        }
        else {
            $('#labelPriceExpress').hide();
            $('#spanPriceExpress').html('0.00');
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
        }

        $("#precioTotalValue").html(precioTotalValue.toFixed(2));
        $("#balanceValue").html(balanceValue.toFixed(2));
        if ($("#balanceValue").html() == '-0.00')
            $("#balanceValue").html('0.00');

        $('#Total').val(precioTotalValue.toFixed(2));
    };

    var calcMoneyPayment = function () {
        var precio = $("#Precio").val();
        var oCargos = $("#OtrosCostos").val();
        var comision = $("#Comision").val();
        var descuento = $("#Descuento").val();
        var pagado = $("#ValorPagado").val();
        var priceExpress = $('#priceExpress').val();
        var precioTotalValue = parseFloat(precio) + parseFloat(oCargos) - parseFloat(descuento) + parseFloat(comision);
        if ($('#Express').is(':checked')) {
            precioTotalValue += parseFloat(priceExpress);
            $('#labelPriceExpress').show();
            $('#spanPriceExpress').html(priceExpress);
        }
        else {
            $('#labelPriceExpress').hide();
            $('#spanPriceExpress').html('0.00');
        }
        var max = precioTotalValue;

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

        $("#precioTotalValue").html(precioTotalValue);
        $("#balanceValue").html(balanceValue.toFixed(2));
        $("#Balance").val(parseFloat(balanceValue.toFixed(2)));
        $("#ValorPagado").attr("max", max.toFixed(2));
    };

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
        if ($("#Precio").val() == "")
            $("#Precio").val(0);
        if ($("#OtrosCostos").val() == "")
            $("#OtrosCostos").val(0);
        if ($("#ValorPagado").val() == "")
            $("#ValorPagado").val(0);
        if ($("#Comision").val() == "")
            $("#Comision").val(0);
        if ($("#Descuento").val() == "")
            $("#Descuento").val(0);
    }

    var loadPrecio = function () {
        var wholesaler = $('#WholesalerId').val();
        var servicio = $("#ServicioConsular").val();
        $.ajax({
            type: "POST",
            url: "/Passport/GetPrice",
            data: {
                servicio: servicio,
                WholesalerId: wholesaler
            },
            async: false,
            beforeSend: function () {
                $('#priceExpress').val(0);
                $('#costExpress').val(0);
            },
            success: function (data) {
                if (data.success) {
                    $("#Precio").val(data.precio);
                    $("#costo").val(data.costo);

                    //Cargar valores de precio y costo para cuando es express
                    $('#servConsularExpress').val(servicio);
                    if (data.spe != null) {
                        $('#priceExpress').val(data.spe.price);
                        $('#costExpress').val(data.spe.costo);
                    }
                }
                else {
                    toastr.warning(data.msg);
                }
                calcMoney();
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            }
        });
    }

    var bindDatePicker = function () {
        $(".date").datetimepicker({
            format: 'MM/DD/YYYY',
            viewMode: "years",
            widgetPositioning: {
                horizontal: 'auto',
                vertical: 'bottom'
            },
            extraFormats: ["YYYY-MM-DD"]
        }).on("dp.change", function (e) {
            var mirrorId = $(this).data("input");

            if (mirrorId) {
                var mirror = $("#" + mirrorId);
                mirror.val(new Date(e.date).toISOString().substr(0, 10)).trigger("change");
            }
            var d = new Date(e.date);
            if (d < new Date("1920-01-01") || d > new Date("2030-12-31")) {
                showWarningMessage("la fecha esta en un rango inválido")
            }

        });
        $(".year").datetimepicker({
            format: 'YYYY',
            viewMode: "years",
            widgetPositioning: {
                horizontal: 'auto',
                vertical: 'bottom'
            },
            extraFormats: ["YYYY-MM-DD"]
        }).on("dp.change", function (e) {
            var mirrorId = $(this).data("input");
            if (mirrorId) {
                var mirror = $("#" + mirrorId);
                mirror.val(new Date(e.date).toISOString().substr(0, 10)).trigger("change");
            }
            var d = new Date(e.date);
            if (d < new Date("1920-01-01") || d > new Date("2030-12-31")) {
                showWarningMessage("la fecha esta en un rango inválido")
            }
        });
        $(".date-input").mask("99/99/9999")
    }

    var parseDate = function (value) {
        if (value == "0001-01-01") {
            return "";
        }
        var m = value.match(/^(\d{4})(\/|-)?(\d{1,2})(\/|-)?(\d{1,2})$/);
        if (m)
            value = ("00" + m[3]).slice(-2) + '/' + ("00" + m[5]).slice(-2) + '/' + m[1];

        return value;
    }

    bindDatePicker();

    $('#inputClientMovil').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });
    $('#Telefono').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });
    $('#TelefonoTrabajo').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });
    $('#TelefonoReferencia').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });

    $(".selectdos").select2({ width: "100%" })

    $("#select-service").select2({ width: "100%", placeholder:"Servicio consular" })

    $("#inputClientState").select2({
        placeholder: "Estado",
        width: "100%"
    });

    $("#inputClientCity").select2({
        placeholder: "Ciudad",
        width: "100%"
    });

    $("#selectPermiso").select2({
        placeholder: "Permiso",
        width: "100%"
    });

    $("#ClientId").select2({
        placeholder: "Buscar cliente por teléfono, nombre o apellido",
        width: "100%",
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

                        return { id: obj.clientId, text: obj.fullData, conflictivo: obj.conflictivo };
                    })
                };
            }
        }
    });

    $("#ClientId").on("select2:select", function (a, b) {
        $('#editarCliente').removeClass("hidden");
        showClient();
    });

    $('#editarCliente').on('click', function () {
        // para que no pueda crear nuevo cliente mientras edita cliente
        $('#nuevoCliente').attr("disabled", "disabled");

        // para que no pueda cambiar de cliente mientras edita cliente
        $('.select2-placeholder-selectClient').attr("disabled", "disabled");

        $("a[href='#next']").addClass("hidden");

        $('#inputClientName').removeAttr("disabled").data("prevVal", $('#inputClientName').val());
        $('#inputClientLastName').removeAttr("disabled").data("prevVal", $('#inputClientLastName').val());
        $('#inputClientName2').removeAttr("disabled").data("prevVal", $('#inputClientName2').val());
        $('#inputClientLastName2').removeAttr("disabled").data("prevVal", $('#inputClientLastName2').val());
        $('#inputClientMovil').removeAttr("disabled").data("prevVal", $('#inputClientMovil').val());
        $('#inputClientEmail').removeAttr("disabled").data("prevVal", $('#inputClientEmail').val());
        $('#inputClientAddress').removeAttr("disabled").data("prevVal", $('#inputClientAddress').val());
        $('#inputClientCity').removeAttr("disabled").data("prevVal", $('inputClientCity').val());
        $('#inputClientState').removeAttr("disabled").data("prevVal", $('#inputClientState').val());
        $('#inputClientZip').removeAttr("disabled").data("prevVal", $('#inputClientZip').val());
        $('#inputClientFNaci').removeAttr("disabled").data("prevVal", $('#inputClientFNaci').val());
        $('#inputClientID').removeAttr("disabled").data("prevVal", $('#inputClientID').val());

        $('#editarCliente').addClass("hidden");
        $("#cancelarCliente").removeClass("hidden");
        $("#guardarCliente").removeClass("hidden");
    });

    $("#cancelarCliente").click(cancelClientForm);

    $('#guardarCliente').on('click', function () {
        if (validateEditarCliente()) {
            $.ajax({
                async: true,
                type: "POST",
                contentType: "application/x-www-form-urlencoded",
                data: {
                    clientId: $("#ClientId").val(),
                    name: $('#inputClientName').val(),
                    name2: $('#inputClientName2').val(),
                    lastname: $('#inputClientLastName').val(),
                    lastname2: $('#inputClientLastName2').val(),
                    email: $('#inputClientEmail').val(),
                    phone: $('#inputClientMovil').val(),
                    address: $('#inputClientAddress').val(),
                    city: $('#inputClientCity').val(),
                    state: $('#inputClientState').val(),
                    zip: $('#inputClientZip').val(),
                    fechaNac: $("#inputClientFNaci").val(),
                    id: $("#inputClientID").val()
                },
                url: "/Clients/EditClientNew",
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

    $("#NacidoOtroPais,#NacidoUSA").on("change", function () {
        if ($("#NacidoUSA").is(":checked")) {
            $("#PaisNacimiento").val("Estados Unidos").trigger('change');
        }
        else if (!$("#NacidoUSA").is(":checked") && !$("#NacidoOtroPais").is(":checked")) {
            $("#PaisNacimiento").val("Cuba").trigger('change');
        }

        if ($("#NacidoOtroPais").is(":checked") || $("#NacidoUSA").is(":checked")) {
            $("#municipiohidden").removeClass("hidden");
            $("#provinciahidden").removeClass("hidden");
            $("#municipio2").removeAttr("disabled");
            $("#provincia2").removeAttr("disabled");
            $("#ProvinciaNacimiento").attr("disabled", "disabled");
            $("#MunicipioNacimiento").attr("disabled", "disabled");
            $("#provincia").addClass("hidden");
            $("#municipio").addClass("hidden");
        }
        else {
            $("#municipiohidden").addClass("hidden");
            $("#provinciahidden").addClass("hidden");
            $("#municipio2").attr("disabled", "disabled");
            $("#provincia2").attr("disabled", "disabled");
            $("#ProvinciaNacimiento").removeAttr("disabled");
            $("#MunicipioNacimiento").removeAttr("disabled");
            $("#provincia").removeClass("hidden");
            $("#municipio").removeClass("hidden");

        }
    });

    $("#PaisNacimiento").on("change", () => {
        const pais = $("#PaisNacimiento").val();
        const p = paises.find(x => x.Nombre == pais)
        $("#Nationality").val(p.Nacinalidad);
    })

    $("#checkpago").on('click', function () {
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
        calcMoney()
    });

    $("#ServicioConsular").on("change", function () {
        const value = $(this).val();
        if (value == "Prorroga1") {
            $('#divProrrogaType').show();
        }
        else {
            $('#divProrrogaType').hide();
            $('#ProrrogaType').val("None").trigger('change');
        }

        if (value == "Prorroga1" || value == "Prorroga2") {
            $('#div_Express').show();
        }
        else {
            $('#div_Express').hide();
        }
    });

    $("#ServicioConsular, #WholesalerId").on("change", loadPrecio);

    $("#ProvinciaReferencia").on("change", function () {
        loadMunicipios($("#ProvinciaReferencia"), $("#MunicipioReferencia"));
    })
    $("#ProvinciaCuba1").on("change", function () {
        loadMunicipios($("#ProvinciaCuba1"), $("#CiudadCuba1"));
    })

    $("#ProvinciaCuba2").on("change", function () {
        loadMunicipios($("#ProvinciaCuba2"), $("#CiudadCuba2"));
    })

    $("#ProvinciaNacimiento").on("change", function () {
        loadMunicipios($("#ProvinciaNacimiento"), $("#MunicipioNacimiento"));
    })

    $("#EstadoActual").on("change", function () {
        loadCiudad($("#EstadoActual"), $("#ProvinciaActual"));
    })

    $("#EstadoTrabajo").on("change", function () {
        loadCiudad($("#EstadoTrabajo"), $("#ProvinciaTrabajo"));
    })

    $("#inputClientState").on("change", function () {
        loadCiudad($("#inputClientState"), $("#inputClientCity"));
    })

    $("#Precio,#Descuento,#Comision,#OtrosCostos,#check_credito,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb").on("change", validarInputVacios);

    $("#Precio,#Descuento,#Comision,#OtrosCostos,#check_credito,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb").on("keyup", calcMoney).on('change', calcMoney);

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
        calcMoney();
    });

    $("#ValorPagado").on("keyup", calcMoneyPayment).on("change", calcMoneyPayment);

    $("#Express").on("change", calcMoney);

    $('#tipoPago').on('change', function () {
        var Id = $(this).val();
        tipoPago = $('option[value = "' + Id + '"]').html();
        if (tipoPago == "Crédito o Débito") {
            $('#contfee').show();
            $('#AddAuthorization').show();

            calcMoney();
        }
        else {
            $('#contfee').hide();
            $('#AddAuthorization').hide();
            calcMoney();
        }

        if (tipoPago == "Zelle" || tipoPago == "Cheque" || tipoPago == "Transferencia Bancaria") {
            $('#contNotaPago').show();
        }
        else {
            $('#contNotaPago').hide();
        }
    });

    $('#ServicioConsular').on('change', function () {
        var Id = $(this).val();
        var value = $('option[value = "' + Id + '"]').html();
        if (value == "PASAPORTE 1 VEZ" || value == "PASAPORTE 1 VEZ MENOR") {
            $("#direccionCuba").show();
            $("#reno_prorroga").hide();
            $("#renovacion").hide();
            $("#prorroga").hide();
            $("#certNacimiento").show();
            $("#no_1_vez").hide();
            $("#Tramite").val("Confeccion").trigger("change");

        }
        else if (value == "PRORROGA 1" || value == "PRORROGAR 2") {
            if (value == "PRORROGA 1") {
                $("#CantidadProrrogas").val(1);
            }
            else {
                $("#CantidadProrrogas").val(2);
            }
            $("#Tramite").val("Prorroga").trigger("change");
            $("#direccionCuba").hide();
            $("#reno_prorroga").show();
            $("#renovacion").hide();
            $("#prorroga").show();
            $("#certNacimiento").hide();
            $("#no_1_vez").show();
        }
        else if (value == "RENOVAR" || value == "RENOVAR MENOR") {
            $("#direccionCuba").hide();
            $("#reno_prorroga").show();
            $("#renovacion").show();
            $("#prorroga").hide();
            $("#certNacimiento").hide();
            $("#no_1_vez").show();
            $("#Tramite").val("Confeccion").trigger("change");
        }
        else {
            $("#direccionCuba").hide();
            $("#reno_prorroga").hide();
            $("#renovacion").hide();
            $("#prorroga").hide();
            $("#certNacimiento").hide();
            $("#no_1_vez").show();
            $("#Tramite").val("Confeccion").trigger("change");
        }
    })

    $("#Estatura").on("change", function () {
        var val = parseFloat($(this).val());
        ft = Math.floor(val);
        p = val - ft;
        p = p * 100;
        var cm = ft * 30.48;
        cm += p * 2.54;
        $("#EstaturaCm").val(cm.toFixed(0));
    })

    $("#EstaturaCm").on("change", function () {
        var val = parseFloat($(this).val());
        var ft = val / 30.48;
        var ft = Math.floor(ft);
        var p = val - (ft * 30.48);
        p = p / 2.54 / 100;
        $("#Estatura").val((ft + p).toFixed(2));
    })

    $("#ExpirePassport1").on("change", function () {
        var val = new Date($(this).val());
        val.setMonth(val.getMonth() - 72);
        $("div[data-input='FechaExpedicion']").find("input:first").val(parseDate(val.toISOString().slice(0, 10)))
        $("#FechaExpedicion").val(val.toISOString().slice(0, 10))
    })

    $("#ProvinciaActual").on("change", function (e) {
        $("#ProvinciaTrabajo").val($(e.target).val()).trigger("change");
        $("#ProvinciaTrabajo").data("value", $(e.target).val());;
    })

    $("#EstadoActual").on("change", function (e) {
        $("#EstadoTrabajo").val($(e.target).val()).trigger("change");
    })


    //mostrarEstados();
    $("#reno_prorroga").hide();
    $("#renovacion").hide();
    $("#prorroga").hide();

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
                $("#ClientId").append(newOption);
                $("#ClientId").val(selectedClient).trigger("change").trigger("select2:select");

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
                $('#inputClientName2').val(data.name2);
                $('#inputClientLastName2').val(data.lastName2);
                $('#inputClientMovil').val(data.movil);
                $('#inputClientEmail').val(data.email);
                $('#inputClientAddress').val(data.calle);
                $('#inputClientCity').data("value", data.city);
                $('#inputClientZip').val(data.zip);
                $('#inputClientState').val(data.state).trigger("change");
                $('#inputClientFNaci').val(parseDate(data.fechaNac.slice(0, 10)));
                $('#inputClientID').val(data.id);

                if (data.getCredito && data.getCredito != 0) {
                    $("#div_credito").removeAttr('hidden');
                    $("#credito").html(data.getCredito);
                }
                else {
                    $("#div_credito").attr('hidden', "hidden");
                }


                $("#PaisResidencia").val(data.country);
                $("#PaisActual").val(data.country);
                $("#Estado").val(data.state).trigger("change");
                $("#PrimerNombre").val(data.name);
                $("#PrimerApellidos").val(data.lastName);
                $("#SegundoNombre").val(data.name2);
                $("#SegundoApellidos").val(data.lastName2);
                $("#DireccionActual").val(data.calle);
                $("#ProvinciaActual").data("value", data.city);;
                $("#EstadoActual").val(data.state).trigger("change");
                $("#CodigoPostalActual").val(data.zip);
                $("#Telefono").val(data.movil);
                $("#EmailActual").val(data.email);
                if (data.fechaNac.slice(0, 10) != "0001-01-01") {
                    $("#DateBirth").val(data.fechaNac.slice(0, 10));
                    $("div[data-input='DateBirth']").find("input:first").val(parseDate(data.fechaNac.slice(0, 10)))
                }
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
    $('#select2-select-service-container').parent().attr('style', 'background-color:#d1f7f8')
});


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

var send = false;

$("#zc").steps({
    headerTag: "h6",
    bodyTag: "fieldset",
    transitionEffect: "fade",
    titleTemplate: '<span class="step">#index#</span> #title#',
    labels: {
        previous: "Anterior",
        next: "Siguiente",
        finish: 'Crear'
    },
    onStepChanging: function (event, currentIndex, newIndex) {
        // Allways allow previous action even if the current form is not valid!
        if (currentIndex > newIndex) {
            if (newIndex == 1)
                $("a[href=#previous]").hide();
            return true;
        }

        if (newIndex == 1) {
            if ($("#ClientId").val() == "") {
                showWarningMessage("Atención", "El campo cliente es obligatorio.");
                return false;
            }
            if (new Date($("#DateBirth").val()) > new Date()) {
                showWarningMessage("Atención", "La fecha de nacimiento no es válida.");
                return false;
            }
            if ($("#select-service").val() == "") {
                showWarningMessage("Atención", "El campo Servicio Consular es obligatorio.");
                return false;
            }

            $('#ServicioConsular').trigger('change');
        }
        if (agencyId != agencyDCubaId) {
            if (newIndex == 2)   {
                var valid = true;
                var Id = $('#ServicioConsular').val();
                var value = $('option[value = "' + Id + '"]').html();

                if (value != "PASAPORTE 1 VEZ" && value != "PASAPORTE 1 VEZ MENOR") {
                    if ($("#PassaportNumber").val() == "") {
                        showWarningMessage("Atención", "El campo número de pasaporte es obligatorio.");
                        valid = false;
                    }
                }
                if ($("#Type").val() == "None") {
                    showWarningMessage("Atención", "El campo tipo de pasaporte es obligatorio.");
                    valid = false;
                }
                if ($("#Tramite").val() == "None") {
                    showWarningMessage("Atención", "El campo tramite es obligatorio.");
                    valid = false;
                }
                if ($("#TipoSolicitud").val() == "None") {
                    showWarningMessage("Atención", "El campo tipo de solicitud es obligatorio.");
                    valid = false;
                }
                if ($("#ServicioConsular").val() == "None") {
                    showWarningMessage("Atención", "El campo servicio consular es obligatorio.");
                    valid = false;
                }
                if ($("#AgencyPassport").val() == "") {
                    showWarningMessage("Atención", "El campo agencia es obligatorio.");
                    valid = false;
                }
                return valid;
            }
            if (newIndex == 3) {
                var valid = true;
                if ($("#PrimerNombre").val() == "") {
                    showWarningMessage("Atención", "El campo primer nombre es obligatorio.");
                    valid = false;
                }
                if ($("#PrimerApellidos").val() == "") {
                    showWarningMessage("Atención", "El campo primer apellido es obligatorio.");
                    valid = false;
                }
                if ($("#SegundoApellidos").val() == "") {
                    showWarningMessage("Atención", "El campo segundo apellido es obligatorio.");
                    valid = false;
                }
                if ($("#Padre").val() == "") {
                    showWarningMessage("Atención", "El campo padre es obligatorio.");
                    valid = false;
                }
                if ($("#Madre").val() == "") {
                    showWarningMessage("Atención", "El campo madre es obligatorio.");
                    valid = false;
                }
                if ($("#Estatura").val() == "") {
                    showWarningMessage("Atención", "El campo estatura es obligatorio.");
                    valid = false;
                }
                if ($("#Sex").val() == "None") {
                    showWarningMessage("Atención", "El campo sexo es obligatorio.");
                    valid = false;
                }
                if ($("#ColorOjos").val() == "None") {
                    showWarningMessage("Atención", "El campo color de ojos es obligatorio.");
                    valid = false;
                }
                if ($("#ColorPiel").val() == "None") {
                    showWarningMessage("Atención", "El campo color de piel es obligatorio.");
                    valid = false;
                }
                if ($("#ColorCabello").val() == "None") {
                    showWarningMessage("Atención", "El campo color de cabello es obligatorio.");
                    valid = false;
                }
                return valid;
            }
            if (newIndex == 4) {
                var regexEmail = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
                if ($("#EmailActual").val() != "" && !regexEmail.test($("#EmailActual").val())) {
                    showWarningMessage("Atención", "El campo Email no tiene el formato correcto.");
                    return false;
                }
                var valid = true;
                if ($("#PaisResidencia").val() == "") {
                    showWarningMessage("Atención", "El campo país de residencia es obligatorio.");
                    valid = false;
                }
                if ($("#Estado").val() == "") {
                    showWarningMessage("Atención", "El campo estado es obligatorio.");
                    valid = false;
                }
                if ($("#ClasificacionMigratoria").val() == "None") {
                    showWarningMessage("Atención", "El campo clasificación migratoria es obligatorio.");
                    valid = false;
                }
                if ($("#FechaSalida").val() == "") {
                    showWarningMessage("Atención", "El campo fecha de salida es obligatorio.");
                    valid = false;
                }
                if ($("#PaisNacimiento").val() == "") {
                    showWarningMessage("Atención", "El campo país de nacimiento es obligatorio.");
                    valid = false;
                }
                if (!$("#NacidoUSA").is(":checked") && !$("#NacidoOtroPais").is(":checked")) {
                    if ($("#ProvinciaNacimiento").val() == "") {
                        showWarningMessage("Atención", "El campo provincia de nacimiento es obligatorio.");
                        valid = false;
                    }
                }
                else {
                    if ($("#provincia2").val() == "") {
                        showWarningMessage("Atención", "El campo provincia de nacimiento es obligatorio.");
                        valid = false;
                    }
                }
                if (!$("#NacidoUSA").is(":checked") && !$("#NacidoOtroPais").is(":checked")) {
                    if ($("#MunicipioNacimiento").val() == "") {
                        showWarningMessage("Atención", "El campo municipio de nacimiento es obligatorio.");
                        valid = false;
                    }
                }
                else {
                    if ($("#municipio2").val() == "") {
                        showWarningMessage("Atención", "El campo municipio de nacimiento es obligatorio.");
                        valid = false;
                    }
                }
                if ($("#Nationality").val() == "") {
                    showWarningMessage("Atención", "El campo nacionalidad es obligatorio.");
                    valid = false;
                }
                if ($("#DateBirth").val() == "") {
                    showWarningMessage("Atención", "El campo fecha de nacimiento es obligatorio.");
                    valid = false;
                }
                if ($("#DireccionActual").val() == "") {
                    showWarningMessage("Atención", "El campo dirección es obligatorio.");
                    valid = false;
                }
                if ($("#PaisActual").val() == "") {
                    showWarningMessage("Atención", "El campo país es obligatorio.");
                    valid = false;
                }
                if ($("#ProvinciaActual").val() == "") {
                    showWarningMessage("Atención", "El campo ciudad es obligatorio.");
                    valid = false;
                }
                if ($("#EstadoActual").val() == "") {
                    showWarningMessage("Atención", "El campo estado es obligatorio.");
                    valid = false;
                }
                if ($("#CodigoPostalActual").val() == "") {
                    showWarningMessage("Atención", "El campo código postal es obligatorio.");
                    valid = false;
                }
                if ($("#Telefono").val() == "") {
                    showWarningMessage("Atención", "El campo teléfono es obligatorio.");
                    valid = false;
                }

                var Id = $('#ServicioConsular').val();
                var value = $('option[value = "' + Id + '"]').html();

                if (value == "RENOVAR") {
                    var date = $("#DateBirth").val();
                    var edad = calcularEdad(date);
                    if (edad < 16) {
                        showWarningMessage("Atención", "Fecha de Nacimiento invalida.");
                        valid = false;
                    }
                }
                return valid;
            }
            if (newIndex == 5) {
                var Id = $('#ServicioConsular').val();
                var value = $('option[value = "' + Id + '"]').html();

                var regexEmail = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
                if ($("#EmailTrabajo").val() != "" && !regexEmail.test($("#EmailTrabajo").val())) {
                    showWarningMessage("Atención", "El campo Email no tiene el formato correcto.");
                    return false;
                }
                var valid = true;
                if ($("#DatosLaborales").val() == "") {
                    showWarningMessage("Atención", "El campo Nombre del centro de trabajo es obligatorio.");
                    valid = false;
                }
                if ($("#Profesion").val() == "") {
                    showWarningMessage("Atención", "El campo Profesión es obligatorio.");
                    valid = false;
                }
                if ($("#Ocupacion").val() == "") {
                    showWarningMessage("Atención", "El campo Ocupación es obligatorio.");
                    valid = false;
                }
                if ($("#CategoriaProfesion").val() == "None") {
                    showWarningMessage("Atención", "El campo Categoría de Prefosión es obligatorio.");
                    valid = false;
                }
                if ($("#NivelCultural").val() == "None") {
                    showWarningMessage("Atención", "El campo Nivel Escolar es obligatorio.");
                    valid = false;
                }
                if ($("#NombreReferencia").val() == "") {
                    showWarningMessage("Atención", "El campo Nombre de Referencia es obligatorio.");
                    valid = false;
                }
                if ($("#ApellidosReferencia").val() == "") {
                    showWarningMessage("Atención", "El campo primer apellido es obligatorio.");
                    valid = false;
                }
                if ($("#ApellidosReferencia2").val() == "") {
                    showWarningMessage("Atención", "El campo segundo apellido es obligatorio.");
                    valid = false;
                }
                if ($("#DireccionReferencia").val() == "") {
                    showWarningMessage("Atención", "El campo dirección de referencia es obligatorio.");
                    valid = false;
                }
                if ($("#ProvinciaReferencia").val() == "") {
                    showWarningMessage("Atención", "El campo provincia de referencia es obligatorio.");
                    valid = false;
                }
                if ($("#MunicipioReferencia").val() == "") {
                    showWarningMessage("Atención", "El campo municipio de referencia es obligatorio.");
                    valid = false;
                }
                if (value == "PASAPORTE 1 VEZ" || value == "PASAPORTE 1 VEZ MENOR") {
                    if ($("#DireccionCuba1").val() == "") {
                        showWarningMessage("Atención", "El campo derección 1 es obligatorio.");
                        valid = false;
                    }
                    if ($("#ProvinciaCuba1").val() == "") {
                        showWarningMessage("Atención", "El campo provincia 1 es obligatorio.");
                        valid = false;
                    }
                    if ($("#CiudadCuba1").val() == "") {
                        showWarningMessage("Atención", "El campo municipio 1 es obligatorio.");
                        valid = false;
                    }
                    if ($("#Desde1").val() == "") {
                        showWarningMessage("Atención", "El campo desde 1 es obligatorio.");
                        valid = false;
                    }
                    if ($("#Hasta1").val() == "") {
                        showWarningMessage("Atención", "El campo hasta 1 es obligatorio.");
                        valid = false;
                    }
                    if ($("#Desde1").val() > $("#Hasta1").val()) {
                        showWarningMessage("Atención", "rango de fecha incorrecto.");
                        valid = false;
                    }
                    if ($("#Dasde2").val() > $("#Hasta2").val()) {
                        showWarningMessage("Atención", "rango de fecha incorrecto.");
                        valid = false;
                    }

                    if ($("#ExpedicionCertNaci").val() == "") {
                        showWarningMessage("Atención", "El campo fecha de expedición es obligatorio.");
                        valid = false;
                    }
                    if ($("#LugarExpedCertNaci").val() == "") {
                        showWarningMessage("Atención", "El campo lugar de expedición es obligatorio.");
                        valid = false;
                    }
                }
                if (value == "PRORROGA 1" || value == "PRORROGAR 2") {
                    if ($("#FechaExpedicion").val() == "") {
                        showWarningMessage("Atención", "El campo fecha de expedición es obligatorio.");
                        valid = false;
                    }

                }
                if (value == "RENOVAR") {
                    if ($("#RazonNoDisponibilidad").val() == "None") {
                        showWarningMessage("Atención", "El campo razón de no disponibilidad es obligatorio.");
                        valid = false;
                    }
                }
                return valid;
            }
        }        
        return true;
    },
    onFinishing: function (event, currentIndex) {
        if (agencyId === agencyDCubaId) {
            var Id = $('#ServicioConsular').val();
            if (Id == "" || Id == "None") {
                showWarningMessage("Atención", "El campo servicio consular es obligatorio.");
                return false;
            }
            if (Id == "Prorroga1" && $('#ProrrogaType').val() == "None") {
                showWarningMessage("Atención", "El campo tipo de prorroga es obligatorio.");
                return false;
            }
        }
        swal({
            title: "¿Está seguro?",
            text: "Verifique que todos los datos ingresados se encuentren correctamente y presione 'ok'.",
            type: "info",
            showCancelButton: true,
            closeOnConfirm: false,
            showLoaderOnConfirm: true,
        }, function (isConfirm) {
            if (isConfirm && !send) {
                console.log("sending...")
                send = true;
                $("#zc").submit();
                form.submit()
            } else {
                return false;
            }

        });
    },
    onFinished: function (event, currentIndex) {
    }
});

$("a[href=#previous]").hide();

$("a[href=#next]").click(function () {
    $("a[href=#previous]").show();
});

$('#priceFoto').on('click', function () {
    var isChecked = $(this).is(':checked');
    var precio = parseFloat($('#Precio').val());
    var valuePreciofoto = parseFloat($('#ValuePrecioFoto').val());
    if (isChecked) {
        $('#Precio').val(precio + valuePreciofoto);
        $('#Precio').attr('min', valuePreciofoto);
    }
    else {
        $('#Precio').val(precio - valuePreciofoto);
        $('#Precio').attr('min', 0);
    }
    $('#Precio').trigger('change');

});
/*********************************************/

$('#select-service').on('change', function () {
    const value = $(this).val();
    location.href = `/Passport/Create?service=${value}&idClient=${selectedClient}`
})

function calcularEdad(fecha) {
    var hoy = new Date();
    var cumpleanos = new Date(fecha);
    var edad = hoy.getFullYear() - cumpleanos.getFullYear();
    var m = hoy.getMonth() - cumpleanos.getMonth();

    if (m < 0 || (m === 0 && hoy.getDate() < cumpleanos.getDate())) {
        edad--;
    }

    return edad;
}
