let storeItems = [];
let cartItems = [];

// Obtener elementos del DOM
const modal = document.getElementById("cartModal");
const openModalBtn = document.getElementById("openModalBtn");
const closeBtn = document.querySelector(".close");
const cartItemsView = document.getElementById("cartItemsView");
const totalPrice = document.getElementById("totalPrice");
const productModal = document.getElementById("productModal");

$(document).ready(function () {
    $('.step-icon').each(function () {
        var $this = $(this);
        if ($this.siblings('span.step').length > 0) {
            $this.siblings('span.step').empty();
            $(this).appendTo($(this).siblings('span.step'));
        }
    });

    $("#zc").steps({
        headerTag: "h6",
        bodyTag: "fieldset",
        transitionEffect: "fade",
        titleTemplate: '<span class="step">#index#</span> #title#',
        labels: {
            previous: "Anterior",
            next: "Siguiente",
            finish: 'Comprar'
        },
        onStepChanging: function (event, currentIndex, newIndex) {
            // Allways allow previous action even if the current form is not valid!
            if (currentIndex > newIndex) {
                return true;
            }

            var error = false

            // Step 1
            if (currentIndex == 0) {
                if (cartItems.length === 0) {
                    toastr.error('No hay productos en el carrito');
                    error = true;
                }
            }

            // Step 2
            if (currentIndex == 1) {
                if ($('#inputClientName').val() === '') {
                    toastr.error('Debe seleccionar un cliente');
                    error = true;
                }
                if ($('#inputContactName').val() === '') {
                    toastr.error('Debe seleccionar un contacto');
                    error = true;
                }
                // obtener nombre de provincia
                const province = $('#select-province option:selected').text();
                if ($('#provincia').val() !== province) {
                    toastr.error('El contacto debe pertenecer a la provincia ' + province);
                    error = true;
                }
            }

            // Step 3
            if (newIndex == 2) {
                buildSummary();
            }

            return !error;
        },
        onFinishing: function (event, currentIndex) {
            return save();
        },
        onFinished: function (event, currentIndex) {
        }
    });

    loadStoreItems();

    $('#select-category,#select-province').on('change', loadStoreItems);

    $('#input-search').on('keyup', searchItems);

    $(document).on('click', '.add-cart', function () {
        const id = $(this).data('id');
        const qty = $(this).siblings('.qty-add-cart').val();
        addCartItem(id, parseInt(qty));
    })

    // Función para abrir el modal

    $('#openModalBtn').on('click', function () {
        if (cartItems.length === 0) {
            toastr.info('No hay productos en el carrito');
            return;
        }
        modal.style.display = "block";
        renderCartItems();
    })

    // Función para cerrar el modal
    closeBtn.onclick = function () {
        modal.style.display = "none";
    }

    $('.close-btn').on('click', function () {
        productModal.style.display = "none";
    })

    // Cerrar el modal si se hace clic fuera de él
    window.onclick = function (event) {
        if (event.target == modal) {
            modal.style.display = "none";
        }
    }

    $('#btn-cart-continue').on('click', function () {
        modal.style.display = "none";
    })
    $(document).on('click', '.btn-remove', function () {
        const id = $(this).data('id');
        removeItem(id + '');
    })

    $(document).on('click', '.show-item', function () {
        renderDetailProduct($(this).data('id') + '');
        productModal.style.display = "flex";
    })

    $('#addToCartBtn').on('click', function () {
        const id = $('#product-id').val();
        addCartItem(id, 1);
    })

    $('#inputClientState').select2({
        placeholder: "Seleccione un estado",
    });

    $(".select2-placeholder-selectClient").select2({
        placeholder: "Buscar cliente por teléfono, nombre o apellido",
        val: null,
        ajax: {
            type: 'POST',
            dataType: "json",
            delay: 500,
            url: '/Clients/findClient',
            data: function (params) {
                var query = {
                    search: params.term,
                }

                // Query parameters will be ?search=[term]&type=public
                return query;
            },
            processResults: function (data) {

                // Transforms the top-level key of the response object from 'items' to 'results'
                return {
                    results: $.map(data, function (obj) {

                        return { id: obj.clientId, text: obj.fullData };
                    })
                };
            }
        }
    });

    $(".select2-placeholder-selectContact").select2({
        placeholder: "Buscar contacto por teléfono, nombre o apellido",
        text: " ",
        ajax: {
            type: 'POST',
            url: '/Contacts/findContacts',
            data: function (params) {
                var query = {
                    search: params.term,
                    idClient: $('.select2-placeholder-selectClient').val()
                }

                // Query parameters will be ?search=[term]&type=public
                return query;
            },
            processResults: function (data) {

                // Transforms the top-level key of the response object from 'items' to 'results'
                return {
                    results: $.map(data, function (obj) {

                        return { id: obj.contactId, text: obj.phone1 + "-" + obj.name + " " + obj.lastName };
                    })
                };
            }
        }
    });

    $('#inputClientMovil').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });

    $('#inputContactPhoneMovil').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });
    $('#contactCI').mask('00000000000', {
        placeholder: "Carnet de Identidad"
    });
    $('#inputContactPhoneHome').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });

    $(".select2-placeholder-selectClient").on('change', function () {
        console.log($(this).val());
        $('.select2-placeholder-selectContact').removeAttr("disabled");
        $('#nuevoContacto').removeAttr("disabled");
        $('#showAllContacts').removeAttr("disabled");
        $('#editarCliente').removeClass("hidden");

        $("a[href='#next']").addClass("hidden");
        $('#cancel').hide();

        showClient($(this).val());
        showContactsOfAClient($(this).val());
    });

    $('.select2-placeholder-selectContact').on("select2:select", function () {
        $('#editarContacto').removeClass("hidden hide-search-contactCity");
        showContact($(this).val());
    });

    $("#provincia").on("change", () => selectMunicipios());

    $('#editarCliente').on('click', function () {
        // para que no pueda crear nuevo cliente mientras edita cliente
        $('#nuevoCliente').attr("disabled", "disabled");

        // para que no pueda cambiar de cliente mientras edita cliente
        $('.select2-placeholder-selectClient').attr("disabled", "disabled");

        // para que no pueda crear nuevo contacto mientras edita cliente
        $('#nuevoContacto').attr("disabled", "disabled");

        // para que no pueda cambiar de contacto mientras edita cliente
        $('.select2-placeholder-selectContact').attr("disabled", "disabled");

        // para que no pueda editar contacto mientras edita cliente
        $('#editarContacto').attr("disabled", "disabled");

        $("a[href='#next']").addClass("hidden");
        $('#cancel').hide();


        $("#showAllContacts").attr('disabled', 'disabled');


        $('#inputClientName').removeAttr("disabled").data("prevVal", $('#inputClientName').val());
        $('#inputClientLastName').removeAttr("disabled").data("prevVal", $('#inputClientLastName').val());
        $('#inputClientMovil').removeAttr("disabled").data("prevVal", $('#inputClientMovil').val());
        $('#inputClientEmail').removeAttr("disabled").data("prevVal", $('#inputClientEmail').val());
        $('#inputClientAddress').removeAttr("disabled").data("prevVal", $('#inputClientAddress').val());
        $('#inputClientCity').removeAttr("disabled").data("prevVal", $('inputClientCity').val());
        $('#inputClientState').removeAttr("disabled").data("prevVal", $('#inputClientState').val());
        $('#inputClientZip').removeAttr("disabled").data("prevVal", $('#inputClientZip').val());

        $('#editarCliente').addClass("hidden");
        $("#cancelarCliente").removeClass("hidden");
        $("#guardarCliente").removeClass("hidden");
    });

    $("#cancelarCliente").on('click', cancelClientForm);

    $('#guardarCliente').on('click', function () {
        if (validateEditarCliente()) {
            const id = $(".select2-placeholder-selectClient").val();
            var source = [
                id,
                $('#inputClientName').val(),
                $('#inputClientLastName').val(),
                $('#inputClientEmail').val(),
                $('#inputClientMovil').val(),
                $('#inputClientAddress').val(),
                $('#inputClientCity').val(),
                $('#inputClientState').val(),
                $('#inputClientZip').val(),
                "",
                "",
                ""
            ];
            $.ajax({
                type: "POST",
                url: "/Clients/EditClient",
                data: JSON.stringify(source),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.msg);
                    }
                    else {
                        toastr.error(response.msg);
                    }
                    showClient(id);
                },
                failure: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                },
                error: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                }
            });

            desactClientForm();
        }
    });

    $('#editarContacto').on('click', function () {
        // para que no pueda crear nuevo cliente mientras edita contacto
        $('#nuevoCliente').attr("disabled", "disabled");

        // para que no pueda cambiar de cliente mientras edita contacto
        $('.select2-placeholder-selectClient').attr("disabled", "disabled");

        // para que no pueda crear nuevo contacto mientras edita contacto
        $('#nuevoContacto').attr("disabled", "disabled");

        // para que no pueda cambiar de contacto mientras edita contacto
        $('.select2-placeholder-selectContact').attr("disabled", "disabled");

        // para que no pueda cambiar de contacto mientras edita contacto
        $('#showAllContacts').attr("disabled", "disabled");

        // para que no pueda editar cliente mientras edita contacto
        $('#editarCliente').attr("disabled", "disabled");

        // para que no pueda avanzar a la otra parte del formulario
        $("a[href='#next']").addClass("hidden");
        $('#cancel').hide();


        $('#inputContactName').removeAttr("disabled").data("prevVal", $('#inputContactName').val());
        $('#inputContactLastName').removeAttr("disabled").data("prevVal", $('#inputContactLastName').val());
        $('#inputContactPhoneMovil').removeAttr("disabled").data("prevVal", $('#inputContactPhoneMovil').val());
        $('#inputContactPhoneHome').removeAttr("disabled").data("prevVal", $('#inputContactPhoneHome').val());
        $('#contactDireccion').removeAttr("disabled").data("prevVal", $('#contactDireccion').val());
        $('#provincia').removeAttr("disabled").data("prevVal", $('#provincia').val());
        $('#municipio').removeAttr("disabled").data("prevVal", $('#municipio').val());
        $('#reparto').removeAttr("disabled").data("prevVal", $('#reparto').val());
        $('#contactCI').removeAttr("disabled").data("prevVal", $('#contactCI').val());

        $('#editarContacto').addClass("hidden");
        $("#cancelarContacto").removeClass("hidden");
        $("#guardarContacto").removeClass("hidden");
    });

    $("#cancelarContacto").click(cancelarContactForm);

    $('#guardarContacto').on('click', function () {
        selectedContact = $('.select2-placeholder-selectContact').val();
        if (validateEditarContacto()) {
            var source = [
                $('.select2-placeholder-selectContact').val(),
                $('#inputContactName').val(),
                $('#inputContactLastName').val(),
                $('#inputContactPhoneMovil').val(),
                $('#inputContactPhoneHome').val(),
                $('#contactDireccion').val(),
                $('#provincia').val(),
                $('#municipio').val(),
                $('#reparto').val(),
                $('#contactCI').val(),
                $(".select2-placeholder-selectClient").val(),

            ];

            $.ajax({
                type: "POST",
                url: "/Contacts/EditContact",
                data: JSON.stringify(source),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function () {
                    showOKMessage("Editar Contacto", "Contacto editado con éxito");

                    showContactsOfAClient($(".select2-placeholder-selectClient").val());
                },
                failure: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                },
                error: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                }

            });

            desactContactForm();
        }
    });

    mostrarEstados();
});

