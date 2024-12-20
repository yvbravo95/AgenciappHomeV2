$(document).ready(function () {
    $("#AffiliateId").select2({
        placeholder: "Seleccione un minorista",
        val: null,
    });

    $("#Charges,#Price,#Cost,#Discount,#Charges,#Price,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb").keyup(function () {
        calculate(false)
    })
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

    $("#ConfirmPasajeros").on("click", function () {
        addPasajero($("#pasajero_name").val(), $("#pasajero_apellidos").val(), $("#pasajero_phone").val(), $("#pasajero_passno").val(), $("#pasajero_passexp").val(), $("#pasajero_passexp_i").val(), $("#pasajero_otroDocumento").val(), $("#pasajero_OtroDocumentoNo").val(), $("#pasajeros_OtroDocumentoExpDate").val(), $("#pasajeros_OtroDocumentoExpDate_i").val(), $("#pasajero_menor").is(":checked"), $("#pasajero_name_aut").val(), $("#pasajero_relation_aut").val(), $("#pasajero_name_lleva").val(), $("#pasajero_relation_lleva").val(), $("#pasajero_name_notario").val(), $("#pasajero_presoknow").val(), $("#pasajero_produ").val(), $("#pasajero_ident_type").val(), $("#pasajero_numb_ide").val());
        $("#row_pasajeros").removeClass('hidden');
    })

    $("#EditPasajeros").on("click", function () {
        var index = $(this).data('index');
        $("[name='Pasajeros[" + index + "].Name']").val($("#pasajero_name_e").val());
        $("[name='Pasajeros[" + index + "].LastName']").val($("#pasajero_apellidos_e").val());
        $("[name='Pasajeros[" + index + "].Phone']").val($("#pasajero_phone_e").val());
        $("[name='Pasajeros[" + index + "].PasspotNumber']").val($("#pasajero_passno_e").val());
        $("[name='Pasajeros[" + index + "].PasspotExpDate']").val($("#pasajero_passexp_e").val());
        $("[name='Pasajeros[" + index + "].PasspotExpDate_i']").val($("#pasajero_passexp_i_e").val());
        $("[name='Pasajeros[" + index + "].OtroDocumento']").val($("#pasajero_otroDocumento_e").val());
        $("[name='Pasajeros[" + index + "].OtroDocumentoNo']").val($("#pasajero_OtroDocumentoNo_e").val());
        $("[name='Pasajeros[" + index + "].OtroDocumentoExpDate']").val($("#pasajero_OtroDocumentoExpDate_e").val());
        $("[name='Pasajeros[" + index + "].OtroDocumentoExpDate_i']").val($("#pasajero_OtroDocumentoExpDate_i_e").val());

        $("[name='Pasajeros[" + index + "].IsMenor']").val($("#pasajero_menor_e").is(":checked"));
        $("[name='Pasajeros[" + index + "].AuthorizesName']").val($("#pasajero_name_aut_e").val());
        $("[name='Pasajeros[" + index + "].AuthorizesRelation']").val($("#pasajero_relation_aut_e").val());
        $("[name='Pasajeros[" + index + "].AuthorizedName']").val($("#pasajero_name_lleva_e").val());
        $("[name='Pasajeros[" + index + "].AuthorizedRelation']").val($("#pasajero_relation_lleva_e").val());
        $("[name='Pasajeros[" + index + "].NotaryName']").val($("#pasajero_name_notario_e").val());
        $("[name='Pasajeros[" + index + "].PersonallyKnown']").val($("#pasajero_presoknow_e").val());
        $("[name='Pasajeros[" + index + "].ProdIdentification']").val($("#pasajero_produ_e").val());
        $("[name='Pasajeros[" + index + "].TypeIdentification']").val($("#pasajero_ident_type_e").val());
        $("[name='Pasajeros[" + index + "].IdentificationNo']").val($("#pasajero_numb_ide_e").val());


        var row = document.getElementById('table_pasajeros').rows[index];
        $(row.cells[0]).html($("#pasajero_name_e").val());
        $(row.cells[1]).html($("#pasajero_apellidos_e").val());
        $(row.cells[2]).html($("#pasajero_phone_e").val());
        $(row.cells[3]).html($("#pasajero_passno_e").val());
        $(row.cells[4]).html($("#pasajero_passexp_i_e").val());

    })

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
        if (paqueteId != "00000000-0000-0000-0000-000000000000") {
            calculatePayment();
        }
        else {
        var affiliateId = $("#AffiliateId").val();
        var cargos = parseFloat($("#Charges").val());
        var precio = parseFloat($("#Price").val());
        var descuento = parseFloat($("#Discount").val());
        var precioTotalValue = precio + cargos - descuento;

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
                if (affiliateId)
                {
                    balanceValue = precioTotalValue;
                    $("#ValorPagado").val(0);
                    $("#ValorPagado").attr('max', precioTotalValue.toFixed(2));
                }
                else
                {
                    $("#ValorPagado").val(precioTotalValue.toFixed(2));
                    $("#ValorPagado").attr('max', precioTotalValue.toFixed(2));
                }
               
            }
        }

        $("#Total").val(precioTotalValue.toFixed(2));
        $("#Debit").val(balanceValue.toFixed(2));
        if ($("#Debit").val() == '-0.00')
            $("#Debit").val('0.00');
        }
    }

    function calculatePayment() {
        var cargos = parseFloat($("#Charges").val());
        var precio = parseFloat($("#Price").val());
        var descuento = parseFloat($("#Discount").val());
        var pagado = parseFloat($("#ValorPagado").val());

        var precioTotalValue = precio + cargos - descuento;
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

        $("#Total").val(precioTotalValue);
        $("#Debit").val(balanceValue.toFixed(2));
        if ($("#Debit").val() == '-0.00')
            $("#Debit").val('0.00');
        $("#ValorPagado").attr("max", max.toFixed(2));
    }

    function addPasajero(name, lastName, phone, passortNo, passportExpDate, passportExpDateI, otherDocument, othereDocNo, otherDocExpDate, otherDocExpDateI, isMenor, authorizesName, authorizesRelation, authorizedName, authorizedRelatio,notaryName, personallyKnown, prodIdentification, typeIdentification,identificationNumber) {
        //Get the reference of the Table's TBODY element.
        var tBody = $("#table_pasajeros > TBODY")[0];
        var length = $("#table_pasajeros tr").length;
        var row = tBody.insertRow(-1);
        var cell0 = $(row.insertCell(-1));
        cell0.append(name);
        var cell1 = $(row.insertCell(-1));
        cell1.append(lastName);
        var cell2 = $(row.insertCell(-1));
        cell2.append(phone);
        var cell3 = $(row.insertCell(-1));
        cell3.append(passortNo);
        var cell4 = $(row.insertCell(-1));
        cell4.append(passportExpDateI);
        var cell6 = $(row.insertCell(-1));
        var btnEdit = $("<button type='button' class='btn btn-warning' name='edit_" + length + "' data-length='" + length + "' title='Editar' style='font-size: 10px'><i class='fa fa-pencil'></i></button>");
        var btnRemove = $("<button type='button' class='btn btn-danger ml-1' title='Eliminar' style='font-size: 10px'><i class=' fa fa-close'></button>");
        var btnDetails = $("<button type='button' class='btn btn-success ml-1' name='details_" + length + "' data-length='" + length + "' title='Detalles' style='font-size: 10px'><i class='fa fa-eye'></button>");
        btnEdit.on("click", function () {
            var l = $(this).data('length');
            $("#EditPasajeros").data("index",l);
            $("#pasajero_name_e").val($("[name='Pasajeros[" + l + "].Name']").val());
            $("#pasajero_apellidos_e").val($("[name='Pasajeros[" + l + "].LastName']").val());
            $("#pasajero_phone_e").val($("[name='Pasajeros[" + l + "].Phone']").val());
            $("#pasajero_passno_e").val($("[name='Pasajeros[" + l + "].PasspotNumber']").val());
            $("#pasajero_passexp_i_e").val($("[name='Pasajeros[" + l + "].PasspotExpDate_i']").val());
            $("#pasajero_passexp_e").val($("[name='Pasajeros[" + l + "].PasspotExpDate']").val());
            $("#pasajero_otroDocumento_e").val($("[name='Pasajeros[" + l + "].OtroDocumento']").val()).trigger("change");
            $("#pasajero_OtroDocumentoExpDate_i_e").val($("[name='Pasajeros[" + l + "].OtroDocumentoExpDate_i']").val());
            $("#pasajero_OtroDocumentoExpDate_e").val($("[name='Pasajeros[" + l + "].OtroDocumentoExpDate']").val());
            $("#pasajero_OtroDocumentoNo_e").val($("[name='Pasajeros[" + l + "].OtroDocumentoNo']").val());

            if ($("[name='Pasajeros[" + l + "].IsMenor']").val() == "true") {
                $("#pasajero_name_aut_e").val($("[name='Pasajeros[" + l + "].AuthorizesName']").val());
                $("#pasajero_relation_aut_e").val($("[name='Pasajeros[" + l + "].AuthorizesRelation']").val());
                $("#pasajero_name_lleva_e").val($("[name='Pasajeros[" + l + "].AuthorizedName']").val());
                $("#pasajero_relation_lleva_e").val($("[name='Pasajeros[" + l + "].AuthorizedRelation']").val());
                $("#pasajero_name_notario_e").val($("[name='Pasajeros[" + l + "].NotaryName']").val());
                $("#pasajero_presoknow_e").val($("[name='Pasajeros[" + l + "].PersonallyKnown']").val());
                $("#pasajero_produ_e").val($("[name='Pasajeros[" + l + "].ProdIdentification']").val());
                $("#pasajero_ident_type_e").val($("[name='Pasajeros[" + l + "].TypeIdentification']").val());
                $("#pasajero_numb_ide_e").val($("[name='Pasajeros[" + l + "].IdentificationNo']").val());

                $("#pasajero_menor_e").prop("checked", true)
                $(".pasajeros_menores_e").removeClass("hidden")
            }
            else {
                $("#pasajero_menor_e").prop("checked", false)
                $(".pasajeros_menores_e").addClass("hidden")
            }

            $("#modal_pasajeros_edit").modal("show");
        });
        btnRemove.on("click", function () {
            row.remove();
            if ($("#table_pasajeros tr").length == 1) {
                $("#row_pasajeros").addClass("hidden")
            }
            else {
                var last = $("#table_pasajeros tr").length;
                $("[name='Pasajeros[" + last + "].Name']").attr("name", "Pasajeros[" + length + "].Name")
                $("[name='Pasajeros[" + last + "].LastName']").attr("name", "Pasajeros[" + length + "].LastName")
                $("[name='Pasajeros[" + last + "].Phone']").attr("name", "Pasajeros[" + length + "].Phone")
                $("[name='Pasajeros[" + last + "].PasspotNumber']").attr("name", "Pasajeros[" + length + "].PasspotNumber")
                $("[name='Pasajeros[" + last + "].PasspotExpDate']").attr("name", "Pasajeros[" + length + "].PasspotExpDate")
                $("[name='Pasajeros[" + last + "].PasspotExpDate_i']").attr("name", "Pasajeros[" + length + "].PasspotExpDate_i")
                $("[name='Pasajeros[" + last + "].OtroDocumento']").attr("name", "Pasajeros[" + length + "].OtroDocumento")
                $("[name='Pasajeros[" + last + "].OtroDocumentoNo']").attr("name", "Pasajeros[" + length + "].OtroDocumentoNo")
                $("[name='Pasajeros[" + last + "].OtroDocumentoExpDate']").attr("name", "Pasajeros[" + length + "].OtroDocumentoExpDate")
                $("[name='Pasajeros[" + last + "].OtroDocumentoExpDate_i']").attr("name", "Pasajeros[" + length + "].OtroDocumentoExpDate_i")
                $("[name='edit_" + last + "']").data("length",length);
                $("[name='edit_" + last + "']").attr('name', "name='edit_" + length + "'");
                $("[name='details_" + last + "']").data("length", length);
                $("[name='details_" + last + "']").attr('name', "name='details_" + length + "'");

                $("[name='Pasajeros[" + last + "].IsMenor']").attr("name", "Pasajeros[" + length + "].IsMenor")
                $("[name='Pasajeros[" + last + "].AuthorizesName']").attr("name", "Pasajeros[" + length + "].AuthorizesName")
                $("[name='Pasajeros[" + last + "].AuthorizesRelation']").attr("name", "Pasajeros[" + length + "].AuthorizesRelation")
                $("[name='Pasajeros[" + last + "].AuthorizedName']").attr("name", "Pasajeros[" + length + "].AuthorizedName")
                $("[name='Pasajeros[" + last + "].AuthorizedRelation']").attr("name", "Pasajeros[" + length + "].AuthorizedRelation")
                $("[name='Pasajeros[" + last + "].NotaryName']").attr("name", "Pasajeros[" + length + "].NotaryName")
                $("[name='Pasajeros[" + last + "].PersonallyKnown']").attr("name", "Pasajeros[" + length + "].PersonallyKnown")
                $("[name='Pasajeros[" + last + "].ProdIdentification']").attr("name", "Pasajeros[" + length + "].ProdIdentification")
                $("[name='Pasajeros[" + last + "].TypeIdentification']").attr("name", "Pasajeros[" + length + "].TypeIdentification")
                $("[name='Pasajeros[" + last + "].IdentificationNo']").attr("name", "Pasajeros[" + length + "].IdentificationNo")
            }
        });
        btnDetails.on("click", function () {
            var l = $(this).data('length');
            $("#pasajero_name_d").val($("[name='Pasajeros[" + l + "].Name']").val());
            $("#pasajero_apellidos_d").val($("[name='Pasajeros[" + l + "].LastName']").val());
            $("#pasajero_phone_d").val($("[name='Pasajeros[" + l + "].Phone']").val());
            $("#pasajero_passno_d").val($("[name='Pasajeros[" + l + "].PasspotNumber']").val());
            $("#pasajero_passexp_d").val($("[name='Pasajeros[" + l + "].PasspotExpDate_i']").val());
            $("#pasajero_otroDocumento_d").val($("[name='Pasajeros[" + l + "].OtroDocumento']").val());
            $("#pasajero_OtroDocumentoNo_d").val($("[name='Pasajeros[" + l + "].OtroDocumentoNo']").val());
            $("#pasajero_OtroDocumentoExpDate_d").val($("[name='Pasajeros[" + l + "].OtroDocumentoExpDate_i']").val());

            if ($("[name='Pasajeros[" + l + "].IsMenor']").val() == "true") {
                $("#pasajero_menor_div_d").removeClass("hidden")
                $(".pasajeros_menores_d").removeClass("hidden")
                $("#pasajero_name_aut_d").val($("[name='Pasajeros[" + l + "].AuthorizesName']").val());
                $("#pasajero_relation_aut_d").val($("[name='Pasajeros[" + l + "].AuthorizesRelation']").val());
                $("#pasajero_name_lleva_d").val($("[name='Pasajeros[" + l + "].AuthorizedName']").val());
                $("#pasajero_relation_lleva_d").val($("[name='Pasajeros[" + l + "].AuthorizedRelation']").val());
                $("#pasajero_name_notario_d").val($("[name='Pasajeros[" + l + "].NotaryName']").val());
                $("#pasajero_presoknow_d").val($("[name='Pasajeros[" + l + "].PersonallyKnown']").val());
                $("#pasajero_produ_d").val($("[name='Pasajeros[" + l + "].ProdIdentification']").val());
                $("#pasajero_ident_type_d").val($("[name='Pasajeros[" + l + "].TypeIdentification']").val());
                $("#pasajero_numb_ide_d").val($("[name='Pasajeros[" + l + "].IdentificationNo']").val());
            }
            else {
                $("#pasajero_menor_div_d").addClass("hidden")
                $(".pasajeros_menores_d").addClass("hidden")
            }
            
            $("#modal_pasajeros_details").modal("show");
        });
        cell6.append(btnEdit.add(btnDetails).add(btnRemove));
        cell6.append("<input name='Pasajeros[" + length + "].Name' type='hidden' value='" + name + "'>")
        cell6.append("<input name='Pasajeros[" + length + "].LastName' type='hidden' value='" + lastName + "'>")
        cell6.append("<input name='Pasajeros[" + length + "].Phone' type='hidden' value='" + phone + "'>")
        cell6.append("<input name='Pasajeros[" + length + "].PasspotNumber' type='hidden' value='" + passortNo + "'>")
        cell6.append("<input name='Pasajeros[" + length + "].PasspotExpDate' type='hidden' value='" + passportExpDate + "'>")
        cell6.append("<input name='Pasajeros[" + length + "].PasspotExpDate_i' type='hidden' value='" + passportExpDateI + "'>")
        cell6.append("<input name='Pasajeros[" + length + "].OtroDocumento' type='hidden' value='" + otherDocument + "'>")
        cell6.append("<input name='Pasajeros[" + length + "].OtroDocumentoNo' type='hidden' value='" + othereDocNo + "'>")
        cell6.append("<input name='Pasajeros[" + length + "].OtroDocumentoExpDate' type='hidden' value='" + otherDocExpDate + "'>")
        cell6.append("<input name='Pasajeros[" + length + "].OtroDocumentoExpDate_i' type='hidden' value='" + otherDocExpDateI + "'>")

        cell6.append("<input name='Pasajeros[" + length + "].IsMenor' type='hidden' value='" + isMenor + "'>")
        cell6.append("<input name='Pasajeros[" + length + "].AuthorizesName' type='hidden' value='" + authorizesName + "'>")
        cell6.append("<input name='Pasajeros[" + length + "].AuthorizesRelation' type='hidden' value='" + authorizesRelation + "'>")
        cell6.append("<input name='Pasajeros[" + length + "].AuthorizedName' type='hidden' value='" + authorizedName + "'>")
        cell6.append("<input name='Pasajeros[" + length + "].AuthorizedRelation' type='hidden' value='" + authorizedRelatio + "'>")
        cell6.append("<input name='Pasajeros[" + length + "].NotaryName' type='hidden' value='" + notaryName + "'>")
        cell6.append("<input name='Pasajeros[" + length + "].PersonallyKnown' type='hidden' value='" + personallyKnown + "'>")
        cell6.append("<input name='Pasajeros[" + length + "].ProdIdentification' type='hidden' value='" + prodIdentification + "'>")
        cell6.append("<input name='Pasajeros[" + length + "].TypeIdentification' type='hidden' value='" + typeIdentification + "'>")
        cell6.append("<input name='Pasajeros[" + length + "].IdentificationNo' type='hidden' value='" + identificationNumber + "'>")
    };

    var costoAislamineto = 0;
    var precioAislamineto = 0;
    $("#HotelId").on("change", (e) => {
        costoAislamineto = 0;
        precioAislamineto = 0;
        if ($(e.target).val()) {
            $.get("/Hotel/Get/" + $(e.target).val(), (result) => {
                costoAislamineto = parseFloat(result.costo);
                precioAislamineto = parseFloat(result.precio);
                $("#Price").val(precioAislamineto * $("#CantidadPersonas").val());
                $("#Cost").val(costoAislamineto * $("#CantidadPersonas").val());
                $("#MayoristaAislamiento").val(result.mayoristaId);
                $("#DescriptionAis").val(result.desc)
                calculate();
            });
        }        
    });

    $('#AffiliateId').on('change', calculate)

    $("#CantidadPersonas").on("change", () => {
        $("#Price").val(precioAislamineto * $("#CantidadPersonas").val());
        $("#Cost").val(costoAislamineto * $("#CantidadPersonas").val());
        calculate();
    });

    $("#pasajero_menor").on("click", (e) => {
        var checked = $(e.target).is(":checked");
        if (checked) {
            $(".pasajeros_menores").removeClass("hidden");
        }
        else {
            $(".pasajeros_menores").addClass("hidden");

        }
    })

    $("#pasajero_menor_e").on("click", (e) => {
        var checked = $(e.target).is(":checked");
        if (checked) {
            $(".pasajeros_menores_e").removeClass("hidden");
        }
        else {
            $(".pasajeros_menores_e").addClass("hidden");

        }
    })
})