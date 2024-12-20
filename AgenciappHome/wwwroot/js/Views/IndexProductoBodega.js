$(document).ready(function () {
    var cantSelect = 0;
    var selectedIds = new Array;

    $(".select2-multiple").select2({
    });

    $(".order-select").on("change", function () {
        if ($(this)[0].checked) {
            cantSelect++;
            selectedIds.push($(this).val());
        } else {
            cantSelect--;
            selectedIds.splice($.inArray($(this).val(), selectedIds), 1);
        }
        if (cantSelect == 0) {
            $('#AddMinoristas').hide();
        } else {
            $('#AddMinoristas').show();
        }
    });

    $('[name="selectAll"]').on('change', function () {
        if ($(this)[0].checked) {
            selectedIds = new Array;
            cantSelect = 0;
            $(".order-select").prop('checked', true)
            $(".order-select").each(function (i, e) {
                if (e.checked) {
                    cantSelect++;
                    selectedIds.push($(e).val());
                }
            });
        }
        else {
            $(".order-select").prop('checked', false)
            cantSelect = 0;
            selectedIds = new Array;
        }

        if (cantSelect == 0) {
            $('#AddMinoristas').hide();
        }
        else {
            $('#AddMinoristas').show();
        }
    });

    $('#AddMinoristas').on('click', function () {
        $('#modalAddMinoristas').modal('show');
    });

    $(document).on('click', '[name="tab"]',function(){
        $('[name="tab"]').removeClass('active');
        $(this).addClass('active');
    });

    $('#btnAsignar').on('click', function () {
        var data = new Object({
            ids: selectedIds,
            agenciasprecio1: $('#agenciasprecio1').val(),
            agenciasprecio2: $('#agenciasprecio2').val(),
            agenciasprecio3: $('#agenciasprecio3').val(),
        });

        $.ajax({
            async: true,
            url: "/ProductoBodega/AsignarMinoristas",
            data: data,
            type: "POST",
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                $.unblockUI();
                if (response.success)
                    toastr.success(response.msg);
                else
                    toastr.error(response.msg);
            },
            error: function (response) {
                $.unblockUI();
                toastr.error("Ha ocurrido un error.")
            }
        });
    });

    $('[name="order-btn"]').on('click', function(){
        var input = $(this).parent().children()[0];
        $.ajax({
            async: true,
            url: "/ProductoBodega/UpdateOrderProduct",
            type: "POST",
            data: {
                id: $(input).attr('data-id'),
                value: $(input).val()
            },
            beforeSend: function(){
                $.blockUI();
            },
            success: function(response){
                if(!response.success){
                    toastr.error(response.msg);
                }
                else{
                    toastr.success(response.msg);
                }

                $.unblockUI();
            },
            error: function(e){
                $.unblockUI();
                toastr.error("No se ha podido establecer el orden");
            }
        })
    })

    $('#gen_reportFecha').on('click', function () {
        $('#modalReporte').modal('show');
    });
    $('#exportReporteAccept').on('click', function () {
        $('#modalReporte').modal('hide');
    })

    $("#exportReporteAccept").click(function () {
        var date = $('#daterangeReport').val();
        var datesplit = date.split('-');
        var start = datesplit[0];
        var end = datesplit[1];

        fetch(`/Bodega/GetReport?start=${start}&end=${end}`)
            .then((resp) => resp.blob())
            .then((blob) => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.style.display = 'none';
                a.href = url;
                const filename = "report.xlsx";
                a.download = filename;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
            })
            .catch(() => console.log('oh no!'));
    });

    $('#importProduct').on('click', function () {
        $('#modalImport').modal('show');
    });

    $('#importProductAccept').on('click', function () {
        var fdata = new FormData();
        var fileUpload = $("#fUpload").get(0);
        var files = fileUpload.files;
        fdata.append(files[0].name, files[0]);
        $.ajax({
            async: true,
            type: "POST",
            url: "/ProductoBodega/ImportExcel",
            beforeSend: function (xhr) {
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
            async: false,

            success: function (response) {
                console.log(response);
                if(response.length == 0) {
                    toastr.success("Los productos se han importado correctamente");
                }
                else{
                    showErrorMessage("ERROR", `${response.length} productos no se han importado correctamente.`);
                }
                
                $.unblockUI();
            },
            error: function (e) {
                showErrorMessage("ERROR", e.responseText);
            }
        });
    });

});