
$(document).ready(function () {

    var totalFacturada = $('#totalFacturada'),
        cobradoPagada = $('#cobradoPagada'),
        debeFacturada = $('#debeFacturada');

    var idsFacturada = [];
    var idsPagadas = [];

    tblFacturada = $('#tblFacturada').DataTable({
        "searching": true,
        "lengthChange": true,
        "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
        "Remittance": [],
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
    tblPagada = $('#tblPagada').DataTable({
        "searching": true,
        "lengthChange": true,
        "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
        "Remittance": [],
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
        tblFacturada.search($(this).val()).draw();
        tblPagada.search($(this).val()).draw();
        calculate();
    })
    $('#tblFacturada_filter').hide();
    $('#tblPagada_filter').hide();


    var calculate = function () {
        idsFacturada = [];
        idsPagadas = [];

        //Tbl Facturada
        var total = 0;
        var pagado = 0;
        var rows = tblFacturada.rows({ filter: 'applied' }).nodes();
        $.each(rows, function (index, value) {
            var tds = $(value).find('td')
            total += parseFloat($(tds[6]).attr('data-total'));
            pagado += parseFloat($(tds[6]).attr('data-pagado'));
            idsFacturada.push($(tds[6]).attr('data-id'));
        });
        totalFacturada.html(total.toFixed(2));
        debeFacturada.html((total - pagado).toFixed(2));

        //Tbl Pagada
        pagado = 0;
        var rows = tblPagada.rows({ filter: 'applied' }).nodes();
        $.each(rows, function (index, value) {
            var tds = $(value).find('td')
            pagado += parseFloat($(tds[6]).attr('data-pagado'));
            idsPagadas.push($(tds[6]).attr('data-id'));
        });
        cobradoPagada.html(pagado.toFixed(2));
    }

    calculate();

    $('#exportarExcel').click(function () {
        var url = '';
        if ($('#tab-facturada').hasClass('active')) {
            url = '/Facturas/ExportExcel?status=Facturada';
            
        }
        else if ($('#tab-pagada').hasClass('active')) {
            url = '/Facturas/ExportExcel?status=Pagada';
        }
        window.open(url, 'Facturas');
    });

})