function save() {
    const clientId = $(".select2-placeholder-selectClient").val();
    const contactId = $(".select2-placeholder-selectContact").val();
    const values = calculate();
    const order = {
        clientId: clientId,
        contactId: contactId,
        price: values.price,
        shipping: values.shipping_cost,
        serviceCost: values.service_cost,
        total: values.total,
        products: cartItems.map(x => {
            return {
                id: x.id,
                sku: x.item.sku,
                quantity: x.qty
            }
        })
    }
    console.log(order);

    $.ajax({
        type: 'POST',
        url: 'CreateOrder',
        data: JSON.stringify(order),
        contentType: 'application/json',
        beforeSend: function () {
            $.blockUI({ message: '<h1>Procesando datos...</h1>' });
        },
        success: function (data) {
            if (data.success) {
                toastr.success("Compra realizada con éxito.");
                window.location.href = '/ordernew/combos';
            }
            else toastr.error(data.message);
            $.unblockUI();
        },
        error: function (data) {
            $.unblockUI();
            console.log(data);
            toastr.error('Ha ocurrido un error al guardar la orden');
        }
    });
}

function calculate() {
    let price = getPriceTotal(cartItems);
    let shipping_cost = getShippingCost(cartItems);
    let service_cost = parseFloat($('#service-cost').val());
    let total = price + shipping_cost + service_cost;
    const paymentValue = getPaymentValue();
    let debit = total - paymentValue;


    return {
        price: price,
        shipping_cost: shipping_cost,
        service_cost: service_cost,
        total: total,
        debit: debit
    }
}

