$(document).on('ready', function () {
    oTable = $('#tableAutos').DataTable({
        "lengthChange": false,
        "dom": 't',
        "order": [[3, "asc"]],
        'language': {
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
        "columnDefs": [
            {
                targets: 3,
                visible: false
            }
        ],
        "scrollY": "400px",
        "scrollCollapse": true,
        "paging": false,
        "drawCallback": function (settings) {
            var api = this.api();
            var rows = api.rows({ page: 'current' }).nodes();
            var last = null;
            api.column(3, { page: 'current' }).data().each(function (group, i) {
                if (last != group) {
                    $(rows).eq(i).before(
                        '<tr class="group"><td colspan=7>' + group + '</td></tr>'
                    );
                    last = group;
                }
            })
        }
    });
    $(".selectdos").select2({
        width: '100%'
    });   
    var i = 0;
    $("#ConfirmAuto").on('click', function (e) {
        var row1 = [
            $("#modelo").val(),
            $("#categoria").val(),
            $("#transmision").val(),
            'Temporada Alta',
            $("#alta_3_6").val(),
            $("#alta_7_15").val(),
            $("#alta_16_30").val(),
            $("#alta_31_45").val()
        ];
        var row2 = [
            $("#modelo").val(),
            $("#categoria").val(),
            $("#transmision").val(),
            'Temporada Media Alta',
            $("#media_alta_3_6").val(),
            $("#media_alta_7_15").val(),
            $("#media_alta_16_30").val(),
            $("#media_alta_31_45").val()
        ];
        var row3 = [
            $("#modelo").val(),
            $("#categoria").val(),
            $("#transmision").val(),
            'Temporada Media Alta II',
            $("#media2_alta_3_6").val(),
            $("#media2_alta_7_15").val(),
            $("#media2_alta_16_30").val(),
            $("#media2_alta_31_45").val()
        ];
        var row4 = [
            $("#modelo").val(),
            $("#categoria").val(),
            $("#transmision").val(),
            'Temporada Extrema Alta',
            $("#extrema_alta_3_6").val(),
            $("#extrema_alta_7_15").val(),
            $("#extrema_alta_16_30").val(),
            $("#extrema_alta_31_45").val()
        ];
        for (var j = 0; j < 7; j++) {
                if (row1[j] == "" || row4[j] == "") {
                    e.preventDefault();
                    showErrorMessage("ERROR", "hay campos sin llenar");
                    return;
                }
        }
        oTable.row.add(row1);
        oTable.row.add(row4);

        oTable.order([3, 'asc']).draw();

        var inputs = '<input id="Autos_' + i + '__Modelo" name="Autos[' + i + '].Modelo" value="' + row1[0] + '" type="hidden"> <input id="Autos_' + i + '__Categoria" name="Autos[' + i + '].Categoria" value="' + row1[1] + '" type="hidden"> <input id="Autos_' + i + '__Transmision" name="Autos[' + i + '].Transmision" value="' + row1[2] + '" type="hidden"> <input id="Autos_' + i + '__Temporada" name="Autos[' + i + '].Temporada" value="Alta" type="hidden"> <input id="Autos_' + i + '__Precio1" name="Autos[' + i + '].Precio1" value="' + row1[4] + '" type="hidden"> <input id="Autos_' + i + '__Precio2" name="Autos[' + i + '].Precio2" value="' + row1[5] + '" type="hidden"> <input id="Autos_' + i + '__Precio3" name="Autos[' + i + '].Precio3" value="' + row1[6] + '" type="hidden"> <input id="Autos_' + i + '__Precio4" name="Autos[' + i + '].Precio4" value="' + row1[7] + '" type="hidden">';
        i++;

        inputs += '<input id="Autos_' + i + '__Modelo" name="Autos[' + i + '].Modelo" value = "' + row2[0] + '" type = "hidden"> <input id="Autos_' + i + '__Categoria" name="Autos[' + i + '].Categoria" value="' + row2[1] + '" type="hidden"> <input id="Autos_' + i + '__Transmision" name="Autos[' + i + '].Transmision" value="' + row2[2] + '" type="hidden"> <input id="Autos_' + i + '__Temporada" name="Autos[' + i + '].Temporada" value="MediaAlta" type="hidden"> <input id="Autos_' + i + '__Precio1" name="Autos[' + i + '].Precio1" value="' + row2[4] + '" type="hidden"> <input id="Autos_' + i + '__Precio2" name="Autos[' + i + '].Precio2" value="' + row2[5] + '" type="hidden"> <input id="Autos_' + i + '__Precio3" name="Autos[' + i + '].Precio3" value="' + row2[6] + '" type="hidden"> <input id="Autos_' + i + '__Precio4" name="Autos[' + i + '].Precio4" value="' + row2[7] + '" type="hidden">';
        i++;

        inputs += '<input id="Autos_' + i + '__Modelo" name="Autos[' + i + '].Modelo" value="' + row3[0] + '" type="hidden"> <input id="Autos_' + i + '__Categoria" name="Autos[' + i + '].Categoria" value="' + row3[1] + '" type="hidden"> <input id="Autos_' + i + '__Transmision" name="Autos[' + i + '].Transmision" value="' + row3[2] + '" type="hidden"> <input id="Autos_' + i + '__Temporada" name="Autos[' + i + '].Temporada" value="MediaAltaII" type="hidden"> <input id="Autos_' + i + '__Precio1" name="Autos[' + i + '].Precio1" value="' + row3[4] + '" type="hidden"> <input id="Autos_' + i + '__Precio2" name="Autos[' + i + '].Precio2" value="' + row3[5] + '" type="hidden"> <input id="Autos_' + i + '__Precio3" name="Autos[' + i + '].Precio3" value="' + row3[6] + '" type="hidden"> <input id="Autos_' + i + '__Precio4" name="Autos[' + i + '].Precio4" value="' + row3[7] + '" type="hidden">';
        i++;

        inputs += '<input id="Autos_' + i + '__Modelo" name = "Autos[' + i + '].Modelo" value = "' + row4[0] + '" type = "hidden" > <input id="Autos_' + i + '__Categoria" name="Autos[' + i + '].Categoria" value="' + row4[1] + '" type="hidden"> <input id="Autos_' + i + '__Transmision" name="Autos[' + i + '].Transmision" value="' + row4[2] + '" type="hidden"> <input id="Autos_' + i + '__Temporada" name="Autos[' + i + '].Temporada" value="ExtremaAlta" type="hidden"> <input id="Autos_' + i + '__Precio1" name="Autos[' + i + '].Precio1" value="' + row4[4] + '" type="hidden"> <input id="Autos_' + i + '__Precio2" name="Autos[' + i + '].Precio2" value="' + row4[5] + '" type="hidden"> <input id="Autos_' + i + '__Precio3" name="Autos[' + i + '].Precio3" value="' + row4[6] + '" type="hidden"> <input id="Autos_' + i + '__Precio4" name="Autos[' + i + '].Precio4" value="' + row4[7] + '" type="hidden">';
        i++;

        $("#rentadoraForm").append(inputs);
    });
    $("#modal_autos").on('hidden.bs.modal', function () {
        $(this).find('form')[0].reset();
        $("#categoria").select(" ").trigger('change');
        $("#transmision").select(" ").trigger('change');
    })
})