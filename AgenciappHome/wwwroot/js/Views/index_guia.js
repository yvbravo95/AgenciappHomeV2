$(document).ready(function () {
    $(".enviar").on("click", function () {
        $.ajax({
            method: "POST",
            url: "/Passport/EnviarGuia/" + $(this).data("guiaid"),
            success: function (data) {
                if (data.success)
                    window.location = "/Passport/IndexGuia";
                else
                    console.log(data.msg)
            }
        })
    });

    $(".recibir").on("click", function () {
        $.ajax({
            method: "POST",
            url: "/Passport/RecibirGuia/" + $(this).data("guiaid"),
            success: function (data) {
                if (data.success)
                    window.location = "/Passport/IndexGuia";
                else
                    console.log(data.msg)
            }
        })
    });

    $(".update").on("click", function (e) {
        let guiaId = $(e.target).data("guiaid");

        let okConfirm = () => {
            $.ajax({
                method: "POST",
                url: "/Passport/UpdateGuiaNumber/" + guiaId,
                success: function (data) {
                    if (data.success)
                        window.location = "/Passport/IndexGuia";
                    else
                        console.log(data.msg)
                }
            })
        }

        confirmationMsg("¿Está seguro que desea actualizar el numero de guía?", "", okConfirm);
    });

    $(".updateGM").on("click", function (e) {
        let guiaId = $(e.target).data("guiaid");

        let okConfirm = () => {
            $.ajax({
                method: "POST",
                url: "/Passport/UpdGuiaManifestNumber/" + guiaId,
                success: function (data) {
                    if (data.success)
                        window.location = "/Passport/IndexGuia";
                    else
                        console.log(data.msg)
                }
            })
        }

        confirmationMsg("¿Está seguro que desea actualizar el numero de guía y de manifiesto? Esta acción solo se debe realizar si el consulado lo solicita", "", okConfirm);
    });

    tableInit = $('#tableInit').DataTable({
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
    tableEnvi = $('#tableEnvi').DataTable({
        "searching": true,
        "lengthChange": false,
        //"order": [[6, "desc"]],
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
    tableRcib = $('#tableRcib').DataTable({
        "searching": true,
        "lengthChange": false,
        //"order": [[6, "desc"]],
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

    $('#search').on('keyup', function () {
        tableInit.search($(this).val()).draw();
        tableEnvi.search($(this).val()).draw();
        tableRcib.search($(this).val()).draw();
    })

    $('#tableInit_filter').hide();
    $('#tableEnvi_filter').hide();
    $('#tableRcib_filter').hide();

});