function getPaymentValue() {
    return 0;
}

function loadStoreItems() {
    $('#input-search').val('');

    // limpiar carrito
    cartItems = [];
    $('#span-qty-items').html(0);

    storeItems = [];
    const category = $('#select-category').val();
    const province = $('#select-province').val();
    const page = 1;
    const pageSize = 100;
    $.ajax({
        type: 'GET',
        url: 'GetProducts',
        data: {
            category: category,
            province: province,
            page: page,
            length: pageSize
        },
        beforeSend: function () {
            $.blockUI({ message: '<h1>Procesando datos...</h1>' });
        },
        success: function (data) {
            $.unblockUI();
            if (data.success) {
                storeItems = data.products;
                buildItems(storeItems);
            }
            else toastr.error(data.message);
        },
        error: function (data) {
            $.unblockUI();
            console.log(data);
            toastr.error('Ha ocurrido un error al cargar los productos');
        }
    });
}

function buildItems(items) {
    $('.product-list').html('');
    for (let i = 0; i < items.length; i++) {
        const item = items[i];
        const id = item.id;
        const image = item?.images?.[0]?.src ?? 'https://via.placeholder.com/150';
        const name = item.name;
        const price = item.price;
        const item_view =
            `<div class="product-item">
            <div class="item-description">
                <img src="${image}" alt="${name}">         
                <h3>${name}</h3>
            </div>
            <p class="price">Price: $${price}</p>
            <div style="display: flex" class="options" data-id="${id}">
                <input class="form-control input-sm mr-1 qty-add-cart" data-id="${id}" type="number" value="1" />
                <button type="button" class="btn btn-primary btn-sm mr-1 add-cart" data-id="${id}"><i class="fa fa-cart-plus"></i></button>
                <button type="button" class="btn btn-info btn-sm show-item" data-id="${id}"><i class="fa fa-eye"></i></button>
            </div>
        </div>`;
        $('.product-list').append(item_view);
    }
}

