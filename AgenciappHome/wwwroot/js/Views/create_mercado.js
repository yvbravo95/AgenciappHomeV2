$('#ServiceCost').trigger('change')

$(document).on("ready", function () {
    var productos;

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
                $('#inputClientMovil').val(data.movil);
                $('#inputClientEmail').val(data.email);
                $('#inputClientAddress').val(data.calle);
                $('#inputClientCity').val(data.city);
                $('#inputClientZip').val(data.zip);
                $('#inputClientState').val(data.state).trigger("change");
                $("#PaisResidencia").val(data.country);
                $("#PaisActual").val(data.country);
                $("#Estado").val(data.state);
                $("#PrimerNombre").val(data.name);
                $("#PrimerApellidos").val(data.lastName);
                $("#DireccionActual").val(data.calle);
                $("#ProvinciaActual").val(data.city);
                $("#EstadoActual").val(data.state);
                $("#CodigoPostalActual").val(data.zip);
                $("#Telefono").val(data.movil);
                $("#EmailActual").val(data.email);

                $("#remitente").html(data.name + " " + data.lastName);
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            }
        });
    };

    var mostrarEstados = function () {
        $("#inputClientState").empty();
        $("#inputClientState").append(new Option())
        $("#inputClientState").append(new Option("Alabama", "Alabama"));
        $("#inputClientState").append(new Option("Alaska", "Alaska"));
        $("#inputClientState").append(new Option("American Samoa", "American Samoa"));
        $("#inputClientState").append(new Option("Arizona", "Arizona"));
        $("#inputClientState").append(new Option("Arkansas", "Arkansas"));
        $("#inputClientState").append(new Option("Armed Forces Americas", "Armed Forces Americas"));
        $("#inputClientState").append(new Option("Armed Forces Europe, Canada, Africa and Middle East", "Armed Forces Europe, Canada, Africa and Middle East"));
        $("#inputClientState").append(new Option("Armed Forces Pacific", "Armed Forces Pacific"));
        $("#inputClientState").append(new Option("California", "California"));
        $("#inputClientState").append(new Option("Colorado", "Colorado"));
        $("#inputClientState").append(new Option("Connecticut", "Connecticut"));
        $("#inputClientState").append(new Option("Delaware", "Delaware"));
        $("#inputClientState").append(new Option("District of Columbia", "District of Columbia"));
        $("#inputClientState").append(new Option("Florida", "Florida"));
        $("#inputClientState").append(new Option("Georgia", "Georgia"));
        $("#inputClientState").append(new Option("Guam", "Guam"));
        $("#inputClientState").append(new Option("Hawaii", "Hawaii"));
        $("#inputClientState").append(new Option("Idaho", "Idaho"));
        $("#inputClientState").append(new Option("Illinois", "Illinois"));
        $("#inputClientState").append(new Option("Indiana", "Indiana"));
        $("#inputClientState").append(new Option("Iowa", "Iowa"));
        $("#inputClientState").append(new Option("Kansas", "Kansas"));
        $("#inputClientState").append(new Option("Kentucky", "Kentucky"));
        $("#inputClientState").append(new Option("Louisiana", "Louisiana"));
        $("#inputClientState").append(new Option("Maine", "Maine"));
        $("#inputClientState").append(new Option("Marshall Islands", "Marshall Islands"));
        $("#inputClientState").append(new Option("Maryland", "Maryland"));
        $("#inputClientState").append(new Option("Massachusetts", "Massachusetts"));
        $("#inputClientState").append(new Option("Michigan", "Michigan"));
        $("#inputClientState").append(new Option("Micronesia", "Micronesia"));
        $("#inputClientState").append(new Option("Minnesota", "Minnesota"));
        $("#inputClientState").append(new Option("Mississippi", "Mississippi"));
        $("#inputClientState").append(new Option("Missouri", "Missouri"));
        $("#inputClientState").append(new Option("Montana", "Montana"));
        $("#inputClientState").append(new Option("Nebraska", "Nebraska"));
        $("#inputClientState").append(new Option("Nevada", "Nevada"));
        $("#inputClientState").append(new Option("New Hampshire", "New Hampshire"));
        $("#inputClientState").append(new Option("New Jersey", "New Jersey"));
        $("#inputClientState").append(new Option("New Mexico", "New Mexico"));
        $("#inputClientState").append(new Option("New York", "New York"));
        $("#inputClientState").append(new Option("North Carolina", "North Carolina"));
        $("#inputClientState").append(new Option("North Dakota", "North Dakota"));
        $("#inputClientState").append(new Option("Northern Mariana Islands", "Northern Mariana Islands"));
        $("#inputClientState").append(new Option("Ohio", "Ohio"));
        $("#inputClientState").append(new Option("Oklahoma", "Oklahoma"));
        $("#inputClientState").append(new Option("Oregon", "Oregon"));
        $("#inputClientState").append(new Option("Palau", "Palau"));
        $("#inputClientState").append(new Option("Pennsylvania", "Pennsylvania"));
        $("#inputClientState").append(new Option("Puerto Rico", "Puerto Rico"));
        $("#inputClientState").append(new Option("Rhode Island", "Rhode Island"));
        $("#inputClientState").append(new Option("South Carolina", "South Carolina"));
        $("#inputClientState").append(new Option("South Dakota", "South Dakota"));
        $("#inputClientState").append(new Option("Tennessee", "Tennessee"));
        $("#inputClientState").append(new Option("Texas", "Texas"));
        $("#inputClientState").append(new Option("Utah", "Utah"));
        $("#inputClientState").append(new Option("Vermont", "Vermont"));
        $("#inputClientState").append(new Option("Virgin Islands", "Virgin Islands"));
        $("#inputClientState").append(new Option("Virginia", "Virginia"));
        $("#inputClientState").append(new Option("Washington", "Washington"));
        $("#inputClientState").append(new Option("West Virginia", "West Virginia"));
        $("#inputClientState").append(new Option("Wisconsin", "Wisconsin"));
        $("#inputClientState").append(new Option("Wyoming", "Wyoming"));
    };

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
    };

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
    };

    var calcMoney = function () {
        var precio = $("#Precio").val();
        var cargos = $("#Cargos").val();
        var descuento = $("#Descuento").val();
        var serviceCost = $("#ServiceCost").val();
        var precioTotalValue = parseFloat(precio) - parseFloat(descuento) + parseFloat(cargos) + parseFloat(serviceCost);

        if (!blockPayment) {
            if ($("#checkpago").is(":checked")) {
                var pagoCash = parseFloat($("#pagoCash").val());
                var pagoZelle = parseFloat($("#pagoZelle").val());
                var pagoCheque = parseFloat($("#pagoCheque").val());
                var pagoTransf = parseFloat($("#pagoTransf").val());
                var pagoFinance = parseFloat($("#pagoFinance").val());
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

                balanceValue = precioTotalValue - pagoCash - pagoZelle - pagoCheque - pagoCrDeb - pagoTransf - pagoWeb - pagoCredito - pagoFinance;

                $("#pagoCash").attr('max', (balanceValue + pagoCash).toFixed(2));
                $("#pagoFinance").attr('max', (balanceValue + pagoFinance).toFixed(2));
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
        }
        else {
            const pagado = parseFloat($("#refPagado").val());
            balanceValue = precioTotalValue - pagado;
        }


        $("#precioTotalValue").html(precioTotalValue.toFixed(2));
        $("#balanceValue").html(balanceValue.toFixed(2));
        if ($("#balanceValue").html() == '-0.00')
            $("#balanceValue").html('0.00');

        $("#Balance").val(parseFloat(balanceValue.toFixed(2)));
    };

    var calcMoneyPayment = function () {
        var precio = $("#Precio").val();
        var cargos = $("#Cargos").val();
        var descuento = $("#Descuento").val();
        var pagado = $("#ValorPagado").val();
        var serviceCost = $("#ServiceCost").val();
        var precioTotalValue = parseFloat(precio) - parseFloat(descuento) + parseFloat(cargos) + parseFloat(serviceCost);
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

        if (blockPayment) {
            pagado = parseFloat($("#refPagado").val());
        }

        var balanceValue = precioTotalValue - pagado;

        $("#precioTotalValue").html(precioTotalValue.toFixed(2));
        $("#balanceValue").html(balanceValue.toFixed(2));
        $("#Balance").val(parseFloat(balanceValue.toFixed(2)));
        $("#ValorPagado").attr("max", max.toFixed(2));
    };

    var validarInputVacios = function () {
        if ($("#pagoCash").val() == "")
            $("#pagoCash").val(0);
        if ($("#pagoFinance").val() == "")
            $("#pagoFinance").val(0);
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
        if ($("#ValorPagado").val() == "")
            $("#ValorPagado").val(0);
        if ($("#Cargos").val() == "")
            $("#Cargos").val(0);
        if ($("#Descuento").val() == "")
            $("#Descuento").val(0);
    };

    var cargarproductos = function() {
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

    var addProductBodegaToTable = function (cant, precio, descripcion, id, maxcant) {
        //Get the reference of the Table's TBODY element.
        var tBody = $("#tblProductos > TBODY")[0];

        var index = $(tBody).children().length;

        var row = tBody.insertRow(index);


        var cell1 = $(row.insertCell(-1));
        cell1.append(cant);

        var cell2 = $(row.insertCell(-1));
        cell2.append(precio);
        
        var cell3 = $(row.insertCell(-1));
        cell3.append(descripcion);

        var cell4 = $(row.insertCell(-1));
        cell4[0].style.display = "inline-flex";
        var btnView = $("<button type='button' data-id='" + id + "' class='btn btn-blue' title='View' style='font-size: 10px;padding:9px;margin-right:1px;'><i class='fa fa-eye'></i></button>");
        var btnEdit = $("<button type='button' data-id='" + id + "' class='btn btn-warning' title='Editar' style='font-size: 10px;padding:9px'><i class='fa fa-pencil'></i></button>");
        var btnRemove = $("<button type='button' data-id='" + id + "' class='btn btn-danger pull-right' title='Eliminar' style='font-size: 10px;padding:9px'><i class=' fa fa-close'></button>");
        var btnConfirm = $("<button type='button' data-id='" + id + "' class='btn btn-success hidden' title='Confirmar' style='font-size: 10px;padding:9px'><i class='fa fa-check'></button>");

        var i_id = $("<input type='hidden' value = '" + id +"' name='Productos[" + index + "].ProductId'/>");
        var i_cant = $("<input type='hidden' value = '" + cant + "' name='Productos[" + index + "].Cantidad'/>");
        var i_price = $("<input type='hidden' value = '" + precio + "' name='Productos[" + index + "].Price'/>");

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
            cell2.html("<input type='number' data-id='" + id + "' name='cell2' class='form-control' value='" + cell2.html() + "'/>");
            btnConfirm.removeClass("hidden");
            btnEdit.addClass("hidden");
            btnRemove.addClass("hidden");
        });
        btnRemove.on("click", function () {
            row.remove();
            updatePrecio();
        });
        btnConfirm.on("click", function () {
            if (validateEditProduct()) {
                var parent = $(this).parent().parent();
                var elemcell1 = parent.find('[name = "cell1"]');
                var elemcell2 = parent.find('[name = "cell2"]');
                var maxval = parseFloat(elemcell1.attr('max'));
                var value = parseFloat(elemcell1.val());
                var value2 = parseFloat(elemcell2.val());
                if (value > maxval) {
                    toastr.warning("La cantidad máxima para ese campo es de " + maxval);
                    return;
                }

                if (value2 === undefined) {
                    toastr.warning("El precio del producto no puede estar vacío.");
                    return;
                }

                cell1.html($("[name='cell1']").val());
                cell2.html($("[name='cell2']").val());
                $(i_cant).val(value);
                $(i_price).val(value2);
                btnConfirm.addClass("hidden");
                btnEdit.removeClass("hidden");
                btnRemove.removeClass("hidden");
                updatePrecio();
            }
        });
        cell4.append(btnView.add(btnEdit).add(btnRemove).add(btnConfirm));
        cell4.append(i_id);
        cell4.append(i_cant);
        cell4.append(i_price);
    };

    var updatePrecio = function() {
        var tBody = $("#tblProductos > TBODY");
        var precio = 0;
        for (var i = 0; i < productos.length; i++) {
            var id = productos[i].productId;
            var buscar = tBody.find('[data-id="' + id + '"]');
            if (buscar.length > 0) {
                var tr = buscar.parent().parent();
                var cant = parseFloat(tr[0].firstChild.innerHTML);
                var price = parseFloat(tr[0].firstChild.nextSibling.innerHTML);
                precio += cant * price;
            }
        }
        $('#Precio').val(precio.toFixed(2));
        calcMoney();
    };

    var validateEditProduct = function () {
        if ($("[name='cell1']").val() == "" || $("[name='cell1']").val() == 0) {
            showWarningMessage("Atención", "El campo Peso no puede estar vacío.");
            return false;
        }
        if ($("[name='cell0']").val() == "") {
            showWarningMessage("Atención", "El campo Descripción no puede estar vacío.");
            return false;
        } 
        return true;
    };

    $('#inputClientMovil').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });

    $(".selectdos").select2({ width: "100%" });

    $("#inputClientState").select2({
        placeholder: "Estado",
        width: "100%"
    });

    $("#selectProduct").select2({
        placeholder: "Buscar producto en la bodega",
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
                $("#ClientId").val(),
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
            $("#pagoFinance").val(0);
            $("#pagoZelle").val(0);
            $("#pagoCheque").val(0);
            $("#pagoCredito").val(0);
            $("#pagoTransf").val(0);
            $("#pagoWeb").val(0);
        }
        calcMoney()
    });
    $("#Precio,#Descuento,#Cargos,#check_credito,#pagoCash,#pagoFinance,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb").on("change", validarInputVacios);

    $("#Precio,#Descuento,#Cargos,#check_credito,#pagoCash,#pagoFinance,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb").on("keyup", calcMoney).on('change', calcMoney);

    $("#ValorPagado").on("keyup", calcMoneyPayment).on("change", calcMoneyPayment);

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

        if (tipoPago == "Zelle" || tipoPago == "Cheque" || tipoPago == "Transferencia Bancaria" || tipoPago == "Financiamiento") {
            $('#contNotaPago').show();
        }
        else {
            $('#contNotaPago').hide();
        }
    });

    $('#tipoPago').trigger('change');

    $("#selectProduct").on("select2:select", function () {
        var productId = $(this).val();

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

            addProductBodegaToTable(1, prodSelect.salePrice, desc, prodSelect.productId, prodSelect.quantity);
            updatePrecio();
        }
        else {
            toastr.warning("El producto " + prodSelect.productName + " ya se añadió a la tabla");
        }
        $("#selectProduct").val("").trigger("change");
    });

    $(".btnView").on("click", function () {
        $('#nombreProducto').html($(this).data("name"));
        $('#descripcion').html($(this).data("description"));
        $('#modalDescripcion').modal().show();
    });

    mostrarEstados();
    $("#reno_prorroga").hide();
    $("#renovacion").hide();
    $("#prorroga").hide();
    cargarproductos();
    calcMoneyPayment();
    if ($("#ClientId").val()) {
        showClient();
    }
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

$("#zc").steps({
    headerTag: "h6",
    bodyTag: "fieldset",
    transitionEffect: "fade",
    titleTemplate: '<span class="step">#index#</span> #title#',
    labels: {
        previous: "Anterior",
        next: "Siguiente",
        finish: 'Guardar'
    },
    onStepChanging: function (event, currentIndex, newIndex) {
        // Allways allow previous action even if the current form is not valid!
        if (currentIndex > newIndex) {
            if (newIndex == 1)
                $("a[href=#previous]").hide();
            return true;
        }

        if (newIndex == 1) {
            //if ($("#ClientId").val() == "") {
            //    showWarningMessage("Atención", "El campo cliente es obligatorio.");
            //    return false;
            //}
        }        
        return true;
    },
    onFinishing: function (event, currentIndex) {
        //bloquear el boton Enviar
        $("#zc").submit();
    },
    onFinished: function (event, currentIndex) {
    }
});

$("a[href=#previous]").hide();

$("a[href=#next]").click(function () {
    $("a[href=#previous]").show();
});


/*********************************************/