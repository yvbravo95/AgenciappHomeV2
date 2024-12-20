$(document).ready(function () {
    $("#inputBarCode").on("change", async function () {
        var code = $(this).val();
        if (!code) return;

        $.ajax({
            type: "POST",
            url: "/OrderCubiq/ScanPackage",
            data: {
                number: code.trim(),
                predespachoId: predespachoId
            },
            async: false,
            success: function (res) {
                if (res.success) {
                    const packageId = res.packageId;
                    const item = res.item;

                    if (packageId && !item) {
                        // buscar fila del item en la tabla tbl por id y colorear fila
                        var row = $("#tbl tbody tr").filter(function () {
                            return $(this).data("id") == packageId;
                        });
                        row.css("background-color", "lightgray");
                    }
                    else if (item) {
                        var row = `<tr>
                                        <td>
                                             <label class="custom-control custom-checkbox">
                                                    <input type="checkbox" class="custom-control-input checkbox" data-id="${item.paqueteId}" checked />
                                                    <span class="custom-control-indicator"></span>
                                                    <span class="custom-control-description"></span>
                                              </label>
                                        </td>
                                        <td><a href="/OrderCubiq/Details/${item.orderId}">${item.orderNumber}</a></td>
                                        <td>${item.paqueteNumber}</td>
                                        <td>${item.description}</td>
                                   </tr>`;
                        $("#tbl-scaned tbody").append(row);
                    }

                    toastr.success("El paquete ha sido escaneado");
                } else {
                    toastr.error(res.msg);
                }

                $("#inputBarCode").val("");
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
        });
    });

    $('#btn-predespacho').on('click', function () {
        $.ajax({
            async: true,
            type: "GET",
            contentType: "application/x-www-form-urlencoded",
            url: "/OrderCubiq/PdfPreDespacho",
            data: {
                id: predespachoId
            },
            success: function (response) {

                fileName = 'document.pdf';
                var byteCharacters = atob(response);
                var byteNumber = new Array(byteCharacters.length);
                for (var i = 0; i < byteCharacters.length; i++) {
                    byteNumber[i] = byteCharacters.charCodeAt(i);
                }
                var byteArray = new Uint8Array(byteNumber);
                var blob = new Blob([byteArray], { type: 'application/pdf' });
                var fileURL = URL.createObjectURL(blob);
                window.open(fileURL);
            },
            error: function () {
                toastr.error("No se ha podido exportar", "Error");
            }
        });
    })

    $('#btn-almacenusa').on('click', function () {
        // swal para confirmar
        swal({
            title: "¿Estás seguro?",
            text: "Se enviará el predespacho a Almacen USA",
            icon: "warning",
            buttons: true,
            dangerMode: true,
            showCancelButton: true,
            closeOnConfirm: false,
            showLoaderOnConfirm: true,
        },
            function () {
                $.ajax({
                    async: true,
                    type: "POST",
                    contentType: "application/x-www-form-urlencoded",
                    url: "/OrderCubiq/PreDespachoToAlmacenUsa",
                    data: {
                        predespachoId: predespachoId
                    },
                    success: function (response) {
                        if (response.success) {
                            toastr.success("El predespacho ha sido enviado a Almacen USA");
                            setTimeout(function () {
                                window.location.href = "/OrderCubiq/Index?type=" + type;
                            }, 2000);
                        } else {
                            toastr.error(response.msg);
                            swal.close();
                        }
                    },
                    error: function () {
                        toastr.error("No se ha podido enviar a Almacen USA", "Error");
                        swal.close();
                    }
                });
            }
        );
    })

    $('#btnaddpredespacho').on('click', function () {
        // obtener los ids de los items seleccionados
        var items = [];
        $(".checkbox").each(function () {
            if ($(this).is(":checked")) {
                items.push($(this).data("id"));
            }
        });

        if (items.length == 0) {
            toastr.error("Debe seleccionar al menos un paquete");
            return;
        }

        // swal para confirmar
        swal({
            title: "¿Estás seguro?",
            text: "Se agregarán los paquetes al predespacho",
            icon: "warning",
            buttons: true,
            dangerMode: true,
            showCancelButton: true,
            closeOnConfirm: false,
            showLoaderOnConfirm: true,
        },
            function () {
                $.ajax({
                    async: true,
                    type: "POST",
                    contentType: "application/x-www-form-urlencoded",
                    url: "/OrderCubiq/MovePackagesToPreDespacho",
                    data: {
                        predespachoId: predespachoId,
                        ids: items
                    },
                    success: function (response) {
                        if (response.success) {
                            toastr.success("Los paquetes han sido agregados al predespacho");
                            setTimeout(function () {
                                window.location.href = "/OrderCubiq/PreDespachoDetails/" + predespachoId;
                            }, 2000);
                        } else {
                            toastr.error(response.msg);
                            swal.close();
                        }
                    },
                    error: function () {
                        toastr.error("No se han podido agregar los paquetes al predespacho", "Error");
                        swal.close();
                    }
                });
            }
        );
    })

    $('#btn-facturar').on('click', function () {
        // swal para confirmar
        swal({
            title: "¿Estás seguro?",
            text: "Desea crear una factura de este pre despacho ?",
            icon: "warning",
            buttons: true,
            dangerMode: true,
            showCancelButton: true,
            closeOnConfirm: false,
            showLoaderOnConfirm: true,
        },
            function () {
                $.ajax({
                    async: true,
                    type: "POST",
                    contentType: "application/x-www-form-urlencoded",
                    url: "/Facturas/CreateBillOfPreDespacho",
                    data: {
                        preDespachoId: predespachoId
                    },
                    success: function (response) {
                        if (response.success) {
                            toastr.success("El predespacho ha sido facturado");
                            setTimeout(function () {
                                window.open("/Facturas/Details/" + response.facturaId, '_blank');
                            }
                            , 2000);
                        } else {
                            toastr.error(response.msg);
                            
                        }
                        swal.close();
                    },
                    error: function () {
                        toastr.error("No se ha podido facturar el predespacho", "Error");
                        swal.close();
                    }
                });
            }
        );
    })

    $('#btn-viewfactura').on('click', function () {
        location.href = "/Facturas/Details/" + facturaId;
    });

    $('#btnAddToGuia').on('click', function () {

        var guiaId = $('#selectedGuia2').val();
        if (!guiaId) {
            toastr.error("Seleccione una guía", "Error");
            return;
        }

        var data = {
            guideId: guiaId,
            predespachoId: predespachoId
        };

        $.ajax({
            async: true,
            type: "POST",
            contentType: 'application/x-www-form-urlencoded',
            url: "/OrderCubiq/AddPreDespachoToGuia",
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
                toastr.error("No se han podido agregar los trámites a la guía", "Error");
                $.unblockUI();
            },
        });
    })
})