function searchItems() {
    const search = $('#input-search').val();
    const filteredItems = storeItems.filter(x => x.name.toLowerCase().includes(search.toLowerCase()));
    buildItems(filteredItems);
}

function addCartItem(id, qty) {
    const item = storeItems.find(x => x.id == id);
    if (!item) {
        toastr.info('Producto no encontrado');
        return;
    }

    const exist = cartItems.find(x => x.id == id);

    if (exist) {
        const total_qty = exist.qty + qty;
        if (total_qty > exist.item.stock_quantity) {
            toastr.info('No hay suficiente stock');
            return;
        }
        exist.qty = total_qty;
    }
    else {
        if (qty > item.stock_quantity) {
            toastr.info('No hay suficiente stock');
            return;
        };

        const cartItem = {
            id: item.id,
            item: item,
            qty: qty
        };

        cartItems.push(cartItem);
    }
    toastr.success(`Producto añadido.`);

    const total_qty = cartItems.reduce((acc, x) => acc + x.qty, 0);
    $('#span-qty-items').html(total_qty);
}

function renderCartItems() {
    cartItemsView.innerHTML = ''; // Limpiar el contenido previo
    cartItems.forEach(item => {
        const itemElement = document.createElement('div');
        itemElement.className = 'cart-item';
        itemElement.innerHTML = `
            <p>${item.item.name} - ${item.qty} x $${item.item.price.toFixed(2)}</p>
            <div class="item-actions">
                <button class="btn-remove" data-id="${item.id}">
                    <i class="fa fa-trash"></i>
                </button>
            </div>
        `;
        cartItemsView.appendChild(itemElement);
    });

    let total = getPriceTotal(cartItems);
    let shipping_cost = getShippingCost(cartItems);

    $('#cart-price').html(`$${total.toFixed(2)}`);
    $('#cart-shipping-cost').html(`$${shipping_cost.toFixed(2)}`);
    $('#cart-total').html(`$${(total + shipping_cost).toFixed(2)}`);
}

function getShippingCost(products) {
    let shipping = [];
    products.forEach(item => {
        const exist_supplier = shipping.find(x => x.id == item.item.supplierId);
        if (!exist_supplier) {
            shipping.push({ id: item.item.supplierId, value: item.item.shippingCost });
        }
    });

    return shipping.reduce((acc, x) => acc + x.value, 0);
}

function getPriceTotal(products) {
    return products.reduce((acc, x) => acc + x.qty * x.item.price, 0);
}

function removeItem(id) {
    const index = cartItems.findIndex(item => item.id === id);
    if (index > -1) {
        cartItems.splice(index, 1);
        renderCartItems();
    }
}

function renderDetailProduct(id) {
    const product = storeItems.find(x => x.id == id);
    if (!product) {
        toastr.info('Producto no encontrado');
        return;
    }

    const idProduct = product.id;
    const images = product.images;
    const image = images?.[0]?.src ?? 'https://via.placeholder.com/150';
    const name = product.name;
    const price = product.price;
    const stock = product.stock_quantity;
    const description = product.description;

    $('#productImage').prop('src', image);
    $('#productName').html(name);
    $('#productPrice').html(`$${price}`);
    //$('#productStock').html(`Stock: ${stock}`);
    $('#productDescription').html(description);
    $('#product-id').val(idProduct);
}

