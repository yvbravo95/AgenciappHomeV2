$(document).on('ready', async function () {

    //--------------- FUNCTIONS ------------------

    function AddProductToTable(tableName, qty, description, id) {
        var tBody = $(`#${tableName} > TBODY`)[0];

        var row = tBody.insertRow(1);
        $(row).attr('data-id', id);

        var cell0 = $(row.insertCell(-1));
        cell0.append(qty);

        var cell1 = $(row.insertCell(-1));
        cell1.append(description);

        var cell2 = $(row.insertCell(-1));


        var btnEdit = $(
            "<button type='button' class='btn btn-warning' title='Editar' style='font-size: 10px'><i class='fa fa-pencil'></i></button>"
        );
        var btnRemove = $(
            "<button type='button' class='btn btn-danger pull-right' title='Eliminar' style='font-size: 10px'><i class=' fa fa-close'></button>"
        );
        var btnConfirm = $(
            "<button type='button' class='btn btn-success hidden' title='Confirmar' style='font-size: 10px'><i class='fa fa-check'></button>"
        );

        btnEdit.on('click', function () {
            cell0.html(`<input name='cell0' type="number" class='form-control' value='${cell0.html()}' />`);
            cell1.html(`<input name='cell1' type="text" class='form-control' value='${cell1.html()}' />`);

            btnConfirm.removeClass("hidden");
            btnEdit.addClass("hidden");
            btnRemove.addClass("hidden");
        });

        btnRemove.on('click', function () {
            row.remove();
        });

        btnConfirm.on('click', function () {
            cell0.html($("[name='cell0']").val());
            cell1.html($("[name='cell1']").val());
            btnConfirm.addClass("hidden");
            btnEdit.removeClass("hidden");
            btnRemove.removeClass("hidden");
        });

        cell2.append(btnEdit.add(btnRemove).add(btnConfirm));
    }

    function Save() {
        var data = new Array;
        var tables = $('[name="table"]');
        for (var i = 0; i < tables.length; i++) {
            table = tables[i];
            data[i] = {
                bag : $(table).attr('id'),
                items : new Array
            }
            var tbody = $(tables[i]).children()[1]
            for (var j = 1; j < tbody.rows.length; j++) {
                var row = tbody.rows[j];
                const qty = row.children[0].innerText;
                const description = row.children[1].innerText;
                const id = $(row).data('id');

                data[i].items.push({
                    qty : qty,
                    description: description,
                    id: id
                })

            }
        }

        $.ajax({
            type: "POST",
            url: "/OrderNew/EditBags?orderId=" + orderId,
            data: JSON.stringify(data),
            dataType: "json",
            contentType: "application/json",
            async: true,
            beforeSend: function () {
                $.blockUI();
            },
            success: function (data) {
                if (data.success)
                    location.href = "/airShipping/index?msg=Las bolsas fueron editadas correctamente."
                else
                    toastr.warning(data.msg);
                $.unblockUI();
            },
            failure: function (response) {
                console.log(response);
                $.unblockUI();
                toastr.error("Ha ocurrido un error");
            },
            error: function (response) {
                console.log(response);
                $.unblockUI();
                toastr.error("Ha ocurrido un error");
            },
        });
    }

    //--------------- ELEMENT ACTIONS ------------------

    $('[name="btnAdd"]').on('click', function () {
        const code = $(this).data("code");

        const childrens = $(this).parent().parent().children();

        const qty = childrens[0].firstChild.value;
        const description = childrens[1].firstChild.value;

        if (!description) {
            toastr.warning("La descripcion es obligatoria");
            return false;
        }

        if (!qty) {
            toastr.warning("La cantidad es obligatoria");
            return false;
        }

        AddProductToTable(code, qty, description);

        $('#newQty').val('1');
        $('#newDescription').val("");

        const items = $(this).parent().parent().children();
        items[0].children[0].value = 1;
        items[1].children[0].value = "";
    })

    $('#save').on('click', Save);

    //---------------- AJAX --------------------

    var data = await $.get("/OrderNew/GetBagsOrder?id=" + orderId);
    if (data) {
        for (var i = 0; i < data.length; i++) {
            var item = data[i];
            AddProductToTable(item.bagCode, item.qty, item.description, item.bagItemId);
        }
    }
})