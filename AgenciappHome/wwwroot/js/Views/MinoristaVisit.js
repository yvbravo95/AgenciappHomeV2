$(document).ready(function () {
    $('[name="btnVisit"]').on('click', function () {
        var agencyId = $(this).attr('data-idAgency');

        $.ajax({
            async: true,
            type: "POST",
            url: "/users/visitretailagency/",
            data: {
                id: agencyId
            },
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                if (response.success) {
                    location.href = "/Home/Index"
                }
                else {
                    toastr.error(response.msg);
                }
                $.unblockUI();

            },
            error: function (response) {
                console.log(response);
                toastr.error("No se ha podido visitar la agencia")
                $.unblockUI();
            }
        })
    })

    $('#btnAddAgencyByNumber').on('click', function () {
        const number = $('#addAgencyByNumber').val();
        if (!number) {
            toastr.error("El numero de agencia no es valido")
            return false;
        }

        $.ajax({
            async: true,
            type: "POST",
            url: "/minorista/AddAgencyByNumber/",
            data: {
                number: number
            },
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                if (response.success) {
                    location.reload();
                }
                else {
                    toastr.error(response.message);
                }
                $.unblockUI();

            },
            error: function (response) {
                console.log(response);
                toastr.error("Ha ocurrido un error.")
                $.unblockUI();
            }
        })
    })

    var table = $('#tbl').DataTable({
        "searching": true,
        "dom": "ltip",
        "lengthChange": false,
        "order": [[0, "desc"]],
        //"scrollX": false,
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

    $('#search').on('keyup change', function (e) {
        table.search($(e.target).val()).draw();
    })

    $('#btn-updateexchangerate').on('click', function () {
        const value = $('#exchangeRate').val();
        // ajax call
        $.ajax({
            async: true,
            type: "POST",
            url: "/minorista/ExchangeRateCup/",
            data: {
                exchangeRate: value
            },
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                if (response.success) {
                    toastr.success("Tipo de cambio actualizado correctamente"); 
                }
                else {
                    toastr.error(response.message);
                }
                $.unblockUI();

            },
            error: function (response) {
                console.log(response);
                toastr.error("Ha ocurrido un error.")
                $.unblockUI();
            }
        })
    })

    $('.btn-agency-info').click(async function () {
        const id = $(this).attr('data-idagency');
        const info = await fetch('/minorista/agencyinfo?id=' + id).then(res => { return res.json() });
        $('#agencyName').html(info.data.agencyName);
        $('#agencyPhone').html(info.data.agencyPhone);
        $('#agencyAddress').html(info.data.agencyAddressLine1);
        $('#agencyCity').html(info.data.agencyAddressCity);
        $('#agencyState').html(info.data.agencyAddressState);
        $('#agencyZip').html(info.data.agencyAddressZip);

        const tablaBody = document.querySelector('#tblUsers tbody');

        $(tablaBody).html("");

        info.data.users.forEach(user => {
            const fila = document.createElement('tr');

            const celdaNombre = document.createElement('td');
            celdaNombre.textContent = user.name;
            fila.appendChild(celdaNombre);

            const celdaApellido = document.createElement('td');
            celdaApellido.textContent = user.lastName;
            fila.appendChild(celdaApellido);

            const celdaTelefono = document.createElement('td');
            celdaTelefono.textContent = user.phone;
            fila.appendChild(celdaTelefono);

            tablaBody.appendChild(fila);
        })

        $('#modalAgencyInfo').modal('show');
    })
});