function showClient(id) {
    $.ajax({
        type: "POST",
        url: "/OrderNew/GetClient",
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(id),
        async: false,
        success: function (data) {
            $('#nameClientCard').html('<strong>Nombre: </strong>' + data.name + ' ' + data.lastName);
            $('#phoneClientCard').html('<strong>Teléfono: </strong>' + data.movil);
            $('#emailClientCard').html('<strong>Email: </strong>' + data.email);
            $('#countryClientCard').html('<strong>País: </strong>' + data.country);
            $('#cityClientCard').html('<strong>Ciudad: </strong>' + data.city);
            $('#addressClientCard').html('<strong>Dirección: </strong>' + data.calle);
            $('#AuthaddressOfSend').val(data.calle);
            $('#Authemail').val(data.email);
            $('#Authphone').val(data.movil);

            //Datos del Cliente en Step 1
            $('#inputClientName').val(data.name);
            $('#inputClientLastName').val(data.lastName);
            $('#inputClientMovil').val(data.movil);
            $('#inputClientEmail').val(data.email);
            $('#inputClientAddress').val(data.calle);
            $('#inputClientCity').val(data.city);
            $('#inputClientZip').val(data.zip);
            $('#inputClientState').val(data.state).trigger("change");

            $('#remitente').html(data.name + " " + data.lastName);

            //Card
            $('[name="Card.SenderName"]').val(data.name);
            if (data.lastName != null) {
                lastname = data.lastName.split(" ");
                if (lastname.length == 1) {
                    $('[name="Card.SenderSurName"]').val(lastname[0]);
                }
                else if (lastname.length > 1) {
                    $('[name="Card.SenderSurName"]').val(lastname[0]);
                    $('[name="Card.SenderSecondSurName"]').val(lastname[1]);
                }
            }
            $('[name="Card.SenderSurName"]').val(data.lastName);
            $('[name="Card.SenderAddressEEUU"]').val(data.fullAddress);
            $('[name="Card.SenderPhoneEEUU"]').val(data.movil);
            if (data.conflictivo) {
                $("#conflictivo").removeAttr("hidden");
            }
            else {
                $("#conflictivo").attr("hidden", "hidden");
            }

        },
        failure: function (response) {
            showErrorMessage("ERROR", response.statusText);
        },
        error: function (response) {
            showErrorMessage("ERROR", response.statusText);
        }
    });
}

function showContact(id) {
    $.ajax({
        type: "POST",
        url: "/OrderNew/GetContact",
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(id),
        async: false,
        success: function (data) {
            $('#inputContactName').val(data.name);
            $('#inputContactLastName').val(data.lastName);
            $('#inputContactPhoneMovil').val(data.movilPhone);
            $('#inputContactPhoneHome').val(data.casaPhone);
            $('#contactDireccion').val(data.direccion);
            $('#provincia').val(data.city).trigger("change");
            $('#municipio').val(data.municipio);
            $('#reparto').val(data.reparto);
            $('#contactCI').val(data.ci);

            $('#destinatario').html(data.name + " " + data.lastName);

            $("a[href='#next']").removeClass("hidden");
            $('#cancel').show();

            selected_municipio = data.municipio;

            //Card
            $('[name="Card.RecipientName"]').val(data.name);
            if (data.lastName != null) {
                var lastname = (data.lastName).split(" ");
                if (lastname.length == 1) {
                    $('[name="Card.RecipientSurname"]').val(lastname[0]);
                }
                else if (lastname.length > 1) {
                    $('[name="Card.RecipientSurname"]').val(lastname[0]);
                    $('[name="Card.RecipientSecondSurname"]').val(lastname[1]);
                }
            }
            $('[name="Card.RecipientNumberCI"]').val(data.ci);
            $('[name="Card.RecipientAddressCountry"]').val(data.fullAddress);
            $('[name="Card.RecipientProvince"]').val(data.city);
            $('[name="Card.RecipientPhone"]').val(data.movilPhone);

        },
        failure: function (response) {
            showErrorMessage("ERROR", response.statusText);
        },
        error: function (response) {
            showErrorMessage("ERROR", response.statusText);
        }
    });

}

