$('#dateNote').val(rangeNote);
$('#dateOrder').val(rangeOrder);

$('#filtrar').on('click', loadFilter)

$('#exportExcel').on('click', function () {
    const dateNote = $('#dateNote').val();
    const dateOrder = $('#dateOrder').val();

    location.href = `/InvoiceNote/ExportExcel?dateNote=${dateNote}&dateOrder=${dateOrder}`;
})

$('#limpiarFiltros').on('click', function () {
    $('#dateNote').val('');
    $('#dateOrder').val('');
    loadFilter()
})

function loadFilter() {
    const dateNote = $('#dateNote').val();
    const dateOrder = $('#dateOrder').val();

    location.href = `/InvoiceNote/Index?dateNote=${dateNote}&dateOrder=${dateOrder}`;
}

var tableND = $('#tblND').DataTable({
    "searching": true,
    "lengthChange": false,
    "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
    //"order": [[2, "desc"]],
    "ordering": false,
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

var tableNP = $('#tblNP').DataTable({
    "searching": true,
    "lengthChange": false,
    "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
    //"order": [[6, "desc"]],
    "ordering": false,
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
    tableND.search($(this).val()).draw();
    tableNP.search($(this).val()).draw();
})

$('#tblND_filter').hide();
$('#tblNP_filter').hide();
