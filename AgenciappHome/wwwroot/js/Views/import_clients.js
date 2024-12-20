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
                async: true,
                type: "POST",
                url: "/Clients/ReadExcel",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("XSRF-TOKEN",
                        $('input:hidden[name="__RequestVerificationToken"]').val());
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
                data: fdata,
                contentType: false,
                processData: false, 

                success: function (response) {
                    if (response.length == 0)
                        showErrorMessage("ERROR", "Ha ocurrido un error mientras cargaba los datos.");
                    else {
                        showValues(response.values);
                    }
                    $.unblockUI();
                },
                error: function (e) {
                    showErrorMessage("ERROR", e.responseText);
                    $("#btnUpload").addClass("hidden");
                }
            });
        }
    });

    var showValues = function (values) {

        $("#tableClients thead tr").remove();
        $("#tableClients tr").remove();

        var tHead = $("#tableClients > THEAD")[0];

        ////Add Row.
        var heads = tHead.insertRow(0);
        for (var i = 0; i < values[0].length; i++) {
            var cell = $(heads.insertCell(-1));
            cell.append("<select class='form-control'><option>Any</option><option>Nombre</option><option>Apellido</option><option>Teléfono</option><option>Email</option><option>Dirección</option><option>Ciudad</option><option>Estado</option><option>Zip</option><option>Pasaporte</option></select>");
        }

        for (var i = 0; i < values.length; i++) {

            var line = values[i];

            //Get the reference of the Table's TBODY element.
            var tBody = $("#tableClients > TBODY")[0];

            var index = $(tBody).children().length - 1;

            ////Add Row.
            var row = tBody.insertRow(index);

            for (var j = 0; j < line.length; j++) {
                ////Add Button cell.
                var cell = $(row.insertCell(-1));
                cell.append(line[j]);
            }
        }

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
            var mapCol = [indexName, indexLastName, indexMovil, indexEmail, indexDir, indexCity, indexState, indexZip, indexPassport];

            var values = new Array;

            var tBody = $("#tableClients > TBODY")[0];
            var cantNewClients = tBody.rows.length;
            for (var i = 0; i < cantNewClients; i++) {
                var fila = tBody.rows[i];
                values[i] = new Array;
                for (var j = 0; j < fila.children.length; j++) {
                    values[i][j] = $(fila.children[j]).html();
                }
            }

            var datosImport = [
                mapCol,
                values,
            ];

            $.ajax({
                type: "POST",
                url: "/Clients/OnPostImport",
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
                    document.location = "/Clients?msg=successImport&cant=" + cantNewClients;
                }
            });
        }
    });

    validateBeforeUpload = function () {
        var cantCol = $("#tableClients thead tr td").length;
        if (cantCol < 3) {
            showWarningMessage("Atención", "No hay la cantidad de columnas suficientes.");
            return false;
        } else {
            indexName = -1;
            indexLastName = -1;
            indexMovil = -1;
            indexEmail = -1;
            indexDir = -1;
            indexCity = -1;
            indexState = -1;
            indexZip = -1;
            indexPassport = -1;

            var repitedCol = false;
            $("#tableClients thead tr td select").each(function (pos, e) {
                for (var i = 0; i < cantCol; i++) {
                    if (pos == i) continue;
                    var varCol = $($("#tableClients thead tr td select")[i]).val();
                    if ($(e).val() != 'Any' && $(e).val() == varCol) {
                        repitedCol = true;
                    }
                    if ($(e).val() == "Nombre") {
                        indexName = pos;
                    } else if ($(e).val() == "Apellido") {
                        indexLastName = pos;
                    } else if ($(e).val() == "Teléfono") {
                        indexMovil = pos;
                    } else if ($(e).val() == "Email") {
                        indexEmail = pos;
                    } else if ($(e).val() == "Dirección") {
                        indexDir = pos;
                    } else if ($(e).val() == "Ciudad") {
                        indexCity = pos;
                    } else if ($(e).val() == "Estado") {
                        indexState = pos;
                    } else if ($(e).val() == "Zip") {
                        indexZip = pos;
                    }else if ($(e).val() == "Pasaporte") {
                        indexPassport = pos;
                    }
                }
            });

            if (repitedCol) {
                showWarningMessage("Atención", "No se pueden repetir los identificadores de las columnas.");
                return false;
            } else if (indexName == -1) {
                showWarningMessage("Atención", "No debe faltar una columna identificada con el Nombre del cliente.");
                return false;
            } else if (indexMovil == -1) {
                showWarningMessage("Atención", "No debe faltar una columna identificada con el Teléfono del cliente.");
                return false;
            }
        }

        return true;
    }
});