function showContactsOfAClient(clientId) {
    $('#inputContactName').val("");
    $('#inputContactLastName').val("");
    $('#inputContactPhoneMovil').val("");
    $('#inputContactPhoneHome').val("");
    $('#contactDireccion').val("");
    $('#provincia').val("").trigger("change");
    $('#municipio').val("");
    $('#reparto').val("");
    $('#contactCI').val("");
    $.ajax({
        type: "GET",
        url: "/Contacts/GetContactsOfAClient?id=" + clientId,
        dataType: 'json',
        contentType: 'application/json',
        async: true,
        success: function (data) {
            $("[name='selectContact']").empty();
            $("[name='selectContact']").append(new Option());

            if (data.length == 0) {
                //showAllContact();
            }
            else {
                for (var i = 0; i < data.length; i++) {
                    var contactData;
                    if (data[i].phone1 != "")
                        contactData = data[i].phone1 + " - " + data[i].name + " " + data[i].lastName;
                    else
                        contactData = data[i].phone2 + " - " + data[i].name + " " + data[i].lastName;;
                    $("[name='selectContact']").append(new Option(contactData, data[i].contactId));
                }
                var last = data[data.length - 1];

                $("[name='selectContact']").val(last.contactId).trigger("change");

                showContact(last.contactId);
                $("#editarContacto, a[href='#next']").removeClass("hidden");
                $('#cancel').show();
            }
        },
        failure: function (response) {
            showErrorMessage("ERROR", response.responseText);
        },
        error: function (response) {
            showErrorMessage("ERROR", response.responseText);
        }
    });
};

function mostrarEstados() {
    $("#inputClientState").empty()
    $("#inputClientState").append(new Option())
    $("#inputClientState").append(new Option("Alabama", "Alabama"))
    $("#inputClientState").append(new Option("Alaska", "Alaska"))
    $("#inputClientState").append(new Option("American Samoa", "American Samoa"))
    $("#inputClientState").append(new Option("Arizona", "Arizona"))
    $("#inputClientState").append(new Option("Arkansas", "Arkansas"))
    $("#inputClientState").append(new Option("Armed Forces Americas", "Armed Forces Americas"))
    $("#inputClientState").append(new Option("Armed Forces Europe, Canada, Africa and Middle East", "Armed Forces Europe, Canada, Africa and Middle East"))
    $("#inputClientState").append(new Option("Armed Forces Pacific", "Armed Forces Pacific"))
    $("#inputClientState").append(new Option("California", "California"))
    $("#inputClientState").append(new Option("Colorado", "Colorado"))
    $("#inputClientState").append(new Option("Connecticut", "Connecticut"))
    $("#inputClientState").append(new Option("Delaware", "Delaware"))
    $("#inputClientState").append(new Option("District of Columbia", "District of Columbia"))
    $("#inputClientState").append(new Option("Florida", "Florida"))
    $("#inputClientState").append(new Option("Georgia", "Georgia"))
    $("#inputClientState").append(new Option("Guam", "Guam"))
    $("#inputClientState").append(new Option("Hawaii", "Hawaii"))
    $("#inputClientState").append(new Option("Idaho", "Idaho"))
    $("#inputClientState").append(new Option("Illinois", "Illinois"))
    $("#inputClientState").append(new Option("Indiana", "Indiana"))
    $("#inputClientState").append(new Option("Iowa", "Iowa"))
    $("#inputClientState").append(new Option("Kansas", "Kansas"))
    $("#inputClientState").append(new Option("Kentucky", "Kentucky"))
    $("#inputClientState").append(new Option("Louisiana", "Louisiana"))
    $("#inputClientState").append(new Option("Maine", "Maine"))
    $("#inputClientState").append(new Option("Marshall Islands", "Marshall Islands"))
    $("#inputClientState").append(new Option("Maryland", "Maryland"))
    $("#inputClientState").append(new Option("Massachusetts", "Massachusetts"))
    $("#inputClientState").append(new Option("Michigan", "Michigan"))
    $("#inputClientState").append(new Option("Micronesia", "Micronesia"))
    $("#inputClientState").append(new Option("Minnesota", "Minnesota"))
    $("#inputClientState").append(new Option("Mississippi", "Mississippi"))
    $("#inputClientState").append(new Option("Missouri", "Missouri"))
    $("#inputClientState").append(new Option("Montana", "Montana"))
    $("#inputClientState").append(new Option("Nebraska", "Nebraska"))
    $("#inputClientState").append(new Option("Nevada", "Nevada"))
    $("#inputClientState").append(new Option("New Hampshire", "New Hampshire"))
    $("#inputClientState").append(new Option("New Jersey", "New Jersey"))
    $("#inputClientState").append(new Option("New Mexico", "New Mexico"))
    $("#inputClientState").append(new Option("New York", "New York"))
    $("#inputClientState").append(new Option("North Carolina", "North Carolina"))
    $("#inputClientState").append(new Option("North Dakota", "North Dakota"))
    $("#inputClientState").append(new Option("Northern Mariana Islands", "Northern Mariana Islands"))
    $("#inputClientState").append(new Option("Ohio", "Ohio"))
    $("#inputClientState").append(new Option("Oklahoma", "Oklahoma"))
    $("#inputClientState").append(new Option("Oregon", "Oregon"))
    $("#inputClientState").append(new Option("Palau", "Palau"))
    $("#inputClientState").append(new Option("Pennsylvania", "Pennsylvania"))
    $("#inputClientState").append(new Option("Puerto Rico", "Puerto Rico"))
    $("#inputClientState").append(new Option("Rhode Island", "Rhode Island"))
    $("#inputClientState").append(new Option("South Carolina", "South Carolina"))
    $("#inputClientState").append(new Option("South Dakota", "South Dakota"))
    $("#inputClientState").append(new Option("Tennessee", "Tennessee"))
    $("#inputClientState").append(new Option("Texas", "Texas"))
    $("#inputClientState").append(new Option("Utah", "Utah"))
    $("#inputClientState").append(new Option("Vermont", "Vermont"))
    $("#inputClientState").append(new Option("Virgin Islands", "Virgin Islands"))
    $("#inputClientState").append(new Option("Virginia", "Virginia"))
    $("#inputClientState").append(new Option("Washington", "Washington"))
    $("#inputClientState").append(new Option("West Virginia", "West Virginia"))
    $("#inputClientState").append(new Option("Wisconsin", "Wisconsin"))
    $("#inputClientState").append(new Option("Wyoming", "Wyoming"))
}

