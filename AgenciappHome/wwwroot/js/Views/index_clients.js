$(document).ready(function () {

    var url = decodeURIComponent(window.location);
    var params = url.split("?")[1];
    var clientId = null;

    $(document).on('click','[name="addNote"]', function(){
        clientId = $(this).attr('data-id');
        $('#modalNota').modal('show');
    });

    $('#btnAddNota').on('click', function(){
        $('#modalNota').modal('hide');
        var note = $('#nota').val();
        if(clientId == null || nota == null){
            toastr.error('No se ha podido crear la nota');
        }
        
        $.ajax({
            async: true,
            type: "Post",
            url: "/Clients/AddNote",
            data: {
                clientId: clientId,
                note: note
            },
            success: function(response){
                if(response.success){
                    toastr.success("La nota ha sido creada");
                }
                else{
                    toastr.error(response.msg);
                }
            },
            error: function(){
                toastr.error("No se ha podido crear la nota");
            }
        })
    })

    if (params != null) {
        var params = params.split("&");
        var msg = params[0].split("=")[1];
        if (msg == "success") {
            var nombre = params[1].split("=")[1];
            showOKMessage("Nuevo Cliente", "Cliente " + nombre + " se ha adicionado con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successEdit") {
            var nombre = params[1].split("=")[1];
            showOKMessage("Editar Cliente", "Cliente " + nombre + " editado con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successDelete") {
            var nombre = params[1].split("=")[1];
            showOKMessage("Eliminar Cliente", "Cliente " + nombre + " eliminado con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successImport") {
            var cantClientsImport = params[1].split("=")[1];
            if (cantClientsImport == 1)
                showOKMessage("Importar Clientes", "1 cliente importado con éxito", { "timeOut": 0, "closeButton": true });
            else
                showOKMessage("Importar Clientes", "Se han importado " + cantClientsImport + " clientes.", { "timeOut": 0, "closeButton": true });
        }
        else if (msg =! null) {
            showOKMessage("Crear Cliente", msg);
        }
    }

    //**************** Table *******************

    const oTable = $('#tableClients').DataTable({
        "searching": true,
        "lengthChange": false,
        processing: true,
        //"order": [[0, "desc"]],
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
        "serverSide": true,
        "ajax": {
            "url": "/Clients/List",
            "type": 'POST',
            "dataType": "json",
            data: function (dtp) {
                // change the return value to what your server is expecting
                // here is the path to the search value in the textbox
                var searchValue = dtp.search.value;
                return dtp;
            }
        },
        "columnDefs": [
            { "data": "firstName", targets: 0 },
            { "data": "surname", targets: 1 },
            {
                "data": "phoneNumber", targets: 2,
                "render": function (data, type, row, meta) {
                    return "<a href='/Clients/Tramites/" + row["clientId"] + "'>" + data + "</a>";

                }
            },
            { "data": "date", targets: 3 },
            {
                targets: 4,
                "data": "clientId",
                "render": function (data, type, row, meta) {
                    return "" +
                        "<a class='primary editClient mr-1' data-id='" + data + "'><i class='fa fa-pencil'  data-id='" + data + "'></i></a>" +
                        "<a class='danger delete mr-1 deleteClients' clientId='" + data + "' name='" + row['name'] + row['lastName'] + "'><i class='fa fa-trash-o'></i></a>" +
                        "<div class='dropdown'>" +
                        "<i class='fa fa-ellipsis-v dropdown-toggle' data-toggle='dropdown' style='cursor: pointer'></i>" +
                        "<div style='left: -146px;' class='dropdown-menu  arrow-left' aria-label='dropdown" + data + "' >" +
                        "<a class='dropdown-item' href='#' name='addNote' data-id='" + data + "'><i class='ft-plus'></i> Nota</a>" +
                        "<a class='dropdown-item editClient' data-id='" + data + "'><i class='ft-edit'></i>Editar</a>" +
                        "<a class='dropdown-item' href='/Clients/Details/" + data + "'><i class='ft-info'></i>Detalles</a>" +
                            "<a class='dropdown-item' href='/Clients/GestionarCredito/" + data + "'><i class='ft-info'></i>Ver Créditos</a>" +
                                "<a class='dropdown-item' href='/Clients/Tramites/" + data + "'><i class='fa fa-th-list'></i>Ver trámites</a>" +
                                    "<a class='dropdown-item deleteClients'clientId='" + data + "' name='" + row['name'] + row['lastName'] + "' href='#'><i class='ft-delete'></i>Eliminar</a>" +
                                    "</div>" +
                                    "</div>"
                        ;
                },
                "createdCell": function (td, cellData, rowData, row, col) {
                    $(td).css('padding', 'gf 0.5');
                    $(td).css('display', 'inline-flex');
                }
            }
        ],
        "rowCallback": function (row, data, index) {
            if (data["conflictivo"]) {
                $(row).find('td:eq(0)').css('color', 'red');
                $(row).find('td:eq(1)').css('color', 'red');
            }
        },
        "initComplete": function () {
            
        }
    });

    $(document).on('click', '.deleteClients', function () {
        var clientId = $(this).attr("clientId");
        var nombre = $(this).attr("name");
        var okConfirm = function () {
            var urlDelete = "/Clients/Delete/" + clientId;
            $.ajax({
                type: "GET",
                url: urlDelete,
                async: false,
                success: function () {
                    document.location = "/Clients?msg=successDelete&nombre=" + nombre;
                }
            });
        };
        getDelConfirmation(okConfirm);
    });

    $(document).on('click', '.editClient', function (e) {
        var id = $(e.target).data("id");
        $.ajax({
            type: "POST",
            url: "/Clients/GetClient",
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(id),
            async: false,
            success: function (data) {
                selectedClientData = data;
                $("#editarCliente").trigger("click");
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            }
        });
    })

    $('#search').on('change', function (a) {
        var inputSearch = $('#tableClients_filter').find('input')[0];

        $(inputSearch).val($(this).val())
        $(inputSearch).trigger('keyup');

    })
    $('#tableClients_filter').hide();

    $("#filtrarProvincia").on("click", () => {
        $("#filtroProvinciaModal").modal("hide");
        var provincias = ($(".provinciasCheck:checked").map((i, e) => $(e).val())).get().join("|");
        oTable.column(4).search(provincias).draw();
    })

    $(".searchColumn").on("change", (e) => {
        var column = $(e.target).data("column")
        oTable.column(column).search(e.target.value).draw();
    })

    
});