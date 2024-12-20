var productos;
var precioProductos = prodExistAmount;
idCliente = "";
var cServicio = 0;
var cost = 0;
var price = 0;
var wholesaleCost = 0;
var wholesalePrice = 0;
$(document).ready(function () {
    
    var isPackage = $('#tipoServicio option:selected').data("ispackage") == "True";

    var data = isPackage ? JSON.parse($("#Data").val()) : [{
        name: "",
        lastName: "",
        adulto: true
    }];

    var url = decodeURIComponent(window.location);
    var params = url.split("?")[1];
    if (params != null) {
        var params = params.split("&");
        var msg = params[0].split("=")[1];
        if (msg != null && msg != "") {
            toastr.info(msg);
        }
        if (params.length > 1) {
            idCliente = params[1].split("=")[1];
            // Selecciono el cliente en el campo select
            $('#selectClient').val(idCliente).trigger("change");
        }
    }

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
        enableCancelButton: true,
        labels: {
            previous: "Anterior",
            next: "Siguiente",
            finish: 'Editar',
            cancel: "Cancelar"
        },
        onCanceled: function () {
            window.location = "/Servicios";
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
                if ($("#selectClient").val() == "") {
                    showWarningMessage("Atención", "El campo cliente es obligatorio.");
                    return false;
                }
            }

            //----------------Step2

            return true;
        },
        onFinishing: function (event, currentIndex) {
            $("#Data").val(JSON.stringify(data));
            $("#zc").submit();
        },
        onFinished: function (event, currentIndex) {
        }
    });
    $("[href='#cancel']").addClass("btn-danger");

    $("#selectMayorista").select2({
        placeholder: "Seleccione un mayorista",
        val: null,
        width: "100%"
    });
    $("#TipoServicioId").select2({
        placeholder: "Seleccione un servicio",
        val: null,
        width: "100%"
    });
    $("#subServicio").select2({
        placeholder: "Seleccione un subservicio",
        val: null,
        width: "100%"
    });
    $("#tipoPago").select2({
        placeholder: "Tipo Pago",
        val: null,
        width: "100%"
    });
    $(".Sel").select2({
        placeholder: "Buscar cliente por teléfono, nombre o apellido",
        val: null,
        width: "100%",
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

    // Cuando se cree un nuevo cliente al recargarse se muestre
    if (idCliente != '') {
        $.ajax({
            type: "POST",
            url: "/Clients/GetClient",
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(idCliente),
            async: false,
            success: function (data) {
                var newOption = new Option(data.movil + "-" + data.name + " " + data.lastName, idCliente, false, false);
                $(".Sel").append(newOption);
                $(".Sel").val(idCliente).trigger("change").trigger("select2:select");
                showClient();
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            }
        });
    }

    function selectMayorista(id) {
        $.ajax({
            type: "POST",
            url: "/Servicios/getMayorista",
            data: {
                id: id,
            },
            async: false,
            success: function (data) {
                wholesaleCost = data.costoMayorista;
                wholesalePrice = data.precioVenta;
                if (wholesaleCost == 0)
                    $('[name = "costo"]').val(cost);
                else
                    $('[name = "costo"]').val(wholesaleCost);

                if (wholesalePrice == 0)
                    $('[name = "importe"]').val(price + precioProductos);
                else
                    $('[name = "importe"]').val(wholesalePrice + precioProductos);
                calculate();
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    }

    function getMayoristas() {
        //Pongo los valores en 0
        $('[name = "costo"]').val(0);
        $('[name = "importeTotal"]').val(0);
        $('[name = "importePagado"]').val(0);
        $('[name = "utilidad"]').val(0);
        $('[name = "cantidadOrdenes_Tienda"]').val(0);

        var Idservicio = $('[name="TipoServicioId"]').val();
        $.ajax({
            type: "POST",
            url: "/Servicios/getMayoristas",
            data: {
                Idservicio: Idservicio,
            },
            async: false,
            success: function (data) {
                cServicio = data.costo;
                cost = data.wholesaleCost;
                price = data.price;

                $("#selectMayorista").empty();
                $("#selectMayorista").append(new Option("Seleccione un mayorista", "", true));

                if (data.mayoristas.length != 0) {
                    for (var i = 0; i < data.mayoristas.length; i++) {
                        $("#selectMayorista").append(new Option(data.mayoristas[i].name, data.mayoristas[i].id));
                    }

                }
                else {
                    wholesaleCost = 0;
                    wholesalePrice = 0;
                }

                $("#CostoXServicio").val(cServicio);
                $('[name = "costo"]').val(cost);
                $('[name = "importe"]').val(price + precioProductos);
                calculate();
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    }

    function getMayoristasSub() {
        //Pongo los valores en 0
        $('[name = "costo"]').val(0);
        $('[name = "importeTotal"]').val(0);
        $('[name = "importePagado"]').val(0);
        $('[name = "utilidad"]').val(0);
        $('[name = "cantidadOrdenes_Tienda"]').val(0);

        var Idservicio = $('#subServicio').val();
        $.ajax({
            type: "POST",
            url: "/Servicios/getMayoristas",
            data: {
                Idservicio: Idservicio,
            },
            async: false,
            success: function (data) {
                cServicio = data.costo;
                cost = data.wholesaleCost;
                price = data.price;
                $("#selectMayorista").empty();
                $("#selectMayorista").append(new Option("Seleccione un mayorista", "", true));

                if (data.mayoristas.length != 0) {
                    for (var i = 0; i < data.mayoristas.length; i++) {
                        $("#selectMayorista").append(new Option(data.mayoristas[i].name, data.mayoristas[i].id));
                    }
                }
                else {
                    wholesaleCost = 0;
                    wholesalePrice = 0;
                }
                $("#CostoXServicio").val(cServicio);
                $('[name = "costo"]').val(cost);
                $('[name = "importe"]').val(price + precioProductos);
                calculate();
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    }

    function getSubServicio() {
        var Idservicio = $('[name="TipoServicioId"]').val();
        $.ajax({
            type: "POST",
            url: "/Servicios/getSubServicios",
            data: {
                Idservicio: Idservicio,
            },
            async: false,
            success: function (data) {
                $("#subServicio").empty();
                $("#subServicio").append(new Option("Seleccione un subservicio", "", true));

                if (data.length != 0) {
                    for (var i = 0; i < data.length; i++) {
                        $("#subServicio").append(new Option(data[i].name, data[i].id));
                    }
                    $("#subServicioDiv").show();
                    getMayoristas();
                }
                else {
                    $("#subServicioDiv").hide();
                    getMayoristas();
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

    $('#selectMayorista').on('change', function () {
        const id = $(this).val();
        selectMayorista(id);
    });

    // La seleccionarse un servicio se cargan los mayoristas de ese servicio
    $('[name="TipoServicioId"]').on('change', function () {
        isPackage = $('option[value="' + $(this).val() + '"]').data("ispackage") == "True";
        updatePackage();
        var servicio = $('option[value="' + $(this).val() + '"]').html();
        getSubServicio();
        if (servicio == "Tienda") {
            $('[name="servTienda"]').show();
        }
        else {
            $('[name="servTienda"]').hide();
        }

        if (servicio == "Poder Menor") {
            $('#container-minorauth').show();
        }
        else {
            $('#container-minorauth').hide();
        }
    });

    $('#subServicio').on('change', function () {
        getMayoristasSub()
    });


    //Cliente
    function showClient() {
        selectedClient = $("#clienteClientId").val();
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
                $('#inputClientMovil').val(data.movil);
                $('#inputClientEmail').val(data.email);
                $('#inputClientAddress').val(data.calle);
                $('#inputClientCity').val(data.city);
                $('#inputClientZip').val(data.zip);
                $('#inputClientState').val(data.state).trigger("change");
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            }
        });
    }
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
    }
    var desactClientForm = function () {
        $('#nuevoCliente').removeAttr("disabled");
        $(".select2-placeholder-selectClient").removeAttr("disabled");

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

        $("a[href='#next']").removeClass("hidden");
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
    $("#clienteClientId").on("select2:select", function (a, b) {
        $('#editarCliente').removeClass("hidden");
        showClient();
    });
    $('#editarCliente').on('click', function () {
        // para que no pueda crear nuevo cliente mientras edita cliente
        $('#nuevoCliente').attr("disabled", "disabled");

        // para que no pueda cambiar de cliente mientras edita cliente
        $('#clienteClientId').attr("disabled", "disabled");

        $("a[href='#next']").addClass("hidden");

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
                $("#clienteClientId").val(),
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
    mostrarEstados();

    //Pagos
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
        calculate()
    });
    $('#tipoPago').on('change', function () {
        var Id = $(this).val();
        tipoPago = $('option[value = "' + Id + '"]').html();
        if (tipoPago == "Crédito o Débito") {
            $('#contfee').show();
            $("#AddAuthorization").show();
            calculate()
        }
        else {
            $('#contfee').hide();
            $("#AddAuthorization").hide();
            calculate();
        }
    })
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
        calculate();
    });
    $("#cantidadOrdenes_Tienda,#importe,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb").on("change", validarInputVacios);
    $("#cantidadOrdenes_Tienda,#importe,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb").on("keyup", calculate).on('change', calculate);
    $("#ValorPagado").on("keyup", calculatePayment).on("change", calculatePayment);
    function calculate() {
        if (blockRight == "True") {
            calculateBlockRight();
        }
        else if (isPaquete == "True") {
            calculatePayment();
        }
        else {
            var importeTotal = parseFloat($('#importe').val());
            var costo = parseFloat($("#CostoXServicio").val());
            importeTotal += costo;
            var servicio = $('option[value="' + $('#TipoServicioId').val() + '"]').html();
            if (servicio == "Tienda") {
                var cantOrdenes = parseFloat($('[name="cantidadOrdenes_Tienda"]').val());

                const fee = 20 * cantOrdenes;
                const discountFee = importeTotal - fee
                const costo = discountFee - (discountFee * 0.05)

                $('[name="costo"]').val(costo.toFixed(2));
                $('[name="utilidad"]').val((importeTotal - costo).toFixed(2));
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
                importeTotal += feeCrDeb;

                var pagoCredito = 0;

                balanceValue = importeTotal - pagoCash - pagoZelle - pagoCheque - pagoCrDeb - pagoTransf - pagoWeb - pagoCredito;

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
                    $('#AuthSaleAmount').val(importeTotal.toFixed(2));
                    importeTotal = importeTotal + (importeTotal * (fee / 100));
                    $('#TotalCharge').val(importeTotal.toFixed(2));
                }
                var balanceValue = 0;
                if ($("#check_credito").is(":checked")) {
                    if (importeTotal.toFixed(2) - parseFloat($("#credito").html()) > 0) {
                        $("#ValorPagado").attr('max', importeTotal.toFixed(2) - parseFloat($("#credito").html()));
                        $("#ValorPagado").val((importeTotal.toFixed(2) - parseFloat($("#credito").html())).toFixed(2));
                    }
                    else {
                        $("#ValorPagado").attr('max', 0);
                        $("#ValorPagado").val(0);
                    }
                }
                else {
                    $("#ValorPagado").val(importeTotal.toFixed(2));
                    $("#ValorPagado").attr('max', importeTotal.toFixed(2));
                }
            }

            $("#debe").val(balanceValue.toFixed(2));
            if ($("#debe").val() == '-0.00')
                $("#debe").val('0.00');

            $('#importeTotal').val(importeTotal.toFixed(2));
        }
    }
    function calculatePayment() {
        var pagado = $("#ValorPagado").val();
        var precioTotalValue = parseFloat($('#importe').val());
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
        balanceValue = precioTotalValue - pagado;

        $("#importeTotal").val(precioTotalValue);
        $("#debe").val(balanceValue.toFixed(2));
        $("#ValorPagado").attr("max", max.toFixed(2));
    };
    function calculateBlockRight() {
        var importeTotal = parseFloat($('#importe').val());
        var costo = parseFloat($("#CostoXServicio").val());
        importeTotal += costo;

        var precioTotalValue = importeTotal + feeReal;
        if (servicio == "Tienda") {
            var cantOrdenes = parseFloat($('[name="cantidadOrdenes_Tienda"]').val());

            const fee = 20 * cantOrdenes;
            const discountFee = importeTotal - fee
            const costo = discountFee - (discountFee * 0.05)

            $('[name="costo"]').val(costo.toFixed(2));
            $('[name="utilidad"]').val((importeTotal - costo).toFixed(2));
        }
        var balanceValue = precioTotalValue - valorPagado;

        $('#importeTotal').val(precioTotalValue.toFixed(2));
        $("#debe").val(parseFloat(balanceValue.toFixed(2)));
    }
    function validarInputVacios() {
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
        if ($("#importe").val() == "")
            $("#import").val(0);
        if ($("#ValorPagado").val() == "")
            $("#ValorPagado").val(0);
    }

    showClient();
    var servicio = $('option[value="' + $("#TipoServicioId").val() + '"]').html();
    if (servicio == "Tienda") {
        $('[name="servTienda"]').show();
    }
    else {
        $('[name="servTienda"]').hide();
    }

    $("#packagename").parent().parent().remove();

    $("#package-count").val(data.length);

    if (data[0].name != "") {
        for (var i = 0; i < data.length; i++) {
            addPerson(data, i)
        }
    }

    updatePackage();

    function updatePackage() {
        if (isPackage) {
            $("#package_div").removeClass("hidden");
        }
        else {
            $("#data").val("");
            $("#package_div").addClass("hidden");
        }
    }

    function addPerson(data, i) {
        if (i == 0) {
            $("#package_div").append(`
            <hr id="hr${i}"/>
                <div class="row" id="div${i}">
                    <div class="col-md-4">
                        <input type="text" class="form-control" value='${data[i].name}' id="packagename${i}" data-index="${i}" required />
                    </div>
                    <div class="col-md-5">
                        <input type="text" class="form-control" value='${data[i].lastName}' id="packagelastname${i}" data-index="${i}" required />
                    </div>
                    <div class="col-md-2">
                        <label>Adulto? <input type="checkbox" ${data[i].adulto ? "checked" : ""} class="custom-checkbox" id="packgecheckbox${i}" data-index="${i}" /></label>
                    </div>
                </div>
            `);
        }
        else {
            $("#package_div").append(`
            <hr id="hr${i}"/>
                <div class="row" id="div${i}">
                    <div class="col-md-4">
                        <input type="text" class="form-control" value='${data[i].name}' id="packagename${i}" data-index="${i}" required />
                    </div>
                    <div class="col-md-5">
                        <input type="text" class="form-control" value='${data[i].lastName}' id="packagelastname${i}" data-index="${i}" required />
                    </div>
                    <div class="col-md-2">
                        <label>Adulto? <input type="checkbox" ${data[i].adulto ? "checked" : ""} class="custom-checkbox" id="packgecheckbox${i}" data-index="${i}" /></label>
                    </div>
                    <div class="col-md-1">
                        <button id="delete${i}" data-index="${i}" class="btn btn-danger btn-sm rounded-0" type="button" data-toggle="tooltip" data-placement="top" title="" data-original-title="Delete"><i class="fa fa-trash"></i></button>
                    </div>
                </div>
            `);
        }

        $(`#packagename${i}`).on("change", (e) => {
            let index = $(e.target).data("index");
            data[index].name = $(e.target).val();
            console.log($(e.target).val());
        })

        $(`#packagelastname${i}`).on("change", (e) => {
            let index = $(e.target).data("index");
            data[index].lastName = $(e.target).val();
        })

        $(`#packgecheckbox${i}`).on("change", (e) => {
            let index = $(e.target).data("index");
            data[index].adulto = $(e.target).is(":checked");
            console.log(data)
        })


        $(`#delete${i}`).on("click", (e) => {
            let index = $(e.target).data("index");
            removePerson(data, index);

            for (var j = i; j < data.length; j++) {
                updatePerson(data, j);
            }
        })
    }

    function updatePerson(data, i) {
        $(`#packagename${i}`).val(data[i].name);
        $(`#packagelastname${i}`).val(data[i].lastName);
        $(`#packgecheckbox${i}`).prop("checked", data[i].adulto);
    }

    function removePerson(data, i) {
        data.splice(i, 1);
        $("#package-count").val(data.length);
        $(`#hr${data.length}`).remove();
        $(`#div${data.length}`).remove();
    }

    $("#package-count").on("change", (e) => {
        let value = $(e.target).val();
        if (value > data.length) {
            let count = value - data.length;
            for (var i = 0; i < count; i++) {
                data.push({ name: "", lastName: "", adulto: true });
                addPerson(data, data.length - 1);
            }
        }
        else {
            let del = data.length - value;
            for (var i = 0; i < del; i++) {
                removePerson(data, data.length - 1);
            }
        }
    })

    var servicio = $('option[value="' + $('[name="TipoServicioId"]').val() + '"]').html();

    if (servicio == "Poder Menor") {
        $('#container-minorauth').show();
    }
    else {
        $('#container-minorauth').hide();
    }

    $("#selectProduct").on("change", function () {
        var productId = $(this).val();
        if (!productId) return;

        //Busco el producto seleccionado
        var prodSelect;
        for (var i = 0; i < productos.length; i++) {
            if (productos[i].productId == productId) {
                prodSelect = productos[i];
                break;
            }
        }

        //Verifico que se haya añadido a la tabla
        var tabla = $('#tblProductos');
        var find = tabla.find('[data-id="' + productId + '"]');
        if (find.length == 0) {
            var desc = prodSelect.productName;

            addProductBodegaToTable(1, desc, prodSelect.productId, prodSelect.quantity);
            updatePrecio();
        }
        else {
            toastr.warning("El producto " + prodSelect.productName + " ya se añadió a la tabla");
        }
        $("#selectProduct").val("").trigger("change");
    });

    var cargarproductos = function () {
        $.ajax({
            type: "GET",
            url: "/BodegaProducto/GetProductExceptCombos",
            async: true,
            success: function (data) {
                productos = data; //Guardo los productos en una variable global

                $('#selectProduct').empty();
                $('#selectProduct').append(new Option("Seleccione los productos", "", true));
                if (data.length != 0) {
                    for (var i = 0; i < data.length; i++) {
                        var x = new Option(data[i].productName, data[i].productId);
                        x.title = data[i].description ?? '';
                        $('#selectProduct').append(x);
                    }
                }
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            }
        });
    };

    var addProductBodegaToTable = function (cant, descripcion, id, maxcant) {
        //Get the reference of the Table's TBODY element.
        var tBody = $("#tblProductos > TBODY")[0];

        var index = $(tBody).children().length;

        var row = tBody.insertRow(index);


        var cell1 = $(row.insertCell(-1));
        cell1.append(cant);

        var cell2 = $(row.insertCell(-1));
        cell2.append(descripcion);

        var cell3 = $(row.insertCell(-1));
        cell3[0].style.display = "inline-flex";
        var btnView = $("<button type='button' data-id='" + id + "' class='btn btn-blue' title='View' style='font-size: 10px;padding:9px;margin-right:1px;'><i class='fa fa-eye'></i></button>");
        var btnEdit = $("<button type='button' data-id='" + id + "' class='btn btn-warning' title='Editar' style='font-size: 10px;padding:9px'><i class='fa fa-pencil'></i></button>");
        var btnRemove = $("<button type='button' data-id='" + id + "' class='btn btn-danger pull-right' title='Eliminar' style='font-size: 10px;padding:9px'><i class=' fa fa-close'></button>");
        var btnConfirm = $("<button type='button' data-id='" + id + "' class='btn btn-success hidden' title='Confirmar' style='font-size: 10px;padding:9px'><i class='fa fa-check'></button>");

        var i_id = $("<input type='hidden' value = '" + id + "' name='Products[" + index + "].ProductId'/>");
        var i_cant = $("<input type='hidden' value = '" + cant + "' name='Products[" + index + "].Cantidad'/>");

        btnView.on("click", function () {
            //Busco el producto y cargo su descripcion en un modal
            for (var i = 0; i < productos.length; i++) {
                if (productos[i].productId == id) {
                    //Actualizo la descripción
                    $('#nombreProducto').html(productos[i].productName);
                    $('#descripcion').html(productos[i].description);
                    $('#modalDescripcion').modal().show();
                }
            }
        });
        btnEdit.on("click", function () {
            cell1.html("<input type='number' data-id='" + id + "' max='" + maxcant + "' name='cell1' class='form-control' value='" + cell1.html() + "'/>");
            btnConfirm.removeClass("hidden");
            btnEdit.addClass("hidden");
            btnRemove.addClass("hidden");
            updatePrecio();
        });
        btnRemove.on("click", function () {
            row.remove();
            updatePrecio();
        });
        btnConfirm.on("click", function () {
            if (validateEditProduct()) {
                var parent = $(this).parent().parent();
                var elemcell1 = parent.find('[name = "cell1"]');
                var maxval = parseFloat(elemcell1.attr('max'));
                var value = parseFloat(elemcell1.val());
                if (value <= maxval) {
                    cell1.html($("[name='cell1']").val());
                    $(i_cant).val(value);
                    btnConfirm.addClass("hidden");
                    btnEdit.removeClass("hidden");
                    btnRemove.removeClass("hidden");
                }
                else {
                    toastr.warning("La cantidad máxima para ese campo es de " + maxval);
                }
                updatePrecio();
            }
        });
        cell3.append(btnView.add(btnEdit).add(btnRemove).add(btnConfirm));
        cell3.append(i_id);
        cell3.append(i_cant);
    };

    var updatePrecio = function () {
        var tBody = $("#tblProductos > TBODY");
        var precio = 0;
        for (var i = 0; i < productos.length; i++) {
            var id = productos[i].productId;
            var buscar = tBody.find('[data-id="' + id + '"]');
            if (buscar.length > 0) {
                var tr = buscar.parent().parent();
                var cant = parseFloat(tr[0].cells[0].innerText);
                precio += cant * parseFloat(productos[i].salePrice);
            }
        }
        precioProductos = precio;
        $("#importe").val(precioProductos + price);
        calculate();
    };


    var validateEditProduct = function () {
        if ($("[name='cell1']").val() == "" || $("[name='cell1']").val() == 0) {
            showWarningMessage("Atención", "El campo Cantidad no puede estar vacío.");
            return false;
        }
        if ($("[name='cell0']").val() == "") {
            showWarningMessage("Atención", "El campo Descripción no puede estar vacío.");
            return false;
        }
        return true;
    };

    cargarproductos();
    //$('#tipoServicio').trigger("change");
});