function validateEditarCliente() {
    if ($("#inputClientName").val() == "") {
        showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
        return false;
    } else if ($("#inputClientLastName").val() == "") {
        showWarningMessage("Atención", "El campo Apellidos no puede estar vacío.");
        return false;
    } else if ($("#inputClientMovil").val() == "") {
        showWarningMessage("Atención", "El campo Teléfono no puede estar vacío.");
        return false;
    }
    return true;
};

function desactClientForm() {
    $('#nuevoCliente').removeAttr("disabled");
    $(".select2-placeholder-selectClient").removeAttr("disabled");
    $('#nuevoContacto').removeAttr("disabled");
    $('.select2-placeholder-selectContact').removeAttr("disabled");
    $('#editarContacto').removeAttr("disabled");

    $('#inputClientName').attr("disabled", "disabled");
    $('#inputClientLastName').attr("disabled", "disabled");
    $('#inputClientMovil').attr("disabled", "disabled");
    $('#inputClientEmail').attr("disabled", "disabled");
    $('#inputClientAddress').attr("disabled", "disabled");
    $('#inputClientCity').attr("disabled", "disabled");
    $('#inputClientState').attr("disabled", "disabled");
    $('#inputClientZip').attr("disabled", "disabled");

    $('#editarCliente').removeClass("hidden");
    $("#cancelarCliente").addClass("hidden");
    $("#guardarCliente").addClass("hidden");

    if ($("#inputContactName").val() != null) {
        $("a[href='#next']").removeClass("hidden");
        $('#cancel').show();

    }
    $("#showAllContacts").removeAttr('disabled');
}

function cancelClientForm() {
    $('#inputClientName').val($('#inputClientName').data("prevVal"));
    $('#inputClientLastName').val($('#inputClientLastName').data("prevVal"));
    $('#inputClientMovil').val($('#inputClientMovil').data("prevVal"));
    $('#inputClientEmail').val($('#inputClientEmail').data("prevVal"));
    $('#inputClientAddress').val($('#inputClientAddress').data("prevVal"));
    $('#inputClientCity').val($('#inputClientCity').data("prevVal"));
    $('#inputClientState').val($('#inputClientState').data("prevVal")).trigger("change");
    $('#inputClientZip').val($('#inputClientZip').data("prevVal"));
    desactClientForm();
}

