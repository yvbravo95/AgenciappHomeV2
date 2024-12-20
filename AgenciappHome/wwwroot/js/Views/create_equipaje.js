$(document).ready(function () {
    $('#shippingType').on('change', function () {
        $("#no_equipaje").html($('#shippingType').val() + $("#time").html())
    });
})

productsShipping = new Array;
listOrdersComplete = new Array;

$(".select2-placeholder-selectCarrier").select2({
    placeholder: "Carrier",
    val: null
});

$(".select2-placeholder-orders").select2({
    placeholder: "Órdenes en estado inicida",
    val: null
});

$("#no_equipaje").html($('#shippingType').val() + $("#time").html());

//al seleccionar una orden nueva
$(".select2-placeholder-orders").change(function () {

    var orderid = $(this).val();
    var noOrden = "E" + $("#no_equipaje").html();
    $("#productsContainer").show();
    var tablaAdd = $("#tableAdd")
    tablaAdd.show();

    cantProducts = 0;

    //Eliminando todos los productos cargados en carga anterior
    $($("#tblProductos > TBODY")[0]).children().remove();

    $.ajax({
        type: "POST",
        url: "/Shippings/GetAllProductsOfAOrder/" + orderid,
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        async: false,
        success: function (data) {
            const colorRow = "#f1f1f1";
            let bag = "";
            for (var i = 0; i < data.items.length; i++) {

                var productArray = data.items[i];

                if (bag != productArray.bagCode)
                {
                    bag = productArray.bagCode;
                    //Add row for bag
                    const btnAdd = $("<button type='button' id='btnadd" + productArray["productId"] + "' data-bag='" + productArray.bagCode + "' class='btn btn-info btn-bagadd'><i class='fa fa-plus'></i></button>");

                    celdas = [
                        "",
                        "",
                        "",
                        "",
                        "",
                        btnAdd
                    ]
                    addCell("#tblProductos", celdas, undefined, colorRow)

                    btnAdd.on('click', function ()
                    {
                        $(this).attr("disabled", "disabled");
                        const code = $(this).data('bag')
                        $('[data-bag="' + code +'"]').each(function (i, e) {
                            $(e).val($(e).attr("max"));
                        });

                        $('[data-btnbag="' + code + '"]').each(function (i, e) {
                            if ($(e).attr("disabled") == undefined)
                                $(e).click();
                        });
                    })
                }

                var countinclude = countproductinlist(productArray["productId"], $(".select2-placeholder-orders option:selected").text().split(' ')[0]);

                //Creando una clase con las propiedades del producto
                var product = new Object();
                product.idProduct = productArray["productId"]
                product.idOrden = orderid;
                product.noOrden = noOrden;
                product.descripcion = productArray["description"]
                product.total = productArray.qty;
                product.empk = productArray.supplier;
                product.tipo = productArray["tipo"]
                product.color = productArray["color"]
                product.tallamarca = productArray["tallaMarca"]
                product.bagCode = productArray["bagCode"]

                const equipado = product.empk + countinclude
                const restante = product.total - equipado;

                if (product.total == product.empk)
                    var textSel = $("<input type='number' id='txtsel" + product.idProduct + i + "' data-valueselect='0' disabled class='form-control onecant' data-bag='" + product.bagCode + "'  style='width: 100px' value='" + restante + "' min='1' max='" + product.total + "'/>");
                else if (countinclude > 0 && countinclude == product.total)
                    var textSel = $("<input type='number' id='txtsel" + product.idProduct + i + "' data-valueselect='0' disabled class='form-control onecant' data-bag='" + product.bagCode + "'  style='width: 100px' value='" + restante + "' min='1' max='" + product.total + "'/>");
                else if (countinclude > 0 && countinclude != product.total)
                    var textSel = $("<input type='number' id='txtsel" + product.idProduct + i + "' data-valueselect='0' class='form-control onecant' data-bag='" + product.bagCode + "'  style='width: 100px' value='" + restante + "' min='1' max='" + (product.total - countinclude) + "'/>");
                else
                    var textSel = $("<input type='number' id='txtsel" + product.idProduct + i + "' data-valueselect='0' class='form-control onecant' data-bag='" + product.bagCode + "'  style='width: 100px' value='" + restante + "' min='1' max='" + product.total + "'/>");

                if (product.total == product.empk)
                    btnAdd = $("<button type='button' id='btnadd" + product.idProduct + i + "' data-productid='" + product.idProduct + "' data-btnbag='" + product.bagCode + "'  disabled class='btn btn-default btn-oneadd'><i class='fa fa-plus'></i></button>");
                else if (countinclude > 0 && countinclude == product.total)
                    btnAdd = $("<button type='button' id='btnadd" + product.idProduct + i + "' data-productid='" + product.idProduct + "' data-btnbag='" + product.bagCode + "'  disabled class='btn btn-default btn-oneadd'><i class='fa fa-plus'></i></button>");
                else
                    btnAdd = $("<button type='button' id='btnadd" + product.idProduct + i + "' data-productid='" + product.idProduct + "' data-btnbag='" + product.bagCode + "' data-name='btnadd" + i + "' class='btn btn-default btn-oneadd'><i class='fa fa-plus'></i></button>");

                celdas = [
                    textSel,
                    product.total,
                    product.empk + countinclude,
                    product.tipo,
                    product.bagCode,
                    btnAdd
                ]
                $("#peso").val(data.peso);
                $("#addAll").removeAttr('disabled');

                addCell("#tblProductos", celdas)

                var select = 0;
                textSel.on("change", function () {
                    $(this).data("valueselect", $(this).val())
                    select = $(this).data("valueselect");
                })

                loadeventclickaddproducts(product.idProduct, i)

                //unblock($(this))
            }
        },
        failure: function (response) {
            showErrorMessage("ERROR", response.responseText);
        },
        error: function (response) {
            showErrorMessage("ERROR", response.responseText);
        }
    });

    //Funcion para adicionar celdas a una tabla
    function addCell(tabla, celdas, productid, color) {
        //Obteniendo un punto de insercion
        var tBody = $(tabla + " > TBODY")[0];
        var index = $(tBody).children().length;

        //Adicionar fila
        var row = tBody.insertRow(index);
        if (productid != null)
            $(row).attr("data-lstproducts", productid)

        for (var i = 0; i < celdas.length; i++) {
            cell = $(row.insertCell(-1));
            cell.append(celdas[i])
        }

        if (color) $(row).css('background-color', color)
    }

    //Funcion que recarga el evento click del boton adicionar productos
    function loadeventclickaddproducts(productid, index) {
        $("#btnadd" + productid + index).on("click", function () {
            var productid = $(this).data("productid")
            var cell = $($(this).parent().parent());
            var equipado = $(cell.children()[2]);
            var ordernumber = $(".select2-placeholder-orders option:selected").text().split(' ')[0];
            var tipo = $(cell.children()[3]).html();
            var bagCode = $(cell.children()[4]).html();
            var cajasel = $("#txtsel" + productid + index);
            var sel = cajasel.val();
            var btnAdd = $(this)
            var empk = $(cell.children()[2]).html();
            var total = parseInt($(cell.children()[1]).html());

            empk = parseInt(empk) + parseInt(sel);
            if (empk > total) {
                toastr.warning("La cantidad seleccionada es mayor que la disponibilidad");
                return false;
            }
            else if (empk == total) {
                btnAdd.attr("disabled", "disabled")
                cajasel.attr("disabled", "disabled")
                equipado.html(empk)
            }
            else {
                var cell = $(btnAdd.parent().parent());
                equipado.html(empk)
                cajasel.val(1)
                cajasel.attr("max", parseInt(total) - empk);
            }

            btnRemove = $("<button type='button' data-ordernumber='" + ordernumber + "' data-productid='" + productid + "' class='btn btn-default btnremove" + productid + equipado.html() + "'><i class='fa fa-remove'></i></button>");
            var celdasNew = [
                ordernumber,
                sel,
                tipo,
                bagCode,
                btnRemove
            ]
            addCell("#productShipping", celdasNew, productid)
            loadeventclickremoveproducts(productid, index, equipado.html());
        })
    }

    //Funcion que recarga el evento click del boton eliminar producto
    function loadeventclickremoveproducts(productid, index, subindex) {
        $(".btnremove" + productid + subindex).on("click", function () {
            var ordernumber = $(this).data("ordernumber")
            var ordernumberactual = $(".select2-placeholder-orders option:selected").text().split(' ')[0];
            if (ordernumberactual == ordernumber) {
                var cellremove = $($(this).parent().parent());
                var cantidad = $(cellremove.children()[1]);
                var total = $($($("#txtsel" + productid + index).parent().parent()).children()[1])
                var equipado = $($($("#txtsel" + productid + index).parent().parent()).children()[2])
                equipado.html(parseInt(equipado.html()) - parseInt(cantidad.html()));
                //selector
                $("#txtsel" + productid + index).removeAttr("disabled")
                $("#btnadd" + productid + index).removeAttr("disabled")
                $("#txtsel" + productid + index).val(1)
                $("#txtsel" + productid + index).attr("max", parseInt(total.html()) - parseInt(equipado.html()));
                cellremove.remove()
            }
            else {
                var cellremove = $($(this).parent().parent());
                cellremove.remove()
            }
        })

    }

    //Funcion que contabiliza la cantidad de productos que hay en una lista, si no hay ninguno devuelve 0
    function countproductinlist(productid, ordernumber) {
        var count = 0;
        $("tr[data-lstproducts]").each(function () {
            if ($(this).data("lstproducts") == productid) {
                if ($($(this).children()[0]).html() == ordernumber) {
                    count += parseInt($($(this).children()[1]).html())
                }
            }
        })
        return count;
    }


})

