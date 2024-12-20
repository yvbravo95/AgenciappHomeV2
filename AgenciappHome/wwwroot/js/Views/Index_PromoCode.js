
$(document).ready(function () {
    var code = $('[name="code"]'),
        date = $('[name="date"]'),
        service = $('[name="service"]'),
        value = $('[name="value"]'),
        type = $('[name="type"]');

    var codeEdit = $('[name="codeEdit"]'),
        dateEdit = $('[name="dateEdit"]'),
        serviceEdit = $('[name="serviceEdit"]'),
        valueEdit = $('[name="valueEdit"]'),
        typeEdit = $('[name="typeEdit"]');

    var idCodeEdit = null;

    $('#newCode').click(() => {
        $('#modalCreate').modal('show');
    })

    $('#btnCrear').click(() => {
        var d = date.val().split('-');
        const formCreate = {
            Code: code.val(),
            Value: value.val(),
            DateInit: d[0],
            DateEnd: d[1],
            OrderType: service.val(),
            PromoType: type.val()
        };
        if (formCreate.Code == null || formCreate.Code == "" || formCreate.Value <= 0) {
            toastr.warning("Los valores no son correcto.");
            return false;
        }
        $.ajax({
            async: true,
            type: "post",
            url: "/PromoCode/Create",
            data: formCreate,
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                if (response.success) {
                    location.reload();
                }
                else {
                    toastr.error(response.msg);
                    $.unblockUI();
                }
            },
            error: function (error) {
                toastr.error("No se ha podido crear el código");
                $.unblockUI();
            }
        })
    })

    $('[name="visible"]').on('change', function () {
        var id = $(this).attr('data-id');
        var checked = $(this).is(':checked');
        $.ajax({
            async: true,
            type: "post",
            url: "/PromoCode/ChangeStatus",
            data: {
                id: id,
                status: checked
            },
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                if (!response.success) {
                    toastr.error(response.msg);
                }
                $.unblockUI();
            },
            error: function (error) {
                toastr.error("No se ha podido cambiar el estado del código");
                $.unblockUI();
            }
        })

    })

    $('[name="edit"]').click(function(){
        idCodeEdit = $(this).attr('data-id');
        console.log($(this).attr('data-id'))
        $.ajax({
            async: true,
            type: "post",
            url: "/PromoCode/GetCode",
            data: {
                id: idCodeEdit,
            },
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                console.log(response.data);
                if (response.success) {
                    codeEdit.val(response.data.code);
                    valueEdit.val(response.data.value);
                    serviceEdit.val(response.data.service).trigger('change');
                    typeEdit.val(response.data.type).trigger('change');
                    var d = response.data.date.replace("a. m.", "AM").replace("a. m.", "AM").replace("p. m.", "PM").replace("p. m.", "PM");
                    dateEdit.val(d).trigger('change');
                    $('#modalEdit').modal('show');
                }
                else {
                    toastr.error(response.msg);
                }
                $.unblockUI();
            },
            error: function (error) {
                toastr.error("No se ha podido obtener la informacion del código");
                $.unblockUI();
            }
        })


    })

    $('#btnEdit').click(() => {
        var d = dateEdit.val().split('-');
        const formEdit = {
            Id: idCodeEdit,
            Code: codeEdit.val(),
            Value: valueEdit.val(),
            DateInit: d[0],
            DateEnd: d[1],
            OrderType: serviceEdit.val(),
            PromoType: typeEdit.val()
        };
        if (formEdit.Code == null || formEdit.Code == "" || formEdit.Value <= 0) {
            toastr.warning("Los valores no son correcto.");
            return false;
        }
        $.ajax({
            async: true,
            type: "post",
            url: "/PromoCode/Edit",
            data: formEdit,
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                if (response.success) {
                    location.reload();
                }
                else {
                    toastr.error(response.msg);
                    $.unblockUI();
                }
            },
            error: function (error) {
                toastr.error("No se ha podido editar el código");
                $.unblockUI();
            }
        })
    })


    table = $('#table').DataTable({
        "searching": true,
        "lengthChange": false,
        "order": [[0, "desc"]],
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

    $('#search').on('keyup change', function () {
        table.search($(this).val()).draw();

    })

    $('#table_filter').hide();

})