function validateEditarContacto() {
    if ($("#inputContactName").val() == "") {
        showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
        return false;
    } else if ($("#inputContactLastName").val() == "") {
        showWarningMessage("Atención", "El campo Apellidos no puede estar vacío.");
        return false;
    } else if ($("#inputContactPhoneMovil").val() == "" && $("#inputContactPhoneHome").val() == "") {
        showWarningMessage("Atención", "Debe introducir al menos un teléfono de contacto.");
        return false;
    } else if ($('#contactCI').val().length > 0) {
        if ($('#contactCI').val().length != 11) {
            showWarningMessage("Atención", "El carnet de identidad debe tener 11 dígitos");
            return false;
        }
    } else if ($("#contactDireccion").val() == "") {
        showWarningMessage("Atención", "El campo Dirección no puede estar vacío");
        return false;
    }
    else if ($("#provincia").val() == "") {
        showWarningMessage("Atención", "El campo Provincia no puede estar vacío.");
        return false;
    } else if ($("#municipio").val() == "") {
        showWarningMessage("Atención", "El campo Municipio no puede estar vacío.");
        return false;
    }
    return true;
};

function desactContactForm() {
    $('#nuevoCliente').removeAttr("disabled");
    $('.select2-placeholder-selectClient').removeAttr("disabled");
    $('#nuevoContacto').removeAttr("disabled");
    $('.select2-placeholder-selectContact').removeAttr("disabled");
    $('#showAllContacts').removeAttr("disabled");
    $('#editarCliente').removeAttr("disabled");

    $('#inputContactName').attr("disabled", "disabled");
    $('#inputContactLastName').attr("disabled", "disabled");
    $('#inputContactPhoneMovil').attr("disabled", "disabled");
    $('#inputContactPhoneHome').attr("disabled", "disabled");
    $('#contactDireccion').attr("disabled", "disabled");
    $('#provincia').attr("disabled", "disabled");
    $('#municipio').attr("disabled", "disabled");
    $('#reparto').attr("disabled", "disabled");
    $('#contactCI').attr("disabled", "disabled");

    $('#editarContacto').removeClass("hidden");
    $("#cancelarContacto").addClass("hidden");
    $("#guardarContacto").addClass("hidden");

    $("a[href='#next']").removeClass("hidden");
    $('#cancel').show();

    $("#showAllContacts").attr('disabled', 'disabled')

}

function cancelarContactForm() {
    $('#inputContactName').val($('#inputContactName').data("prevVal"));
    $('#inputContactLastName').val($('#inputContactLastName').data("prevVal"));
    $('#inputContactPhoneMovil').val($('#inputContactPhoneMovil').data("prevVal"));
    $('#inputContactPhoneHome').val($('#inputContactPhoneHome').data("prevVal"));
    $('#contactDireccion').val($('#contactDireccion').data("prevVal"));
    $('#provincia').val($('#provincia').data("prevVal")).trigger("change");
    $('#reparto').val($('#reparto').data("prevVal"));
    $('#municipio').val($('#municipio').data("prevVal"));
    $('#contactCI').val($('#contactCI').data("prevVal"));


    desactContactForm();
}

function selectMunicipios() {
    var provincia = $("#provincia").val();
    if (!provincia)
        return;

    $.ajax({
        url: "/Provincias/Municipios?nombre=" + provincia,
        type: "POST",
        dataType: "json",
        success: function (response) {
            var municipios = $("#municipio");
            municipios.empty();
            municipios.append(new Option())
            for (var i in response) {
                var m = response[i];
                municipios.append(new Option(m, m))
            }
            municipios.val(selected_municipio).trigger("change");
        }
    })
}

function buildSummary() {
    $('.cart-items').html('');
    for (var i = 0; i < cartItems.length; i++) {
        const item = cartItems[i];
        const id = item.id;
        const image = item?.item?.images?.[0]?.src ?? 'https://via.placeholder.com/100';
        const name = item.item.name;
        const price = item.item.price;
        const qty = item.qty;
        const item_total = price * qty;
        const itemView = `<div class="cart-item">
                            <img src="${image}" alt="${name}" class="product-image">
                            <div class="product-details">
                                <h3>${name}</h3>
                                <p>Cantidad: ${qty}</p>
                                <p>Precio: $${price}</p>
                            </div>
                            <div class="product-total">
                                <p>Total: $${item_total.toFixed(2)}</p>
                            </div>
                        </div>`;
        $('.cart-items').append(itemView);
    }

    const summary = calculate();
    $('#summary-price').html(`$${summary.price.toFixed(2)}`);
    $('#summary-shipping-cost').html(`$${summary.shipping_cost.toFixed(2)}`);
    $('#summary-service-cost').html(`$${summary.service_cost.toFixed(2)}`);
    $('#summary-total').html(`$${summary.total.toFixed(2)}`);
    $('#summary-debit').html(`$${summary.debit.toFixed(2)}`);
}
