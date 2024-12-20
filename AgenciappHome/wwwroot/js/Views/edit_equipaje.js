$(document).ready(function () {
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
        block_ele.unblock();
    }

    $(".select2-placeholder-selectCarrier").select2({
        placeholder: "Carrier",
        val: null
    });

    $(".select2-placeholder-orders").select2({
        placeholder: "Órdenes en estado inicida",
        val: null
    });

    $(".select2-container--default").attr("style", "width: 100%;");

    $("#shippingType").select2({
        placeholder: "Tipo de Equipaje",
        text: " "
    });

    /***********DATOS DEL EQUIPAJE *************/

    productsDelete = [];
    productsShipping = [];
    listOrdersComplete = [];

    $('#shippingType').on('change', function () {
        var type = $("#no_equipaje").html().substring(0,3);
        $("#no_equipaje").html($("#no_equipaje").html().replace(type,"E" + $('#shippingType').val() ));           
    });

    //al seleccionar una orden nueva
    $("#selectOrder").on("change", function () {
        var orderid = $(this).val();

        //Eliminando todos los productos cargados en carga anterior
        $($("#tblProductos > TBODY")[0]).children().remove();

        var id = orderid.split(" ");
        if (id[1] != "" && id[1] != undefined) {
            var productid = id[0];            
            var ordernumber = id[1];

            var item;
            for (var i = 0; i < productsDelete.length; i++) {
                if (productsDelete[i][1] == productid && productsDelete[i][0] == ordernumber) {
                    item = productsDelete[i];
                    break;
                }
            }
            var tipo = item[3];
            var qty = item[2];
            
            btnRemove = $("<button type='button' class='btn btn-default btnremove'><i class='fa fa-remove'></i></button>");

            var celdasNew = [
                ordernumber,
                qty,
                tipo,
                btnRemove
            ];

            var tBody = $("#productShipping  > TBODY")[0];
            var index = $(tBody).children().length - 1;

            //Adicionar fila
            var row = tBody.insertRow(index);
            for (var i = 0; i < celdasNew.length; i++) {
                cell = $(row.insertCell(-1));
                cell.append(celdasNew[i]);
            }

            $(btnRemove).on("click", function () {
                var ordernumber = item[0];
                var productId = item[1]
                var productQty = item[2]
                var productName = item[3]

                productsDelete.push([ordernumber, productId, productQty, productName]);

                var cellremove = $($(this).parent().parent());
                cellremove.remove();

                $("#selectOrder").append(new Option(ordernumber + " " + productName + " (Elimindado)", productId + " " + ordernumber))
            });
            $("#selectOrder option:selected").remove();
            productsDelete.pop(item);
        }
        else {
            var noOrden = "E" + $("#no_equipaje").html();
            $("#productsContainer").show();
            var tablaAdd = $("#tableAdd");
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
                    for (var i = 0; i < data.items.length; i++) {
                        var productArray = data.items[i];

                        var countinclude = countproductinlist(productArray.productId, $(".select2-placeholder-orders option:selected").text().split(' ')[0]);

                        //Creando una clase con las propiedades del producto
                        var product = {
                            idProduct: productArray.productId,
                            idOrden: orderid,
                            noOrden: noOrden,
                            total: productArray.qty,
                            empk: productArray.supplier,
                            tipo: productArray.tipo
                        };
                        var textSel = "";
                        if (product.total == product.empk)
                            textSel = $("<input type='number' id='txtsel" + product.idProduct + i + "' data-valueselect='0' disabled class='form-control onecant'  style='width: 100px' value='0' min='1' max='" + product.total + "'/>");
                        else if (countinclude > 0 && countinclude == product.total)
                            textSel = $("<input type='number' id='txtsel" + product.idProduct + i + "' data-valueselect='0' disabled class='form-control onecant'  style='width: 100px' value='0' min='1' max='" + product.total + "'/>");
                        else if (countinclude > 0 && countinclude != product.total)
                            textSel = $("<input type='number' id='txtsel" + product.idProduct + i + "' data-valueselect='0' class='form-control onecant'  style='width: 100px' value='1' min='1' max='" + (product.total - countinclude) + "'/>");
                        else
                            textSel = $("<input type='number' id='txtsel" + product.idProduct + i + "' data-valueselect='0' class='form-control onecant'  style='width: 100px' value='1' min='1' max='" + product.total + "'/>");

                        if (product.total == product.empk)
                            btnAdd = $("<button type='button' id='btnadd" + product.idProduct + i + "' data-productid='" + product.idProduct + "'  disabled class='btn btn-default btn-oneadd'><i class='fa fa-plus'></i></button>");
                        else if (countinclude > 0 && countinclude == product.total)
                            btnAdd = $("<button type='button' id='btnadd" + product.idProduct + i + "' data-productid='" + product.idProduct + "'  disabled class='btn btn-default btn-oneadd'><i class='fa fa-plus'></i></button>");
                        else
                            btnAdd = $("<button type='button' id='btnadd" + product.idProduct + i + "' data-productid='" + product.idProduct + "' data-name='btnadd" + i + "' class='btn btn-default btn-oneadd'><i class='fa fa-plus'></i></button>");

                        celdas = [
                            textSel,
                            product.total,
                            product.empk + countinclude,
                            product.tipo,
                            btnAdd
                        ];
                        $("#peso").val(data.peso);
                        $("#addAll").removeAttr('disabled');
                        addCell("#tblProductos", celdas);

                        textSel.on("change", function () {
                            $(this).data("valueselect", $(this).val());
                        });

                        loadeventclickaddproducts(product.idProduct, i);
                    }
                },
                failure: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                },
                error: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                }
            });

            function addCell(tabla, celdas, productid) {
                //Obteniendo un punto de insercion
                var tBody = $(tabla + " > TBODY")[0];
                var index = $(tBody).children().length - 1;

                //Adicionar fila
                var row = tBody.insertRow(index);
                if (productid != null)
                    $(row).attr("data-lstproducts", productid);

                for (var i = 0; i < celdas.length; i++) {
                    cell = $(row.insertCell(-1));
                    cell.append(celdas[i]);
                }
            }

            //Funcion que recarga el evento click del boton adicionar productos
            function loadeventclickaddproducts(productid, index) {
                $("#btnadd" + productid + index).on("click", function () {
                    var productid = $(this).data("productid");
                    var cell = $($(this).parent().parent());
                    var equipado = $(cell.children()[2]);
                    var ordernumber = $(".select2-placeholder-orders option:selected").text().split(' ')[0];
                    var tipo = $(cell.children()[3]).html();
                    var cajasel = $("#txtsel" + productid + index);
                    var sel = cajasel.val();
                    var btnAdd = $(this);
                    var empk = $(cell.children()[2]).html();
                    var total = $(cell.children()[1]).html();

                    empk = parseInt(empk) + parseInt(sel);
                    if (empk == total) {
                        btnAdd.attr("disabled", "disabled");
                        cajasel.attr("disabled", "disabled");
                        equipado.html(empk);
                    }
                    else {
                        cell = $(btnAdd.parent().parent());
                        equipado.html(empk);
                        cajasel.val(1);
                        cajasel.attr("max", parseInt(total) - empk);
                    }

                    btnRemove = $("<button type='button' data-ordernumber='" + ordernumber + "' data-productid='" + productid + "' class='btn btn-default btnremove" + productid + equipado.html() + "'><i class='fa fa-remove'></i></button>");
                    var celdasNew = [
                        ordernumber,
                        sel,
                        tipo,
                        btnRemove
                    ];
                    addCell("#productShipping", celdasNew, productid);
                    loadeventclickremoveproducts(productid, index, equipado.html());
                });
            }

            //Funcion que recarga el evento click del boton eliminar producto
            function loadeventclickremoveproducts(productid, index, subindex) {
                $(".btnremove" + productid + subindex).on("click", function () {
                    var ordernumber = $(this).data("ordernumber");
                    var ordernumberactual = $(".select2-placeholder-orders option:selected").text().split(' ')[0];
                    var cellremove = $($(this).parent().parent());
                    if (ordernumberactual == ordernumber) {
                        var cantidad = $(cellremove.children()[1]);
                        var total = $($($("#txtsel" + productid + index).parent().parent()).children()[1]);
                        var equipado = $($($("#txtsel" + productid + index).parent().parent()).children()[2]);
                        equipado.html(parseInt(equipado.html()) - parseInt(cantidad.html()));
                        //selector
                        $("#txtsel" + productid + index).removeAttr("disabled");
                        $("#btnadd" + productid + index).removeAttr("disabled");
                        $("#txtsel" + productid + index).val(1);
                        $("#txtsel" + productid + index).attr("max", parseInt(total.html()) - parseInt(equipado.html()));
                    }
                    cellremove.remove();
                });
            }

            //Funcion que contabiliza la cantidad de productos que hay en una lista, si no hay ninguno devuelve 0
            function countproductinlist(productid, ordernumber) {
                var count = 0;
                $("tr[data-lstproducts]").each(function () {
                    if ($(this).data("lstproducts") == productid) {
                        if ($($(this).children()[0]).html() == ordernumber) {
                            count += parseInt($($(this).children()[1]).html());
                        }
                    }
                });
                return count;
            }
        }        
    });

    $("#saveShipping").click(function () {
        blockUI();
        if (validateShipping()) {
            var listProduct = [];

            var tBody = $("#productShipping > TBODY")[0];

            //Intro la cantidad de ordenes
            $("tr[data-lstproducts]").each(function () {
                var orderid = $($(this).children()[0]).html();
                if (!listOrdersComplete.includes(orderid)) {
                    listOrdersComplete.push(orderid);
                }
            });            

            for (var i = 0; i < tBody.rows.length; i++) {
                var fila = tBody.rows[i];
                if ($(fila).data('lstproducts') != undefined) {
                    var aux = [];
                    for (var j = 0; j < 6; j++) {
                        if (j == 2) {
                            aux[j] = $(fila).data('lstproducts');
                        }
                        else
                            aux[j] = $(fila.children[j]).html();
                    }
                    listProduct.push(aux);
                }
                
            }

            var datosShipping = [
                $("#PackingId").val(),    //0
                $("#shippingType").val(),  //1
                $("#no_equipaje").html(),  //2
                $(".select2-placeholder-selectCarrier").val(), //3
                listProduct,  //4
                listOrdersComplete,  //5
                productsDelete,  //6
                $("#peso_actual").val().replace(",", "."), //7
                $("#no_vuelo").val(), //8
                $("#fecha").val() //9
            ];

            $.ajax({
                type: "POST",
                url: "/Shippings/Edit",
                data: JSON.stringify(datosShipping),
                dataType: 'json',
                contentType: 'application/json',
                async: true,
                success: function () {
                    document.location = "/Shippings?msg=success&noEquipaje=E" + $("#no_equipaje").html();
                },
                failure: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                    $.unblockUI();
                },
                error: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                    $.unblockUI();
                }
            });
        }
    });

    function blockUI() {
        $.blockUI({
            message: '<div class="semibold" style="text-size:20px;"><span class="ft-refresh-cw icon-spin text-left"></span>&nbsp; Cargando Información ...</div>',
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
    var validateShipping = function () {
        var tBody = $("#productShipping > TBODY")[0];

        if (tBody.rows.length == 0 && t.rows().length == 0) {
            showWarningMessage("Atención", "El equipaje debe tener al menos un producto.");
            return false;
        }

        return true;
    };
   
    $("#addAll").click(function () {
        $(this).attr("disabled", "disabled");

        $("input.onecant").each(function (i, e) {
            $(e).val($(e).attr("max"));
        });

        $("button.btn-oneadd").each(function (i,e) {
            if ($(e).attr("disabled") == undefined)
                $(e).click();
        });
        var peso = parseFloat($("#peso_actual").val());
        peso += parseFloat($("#peso").val());
        $("#peso_actual").val(peso);
    });

    //$(".btnremove").on("click", function () {
    //    var ordernumber = $(this).data("ordernumber");
    //    var productId = $(this).data("productid");
    //    var productQty = $(this).data("productqty");
    //    var shipinItemId = $(this).data("shipinItemId");

    //    productsDelete.push([ordernumber, productId, productQty, shipinItemId]);

    //    var cellremove = $($(this).parent().parent());
    //    cellremove.remove();

    //    //$("#selectOrder").append(new Option(ordernumber + " " + productName + " (Eliminado)", productId + " " + ordernumber))
    //});
    //var t1 = $("#productShipping").DataTable({})
    var t = $("#productShipping1").DataTable({
        columnDefs: [
            {
                visible: false,
                targets: 0
            }
        ],
        order: [[0, 'asc']],
        paginate: false,
        changeLength:false,
        drawCallback: function (settings) {
            var api = this.api();
            var rows = api.rows({ page: 'current' }).nodes();
            var last = null;
            api.column(0, { page: 'current' })
                .data().each(function (group, i) {
                    if (last !== group) {
                        var r = $('<tr class="group"><td colspan="2">' + group + '</td><td><button type="button" class="btn btn-outline-primary"><i class="fa fa-remove"></i></button></td ></tr > ');
                        $(rows).eq(i).before(r);
                        last = group;
                        r.on("click", function () {
                            var ordernumber = group;
                            $("button.btnremove").each(function (i, e) {
                                var rordernumber = $(e).data("ordernumber");
                                var bagcode = $(e).data("bagcode");
                                if (bagcode == ordernumber) {
                                    var productId = $(e).data("productid");
                                    var productQty = $(e).data("productqty");
                                    var shipinItemId = $(e).data("shipinitemid");

                                    productsDelete.push([rordernumber, productId, productQty, shipinItemId]);
                                    t.row($(e).parent()).remove();
                                    t.draw();
                                }
                            })
                        })
                    }
                });
        }
    });

    $(document).on("click", ".btnremove", function () {
        var ordernumber = $(this).data("ordernumber");
        var productId = $(this).data("productid");
        var productQty = $(this).data("productqty");
        var shipinItemId = $(this).data("shipinitemid");

        productsDelete.push([ordernumber, productId, productQty, shipinItemId]);
        t.row($(this).parent()).remove();
        t.draw();
    });
    
});