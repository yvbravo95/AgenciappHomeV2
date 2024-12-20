const getPriceAv = (province, guide) => $.get("/OrderCubiq/GetPriceAv?province=" + province + "&guideId=" + guide);
const getValoresAduanales = (wholesaler) => $.get("/OrderCubiq/GetValoresAduanales?wholesalerId=" + wholesaler);

var selected_municipio;
var precioCB = 0;
var precio1 = 0;
var precio2 = 0;
var priceAv = null;
var configHandlingTransp = null;
var guideSelected = null;
let cargoSeguros = [];
let valoresAduanales = [];
var productsSelected = new Array();

var valid = true;
$(document).ready(function () {
    $(document).on("change", "#selectMayorista", function () {
        var id = $(this).val();
        aux(id);
    });

    function aux(idmayorista) {
        $.ajax({
            type: "POST",
            url: "/OrderNew/getMayorista",
            data: {
                id: idmayorista,
            },
            async: false,
            success: function (data) {
                $("#select2-selectMayorista-container").attr(
                    "title",
                    data.termsConditions
                );

                getCostoEnvio();
                getCargosSeguros();
                renderSelectVA("#select-aduanal-order");

                if (data.name == "District Cargo and Logistics") {
                    $('#div_valores-aduanales').addClass('hidden');
                    renderSelectVA("#select-aduanal");
                }
                else if (data.name == "Fly Away Travel") {
                    $('#div_valores-aduanales').removeClass('hidden');
                    $('#btn-addproductautom').show();
                    loadProductosAutom(agencyFly);
                }
                else if (data.name == "Rapi Cargo") {
                    $('#btn-addproductautom').show();
                    renderSelectVA("#select-aduanal");
                    $('#div_valores-aduanales').addClass('hidden');
                    loadProductosAutom(agencyRapiCargo);
                }
                else if (agencyId != agencyFly) {
                    renderSelectVA("#select-aduanal");
                    $('#div_valores-aduanales').addClass('hidden');
                }
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
        });
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
        block_ele.unblock();
    }

    /**********************************
         *   Form Wizard Step Icon
    **********************************/
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
            finish: 'Editar orden'
        },
        onStepChanging: async function (event, currentIndex, newIndex) {
            // Allways allow previous action even if the current form is not valid!
            if (currentIndex > newIndex) {
                return true;
            }

            //----------------Step1
            var error = false;

            if (newIndex == 1) {
                if ($("#single-placeholder-2").val() == "" || $("#single-placeholder-3").val() == "") {
                    return false;
                }

                if ($("#inputClientMovil").val() == "") {
                    showWarningMessage("Atención", "El campo de teléfono del cliente es obligatorio.");
                    return false;
                }
                if ($("#inputClientId").val() == "") {
                    showWarningMessage("Atención", "El campo ID del cliente es obligatorio.");
                    return false;
                }
                var validContact = validateEditarContacto();
                if (!validContact) {
                    return false;
                }
                if (!valid) {
                    showWarningMessage("Atención", "No hay guía aérea para la provincia del contacto seleccionado");
                    return false;
                }

                // Para obtener el mayorista seleccionado
                if (idMayoristabyTransferencia) {
                    $("#selectMayorista").val(idMayoristabyTransferencia).trigger("change");
                }

                var provincia = $("#provincia").val();
                priceAv = await getPriceAv(provincia);
                loadSavedPaquetes();
                InitSelectProduct();
                inicializarProductosGuardados(); // Para inicializar los productos de la orden
                inicializarInsuranceValues(); // Para inicializar los seguros de la orden
                inicializarValoresAduanales(); // Para inicializar los valores aduaneros de la orden
                $("#Type").trigger("change");
                checkpago();
            }

            //----------------Step2

            if (newIndex == 2) {
                if ($("#txtNameVA").val() == "")
                    return false;
                else if ($("#tblPaquetes > TBODY")[0].rows.length == 1) {
                    return false;
                }
                else {
                    //Preparando el step3 con los productos seleccinados
                    var listPaquetes = [];

                    var tBody = $("#tblPaquetes > TBODY")[0];

                    for (var i = 0; i < tBody.rows.length - 1; i++) {
                        var fila = tBody.rows[i];
                        listPaquetes[i] = new Array;

                        listPaquetes[i]["cantidad"] = $(fila.children[0]).html();
                        listPaquetes[i]["descripcion"] = $(fila.children[1]).html();

                        var chk = '<label class="custom-control custom-checkbox">\n' +
                            '                                    <input type="checkbox" class="custom-control-input order-select" id="chk' + i + '" />\n' +
                            '                                    <span class="custom-control-indicator"></span>\n' +
                            '                                    <span class="custom-control-description"></span>\n' +
                            '                                </label>';

                        var tr = '<tr><td>' + chk + '</td><td>' +
                            '<input class="form-control" style="width: 85px" type="number" min="1" max="' + listPaquetes[i]["cantidad"] + '" value="1">' + '</td><td>' +
                            listPaquetes[i]["cantidad"] + '</td><td>' +
                            listPaquetes[i]["descripcion"] +
                            '</td></tr>';

                        $("#tableBody").html($("#tableBody").html() + tr)


                    }


                }

            }
            return true;
        },
        onFinishing: function (event, currentIndex) {
            //bloquear el boton Enviar

            return sendOrder();
        },
        onFinished: function (event, currentIndex) {


        }
    });

    $("a[href='#next']").addClass("hidden");
    $("a[href=#previous]").hide();
    $("a[href=#previous]").click(function () {
        $("#showAllContacts").removeAttr('disabled')
        $("a[href=#previous]").hide();
    });
    $("a[href=#next]").click(function () {
        $("#showAllContacts").attr('disabled', 'disabled')

        $("a[href=#previous]").show();
    });

    /*********************************************/


    //placeholder cliente y contacto
    $("#municipio").select2({
    });
    $("#provincia").select2({
    });
    $("#select-aduanal-order").select2({})

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
                };

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
                };

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

    $("#selectMayorista").select2({
        placeholder: "Seleccione un mayorista",
    });

    $(".hide-search-clientState").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Estado",
    });

    $(".hide-search-contactCity2").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Provincia",
    });

    $('#inputClientMovil').mask('(000)-000-0000', {
        placeholder: "(000)-000-0000"
    });

    $('#inputContactPhoneMovil').mask('0000000000', {
        placeholder: "0000000000"
    });

    $('#contactCI').mask('00000000000', {
        placeholder: "Carnet de Identidad"
    });

    $('#inputContactPhoneHome').mask('0000000000', {
        placeholder: "0000000000"
    });

    //placeholder orden

    $(".hide-search-pago").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Tipo de Pago",
    });

    $(".select2-placeholder-valor").select2({
        placeholder: "Valor Aduanal",

    });

    $(".select2-placeholder-selectProduct").select2({
        placeholder: "Buscar producto en bodega",
    });

    /**********Clientes y Contactos ***********/
    var selectMunicipios = function () {
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
    };

    var showClient = function () {
        var value = $(".select2-placeholder-selectClient").val();
        selectedClient = value;
        $.ajax({
            type: "POST",
            url: "/Clients/GetClient",
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(value),
            async: false,
            success: function (data) {
                //Datos del Cliente en Authorization Card
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
                $('#inputClientId').val(data.id);
                $('#inputClientState').val(data.state).trigger("change");

                $('#remitente').html(data.name + " " + data.lastName);
                if (data.credito && data.credito != 0) {
                    $("#div_credito").removeAttr('hidden');
                    $("#credito").html(data.getCredito);
                }

                $('#remitente').html(data.name + " " + data.lastName);
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            }
        });
    };

    var showContact = function () {
        var idContacto = $('.select2-placeholder-selectContact').val();
        if (idContacto != null) {
            $.ajax({
                type: "POST",
                url: "/OrderNew/GetContact",
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(idContacto),
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
                    selected_municipio = data.municipio;
                    $("a[href='#next']").removeClass("hidden");
                    $('#destinatario').html(data.name + " " + data.lastName);
                },
                failure: function (response) {
                    showErrorMessage("ERROR", response.statusText);
                },
                error: function (response) {
                    showErrorMessage("ERROR", response.statusText);
                }
            });
        }

    };

    var showContactsOfAClient = function () {
        var idClient = $(".select2-placeholder-selectClient").val();
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
            url: "/Contacts/GetContactsOfAClient?id=" + idClient,
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
                    if (selectedContact != null && selectedContact != "00000000-0000-0000-0000-000000000000") {
                        $("[name='selectContact']").val(selectedContact).trigger("change");
                        selectedContact = "00000000-0000-0000-0000-000000000000";

                    }
                    else {
                        $("[name='selectContact']").val(last.contactId).trigger("change");

                    }
                    showContact();
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

    var mostrarEstados = function () {
        $("#inputClientState").empty();
        $("#inputClientState").append(new Option());
        $("#inputClientState").append(new Option("Alabama", "Alabama"));
        $("#inputClientState").append(new Option("Alaska", "Alaska"));
        $("#inputClientState").append(new Option("American Samoa", "American Samoa"));
        $("#inputClientState").append(new Option("Arizona", "Arizona"));
        $("#inputClientState").append(new Option("Arkansas", "Arkansas"));
        $("#inputClientState").append(new Option("Armed Forces Americas", "Armed Forces Americas"));
        $("#inputClientState").append(new Option("Armed Forces Europe, Canada, Africa and Middle East", "Armed Forces Europe, Canada, Africa and Middle East"));
        $("#inputClientState").append(new Option("Armed Forces Pacific", "Armed Forces Pacific"));
        $("#inputClientState").append(new Option("California", "California"));
        $("#inputClientState").append(new Option("Colorado", "Colorado"));
        $("#inputClientState").append(new Option("Connecticut", "Connecticut"));
        $("#inputClientState").append(new Option("Delaware", "Delaware"));
        $("#inputClientState").append(new Option("District of Columbia", "District of Columbia"));
        $("#inputClientState").append(new Option("Florida", "Florida"));
        $("#inputClientState").append(new Option("Georgia", "Georgia"));
        $("#inputClientState").append(new Option("Guam", "Guam"));
        $("#inputClientState").append(new Option("Hawaii", "Hawaii"));
        $("#inputClientState").append(new Option("Idaho", "Idaho"));
        $("#inputClientState").append(new Option("Illinois", "Illinois"));
        $("#inputClientState").append(new Option("Indiana", "Indiana"));
        $("#inputClientState").append(new Option("Iowa", "Iowa"));
        $("#inputClientState").append(new Option("Kansas", "Kansas"));
        $("#inputClientState").append(new Option("Kentucky", "Kentucky"));
        $("#inputClientState").append(new Option("Louisiana", "Louisiana"));
        $("#inputClientState").append(new Option("Maine", "Maine"));
        $("#inputClientState").append(new Option("Marshall Islands", "Marshall Islands"));
        $("#inputClientState").append(new Option("Maryland", "Maryland"));
        $("#inputClientState").append(new Option("Massachusetts", "Massachusetts"));
        $("#inputClientState").append(new Option("Michigan", "Michigan"));
        $("#inputClientState").append(new Option("Micronesia", "Micronesia"));
        $("#inputClientState").append(new Option("Minnesota", "Minnesota"));
        $("#inputClientState").append(new Option("Mississippi", "Mississippi"));
        $("#inputClientState").append(new Option("Missouri", "Missouri"));
        $("#inputClientState").append(new Option("Montana", "Montana"));
        $("#inputClientState").append(new Option("Nebraska", "Nebraska"));
        $("#inputClientState").append(new Option("Nevada", "Nevada"));
        $("#inputClientState").append(new Option("New Hampshire", "New Hampshire"));
        $("#inputClientState").append(new Option("New Jersey", "New Jersey"));
        $("#inputClientState").append(new Option("New Mexico", "New Mexico"));
        $("#inputClientState").append(new Option("New York", "New York"));
        $("#inputClientState").append(new Option("North Carolina", "North Carolina"));
        $("#inputClientState").append(new Option("North Dakota", "North Dakota"));
        $("#inputClientState").append(new Option("Northern Mariana Islands", "Northern Mariana Islands"));
        $("#inputClientState").append(new Option("Ohio", "Ohio"));
        $("#inputClientState").append(new Option("Oklahoma", "Oklahoma"));
        $("#inputClientState").append(new Option("Oregon", "Oregon"));
        $("#inputClientState").append(new Option("Palau", "Palau"));
        $("#inputClientState").append(new Option("Pennsylvania", "Pennsylvania"));
        $("#inputClientState").append(new Option("Puerto Rico", "Puerto Rico"));
        $("#inputClientState").append(new Option("Rhode Island", "Rhode Island"));
        $("#inputClientState").append(new Option("South Carolina", "South Carolina"));
        $("#inputClientState").append(new Option("South Dakota", "South Dakota"));
        $("#inputClientState").append(new Option("Tennessee", "Tennessee"));
        $("#inputClientState").append(new Option("Texas", "Texas"));
        $("#inputClientState").append(new Option("Utah", "Utah"));
        $("#inputClientState").append(new Option("Vermont", "Vermont"));
        $("#inputClientState").append(new Option("Virgin Islands", "Virgin Islands"));
        $("#inputClientState").append(new Option("Virginia", "Virginia"));
        $("#inputClientState").append(new Option("Washington", "Washington"));
        $("#inputClientState").append(new Option("West Virginia", "West Virginia"));
        $("#inputClientState").append(new Option("Wisconsin", "Wisconsin"));
        $("#inputClientState").append(new Option("Wyoming", "Wyoming"));
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
        else if ($("#inputClientId").val() == "") {
            showWarningMessage("Atención", "El campo ID no puede estar vacío.");
            return false;
        }
        else if ($("#inputClientEmail").val() != "") {
            var regexEmail = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
            if (!regexEmail.test($("#inputClientEmail").val())) {
                showWarningMessage("Atención", "El campo Email no tiene el formato correcto.");
                return false;
            }
        }

        return true;
    };

    var desactClientForm = function () {
        $('#nuevoCliente').removeAttr("disabled");
        $(".select2-placeholder-selectClient").removeAttr("disabled");
        $('#nuevoContacto').removeAttr("disabled");
        $('.select2-placeholder-selectContact').removeAttr("disabled");
        $('#editarContacto').removeAttr("disabled");

        $('#inputClientName').attr("disabled", "disabled");
        $('#inputClientId').attr("disabled", "disabled");
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
        }
        $("#showAllContacts").removeAttr('disabled');

    };

    var cancelClientForm = function () {
        $('#inputClientName').val($('#inputClientName').data("prevVal"));
        $('#inputClientLastName').val($('#inputClientLastName').data("prevVal"));
        $('#inputClientMovil').val($('#inputClientMovil').data("prevVal"));
        $('#inputClientEmail').val($('#inputClientEmail').data("prevVal"));
        $('#inputClientAddress').val($('#inputClientAddress').data("prevVal"));
        $('#inputClientCity').val($('#inputClientCity').data("prevVal"));
        $('#inputClientState').val($('#inputClientState').data("prevVal")).trigger("change");
        $('#inputClientZip').val($('#inputClientZip').data("prevVal"));
        desactClientForm();
    };

    var validateEditarContacto = function () {
        if ($("#inputContactName").val() == "") {
            showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
            return false;
        } else if ($("#inputContactLastName").val() == "") {
            showWarningMessage("Atención", "El campo Apellidos no puede estar vacío.");
            return false;
        } else if ($("#inputContactPhoneMovil").val().length == 0 && $("#inputContactPhoneHome").val().length == 0) {
            showWarningMessage("Atención", "Debe introducir al menos un teléfono de contacto.");
            return false;
        }
        else if ($('#contactCI').val().length != 11) {
            showWarningMessage("Atención", "El carnet de identidad debe tener 11 dígitos");
            return false;
        }

        else if ($("#contactDireccion").val().length < 8) {
            showWarningMessage("Atención", "El campo Dirección debe ser de ocho caracteres en adelante");
            return false;
        }
        else if ($("#provincia").val() == "") {
            showWarningMessage("Atención", "El campo Provincia no puede estar vacío.");
            return false;
        } else if ($("#municipio").val() == "") {
            showWarningMessage("Atención", "El campo Municipio no puede estar vacío.");
            return false;
        }
        const splitLasName = $("#inputContactLastName").val().trim().split(' ');
        if (splitLasName.length <= 1) {
            showWarningMessage(
                "Atención",
                "El campo Apellidos debe contener el primer y segundo apellido."
            );
            return false;
        }
        return true;
    };

    var desactContactForm = function () {
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
        $("#showAllContacts").attr('disabled', 'disabled')

    };

    var cancelarContactForm = function () {
        $('#inputContactName').val($('#inputContactName').data("prevVal"));
        $('#inputClientId').val($('#inputClientId').data("prevVal"));
        $('#inputContactLastName').val($('#inputContactLastName').data("prevVal"));
        $('#inputContactPhoneMovil').val($('#inputContactPhoneMovil').data("prevVal"));
        $('#inputContactPhoneHome').val($('#inputContactPhoneHome').data("prevVal"));
        $('#contactDireccion').val($('#contactDireccion').data("prevVal"));
        $('#provincia').val($('#provincia').data("prevVal")).trigger("change");
        $('#reparto').val($('#reparto').data("prevVal"));
        $('#municipio').val($('#municipio').data("prevVal"));
        $('#contactCI').val($('#contactCI').data("prevVal"));

        desactContactForm();
    };

    var esprovinciavalida = function () {
        var provincia = $("#provincia").val();
        $.ajax({
            url: "/OrderCubiq/IsValidProv",
            method: "POST",
            data: { provincia: provincia },
            success: function (data) {
                if (data) {
                    $("#no_guia").html(data);
                    valid = true;
                }
                else {
                    valid = false;
                }
            }
        })
    }

    $("#provincia").on("change", function () {
        selectMunicipios();
        esprovinciavalida();
    });

    $(".select2-placeholder-selectClient").on("select2:select", function (a, b) {
        $('.select2-placeholder-selectContact').removeAttr("disabled");
        $('#nuevoContacto').removeAttr("disabled");
        $('#showAllContacts').removeAttr("disabled");
        $('#editarCliente').removeClass("hidden");

        $("a[href='#next']").addClass("hidden");
        showClient();

        showContactsOfAClient();
    });

    $('.select2-placeholder-selectContact').on("select2:select", function () {
        $('#editarContacto').removeClass("hidden hide-search-contactCity");
        showContact();
    });

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

        $("a[href='#next']").addClass("hidden");

        $("#showAllContacts").attr('disabled', 'disabled');


        $('#inputClientName').removeAttr("disabled").data("prevVal", $('#inputClientName').val());
        $('#inputClientId').removeAttr("disabled").data("prevVal", $('#inputClientId').val());
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
                $('#inputClientCity').val(),
                $('#inputClientState').val(),
                $('#inputClientZip').val(),
                "",
                "",
                $('#inputClientId').val(),
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
                    showClient();
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
                success: function (data) {
                    showOKMessage("Editar Contacto", "Contacto editado con éxito");
                    showContactsOfAClient();
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

    $("#nuevoContacto").attr('disabled', 'disabled');

    //showAllClients();

    mostrarEstados();

    /***********DATOS DE LA ORDEN *************/

    var calcMoney = function () {
        if (blockRight == "True") {
            calcMoneyBlockRight();
        }
        else {
            displayValorAduanal();

            const priceHandling = parseFloat($("#HandlingAndTransportation_Sale").val()) || 0;
            if (priceHandling < 0) return false;

            const precio = parseFloat($("#Price").val()) || 0;
            const oCargos = parseFloat($("#OtrosCostos").val()) || 0;
            const valAduanalValue = parseFloat($('#ValorAduanal').val()) || 0;
            const descuento = parseFloat($("#Descuento").val()) || 0;
            const cargoAdicional = parseFloat($("#CargoAdicional").val()) || 0;
            const insuranceValue = parseFloat($("#InsuranceValue").val()) || 0;

            let precioTotalValue = precio + priceHandling + oCargos + cargoAdicional + insuranceValue + valAduanalValue - descuento;
            let balanceValue = 0;
            if ($("#checkpago").is(":checked")) {
                const pagoCash = parseFloat($("#pagoCash").val()) || 0;
                const pagoZelle = parseFloat($("#pagoZelle").val()) || 0;
                const pagoCheque = parseFloat($("#pagoCheque").val()) || 0;
                const pagoTransf = parseFloat($("#pagoTransf").val()) || 0;
                const pagoWeb = parseFloat($("#pagoWeb").val()) || 0;

                const pagoCrDeb = parseFloat($("#pagoCredito").val()) || 0;
                const fee = parseFloat($("#fee").html()) || 0;
                const pagoCrDebReal = pagoCrDeb / (1 + fee / 100);
                const feeCrDeb = pagoCrDeb - pagoCrDebReal;

                if (pagoCrDeb > 0) {
                    $("#contfee").show();
                } else {
                    $("#contfee").hide();
                }
                precioTotalValue += feeCrDeb;

                let pagoCredito = 0;
                if ($("#check_credito").is(":checked")) {
                    pagoCredito = Math.min(parseFloat($("#credito").html()) || 0, precioTotalValue);
                }

                balanceValue = precioTotalValue - pagoCash - pagoZelle - pagoCheque - pagoCrDeb - pagoTransf - pagoWeb - pagoCredito;

                $("#pagoCash").attr("max", (balanceValue + pagoCash).toFixed(2));
                $("#pagoZelle").attr("max", (balanceValue + pagoZelle).toFixed(2));
                $("#pagoCheque").attr("max", (balanceValue + pagoCheque).toFixed(2));
                $("#pagoTransf").attr("max", (balanceValue + pagoTransf).toFixed(2));
                $("#pagoWeb").attr("max", (balanceValue + pagoWeb).toFixed(2));
                $("#pagoCredito").attr("max", (balanceValue + pagoCrDebReal + (balanceValue + pagoCrDebReal) * fee / 100).toFixed(2));
                $("#pagar_credit").html(`$${(balanceValue + pagoCrDebReal + (balanceValue + pagoCrDebReal) * fee / 100).toFixed(2)} (${((balanceValue + pagoCrDebReal) * fee / 100).toFixed(2)} fee)`);

                // Valor Sale Amount en authorization card
                $('[name="AuthSaleAmount"]').val(pagoCrDebReal.toFixed(2));
                const aconvcharge = parseFloat($('[name="AuthConvCharge"]').val()) || 0;
                const total = pagoCrDebReal + (pagoCrDebReal * aconvcharge / 100);
                $('[name="TotalCharge"]').val(total.toFixed(2));
            } else {
                const tipoPagoId = $('[name="TipoPago"]').val();
                const tipoPago = $(`option[value="${tipoPagoId}"]`).html();

                if (tipoPago === "Crédito o Débito") {
                    const fee = parseFloat($("#fee").html()) || 0;
                    // Valor Sale Amount en authorization card
                    $('[name="AuthSaleAmount"]').val(precioTotalValue.toFixed(2));
                    precioTotalValue += precioTotalValue * (fee / 100);
                    $('[name="TotalCharge"]').val(precioTotalValue.toFixed(2));
                }

                if ($("#check_credito").is(":checked")) {
                    const credito = parseFloat($("#credito").html()) || 0;
                    if (precioTotalValue - credito > 0) {
                        $("#ValorPagado").attr("max", (precioTotalValue - credito).toFixed(2));
                        $("#ValorPagado").val((precioTotalValue - credito).toFixed(2));
                    } else {
                        $("#ValorPagado").attr("max", 0);
                        $("#ValorPagado").val(0);
                    }
                } else {
                    $("#ValorPagado").val(precioTotalValue.toFixed(2));
                    $("#ValorPagado").attr("max", precioTotalValue.toFixed(2));
                }
            }

            $("#precioTotalValue").html(precioTotalValue.toFixed(2));
            $("#balanceValue").html(balanceValue.toFixed(2));
            if ($("#balanceValue").html() === "-0.00") $("#balanceValue").html("0.00");
        }
    };

    var calcMoneyBlockRight = function () {
        displayValorAduanal();

        const priceHandling = parseFloat($("#HandlingAndTransportation_Sale").val()) || 0;
        if (priceHandling < 0) return false;

        const precio = parseFloat($("#Price").val()) || 0;
        const oCargos = parseFloat($("#OtrosCostos").val()) || 0;
        const valAduanalValue = parseFloat($('#ValorAduanal').val()) || 0;
        const descuento = parseFloat($("#Descuento").val()) || 0;
        const cargoAdicional = parseFloat($("#CargoAdicional").val()) || 0;
        const insuranceValue = parseFloat($("#InsuranceValue").val()) || 0;

        let precioTotalValue = precio + priceHandling + oCargos + cargoAdicional + insuranceValue + valAduanalValue - descuento;
        let balanceValue = precioTotalValue - valorPagado;

        $("#precioTotalValue").html(precioTotalValue.toFixed(2));
        $("#balanceValue").html(balanceValue.toFixed(2));
        if ($("#balanceValue").html() === "-0.00") $("#balanceValue").html("0.00");
    };

    var calcMoneyPayment = function () {
        if (blockRight == "True") {
            calcMoneyBlockRight();
        }
        else {
            displayValorAduanal();

            const priceHandling = parseFloat($("#HandlingAndTransportation_Sale").val()) || 0;
            if (priceHandling < 0) return false;

            const precio = parseFloat($("#Price").val()) || 0;
            const oCargos = parseFloat($("#OtrosCostos").val()) || 0;
            const pagado = parseFloat($("#ValorPagado").val()) || 0;
            const valAduanalValue = parseFloat($('#ValorAduanal').val()) || 0;
            const descuento = parseFloat($("#Descuento").val()) || 0;
            const cargoAdicional = parseFloat($("#CargoAdicional").val()) || 0;
            const insuranceValue = parseFloat($("#InsuranceValue").val()) || 0;

            let precioTotalValue = precio + priceHandling + oCargos + cargoAdicional + insuranceValue + valAduanalValue - descuento;
            let max = precioTotalValue;

            let balanceValue = 0
            if ($("#checkpago").is(":checked")) {
                const pagoCash = parseFloat($("#pagoCash").val()) || 0;
                const pagoZelle = parseFloat($("#pagoZelle").val()) || 0;
                const pagoCheque = parseFloat($("#pagoCheque").val()) || 0;
                const pagoTransf = parseFloat($("#pagoTransf").val()) || 0;
                const pagoWeb = parseFloat($("#pagoWeb").val()) || 0;

                const pagoCrDeb = parseFloat($("#pagoCredito").val()) || 0;
                const fee = parseFloat($("#fee").html()) || 0;
                const pagoCrDebReal = pagoCrDeb / (1 + fee / 100);
                const feeCrDeb = pagoCrDeb - pagoCrDebReal;

                if (pagoCrDeb > 0) {
                    $("#contfee").show();
                } else {
                    $("#contfee").hide();
                }
                precioTotalValue += feeCrDeb;

                let pagoCredito = 0;
                if ($("#check_credito").is(":checked")) {
                    pagoCredito = Math.min(parseFloat($("#credito").html()) || 0, precioTotalValue);
                }

                balanceValue = precioTotalValue - pagoCash - pagoZelle - pagoCheque - pagoCrDeb - pagoTransf - pagoWeb - pagoCredito;

                $("#pagoCash").attr("max", (balanceValue + pagoCash).toFixed(2));
                $("#pagoZelle").attr("max", (balanceValue + pagoZelle).toFixed(2));
                $("#pagoCheque").attr("max", (balanceValue + pagoCheque).toFixed(2));
                $("#pagoTransf").attr("max", (balanceValue + pagoTransf).toFixed(2));
                $("#pagoTransf").attr("max", (balanceValue + pagoTransf).toFixed(2));
                $("#pagoWeb").attr("max", (balanceValue + pagoWeb).toFixed(2));
                $("#pagoCredito").attr("max", (balanceValue + pagoCrDebReal + (balanceValue + pagoCrDebReal) * fee / 100).toFixed(2));
                $("#pagar_credit").html(`$${(balanceValue + pagoCrDebReal + (balanceValue + pagoCrDebReal) * fee / 100).toFixed(2)} (${((balanceValue + pagoCrDebReal) * fee / 100).toFixed(2)} fee)`);

                // Valor Sale Amount en authorization card
                $('[name="AuthSaleAmount"]').val(pagoCrDebReal.toFixed(2));
                const aconvcharge = parseFloat($('[name="AuthConvCharge"]').val()) || 0;
                const total = pagoCrDebReal + (pagoCrDebReal * aconvcharge / 100);
                $('[name="TotalCharge"]').val(total.toFixed(2));
            }
            else {
                const tipoPagoId = $('[name="TipoPago"]').val();
                const tipoPago = $(`option[value="${tipoPagoId}"]`).html();
                if (tipoPago === "Crédito o Débito") {
                    const fee = parseFloat($("#fee").html()) || 0;
                    const pagdoReal = pagado / (1 + fee / 100);
                    const feeCrDeb = pagado - pagdoReal;
                    precioTotalValue += feeCrDeb;
                    max += (max * fee) / 100;

                    // Valor Sale Amount en authorization card
                    $('[name="AuthSaleAmount"]').val(pagdoReal.toFixed(2));
                    $('[name="TotalCharge"]').val(pagado);
                }

                if ($("#check_credito").is(":checked")) {
                    const pagoCredito = Math.min(parseFloat($("#credito").html()) || 0, precioTotalValue);
                    balanceValue = precioTotalValue - pagado - pagoCredito;
                } else {
                    balanceValue = precioTotalValue - pagado;
                }
            }

            $("#precioTotalValue").html(precioTotalValue.toFixed(2));
            $("#balanceValue").html(balanceValue.toFixed(2));
            $("#ValorPagado").attr("max", max.toFixed(2));
        }
    };

    $('#AWB').on('change', async function () {
        $(this).prop('disabled', true);
        var provincia = $("#provincia").val();
        var guide = $(this).val();

        priceAv = await getPriceAv(provincia, guide);
        $("#newPesoLb").val("0");
        $("#newPesoKg").val("0");
        $("#newLargo").val("0");
        $("#newAlto").val("0");
        $("#newAncho").val("0");
        $("#newPrecio").val("0");
        $("#newDescrip").val("");
    })

    var validateAddPaquete = function () {
        if ($("#newPesoLb").val() == 0) {
            showWarningMessage("Atención", "El campo Peso no puede estar vacío.");
            return false;
        }

        if (!$("#package-type").val()) {
            showWarningMessage("Atención", "El campo Tipo no puede estar vacío.");
            return false;
        }
        if ($("#newDescrip").val() == "") {
            showWarningMessage(
                "Atención",
                "El campo Descripción no puede estar vacío."
            );
            return false;
        }
        return true;
    };

    var validateEditPaquete = function () {
        if (!$("[name='cell2']").val()) {
            showWarningMessage("Atención", "El campo Tipo no puede estar vacío.");
            return false;
        }

        if ($("[name='cell3']").val() == "") {
            showWarningMessage("Atención", "El campo Peso no puede estar vacío.");
            return false;
        }
        if ($("[name='cell1']").val() == "") {
            showWarningMessage(
                "Atención",
                "El campo Descripción no puede estar vacío."
            );
            return false;
        }
        return true;
    };

    var sendOrder = function () {
        $("a[href=#finish]").block({
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
        })
        //Valido la orden
        if (!isSend && validateCreateOrder()) {
            isSend = true;
            //tomando los productos
            var listPaquetes = [];

            var tBody = $("#tblPaquetes > TBODY")[0];
            var index = 0;
            for (var i = 1; i < tBody.rows.length; i++) {
                //filas de cada producto (excepto la 1era)
                listPaquetes[index] = [];
                var fila = tBody.rows[i];

                for (var j = 0; j < 5; j++) {
                    // columnas de un paqute
                    listPaquetes[index][j] = fila.children[j]
                        ? $(fila.children[j]).html()
                        : "";
                }
                listPaquetes[index][6] = $(fila).prop('id');
                listPaquetes[index][7] = $(fila).attr('data-aduanal') ?? 0;
                listPaquetes[index][8] = $(fila).attr('data-aduanalId');
                listPaquetes[index][9] = $(fila).attr('data-rawmaterial'); // materia prima
                listPaquetes[index][10] = $(fila).attr('data-databaseId'); // id de la base de datos
                listPaquetes[index][11] = $(fila).attr('data-prodautom'); // saber si es producto automatico
                index += 1;
            }

            var listProducts = [];
            for (var i = 0; i < productsSelected.length; i++) {
                var item = productsSelected[i];
                listProducts.push({
                    id: item.product.idProducto,
                    qty: item.qty,
                    description: item.product.nombre,
                    price: item.product.precioVentaReferencial,
                });
            }

            var valorespagado = [];
            var tipopagos = [];
            var notas = [];
            if (blockRight == "False") {
                if ($("#checkpago").is(":checked")) {
                    valorespagado = [
                        $("#pagoCash").val().replace(",", "."),
                        $("#pagoZelle").val().replace(",", "."),
                        $("#pagoCheque").val().replace(",", "."),
                        $("#pagoCredito").val().replace(",", "."),
                        $("#pagoTransf").val().replace(",", "."),
                        $("#pagoWeb").val().replace(",", ".")
                    ];
                    notas = [
                        "",
                        $("#NotaZelle").val(),
                        $("#NotaCheque").val(),
                        "",
                        $("#NotaTransf").val(),
                        ""
                    ];
                    tipopagos = ['Cash', 'Zelle', 'Cheque', 'Crédito o Débito', 'Transferencia Bancaria', 'Web'];
                }
                else {
                    valorespagado = [$("#ValorPagado").val().replace(",", ".")];
                    tipopagos = [$(".hide-search-pago").val()];
                    notas = [$('#NotaPago').val()];
                }
            }

            let idValoresAduanales = [];
            let tBodyAduanales = $("#tblValoresAduanales > TBODY")[0];
            for (var i = 0; i < tBodyAduanales.rows.length; i++) {
                var fila = tBodyAduanales.rows[i];
                const id = $(fila).prop('id');
                idValoresAduanales.push(id);
            }

            let idInsuranceValues = [];
            let tBodyInsurance = $("#tblSeguros > TBODY")[0];
            for (var i = 0; i < tBodyInsurance.rows.length; i++) {
                var fila = tBodyInsurance.rows[i];
                const id = $(fila).prop('id');
                idInsuranceValues.push(id);
            }

            var datosOrden = [
                "", // 0
                $("#no_orden").html(), //                                                   1
                $(".select2-placeholder-selectClient").val(), //                            2
                $(".select2-placeholder-selectContact").val(), //                           3
                tipopagos, //                                                               4
                listPaquetes, //                                                            5
                idValoresAduanales, //                                                   6
                $("#Price").val().replace(",", "."), //                                     7
                $("#OtrosCostos").val().replace(",", "."), //                               8
                valorespagado, //                                                           9
                $("#ValorAduanal").val(), //                          10
                $("#precioTotalValue").html().replace(",", "."), //                         11
                $("#balanceValue").html().replace(",", "."), //                             12
                $('#orderNote').val(), //                                                   13
                $('#AuthTypeCard').val(), //                                                14
                $('#AuthCardCreditEnding').val(), //                                        15
                $('#AuthExpDate').val(), //                                                 16
                $('#AuthCCV').val(), //                                                     17
                $('#AuthaddressOfSend').val(), //                                           18
                $('#AuthOwnerAddressDiferent').val(), //                                    19
                $('#Authemail').val(), //                                                   20
                $('#Authphone').val(), //                                                   21
                $('#AuthSaleAmount').val().replace(",", "."),//                             22
                $('#AuthConvCharge').val().replace(",", "."),//                             23
                $('#TotalCharge').val().replace(",", "."),//                                24
                $('#selectMayorista').val(), //                                             25
                notas, //                                                                   26
                $('#express').is(':checked'), //                                            27 
                $('#check_credito').is(':checked') ?
                    $("#credito").html().replace(",", ".") : 0, //                          28
                $('#Descuento').val(), //                                                   29
                blockRight, //                                                              30
                $('#HandlingAndTransportation_Sale').val(), //                              31
                $('#Stamp').val(), //                                                       32
                $('#CargoAdicional').val(), //                                             33
                $('#CargoAdicionalDescription').val(), //                                  34
                idInsuranceValues, //                                                      35
                $('#InsuranceValue').val(), //                                             36
                listProducts, //                                                           37
                $('#EnaPassport').val(), //                                                38,
                $('#NoAduana').is(':checked'), //                                                   39
                $('#RecogidaAlmacen').is(':checked'), //                                            40
            ];
            guardarEnvio(datosOrden);
        }
        else {
            $("a[href=#finish]").unblock();
        }
        return true;
    };

    var isSend = false;

    function guardarEnvio(datosOrden) {
        //Guardo el envio
        $.ajax({
            type: "POST",
            url: "/OrderCubiq/EditOrderAV",
            data: JSON.stringify(datosOrden),
            dataType: 'json',
            contentType: 'application/json',
            async: true,
            beforeSend: function () {
            },
            success: function (data) {
                if (data.status == "success") {
                    window.location = "/OrderCubiq/Details/" + data.orderId + "?msg=success&orderNumber=" + $("#no_orden").html();
                }
                else {
                    toastr.error(data.msg, "ERROR");
                }
                isSend = false;
                $("a[href=#finish]").unblock();
            },
            failure: function (response) {
                showErrorMessage("FAILURE", "No se ha podido crear el trámite");
                $.unblockUI();
                isSend = false;
                $("a[href=#finish]").unblock();
            },
            error: function (response) {
                showErrorMessage("ERROR", "No se ha podido crear el trámite");
                $.unblockUI();
                isSend = false;
                $("a[href=#finish]").unblock();
            }
        });
    }

    var validateCreateOrder = function () {
        var tBody = $("#tblPaquetes > TBODY")[0];
        if ($("#Price").val() == "") {
            showWarningMessage("Atención", "El campo Precio no puede estar vacío.");
            return false;
        } else if ($("#OtrosCostos").val() == "") {
            showWarningMessage("Atención", "El campo Otros Cargos no puede estar vacío.");
            return false;
        } else if ($("#ValorPagado").val() == "") {
            showWarningMessage("Atención", "El campo Importe Pagado no puede estar vacío.");
            return false;
        } else if (parseFloat($("#balanceValue").html()) < 0) {
            showWarningMessage("Atención", "El Importe Pagado no puede ser superior al Precio Total.");
            return false;
        } else if ($("#Price").val() <= 0) {
            showWarningMessage("Atención", "El campo Precio debe ser mayor que 0.");
            return false;
        }
        else if ($("#OtrosCostos").val() < 0) {
            showWarningMessage("Atención", "El campo Otros Cargos debe ser mayor o igual que 0.");
            return false;
        }
        else if ($("#ValorPagado").val() < 0) {
            showWarningMessage("Atención", "El campo Importe Pagado debe ser mayor o igual que 0.");
            return false;
        }
        const splitLasName = $("#inputContactLastName").val().trim().split(' ');
        if (splitLasName.length <= 1) {
            showWarningMessage(
                "Atención",
                "El campo Apellidos debe contener el primer y segundo apellido."
            );
            return false;
        }
        return true;
    };

    var checkpago = function () {
        if ($("#checkpago").is(" :checked")) {

            $("#untipopago").attr("hidden", 'hidden');
            $(".multipopago").removeAttr("hidden");
        }
        else {
            $(".multipopago").attr("hidden", 'hidden');
            $("#untipopago").removeAttr("hidden");
        }
        calcMoney();
    }

    $("#checkpago").on('click', checkpago);
    $("#check_credito").on('click', function () {
        $("#pagoCash").val(0);
        $("#pagoZelle").val(0);
        $("#pagoCheque").val(0);
        $("#pagoCredito").val(0);
        $("#pagoTransf").val(0);
        $("#pagoWeb").val(0);
    });

    //Mostrar y ocultar el credit card fee
    $('[name = "TipoPago"]').on('change', function () {
        var Id = $(this).val();
        var tipoPago = $('option[value = "' + Id + '"]').html();
        if (tipoPago == "Crédito o Débito") {
            $('#contfee').show();
            calcMoney();
        }
        else {
            $('#contfee').hide();
            calcMoney();
        }
    });

    $("#OtrosCostos,#check_credito,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb,#Descuento,#HandlingAndTransportation_Sale,#CargoAdicional").on("keyup", calcMoney).on("change", calcMoney);

    $("#ValorPagado").on("keyup", calcMoneyPayment).on("change", calcMoneyPayment);
    $("#newPesoLb").on("change input", calcNewRow);
    $("#select-aduanal").on("change", calcNewRow);

    $("#Type").on("change", function () {
        if ($(this).val() == "MARITIMO") {
            $('#container-seguros').show();
            $('#div_ena').show()
        }
        else {
            $('#container-seguros').hide();
            $('#ena_checkbox').prop('checked', false).trigger("change");

            $('#div_ena').hide()
        }
    });

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

    var addPaqueteToTable = async function (aduanalId, descripcion, type, pesoLb, precio, rawMaterial, aduanal, isProdAutom, reference, databaseId) {
        //Get the reference of the Table's TBODY element.
        var tBody = $("#tblPaquetes > TBODY")[0];

        var row = undefined;
        var id = Math.random().toString(36).substr(2, 9);
        let isParent = false;
        if (reference) {
            //insertar fila en la posicion siguiente de la fila con id de referencia
            var refRow = $("#" + reference)[0];

            if (!refRow) { // Para cuando se inserta un paquete padre que no existe en la tabla
                id = reference;
                var row = tBody.insertRow(-1);
                row.id = id;
                isParent = true;
            }
            else {
                row = tBody.insertRow(refRow.rowIndex);
                //agregar background a la fila
                $(row).prop("style", "background-color: #ededed");
                id = `${reference}-${id}`;
                row.id = id;
            }
        }
        else {
            var row = tBody.insertRow(-1);
            row.id = id;
            isParent = true;
        }

        if (databaseId) {
            $(row).attr("data-databaseId", databaseId);
        }

        $(row).attr("data-rawmaterial", rawMaterial);
        $(row).attr("data-aduanal", aduanal);
        $(row).attr("data-prodautom", isProdAutom);
        if (aduanalId) {
            const itemAduana = valoresAduanales.find(v => v.id == aduanalId);
            $(row).attr("data-aduanalId", itemAduana.id);

            var cell0 = $(row.insertCell(-1));
            cell0.append(itemAduana.name);
        }
        else {
            var cell0 = $(row.insertCell(-1));
            cell0.append("");
        }

        var cell1 = $(row.insertCell(-1));
        cell1.append(descripcion);

        var cell2 = $(row.insertCell(-1));
        cell2.append(type);

        var cell3 = $(row.insertCell(-1));
        cell3.append(pesoLb);

        var cell4 = $(row.insertCell(-1));
        cell4.append(precio);

        var cell5 = $(row.insertCell(-1));
        cell5.prop("style", "display: flex");
        var btnEdit = $(
            "<button type='button' class='btn btn-warning ml-1' title='Editar' style='font-size: 10px'><i class='fa fa-pencil'></i></button>"
        );
        var btnRemove = $(
            "<button type='button' class='btn btn-danger pull-right ml-1' title='Eliminar' style='font-size: 10px'><i class=' fa fa-close'></button>"
        );
        var btnConfirm = $(
            "<button type='button' class='btn btn-success hidden ml-1' title='Confirmar' style='font-size: 10px'><i class='fa fa-check'></button>"
        );
        var btnAdd = $(
            "<button type='button' class='btn btn-info ml-1' title='Agregar' style='font-size: 10px'><i class='fa fa-plus'></button>"
        );
        btnEdit.on("click", function () {
            $("#Price").val(parseFloat($("#Price").val() - parseFloat(cell4.html())).toFixed(2));

            const refVAId = $(row).attr('data-aduanalId')

            // crear select en cell0 de valores aduanales usando el listado valoresAduanales y que se seleccione el id refVAId
            const itemSelectVA = "<select class='form-control' style='width:100%' name='cell0'> <option value=''></option>" + valoresAduanales.map(v => `<option value='${v.id}' ${v.id == refVAId ? 'selected' : ''}>${v.name} -$${v.value}</option>`).join('') + "</select>";
            cell0.html(itemSelectVA);
            cell0.find('select').select2({
                placeholder: "Seleccione un valor aduanal"
            });

            cell1.html("<input name='cell1' class='form-control' value='" + cell1.html() + "'/>");

            var itemSelect = "";
            if (cell2.html() == "Miscelaneo") {
                itemSelect = "<select class='form-control' name='cell2'  value='" + cell2.html() + "'> <option value='Miscelaneo' selected>Miscelaneos</option> <option value='Duradero'>Duradero</option> </select>"
            }
            else if (cell2.html() == "Duradero") {
                itemSelect = "<select class='form-control' name='cell2'  value='" + cell2.html() + "'> <option value='Miscelaneo'>Miscelaneos</option><option value='Duradero' selected>Duradero</option> </select>"
            }
            else if (cell2.html() == "") {
                itemSelect = "<select class='form-control' name='cell2'  value=''> <option value=''></option><option value='Miscelaneo'>Miscelaneos</option> <option value='Duradero'>Duradero</option> </select>"
            }
            cell2.html(itemSelect);

            cell3.html(
                "<input type='number' name='cell3' step='0.01' class='form-control' value='" +
                cell3.html() +
                "'/>"
            );
            cell4.html(
                "<input type='number' name='cell4' step='0.01' class='form-control' value='" +
                cell4.html() +
                "'/>"
            );
            $("[name='cell0'],[name='cell2'],[name='cell3']").on("change", function () {
                const aduanalId = $("[name='cell0']").val();
                const pesoAux = parseFloat($("[name='cell3']").val());
                const packageTypeAux = $("[name='cell2']").val();

                const res = calcProduct(pesoAux, packageTypeAux, aduanalId);

                $("[name='cell4']").val(res.productPrice.toFixed(2));
            });
            btnConfirm.removeClass("hidden");
            btnEdit.addClass("hidden");
            btnRemove.addClass("hidden");
            calcMoney();
        });
        btnRemove.on("click", function () {
            $("#Price").val(
                parseFloat($("#Price").val() - parseFloat(cell4.html())).toFixed(2)
            );
            row.remove();
            $("#noPaquetes").html($("#tblPaquetes > TBODY")[0].rows.length - 1);

            $("#select-aduanal").removeAttr("disabled");
            $("#newDescrip").removeAttr("disabled");
            $("#newPesoLb").removeAttr("disabled");
            $("#newPrecio").removeAttr("disabled");
            $("#package-type").removeAttr("disabled");
            calcMoney();
        });
        btnConfirm.on("click", function () {
            if (validateEditPaquete()) {
                const aduanalId = $("[name='cell0']").val();
                const pesoAux = parseFloat($("[name='cell3']").val());
                const packageTypeAux = $("[name='cell2']").val();

                const res = calcProduct(pesoAux, packageTypeAux, aduanalId);

                $(row).attr("data-aduanal", res.productAduanal);
                $(row).attr("data-aduanalId", aduanalId);
                $(row).attr("data-rawmaterial", res.productRawMaterial);

                if (aduanalId) {
                    const itemAduana = valoresAduanales.find(v => v.id == aduanalId);
                    cell0.html(itemAduana.name);
                }
                else {
                    cell0.html("");
                }

                cell1.html($("[name='cell1']").val());
                cell2.html(packageTypeAux);
                cell3.html(pesoAux);
                cell4.html(res.productPrice.toFixed(2));
                $("#Price").val(
                    (parseFloat($("#Price").val()) + parseFloat(cell4.html())).toFixed(2)
                );
                btnConfirm.addClass("hidden");
                btnEdit.removeClass("hidden");
                btnRemove.removeClass("hidden");
                calcMoney();
            }
        });
        btnAdd.on("click", function () {
            // validar datos
            addPaqueteToTable(
                "",
                "",
                "",
                "0",
                "0",
                "0",
                "0",
                "0",
                row.id
            );
        });

        if (reference && !isParent) {
            cell5.append(btnEdit.add(btnRemove).add(btnConfirm));
        }
        else {
            cell5.append(btnEdit.add(btnRemove).add(btnConfirm).add(btnAdd));
        }


        const precioVal = parseFloat(precio);
        if (precioVal > 0) {
            await actualizarPrecio(precioVal);
        }

        $("#noPaquetes").html($("#tblPaquetes > TBODY")[0].rows.length - 1);

        if (reference && !databaseId) {
            $(btnEdit).trigger('click');
        }
    };

    async function actualizarPrecio(precio) {
        return new Promise((resolve, reject) => {
            try {
                var currentPrice = parseFloat($("#Price").val());
                currentPrice += parseFloat(precio);
                $("#Price").val(currentPrice.toFixed(2));
                resolve();
            } catch (error) {
                reject(error);
            }
        });
    }


    $("#btnAdd").on("click", function () {
        if (validateAddPaquete()) {
            calcNewRow();
            const cant = 1; // cantidad de items a agregar
            var elementTable = $("#tblPaquetes > TBODY")[0].rows.length - 1;
            if ((elementTable + cant) > 10) {
                toastr.info("El total de paquetes no debe ser mayor que 10");
                return false;
            }
            if (cant >= 0) {
                for (var i = 0; i < cant; i++) {
                    addPaqueteToTable(
                        $("#select-aduanal").val(),
                        $("#newDescrip").val(),
                        $("#package-type").val(),
                        $("#newPesoLb").val(),
                        $("#newPrecio").val(),
                        $("#newProductRawMaterial").val(),
                        $("#newAduanal").val(),
                        $("#newProdAutom").val()
                    );
                }

                $("#select-aduanal").val("").trigger('change');
                $("#newPesoLb").val("0");
                $("#newPrecio").val("0");
                $("#newDescrip").val("");
                $("#package-type").val("");
                $("#newProductRawMaterial").val("0");
                $("#newAduanal").val("0");
                $("#newProdAutom").val("0");
                $("#newDescrip").focus();
                calcMoney();
            }
            else {
                toastr.info("La cantidad de paquetes a añadir debe ser mayor que 0");
            }
        }
    });

    $('#package-type').on('change', function () {
        var type = $(this).val();
        if (type == "Miscelaneo") {
            $('#newPesoLb').val('0').trigger('change');
        }
        else {
            calcNewRow();
        }
    })

    $("#newPesoLb,#newPrecio").on("keypress", function (a) {
        if (a.key == "Enter" && validateAddPaquete()) {
            calcNewRow();
            addPaqueteToTable(
                $("#select-aduanal").val(),
                $("#newDescrip").val(),
                $("#package-type").val(),
                $("#newPesoLb").val(),
                $("#newPrecio").val(),
                $("#newProductRawMaterial").val(),
                $("#newAduanal").val(),
                $("#newProdAutom").val()
            );

            $("#select-aduanal").val("").trigger('change');
            $("#newPesoLb").val("0");
            $("#newPrecio").val("0");
            $("#newDescrip").val("");
            $("#package-type").val("");
            $("#newProductRawMaterial").val("0");
            $("#newAduanal").val("0");
            $("#newProdAutom").val("0");
            $("#newDescrip").focus();
            calcMoney();
        }
    });

    $('#newDescrip').on('keypress', function (e) {
        if (e.key == 'Enter') {
            $('#newPesoLb').focus();
        }
    });

    /**************AuthCard************************************/
    $('[name="TipoPago"]').on('change', function () {
        var id = $(this).val();
        var value = $('option[value = "' + id + '"]').html();
        if (value == "Crédito o Débito") {
            $('#AddAuthorization').show();
        }
        else {
            $('#AddAuthorization').hide();

        }
        if (value == "Zelle" || value == "Cheque" || value == "Transferencia Bancaria") {
            $('#contNotaPago').show();
        }
        else {
            $('#contNotaPago').hide();
        }
    });

    $('[name = "AuthConvCharge"]').on('change', function () {
        calcAuthCard();
    });

    $('#ConfirmAuth').click(function () {
        var error = false;
        if ($('[name="AuthTypeCard"]').val() == "") {
            error = true;
        }

        if ($('[name="AuthCardCreditEnding"]').val() == "") {
            error = true;
        }

        if ($('[name="AuthExpDate"]').val() == "") {
            error = true;
        }

        if ($('[name="AuthCCV"]').val() == "") {
            error = true;
        }

        if ($('[name="AuthaddressOfSend"]').val() == "") {
            error = true;
        }

        if ($('[name="AuthOwnerAddressDiferent"]').val() == "") {
            error = true;
        }

        if ($('[name="Authemail"]').val() == "") {
            error = true;
        }

        if ($('[name="Authphone"]').val() == "") {
            error = true;
        }

        if ($('[name="AuthConvCharge"]').val() == "") {
            error = true;
        }

        if ($('[name="TotalCharge"]').val() == "") {
            error = true;
        }

        if ($('[name="AuthSaleAmount"]').val() == "") {
            error = true;
        }

        $('#AuthorizationCard').modal("hide");
        $('body').removeClass('modal-open');
        $('.modal-backdrop').remove();
        if (!error) {

            $('#IcoAuthorizationAdd').attr('class', 'fa fa-check-circle')
            $('#AddAuthorization').attr('class', 'btn mr-1 mb-1 btn-success')
        }
        else {
            $('#IcoAuthorizationAdd').attr('class', 'fa fa-plus-circle')
            $('#AddAuthorization').attr('class', 'btn mr-1 mb-1 btn-secondary')
        }
    });

    function calcAuthCard() {
        var saleamount = parseFloat($('[name = "AuthSaleAmount"]').val());
        var aconvcharge = parseFloat($('[name = "AuthConvCharge"]').val());
        var total = saleamount + aconvcharge;
        $('[name = "TotalCharge"]').val(total.toFixed(2));
    }

    $(document).ready(function () {
        $(".select2-container--default").attr("style", "width: 100%;");
    });

    // Cuando se cree un nuevo cliente al recargarse se muestre
    if (selectedClient != '00000000-0000-0000-0000-000000000000') {
        $.ajax({
            type: "POST",
            url: "/Clients/GetClient",
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(selectedClient),
            async: false,
            success: function (data) {
                var newOption = new Option(data.movil + "-" + data.name + " " + data.lastName, selectedClient, false, false);
                $("[name='selectClient']").append(newOption);
                $(".select2-placeholder-selectClient").val(selectedClient).trigger("change").trigger("select2:select");

                //Datos del Cliente en Step 1
                $('#inputClientName').val(data.name);
                $('#inputClientLastName').val(data.lastName);
                $('#inputClientMovil').val(data.movil);
                $('#inputClientEmail').val(data.email);
                $('#inputClientAddress').val(data.calle);
                $('#inputClientCity').val(data.city);
                $('#inputClientZip').val(data.zip);
                $('#inputClientId').val(data.id);
                $('#inputClientState').val(data.state).trigger("change");
                $('#credito').html(data.credito);
                if (data.credito && data.credito != 0)
                    $("#div_credito").removeAttr('hidden');

                $('#remitente').html(data.name + " " + data.lastName);
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            }
        });
    }


    if (pagos.length == 1) {
        $('select[name = "TipoPago"] option[value="' + pagos[0][1] + '"]').attr('selected', 'selected').trigger("change");
    }
    var value = $('select[name = "TipoPago"] option:selected').text();
    if (value == "Crédito o Débito") {
        $('#AddAuthorization').show();
    }
    if (value == "Zelle" || value == "Cheque" || value == "Transferencia Bancaria") {
        $('#contNotaPago').show();
    }

    function loadSavedPaquetes() {
        // obtener la longitud del body de la tabla tblPaquetes
        const tBody = $("#tblPaquetes > TBODY")[0];

        if (paquetes != [] && tBody.rows.length == 1) {
            const conjuntos = agruparPaquetes(paquetes);
            for (var i = 0; i < conjuntos.length; i++) {
                const conjunto = conjuntos[i];
                if (conjunto.length > 1) {
                    const prd_parent = conjunto.find(producto => producto.number.endsWith('-1'));
                    const number = prd_parent.number.split('-')[0];
                    addPaqueteToTable(prd_parent.aduanalId, prd_parent.descripcion, prd_parent.type, prd_parent.peso, prd_parent.precio, prd_parent.RawMaterial, prd_parent.aduanal, prd_parent.isAutomatico == true ? "1" : "0", number, prd_parent.paqueteId);
                    const prd_childs = conjunto.filter(producto => !producto.number.endsWith('-1'));
                    for (var j = 0; j < prd_childs.length; j++) {
                        const prd = prd_childs[j];
                        addPaqueteToTable(prd.aduanalId, prd.descripcion, prd.type, prd.peso, prd.precio, prd.RawMaterial, prd.aduanal, prd.isAutomatico == true ? "1" : "0", number, prd.paqueteId);
                    }
                }
                else {
                    const prd = conjunto[0];
                    addPaqueteToTable(prd.aduanalId, prd.descripcion, prd.type, prd.peso, prd.precio, prd.RawMaterial, prd.aduanal, prd.isAutomatico == true ? "1" : "0", prd.number, prd.paqueteId);
                }
            }
        }
    }

    $('#btn-add-seguro').on('click', function () {
        const id = $('#select-seguros').val();
        addSegurosToTable(id);
    })

    function addSegurosToTable(id) {
        if (id) {
            const seguro = cargoSeguros.find(x => x.id == id);
            if (seguro) {
                // agregar a tabla
                const tBody = $('#tblSeguros > TBODY')[0];
                const row = tBody.insertRow(-1);
                row.id = id;
                const cell0 = $(row.insertCell(-1))
                cell0.append(seguro.name);
                const cell1 = $(row.insertCell(-1))
                cell1.append(seguro.value);
                const cell2 = $(row.insertCell(-1))
                const btnRemove = $('<button type="button" class="btn btn-danger" title="Eliminar" style="font-size: 10px"><i class=" fa fa-close"></button>');
                btnRemove.on('click', function () {
                    row.remove();
                    renderTotalSeguros();
                });
                cell2.append(btnRemove);
                renderTotalSeguros();
                $('#select-seguros').val("").trigger("change");
            }
            else {
                toastr.error("No se ha encontrado el cargo de seguro seleccionado");
            }

        }
    }

    function renderTotalSeguros() {
        const tBody = $('#tblSeguros > TBODY')[0];
        let total = 0;
        for (var i = 0; i < tBody.rows.length; i++) {
            const fila = tBody.rows[i];
            const price = parseFloat(fila.children[1].innerText);
            total += price;
        }

        $('#InsuranceValue').val(total);
        $('#displayCargoSeguros').html(total.toFixed(2));

        calcMoney();
    }

    function inicializarInsuranceValues() {
        const tBody = $("#tblSeguros > TBODY")[0];

        if (segurosId.length > 0 && tBody.rows.length == 0) {
            for (var i = 0; i < segurosId.length; i++) {
                const seguroId = segurosId[i];
                addSegurosToTable(seguroId);
            }
        }

        renderTotalSeguros();
    }

    function agruparPaquetes(productos) {
        const regex = /-\d+$/;
        const grupos = {};

        productos.forEach(producto => {
            const match = producto.number.match(regex);
            if (match) {
                const prefijo = producto.number.split(match[0])[0]
                if (!grupos[prefijo]) {
                    grupos[prefijo] = [];
                }
                grupos[prefijo].push(producto);
            }
            else {
                // si no tiene sufijo
                if (!grupos[producto.number]) {
                    grupos[producto.number] = [];
                }

                grupos[producto.number].push(producto);
            }
        });

        return Object.values(grupos);
    };

    function calcNewRow() {
        var aduanalId = $('#select-aduanal').val();
        var packageType = $("#package-type").val();
        var peso = parseFloat($("#newPesoLb").val());

        const res = calcProduct(peso, packageType, aduanalId);

        $("#newPrecio").val(res.productPrice.toFixed(2));
        $("#newAduanal").val(res.productAduanal.toFixed(2));
        $("#newProductRawMaterial").val(res.productRawMaterial.toFixed(2));
    };

    function calcProduct(peso, packageType, valorAduanalId) {
        // obtener nombre del mayorista seleccionado
        const wholesaler = $('#selectMayorista').val();
        let wholesalerName = "";
        if (wholesaler) {
            // buscar el option del select con el valor del mayorista
            wholesalerName = $('#selectMayorista option[value="' + wholesaler + '"]').html();
        }

        const type = $("#Type").val();
        let productPrice = 0;
        let productAduanal = 0;
        let productRawMaterial = 0;
        let pesoVolum = 0;

        if (valorAduanalId) {
            var aduanalItem = valoresAduanales.find(x => x.id == valorAduanalId);
            if (!aduanalItem) {
                toastr.error("No se ha podido obtener el valor aduanal");
                return;
            }

            productAduanal = parseFloat(aduanalItem.value);
        }

        /*if (packageType == "Duradero") {
            pesoVolum = ((alto * ancho * largo) / 366) * 2.20462;
        }

        peso = peso > pesoVolum ? peso : pesoVolum;*/

        var precioBD = 0;
        if (priceAv != null) {
            if (type == "AEREO") {
                if (packageType == "Duradero") {
                    precioBD = priceAv.valor1;
                }
                else {
                    precioBD = priceAv.valor2;
                }
            }
            else if (type == "MARITIMO") {
                if (packageType == "Duradero") {
                    precioBD = priceAv.valor3;
                }
                else {
                    precioBD = priceAv.valor4;
                }
            }
            else {
                toastr.error("No se ha podido obtener el precio del producto. No se ha seleccionado el tipo de trámite.")
                return;
            }
        }
        else {
            toastr.error(" Favor:  Regrese al paso anterior. Revisar que los Campos ID del Cliente esten completos y los campos del Destinatario Provincia, Municipio y CI esten completos.")
            return;
        }

        if ((agencyId == agencyFly || agencyId == agencyLogisticExports) && packageType == "Miscelaneo") { // para FlyAwayTravel en miscelaneos de maritimo
            const configWeight = [
                {
                    min: 100,
                    max: 10000,
                    price: 1.5,
                    rawMaterial: 10,
                    aduanal: 11
                },
                {
                    min: 50,
                    max: 100,
                    price: 1.8,
                    rawMaterial: 10,
                    aduanal: 11
                },
                {
                    min: 6,
                    max: 50,
                    price: 1.8,
                    rawMaterial: 5,
                    aduanal: 5
                }
            ];

            const configAux = configWeight.find(c => peso >= c.min && peso < c.max);
            if (configAux) {
                productAduanal += configAux.aduanal;
                productPrice = precioBD * peso + configAux.rawMaterial + productAduanal;
                productRawMaterial = configAux.rawMaterial;
            }
            if (peso == 100 && type == "MARITIMO") productPrice = 170;

        }
        else if (agencyId == agencyMdlTravel && packageType == "Miscelaneo") {
            if (peso <= 11) productPrice = 35;
            else if (peso > 11 && peso <= 22) productPrice = 55;
            else if (peso > 22 && peso <= 44) productPrice = 90;
            else if (peso > 44 && peso <= 77) productPrice = 120;
            else if (peso > 77 && peso <= 100) productPrice = 140;
            else {
                productPrice = precioBD * peso + productAduanal;
            }
        }
        else {
            productPrice = precioBD * peso + productAduanal;
        }

        //if (peso > 100) productPrice = productPrice + 30;

        return {
            productPrice,
            productAduanal,
            productRawMaterial
        }
    }

    $('#btn-add-aduanal').on('click', function () {
        const value = $('#select-aduanal-order').val();
        if (value) {
            const option = $('#select-aduanal-order option[value="' + value + '"]');
            const text = option.text();
            const price = parseFloat(text.split('$')[1]);

            // agregar valor aduanal a tabla tblValoresAduanales
            const tBody = $('#tblValoresAduanales > TBODY')[0];
            const row = tBody.insertRow(-1);
            row.id = value;
            const cell0 = $(row.insertCell(-1))
            cell0.append(text);
            const cell1 = $(row.insertCell(-1))
            cell1.append(price);

            const cell2 = $(row.insertCell(-1))
            const btnRemove = $('<button type="button" class="btn btn-danger" title="Eliminar" style="font-size: 10px"><i class=" fa fa-close"></button>');
            btnRemove.on('click', function () {
                row.remove();
                calcMoney();
            });
            cell2.append(btnRemove);

            $('#select-aduanal-order').val("").trigger("change");
        }

        calcMoney();
    })

    function displayValorAduanal() {
        // tabla de productos
        let aduanal = 0;
        let libras = 0;
        var tBody = $("#tblPaquetes > TBODY")[0];
        for (var i = 1; i < tBody.rows.length; i++) {
            var fila = tBody.rows[i];
            aduanal += parseFloat($(fila).attr('data-aduanal') ?? 0);
            libras += parseFloat(fila.children[3].innerText);
        }

        // tabla de valores aduanales
        const tBody2 = $('#tblValoresAduanales > TBODY')[0];
        let order_aduanal = 0;
        for (var i = 0; i < tBody2.rows.length; i++) {
            const fila = tBody2.rows[i];
            const price = parseFloat(fila.children[1].innerText);
            aduanal += price;
            order_aduanal += price;
        }

        $('#ValorAduanal').val(order_aduanal.toFixed(2));
        $('#displayValorAduanal').html(aduanal.toFixed(2))
        $('#noLibras').html(libras.toFixed(2));
    }

    async function renderSelectVA(selector) {
        var wholesaler = $('#selectMayorista').val();
        valoresAduanales = await getValoresAduanales(wholesaler);
        $(selector).empty();
        $(selector).append($('<option>', {
            value: "",
            text: "Seleccione un valor aduanal"
        }));
        $.each(valoresAduanales, function (index, item) {
            $(selector).append($('<option>', {
                value: item.id,
                text: item.name + " - $" + item.value
            }));
        });
    }

    function inicializarValoresAduanales() {
        const tBody = $("#tblValoresAduanales > TBODY")[0];

        if (aduanalItemsSaved.length > 0 && tBody.rows.length == 0) {
            for (var i = 0; i < aduanalItemsSaved.length; i++) {
                const item = aduanalItemsSaved[i];
                $('#select-aduanal-order').val(item.id).trigger("change");
                $('#btn-add-aduanal').trigger('click');
            }

            $('#select-aduanal-order').val("").trigger("change");
        }
    }

    function getCostoEnvio() {
        if (agencyId == agencyMiIslaServicesFlagler || agencyId == agencyMiIslaServices) {
            const provinces = ["La Habana", "Matanzas", "Cienfuegos", "Villa Clara", "Sancti Spíritus", "Ciego de Ávila", "Camaguey"]
            const contactProvince = $("#provincia").val();
            if (provinces.includes(contactProvince)) {
                $('#HandlingAndTransportation_Sale').val(0);
                $('#displayCostoEnvio').html(0);
                calcMoney();
                return;
            }
        }

        var wholesalerId = $('#selectMayorista').val();
        $.ajax({
            type: "GET",
            url: "/ordercubiq/GetCostoEnvio",
            async: false,
            data: {
                wholesalerId: wholesalerId,
                province: $('#provincia').val(),
                municipality: $('#municipio').val()
            },
            success: function (data) {
                let costoEnvio = 0
                if (data.success) {
                    costoEnvio = data.costoEnvio;
                }
                $('#HandlingAndTransportation_Sale').val(costoEnvio.toFixed(2));
                $('#displayCostoEnvio').html(costoEnvio.toFixed(2));
                calcMoney();
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
        });
    }

    function getCargosSeguros() {
        var wholesalerId = $('#selectMayorista').val();

        $.ajax({
            type: "GET",
            url: "/ordercubiq/GetCargosSeguro",
            async: false,
            data: {
                wholesalerId: wholesalerId
            },
            success: function (data) {
                if (data.success) {
                    cargoSeguros = data.data;
                    // cargar seguros en select
                    $('#select-seguros').empty();
                    $('#select-seguros').append($('<option>', {
                        value: "",
                        text: "Seleccione un seguro"
                    }));
                    $.each(data.data, function (index, item) {
                        $('#select-seguros').append($('<option>', {
                            value: item.id,
                            text: item.name
                        }));
                    });
                }
                else {
                    showErrorMessage("ERROR", data.msg);
                }
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
        });
    }

    async function InitSelectProduct() {
        var data = await $.get("/BodegaProducto/getProduct?category=Tienda");

        if (data && data.length > 0) {
            $('#div_productos').show();
        }

        $('[name = "selectProduct"]').empty();
        $('[name = "selectProduct"]').append(
            new Option("Seleccione los productos", "", true)
        );
        if (data.length != 0) {
            productos = data;
            for (var i = 0; i < data.length; i++) {
                var x = new Option(data[i].productName, data[i].productId);
                x.title = data[i].description;
                $('[name = "selectProduct"]').append(x);
            }
        }
    }

    async function GetProduct(id) {
        return await $.post("/ProductoBodega/getProducto", {
            id: id,
            function(response) {
                return response;
            },
        });
    }

    $(".select2-placeholder-selectProduct")
        .on("select2:select", async function () {
            var productId = $(this).val();
            var product = await GetProduct(productId);
            var productItem = productsSelected.find((x) => x.product.idProducto == product.idProducto);
            if (!productItem) {
                productsSelected.push({
                    qty: 1,
                    product: product
                });

            } else {
                productItem.qty += 1;
            }


            var currentprice = parseFloat($("#Price").val());
            currentprice += parseFloat(product.precioVentaReferencial);
            $("#Price").val(currentprice.toFixed(2));

            $(".select2-placeholder-selectProduct").val("").trigger("change");
            renderTableProducts();
        })
        .data("select2")
        .$container.find(".select2-selection")
        .css("background-color", "#ffc8004a");

    function renderTableProducts() {
        //agregar productos a tabla tblProductos
        var tBody = $("#tblProductos > TBODY")[0];
        tBody.innerHTML = "";
        for (var i = 0; i < productsSelected.length; i++) {
            var row = tBody.insertRow(-1);
            row.id = productsSelected[i].product.idProducto;
            var cell0 = row.insertCell(0);
            cell0.innerHTML = productsSelected[i].qty;
            var cell1 = row.insertCell(1);
            cell1.innerHTML = productsSelected[i].product.nombre;
            var cell2 = row.insertCell(2);
            cell2.innerHTML = productsSelected[i].product.precioVentaReferencial;
            var cell3 = $(row.insertCell(3));
            var btnRemove = $('<button type="button" class="btn btn-danger" title="Eliminar" style="font-size: 10px"><i class=" fa fa-close"></button>');
            btnRemove.on('click', function () {
                // obtener el id de la fila
                var id = $(this).closest('tr').attr('id');
                // reducir precio
                var currentprice = parseFloat($("#Price").val());
                const prodSelected = productsSelected.find(x => x.product.idProducto == id);
                currentprice -= parseFloat(prodSelected.product.precioVentaReferencial) * prodSelected.qty;
                $("#Price").val(currentprice.toFixed(2));
                // eliminar el producto de la lista
                productsSelected = productsSelected.filter(x => x.product.idProducto != id);
                renderTableProducts();
            });
            cell3.append(btnRemove);
        }

        calcMoney();
    }

    async function inicializarProductosGuardados() {
        const tBody = $("#tblProductos > TBODY")[0];

        if (productosSaved.length > 0 && tBody.rows.length == 0) {
            $('#div_productos').show();
            for (var i = 0; i < productosSaved.length; i++) {
                const prd = productosSaved[i];
                const product = await GetProduct(prd.id);
                productsSelected.push({
                    qty: parseInt(prd.qty),
                    product: product
                });

                await actualizarPrecio(product.precioVentaReferencial);
            }

            renderTableProducts();
        }
    }

    $('#btn-addproductautom').on('click', function () {
        $('#modalProductosAutom').modal('show');
    })

    $('#confirm-productosautom').on('click', function () {
        const value = $('#select-productosautom').val();
        if (!value) {
            toastr.warning("Debe seleccionar un producto");
            return;
        }

        $('#newProdAutom').val("1");
        $('#newDescrip').val(value);
        $('#modalProductosAutom').modal('hide');
    })

    function loadProductosAutom(agencyRef) {
        $('#select-productosautom').select2({
            placeholder: "Seleccione un producto"
        })

        let options = [];

        if (agencyRef == agencyFly) {
            options = [
                "Misceláneas y otros artículos al peso (KG) (MISCELÁNEAS)",
                "Cocinas y hornillas eléctricas, sin horno",
                "Horno microonda",
                "Olla arrocera eléctrica",
                "Olla vaporera eléctrica",
                "Olla multifunción eléctrica",
                "Freidora eléctrica",
                "Máquinas lavadoras/ secadoras domésticas",
                "Cafetera eléctrica",
                "Ventiladores",
                "Plantas generadoras de electricidad hasta 900 va",
                "Bombas de agua o compresores para agua",
                "Impresoras. Fotocopiadoras",
                "Teléfonos celulares o inteligentes",
                "Colchones",
                "Ciclomotores o motocicletas eléctricas, con o sin sidecar",
                "Baterías o acumuladores de litio",
                "Otras baterías y acumuladores",
                "Kit o set de baterías de gel",
                "Neumáticos para autos ligeros (con o sin cámaras)",
                "Neumáticos para motos o ciclomotores (con o sin cámaras)",
                "Llantas para autos ligeros",
                "Llantas para motos o ciclomotores",
                "Bombas de aceite, combustible, agua y cloche",
                "Bicicletas eléctricas y de pedaleo asistido",
                "Herramientas con motor incorporado (afiladoras, atornilladores eléctricos y neumáticos, cepillos, clavadoras, decapadoras, grapadoras, lijadoras, martillos, remachadoras, roscadoras, sierras circulares, taladros y pulidoras, compresores neumáticos pequeños, motosierras)",
                "Medicamentos",
                "Alimentos, aseo, medicamentos e insumos Exento",
                "Literatura"
            ]
        }
        else if (agencyRef == agencyRapiCargo) {
            options = [
                "Motos"
            ]
        }

        options.map(x => $('#select-productosautom').append(new Option(x, x, false, false)));
    }
    loadProductosAutom();

    if (agencyId == agencyFly) {
        loadProductosAutom(agencyFly);
    }
    if (agencyId == agencyRapiCargo) {
        loadProductosAutom(agencyRapiCargo);
    }
    else {
        $('#btn-addproductautom').hide();
    }
});