
var containerRegistro = $('#containerRegistro');

var tbls = [];
var tblCancelada = null;

window.onload = async () => {
    const loadTable = async (container, tableName) => {
        $(container).block({
            message: '<span class="semibold"> Loading...</span>',
            overlayCSS: {
                backgroundColor: '#fff',
                opacity: 0.8,
                cursor: 'wait'
            },
            css: {
                border: 0,
                padding: 0,
                backgroundColor: 'transparent'
            }
        });
        await $.get("/ReporteCobro/GetRegistroCobro", (res) => {
            $(container).html(res);
            initTable(tableName);
            $(container).unblock();
        });
    }

    const initTable = (tableName) => {
        $('.dropdown-toggle').dropdown();
        var selector;
        var Name = "";
        switch (tableName) {
            case "tableRegistro":
                selector = $('#tableRegistro');
                Name = 'tableRegistro';
                break;
            default:
                break;
        }

        var table = selector.DataTable({
            "searching": true,
            "lengthChange": true,
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
        tbls.push(table);
        $('#' + Name + '_filter').hide();
    }

    const ExecuteLoadTable = async () => {
        await loadTable(containerRegistro, "tableRegistro");

        $('#searchRegistro').on('keyup change', function () {
            var search = $(this);
            tbls.forEach((item) => {
                if (item != null)
                    item.search($(search).val()).draw();
            })
        });
    }

    await ExecuteLoadTable();
}