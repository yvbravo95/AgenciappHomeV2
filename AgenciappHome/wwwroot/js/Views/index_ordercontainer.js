let pageSize = 10;
let actualPage = 1;
let selected = [];
let search = $('#search').val();
$('#import-excel').on('click', function () {
    $('#modalImport').modal('show');
});

$('.order-select').on('change', orderSelect)

$('#checkAll').on('change', function () {
    if ($(this).is(':checked')) {
        $('.order-select').prop('checked', true)
    } else {
        $('.order-select').prop('checked', false)
    }
    orderSelect();
})

$('#btn_import').on('click', function () {
    var fdata = new FormData();
    var fileUpload = $("#fUpload").get(0);
    var files = fileUpload.files;
    fdata.append(files[0].name, files[0]);
    const afiliadoId = $('#afiliadoId').val();
    if (!afiliadoId) {
        toastr.error('Debe seleccionar un afiliado');
        return;
    }
    fdata.append('afiliadoId', afiliadoId);

    $.ajax({
        type: "POST",
        url: "/OrderContainer/ImportExcel",
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
        async: true,

        success: function (response) {
            if (response.success) {
                location.reload();
            }
            else {
                showErrorMessage("ERROR", response.message);
            }
            $.unblockUI();
        },
        error: function (e) {
            showErrorMessage("ERROR", e.responseText);
        }
    });
})

$('#search').on('keyup', function (event) {
    // enter key
    if (event.keyCode === 13) {
        search = $(this).val();
        reload();
    }
})

$('#btn-distribuir').on('click', function ()
{
    $('#modalDistribuir').modal('show');
});

$('#btn_distribuir').on('click', function () {
    const distributorId = $('#distributorId').val();
    if (!distributorId) {
        toastr.error('Debe seleccionar un afiliado');
        return;
    }
    console.log(selected)
    $.ajax({
        type: "POST",
        url: "/OrderContainer/DistributeOrder",
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
        data: {
            distributorId: distributorId,
            orderIds: selected
        },
        success: function (response) {
            if (response.success) {
                location.reload();
            }
            else {
                showErrorMessage("ERROR", response.message);
            }
            $.unblockUI();
        },
        error: function (e) {
            showErrorMessage("ERROR", e.responseText);
        }
    });
})

$('.change-status').on('click', function () {
    $('#input-id-ref').val($(this).data('id'))
    $('#modalChangeStatus').modal('show');
})

$('#btn-change-status-confirm').on('click', changeStatus)

function loadPage(page) {
    actualPage = page;
    reload();
}

function reload() {
    location.href = `/OrderContainer/Index?page=${actualPage}&pagesize=${pageSize}&search=${search}`;
}

function orderSelect() {
    // obtener check marcados
    selected = [];
    $('.order-select:checked').each(function () {

        selected.push($(this).data('id'));
    });

    if (selected.length > 0) {
        $('#btn-distribuir').show();
    }
    else {
        $('#btn-distribuir').hide();
    }
}

function changeStatus() {
    const id = $('#input-id-ref').val();
    const status = $('#select-change-status').val();
$.ajax({
        type: "POST",
        url: "/OrderContainer/ChangeStatus",
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
        data: {
            id: id,
            status: status
        },
        success: function (response) {
            if (response.success) {
                location.reload();
            }
            else {
                showErrorMessage("ERROR", response.message);
            }
            $.unblockUI();
        },
        error: function (e) {
            showErrorMessage("ERROR", e.responseText);
        }
    });
}