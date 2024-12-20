var NoDespacho = "";
var Nota = "";
$.fn.editable.defaults.mode = 'inline';
$(function () {
    $('.status').editable({
        source: function (p) {
            s = [
                {// Estado Pendiente
                    "1": 'Pendiente',
                },
                {// Estado Confeccion
                    "2": 'Confeccion',
                    "3": 'Iniciada',
                },
                {// Estado Iniciada
                    "3": 'Iniciada',
                    "4": 'Despachada',
                },
                {//Despachada
                    "3": 'Iniciada',
                    "4": 'Despachada',
                },
                {//Cancelada
                    "5": 'Cancelada',
                },

            ];
            return s[$(this).data('value') - 1];

        },
        validate: function (x) {
            if (x == "4") {
                NoDespacho = prompt("Número de Despacho:", "");
                if (NoDespacho == "" || NoDespacho == null) {
                    toastr.warning("El estado del trámite no será actualizado.");
                    return " ";
                }
                else {
                    Nota = prompt("Nota:", "");
                    if (Nota == null) {
                        toastr.warning("El estado del trámite no será actualizado.");
                        return " ";
                    }
                }
            }
        },
        display: function (value, sourceData) {
            var colors = { "": "gray", 1: "#ffb019", 2: "red", 3: "green", 4: "red", 5: "#ef00ff", 6: "#000da8" },
                elem = $.grep(sourceData, function (o) { return o.value == value; });

            if (elem.length) {
                $(this).text(elem[0].text).css("color", colors[value]);
            } else {
                $(this).empty();
            }

        },
        ajaxOptions: {
            url: '/EnviosCaribe/index',
            type: 'post',
            dataType: 'json'
        },
        params: function (params) {
            params.id = $(this).data("id");
            params.numdespacho = NoDespacho;
            params.nota = Nota;
            return params;
        },
        success: function (response, newValue) {
            location.reload();
            //showOKMessage("Cambio de estatus", "Se ha cambiado satifactoriamente el estatus");
        },
        //showbuttons: false
    });
});

$(document).ready(function () {
    


    $('#exportExcelAccept').attr('href', '/EnviosCaribe/ExportExcel/?date=' + $('#daterange').val() + "&type=" + $("#ul_tab").find('a.active').data('type'));

    $('#daterange').on('change', function () {
        $('#exportExcelAccept').attr('href', '/EnviosCaribe/ExportExcel/?date=' + $('#daterange').val() + "&type=" + $("#ul_tab").find('a.active').data('type'));
    });

    $("#exportExcel").on('click', function (e) {
        $('#exportExcelAccept').attr('href', '/EnviosCaribe/ExportExcel/?date=' + $('#daterange').val() + "&type=" + $("#ul_tab").find('a.active').data('type'))
    });

    $('#exportExcelAccept').on('click', function () {
        $('#modalExport').modal('hide');
    })

    $('#exportExcel').on('click', function () {
        $('#modalExport').modal('show');
    });
});


