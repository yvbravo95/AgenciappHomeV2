$(document).ready(function () {
    $("#wz").steps({
        headerTag: "h6",
        bodyTag: "fieldset",
        transitionEffect: "fade",
        titleTemplate: '<span class="step">#index#</span> #title#',
        labels: {
            previous: "Anterior",
            next: "Siguiente",
            finish: 'Comprar',
            cancel: 'Cancelar'
        },
        enableCancelButton: true,
        onStepChanging: function (event, currentIndex, newIndex) {
            // Allways allow previous action even if the current form is not valid!
            if (currentIndex > newIndex) {
                return true;
            }

            //----------------Step1
            if (newIndex == 1) {
                if ($("#ClientId").val() == "") {
                    showWarningMessage("Atención", "El campo Cliente es obligatorio.");
                    error = true;
                }
            }
            //----------------Step2
            var error = false
            if (newIndex == 2) {
                if ($("#DateOutAuto").val() == "") {
                    showWarningMessage("Atención", "El campo Fecha de recogida es obligatorio.");
                    error = true;
                }
                else if ($("#DateInAuto").val() == "") {
                    showWarningMessage("Atención", "El campo Fecha de Entrega es obligatorio.");
                    error = true;
                }
                else if (new Date($("#DateInAuto").val()).getTime() < new Date($("#DateOutAuto").val()).getTime()) {
                    showWarningMessage("Atención", "El campo Fecha de entrega debe ser menor que el de Fecha de recogida.");
                    error = true;
                }
                else if (Math.round((new Date($("#DateInAuto").val()).getTime() - new Date($("#DateOutAuto").val()).getTime()) / (1000 * 60 * 60 * 24)) + 1 < 3) {
                    showWarningMessage("Atención", "La reserva debe ser de más de 3 dias.");
                    error = true;
                }
                else if (Math.round((new Date($("#DateInAuto").val()).getTime() - new Date($("#DateOutAuto").val()).getTime()) / (1000 * 60 * 60 * 24)) + 1 > 45) {
                    showWarningMessage("Atención", "La reserva debe ser de menos de 45 dias.");
                    error = true;
                }
                else {
                    $.ajax({
                        url: "/Ticket/GetPrecioAuto",
                        method: "POST",
                        data: {
                            dateOut: $("#DateOutAuto").val(),
                            dateIn: $("#DateInAuto").val()
                        },
                        success: function (response) {
                            if (response.success) {
                                var temporadas = {
                                    "Alta": "Alta",
                                    "Extrema Alta": "ExtremaAlta",
                                    "Media Alta": "MediaAlta",
                                    "Media ALta II": "MediaAltaII"
                                };

                                $(".div_auto").each((i, e) => {
                                    if ($(e).data("temporada") != temporadas[response.temporada]) {
                                        $(e).remove();
                                    }
                                })

                                for (var i = 0; i < response.data.length; i++) {
                                    $("." + response.data[i]).each(function (j, v) {
                                        if (response.costos[i] == 0) {
                                            $(v).parent().parent().parent().remove();
                                        }
                                        else {
                                            $(v).html(`$${response.total[i]}`);
                                            $(v).parent().data("costo", response.costos[i]);
                                            $(v).parent().data("costo2", response.costos2[i]);
                                            $(v).parent().data("total", response.total[i]);
                                        }
                                    });
                                }
                                $("#tempInfo").html(response.temporada);
                                $("#daysInfo").html(response.days);

                                $(".rentadora_div").each((i, e) => {
                                    console.log($(e).html())
                                    if (!$(e).html()) {
                                        $(e).parent().remove();
                                    }
                                })
                            }
                            else
                                showErrorMessage("Atención", "Error al cargar el precio y el costo");
                        }
                    })
                }
            }
            //----------------Step3
            if (newIndex == 3) {
                if ($("input[name=ModeloAuto]:checked").length == 0 || $("input[name=ModeloAuto]:checked").is(":hidden")) {
                    showWarningMessage("Atención", "Debe seleccionar un Auto.");
                    error = true;
                }
            }

            //----------------Step4
            if (newIndex == 4) {

            }

            return !error;
        },
        onFinishing: function (event, currentIndex) {
            var error = false
            //----------------Step4
            if ($("#Charges").val() == "" || $("#Charges").val() < 0) {
                $("span[data-valmsg-for=Charges]").html("Este campo debe ser un número mayor o igual a 0")
                $("span[data-valmsg-for=Charges]").show()
                error = true
            }
            else {
                $("span[data-valmsg-for=Charges]").hide()
            }
            if ($("#Price").val() == "" || $("#Price").val() < 0) {
                $("span[data-valmsg-for=Price]").html("Este campo  debe ser un número mayor o igual a 0")
                $("span[data-valmsg-for=Price]").show()
                error = true
            }
            else {
                $("span[data-valmsg-for=Price]").hide()
            }
            if ($("#ValorPagado").val() == "" || $("#ValorPagado").val() < 0) {
                $("span[data-valmsg-for=Payment]").html("Este campo debe ser un número mayor o igual a 0")
                $("span[data-valmsg-for=Payment]").show()
                error = true
            }
            else {
                $("span[data-valmsg-for=Payment]").hide()
            }
            if ($("#Debit").val() < 0) {
                showWarningMessage("Atención", "El valor de la deuda no puede ser menor que 0.");
                error = true
            }
            else {
                $("span[data-valmsg-for=Debit]").hide();
            }

            if (!error)
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
            else {
                console.log(error);
            }
            return !error;
        },
        onFinished: function (event, currentIndex) {

        },
        onCanceled: function () {
            window.location = "/Ticket"
        }
    });
    $("[href='#cancel']").addClass('btn-danger');

    $('div[class="content clearfix"]').attr("style", "overflow:visible");

    $(".selectdos").select2({ 'width': '100%' });

    $('#Phone').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });
    $('#phoneCuba').mask('+00 00000000', {
        placeholder: "Teléfono en Cuba"
    });
    $('#Authphone').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });
    $('#inputClientMovil').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });

    $('#Zip').mask('00000', {
        placeholder: "Zip Code"
    });

    $('#Authphone').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });

    $('#inputClientMovil').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });

    $(".selectdos").select2({
        width: "100%",
    });

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

    $('.pickadate').pickadate();

    $("#inputClientFecha").pickadate({
        labelMonthNext: 'Next month',
        labelMonthPrev: 'Previous month',
        labelMonthSelect: 'Pick a Month',
        selectMonths: true,
        selectYears: 200,
        format: 'dd mmmm yyyy',
        formatSubmit: 'yyyy/dd/mm',
    });

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
    });

    var parseDate = function (value) {
        if (value == "0001-01-01") {
            return "";
        }
        var m = value.match(/^(\d{4})(\/|-)?(\d{1,2})(\/|-)?(\d{1,2})$/);
        if (m)
            value = ("00" + m[3]).slice(-2) + '/' + ("00" + m[5]).slice(-2) + '/' + m[1];

        return value;
    }

    function updateautos(categoria, transmision) {
        $.each($(".Auto"), function (i, v) {
            var dataT = $(v).data("transmision");
            var dataC = $(v).data("categoria");
            if ((dataT == transmision && dataC == categoria) || (!transmision && dataC == categoria) || (!categoria && dataT == transmision) || (!categoria && !transmision)) {
                $(v).parent().parent().show();
                $(v).show();
            }
            else if (dataT != transmision || dataC != categoria) {
                $(v).parent().parent().hide();
                $(v).hide();
            }
        });
    }

    $("#Transmision").on("change", function () {
        var transmision = $("#Transmision").val();
        var categoria = $("#Categoria").val();
        updateautos(categoria, transmision)
    });

    $("#Categoria").on("change", function () {
        var transmision = $("#Transmision").val();
        var categoria = $("#Categoria").val();
        updateautos(categoria, transmision)
    });

    updateautos($("#Categoria") .val(),$("#Transmision").val());

    $("#Charges,#Price,#Cost,#Discount,#Charges,#Price,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb").on("keyup", function () {
        calculate(false)
    });

    $("#Charges,#Price,#Cost,#Discount,#Charges,#Price,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb,#check_credito").on("change", function () {
        calculate(false)
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
    });

    $("#ValorPagado").keyup(function () {
        calculatePayment(true)
    })

    $("#ValorPagado").on('change', function () {
        calculatePayment(true)
    })

    $("#btnprecioscosto").on('click', function () {
        calculate();
    });

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

        if (tipoPago == "Crédito o Débito" && $("#Price").val() > 0) {
            $('#AddAuthorization').show();
            $('#contfee').show();
        }
        else {
            $('#AddAuthorization').hide();
            $('#contfee').hide();
        }
        calculate(false);
    });

    function calculate() {
        var cargos = parseFloat($("#Charges").val());
        var precio = parseFloat($("#Price").val());
        var descuento = parseFloat($("#Discount").val());
        var creditodebito = parseFloat($('#creditodebito').val()); //Para saber cuando hay pagos que fue lo que se incremento en credito al total
        var precioTotalValue = precio + cargos - descuento + creditodebito;
        var balanceValue = 0;
        if (blockRight == "True") {//Si hay registros de pago no se modifican los pagos
            var pagado = parseFloat($('#refPagado').val());
            balanceValue = precioTotalValue - pagado;
        }
        else {
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
                if (blockRight == "True") {
                    var max = precioTotalValue;
                    var pagado = parseFloat($("#ValorPagado").val());

                    if ($("#check_credito").is(":checked")) {
                        var pagoCredito = parseFloat($("#credito").html());
                        pagoCredito = pagoCredito > precioTotalValue ? precioTotalValue : pagoCredito;
                        balanceValue = precioTotalValue - pagado - pagoCredito;
                        max = max - pagoCredito;
                    }
                    else {
                        balanceValue = precioTotalValue - pagado;
                    }
                }
                else {
                    if ($("#check_credito").is(":checked")) {
                        if (precioTotalValue.toFixed(2) - parseFloat($("#credito").html()) > 0) {
                            $("#ValorPagado").attr('max', precioTotalValue.toFixed(2) - parseFloat($("#credito").html()));
                            $("#ValorPagado").val(precioTotalValue.toFixed(2) - parseFloat($("#credito").html()));
                        }
                        else {
                            $("#ValorPagado").attr('max', 0);
                            $("#ValorPagado").val(0);
                        }
                    }
                    else {
                        $("#ValorPagado").attr('max', precioTotalValue.toFixed(2));
                        $("#ValorPagado").val(precioTotalValue.toFixed(2));
                    }
                }

            }
        }


        $("#Total").val(precioTotalValue.toFixed(2));
        $("#Debit").val(balanceValue.toFixed(2));
        if ($("#Debit").val() == '-0.00')
            $("#Debit").val('0.00');
    }
    calculate();
    function calculatePayment() {
        var cargos = parseFloat($("#Charges").val());
        var precio = parseFloat($("#Price").val());
        var descuento = parseFloat($("#Discount").val());
        var pagado = parseFloat($("#ValorPagado").val());
        var creditodebito = parseFloat($('#creditodebito').val()); //Para saber cuando hay pagos que fue lo que se incremento en credito al total
        var precioTotalValue = precio + cargos - descuento + creditodebito;
        var balanceValue = 0;
        if (blockRight == "True") {//Si hay registros de pago no se modifican los pagos
            var pagado = parseFloat($('#refPagado').val());
            balanceValue = precioTotalValue - pagado;
        }
        else {
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

            if ($("#check_credito").is(":checked")) {
                var pagoCredito = parseFloat($("#credito").html());
                pagoCredito = pagoCredito > precioTotalValue ? precioTotalValue : pagoCredito;
                balanceValue = precioTotalValue - pagado - pagoCredito;
                max = max - pagoCredito;
            }
            else {
                balanceValue = precioTotalValue - pagado;
            }
        }


        $("#Total").val(precioTotalValue);
        $("#Debit").val(balanceValue.toFixed(2));
        if ($("#Debit").val() == '-0.00')
            $("#Debit").val('0.00');
        $("#ValorPagado").attr("max", max.toFixed(2));
    }


    $('#ConfirmAuth').click(function () {
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

    $('#ModeloAuto').on('change', function () {
        var rentadoraId = $(this).attr('data-rentadoraId');
        $('#RentadoraId').val(rentadoraId);
    })

    $("#MayoristaAuto").select2({ width: "100%" });

    //if (byTransferencia == "True") {
    //    $("#MayoristaAuto").val(wholesalerId).trigger('change');
    //}

    $(".Auto").on("change", function () {
        if ($(this).is(":checked")) {
            var tran = $(this).data("transmision");
            var mayo = $(this).data("mayorista");
            var cate = $(this).data("categoria");
            var rent = $(this).data("rentadoraid");
            var precio = parseFloat($($("." + cate + tran + rent)[0]).parent().data("total"));
            var costo = $($("." + cate + tran + rent)[0]).parent().data("costo");
            var costo2 = $($("." + cate + tran + rent)[0]).parent().data("costo2");
            $("#Cost").val(costo);
            $("#CostMayorista").val(costo2);
            $("#Price").val(precio);
            $("#CategoriaAuto").val(cate);
            $("#TransmisionAuto").val(tran);
            $("#catInfo").html($(`option[value="${cate}"]`).html());
            $("#traInfo").html($(`option[value="${tran}"]`).html());

            $("#RentadoraId").val(rent);
            if (byTransferencia != "True")
                $("#MayoristaAuto").val(mayo).trigger('change');
            calculate();
        }
    })
});