$("#addAll").click(function () {
    $(this).attr("disabled", "disabled");

    $("input.onecant").each(function (i, e) {
        $(e).val($(e).attr("max"));
    });

    $("button.btn-oneadd").each(function (i, e) {
        if ($(e).attr("disabled") == undefined)
            $(e).click();
    });
    var peso = parseFloat($("#peso_actual").val());
    peso += parseFloat($("#peso").val());
    $("#peso_actual").val(peso);
});

var isSend = false;

$("#saveShipping").click(async function () {
    if (validateShipping() && !isSend) {
        isSend = true;
        var listProduct = new Array;

        var tBody = $("#productShipping > TBODY")[0];

        //Intro la cantidad de ordenes
        $("tr[data-lstproducts]").each(function () {
            var orderid = $($(this).children()[0]).html()
            if (!listOrdersComplete.includes(orderid)) {
                listOrdersComplete.push(orderid);
            }
        })

        for (var i = 0; i < tBody.rows.length; i++) {
            var fila = tBody.rows[i];
            for (var j = 0; j < 6; j++) {
                var aux = [];
                for (var j = 0; j < 6; j++) {
                    if (j == 2) {
                        aux[j] = $(fila).data('lstproducts');
                    }
                    else
                        aux[j] = $(fila.children[j]).html();
                }
                listProduct.push({
                    OrderNumber: aux[0],
                    Qty: aux[1],
                    ProductId: aux[2]
                });
            }
        }

        var data = {
            Type: $("#shippingType").val(),
            Number: "E" + $("#no_equipaje").html(),
            CarrierId: $(".select2-placeholder-selectCarrier").val(),
            Peso: $("#peso_actual").val().replace(",", "."),
            Nota: $("#Nota").val(),
            Products: listProduct,
            OrdersComplete: listOrdersComplete
        }

        $.ajax({
            type: "POST",
            url: "/Shippings/CreateShippingV2",
            data: data,
            contentType: "application/x-www-form-urlencoded",
            async: true,
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                if (response.success)
                    document.location = "/Shippings?msg=success&noEquipaje=E" + $("#no_equipaje").html();
                else
                    toastr.info(response.msg);
                $.unblockUI();
            },
            failure: function (response) {
                $.unblockUI();
                console.log(response.responseText);
                isSend = false;
                showErrorMessage("Error", "Ha ocurrido un error");
            },
            error: function (response) {
                $.unblockUI();
                console.log(response.responseText);
                isSend = false;
                showErrorMessage("Error", "Ha ocurrido un error");
            }
        });
    }
});

var validateShipping = function () {
    var tBody = $("#productShipping > TBODY")[0];

    if (tBody.rows.length == 0) {
        showWarningMessage("Atención", "El equipaje debe tener al menos un producto.");
        return false;
    }

    return true;
}

function block($this) {
    var block_ele = $this.closest('.card');
    block_ele.block({
        message: '<div id="load" class="ft-refresh-cw icon-spin font-medium-2"></div>',
        overlayCSS: {
            backgroundColor: '#FFF',
            cursor: 'wait',
        },
        css: {
            border: 0,
            padding: 0,
            backgroundColor: 'none'
        }
    });
}

function unblock($this) {
    var block_ele = $this.closest('.card');
    block_ele.unblock()
}