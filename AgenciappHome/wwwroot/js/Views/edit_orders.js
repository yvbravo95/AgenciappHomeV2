$(document).ready(function () {
    
    var step = 1;
    var allContact = false;
    tipo_orden_sufij = "MX";

    $("#nuevoContactName").val("");
    $("#nuevoContactLastName").val("");
    $("#nuevoContactPhoneMovil").val("");
    $("#nuevoContactPhoneHome").val("");
    $("#nuevoContactDir").val();
    $(".hide-search-newContactCity").val();
    $(".hide-search-newContactProvince").val();
    $("#nuevoContactReparto").val();

    var setNoOrden = function () {
        var time = $("#time").html();
        $('#no_orden').html(tipo_orden_sufij + time);
    };

    // Single Select Placeholder

    $("#orderType").select2({
        placeholder: "Tipo de orden",
        text: " "
    });

    $(".select2-placeholder-selectClient").select2({
        placeholder: "Buscar cliente por teléfono, nombre o apellido",
        val: null
    });

    $(".hide-search-clientState, .hide-search-newClientState").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Estado",
    });

    $(".hide-search-clientCity, .hide-search-newClientCity").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Ciudad",
    });
    
    $(".select2-placeholder-selectContact").select2({
        placeholder: "Buscar contacto por teléfono, nombre o apellido",
        text: " "
    });

    $(".hide-search-contactProvince, .hide-search-newContactProvince").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Provincia",
    });

    $(".hide-search-contactCity, .hide-search-newContactCity").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Ciudad",
    });

    /***********DATOS DE LA ORDEN *************/

    $("#radioMX, #radioPA, #radioAL, #radioME, #radioRE").click(function () {
        tipo_orden_sufij = $(this).val();
        setNoOrden(tipo_orden_sufij);
    });

    $(".hide-search-pago").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Tipo de Pago",
    });

    // Single Select Placeholder
    $(".select2-placeholder-valor").select2({
        placeholder: "Seleccione Valor Aduanal",
    });

    $(".select2-placeholder-selectProduct").select2({
        placeholder: "Buscar producto por tipo, color, talla o marca",
    });

    $(".select2-container--default").attr("style", "width: 100%;");

    var cleanNuevoCliente = function () {
        $('#nuevoClientName').val("");
        $('#nuevoClientLastName').val("");
        $('#nuevoClientEmail').val("");
        $('#nuevoClientMovil').val("");
        $('#nuevoClientAddress').val("");
        $('.hide-search-newClientCity').val("").trigger("change");
        $('.hide-search-newClientState').val("").trigger("change");
        $('#nuevoClientZip').val("");
    };

    $("#cancelarNuevoCliente").click(cleanNuevoCliente);

    $("#guardarNuevoCliente").click(function () {
        if (validateNuevoCliente()) {
            var source = [
                $('#nuevoClientName').val(),
                $('#nuevoClientLastName').val(),
                $('#nuevoClientEmail').val(),
                $('#nuevoClientMovil').val(),
                $('#nuevoClientAddress').val(),
                $('.hide-search-newClientCity').val(),
                $('.hide-search-newClientState').val(),
                $('#nuevoClientZip').val(),
            ];

            $.ajax({
                type: "POST",
                url: "/Orders/AddClient",
                data: JSON.stringify(source),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function () {
                    $("#nuevoClienteModal").modal("hide");
                    $("body").removeClass("modal-open");
                    $(".modal-backdrop").remove();

                    showOKMessage("Nuevo Cliente", "Cliente adicionado con éxito");

                    showAllClients();

                    $("a[href='#next']").addClass("hidden");
                },
                failure: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                },
                error: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                }
            });

            cleanNuevoCliente();
        }
    });

    var showAllClients = function () {
        $.ajax({
            type: "POST",
            url: "/Orders/GetAllClients",
            dataType: 'json',
            contentType: 'application/json',
            async: false,
            success: function (data) {
                // Contactos del cliente
                $("[name='selectClient'] > *").remove();
                $("[name='selectClient']").select2({ placeholder: "Buscar cliente por teléfono, nombre o apellido" });

                if (data.clients.length == 0) {
                    $('#inputClientName').val("");
                    $('#inputClientLastName').val("");
                    $('#inputClientMovil').val("");
                    $('#inputClientEmail').val("");
                    $('#inputClientAddress').val("");
                    $('.hide-search-clientCity').val("").trigger("change");
                    $('.hide-search-clientProvince').val("").trigger("change");
                    $('#inputClientZip').val("");
                } else {
                    var lastId;
                    for (var i = 0; i < data.clients.length; i++) {
                        var clientData = data.phones1[i].number + " - " + data.clients[i].name + " " + data.clients[i].lastName;
                        $("[name='selectClient']").select2({ data: [clientData] });
                        $("[value='" + clientData + "']").attr("value", data.clients[i].clientId);

                        lastId = data.clients[i].clientId;
                    }

                    var last = data.clients.length - 1;

                    $(".select2-placeholder-selectClient").val(lastId).trigger("change");
                    $('#inputClientName').val(data.clients[last].name);
                    $('#inputClientLastName').val(data.clients[last].lastName);
                    $('#inputClientMovil').val(data.phones1[last].number);
                    $('#inputClientEmail').val(data.clients[last].email);
                    $('#inputClientAddress').val(data.address[last].addressLine1);
                    $('.hide-search-clientCity').val(data.address[last].city).trigger("change");
                    $('.hide-search-clientState').val(data.address[last].state).trigger("change");
                    $('#inputClientZip').val(data.address[last].zip);

                    $('.select2-placeholder-selectContact').removeAttr("disabled");
                    $('#nuevoContacto').removeAttr("disabled");
                    $('#showAllContacts').removeAttr("disabled");
                    $('#remitente').html(data.clients[last].name + " " + data.clients[last].lastName);

                    showContactsOfAClient();
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

    $(".select2-placeholder-selectClient").on("select2:select", function () {
        var value = $(this).val();

        $('.select2-placeholder-selectContact').removeAttr("disabled");
        $('#nuevoContacto').removeAttr("disabled");
        $('#showAllContacts').removeAttr("disabled");
        $('#editarCliente').removeClass("hidden");

        $.ajax({
            type: "POST",
            url: "/Orders/GetClient",
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(value),
            async: false,
            success: function (data) {
                $('#inputClientName').val(data.name);
                $('#inputClientLastName').val(data.lastName);
                $('#inputClientMovil').val(data.movil);
                $('#inputClientEmail').val(data.email);
                $('#inputClientAddress').val(data.calle);
                $('.hide-search-clientCity').val(data.city).trigger("change");
                $('.hide-search-clientState').val(data.state).trigger("change");
                $('#inputClientZip').val(data.zip);

                $('#remitente').html(data.name + " " + data.lastName);

                $("a[href='#next']").removeClass("hidden");
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            }
        });

        showContactsOfAClient();

    });

    var showContactsOfAClient = function () {
        var idClient = $(".select2-placeholder-selectClient").val();

        $.ajax({
            type: "GET",
            url: "/Contacts/GetContactsOfAClient?id=" + idClient,
            dataType: 'json',
            contentType: 'application/json',
            async: true,
            success: function (data) {
                // Contactos del cliente
                $("[name='selectContact'] > *").remove();
                $("[name='selectContact']").select2({ placeholder: "Buscar contacto por teléfono, nombre o apellido" });

                if (data.contacts.length == 0) {
                    $("#showAllContacts").click();
                } else {
                    allContact = false;
                    var lastId;
                    for (var i = 0; i < data.contacts.length; i++) {
                        var contactData;
                        if (data.phones1[i].number != "")
                            contactData = data.phones1[i].number + " - " + data.contacts[i].name + " " + data.contacts[i].lastName;
                        else
                            contactData = data.phones2[i].number + " - " + data.contacts[i].name + " " + data.contacts[i].lastName;
                        $("[name='selectContact']").select2({ data: [contactData] });
                        $("[value='" + contactData + "']").attr("value", data.contacts[i].contactId);

                        lastId = data.contacts[i].contactId;
                    }

                    var last = data.contacts.length - 1;

                    $('.select2-placeholder-selectContact').val(lastId).trigger("change");
                    $('#inputContactName').val(data.contacts[last].name);
                    $('#inputContactLastName').val(data.contacts[last].lastName);
                    $('#inputContactPhoneMovil').val(data.phones1[last].number);
                    $('#inputContactPhoneHome').val(data.phones2[last].number);
                    $('#contactDireccion').val(data.address[last].addressLine1);
                    $('.hide-search-contactCity').val(data.address[last].city).trigger("change");
                    $('.hide-search-contactProvince').val(data.address[last].state).trigger("change");
                    $('#contactReparto').val(data.address[last].zip);

                    $('#destinatario').html(data.contacts[last].name + " " + data.contacts[last].lastName);

                    $("#editarContacto, a[href='#next']").removeClass("hidden");
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

    //se activan los campos desactivados y se muestran los botones guardar y cancelar
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

        // para que no pueda avanzar a la otra parte del formulario
        $("a[href='#next']").addClass("hidden");

        $('#inputClientName').removeAttr("disabled").data("prevVal", $('#inputClientName').val());
        $('#inputClientLastName').removeAttr("disabled").data("prevVal", $('#inputClientLastName').val());
        $('#inputClientMovil').removeAttr("disabled").data("prevVal", $('#inputClientMovil').val());
        $('#inputClientEmail').removeAttr("disabled").data("prevVal", $('#inputClientEmail').val());
        $('#inputClientAddress').removeAttr("disabled").data("prevVal", $('#inputClientAddress').val());
        $('.hide-search-clientCity').removeAttr("disabled").data("prevVal", $('.hide-search-clientCity').val());
        $('.hide-search-clientState').removeAttr("disabled").data("prevVal", $('.hide-search-clientState').val());
        $('#inputClientZip').removeAttr("disabled").data("prevVal", $('#inputClientZip').val());

        $('#editarCliente').addClass("hidden");
        $("#cancelarCliente").removeClass("hidden");
        $("#guardarCliente").removeClass("hidden");
    });

    var desactClientForm = function () {
        //$(".select2-placeholder-selectClient").removeAttr("disabled");
        $('#nuevoContacto').removeAttr("disabled");
        $('.select2-placeholder-selectContact').removeAttr("disabled");
        $('#editarContacto').removeAttr("disabled");

        $('#inputClientName').attr("disabled", "disabled");
        $('#inputClientLastName').attr("disabled", "disabled");
        $('#inputClientMovil').attr("disabled", "disabled");
        $('#inputClientEmail').attr("disabled", "disabled");
        $('#inputClientAddress').attr("disabled", "disabled");
        $('.hide-search-clientCity').attr("disabled", "disabled");
        $('.hide-search-clientState').attr("disabled", "disabled");
        $('#inputClientZip').attr("disabled", "disabled");

        $("#cancelarCliente").addClass("hidden");
        $("#guardarCliente").addClass("hidden");

        if ($(".select2-placeholder-selectContact").val() != null) {
            $("a[href='#next']").removeClass("hidden");
        }
    }

    var cancelClientForm = function() {
        $('#inputClientName').val($('#inputClientName').data("prevVal"));
        $('#inputClientLastName').val($('#inputClientLastName').data("prevVal"));
        $('#inputClientMovil').val($('#inputClientMovil').data("prevVal"));
        $('#inputClientEmail').val($('#inputClientEmail').data("prevVal"));
        $('#inputClientAddress').val($('#inputClientAddress').data("prevVal"));
        $('.hide-search-clientCity').val($('.hide-search-clientCity').data("prevVal")).trigger("change");
        $('.hide-search-clientState').val($('.hide-search-clientState').data("prevVal")).trigger("change");
        $('#inputClientZip').val($('#inputClientZip').data("prevVal"));

        desactClientForm();
    }

    $("#cancelarCliente").click(cancelClientForm);

    $('#guardarCliente').on('click', function () {
        if (validateEditarCliente()) {
            var source = [
                $(".select2-placeholder-selectClient").val(),
                $('#inputClientName').val(),
                $('#inputClientLastName').val(),
                $('#inputClientEmail').val(),
                $('#inputClientMovil').val(),
                $('#inputClientAddress').val(),
                $('.hide-search-clientCity').val(),
                $('.hide-search-clientState').val(),
                $('#inputClientZip').val(),
            ];

            $.ajax({
                type: "POST",
                url: "/Orders/EditClient",
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

                    var clientId = $(".select2-placeholder-selectClient").val();

                    showAllClients();

                    $(".select2-placeholder-selectClient").val(clientId).trigger("change").trigger("select2:select");

                    if ($(".select2-placeholder-selectContact").val() != null)
                        $("a[href='#next']").removeClass("hidden");

                    $("#remitente").html($('#inputClientName').val() + " " + $('#inputClientLastName').val());
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

    var cleanNuevoContacto = function () {
        $('#nuevoContactName').val("");
        $('#nuevoContactLastName').val("");
        $('#nuevoContactPhoneMovil').val("");
        $('#nuevoContactPhoneHome').val("");
        $('#nuevoContactDir').val("");
        $('.hide-search-newContactCity').val("").trigger("change");;
        $('.hide-search-newContactProvince').val("").trigger("change");;
        $('#nuevoContactReparto').val("");
    };

    $("#cancelarNuevoContacto").click(cleanNuevoContacto);

    $("#guardarNuevoContacto").click(function () {
        if (validateNuevoContacto()) {
            var source = [
                $('.select2-placeholder-selectClient').val(),    // id del cliente al cual se asocia
                $('#nuevoContactName').val(),
                $('#nuevoContactLastName').val(),
                $('#nuevoContactPhoneMovil').val(),
                $('#nuevoContactPhoneHome').val(),
                $('#nuevoContactDir').val(),
                $('.hide-search-newContactCity').val(),
                $('.hide-search-newContactProvince').val(),
                $('#nuevoContactReparto').val(),
            ];

            $.ajax({
                type: "POST",
                url: "/Orders/AddContact",
                data: JSON.stringify(source),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function (data) {
                    $("#nuevoContactoModal").modal("hide");
                    $("body").removeClass("modal-open");
                    $(".modal-backdrop").remove();

                    showOKMessage("Nuevo Contacto", "Contacto adicionado con éxito");

                    showContactsOfAClient();
                },
                failure: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                },
                error: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                }
            });

            cleanNuevoContacto();
        }
    });

    $('.select2-placeholder-selectContact').on("select2:select", function () {
        var idContact = $(this).val();

        $('#editarContacto').removeClass("hidden");

        $.ajax({
            type: "POST",
            url: "/Orders/GetContact",
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(idContact),
            async: false,
            success: function (data) {
                $('#inputContactName').val(data.name);
                $('#inputContactLastName').val(data.lastName);
                $('#inputContactPhoneMovil').val(data.movilPhone);
                $('#inputContactPhoneHome').val(data.casaPhone);
                $('#contactDireccion').val(data.direccion);
                $('.hide-search-contactCity').val(data.city).trigger("change");
                $('.hide-search-contactProvince').val(data.state).trigger("change");
                $('#contactReparto').val(data.reparto);

                $('#destinatario').html(data.name + " " + data.lastName);

                $("a[href='#next']").removeClass("hidden");
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            }
        });
    });

    $("#showAllContacts").click(function (e) {
        e.preventDefault();
        allContact = true;

        $.ajax({
            type: "POST",
            url: "/Orders/GetAllContacts",
            dataType: 'json',
            contentType: 'application/json',
            async: false,
            success: function (data) {
                // Contactos del cliente
                $("[name='selectContact'] > *").remove();

                for (var i = 0; i < data.contacts.length; i++) {
                    var contactData;
                    if (data.phones1[i].number != "")
                        contactData = data.phones1[i].number + " - " + data.contacts[i].name + " " + data.contacts[i].lastName;
                    else
                        contactData = data.phones2[i].number + " - " + data.contacts[i].name + " " + data.contacts[i].lastName;
                    $("[name='selectContact']").select2({ data: [contactData] });
                    $("[value='" + contactData + "']").attr("value", data.contacts[i].contactId);
                }

                $("[name='selectContact']").select2({
                    placeholder: "Buscar contacto por teléfono, nombre o apellido"
                });

                $("[name='selectContact']").val("").trigger("change");
                $('#inputContactName').val("");
                $('#inputContactLastName').val("");
                $('#inputContactPhoneHome').val("");
                $('#inputContactPhoneMovil').val("");
                $('#contactDireccion').val("");
                $('.hide-search-contactCity').val("").trigger("change");
                $('.hide-search-contactProvince').val("").trigger("change");
                $('#contactReparto').val("");

                $("a[href='#next']").addClass("hidden");
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    });

    //se activan los campos desactivados y se muestran los botones guardar y cancelar
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

        $('#inputContactName').removeAttr("disabled").data("prevVal", $('#inputContactName').val());
        $('#inputContactLastName').removeAttr("disabled").data("prevVal", $('#inputContactLastName').val());
        $('#inputContactPhoneMovil').removeAttr("disabled").data("prevVal", $('#inputContactPhoneMovil').val());
        $('#inputContactPhoneHome').removeAttr("disabled").data("prevVal", $('#inputContactPhoneHome').val());
        $('#contactDireccion').removeAttr("disabled").data("prevVal", $('#contactDireccion').val());
        $('.hide-search-contactCity').removeAttr("disabled").data("prevVal", $('.hide-search-contactCity').val());
        $('.hide-search-contactProvince').removeAttr("disabled").data("prevVal", $('.hide-search-contactProvince').val());
        $('#contactReparto').removeAttr("disabled").data("prevVal", $('#contactReparto').val());

        $('#editarContacto').addClass("hidden");
        $("#cancelarContacto").removeClass("hidden");
        $("#guardarContacto").removeClass("hidden");
    });

    var desactContactForm = function () {
        //$('.select2-placeholder-selectClient').removeAttr("disabled");
        $('#nuevoContacto').removeAttr("disabled");
        $('.select2-placeholder-selectContact').removeAttr("disabled");
        $('#showAllContacts').removeAttr("disabled");

        $('#inputContactName').attr("disabled", "disabled");
        $('#inputContactLastName').attr("disabled", "disabled");
        $('#inputContactPhoneMovil').attr("disabled", "disabled");
        $('#inputContactPhoneHome').attr("disabled", "disabled");
        $('#contactDireccion').attr("disabled", "disabled");
        $('.hide-search-contactCity').attr("disabled", "disabled");
        $('.hide-search-contactProvince').attr("disabled", "disabled");
        $('#contactReparto').attr("disabled", "disabled");

        $('#editarContacto').removeClass("hidden");
        $("#cancelarContacto").addClass("hidden");
        $("#guardarContacto").addClass("hidden");

        $("a[href='#next']").removeClass("hidden");
    }

    var cancelarContactForm = function () {
        $('#inputContactName').val($('#inputContactName').data("prevVal"));
        $('#inputContactLastName').val($('#inputContactLastName').data("prevVal"));
        $('#inputContactPhoneMovil').val($('#inputContactPhoneMovil').data("prevVal"));
        $('#inputContactPhoneHome').val($('#inputContactPhoneHome').data("prevVal"));
        $('#contactDireccion').val($('#contactDireccion').data("prevVal"));
        $('.hide-search-contactCity').val($('.hide-search-contactCity').data("prevVal")).trigger("change");
        $('.hide-search-contactProvince').val($('.hide-search-contactProvince').data("prevVal")).trigger("change");
        $('#contactReparto').val($('#contactReparto').data("prevVal"));

        desactContactForm();
    }

    $("#cancelarContacto").click(cancelarContactForm);

    $('#guardarContacto').on('click', function () {
        if (validateEditarContacto()) {
            var source = [
                $('.select2-placeholder-selectContact').val(),
                $('#inputContactName').val(),
                $('#inputContactLastName').val(),
                $('#inputContactPhoneMovil').val(),
                $('#inputContactPhoneHome').val(),
                $('#contactDireccion').val(),
                $('.hide-search-contactCity').val(),
                $('.hide-search-contactProvince').val(),
                $('#contactReparto').val(),
            ];

            $.ajax({
                type: "POST",
                url: "/Orders/EditContact",
                data: JSON.stringify(source),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function () {
                    showOKMessage("Editar Contacto", "Contacto editado con éxito");

                    var clientId = $('.select2-placeholder-selectContact').val();

                    //var prevName = $('#inputContactName').data("prevVal");
                    //var prevLastName = $('#inputContactLastName').data("prevVal");
                    //var prevPhoneMovil = $('#inputContactPhoneMovil').data("prevVal");
                    var name = $('#inputContactName').val();
                    var lastName = $('#inputContactLastName').val();
                    //var phoneMovil = $('#inputContactPhoneMovil').val();

                    //var prevClientData = prevPhoneMovil + " - " + prevName + " " + prevLastName;
                    //var clientData = phoneMovil + " - " + name + " " + lastName;
                    //$("[title='" + prevClientData + "']").attr("title", clientData).html(clientData);
                    //$("[value='" + clientId + "']").html(clientData);

                    if (allContact)
                        $("#showAllContacts").click();
                    else
                        showContactsOfAClient();
                    $(".select2-placeholder-selectContact").val(clientId).trigger("change").trigger("select2:select");

                    $("#destinatario").html(name + " " + lastName);

                    $("a[href='#next']").removeClass("hidden");
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

    var calcMoney = function () {
        var cantLb = $("#CantLb").val();
        var precioLb = $("#PriceLb").val();
        var oCargos = $("#OtrosCostos").val();
        var pagado = $("#ValorPagado").val();
        var valAduanalValue = $("#valAduanalValue").html();

        var precioTotalValue = cantLb * precioLb + parseFloat(valAduanalValue) + parseFloat(oCargos);
        var balanceValue = precioTotalValue - pagado;

        $("#precioTotalValue").html(precioTotalValue);
        $("#balanceValue").html(balanceValue);

        $("#ValorPagado").attr("max", precioTotalValue);
    };

    $("#CantLb, #PriceLb, #OtrosCostos, #ValorPagado").on("keyup", calcMoney).on("change", calcMoney);

    $("#txtNameVA").on("change", function () {
        var vas = $('#txtNameVA').val();
        $("#valAduanalValue").html("0");

        if (vas == null) {
            calcMoney();
        } else {
            for (var i = 0; i < vas.length; i++) {
                $.ajax({
                    type: "POST",
                    url: "/Orders/GetValueOfVAduanalId",
                    data: JSON.stringify(vas[i]),
                    dataType: 'json',
                    contentType: 'application/json',
                    async: false,
                    success: function (data) {
                        var antVAValue = parseFloat($("#valAduanalValue").html());
                        var vaValue = parseFloat(data);
                        var newVAValue = antVAValue + vaValue;
                        $("#valAduanalValue").html(newVAValue);
                        calcMoney();
                    },
                    failure: function (response) {
                        showErrorMessage("ERROR", response.responseText);
                    },
                    error: function (response) {
                        showErrorMessage("ERROR", response.responseText);
                    }

                });
            }
        }
    });

    $(".select2-placeholder-buscar").on("select2:select", function () {

        //Get the reference of the Table's TBODY element.
        var tBody = $("#tblProductos > tbody")[0];

        ////Add Row.
        var row = tBody.insertRow(-1);

        ////Add Cant cell.
        var cell = $(row.insertCell(-1));
        var celCant = $("<input id='newCant' type='number' class='form-control' value='1' min='1'/>");
        cell.append(celCant);

        cell = $(row.insertCell(-1));
        var celTipo = $("<input id='newTipo' class='form-control'/>");
        cell.append(celTipo);


        ////Add Remove cell.
        cell = $(row.insertCell(-1));
        var btnRemove = $("<input  />");
        btnRemove.attr("type", "button");
        btnRemove.attr("class", "btn btn-default");
        btnRemove.attr("onclick", "Remove");
        btnRemove.val("Eliminar");
        cell.append(btnRemove);

    });

    addProductToTable = function (cant, tipo, color, tallamarca, descripcion) {
        //Get the reference of the Table's TBODY element.
        var tBody = $("#tblProductos > TBODY")[0];

        var index = $(tBody).children().length - 1;

        ////Add Row.
        var row = tBody.insertRow(index);

        ////Add Button cell.
        var cell1 = $(row.insertCell(-1));
        cell1.append(cant);

        ////Add Tipo
        var cell2 = $(row.insertCell(-1));
        cell2.append(tipo);

        ////Add Color
        var cell3 = $(row.insertCell(-1));
        cell3.append(color);

        ////Add Add Talla.
        var cell4 = $(row.insertCell(-1));
        cell4.append(tallamarca);

        ////Add Country cell.
        var cell5 = $(row.insertCell(-1));
        cell5.append(descripcion);

        ////Add Button cell.
        var cell6 = $(row.insertCell(-1));
        var btnEdit = $("<button type='button' class='btn btn-warning' title='Editar' style='font-size: 10px'><i class='fa fa-pencil'></i></button>");
        var btnRemove = $("<button type='button' class='btn btn-danger pull-right' title='Eliminar' style='font-size: 10px'><i class='fa fa-close'></button>");
        var btnConfirm = $("<button type='button' class='btn btn-success hidden' title='Confirmar' style='font-size: 10px'><i class='fa fa-check'></button>");
        btnEdit.on("click", function () {
            cell1.html("<input type='number' name='cell1' class='form-control' value='" + cell1.html() + "'/>");
            cell2.html("<input name='cell2' class='form-control' value='" + cell2.html() + "'/>");
            cell3.html("<input name='cell3' class='form-control' value='" + cell3.html() + "'/>");
            cell4.html("<input name='cell4' class='form-control' value='" + cell4.html() + "'/>");
            cell5.html("<input name='cell5' class='form-control' value='" + cell5.html() + "'/>");

            btnConfirm.removeClass("hidden");
            btnEdit.addClass("hidden");
            btnRemove.addClass("hidden");
        });
        btnRemove.on("click", function () {
            row.remove();
        });
        btnConfirm.on("click", function () {
            if (validateEditProduct()) {
                cell1.html($("[name='cell1']").val());
                cell2.html($("[name='cell2']").val());
                cell3.html($("[name='cell3']").val());
                cell4.html($("[name='cell4']").val());
                cell5.html($("[name='cell5']").val());

                btnConfirm.addClass("hidden");
                btnEdit.removeClass("hidden");
                btnRemove.removeClass("hidden");
            }
        });
        cell6.append(btnEdit.add(btnRemove).add(btnConfirm));
    };

    $(".select2-placeholder-selectProduct").on("select2:select", function (){
        var productId = $(this).val();
        $.ajax({
            type: "POST",
            url: "/Orders/GetProduct",
            data: JSON.stringify(productId),
            dataType: 'json',
            contentType: 'application/json',
            async: false,
            success: function (data) {
                $("#newTipo").val(data.tipo);
                $("#newColor").val(data.color);
                $("#newTalla").val(data.tallamarca);

                $(".select2-placeholder-selectProduct").val("").trigger("change");
            }
        });
    });

    $('#newCant').on('keypress', function (e) {
        if (e.which == 13) {
            $('#newTipo').focus();
        }
    });

    $('#newTipo').on('keypress', function (e) {
        if (e.which == 13) {
            $('#newColor').focus();
        }
    });

    $('#newColor').on('keypress', function (e) {
        if (e.which == 13) {
            $('#newTalla').focus();
        }
    });

    $('#newTalla').on('keypress', function (e) {
        if (e.which == 13) {
            $('#newDescrip').focus();
        }
    });

    $('#btnAdd').on('click', function () {
        if (validateAddProduct()) {
            addProductToTable($("#newCant").val(), $("#newTipo").val(), $("#newColor").val(), $("#newTalla").val(), $("#newDescrip").val());

            $("#newCant").val("1");
            $("#newTipo").val("");
            $("#newColor").val("");
            $("#newTalla").val("");
            $("#newDescrip").val("");
        }
    });

    $("a[href='#previous']").html("Cancelar").click(function (e) {
        if (step == 1) {
            document.location = "/Orders";
        } else if (step == 2) {
            step--;
            $(this).html("Cancelar");
            $("a[href='#next']").html("Siguiente").removeClass("hidden").parent().show();
            $(".select2-container--default").attr("style", "width: 100%;");
        }
    });

    $("a[href='#next']").addClass("hidden").html("Siguiente").click(function () {
        if (step == 1) {
            step++;
            $("a[href='#previous']").html("Atrás");
            $("a[href='#next']").addClass("hidden");
            $("a[href='#next']").html("Editar");
            $("a[href='#next']").parent().removeClass("disabled");
        }
    });

    $("a[href='#finish']").off().html("Editar").click(function () {
        if (validateCreateOrder()) {
            var okConfirm = function () {
                var listProduct = new Array;

                var tBody = $("#tblProductos > TBODY")[0];

                for (var i = 0; i < tBody.rows.length - 1; i++) {
                    var fila = tBody.rows[i];
                    listProduct[i] = new Array;
                    for (var j = 0; j < 5; j++) {
                        listProduct[i][j] = $(fila.children[j]).html();
                    }
                }

                var datosOrden = [
                    $("#orderId").val(),
                    $("[name='orderType'][value='" + tipo_orden_sufij + "']").attr("valor"),
                    $("#no_orden").html(),
                    //$(".select2-placeholder-selectClient").val(),
                    $(".select2-placeholder-selectContact").val(),
                    $(".hide-search-pago").val(),
                    listProduct,
                    $('#txtNameVA').val(),
                    $("#CantLb").val().replace(".", ","),
                    $("#PriceLb").val().replace(".", ","),
                    $("#OtrosCostos").val().replace(".", ","),
                    $("#ValorPagado").val().replace(".", ","),
                    $("#valAduanalValue").html().replace(".", ","),
                    $("#precioTotalValue").html().replace(".", ","),
                    $("#balanceValue").html().replace(".", ","),
                    $('#orderNote').val(),
                ];

                $.ajax({
                    type: "POST",
                    url: "/Orders/EditOrder",
                    data: JSON.stringify(datosOrden),
                    dataType: 'json',
                    contentType: 'application/json',
                    async: false,
                    success: function () {
                        //window.open("/Orders/ExportarComprobante?orderNumber=" + $("#no_orden").html() + "&cantPay=" + $("#ValorPagado").val().replace(".", ","), "_blank");
                        document.location = "/Orders?msg=successEdit&orderNumber=" + $("#no_orden").html();
                    }
                });
            }
            confirmationMsg("¿Está seguro que desea editar esta orden?", "", okConfirm);
        }
    });

    var validateNuevoCliente = function () {
        if ($("#nuevoClientName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($("#nuevoClientLastName").val() == "") {
            showWarningMessage("Atención", "El campo Apellidos no puede estar vacío.");
            return false;
        } else if ($("#nuevoClientMovil").val() == "") {
            showWarningMessage("Atención", "El campo Móvil no puede estar vacío.");
            return false;
        }

        return true;
    };

    var validateEditarCliente = function () {
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

    var validateNuevoContacto = function () {
        if ($("#nuevoContactName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($("#nuevoContactLastName").val() == "") {
            showWarningMessage("Atención", "El campo Apellidos no puede estar vacío.");
            return false;
        } else if ($("#nuevoContactPhoneMovil").val() == "" && $("#nuevoContactPhoneHome").val() == "") {
            showWarningMessage("Atención", "Debe introducir al menos un teléfono de contacto.");
            return false;
        } else if ($("#nuevoContactDir").val() == "") {
            showWarningMessage("Atención", "El campo Dirección no puede estar vacío.");
            return false;
        } else if ($(".hide-search-newContactCity").val() == "") {
            showWarningMessage("Atención", "El campo Ciudad no puede estar vacío.");
            return false;
        } else if ($(".hide-search-newContactProvince").val() == "") {
            showWarningMessage("Atención", "El campo Provincia no puede estar vacío.");
            return false;
        } else if ($("#nuevoContactReparto").val() == "") {
            showWarningMessage("Atención", "El campo Reparto no puede estar vacío.");
            return false;
        }

        return true;
    };

    var validateEditarContacto = function () {
        if ($("#inputContactName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($("#inputContactLastName").val() == "") {
            showWarningMessage("Atención", "El campo Apellidos no puede estar vacío.");
            return false;
        } else if ($("#inputContactPhoneMovil").val() == "" && $("#inputContactPhoneHome").val() == "") {
            showWarningMessage("Atención", "Debe introducir al menos un teléfono de contacto.");
            return false;
        } else if ($("#contactDireccion").val() == "") {
            showWarningMessage("Atención", "El campo Dirección no puede estar vacío.");
            return false;
        } else if ($(".hide-search-contactCity").val() == "") {
            showWarningMessage("Atención", "El campo Ciudad no puede estar vacío.");
            return false;
        } else if ($(".hide-search-contactProvince").val() == "") {
            showWarningMessage("Atención", "El campo Provincia no puede estar vacío.");
            return false;
        } else if ($("#contactReparto").val() == "") {
            showWarningMessage("Atención", "El campo Reparto no puede estar vacío.");
            return false;
        }

        return true;
    };

    var validateAddProduct = function () {
        if ($("#newCant").val() == "") {
            showWarningMessage("Atención", "El campo Cantidad no puede estar vacío.");
            return false;
        }
        if ($("#newTipo").val() == "") {
            showWarningMessage("Atención", "El campo Tipo no puede estar vacío.");
            return false;
        }
        if ($("#newColor").val() == "") {
            showWarningMessage("Atención", "El campo Color no puede estar vacío.");
            return false;
        }
        if ($("#newTalla").val() == "") {
            showWarningMessage("Atención", "El campo Talla/Marca no puede estar vacío.");
            return false;
        }

        return true;
    };

    var validateEditProduct = function () {
        if ($("[name='cell1']").val() == "") {
            showWarningMessage("Atención", "El campo Cantidad no puede estar vacío.");
            return false;
        } else if ($("[name='cell2']").val() == "") {
            showWarningMessage("Atención", "El campo Tipo no puede estar vacío.");
            return false;
        } else if ($("[name='cell3']").val() == "") {
            showWarningMessage("Atención", "El campo Color no puede estar vacío.");
            return false;
        } else if ($("[name='cell4']").val() == "") {
            showWarningMessage("Atención", "El campo Talla/Marca no puede estar vacío.");
            return false;
        }

        return true;
    }

    var validateCreateOrder = function () {
        var tBody = $("#tblProductos > TBODY")[0];
        if ($("#CantLb").val() == "") {
            showWarningMessage("Atención", "El campo Peso(Lb) no puede estar vacío.");
            return false;
        } else if ($("#PriceLb").val() == "") {
            showWarningMessage("Atención", "El campo Precio no puede estar vacío.");
            return false;
        } else if ($("#OtrosCostos").val() == "") {
            showWarningMessage("Atención", "El campo Otros Cargos no puede estar vacío.");
            return false;
        } else if ($("#ValorPagado").val() == "") {
            showWarningMessage("Atención", "El campo Importe Pagado no puede estar vacío.");
            return false;
        } else if ($("#balanceValue").html().includes("-")) {
            showWarningMessage("Atención", "El Importe Pagado no puede ser superior al Precio Total.");
            return false;
        } else if ($("#CantLb").val() <= 0) {
            showWarningMessage("Atención", "El campo Peso(Lb) debe ser mayor que 0.");
            return false;
        } else if ($("#PriceLb").val() <= 0) {
            showWarningMessage("Atención", "El campo Precio debe ser mayor que 0.");
            return false;
        } else if ($("#OtrosCostos").val() < 0) {
            showWarningMessage("Atención", "El campo Otros Cargos debe ser mayor o igual que 0.");
            return false;
        } else if ($("#ValorPagado").val() < 0) {
            showWarningMessage("Atención", "El campo Importe Pagado debe ser mayor o igual que 0.");
            return false;
        } else if (tBody.rows.length == 1) {
            showWarningMessage("Atención", "Debe adicionar al menos un producto.");
            return false;
        }

        return true;
    }
});