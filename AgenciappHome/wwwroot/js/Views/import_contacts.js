$(document).ready(function () {
    $('#fUpload').change(function () {
        if (validateFile()) {
            $('#btnLoad').removeAttr("disabled");
        } else {
            $('#btnLoad').attr("disabled", "disabled");
        }
    });

    $('#btnLoad').on('click', function () {
        if (validateFile()) {
            var fdata = new FormData();
            var fileUpload = $("#fUpload").get(0);
            var files = fileUpload.files;
            fdata.append(files[0].name, files[0]);
            $.ajax({
                type: "POST",
                url: "/Contacts/ReadExcel",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("XSRF-TOKEN",
                        $('input:hidden[name="__RequestVerificationToken"]').val());
                },
                data: fdata,
                contentType: false,
                processData: false,
                async: false,
                success: function (response) {
                    if (response.length == 0)
                        showErrorMessage("ERROR", "Ha ocurrido un error mientras cargaba los datos.");
                    else {
                        showValues(response.values);
                    }
                },
                error: function (e) {
                    showErrorMessage("ERROR", e.responseText);
                    $("#btnUpload").addClass("hidden");
                }
            });
        }
    });
    var valuesSub;
    var cantNewContacts;
    var showValues = function (values) {

        $("#tableContacts thead tr").remove();
        $("#tableContacts tr").remove();

        var tHead = $("#tableContacts > THEAD")[0];

        ////Add Row.
        var heads = tHead.insertRow(0);
        var l = 0;
        for (var i = 0; i < values.length; i++) {
            if (values[i].length > l)
                l = values[i].length
        }
        for (var i = 0; i < l; i++) {
            var cell = $(heads.insertCell(-1));
            cell.append("<select class='form-control'><option>Nombre</option><option>Apellido</option><option>Teléf. Primario</option><option>Teléf. Secundario</option><option>Dirección</option><option>Provincia</option><option>Municipio</option><option>Reparto</option><option>CI</option></select>");
        }

        for (var i = 0; i < values.length; i++) {

            var line = values[i];

            //Get the reference of the Table's TBODY element.
            var tBody = $("#tableContacts > TBODY")[0];

            var index = $(tBody).children().length - 1;

            ////Add Row.
            var row = tBody.insertRow(index);

            for (var j = 0; j < l; j++) {
                ////Add Button cell.
                var cell = $(row.insertCell(-1));
                if (j > line.length)
                    cell.append("");
                else
                    cell.append(line[j]);
            }
        }

        valuesSub = new Array;

        var tBody = $("#tableContacts > TBODY")[0];
        cantNewContacts = tBody.rows.length;
        for (var i = 0; i < cantNewContacts; i++) {
            var fila = tBody.rows[i];
            valuesSub[i] = new Array;
            for (var j = 0; j < fila.children.length; j++) {
                valuesSub[i][j] = $(fila.children[j]).html();
            }
        }
        $('#tableContacts').DataTable();
        $("#btnUpload").removeClass("hidden");
    }

    var validateFile = function () {
        var fileExtension = ['xls', 'xlsx'];
        var filename = $('#fUpload').val();
        if (filename.length == 0) {
            showWarningMessage("Atención", "Por favor, seleccione un archivo.");
            return false;
        } else {
            var extension = filename.replace(/^.*\./, '');
            if ($.inArray(extension, fileExtension) == -1) {
                showWarningMessage("Atención", "Por favor, seleccione solamente archivos excel.");
                return false;
            }
        }

        return true;
    }

    $('#btnUpload').click(function () {
        if (validateBeforeUpload()) {
            var mapCol = [indexName, indexLastName, indexTelefMovil, indexTelefCasa, indexDir, indexProvince, indexMunicipio, indexReparto, indexCI];

            

            var datosImport = [
                mapCol,
                valuesSub,
            ];

            $.ajax({
                type: "POST",
                url: "/Contacts/OnPostImport",
                data: JSON.stringify(datosImport),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                beforeSend: function () {
                    $.blockUI({
                        message: '<div class="ft-refresh-cw icon-spin font-medium-2"></div>',
                        timeout: 60000,
                        overlayCSS: {
                            backgroundColor: '#FFF',
                            opacity: 0.8,
                            cursor: 'wait'
                        },
                        css: {
                            border: 0,
                            padding: 0,
                            backgroundColor: 'transparent'
                        }
                    });
                },
                success: function () {
                    document.location = "/Contacts/ImportContact?msg=Se han importado " + cantNewContacts + " contactos";
                }
            });
        }
    });

    validateBeforeUpload = function () {
        var cantCol = $("#tableContacts thead tr td").length;
        if (cantCol < 3) {
            showWarningMessage("Atención", "No hay la cantidad de columnas suficientes.");
            return false;
        } else {
            indexName = -1;
            indexLastName = -1;
            indexTelefMovil = -1;
            indexTelefCasa = -1;
            indexDir = -1;
            indexProvince = -1;
            indexMunicipio = -1;
            indexReparto = -1;
            indexCI = -1;

            var repitedCol = false;
            $("#tableContacts thead tr td select").each(function (pos, e) {
                for (var i = 0; i < cantCol; i++) {
                    if (pos == i) continue;
                    var varCol = $($("#tableContacts thead tr td select")[i]).val();
                    if ($(e).val() == varCol) {
                        repitedCol = true;
                    }
                    if ($(e).val() == "Nombre") {
                        indexName = pos;
                    } else if ($(e).val() == "Apellido") {
                        indexLastName = pos;
                    } else if ($(e).val() == "Teléf. Primario") {
                        indexTelefMovil = pos;
                    } else if ($(e).val() == "Teléf. Secundario") {
                        indexTelefCasa = pos;
                    } else if ($(e).val() == "Dirección") {
                        indexDir = pos;
                    } else if ($(e).val() == "Provincia") {
                        indexProvince = pos;
                    } else if ($(e).val() == "Municipio") {
                        indexMunicipio = pos;
                    } else if ($(e).val() == "Reparto") {
                        indexReparto = pos;
                    }
                    else if ($(e).val() == "CI") {
                        indexCI = pos;
                    }
                }
            });

            if (repitedCol) {
                showWarningMessage("Atención", "No se pueden repetir los identificadores de las columnas.");
                return false;
            } else if (indexName == -1) {
                showWarningMessage("Atención", "No debe faltar una columna identificada con el Nombre del contacto.");
                return false;
            } else if (indexLastName == -1) {
                showWarningMessage("Atención", "No debe faltar una columna identificada con el Apellido del contacto.");
                return false;
            } else if (indexTelefMovil == -1 && indexTelefCasa == -1) {
                showWarningMessage("Atención", "Debe haber al menos una columna con el Teléfono del contacto.");
                return false;
            //} else if (indexReparto == -1) {
            //    showWarningMessage("Atención", "No debe faltar una columna identificada con el Municipio del contacto.");
            //    return false;
            }
        }

        return true;
    }
});