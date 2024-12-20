$(document).ready(function () {
    //code here
    $('#addGasto').click(function () {
        $('#ModalAddGasto').modal('show');
    });

    $('#editName').on('click', function () {
        //Ingresamos un mensaje a mostrar
        var name = prompt("Escriba un nombre para la Caja", "");
        //Detectamos si el usuario ingreso un valor
        if (name != null && name != "") {
            $.ajax({
                type: "POST",
                url: "/contabilidad/EditNameBox",
                data: {
                    name: name,
                    boxId: boxId
                },
                async: false,
                beforeSend: function () {
                    $.blockUI();
                },
                success: function (data) {
                    if (data.success) {
                        $('#nameBox').html(name);
                        toastr.success(data.msg);
                    }
                    else {
                        toastr.error(data.msg);
                    }
                    $.unblockUI();
                },
                failure: function (response) {
                    $.unblockUI();
                    showErrorMessage("ERROR", "No se ha podido editar el nombre de la caja");
                },
                error: function (response) {
                    $.unblockUI();
                    showErrorMessage("ERROR", "No se ha podido editar el nombre de la caja");
                }
            });
        }
        //Detectamos si el usuario NO ingreso un valor
        else {
            alert("No se ha ingresado un nombre");
        }
    });
});