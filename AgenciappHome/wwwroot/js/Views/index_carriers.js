var table = $('#tableCarriers').DataTable({
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

$('#tableCarriers_filter').hide();

$(document).ready(function () {

    var url = decodeURIComponent(window.location);
    var params = url.split("?")[1];
    if (params != null) {
        var params = params.split("&");
        var msg = params[0].split("=")[1];
        if (msg == "success") {
            var nombre = params[1].split("=")[1];
            showOKMessage("Nuevo Carrier", "Carrier " + nombre + " se ha adicionado con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successEdit") {
            var nombre = params[1].split("=")[1];
            showOKMessage("Editar Carrier", "Carrier " + nombre + " editado con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successDelete") {
            var nombre = params[1].split("=")[1];
            showOKMessage("Eliminar Carrier", "Carrier " + nombre + " eliminado con éxito", { "timeOut": 0, "closeButton": true });
        }
    }

    var searchAction = function () {
        table.search($(this).val()).draw();
    };

    $("#btnSearch").click(searchAction);
    $("#searchField").on("keyup change", searchAction);

    $(".deleteCarriers").click(function () {
        var carrierId = $(this).attr("carrierId");
        var nombre = $(this).attr("name");
        var okConfirm = function () {
            var urlDelete = "/Carriers/Delete/" + carrierId;
            $.ajax({
                type: "GET",
                url: urlDelete,
                async: false,
                success: function () {
                    document.location = "/Carriers?msg=successDelete&nombre=" + nombre;
                }
            });
        };
        getDelConfirmation(okConfirm);
    });

    

});