
oTable = $('[name="table"]').DataTable({
    "searching": true,
    "lengthChange": false,
    //"order": [[0, "desc"]],
    "paging": false,
    language: {
        "decimal": "",
        "emptyTable": "No hay información",
        "info": "Mostrando _START_ a _END_ de _TOTAL_ Entradas",
        "infoEmpty": "Mostrando 0 a 0 de 0 Entradas",
        "infoFiltered": "(Filtrado de _MAX_ total entradas)",
        "infoPostFix": "",
        "thousands": ",",
        "lengthMenu": "Mostrar _MENU_ Entradas",
        "loadingRecords": "Cargando...",
        "processing": "Procesando...",
        "search": "Buscar:",
        "zeroRecords": "Sin resultados encontrados",
        "paginate": {
            "first": "Primero",
            "last": "Ultimo",
            "next": "Siguiente",
            "previous": "Anterior"
        }
    },
});

$('.itemSetting').on('click', async function () {
    $('[name="ProductId"]').val("");
    $('[name="AliasName"]').val("");
    $('[name="Price"]').val("");
    $('[name="Visibility"]').val("");
    $('#btnDeleteSetting').hide();
    $('#originalName').html("");
    $('#originalPrice').html("");
    $('#containerProvinces').html("");

    var productId = $(this).attr('data-id');
    var response = await $.get("/ProductoBodega/GetSettingMinoristaProduct?productId=" + productId);
    $('[name="ProductId"]').val(productId);
    if (response.success) {
        if (response.data.setting != null) {
            $('[name="AliasName"]').val(response.data.setting.aliasName);
            $('[name="Price"]').val(response.data.setting.price);
            $('[name="Visibility"]').prop("checked", response.data.setting.visibility);
            $('#btnDeleteSetting').show();

            response.data.setting.provinces.map(function (item) {
                var div = $('<div class="col-md-6" style="margin-bottom: 4px"></div>');
                var btnRemove = $('<button type="button" class="btn btn-danger btn-sm" data-id="' + item.id + '" name="deleteItemProvince"><i class="fa fa-trash"></i></button>');
                var element = $('<span><strong> ' + item.nombreProvincia + ': </strong> $' + item.price + '</span>');

                $(btnRemove).on('click', function () {
                    const id = $(this).attr('data-id');
                    var formData = {
                        id: id
                    }
                    const element = $(this);
                    $.post("/ProductoBodega/DeletePriceProvinceToProduct", data = formData
                        , function (response) {
                            if (response.success) {
                                $(element).parent().remove();
                                toastr.success("Se ha eliminado el elemento");
                            }
                            else {
                                toastr.error("No se ha podido eliminar el elemento");
                            }
                        });
                })

                $(div).append(btnRemove);
                $(div).append(element);
                $('#containerProvinces').append(div);
            })
        }
        else{
            if(response.data.product != null){
                $('[name="AliasName"]').val(response.data.product.name);
                $('[name="Price"]').val(response.data.product.price);
                $('[name="Visibility"]').prop("checked", true);
            }
        }
        if (response.data.product != null) {
            $('#originalName').html("(" + response.data.product.name + ")");
            $('#originalPrice').html("(" + response.data.product.price + ")");
            
        }
    }
    else {
        toastr.error(response.msg);
    }

    $('#modalSetting').modal('show');
});

$('#formSetting').on('submit', async function (e) {
    e.preventDefault();
    if (parseFloat($('[name="Price"]').val()) <= 0) {
        toastr.error("El precio debe ser mayor que 0");
    }
    else {
        var formData = {
            ProductId: $('[name="ProductId"]').val(),
            AliasName: $('[name="AliasName"]').val(),
            Price: $('[name="Price"]').val(),
            Visibility: $('[name="Visibility"]').is(':checked'),
        }
        $.post("/ProductoBodega/SettingMinoristaProduct", data = formData
            , function (response) {
                if (response.success) {
                    location.href = "/Report/ProductsEmpleado?msg=Se ha añadido la configuración para el producto";
                }
                else {
                    toastr.error(response.msg);
                }
            });
    }
    
    
})

$('#btnDeleteSetting').on('click', async function () {
    var response = await $.get("/ProductoBodega/DeleteSettingProduct?productId=" + $('[name="ProductId"]').val());
    if (response.success)
        location.href = "/Report/ProductsEmpleado?msg=" + response.msg;
    else
        toastr.error(response.msg);
})

$('#addProvince').on('click', function () {
    var provinceId = $('[name="selectAddProvince"]').val();
    var price = $('[name="inputPrice"]').val();
    var productId = $('[name="ProductId"]').val();

    var formData = {
        productId: productId,
        provinceId: provinceId,
        price: price
    }
    $.post("/ProductoBodega/AddPriceProvinceToProduct", data = formData
        , function (response) {
            if (response.success) {
                var div = $('<div class="col-md-6" style="margin-bottom: 4px"></div>');
                var btnRemove = $('<button type="button" class="btn btn-danger btn-sm" data-id="' + response.data.id + '" name="deleteItemProvince"><i class="fa fa-trash"></i></button>');
                var element = $('<span><strong> ' + response.data.nombreProvincia + ': </strong> $' + response.data.price + '</span>');

                $(btnRemove).on('click', function () {
                    const id = $(this).attr('data-id');
                    var formData = {
                        id: id
                    }
                    const element = $(this);
                    $.post("/ProductoBodega/DeletePriceProvinceToProduct", data = formData
                        , function (response) {
                            if (response.success) {
                                $(element).parent().remove();
                                toastr.success("Se ha eliminado el elemento");
                            }
                            else {
                                toastr.error("No se ha podido eliminar el elemento");
                            }
                        });
                })

                $(div).append(btnRemove);
                $(div).append(element);
                $('#containerProvinces').append(div);

                toastr.success("Se ha añadido el precio para la provincia" + response.data.nombreProvincia);
            }
            else {
                toastr.error(response.msg);
            }
        });
})