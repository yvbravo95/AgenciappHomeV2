$(document).ready(function () {
    // Code here

    //Para cuando se pulse el boton editar de un campo
    $('[name = "editval"]').on('click', function () {
        var parent = $(this).parent();
        var input = parent.children()[0];
        var provincia = input.attributes['data-prov'].value;
        var tramite = input.attributes['data-agency'].value;
        if (provincia != "" && tramite != "") {
            input.removeAttribute("disabled");
            var confirm = parent.children()[2];
            confirm.style.display = "";
            var edit = parent.children()[1];
            edit.style.display = "none";
        }
    });

    //Para cuando se pulse el boton editar de un campo remesa 2
    $('[name = "editval"]').on('click', function () {
        var parent = $(this).parent();
        var input = parent.children()[3];
        var provincia = input.attributes['data-prov'].value;
        var tramite = input.attributes['data-agency'].value;
        if (provincia != "" && tramite != "") {
            input.removeAttribute("disabled");
            var confirm = parent.children()[5];
            confirm.style.display = "";
            var edit = parent.children()[4];
            edit.style.display = "none";
        }
    });

    // Para cuando se pulse el boton guardar de un campo
    $('[name = "confirmval"]').on('click', function () {
        //Bloquear la pantalla

        //************************
        var parent = $(this).parent();
        var input = parent.children()[0];
        var provincia = input.attributes['data-prov'].value;
        var tramite = input.attributes['data-agency'].value;
        var type = input.attributes['data-type'].value;
        if (provincia != "" && tramite != "") {
            var valInput = input.value;
            data = [
                valInput,
                tramite,
                provincia,
                type
            ]
                
            $.ajax({
                async: true,
                type: "POST",
                dataType: 'json',
                contentType: 'application/json',
                url: "/Minorista/setValuePrecios",
                data: JSON.stringify(data),
                beforeSend: function () {
                   
                },
                success: function (response) {
                    var mensaje = response.split(',');
                    if (mensaje[0] == "fails") {
                        toastr.error(mensaje[1]);
                    }
                    else if (mensaje[0] == "success") {
                        input.setAttribute("disabled", true);
                        var confirm = parent.children()[2];
                        confirm.style.display = "none";
                        var edit = parent.children()[1];
                        edit.style.display = "";

                        toastr.success(mensaje[1]);
                    }
                   
                },
                error: function () {
                    toastr.error("No se ha podido guardar el valor", "Error");
                },
                timeout: 4000,
            });
        }
    });
    $('#tbl2').DataTable({
        "scrollX": true,
        "lengthChange": false,
        "searching": false,
        "paginate": false,
        "order": false
    })

    $('#tbl1').DataTable({
        "scrollX": true,
        "lengthChange": false,
        "searching": false,
        "paginate": false,
        "order": false,
        "fixedColumns": {
            "left": 1
        }
    })
});