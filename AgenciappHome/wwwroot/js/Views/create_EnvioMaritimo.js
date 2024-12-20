$(document).ready(function () {
    var agencyReyEnvios = "2F7B03FB-4BE1-474D-8C95-3EE8C6EAEAC1";
    var agencyRapidMId = "680B03D4-A92D-44F5-8B34-FD70E0D9847C";
    var agencyEnvieConFe = "68A559FA-AA00-4D52-93B5-DD833B37ED02";
    var productsSelected = new Array();
    var pesoProductos = 0;
    var pesoProductosDuradero = 0;
    var productos = new Array();
    var addcosto = 0;
    var addprecio = 0;
    var valueMiscelaneas = 6.6;
    let enableDuadero = false;
    var byPackage = false;
    var cantidadProductosByPackage = 0; //Excepto los de la bodega
    var precioslibras = [];
    var priceByPackage = 0;

    if (agencyId.toUpperCase() == agencyEnvieConFe.toUpperCase()) {
        valueMiscelaneas = 3.0;
    }
    /**********************************
     *   Form Wizard Step Icon
     **********************************/
    $(".step-icon").each(function () {
        var $this = $(this);
        if ($this.siblings("span.step").length > 0) {
            $this.siblings("span.step").empty();
            $(this).appendTo($(this).siblings("span.step"));
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
            finish: "Enviar",
        },
        onStepChanging: function (event, currentIndex, newIndex) {
            // Allways allow previous action even if the current form is not valid!
            if (currentIndex > newIndex) {
                return true;
            }

            //----------------Step1
            var error = false;

            if (newIndex == 0) {
                alert("df   ");
            }

            if (newIndex == 1) {
                if (
                    $("#single-placeholder-2").val() == "" ||
                    $("#single-placeholder-3").val() == ""
                ) {
                    return false;
                }

                if ($("#inputClientMovil").val() == "") {
                    showWarningMessage(
                        "Atención",
                        "El campo de teléfono del cliente es obligatorio."
                    );
                    return false;
                }
                if (agencyId.toUpperCase() != agencyReyEnvios) {
                    if ($("#inputClientId").val() == "") {
                        showWarningMessage(
                            "Atención",
                            "El campo ID del cliente es obligatorio."
                        );
                        return false;
                    }
                }

                if ($("#provincia").val() == null) {
                    showWarningMessage(
                        "Atención",
                        "El campo de provincia del contacto es obligatorio."
                    );
                    return false;
                }
                getprecioxlibra();
            }

            //----------------Step2

            if (newIndex == 2) {
                if ($("#txtNameVA").val() == "") return false;
                else if ($("#tblProductos > TBODY")[0].rows.length == 1) {
                    return false;
                } else {
                    //Preparando el step3 con los productos seleccinados
                    var listProduct = new Array();

                    var tBody = $("#tblProductos > TBODY")[0];

                    for (var i = 0; i < tBody.rows.length - 1; i++) {
                        var fila = tBody.rows[i];
                        listProduct[i] = new Array();

                        listProduct[i]["cantidad"] = $(fila.children[0]).html();
                        listProduct[i]["descripcion"] = $(fila.children[1]).html();

                        var chk =
                            '<label class="custom-control custom-checkbox">\n' +
                            '                                    <input type="checkbox" class="custom-control-input order-select" id="chk' +
                            i +
                            '" />\n' +
                            '                                    <span class="custom-control-indicator"></span>\n' +
                            '                                    <span class="custom-control-description"></span>\n' +
                            "                                </label>";

                        var tr =
                            "<tr><td>" +
                            chk +
                            "</td><td>" +
                            '<input class="form-control" style="width: 85px" type="number" min="1" max="' +
                            listProduct[i]["cantidad"] +
                            '" value="1">' +
                            "</td><td>" +
                            listProduct[i]["cantidad"] +
                            "</td><td>" +
                            listProduct[i]["descripcion"] +
                            "</td></tr>";

                        $("#tableBody").html($("#tableBody").html() + tr);
                    }
                }
            }
            return true;
        },
        onFinishing: function (event, currentIndex) {
            return sendOrder();
        },
        onFinished: function (event, currentIndex) { },
    });
    $('ul[aria-label="Pagination"]').prepend(
        '<li class="" aria-disabled="false"><a id="cancel" style="background-color:#ff7e7e;" href="/EnvioMaritimo/index/" role="menuitem">Cancelar</a></li>'
    );

    $("a[href='#next']").addClass("hidden");
    $("#cancel").hide();

    $("a[href=#previous]").hide();
    $("a[href=#previous]").click(function () {
        $("#showAllContacts").removeAttr("disabled");
        $("a[href=#previous]").hide();
    });
    $("a[href=#next]").click(function () {
        $("#showAllContacts").attr("disabled", "disabled");

        $("a[href=#previous]").show();
    });

    /*********************************************/

    function block($this) {
        var block_ele = $this.closest(".card");
        block_ele.block({
            message:
                '<div id="load" class="ft-refresh-cw icon-spin font-medium-2"></div>',
            overlayCSS: {
                backgroundColor: "#FFF",
                cursor: "wait",
            },
            css: {
                border: 0,
                padding: 0,
                backgroundColor: "none",
            },
        });
    }

    function unblock($this) {
        var block_ele = $this.closest(".card");
        block_ele.unblock();
    }

    var calcMoney = function () {
        UpdatePriceProductsTable();

        var priceProductBodega = 0;
        productsSelected.forEach((element) => {
            var row = $('tr[data-id="' + element.idProducto + '"]');
            var cant = parseInt($(row).first().children(0).first().html());
            priceProductBodega += element.precioVentaReferencial * cant;
        });

        var cantLb = $("#CantLb").val();
        var precioLb = $("#precioxlibra").val();
        var oCargos = $("#OtrosCostos").val();
        var cargoAdicional = parseFloat($('[name="cargoAdicional"]').val());
        var valAduanalValue = $("#valAduanalValue").html();
        const priceLbDuradero = $("#PriceLbDuradero").val();
        const cantLbDuradero = $("#CantLbDuradero").val();

        var precioTotalValue =
            parseFloat(valAduanalValue) +
            parseFloat(oCargos) +
            cargoAdicional + addprecio;

        if (byPackage) {
            precioTotalValue += priceByPackage;
        }
        else {
            precioTotalValue += cantLb * precioLb;
        }

        precioTotalValue += priceProductBodega;

        if (enableDuadero) {
            precioTotalValue += priceLbDuradero * cantLbDuradero;
        }

        if ($("#checkpago").is(":checked")) {
            var pagoCash = parseFloat($("#pagoCash").val());
            var pagoZelle = parseFloat($("#pagoZelle").val());
            var pagoCheque = parseFloat($("#pagoCheque").val());
            var pagoTransf = parseFloat($("#pagoTransf").val());
            var pagoWeb = parseFloat($("#pagoWeb").val());

            var pagoCrDeb = parseFloat($("#pagoCredito").val());
            var pagoCrDebReal =
                parseFloat($("#pagoCredito").val()) /
                (1 + parseFloat($("#fee").html()) / 100);
            var feeCrDeb = pagoCrDeb - pagoCrDebReal;
            if (pagoCrDeb > 0) {
                $("#contfee").show();
            } else {
                $("#contfee").hide();
            }
            precioTotalValue += feeCrDeb;

            var pagoCredito = 0;
            if ($("#check_credito").is(":checked")) {
                pagoCredito = parseFloat($("#credito").html()).toFixed(2);
                pagoCredito =
                    pagoCredito > precioTotalValue ? precioTotalValue : pagoCredito;
            }

            var balanceValue =
                precioTotalValue -
                pagoCash -
                pagoZelle -
                pagoCheque -
                pagoCrDeb -
                pagoTransf -
                pagoWeb -
                pagoCredito;

            $("#pagoCash").attr("max", (balanceValue + pagoCash).toFixed(2));
            $("#pagoZelle").attr("max", (balanceValue + pagoZelle).toFixed(2));
            $("#pagoCheque").attr("max", (balanceValue + pagoCheque).toFixed(2));
            $("#pagoTransf").attr("max", (balanceValue + pagoTransf).toFixed(2));
            $("#pagoWeb").attr("max", (balanceValue + pagoWeb).toFixed(2));
            $("#pagoCredito").attr(
                "max",
                (
                    balanceValue +
                    pagoCrDebReal +
                    ((balanceValue + pagoCrDebReal) * parseFloat($("#fee").html())) / 100
                ).toFixed(2)
            );
            $("#pagar_credit").html(
                "$" +
                (
                    balanceValue +
                    pagoCrDebReal +
                    ((balanceValue + pagoCrDebReal) * parseFloat($("#fee").html())) /
                    100
                ).toFixed(2) +
                " (" +
                (
                    ((balanceValue + pagoCrDebReal) * parseFloat($("#fee").html())) /
                    100
                ).toFixed(2) +
                " fee)"
            );

            //Valor Sale Amount en authorization card
            $('[name = "AuthSaleAmount"]').val(pagoCrDebReal.toFixed(2));
            var aconvcharge = parseFloat($('[name = "AuthConvCharge"]').val());
            var total = pagoCrDebReal + (pagoCrDebReal * aconvcharge) / 100;
            $('[name = "TotalCharge"]').val(total.toFixed(2));
        } else {
            tipoPagoId = $('[name = "TipoPago"]').val();
            tipoPago = $('option[value = "' + tipoPagoId + '"]').html();

            if (tipoPago == "Crédito o Débito") {
                var fee = parseFloat($("#fee").html());
                //Valor Sale Amount en authorization card
                $('[name = "AuthSaleAmount"]').val(precioTotalValue.toFixed(2));
                precioTotalValue = precioTotalValue + precioTotalValue * (fee / 100);
                $('[name = "TotalCharge"]').val(precioTotalValue.toFixed(2));
            }
            var balanceValue = 0;
            if ($("#check_credito").is(":checked")) {
                if (
                    precioTotalValue.toFixed(2) - parseFloat($("#credito").html()) >
                    0
                ) {
                    $("#ValorPagado").attr(
                        "max",
                        precioTotalValue.toFixed(2) - parseFloat($("#credito").html())
                    );
                    $("#ValorPagado").val(
                        (
                            precioTotalValue.toFixed(2) - parseFloat($("#credito").html())
                        ).toFixed(2)
                    );
                } else {
                    $("#ValorPagado").attr("max", 0);
                    $("#ValorPagado").val(0);
                }
            } else {
                $("#ValorPagado").val(precioTotalValue.toFixed(2));
                $("#ValorPagado").attr("max", precioTotalValue.toFixed(2));
            }
        }
        $("#precioTotalValue").html(precioTotalValue.toFixed(2));
        $("#balanceValue").html(balanceValue.toFixed(2));
        if ($("#balanceValue").html() == "-0.00") $("#balanceValue").html("0.00");

        $("#productosBodegaValue").html(priceProductBodega.toFixed(2));
    };

    var calcMoneyPayment = function () {
        UpdatePriceProductsTable();

        var priceProductBodega = 0;
        productsSelected.forEach((element) => {
            var row = $('tr[data-id="' + element.idProducto + '"]');
            var cant = parseInt($(row).first().children(0).first().html());
            priceProductBodega += element.precioVentaReferencial * cant;
        });
        var cantLb = $("#CantLb").val();
        var precioLb = $("#precioxlibra").val();
        var oCargos = $("#OtrosCostos").val();
        var pagado = $("#ValorPagado").val();
        var valAduanalValue = $("#valAduanalValue").html();
        var cargoAdicional = parseFloat($('[name="cargoAdicional"]').val());
        const priceLbDuradero = $("#PriceLbDuradero").val();
        const cantLbDuradero = $("#CantLbDuradero").val();

        var precioTotalValue =
            parseFloat(valAduanalValue) +
            parseFloat(oCargos) +
            cargoAdicional + addprecio;

        if (byPackage) {
            precioTotalValue += priceByPackage;
        }
        else {
            precioTotalValue += cantLb * precioLb;
        }

        precioTotalValue += priceProductBodega;

        if (enableDuadero) {
            precioTotalValue += priceLbDuradero * cantLbDuradero;
        }

        var max = precioTotalValue;

        if (tipoPago == "Crédito o Débito") {
            var fee = parseFloat($("#fee").html());
            var pagdoReal = pagado / (1 + fee / 100);
            var feeCrDeb = pagado - pagdoReal;

            precioTotalValue = precioTotalValue + feeCrDeb;
            max = max + (max * fee) / 100;

            //Valor Sale Amount en authorization card
            $('[name = "AuthSaleAmount"]').val(pagdoReal.toFixed(2));
            $('[name = "TotalCharge"]').val(pagado);
        }

        precioTotalValue = precioTotalValue.toFixed(2);
        var balanceValue = precioTotalValue - pagado;

        if ($("#check_credito").is(":checked")) {
            var pagoCredito = parseFloat($("#credito").html()).toFixed(2);
            pagoCredito =
                pagoCredito > precioTotalValue ? precioTotalValue : pagoCredito;
            balanceValue = precioTotalValue - pagado - pagoCredito;
        } else {
            balanceValue = precioTotalValue - pagado;
        }

        $("#precioTotalValue").html(precioTotalValue);
        $("#balanceValue").html(balanceValue.toFixed(2));
        $("#ValorPagado").attr("max", max.toFixed(2));
    };

    var validarInputVacios = function () {
        if ($("#pagoCash").val() == "") $("#pagoCash").val(0);
        if ($("#pagoZelle").val() == "") $("#pagoZelle").val(0);
        if ($("#pagoCheque").val() == "") $("#pagoCheque").val(0);
        if ($("#pagoCredito").val() == "") $("#pagoCredito").val(0);
        if ($("#pagoTransf").val() == "") $("#pagoTransf").val(0);
        if ($("#pagoWeb").val() == "") $("#pagoWeb").val(0);
        if ($("#PriceLb").val() == "") $("#PriceLb").val(0);
        if ($("#OtrosCostos").val() == "") $("#OtrosCostos").val(0);
        if ($("#ValorPagado").val() == "") $("#ValorPagado").val(0);
        if ($("#CantLb").val() == "") $("#CantLb").val(0);
        if ($("#CantLbDuradero").val() == "") $("#CantLbDuradero").val(0);
    };

    $(
        "#CantLb,#PriceLb,#OtrosCostos,#CantidadCombo,#check_credito,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb"
    ).on("change", validarInputVacios);

    $("#checkpago").on("click", function () {
        if ($("#checkpago").is(" :checked")) {
            $("#untipopago").attr("hidden", "hidden");
            $(".multipopago").removeAttr("hidden");
            $("#contfee").hide();
        } else {
            $(".multipopago").attr("hidden", "hidden");
            $("#untipopago").removeAttr("hidden");
        }
        calcMoney();
    });

    $("#check_credito").on("click", function () {
        $("#pagoCash").val(0);
        $("#pagoZelle").val(0);
        $("#pagoCheque").val(0);
        $("#pagoCredito").val(0);
        $("#pagoTransf").val(0);
        $("#pagoWeb").val(0);
    });

    // Para cargar el valor del precio por libra

    function getprecioxlibra() {
        var idcontacto = $('[name = "selectContact"]').val();
        var idmayorista = $("#mayorista").val();
        $.ajax({
            type: "POST",
            url: "/EnvioMaritimo/getPrecioxLibra",
            async: false,
            data: {
                idmayorista: idmayorista,
                idContacto: idcontacto,
            },
            beforeSend: function () {
                $.blockUI({
                    message: '<div class="ft-refresh-cw icon-spin font-medium-2"></div>',
                    timeout: 60000,
                    overlayCSS: {
                        backgroundColor: "#FFF",
                        opacity: 0.8,
                        cursor: "wait",
                    },
                    css: {
                        border: 0,
                        padding: 0,
                        backgroundColor: "transparent",
                    },
                });
            },
            success: function (data) {
                precioslibras = data.preciosLibras??[];
                if (precioslibras.length > 0) {
                    $("#precioxlibra").val(data.preciosLibras[0].item2);
                    $("#tag_container").html("");
                    for (var i = 1; i < precioslibras.length; i++) {
                        var u = $(`
                             <span class="tag tag-primary tag_paquete mx-1" style="cursor: pointer;" name="utility" data-value="${precioslibras[i].item1.toUpperCase()}">${precioslibras[i].item1}</span>
                        `)
                        u.on("click", function () {
                            var value = $(this).attr("data-value");
                            $("#newDescrip").val(value);
                            $("#newDescrip").focus();
                            var e = jQuery.Event("keypress");
                            e.which = 13; // # Some key code value
                            e.key = "Enter";
                            $("#newDescrip").trigger(e);
                        });

                        $("#tag_container").append(u)
                    }
                }
                else {
                    $("#precioxlibra").val(data.precioporlibra);
                }
                $("#PriceLbDuradero").val(data.precioporlibraDuradero);
                byPackage = data.byPackage;
                if (byPackage) {
                    $('#labelPrecio').html("Precio por paquete")
                }
                else {
                    $('#labelPrecio').html("Precio x Lb")
                }
                calcMoney();
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
        });
        $.unblockUI();
    }

    //Al elegir un mayorista se cambie el precio x libra
    var idMayoristabyTransferencia = $("#idMayoristabyTransf").html();

    $(document).on("change", "#mayorista", function () {
        var nombre = $('option[value="' + $(this).val() + '"]').html();
        // Si existe un mayorista por transferencia envio una alerta
        if (idMayoristabyTransferencia != "") {
            swal(
                {
                    title: "Está usted seguro",
                    text:
                        "Esta seguro que desea cambiar al mayorista por transferencia a " +
                        nombre,
                    type: "warning",
                    showCancelButton: true,
                    confirmButtonColor: "#DA4453",
                    confirmButtonText: "Si, cambiar",
                    cancelButtonText: "No, mantener",
                    closeOnConfirm: false,
                    closeOnCancel: false,
                },
                function (isConfirm) {
                    if (isConfirm) {
                        idMayoristabyTransferencia = "";
                        swal.close();
                        getprecioxlibra();
                    } else {
                        $("#mayorista").val(idMayoristabyTransferencia).trigger("change");

                        swal.close();
                    }
                }
            );
        } else {
            getprecioxlibra();
        }
    });

    //placeholder cliente y contacto

    $("#provincia").on("change", () => selectMunicipios());

    var selectMunicipios = function () {
        var provincia = $("#provincia").val();
        if (!provincia) return;

        $.ajax({
            url: "/Provincias/Municipios?nombre=" + provincia,
            type: "POST",
            dataType: "json",
            success: function (response) {
                var municipios = $("#municipio");
                municipios.empty();
                municipios.append(new Option());
                for (var i in response) {
                    var m = response[i];
                    municipios.append(new Option(m, m));
                }
                municipios.val(selected_municipio).trigger("change");
            },
        });
    };
    $("#municipio").select2({
        placeholder: "Municipio",
    });

    $(".select2-placeholder-selectClient").select2({
        placeholder: "Buscar cliente por teléfono, nombre o apellido",
        val: null,
        ajax: {
            type: "POST",
            dataType: "json",
            delay: 500,
            url: "/Clients/findClient",
            data: function (params) {
                var query = {
                    search: params.term,
                };

                // Query parameters will be ?search=[term]&type=public
                return query;
            },
            processResults: function (data) {
                // Transforms the top-level key of the response object from 'items' to 'results'
                return {
                    results: $.map(data, function (obj) {
                        return { id: obj.clientId, text: obj.fullData };
                    }),
                };
            },
        },
    });

    $(".hide-search-clientState").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Estado",
    });

    $(".select2-placeholder-selectContact").select2({
        placeholder: "Buscar contacto por teléfono, nombre o apellido",
        text: " ",
        ajax: {
            type: "POST",
            dataType: "json",
            delay: 500,
            url: "/Contacts/findContacts",
            data: function (params) {
                var query = {
                    search: params.term,
                    idClient: $(".select2-placeholder-selectClient").val(),
                };

                // Query parameters will be ?search=[term]&type=public
                return query;
            },
            processResults: function (data) {
                // Transforms the top-level key of the response object from 'items' to 'results'
                return {
                    results: $.map(data, function (obj) {
                        return {
                            id: obj.contactId,
                            text: obj.phone1 + "-" + obj.name + " " + obj.lastName,
                        };
                    }),
                };
            },
        },
    });

    $(".hide-search-contactCity2").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Provincia",
    });

    $("#inputClientMovil").mask("(000)-000-0000", {
        placeholder: "(000)-000-0000",
    });

    $("#inputContactPhoneMovil").mask("(000)-000-0000", {
        placeholder: "(000)-000-0000",
    });

    $("#contactCI").mask("00000000000", {
        placeholder: "Carnet de Identidad",
    });

    $("#inputContactPhoneHome").mask("(000)-000-0000", {
        placeholder: "(000)-000-0000",
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
        placeholder: "Buscar producto en la bodega",
        val: null,
    });

    InitSelectProduct();
    /**********Clientes y Contactos ***********/

    var showClient = function () {
        var value = $(".select2-placeholder-selectClient").val();
        selectedClient = value;
        $.ajax({
            type: "POST",
            url: "/Clients/GetClient",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(value),
            async: false,
            success: function (data) {
                //Datos del Cliente en Authorization Card
                $("#nameClientCard").html(
                    "<strong>Nombre: </strong>" + data.name + " " + data.lastName
                );
                $("#phoneClientCard").html("<strong>Teléfono: </strong>" + data.movil);
                $("#emailClientCard").html("<strong>Email: </strong>" + data.email);
                $("#countryClientCard").html("<strong>País: </strong>" + data.country);
                $("#cityClientCard").html("<strong>Ciudad: </strong>" + data.city);
                $("#addressClientCard").html(
                    "<strong>Dirección: </strong>" + data.calle
                );
                $("#AuthaddressOfSend").val(data.calle);
                $("#Authemail").val(data.email);
                $("#Authphone").val(data.movil);

                //Datos del Cliente en Step 1
                $("#inputClientName").val(data.name);
                $("#inputClientName2").val(data.name2);
                $("#inputClientLastName").val(data.lastName);
                $("#inputClientLastName2").val(data.lastName2);
                $("#inputClientMovil").val(data.movil);
                $("#inputClientEmail").val(data.email);
                $("#inputClientAddress").val(data.calle);
                $("#inputClientId").val(data.id);
                $("#inputClientCity").val(data.city);
                $("#inputClientZip").val(data.zip);
                $("#inputClientState").val(data.state).trigger("change");

                $("#remitente").html(data.name + " " + data.lastName);

                if (data.conflictivo) {
                    $("#conflictivo").removeAttr("hidden");
                }
                else {
                    $("#conflictivo").attr("hidden", "hidden");
                }

                if (data.getCredito && data.getCredito > 0) {
                    $("#div_credito").removeAttr("hidden");
                    $("#credito").html(data.getCredito);
                }
                else {
                    $("#div_credito").attr("hidden", "hidden");
                    $("#credito").html(0);
                }

                selectedClientData = data;
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
        });
    };

    var showContact = function () {
        var idContacto = $(".select2-placeholder-selectContact").val();
        if (idContacto != null) {
            $.ajax({
                type: "POST",
                url: "/OrderNew/GetContact",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(idContacto),
                async: false,
                success: function (data) {
                    $("#inputContactName").val(data.name);
                    $("#inputContactLastName").val(data.lastName);
                    $("#inputContactPhoneMovil").val(data.movilPhone);
                    $("#inputContactPhoneHome").val(data.casaPhone);
                    $("#contactDireccion").val(data.direccion);
                    $("#provincia").val(data.city).trigger("change");
                    $("#municipio").val(data.municipio);
                    $("#reparto").val(data.reparto);
                    $("#contactCI").val(data.ci);

                    $("#destinatario").html(data.name + " " + data.lastName);

                    $("a[href='#next']").removeClass("hidden");
                    $("#cancel").show();

                    //getprecioxlibra();
                    selected_municipio = data.municipio;
                },
                failure: function (response) {
                    showErrorMessage("ERROR", response.statusText);
                },
                error: function (response) {
                    showErrorMessage("ERROR", response.statusText);
                },
            });
        }
    };

    var showContactsOfAClient = function () {
        var idClient = $(".select2-placeholder-selectClient").val();
        $("#inputContactName").val("");
        $("#inputContactLastName").val("");
        $("#inputContactPhoneMovil").val("");
        $("#inputContactPhoneHome").val("");
        $("#contactDireccion").val("");
        $("#provincia").val("").trigger("change");
        $("#municipio").val("");
        $("#reparto").val("");
        $("#contactCI").val("");

        $.ajax({
            type: "GET",
            url: "/Contacts/GetContactsOfAClient?id=" + idClient,
            dataType: "json",
            contentType: "application/json",
            async: true,
            success: function (data) {
                $("[name='selectContact']").empty();
                $("[name='selectContact']").append(new Option());

                if (data.length == 0) {
                    //showAllContact();
                } else {
                    for (var i = 0; i < data.length; i++) {
                        var contactData;
                        if (data[i].phone1 != "")
                            contactData =
                                data[i].phone1 + " - " + data[i].name + " " + data[i].lastName;
                        else
                            contactData =
                                data[i].phone2 + " - " + data[i].name + " " + data[i].lastName;
                        $("[name='selectContact']").append(
                            new Option(contactData, data[i].contactId)
                        );
                    }
                    var last = data[data.length - 1];
                    if (
                        selectedContact != null &&
                        selectedContact != "00000000-0000-0000-0000-000000000000"
                    ) {
                        $("[name='selectContact']").val(selectedContact).trigger("change");
                        selectedContact = "00000000-0000-0000-0000-000000000000";
                    } else {
                        $("[name='selectContact']").val(last.contactId).trigger("change");
                    }
                    showContact();
                    $("#editarContacto, a[href='#next']").removeClass("hidden");
                    $("#cancel").show();
                }
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
        });
    };

    var mostrarEstados = function () {
        $("#inputClientState").empty();
        $("#inputClientState").append(new Option());
        $("#inputClientState").append(new Option("Alabama", "Alabama"));
        $("#inputClientState").append(new Option("Alaska", "Alaska"));
        $("#inputClientState").append(
            new Option("American Samoa", "American Samoa")
        );
        $("#inputClientState").append(new Option("Arizona", "Arizona"));
        $("#inputClientState").append(new Option("Arkansas", "Arkansas"));
        $("#inputClientState").append(
            new Option("Armed Forces Americas", "Armed Forces Americas")
        );
        $("#inputClientState").append(
            new Option(
                "Armed Forces Europe, Canada, Africa and Middle East",
                "Armed Forces Europe, Canada, Africa and Middle East"
            )
        );
        $("#inputClientState").append(
            new Option("Armed Forces Pacific", "Armed Forces Pacific")
        );
        $("#inputClientState").append(new Option("California", "California"));
        $("#inputClientState").append(new Option("Colorado", "Colorado"));
        $("#inputClientState").append(new Option("Connecticut", "Connecticut"));
        $("#inputClientState").append(new Option("Delaware", "Delaware"));
        $("#inputClientState").append(
            new Option("District of Columbia", "District of Columbia")
        );
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
        $("#inputClientState").append(
            new Option("Marshall Islands", "Marshall Islands")
        );
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
        $("#inputClientState").append(
            new Option("North Carolina", "North Carolina")
        );
        $("#inputClientState").append(new Option("North Dakota", "North Dakota"));
        $("#inputClientState").append(
            new Option("Northern Mariana Islands", "Northern Mariana Islands")
        );
        $("#inputClientState").append(new Option("Ohio", "Ohio"));
        $("#inputClientState").append(new Option("Oklahoma", "Oklahoma"));
        $("#inputClientState").append(new Option("Oregon", "Oregon"));
        $("#inputClientState").append(new Option("Palau", "Palau"));
        $("#inputClientState").append(new Option("Pennsylvania", "Pennsylvania"));
        $("#inputClientState").append(new Option("Puerto Rico", "Puerto Rico"));
        $("#inputClientState").append(new Option("Rhode Island", "Rhode Island"));
        $("#inputClientState").append(
            new Option("South Carolina", "South Carolina")
        );
        $("#inputClientState").append(new Option("South Dakota", "South Dakota"));
        $("#inputClientState").append(new Option("Tennessee", "Tennessee"));
        $("#inputClientState").append(new Option("Texas", "Texas"));
        $("#inputClientState").append(new Option("Utah", "Utah"));
        $("#inputClientState").append(new Option("Vermont", "Vermont"));
        $("#inputClientState").append(
            new Option("Virgin Islands", "Virgin Islands")
        );
        $("#inputClientState").append(new Option("Virginia", "Virginia"));
        $("#inputClientState").append(new Option("Washington", "Washington"));
        $("#inputClientState").append(new Option("West Virginia", "West Virginia"));
        $("#inputClientState").append(new Option("Wisconsin", "Wisconsin"));
        $("#inputClientState").append(new Option("Wyoming", "Wyoming"));
    };

    var validateEditarContacto = function () {
        if ($("#inputContactName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($("#inputContactLastName").val() == "") {
            showWarningMessage(
                "Atención",
                "El campo Apellidos no puede estar vacío."
            );
            return false;
        } else if (
            $("#inputContactPhoneMovil").val() == "" &&
            $("#inputContactPhoneHome").val() == ""
        ) {
            showWarningMessage(
                "Atención",
                "Debe introducir al menos un teléfono de contacto."
            );
            return false;
        } else if ($("#contactCI").val().length > 0) {
            if ($("#contactCI").val().length != 11) {
                showWarningMessage(
                    "Atención",
                    "El carnet de identidad debe tener 11 dígitos"
                );
                return false;
            }
        } else if ($("#contactDireccion").val() == "") {
            showWarningMessage("Atención", "El campo Dirección no puede estar vacío");
            return false;
        } else if ($("#provincia").val() == "") {
            showWarningMessage(
                "Atención",
                "El campo Provincia no puede estar vacío."
            );
            return false;
        } else if ($("#municipio").val() == "") {
            showWarningMessage(
                "Atención",
                "El campo Municipio no puede estar vacío."
            );
            return false;
        }
        return true;
    };

    var desactContactForm = function () {
        $("#nuevoCliente").removeAttr("disabled");
        $(".select2-placeholder-selectClient").removeAttr("disabled");
        $("#nuevoContacto").removeAttr("disabled");
        $(".select2-placeholder-selectContact").removeAttr("disabled");
        $("#showAllContacts").removeAttr("disabled");
        $("#editarCliente").removeAttr("disabled");

        $("#inputContactName").attr("disabled", "disabled");
        $("#inputContactLastName").attr("disabled", "disabled");
        $("#inputContactPhoneMovil").attr("disabled", "disabled");
        $("#inputContactPhoneHome").attr("disabled", "disabled");
        $("#contactDireccion").attr("disabled", "disabled");
        $("#provincia").attr("disabled", "disabled");
        $("#municipio").attr("disabled", "disabled");
        $("#reparto").attr("disabled", "disabled");
        $("#contactCI").attr("disabled", "disabled");

        $("#editarContacto").removeClass("hidden");
        $("#cancelarContacto").addClass("hidden");
        $("#guardarContacto").addClass("hidden");

        $("a[href='#next']").removeClass("hidden");
        $("#cancel").show();

        $("#showAllContacts").attr("disabled", "disabled");
    };

    var cancelarContactForm = function () {
        $("#inputContactName").val($("#inputContactName").data("prevVal"));
        $("#inputContactLastName").val($("#inputContactLastName").data("prevVal"));
        $("#inputContactPhoneMovil").val(
            $("#inputContactPhoneMovil").data("prevVal")
        );
        $("#inputContactPhoneHome").val(
            $("#inputContactPhoneHome").data("prevVal")
        );
        $("#contactDireccion").val($("#contactDireccion").data("prevVal"));
        $("#provincia").val($("#provincia").data("prevVal")).trigger("change");
        $("#reparto").val($("#reparto").data("prevVal"));
        $("#municipio").val($("#municipio").data("prevVal"));
        $("#contactCI").val($("#contactCI").data("prevVal"));

        desactContactForm();
    };

    $(".select2-placeholder-selectClient").on("select2:select", function (a, b) {
        $(".select2-placeholder-selectContact").removeAttr("disabled");
        $("#nuevoContacto").removeAttr("disabled");
        $("#showAllContacts").removeAttr("disabled");
        $("#editarCliente").removeClass("hidden");

        $("a[href='#next']").addClass("hidden");
        $("#cancel").hide();

        showClient();
        showContactsOfAClient();
    });

    $(".select2-placeholder-selectContact").on("select2:select", function () {
        $("#editarContacto").removeClass("hidden hide-search-contactCity");
        showContact();
        //getprecioxlibra();
    });

    $("#showAllContacts").on("click", function (e) {
        e.preventDefault();
        //showAllContact();
    });

    //se activan los campos desactivados y se muestran los botones guardar y cancelar
    $("#editarContacto").on("click", function () {
        // para que no pueda crear nuevo cliente mientras edita contacto
        $("#nuevoCliente").attr("disabled", "disabled");

        // para que no pueda cambiar de cliente mientras edita contacto
        $(".select2-placeholder-selectClient").attr("disabled", "disabled");

        // para que no pueda crear nuevo contacto mientras edita contacto
        $("#nuevoContacto").attr("disabled", "disabled");

        // para que no pueda cambiar de contacto mientras edita contacto
        $(".select2-placeholder-selectContact").attr("disabled", "disabled");

        // para que no pueda cambiar de contacto mientras edita contacto
        $("#showAllContacts").attr("disabled", "disabled");

        // para que no pueda editar cliente mientras edita contacto
        $("#editarCliente").attr("disabled", "disabled");

        // para que no pueda avanzar a la otra parte del formulario
        $("a[href='#next']").addClass("hidden");
        $("#cancel").hide();

        $("#inputContactName")
            .removeAttr("disabled")
            .data("prevVal", $("#inputContactName").val());
        $("#inputContactLastName")
            .removeAttr("disabled")
            .data("prevVal", $("#inputContactLastName").val());
        $("#inputContactPhoneMovil")
            .removeAttr("disabled")
            .data("prevVal", $("#inputContactPhoneMovil").val());
        $("#inputContactPhoneHome")
            .removeAttr("disabled")
            .data("prevVal", $("#inputContactPhoneHome").val());
        $("#contactDireccion")
            .removeAttr("disabled")
            .data("prevVal", $("#contactDireccion").val());
        $("#provincia")
            .removeAttr("disabled")
            .data("prevVal", $("#provincia").val());
        $("#municipio")
            .removeAttr("disabled")
            .data("prevVal", $("#municipio").val());
        $("#reparto").removeAttr("disabled").data("prevVal", $("#reparto").val());
        $("#contactCI")
            .removeAttr("disabled")
            .data("prevVal", $("#contactCI").val());

        $("#editarContacto").addClass("hidden");
        $("#cancelarContacto").removeClass("hidden");
        $("#guardarContacto").removeClass("hidden");
    });

    $("#cancelarContacto").click(cancelarContactForm);

    $("#guardarContacto").on("click", function () {
        selectedContact = $(".select2-placeholder-selectContact").val();
        if (validateEditarContacto()) {
            var source = [
                $(".select2-placeholder-selectContact").val(),
                $("#inputContactName").val(),
                $("#inputContactLastName").val(),
                $("#inputContactPhoneMovil").val(),
                $("#inputContactPhoneHome").val(),
                $("#contactDireccion").val(),
                $("#provincia").val(),
                $("#municipio").val(),
                $("#reparto").val(),
                $("#contactCI").val(),
                $(".select2-placeholder-selectClient").val(),
            ];

            $.ajax({
                type: "POST",
                url: "/Contacts/EditContact",
                data: JSON.stringify(source),
                dataType: "json",
                contentType: "application/json",
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
                },
            });

            desactContactForm();
        }
    });

    $("#nuevoContacto").attr("disabled", "disabled");

    //showAllClients();

    mostrarEstados();

    /***********DATOS DE LA ORDEN *************/

    var tipo_orden_sufij = "EM";

    var setNoOrden = function () {
        var time = $("#time").html();
        $("#no_orden").html(tipo_orden_sufij + time);
    };

    var addProductToTable = function (cant, descripcion, peso, precio = "0") {
        var isProductBodega = false;

        //Get the reference of the Table's TBODY element.
        var tBody = $("#tblProductos > TBODY")[0];

        var index = $(tBody).children().length - 1;

        ////Add Row.
        var priceRef = 0;
        var row = tBody.insertRow(index);
        $(row).attr("data-id", $("#newProductId").val());

        if ($("#newProductId").val() != "" && $("#newProductId").val() != null) {
            $(row).attr("style", "background-color: #ffc8004a;");
            isProductBodega = true;
            priceRef = parseFloat(precio) * parseInt(cant);
        } else {
            if (descripcion.toUpperCase() != "MISCELANEAS" && enableDuadero) {
                priceRef = peso * parseFloat($("#PriceLbDuradero").val());
            }
            else {
                priceRef = peso * parseFloat($("#precioxlibra").val());
            }
        }

        //No mostrar el precio para las que no sean miscelaneas
        if (descripcion.toUpperCase() != "MISCELANEAS" && !isProductBodega) {
            priceRef = 0;
        }

        var cell1 = $(row.insertCell(-1));
        cell1.append(cant);

        var cell5 = $(row.insertCell(-1));
        cell5.append(descripcion);

        var cell7 = $(row.insertCell(-1));
        cell7.append(peso);

        var cell3 = $(row.insertCell(-1));
        cell3.append(priceRef.toFixed(2));

        if (descripcion.toUpperCase() == "MISCELANEAS") {
            pesoProductos += parseFloat(peso);
        }
        else {
            pesoProductosDuradero += parseFloat(peso);
        }

        const p = valueMiscelaneas * parseFloat($("#CantPqt").val());

        if (enableDuadero) {
            $("#CantLb")
                .val((pesoProductos + p).toFixed(2))
                .trigger("change");

            $("#CantLbDuradero")
                .val((pesoProductosDuradero).toFixed(2))
                .trigger("change");
        }
        else {
            $("#CantLb")
                .val((pesoProductos + pesoProductosDuradero + p).toFixed(2))
                .trigger("change");

            $("#CantLbDuradero")
                .val(0)
                .trigger("change");
        }
        

        ////Add Button cell.
        var cell6 = $(row.insertCell(-1));
        var btnEdit = $(
            "<button type='button' class='btn btn-warning' title='Editar' style='font-size: 10px'><i class='fa fa-pencil'></i></button>"
        );
        var btnRemove = $(
            "<button type='button' class='btn btn-danger pull-right' title='Eliminar' style='font-size: 10px'><i class=' fa fa-close'></button>"
        );
        var btnConfirm = $(
            "<button type='button' class='btn btn-success hidden' title='Confirmar' style='font-size: 10px'><i class='fa fa-check'></button>"
        );
        btnEdit.on("click", function () {
            cell1.html(
                "<input type='number' name='cell1' class='form-control' data-preVal=" +
                cell1.html() +
                " value='" +
                cell1.html() +
                "'/>"
            );

            if (!isProductBodega) {
                cell5.html(
                    "<input name='cell5' class='form-control' value='" +
                    cell5.html() +
                    "'/>"
                );
                cell7.html(
                    "<input type='number' name='cell7' data-preVal=" +
                    cell7.html() +
                    " step='0.1' min='0' class='form-control' value='" +
                    cell7.html() +
                    "'/>"
                );
            }
            btnConfirm.removeClass("hidden");
            btnEdit.addClass("hidden");
            btnRemove.addClass("hidden");
        });
        btnRemove.on("click", function () {
            var id = $(row).attr("data-id");
            if (id != null && id != "") {
                var product = productsSelected.find((x) => x.idProducto == id);
                const index = productsSelected.indexOf(product);
                if (index > -1) {
                    productsSelected.splice(index, 1);
                }
            }
            const p = valueMiscelaneas * parseFloat($("#CantPqt").val());
            const description = cell5.html();
            if (description.toUpperCase() === "MISCELANEAS") {
                pesoProductos -= parseFloat(cell7.html())
            }
            else {
                pesoProductosDuradero -= parseFloat(cell7.html());
            }

            if (enableDuadero) {
                $("#CantLb")
                    .val((pesoProductos + p).toFixed(2))
                    .trigger("change");

                $("#CantLbDuradero")
                    .val((pesoProductosDuradero).toFixed(2))
                    .trigger("change");
            }
            else {
                $("#CantLb")
                    .val((pesoProductos + pesoProductosDuradero + p).toFixed(2))
                    .trigger("change");

                $("#CantLbDuradero")
                    .val(0)
                    .trigger("change");
            }
            row.remove();

            calcMoney();
        });
        btnConfirm.on("click", function () {
            if (validateEditProduct()) {
                var prevCant = parseFloat($("[name='cell1']").data("preval"));
                var description = $("[name='cell5']").val();
                var cant = $("[name='cell1']").val();
                cell1.html($("[name='cell1']").val());
                if (!isProductBodega) {
                    cell5.html($("[name='cell5']").val());
                    if (cell7) {
                        var p = parseFloat($("[name='cell7']").data("preval"));
                        if (description.toUpperCase() == "MISCELANEAS") {
                            pesoProductos -= p;
                            //var pesoMisc = parseInt(cant) * valueMiscelaneas;
                            pesoProductos += parseFloat($("[name='cell7']").val());;
                            //$("[name='cell7']").val(pesoMisc.toFixed(2));
                        } else {
                            pesoProductosDuradero -= p;
                            pesoProductosDuradero += parseFloat($("[name='cell7']").val());
                        }

                        p = valueMiscelaneas * parseFloat($("#CantPqt").val());
                        if (enableDuadero) {
                            $("#CantLb")
                                .val((pesoProductos + p).toFixed(2))
                                .trigger("change");
                            $("#CantLbDuradero")
                                .val((pesoProductosDuradero).toFixed(2))
                                .trigger("change");
                        }
                        else {
                            $("#CantLb")
                                .val((pesoProductos + pesoProductosDuradero + p).toFixed(2))
                                .trigger("change");
                        }

                        cell7.html($("[name='cell7']").val());
                    }
                } else {
                    $(cell3).html(parseFloat(precio) * parseInt($(cell1).html()));
                }

                btnConfirm.addClass("hidden");
                btnEdit.removeClass("hidden");
                btnRemove.removeClass("hidden");

                calcMoney();
            }
        });
        cell6.append(btnEdit.add(btnRemove).add(btnConfirm));

        $("#newDescrip").focus();
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
    };

    var sendOrder = function () {
        if (validateCreateOrder()) {
            //tomando los productos
            var listProduct = new Array();

            var tBody = $("#tblProductos > TBODY")[0];

            for (var i = 0; i < tBody.rows.length - 1; i++) {
                var fila = tBody.rows[i];
                listProduct[i] = new Array();
                listProduct[i][0] = $(fila.children[0]).html();
                listProduct[i][1] = $(fila.children[1]).html();
                listProduct[i][2] = $(fila).attr("data-id");
                listProduct[i][3] = $(fila.children[2]).html();
            }

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
                    $("#pagoWeb").val().replace(",", "."),
                ];
                notas = [
                    "",
                    $("#NotaZelle").val(),
                    $("#NotaCheque").val(),
                    "",
                    $("#NotaTransf").val(),
                    "",
                ];
                tipopagos = [
                    "Cash",
                    "Zelle",
                    "Cheque",
                    "Crédito o Débito",
                    "Transferencia Bancaria",
                    "Web",
                ];
            } else {
                valorespagado = [$("#ValorPagado").val().replace(",", ".")];
                tipopagos = [
                    $(
                        '.hide-search-pago > option[value="' +
                        $(".hide-search-pago").val() +
                        '"]'
                    ).html(),
                ];

                notas = [$("#NotaPago").val()];
            }

            if (!$("#mayorista").val()) {
                showErrorMessage("Error", "El campo mayorista es obligatorio");
                return;
            }

            const precioLb = parseFloat($("#precioxlibra").val());
            const precioLbDuradero = parseFloat($("#PriceLbDuradero").val());
            const pesoLb = parseFloat($("#CantLb").val());
            const pesoLbDuradero = parseFloat($("#CantLbDuradero").val());

            var datosOrden = [
                $("#mayorista").val(), //0
                $("#no_orden").html(), //1
                $(".select2-placeholder-selectClient").val(), //2
                $(".select2-placeholder-selectContact").val(), //3
                tipopagos, //4
                listProduct, //5
                $("#txtNameVA").val(), //6
                pesoLb, //7
                precioLb, //8
                $("#OtrosCostos").val().replace(",", "."), //9
                valorespagado, //10
                $("#valAduanalValue").html().replace(",", "."), //11
                $("#precioTotalValue").html().replace(",", "."), //12
                $("#balanceValue").html().replace(",", "."), //13
                $("#orderNote").val(), //14
                $("#AuthTypeCard").val(), //15
                $("#AuthCardCreditEnding").val(), //16
                $("#AuthExpDate").val(), //17
                $("#AuthCCV").val(), //18
                $("#AuthaddressOfSend").val(), //19
                $("#AuthOwnerAddressDiferent").val(), //20
                $("#Authemail").val(), //21
                $("#Authphone").val(), //22
                $("#AuthSaleAmount").val().replace(",", "."), //23
                $("#AuthConvCharge").val().replace(",", "."), //24
                $("#TotalCharge").val().replace(",", "."), //25
                $('[name="cargoAdicional"]').val().replace(",", "."), //26
                $('[name="referCargoAdicional"]').val(), //27
                notas, //28
                $("#Referencia").val(), //29
                $("#CantPqt").val(), //30
                addprecio, //31
                addcosto, //32
                enableDuadero, //33
                precioLbDuradero, //34
                pesoLbDuradero, //35
                cantidadProductosByPackage, //36
                $("#check_credito").is(":checked") ? $("#credito").html().replace(",", ".") : 0, // 37
                $('#TipoEnvio').val(), // 38
                $('#Transitaria').val() // 39

            ];

            $.ajax({
                type: "POST",
                url: "/EnvioMaritimo/Create",
                data: JSON.stringify(datosOrden),
                dataType: "json",
                contentType: "application/json",
                async: true,
                beforeSend: function () {
                    $.blockUI({
                        message:
                            '<div class="ft-refresh-cw icon-spin font-medium-2"></div>',
                        timeout: 60000,
                        overlayCSS: {
                            backgroundColor: "#FFF",
                            opacity: 0.8,
                            cursor: "wait",
                        },
                        css: {
                            border: 0,
                            padding: 0,
                            backgroundColor: "transparent",
                        },
                    });
                },
                success: function (data) {
                    if (data.success) {
                        salvarImagesClient();
                        window.location =
                            "/EnvioMaritimo/Details/" +
                            data.orderId +
                            "?msg=success&orderNumber=" +
                            $("#no_orden").html();
                    } else {
                        toastr.error(data.msg);
                        $.unblockUI();
                    }
                },
            });
        }

        return true;
    };

    function salvarImagesClient() {
        var fileID = $("#IdImg")[0].files[0];
        var textoId = $("#TextoIDImg").val();

        var filePasaporte = $("#pasaporteImg")[0].files[0];
        var fechapasaporte = $("#FechaPasaporteImg").val();
        var clientId = $('[name="selectClient"]').val();
        var formData = new FormData();
        formData.append("fileId", fileID);
        formData.append("filePasaporte", filePasaporte);
        formData.append("textoId", textoId);
        formData.append("fechapasaporte", fechapasaporte);
        formData.append("clientId", clientId);

        $.ajax({
            url: "/EnvioMaritimo/SaveImage",
            type: "post",
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) { },
        });
    }

    var embolsar = function () {
        var listProduct = new Array();

        var tBody = $("#tblProductos > TBODY")[0];
        var tr = "";
        var codigoBolsa = "";
        var countFilas = tBody.rows.length - 1;
        for (var i = 0; i < countFilas; i++) {
            var fila = tBody.rows[0];
            listProduct[i] = new Array();

            //Creando codigo de bolsa
            var date = new Date(Date.now());
            codigoBolsa =
                "BL" +
                date.getFullYear() +
                "" +
                (date.getUTCMonth() + 1) +
                "" +
                date.getUTCDate() +
                "" +
                date.getMinutes() +
                "" +
                date.getUTCMilliseconds();
            tr +=
                "<tr><td>" +
                $(fila.children[0]).html() +
                "</td><td>" +
                $(fila.children[1]).html() +
                "</td></tr>";
            $(fila).remove();
        }

        if (tr != "") {
            var bolsaHtml =
                '<div class="col-md-6" style="margin-bottom: 15px">\n' +
                '                            <h4 class="form-section" style="border-bottom: 1px solid silver;border-top: 1px solid silver;">' +
                '                                <div class="btn btn-default btnRemoveBolsa" style="padding: 5px;" data-toggle="tooltip" data-placement="top" data-original-title="Eliminar bolsa"><i style="font-size: 12px" class="fa fa-remove"></i></div> ' +
                //'                                <div class="btn btn-default btnEditBolsa" style="padding: 5px;" data-toggle="tooltip" data-placement="top" data-original-title="Eliminar bolsa"><i style="font-size: 12px" class="fa fa-edit"></i></div> ' +
                "                                Bolsa: " +
                codigoBolsa +
                '                                <div class="btn btn-default btnDownUp" style="padding: 5px;float:right; margin-top: 8px;" data-toggle="tooltip" data-placement="top" data-original-title="Detalles"><i style="font-size: 12px" class="fa fa-angle-up"></i></div> ' +
                "                            </h4>\n" +
                '                            <div  style="display: none;"class="table-responsive">\n' +
                '                                <table class="table productsBag" data-codigobolsa="' +
                codigoBolsa +
                '">\n' +
                "                                    <thead>\n" +
                "                                    <tr>\n" +
                "                                        <th>Cantidad</th>\n" +
                "                                        <th>Descripción</th>\n" +
                "                                    </tr>\n" +
                "                                    </thead>\n" +
                "                                    <tbody>\n" +
                "                                    <tr>\n" +
                tr;
            "                                    </tr>\n" +
                "                                    </tbody>\n" +
                "                                </table>\n" +
                "                            </div>\n" +
                "                        </div>";
            ("                        </div>");

            $("#bolsasContainer").html($("#bolsasContainer").html() + bolsaHtml);
            $("#noBolsas").hide();
            functionClickRemoveBag();
            functionClickbtnDownUp();
        }
    };

    var functionClickRemoveBag = function () {
        $(".btnRemoveBolsa").click(function () {
            var $this = $(this);
            confirmationMsg(
                "Eliminar bolsa",
                "¿Desea eliminar la bolsa selecta?",
                function () {
                    var html = $($this.parent().parent().parent());
                    $($this.parent().parent()).remove();
                    if (html.find("div").length === 0) {
                        $("#noBolsas").show();
                    }
                }
            );
        });
    };

    var functionClickRemoveBag = function () {
        $(".btnRemoveBolsa").click(function () {
            var $this = $(this);
            confirmationMsg(
                "Eliminar bolsa",
                "¿Desea eliminar la bolsa selecta?",
                function () {
                    var html = $($this.parent().parent().parent());
                    $($this.parent().parent()).remove();
                    if (html.find("div").length === 0) {
                        $("#noBolsas").show();
                    }
                }
            );
        });
    };

    var functionClickbtnDownUp = function () {
        $(".btnDownUp").click(function () {
            var table = $($(this).parent().parent()).find(
                "div[class=table-responsive]"
            );
            if (table.attr("style") == "display: none;") {
                $(this).html(
                    '<i style="font-size: 12px" class="fa fa-angle-down"></i>'
                );
                table.show("fast");
            } else {
                $(this).html('<i style="font-size: 12px" class="fa fa-angle-up"></i>');
                table.hide("fast");
            }
        });
    };

    var validateCreateOrder = function () {
        const precioLb = parseFloat($("#precioxlibra").val());
        const precioLbDuradero = parseFloat($("#PriceLbDuradero").val());
        const pesoLb = parseFloat($("#CantLb").val());
        const pesoLbDuradero = parseFloat($("#CantLbDuradero").val());

        var tBody = $("#tblProductos > TBODY")[0];

        if (Number.isNaN(pesoLb)) {
            showWarningMessage("Atención", "El campo Peso(Lb) no puede estar vacío.");
            return false;
        } else if (Number.isNaN(precioLb)) {
            showWarningMessage("Atención", "El campo Precio no puede estar vacío.");
            return false;
        } else if (Number.isNaN(precioLbDuradero)) {
            showWarningMessage("Atención", "El campo Precio duradero no puede estar vacío.");
            return false;
        } else if (Number.isNaN(pesoLbDuradero)) {
            showWarningMessage("Atención", "El campo Peso(Lb) duradero no puede estar vacío.");
            return false;
        } else if ($("#OtrosCostos").val() == "") {
            showWarningMessage(
                "Atención",
                "El campo Otros Cargos no puede estar vacío."
            );
            return false;
        } else if ($("#ValorPagado").val() == "") {
            showWarningMessage(
                "Atención",
                "El campo Importe Pagado no puede estar vacío."
            );
            return false;
        } else if ($("#balanceValue").html().includes("-")) {
            showWarningMessage(
                "Atención",
                "El Importe Pagado no puede ser superior al Precio Total."
            );
            return false;
        } else if (precioLb + precioLbDuradero <= 0) {
            showWarningMessage("Atención", "El precio por libra del trámite debe ser mayor que 0");
            return false;
        } else if ((pesoLb + pesoLbDuradero <= 0) && productsSelected == 0) {
            showWarningMessage("Atención", "El peso del trámite debe ser mayor que 0");
            return false;
        } else if ($("#OtrosCostos").val() < 0) {
            showWarningMessage(
                "Atención",
                "El campo Otros Cargos debe ser mayor o igual que 0."
            );
            return false;
        } else if ($("#ValorPagado").val() < 0) {
            showWarningMessage(
                "Atención",
                "El campo Importe Pagado debe ser mayor o igual que 0."
            );
            return false;
        } else if (tBody.rows.length == 1) {
            showWarningMessage("Atención", "Debe adicionar al menos un producto.");
            return false;
        }

        return true;
    };

    //Mostrar y ocultar el credit card fee
    $('[name = "TipoPago"]').on("change", function () {
        var Id = $(this).val();
        tipoPago = $('option[value = "' + Id + '"]').html();
        if (tipoPago == "Crédito o Débito") {
            $("#contfee").show();
            calcMoney();
        } else {
            $("#contfee").hide();
            calcMoney();
        }
    });

    $(
        "#CantLb, #CantLbDuradero, #PriceLbDuradero, #PriceLb, #OtrosCostos, #precioxlibra,[name='cargoAdicional'],#check_credito,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb"
    )
        .on("keyup", calcMoney)
        .on("change", calcMoney);

    $("#ValorPagado")
        .on("keyup", calcMoneyPayment)
        .on("change", calcMoneyPayment);

    $("#feeAduanal").on("change", function () {
        var antVAValue = parseFloat($("#valAduanalValue").html());
        var vaValue = parseFloat(this.value);
        var newVAValue = vaValue;
        $("#valAduanalValue").html(newVAValue);
        calcMoney();
    })

    $("#txtNameVA").on("change", function () {
        var vas = $("#txtNameVA").val();
        $("#valAduanalValue").html("0");

        if (vas == null) {
            calcMoney();
        } else {
            for (var i = 0; i < vas.length; i++) {
                $.ajax({
                    type: "POST",
                    url: "/Orders/GetValueOfVAduanalId",
                    data: JSON.stringify(vas[i]),
                    dataType: "json",
                    contentType: "application/json",
                    async: false,
                    success: function (data) {
                        var antVAValue = parseFloat($("#valAduanalValue").html());
                        var vaValue = parseFloat(data);
                        var newVAValue = antVAValue + vaValue;
                        $("#valAduanalValue").html(newVAValue);
                        calcMoney();
                    },
                    failure: function (response) {
                        showErrorMessage("ERROR", response.responseText);
                    },
                    error: function (response) {
                        showErrorMessage("ERROR", response.responseText);
                    },
                });
            }
        }
    });

    $(".select2-placeholder-buscar").on("select2:select", function () {
        //Get the reference of the Table's TBODY element.
        var tBody = $("#tblProductos > tbody")[0];

        ////Add Row.
        var row = tBody.insertRow(-1);

        ////Add Cant cell.
        var cell = $(row.insertCell(-1));
        var celCant = $(
            "<input id='newCant' type='number' class='form-control' value='1' min='1'/>"
        );
        cell.append(celCant);

        cell = $(row.insertCell(-1));
        var celTipo = $("<input id='newTipo' class='form-control'/>");
        cell.append(celTipo);

        ////Add Remove cell.
        cell = $(row.insertCell(-1));
        var btnRemove = $("<input  />");
        btnRemove.attr("type", "button");
        btnRemove.attr("class", "btn btn-default");
        btnRemove.attr("onclick", "Remove");
        btnRemove.val("Eliminar");
        cell.append(btnRemove);
    });

    $(".select2-placeholder-selectProduct")
        .on("select2:select", async function () {
            var productId = $(this).val();
            var product = await GetProduct(productId);

            if (
                productsSelected.find((x) => x.idProducto == product.idProducto) == null
            ) {
                $("#newDescrip").val(product.nombre);
                $("#newProductId").val(product.idProducto);
                productsSelected.push(product);
                addProductToTable(
                    $("#newCant").val(),
                    $("#newDescrip").val(),
                    0,
                    product.precioVentaReferencial
                );

                $("#newCant").val("1");
                $("#newDescrip").val("");
                $("#newProductId").val("");

                calcMoney();
            } else {
                toastr.error("El producto ya ha sido añadido");
            }

            $(".select2-placeholder-selectProduct").val("").trigger("change");
        })
        .data("select2")
        .$container.find(".select2-selection")
        .css("background-color", "#ffc8004a");

    $("#btnAdd").on("click", function () {
        if (validateAddProduct()) {
            addProductToTable(
                $("#newCant").val(),
                $("#newDescrip").val(),
                $("#newPeso").val()
            );

            $("#newCant").val("1");
            $("#newDescrip").val("");
            $("#newPeso").val("0");
        }
    });

    $("#newPeso").on("keypress", function (a) {
        if (a.key == "Enter" && validateAddProduct()) {
            addProductToTable(
                $("#newCant").val(),
                $("#newDescrip").val(),
                $("#newPeso").val()
            );

            $("#newCant").val("1");
            $("#newDescrip").val("");
            $("#newPeso").val("0");
        }
    });

    $("#newDescrip").on("keypress", function (a) {
        if ($("#newPeso").length > 0 && a.key == "Enter") {
            if (precioslibras.length > 0) {
                var value = precioslibras.find(x => x.item1.toUpperCase() == $("#newDescrip").val().toUpperCase())
                if (value) {
                    var p = parseInt($("#newCant").val()) * parseFloat(value.item3);
                    addProductToTable(
                        $("#newCant").val(),
                        $("#newDescrip").val(),
                        p.toFixed(2)
                    );
                    $("#newCant").val("1");
                    $("#newDescrip").val("");
                }
                else {
                    $("#newPeso").focus();
                }
            }
            else if ($("#newDescrip").val().toUpperCase() === "MISCELANEAS") {
                var p = parseInt($("#newCant").val()) * valueMiscelaneas;
                addProductToTable(
                    $("#newCant").val(),
                    $("#newDescrip").val(),
                    p.toFixed(2)
                );
                $("#newCant").val("1");
                $("#newDescrip").val("");
            } else {
                $("#newPeso").focus();
            }
        } else if (a.key == "Enter" && validateAddProduct()) {
            addProductToTable($("#newCant").val(), $("#newDescrip").val(), null);

            $("#newCant").val("1");
            $("#newDescrip").val("");
        }
    });

    $('[name="utility"]').on("click", function () {
        var value = $(this).attr("data-value");
        $("#newDescrip").val(value);
        $("#newDescrip").focus();
        var e = jQuery.Event("keypress");
        e.which = 13; // # Some key code value
        e.key = "Enter";
        $("#newDescrip").trigger(e);
    });

    $("#newCant").on("keypress", function (e) {
        if (e.which == 13) {
            $("#newDescrip").focus();
        }
    });

    $("#crearBolsa").click(function () {
        embolsar();
    });

    setNoOrden();

    calcMoney();

    /**************AuthCard************************************/
    $('[name="TipoPago"]').on("change", function () {
        var id = $(this).val();
        var value = $('option[value = "' + id + '"]').html();
        if (value == "Crédito o Débito") {
            $("#AddAuthorization").show();
        } else {
            $("#AddAuthorization").hide();
        }
        if (
            value == "Zelle" ||
            value == "Cheque" ||
            value == "Transferencia Bancaria"
        ) {
            $("#contNotaPago").show();
        } else {
            $("#contNotaPago").hide();
        }
    });

    $("#ConfirmAuth").click(function () {
        var error = false;
        if ($('[name="AuthTypeCard"]').val() == "") {
            error = true;
        }

        if ($('[name="AuthCardCreditEnding"]').val() == "") {
            error = true;
        }

        if ($('[name="AuthExpDate"]').val() == "") {
            error = true;
        }

        if ($('[name="AuthCCV"]').val() == "") {
            error = true;
        }

        if ($('[name="AuthaddressOfSend"]').val() == "") {
            error = true;
        }

        if ($('[name="AuthOwnerAddressDiferent"]').val() == "") {
            error = true;
        }

        if ($('[name="Authemail"]').val() == "") {
            error = true;
        }

        if ($('[name="Authphone"]').val() == "") {
            error = true;
        }

        if ($('[name="AuthConvCharge"]').val() == "") {
            error = true;
        }

        if ($('[name="TotalCharge"]').val() == "") {
            error = true;
        }

        if ($('[name="AuthSaleAmount"]').val() == "") {
            error = true;
        }

        $("#AuthorizationCard").modal("hide");
        $("body").removeClass("modal-open");
        $(".modal-backdrop").remove();
        if (!error) {
            $("#IcoAuthorizationAdd").attr("class", "fa fa-check-circle");
            $("#AddAuthorization").attr("class", "btn mr-1 mb-1 btn-success");
        } else {
            $("#IcoAuthorizationAdd").attr("class", "fa fa-plus-circle");
            $("#AddAuthorization").attr("class", "btn mr-1 mb-1 btn-secondary");
        }
    });

    $(document).ready(function () {
        $(".select2-container--default").attr("style", "width: 100%;");
    });

    // Cuando se cree un nuevo cliente al recargarse se muestre
    if (selectedClient != "00000000-0000-0000-0000-000000000000") {
        $.ajax({
            type: "POST",
            url: "/Clients/GetClient",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(selectedClient),
            async: false,
            success: function (data) {
                var newOption = new Option(
                    data.movil + "-" + data.name + " " + data.lastName,
                    selectedClient,
                    false,
                    false
                );
                $("[name='selectClient']").append(newOption);
                $(".select2-placeholder-selectClient")
                    .val(selectedClient)
                    .trigger("change")
                    .trigger("select2:select");

                //Datos del Cliente en Step 1
                $("#inputClientName").val(data.name);
                $("#inputClientName2").val(data.name2);
                $("#inputClientLastName").val(data.lastName);
                $("#inputClientLastName2").val(data.lastName2);
                $("#inputClientMovil").val(data.movil);
                $("#inputClientEmail").val(data.email);
                $("#inputClientAddress").val(data.calle);
                $("#inputClientId").val(data.id);
                $("#inputClientCity").val(data.city);
                $("#inputClientZip").val(data.zip);
                $("#inputClientState").val(data.state).trigger("change");

                $("#remitente").html(data.name + " " + data.lastName);
                showContactsOfAClient();
                selectedClientData = data;
                $("#editarCliente").removeClass("hidden");
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
        });
    }

    async function InitSelectProduct() {
        var data = await $.get("/BodegaProducto/getProduct?category=Tienda");

        $('[name = "selectProduct"]').empty();
        $('[name = "selectProduct"]').append(
            new Option("Seleccione los productos", "", true)
        );
        if (data.length != 0) {
            productos = data;
            for (var i = 0; i < data.length; i++) {
                var x = new Option(data[i].productName, data[i].productId);
                x.title = data[i].description;
                $('[name = "selectProduct"]').append(x);
            }
        }
    }

    async function GetProduct(id) {
        return await $.post("/ProductoBodega/getProducto", {
            id: id,
            function(response) {
                return response;
            },
        });
    }

    $("#CantPqt").on("change", () => {
        const cant = parseFloat($("#CantPqt").val());
        const peso = valueMiscelaneas * cant;
        if (enableDuadero) {
            $("#CantLb")
                .val((pesoProductos + peso).toFixed(2))
                .trigger("change");
        }
        else {
            $("#CantLb")
                .val((pesoProductos + pesoProductosDuradero + peso).toFixed(2))
                .trigger("change");
        }
        
    });

    $("#inputBarCode").on("change", async function () {
        var code = $(this).val();
        let exist = true;
        exist = await barCodeSelectBodega(code);
        if (!exist) {
            exist = await barCodeLookup(code);
        }

        if (!exist) {
            toastr.info("El producto no existe");
        }
        else {
            toastr.info("El producto ha sido añadido");
        }

        $(this).val("");
        $(this).focus();
    });

    async function barCodeLookup(code) {
        var product = await $.get("/BarcodeLookup/FindByCode?code=" + code);
        if (!product.success) {
            console.log(product.msg);
            return false;
        }

        var tBody = $("#tblProductos > TBODY")[0];
        var wasAdded = false;
        for (var i = 0; i < tBody.rows.length - 1; i++) {
            var fila = tBody.rows[i];

            const qty = $(fila.children[0]).html();
            const description = $(fila.children[1]).html();

            if (description === product.data.title) {
                var newCant = parseFloat(qty);
                newCant += 1;
                $(fila.children[0]).html(newCant);
                wasAdded = true;
                break;
            }
        }

        if (!wasAdded) {
            addProductToTable(
                $("#newCant").val(),
                product.data.title,
                $("#newPeso").val()
            );
        }
        return true;
    }

    async function barCodeSelectBodega(code) {
        if (code != "" && code != null) {
            var productAux = null;
            productos.forEach(function (item) {
                if (item.barCode == code) {
                    productAux = item;
                }
            });
            if (productAux != null) {
                var product = await GetProduct(productAux.productId);

                if (
                    productsSelected.find((x) => x.idProducto == product.idProducto) ==
                    null
                ) {
                    $("#newDescrip").val(product.nombre);
                    $("#newProductId").val(product.idProducto);
                    productsSelected.push(product);
                    addProductToTable(
                        $("#newCant").val(),
                        $("#newDescrip").val(),
                        0,
                        product.precioVentaReferencial
                    );

                    $("#newCant").val("1");
                    $("#newDescrip").val("");
                    $("#newProductId").val("");
                } else {
                    var row = $('[data-id="' + productAux.productId + '"]');
                    var childrens = $(row).children();
                    var input = childrens[0];

                    $(input).html(parseInt($(input).html()) + 1);
                    var price =
                        product.precioVentaReferencial * parseFloat($(input).html());
                    $(childrens[3]).html(price);
                    calcMoney();
                }
                return true;
            }
        }

        return false;
    }

    function UpdatePriceProductsTable() {
        cantidadProductosByPackage = 0;
        var tBody = $("#tblProductos > TBODY")[0];
        var rows = $(tBody).children();
        const precioxLibra = parseFloat($("#precioxlibra").val());
        const precioxLibraDuradero = parseFloat($("#PriceLbDuradero").val());
        priceByPackage = 0;
        for (var i = 0; i < rows.length; i++) {
            var row = rows[i];
            if ($(row).attr('class') != 'newRow') {
                if ($(row).attr("data-id") == "" || $(row).attr("data-id") == null) {
                    var elementCant = $($(row).children("td")[0]).html();
                    var elementDescription = $($(row).children("td")[1]).html();
                    var elementPrice = $(row).children("td")[3];
                    var elementPeso = $(row).children("td")[2];
                    var price = 0;

                    if (precioslibras.length > 0) {
                        var value = precioslibras.find(x => x.item1.toUpperCase() == elementDescription.toUpperCase())
                        if (value) {
                            price = parseInt(elementCant) * parseFloat(value.item2);
                        }
                        else {
                            price = parseInt(elementCant) * precioxLibra;
                        }
                        priceByPackage += price;
                    }
                    else if (elementDescription.toUpperCase() != "MISCELANEAS" && enableDuadero) {
                        price = precioxLibraDuradero * parseFloat($(elementPeso).html());
                    }
                    else {
                        if (byPackage) {
                            price = parseInt(elementCant) * precioxLibra;
                            cantidadProductosByPackage += parseInt(elementCant);
                        }
                        else {
                            price = precioxLibra * parseFloat($(elementPeso).html());
                            if (elementDescription.toUpperCase() != "MISCELANEAS") {
                                price = 0;
                            }
                        }
                    }

                    $(elementPrice).html(price.toFixed(2));
                }
            }
        }
    }

    //Cuando se pulse el boton aceptar en el modal de añadir precio y costo para combos
    $("#btnprecioscosto").on("click", function () {
        addcosto = parseFloat($("#addcosto").val());
        addprecio = parseFloat($("#addprecio").val());
        calcMoney();
    });

    $("#EnablePriceLbDuradero").on('change', function (e) {
        enableDuadero = $(this).is(":checked");
        if (enableDuadero) {
            $(".divduradero").show()
            //$("#CantLbDuradero").prop("disabled", false);
            $("#PriceLbDuradero").prop("disabled", false);
        }
        else {
            $(".divduradero").hide()
           // $("#CantLbDuradero").prop("disabled", true);
            $("#PriceLbDuradero").prop("disabled", true);
        }

        const p = valueMiscelaneas * parseFloat($("#CantPqt").val());
        if ( enableDuadero) {
            $("#CantLb")
                .val((pesoProductos + p).toFixed(2));

            $("#CantLbDuradero")
                .val((pesoProductosDuradero).toFixed(2));
        }
        else {
            $("#CantLb")
                .val((pesoProductos + pesoProductosDuradero + p).toFixed(2));

            $("#CantLbDuradero")
                .val(0);
        }
        UpdatePriceProductsTable();
        calcMoney();
    })
});
