
$('#btn-confirm').on('click', function () {
    confirmationMsg("Confirmar Equipajes", "¿Está seguro que desea confirmar estos equipajes?", confirm)
})

$('#btn-cancel').on('click', function () {
    location.href = "/Shippings/Index";
})

function confirm() {
    var tBody = $("#tbl > TBODY")[0];
    var data = new Array;
    for (var i = 0; i < tBody.rows.length; i++) {
        var fila = tBody.rows[i];
        const col_check = fila.children[0];
        const check_input = $(col_check).find('input')
        const is_checked = $(check_input).is(':checked')
        const col_packing = fila.children[1];
        const col_order = fila.children[2]
        const col_bag = fila.children[3];
        const col_desc = fila.children[4];

        data.push({
            id: fila.dataset['id'],
            isChecked: is_checked,
            packingNumber: col_packing.innerHTML,
            bagNumber: $(col_bag).data('value'),
            description: col_desc.children[0].value
        })
    }

    saveData(data);
}

function saveData(data) {
    $.ajax({
        type: "POST",
        url: "/Shippings/CheckEquipaje",
        data: JSON.stringify(data),
        dataType: "json",
        contentType: "application/json",
        async: true,
        beforeSend: function () {
            $.blockUI();
        },
        success: function (data) {
            if (data.success)
                location.href = "/Shippings/Index?msg=" + data.message;
            else {
                console.log(data);
                toastr.error("No se han podido revisar los equipajes");
            }

            $.unblockUI();
        },
        failure: function (response) {
            $.unblockUI();
            console.log(response.responseText)
            showErrorMessage("ERROR", "Ha ocurrido un error");
        },
        error: function (response) {
            $.unblockUI();
            console.log(response.responseText)
            showErrorMessage("ERROR", "Ha ocurrido un error");
        },
    });
}