$(document).ready(function () {
    // Setup - add a text input to each footer cell
    document.getElementById('inputBarCode').focus();

    $('#tbl tfoot th').each(function () {
        var title = $(this).text();
        $(this).html('<input type="text" class="input-sm" placeholder="' + title + '" />');
    });

    $('#exportHAWB').on('click', function () {
        const qty = parseInt($(this).attr('data-qty'));
        const guiaId = $(this).attr('data-guiaId');
        if (qty <= 500) {
            const href = `/Print/CubiqHAWB?id=${guiaId}&init=${0}&end=${qty}`;
            window.open(href, '_blank');
        }
        else {
            const middle = parseInt(qty / 2);
            const href1 = `/Print/CubiqHAWB?id=${guiaId}&init=${0}&end=${middle}`;
            const href2 = `/Print/CubiqHAWB?id=${guiaId}&init=${middle}&end=${qty}`;
            window.open(href1, '_blank');
            window.open(href2, '_blank');
        }
    })

    // DataTable
    var table = $('#tbl').DataTable({
        dom: 'Bfrtip',
        buttons: [
            'copy',
            'csv',
            'excel',
            {
                text: 'Excel Orders',
                action: function (e, dt, node, config) {
                    var url = "/OrderCubiq/ExcelOrdersGuia/" + guiaId;
                    window.open(url, '_blank').focus();
                }
            },
            {
                text: 'Excel Ventas',
                action: function (e, dt, node, config) {
                    var url = "/OrderCubiq/ExcelVentasGuia/" + guiaId;
                    window.open(url, '_blank').focus();
                }
            },
             {
                text: 'Excel Costos',
                action: function (e, dt, node, config) {
                    var url = "/OrderCubiq/ExcelCostosGuia/" + guiaId;
                    window.open(url, '_blank').focus();
                }
            }
        ],
        initComplete: function () {
            // Apply the search
            this.api().columns().every(function () {
                var that = this;

                $('input', this.footer()).on('keyup change clear', function () {
                    if (that.search() !== this.value) {
                        that
                            .search(this.value)
                            .draw();
                    }
                });
            });
        }
    });

    $('.buttons-copy, .buttons-csv, .buttons-print, .buttons-pdf, .buttons-excel, .dt-button').addClass('btn btn-primary mr-1');


    $('.toggle-vis').on('change', function (e) {
        e.preventDefault();

        // Get the column API object
        var column = table.column($(this).attr('data-column'));

        // Toggle the visibility
        column.visible(!column.visible());
    });

    updateColumnsTable();

    function updateColumnsTable() {
        $('.toggle-vis').each(function (index, element) {
            // Get the column API object
            var column = table.column($(element).attr('data-column'));

            // Toggle the visibility
            column.visible($(element).prop('checked'));
        });
    }

    $('#inputBarCode').on('change', function () {
        var type = $('#scan-type').val();
        var code = $(this).val();
        if (!code) {
            toastr.error("Debe escanear un código de barras", "Error");
            return;
        }

        if (type == 'bolsa') {
            addBolsa(code, guiaId);
        }
        else if (type == 'orden') {
            addOrden(code, guiaId);
        }
        else if (type == 'pallet') {
            addPallet(code, guiaId);
        }

        
    })

    function addBolsa(code, guiaId) {
        $.ajax({
            type: "POST",
            url: "/OrderCubiq/AddPackageToGuia",
            data: {
                number: code.trim(),
                guideId: guiaId
            },
            async: false,
            success: function (response) {
                if (response.success) {
                    // eliminar elementos de la tabla
                    table.clear().draw();
                    //  agregar items a la tabla tbl
                    const items = response.items;
                    items.forEach(item => {
                        table.row.add([
                            item.numero,
                            `<a href="/OrderCubiq/Details/${item.orderCubiqId}" target="_blank">${item.orderNumber}</a>`,
                            item.clientFullName,
                            item.contactFullName,
                            item.contactCI,
                            item.contactPhone1,
                            item.contactPhone2,
                            item.descripcion,
                            item.pesoLb,
                            item.pesoKg,
                            item.precio,
                            item.alto,
                            item.ancho,
                            item.largo,
                            `<button class="btn btn-danger btn-sm delete-package" style="margin: 2px" data-numero="${item.numero}" data-id="${item.paqueteId}" title="Sacar paquete de la guia/contenedor"> <i class="fa fa-trash"></i> </button>` +
                            `<button class="btn btn-info btn-sm transfer-package" style="margin: 2px" data-numero="${item.numero}" data-id="${item.paqueteId}" title="Transferir a otra guia/contenedor"> <i class="fa fa-arrow-right"></i> </button>`
                        ]).draw(false);
                    });

                    toastr.success("El paquete ha sido escaneado");
                }
                else {
                    if (response.type == 'warning') {
                        toastr.warning(response.msg);
                    }
                    else {
                        toastr.error(response.msg);
                    }
                }
                $("#inputBarCode").val("");
                $.unblockUI();
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
        });
    }

    function addOrden(code, guiaId) {
        $.ajax({
            async: true,
            type: "POST",
            contentType: 'application/x-www-form-urlencoded',
            url: "/OrderCubiq/AddOrdersToGuia",
            data: {
                guideId: guiaId,
                numbers: [code]
            },
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                if (response.success) {
                    // eliminar elementos de la tabla
                    table.clear().draw();
                    //  agregar items a la tabla tbl
                    const items = response.items;
                    items.forEach(item => {
                        table.row.add([
                            item.numero,
                            `<a href="/OrderCubiq/Details/${item.orderCubiqId}" target="_blank">${item.orderNumber}</a>`,
                            item.clientFullName,
                            item.contactFullName,
                            item.contactCI,
                            item.contactPhone1,
                            item.contactPhone2,
                            item.descripcion,
                            item.pesoLb,
                            item.pesoKg,
                            item.precio,
                            item.alto,
                            item.ancho,
                            item.largo,
                            `<button class="btn btn-danger btn-sm delete-package" style="margin: 2px" data-numero="${item.numero}" data-id="${item.paqueteId}" title="Sacar paquete de la guia/contenedor"> <i class="fa fa-trash"></i> </button>` +
                            `<button class="btn btn-info btn-sm transfer-package" style="margin: 2px" data-numero="${item.numero}" data-id="${item.paqueteId}" title="Transferir a otra guia/contenedor"> <i class="fa fa-arrow-right"></i> </button>`
                        ]).draw(false);
                    });
                }
                else {
                    if (response.type == 'warning') {
                        toastr.warning(response.msg);
                    }
                    else {
                        toastr.error(response.msg);
                    }
                }
                $("#inputBarCode").val("");
                $.unblockUI();
            },
            error: function () {
                $("#inputBarCode").val("");
                toastr.error("No se han podido agregar los trámites a la guía", "Error");
                $.unblockUI();
            },
        });
    }

    function addPallet(code, guiaId) {
        $.ajax({
            async: true,
            type: "POST",
            contentType: 'application/x-www-form-urlencoded',
            url: "/OrderCubiq/AddPalletsToGuia",
            data: {
                guideId: guiaId,
                numbers: [code]
            },
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                if (response.success) {
                    // eliminar elementos de la tabla
                    table.clear().draw();
                    //  agregar items a la tabla tbl
                    const items = response.items;
                    items.forEach(item => {
                        table.row.add([
                            item.numero,
                            `<a href="/OrderCubiq/Details/${item.orderCubiqId}" target="_blank">${item.orderNumber}</a>`,
                            item.clientFullName,
                            item.contactFullName,
                            item.contactCI,
                            item.contactPhone1,
                            item.contactPhone2,
                            item.descripcion,
                            item.pesoLb,
                            item.pesoKg,
                            item.precio,
                            item.alto,
                            item.ancho,
                            item.largo,
                            `<button class="btn btn-danger btn-sm delete-package" style="margin: 2px" data-numero="${item.numero}" data-id="${item.paqueteId}" title="Sacar paquete de la guia/contenedor"> <i class="fa fa-trash"></i> </button>` +
                            `<button class="btn btn-info btn-sm transfer-package" style="margin: 2px" data-numero="${item.numero}" data-id="${item.paqueteId}" title="Transferir a otra guia/contenedor"> <i class="fa fa-arrow-right"></i> </button>`
                        ]).draw(false);
                    });
                }
                else {
                    if (response.type == 'warning') {
                        toastr.warning(response.msg);
                    }
                    else {
                        toastr.error(response.msg);
                    }
                }
                $("#inputBarCode").val("");
                $.unblockUI();
            },
            error: function () {
                $("#inputBarCode").val("");
                toastr.error("No se han podido agregar los trámites a la guía", "Error");
                $.unblockUI();
            },
        });
    }

    $('#btn-modaleditarguia').on('click', function () {
        var noGuia = $('#input-guidenumber').val();
        var agente = $('#input-agente').val();
        var smlu = $('#input-smlu').val();
        var seal = $('#input-seal').val();
        var cat = $('#input-cat').val();
        var manifiesto = $('#input-manifiesto').val();
        var fechaRecogida = $('#input-fecharecogida').val();
        var fechaViaje = $('#input-fechaviaje').val();
        var noVuelo = $('#input-novuelo').val();

        let data = {
            noGuia: noGuia,
            id: guiaId,
            agente: agente,
            smlu: smlu,
            seal: seal,
            cat: cat,
            manifiesto: manifiesto,
            fechaRecogida: fechaRecogida,
            fechaViaje: fechaViaje,
            noVuelo: noVuelo
        };

        $.ajax({
            async: true,
            type: "POST",
            contentType: 'application/x-www-form-urlencoded',
            url: "/OrderCubiq/EditGuia",
            data: data,
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                if (response.success) {
                    location.reload();
                }
                else {
                    toastr.error(response.msg);
                }

                $.unblockUI();
            },
            error: function () {
                toastr.error("No se han podido editar la guía", "Error");
                $.unblockUI();
            },
        });
    });

    $(document).on('click', '.delete-package', function () {
        var id = $(this).data('id');
        var numero = $(this).data('numero');
        const row = $(this).closest('tr');
        Swal.fire({
            title: '¿Estás seguro?',
            text: "Desea eliminar el paquete " + numero + " del pallet",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Sí, eliminarlo!',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    type: "DELETE",
                    url: "/OrderCubiq/DeletePackageOfGuia",
                    data: {
                        packageId: id
                    },
                    async: false,
                    success: function (res) {
                        if (res.success) {
                            // actualizar la tabla
                            table.row(row).remove().draw();
                            toastr.success("El paquete ha sido eliminado");

                        } else {
                            toastr.error(res.msg);
                        }
                    },
                    failure: function (response) {
                        showErrorMessage("ERROR", response.responseText);
                    },
                    error: function (response) {
                        showErrorMessage("ERROR", response.responseText);
                    },
                })
            }
        });
    })

    const tableCheck = $('#tblListado').DataTable({
        lengthChange: false,
        searching: false,
        paging: false,
    });

    $('#btnCheck').on('click', function () {
        const id = $(this).attr('data-guiaId');
        $.ajax({
            type: "GET",
            url: "/OrderCubiq/CheckGuia",
            data: {
                id: id
            },
            async: false,
            success: function (response) {
                if (response.success) {
                    // eliminar elementos de la tabla
                   
                    tableCheck.clear().draw();
                    //  agregar items a la tabla tbl
                    const items = response.data;
                    items.forEach(item => {
                        tableCheck.row.add([
                            item.agencia,
                            item.guia,
                            `<a href="/OrderCubiq/Details/${item.orderCubiqId}" target="_blank">${item.noOrden}</a>`,
                            item.noPaquete
                        ]).draw(false);
                    });
                    // mostrar en modalListado
                    $('#modalListado').modal('show');
                }
                else {
                    toastr.error(response.msg);
                }
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
        });
    })

    $(document).on('click', '.transfer-package', function () {
        const id = $(this).data('id');
        const number = $(this).data('numero');
        $('#package-transfer-id').val(id);
        $('#package-transfer-number').html(number);

        $('#modalTransferirPaquete').modal('show');
    });

    $('#btn-modalTransferPackage').on('click', function () {
        const guiaId = $('#select-guideTransfer').val();
        const packageId = $('#package-transfer-id').val();
        $.ajax({
            type: "POST",
            url: "/OrderCubiq/TransferPackageToAnotherGuia",
            data: {
                packageId: packageId,
                guideId: guiaId
            },
            async: false,
            success: function (response) {
                if (response.success) {
                    // eliminar elementos de la tabla
                    table.clear().draw();
                    //  agregar items a la tabla tbl
                    const items = response.items;
                    items.forEach(item => {
                        table.row.add([
                            item.numero,
                            `<a href="/OrderCubiq/Details/${item.orderCubiqId}" target="_blank">${item.orderNumber}</a>`,
                            item.clientFullName,
                            item.contactFullName,
                            item.contactCI,
                            item.contactPhone1,
                            item.contactPhone2,
                            item.descripcion,
                            item.pesoLb,
                            item.pesoKg,
                            item.precio,
                            item.alto,
                            item.ancho,
                            item.largo,
                            `<button class="btn btn-danger btn-sm delete-package" style="margin: 2px" data-numero="${item.numero}" data-id="${item.paqueteId}" title="Sacar paquete de la guia/contenedor"> <i class="fa fa-trash"></i> </button>` +
                            `<button class="btn btn-info btn-sm transfer-package" style="margin: 2px" data-numero="${item.numero}" data-id="${item.paqueteId}" title="Transferir a otra guia/contenedor"> <i class="fa fa-arrow-right"></i> </button>`
                        ]).draw(false);
                    });

                    toastr.success("El paquete ha sido transferido");
                }
                else {
                    if (response.type == 'warning') {
                        toastr.warning(response.msg);
                    }
                    else {
                        toastr.error(response.msg);
                    }
                }
                $("#inputBarCode").val("");
                $.unblockUI();
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
        });
    });

    $('#btn-modalactualizarEstado').on('click', function () {
        let formData = new FormData();
        let fileInput = $('#excelFile')[0].files[0];

        // Validar si se seleccionó un archivo
        if (!fileInput) {
            $('#response').text('Por favor, selecciona un archivo.');
            return;
        }

        formData.append('excelFile', fileInput);

        $.ajax({
            url: '/orderCubiq/UpdateDeliveryDateFromExcel', // URL del servidor que procesará el archivo
            type: 'POST',
            data: formData,
            processData: false, // Impedir que jQuery procese los datos
            contentType: false, // Impedir que jQuery configure el encabezado `Content-Type`
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                if (response.success) {
                    $('#recibidas').html(response.data.recibidas);
                    $('#transito').html(response.data.transito);
                    $('#entregadas').html(response.data.entregadas);

                    if (response.data.fails && response.data.fails.length > 0) {
                        $('#result_error').show();
                        const listItems = response.data.fails.map(item => `<li>${item}</li>`).join('');
                        $('#items').html(`<ul>${listItems}</ul>`);
                    }
                    else {
                        $('#result_error').hide();
                    }

                    $('#modalResultExcelEstado').modal('show');
                }
                else {
                    toastr.error(response.msg)
                }
                $.unblockUI();
            },
            error: function (xhr, status, error) {
                $('#response').text('Hubo un error al subir el archivo.');
                $.unblockUI();
            }
        });
    })
});