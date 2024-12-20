$(document).ready(function () {

    var url = decodeURIComponent(window.location);
    var params = url.split("?")[1];
    if (params != null) {
        var params = params.split("&");
        var msg = String.split(params[0], "=")[1];
        if (msg == "success") {
            var nombre = params[1].split("=")[1];
            showOKMessage("Nuevo Usuario", "Usuario " + nombre + " se ha adicionado con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successEdit") {
            var nombre = params[1].split("=")[1];
            showOKMessage("Editar Usuario", "Usuario " + nombre + " editado con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successDelete") {
            var nombre = params[1].split("=")[1];
            showOKMessage("Eliminar Usuario", "Usuario " + nombre + " eliminado con éxito", { "timeOut": 0, "closeButton": true });
        }
    }

    table = $('#tableUsers').DataTable({
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

    $('#tableUsers_filter').hide();


    

    $(".deleteUsers").click(function () {
        var userId = $(this).attr("userId");
        var nombre = $(this).attr("name");
        var okConfirm = function () {
            var urlDelete = "/Users/Delete/" + userId;
            $.ajax({
                type: "GET",
                url: urlDelete,
                async: false,
                success: function () {
                    document.location = "/Users?msg=successDelete&nombre=" + nombre;
                }
            });
        };
        getDelConfirmation(okConfirm);
    });

});