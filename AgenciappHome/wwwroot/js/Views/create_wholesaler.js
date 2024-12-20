var typeMaritimo = "0";

$(document).ready(function () {
    // Al pulsar el check de transferida
    var check = false;
    $('#checkTransferencia').on('change', function () {
        check = $(this)[0].checked;
        var idcategory = $('[name="category"]').val();
        var category = $('option[value="' + idcategory + '"]').html();
        //deshabilito el campo costo si el check esta activado
        if (check) {
            $('[name = "CostoMayorista"]').prop("disabled", true);
            if (category == "Combos")
                $('#ContCombos').show();

            $('#costByProvincePaquete').hide();
            $('#costByProvinceCombo').hide();
            $('#costByProvinceCarga').hide();
        }
        else {
            $('[name = "CostoMayorista"]').prop("disabled", false);
            $('[name = "CostoMayorista"]').val("0");
            if (category == "Paquete Aereo") {
                $('#costByProvincePaquete').show();
                $('#costByProvinceCombo').show();
            }
            else if (category == "Combos") {
                $('#costByProvincePaquete').show();
                $('#costByProvinceCombo').show();
            }
            else if (category == "Carga AM") {
                $('#costByProvinceCarga').show();
            }
            else {
                $('#costByProvinceCombo').hide();
                $('#costByProvincePaquete').hide();
            }
        }
    });

    $('[name="category"]').on('change', changeView);

    function changeView() {
        var idcategory = $('[name="category"]').val();
        var category = $('option[value="' + idcategory + '"]').html();

        $('#div_tipo_envio').hide();
        $('#contRecarga').hide();
        $('#envioscaribe').hide();
        $('#pasaporte').hide();
        $('#ContCombos').hide();
        $('#contComodin').hide();
        $('#costByProvinceCombo').hide();
        $('#costByProvincePaquete').hide();
        $('#costByProvinceCarga').hide();
        $('#contPrecioVentaMedicina').hide();
        $('#contPrecioVenta2').hide();
        $('#divCheckTypeMaritimo').hide();
        $('#costo2mayorista').hide();
        $('#costo3mayorista').hide();
        $('#costo4mayorista').hide();
        $('#costo5mayorista').hide();
        $('#costo6mayorista').hide();
        $('#contPrecioVenta').hide();
        $('#contPrecioMayorista').hide();
        $('#contExchangeRate').hide();
        $('#precioVentaMedicina').hide();


        if (category == "Remesa") {
            $('#costo2mayorista').show();
            $('#costo3mayorista').show();
            $('#costo4mayorista').show();
            $('#costo5mayorista').show();
            $('#costo6mayorista').show();
            $('#labelcostomayorista').html("Precio Fijo Mayorista");
            $('#contPrecioVenta').show();
            $('#contPrecioMayorista').show();
            $('#contExchangeRate').show();
            $('#labelcosto2mayorista').html("Precio Porciento Mayorista");
        }
        else if (category == "Recarga") {
            $('#contRecarga').show();  
            $('#labelcostomayorista').html("Precio Mayorista");
        }
        else if (category == "Combos") {
            $('#labelcostomayorista').html("Precio Mayorista"); 
            if($('#checkTransferencia').is(':checked'))
                $('#ContCombos').show();
            $('#costByProvinceCombo').show();
        }
        else if (category == "Maritimo-Aereo") {
            $('#envioscaribe').show();
        }
        else if (category == "Pasaporte") {
            $('#pasaporte').show();
            $('#contComodin').show();
        }
        else if (category == "Paquete Aereo") {
            $('#labelcostomayorista').html("Precio Mayorista");
           
            $('#contComodin').show();

            if (check) {
                $('#costByProvincePaquete').hide();
            }
            else {
                $('#costByProvincePaquete').show();
            }
        }
        else if (category == "Maritimo"){
            $('#labelcostomayorista').html("Precio Mayorista");
            $('#contPrecioVenta').show();
            $('#contPrecioMayorista').show();
            $('#contPrecioVenta2').show();
            $('#labelPrecioVenta2').html("Precio Venta - Duradero");
            $('#costo2mayorista').show();
            $('#labelcosto2mayorista').html("Precio Mayorista - Duradero");
            $('#divCheckTypeMaritimo').show();
            $('#div_tipo_envio').show();
        }
        else if (category == "Carga AM") {
            $('#costByProvinceCarga').show();
        }
        else {    
            $('#labelcostomayorista').html("Precio Mayorista");
            $('#contPrecioVenta').show();
            $('#contPrecioMayorista').show();
        }
    }

    $('.TypeMaritimo').on('click', function () {
        typeMaritimo = $(this).attr('valor');

        changeTpeMaritimo();
    })

    function changeTpeMaritimo() {
        var idcategory = $('[name="category"]').val();
        var category = $('option[value="' + idcategory + '"]').html();
        if (category == "Maritimo" && typeMaritimo == "1") {
            $('#containerTypeMaritimo').show();
            $('#contPrecioMayorista').hide();
            $('#costo2mayorista').hide();
            $('#contPrecioVenta').hide();
            $('#contPrecioVenta2').hide();
        }
        else {
            $('#containerTypeMaritimo').hide();
            $('#contPrecioMayorista').show();
            $('#costo2mayorista').show();
            $('#contPrecioVenta').show();
            $('#contPrecioVenta2').show();
        }
    }

    $("#add_tag").on("click", () => {
        $(".tag_container").show();
        var container = $(`
            <div class="tag_div">
                <div class="col-md-4">
                    <input lang="en" class="form-control tag_description" />
                </div>
                <div class="col-md-3">
                    <input lang="en" type="number" value="0" step="0.000000001" class="form-control tag_price" />
                </div>
                <div class="col-md-3">
                    <input lang="en" type="number" value="0" step="0.000000001" class="form-control tag_cost" />
                </div>
                <div class="col-md-3">
                    <input lang="en" type="number" value="0" step="0.000000001" class="form-control tag_peso" />
                </div>
            </div>`)

        var btn_remove = $(`
                <div class="col-md-1">
                    <button type="button" class="btn btn-danger remove_tag"><i class="fa fa-trash"></i></button>
                </div>`)

        container.append(btn_remove);

        btn_remove.on("click", () => {
            container.remove();
        })

        $(".tag_container").append(container);
    })

    changeView();

    $("#form").on("submit", function () {
        var tags = [];
        var x = $(".tag_div")
        for (var i = 0; i < x.length; i++) {
            var element = x[i];
            var d = $(element).find(".tag_description").val()
            var c = $(element).find(".tag_cost").val()
            var p = $(element).find(".tag_price").val()
            var w = $(element).find(".tag_peso").val()
            tags.push({
                description: d,
                cost: c,
                price: p,
                peso: w
            })
        }
        $("#tags").val(JSON.stringify(tags));

        var idcategory = $('[name="category"]').val();
        var category = $('option[value="' + idcategory + '"]').html();

        if (category == "Carga AM") {
            $('#costByProvincePaquete').remove();
            $('#costByProvinceCombo').remove();
        }
        else if (category == "Combos") {
            $('#costByProvincePaquete').remove();
            $('#costByProvinceCarga').remove();
        }
        else if (category == "Paquete Aereo") {
            $('#costByProvinceCombo').remove();
            $('#costByProvinceCarga').remove();
        }    
    });

    $('#checkTransferencia').trigger('change');
});