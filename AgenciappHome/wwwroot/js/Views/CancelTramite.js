$(document).ready(function () {
    //Script para cancelar los trámites
    var tipeTramite = null;
    var tramiteId = null;
    var order_type = null; //ejemplo maritimo y aereo en carga am
    $(document).on('click', '[name="cancel"]', async function () {
        tipeTramite = $(this).attr('data-type');
        tramiteId = $(this).attr('data-id');
        order_type = $(this).attr('data-order-type');
        var verify = await verifyFactura(tramiteId,tipeTramite);
        if(verify){
            $('#ModalCancel').modal('show');
        }
    });

    $('#btnCancelTramite').on('click', function () {
        var option = null;
        $('[name="checkCancelTramite"]').each(function () {
            if ($(this).prop('checked') == true) {
                option = $(this).val();
            }
        });

        var url = "";
        var value = null;
        var href = ""
        if (tipeTramite == "passport") {
            url = "/passport/index";
            value = 6;
            href = "/passport/index?msg=El trámite ha sido cancelado."
        }
        else if (tipeTramite == "ticket") {
            url = "/ticket/index";
            value = 3;
            href = "/ticket/index?msg=El trámite ha sido cancelado."
        }
        else if (tipeTramite == "order") {
            url = "/ordernew/index";
            value = 3;
            href = "/airshipping/index?msg=El trámite ha sido cancelado."
        }
        else if (tipeTramite == "combo") {
            url = "/ordernew/indexcombo";
            value = 3;
            href = "/ordernew/combos?msg=El trámite ha sido cancelado."
        }
        else if (tipeTramite == "rechargue") {
            url = "/rechargue/index";
            value = 3;
            href = "/rechargue/index?msg=El trámite ha sido cancelado."
        }
        else if (tipeTramite == "otherService") {
            url = "/servicios/index";
            value = 6;
            href = "/servicios/index?msg=El trámite ha sido cancelado."
        }
        else if (tipeTramite == "maritimo") {
            url = "/enviomaritimo/index";
            value = 4;
            href = "/enviomaritimo/index?msg=El trámite ha sido cancelado."
        }
        else if (tipeTramite == "envioCaribe") {
            url = "/envioscaribe/index";
            value = 5;
            href = "/envioscaribe/index?msg=El trámite ha sido cancelado."
        }
        else if (tipeTramite == "remesa") {
            url = "/remesas/index";
            value = 4;
            href = "/remesas/index?msg=El trámite ha sido cancelado."
        }
        else if (tipeTramite == "paqueteturistico") {
            url = "/paqueteTuristico/index";
            value = 0;
            href = "/paqueteTuristico/index?msg=El trámite ha sido cancelado."
        }
        else if (tipeTramite == "cargaam") {
            url = "/ordercubiq/index";
            value = 8;
            href = `/ordercubiq/index?type=${order_type}&msg=El trámite ha sido cancelado.`
        }
        $.ajax({
            async: true,
            url: url,
            type: "POST",
            data: {
                value: value,
                id: tramiteId,
                option: option
            },
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                if (tipeTramite == "combo") {
                    if (response.success == true)
                        location.href = href;
                    else
                        toastr.error(response.msg);
                }
                else if (tipeTramite = "cargaam") {
                    if (response.success == false) {
                        toastr.error(response.msg);
                    }
                    else {
                        location.href = href;
                    }
                }
                else {
                    location.href = href;
                }
                $.unblockUI();
            },
            error: function (response) {
                toastr.error("No se ha podido cancelar el trámite");
                $.unblockUI();
            }
        });
    });

    async function verifyFactura(tramiteId, type){
        $('[name="optionCredito"]').show();
        $('[name="optionDevolucion"]').show();
        $('[name="optionCancelar"]').show();
       var response = await $.get("/Cancellations/VerifyCancelOrder?id=" + tramiteId + "&type=" + type);
       if(response.success){
        if(response.options.length == 0){
            swal("No se puede cancelar!", "El trámite no puede ser cancelado", "info");
            return false;
           }
           else
           {
                if(!response.options.includes("credito")){
                    $('[name="optionCredito"]').hide();
                }
                if(!response.options.includes("devolucion")){
                    $('[name="optionDevolucion"]').hide();
                }
                if(!response.options.includes("cancelar")){
                    $('[name="optionCancelar"]').hide();
                }
                
                if(response.msg != "" && response.msg != null) {
                    toastr.info(response.msg);
                }
           }
           return true;
       }
       else{
        swal("No se puede cancelar!", response.msg, "info");
       }
       return false;
    }
})