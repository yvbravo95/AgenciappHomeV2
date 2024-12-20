$(document).ready(function () {

    // Cuando se elija un controlador se obtienen las acciones
    $('#selectController').on('change', function () {
        var name = $(this).val();
        $.ajax({
            type: "POST",
            url: "/Users/GetActionOfController",
            data: JSON.stringify(name),
            dataType: 'json',
            contentType: 'application/json',
            async: false,
            success: function (data) {
                // Asigno los valores al select
                $("#selectAction").empty();
                $("#selectAction").append(new Option("Asignar Todo", ""));
                for (var i = 0; i < data.length; i++) {
                    $("#selectAction").append(new Option(data[i].action, data[i].accessListId));
                }

            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    });

    // Asignar al usuario el permiso de acceder
    $('#addPermiso').on('click', function () {
        var idUser = $('#idUser').attr('data-id');
        var idAction = $('#selectAction').val();
        var idController = $('#selectController').val();
        //verifico que el controlador y el action no esten vacios
        if (idUser != null && idAction != null) {
            $.ajax({
                type: "POST",
                url: "/Users/AddPermisoUser",
                data: {
                    idUser: idUser,
                    idAction: idAction,
                    idController:idController
                },
                async: false,
                success: function (data) {
                    if (data != "") {
                        showErrorMessage("ERROR", data);
                    }
                    else {
                        location.reload();
                    }

                },
                failure: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                },
                error: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                }
            });
        }
        else {
            showErrorMessage("ERROR", "Verifique que el controllador y la acción no estén vacíos");

        }
    });

    // Quitar un permiso al usuario
    $('[name = "delete"]').on('click', function () {
        var idUser = $(this).attr('data-id');
        var idAction = $(this).attr('data-action');
        var data = [
            idUser,
            idAction
        ];
        if (idUser != null && idAction != null) {
            $.ajax({
                type: "POST",
                url: "/Users/DeletePermisoUser",
                data: JSON.stringify(data),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function (data) {
                    if (data != "") {
                        showErrorMessage("ERROR", data);

                    }
                    else {
                        location.reload();
                    }

                },
                failure: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                },
                error: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                }
            });
        }
        else {
            showErrorMessage("ERROR", "No se ha podido eliminar el permiso");

        }
    });

    // Para que un empleado pueda ver el menu de administración
    $('#viewadmin').on('click', function () {
        var ischecked = $(this).is(':checked');
        var idUser = $(this).attr('data-id');
        $.ajax({
            type: "POST",
            url: "/Users/changeViewAdmin",
            data: {
                ischecked: ischecked,
                idUser: idUser
            },
            async: false,
            success: function (data) {
                if (data == "success") {
                    if (ischecked) {
                        toastr.info("El Usuario podrá ver la administración");
                    }
                    else {
                        toastr.info("El Usuario no podrá ver la administración")
                    }
                }
                else {
                    toastr.error("No se ha podido establecer el permiso");
                }

            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    });
    $('#viewcuentas').on('click', function () {
        var ischecked = $(this).is(':checked');
        var idUser = $(this).attr('data-id');
        $.ajax({
            type: "POST",
            url: "/Users/changeViewCuentas",
            data: {
                ischecked: ischecked,
                idUser: idUser
            },
            async: false,
            success: function (data) {
                if (data == "success") {
                    if (ischecked) {
                        toastr.info("El Usuario podrá ver el menú cuentas");
                    }
                    else {
                        toastr.info("El Usuario no podrá ver el menú cuentas")
                    }
                }
                else {
                    toastr.error("No se ha podido establecer el permiso");
                }

            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    });
    $('#viewindexadmin').on('click', function () {
        var ischecked = $(this).is(':checked');
        var idUser = $(this).attr('data-id');
        $.ajax({
            type: "POST",
            url: "/Users/changeViewIndexAdmin",
            data: {
                ischecked: ischecked,
                idUser: idUser
            },
            async: false,
            success: function (data) {
                if (data == "success") {
                    if (ischecked) {
                        toastr.info("El Usuario podrá ver el Index de Administración");
                    }
                    else {
                        toastr.info("El Usuario no podrá ver el Index de Administración")
                    }
                }
                else {
                    toastr.error("No se ha podido establecer el permiso");
                }

            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    });

});