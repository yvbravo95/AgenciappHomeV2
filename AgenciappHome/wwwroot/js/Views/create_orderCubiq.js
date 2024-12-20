var selected_municipio;
var precioCB = 0;
var precio1 = 0;
var precio2 = 0;
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

    //Guardo en esta variable si existe un mayorista por transferencia
    var idMayoristabyTransferencia = "";

    //Para cargar los datos del mayorista
    $(document).on('change', '#selectMayorista', function () {
        var id = $(this).val();
        var nombre = $('option[value="' + $(this).val() + '"]').html();
        // Si existe un mayorista por transferencia envio una alerta
        if (idMayoristabyTransferencia != "" && idMayoristabyTransferencia != id) {
            swal({
                title: "Está usted seguro",
                text: "Esta seguro que desea cambiar al mayorista por transferencia a " + nombre,
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DA4453",
                confirmButtonText: "Si, cambiar",
                cancelButtonText: "No, mantener",
                closeOnConfirm: false,
                closeOnCancel: false
            }, function (isConfirm) {
                if (isConfirm) {
                    idMayoristabyTransferencia = "";
                    swal.close();
                    aux(id);
                } else {
                    $('#selectMayorista').val(idMayoristabyTransferencia).trigger("change");

                    swal.close();

                }
            });
        }
    });

    function aux(idmayorista) {
        $.ajax({
            type: "POST",
            url: "/OrderNew/getMayorista",
            data: {
                id: idmayorista
            },
            async: false,
            success: function (data) {
                $("#select2-selectMayorista-container").attr('title', data.termsConditions);
                getprecioxlibra();
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    }

    function getMayorista(category) {
        $.ajax({
            type: "POST",
            url: "/OrderNew/getMayoristas",
            data: {
                category: category,
            },
            async: false,
            success: function (data) {
                idMayoristabyTransferencia = "";
                $("[name='selectMayorista']").val("").trigger("change");
                $("#selectMayorista").empty();
                $("#selectMayorista").append(new Option("Seleccione un mayorista", "", true));

                if (data.length != 0) {
                    for (var i = 0; i < data.length; i++) {
                        $("[name='selectMayorista']").append(new Option(data[i].name, data[i].idWholesaler));
                        if (data[i].byTransferencia) {
                            idMayoristabyTransferencia = data[i].idWholesaler;
                            $("[name='selectMayorista']").val(idMayoristabyTransferencia).trigger("change");
                            aux(idMayoristabyTransferencia);
                        }
                    }
                }
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
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
            finish: 'Enviar orden'
        },
        onStepChanging: function (event, currentIndex, newIndex) {
            // Allways allow previous action even if the current form is not valid!
            if (currentIndex > newIndex) {
                return true;
            }

            //----------------Step1
            var error = false;

            if (newIndex == 0) {
                alert("df   ")
            }

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
                getMayorista("Cubiq");
                getprecioxlibra();

            }

            //----------------Step2

            if (newIndex == 2) {
                if ($("#txtNameVA").val() == "")
                    return false;
                else if ($("#tblProductos > TBODY")[0].rows.length == 1) {
                    return false;
                }
                else {
                    //Preparando el step3 con los productos seleccinados
                    var listProduct = [];

                    var tBody = $("#tblProductos > TBODY")[0];

                    for (var i = 0; i < tBody.rows.length - 1; i++) {
                        var fila = tBody.rows[i];
                        listProduct[i] = new Array;

                        listProduct[i]["cantidad"] = $(fila.children[0]).html();
                        listProduct[i]["descripcion"] = $(fila.children[1]).html();

                        var chk = '<label class="custom-control custom-checkbox">\n' +
                            '                                    <input type="checkbox" class="custom-control-input order-select" id="chk' + i + '" />\n' +
                            '                                    <span class="custom-control-indicator"></span>\n' +
                            '                                    <span class="custom-control-description"></span>\n' +
                            '                                </label>';

                        var tr = '<tr><td>' + chk + '</td><td>' +
                            '<input class="form-control" style="width: 85px" type="number" min="1" max="' + listProduct[i]["cantidad"] + '" value="1">' + '</td><td>' +
                            listProduct[i]["cantidad"] + '</td><td>' +
                            listProduct[i]["descripcion"] +
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
        val: null
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
                if (data.getCredito && data.getCredito != 0) {
                    $("#div_credito").removeAttr('hidden');
                    $("#credito").html(data.getCredito);
                }

                $('#remitente').html(data.name + " " + data.lastName);
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
        } else if ($("#inputContactPhoneMovil").val() == "" && $("#inputContactPhoneHome").val() == "") {
            showWarningMessage("Atención", "Debe introducir al menos un teléfono de contacto.");
            return false;
        }
        else if ($("#inputContactPhoneMovil").val().length != 0 && $("#inputContactPhoneMovil").val().length != 10) {
            showWarningMessage("Atención", "E teléfono primario debe ser de 10 dígitos.");
            return false;
        }
        else if ($("#inputContactPhoneHome").val().length != 0 && $("#inputContactPhoneHome").val().length != 10) {
            showWarningMessage("Atención", "E teléfono secundario debe ser de 10 dígitos.");
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

    $("#provincia").on("change", selectMunicipios);

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

    var tipo_orden_sufij = "CQ";

    /***********DATOS DE LA ORDEN *************/
    var setNoOrden = function () {
        var time = $("#time").html();
        $('#no_orden').html(tipo_orden_sufij + time);
    };

    var calcMoney = function () {
        var precio = $("#Price").val();
        var oCargos = $("#OtrosCostos").val();
        var valAduanalValue = $("#valAduanalValue").html();
        var descuento = $("#Descuento").val();
        var precioTotalValue = parseFloat(precio) + parseFloat(valAduanalValue) + parseFloat(oCargos) - parseFloat(descuento);

        if ($("#checkpago").is(":checked")) {
            var pagoCash = parseFloat($("#pagoCash").val());
            var pagoZelle = parseFloat($("#pagoZelle").val());
            var pagoCheque = parseFloat($("#pagoCheque").val());
            var pagoTransf = parseFloat($("#pagoTransf").val());
            var pagoWeb = parseFloat($("#pagoWeb").val());

            var pagoCrDeb = parseFloat($("#pagoCredito").val());
            var pagoCrDebReal = parseFloat($("#pagoCredito").val()) / (1 + parseFloat($('#fee').html()) / 100);
            var feeCrDeb = pagoCrDeb - pagoCrDebReal;
            if (pagoCrDeb > 0) {
                $('#contfee').show();
            }
            else {
                $('#contfee').hide();
            }
            precioTotalValue += feeCrDeb;

            var pagoCredito = 0;
            if ($("#check_credito").is(":checked")) {
                pagoCredito = parseFloat($("#credito").html()).toFixed(2);
                pagoCredito = pagoCredito > precioTotalValue ? precioTotalValue : pagoCredito;
            }

            var balanceValue = precioTotalValue - pagoCash - pagoZelle - pagoCheque - pagoCrDeb - pagoTransf - pagoWeb - pagoCredito;

            $("#pagoCash").attr('max', (balanceValue + pagoCash).toFixed(2));
            $("#pagoZelle").attr('max', (balanceValue + pagoZelle).toFixed(2));
            $("#pagoCheque").attr('max', (balanceValue + pagoCheque).toFixed(2));
            $("#pagoTransf").attr('max', (balanceValue + pagoTransf).toFixed(2));
            $("#pagoWeb").attr('max', (balanceValue + pagoWeb).toFixed(2));
            $("#pagoCredito").attr('max', ((balanceValue + pagoCrDebReal) + (balanceValue + pagoCrDebReal) * parseFloat($('#fee').html()) / 100).toFixed(2));
            $("#pagar_credit").html("$" + ((balanceValue + pagoCrDebReal) + (balanceValue + pagoCrDebReal) * parseFloat($('#fee').html()) / 100).toFixed(2) + " (" + ((balanceValue + pagoCrDebReal) * parseFloat($('#fee').html()) / 100).toFixed(2) + " fee)");

            //Valor Sale Amount en authorization card
            $('[name = "AuthSaleAmount"]').val(pagoCrDebReal.toFixed(2));
            var aconvcharge = parseFloat($('[name = "AuthConvCharge"]').val());
            var total = pagoCrDebReal + (pagoCrDebReal * aconvcharge / 100);
            $('[name = "TotalCharge"]').val(total.toFixed(2));
        }
        else {
            tipoPagoId = $('[name = "TipoPago"]').val();
            tipoPago = $('option[value = "' + tipoPagoId + '"]').html();

            if (tipoPago == "Crédito o Débito") {
                var fee = parseFloat($('#fee').html());
                //Valor Sale Amount en authorization card
                $('[name = "AuthSaleAmount"]').val(precioTotalValue.toFixed(2));
                precioTotalValue = precioTotalValue + (precioTotalValue * (fee / 100));
                $('[name = "TotalCharge"]').val(precioTotalValue.toFixed(2));
            }
            var balanceValue = 0;
            if ($("#check_credito").is(":checked")) {
                if (precioTotalValue.toFixed(2) - parseFloat($("#credito").html()) > 0) {
                    $("#ValorPagado").attr('max', precioTotalValue.toFixed(2) - parseFloat($("#credito").html()));
                    $("#ValorPagado").val((precioTotalValue.toFixed(2) - parseFloat($("#credito").html())).toFixed(2));
                }
                else {
                    $("#ValorPagado").attr('max', 0);
                    $("#ValorPagado").val(0);
                }
            }
            else {
                $("#ValorPagado").val(precioTotalValue.toFixed(2));
                $("#ValorPagado").attr('max', precioTotalValue.toFixed(2));

            }
        }

        $("#precioTotalValue").html(precioTotalValue.toFixed(2));
        $("#balanceValue").html(balanceValue.toFixed(2));
        if ($("#balanceValue").html() == '-0.00')
            $("#balanceValue").html('0.00');
    };

    var calcMoneyPayment = function () {
        var precio = $("#Price").val();
        var oCargos = $("#OtrosCostos").val();
        var pagado = $("#ValorPagado").val();
        var valAduanalValue = $("#valAduanalValue").html();
        var descuento = $("#Descuento").val();

        var precioTotalValue = parseFloat(precio) + parseFloat(valAduanalValue) + parseFloat(oCargos) - parseFloat(descuento);
        var max = precioTotalValue;

        if (tipoPago == "Crédito o Débito") {
            var fee = parseFloat($('#fee').html());
            var pagdoReal = pagado / (1 + fee / 100)
            var feeCrDeb = pagado - pagdoReal;
            precioTotalValue = precioTotalValue + feeCrDeb;
            max = max + max * fee / 100;

            //Valor Sale Amount en authorization card
            $('[name = "AuthSaleAmount"]').val(pagdoReal.toFixed(2));
            $('[name = "TotalCharge"]').val(pagado);
        }

        precioTotalValue = precioTotalValue.toFixed(2);
        var balanceValue = 0;
        if ($("#check_credito").is(":checked")) {
            var pagoCredito = parseFloat($("#credito").html()).toFixed(2);
            pagoCredito = pagoCredito > precioTotalValue ? precioTotalValue : pagoCredito;
            balanceValue = precioTotalValue - pagado - pagoCredito;
        }
        else {
            balanceValue = precioTotalValue - pagado;
        }
        $("#precioTotalValue").html(precioTotalValue);
        $("#balanceValue").html(balanceValue.toFixed(2));
        $("#ValorPagado").attr("max", max.toFixed(2));
    };

    var calc = function () {
        var peso = $("#newPesoLb").val();
        var pesoVolum = $("#newAlto").val() * $("#newAncho").val() * $("#newLargo").val() / 360
        peso = peso > pesoVolum ? peso : pesoVolum;
        var precioVenta = 0;
        if (peso <= 3.3)
            precioVenta = precioCB + precio1;
        else
            precioVenta = precioCB - 1.88 + precio2 * peso;
        $("#newPrecio").val(precioVenta.toFixed(2));
    };

    var validateAddProduct = function () {
        if ($("#tblProductos > TBODY")[0].rows.length == 11) {
            showWarningMessage("Atención", "Ya alcanzó el número máximo de paquetes");
            return false;
        }
        if ($("#newPesoLb").val() == 0) {
            showWarningMessage("Atención", "El campo Peso no puede estar vacío.");
            return false;
        }
        if ($("#newDescrip").val() == "") {
            showWarningMessage("Atención", "El campo Descripción no puede estar vacío.");
            return false;
        }
        return true;
    };

    var validateEditProduct = function () {
        if ($("[name='cell1']").val() == "" || $("[name='cell1']").val() == 0) {
            showWarningMessage("Atención", "El campo Peso no puede estar vacío.");
            return false;
        }
        if ($("[name='cell0']").val() == "") {
            showWarningMessage("Atención", "El campo Descripción no puede estar vacío.");
            return false;
        }
        return true;
    };

    var isSend = false;

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

            var tBody = $("#tblProductos > TBODY")[0];
            var index = 0;
            for (var i = 1; i < tBody.rows.length; i++) {//filas de cada producto (excepto la 1era)
                listPaquetes[index] = [];
                var fila = tBody.rows[i];
                for (var j = 0; j < 6; j++) { // columnas de un paqute
                    listPaquetes[index][j] = (fila.children[j]) ? $(fila.children[j]).html() : '';
                }
                index += 1;
            }

            var valorespagado = [];
            var tipopagos = [];
            var notas = [];
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

            var datosOrden = [
                "", // 0
                $("#no_orden").html(), //                                                   1
                $(".select2-placeholder-selectClient").val(), //                            2
                $(".select2-placeholder-selectContact").val(), //                           3
                tipopagos, //                                                               4
                listPaquetes, //                                                            5
                $('#txtNameVA').val(), //                                                   6
                $("#Price").val().replace(",", "."), //                                     7
                $("#OtrosCostos").val().replace(",", "."), //                               8
                valorespagado, //                                                           9
                $("#valAduanalValue").html().replace(",", "."), //                          10
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
                $('#check_credito').is(':checked') ? $("#credito").html().replace(",", ".") : 0,// 28
                $('#Descuento').val(), //                                                    29     
                orderid   //30
            ];
            guardarEnvio(datosOrden);
        }
        else
            console.log("is send");
        return true;
    };

    function guardarEnvio(datosOrden) {
        //Guardo el envio
        $.ajax({
            type: "POST",
            url: "/OrderCubiq/CreateOrder",
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
                else if (data.orderId != "") {
                    toastr.error(data.msg, "ERROR");
                }
                else {
                    toastr.error("No se ha podido crear el trámite en estos momentos. Intente orta vez", "Alerta");
                }
                isSend = false;
                $("a[href=#finish]").unblock();
            },
            failure: function (response) {
                showErrorMessage("Alerta", "No se ha podido crear el trámite en estos momentos. Intente orta vez");
                $.unblockUI();
                isSend = false;
                $("a[href=#finish]").unblock();
            },
            error: function (response) {
                showErrorMessage("Alerta", "No se ha podido crear el trámite en estos momentos. Intente orta vez");
                $.unblockUI();
                isSend = false;
                $("a[href=#finish]").unblock();
            }
        });
    }

    var validateCreateOrder = function () {
        var tBody = $("#tblProductos > TBODY")[0];
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
        return true;
    };

    $("#checkpago").on('click', function () {
        if ($("#checkpago").is(" :checked")) {

            $("#untipopago").attr("hidden", 'hidden');
            $(".multipopago").removeAttr("hidden");
        }
        else {
            $(".multipopago").attr("hidden", 'hidden');
            $("#untipopago").removeAttr("hidden");
        }
        calcMoney();
    });
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
        tipoPago = $('option[value = "' + Id + '"]').html();
        if (tipoPago == "Crédito o Débito") {
            $('#contfee').show();
            calcMoney();
        }
        else {
            $('#contfee').hide();
            calcMoney();
        }
    });

    $("#OtrosCostos,#check_credito,#pagoCash,#pagoZelle,#pagoCheque,#pagoCredito,#pagoTransf,#pagoWeb,#Descuento").on("keyup", calcMoney).on("change", calcMoney);

    $("#ValorPagado").on("keyup", calcMoneyPayment).on("change", calcMoneyPayment);
    $("#newPesoLb").on("change", calc);

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

    var addProductToTable = function (pesoLb, largo, alto, ancho, precio, descripcion) {
        //Get the reference of the Table's TBODY element.
        var tBody = $("#tblProductos > TBODY")[0];

        var row = tBody.insertRow(-1);

        var cell0 = $(row.insertCell(-1));
        cell0.append(descripcion);
        var cell1 = $(row.insertCell(-1));
        cell1.append(pesoLb);
        var cell2 = $(row.insertCell(-1));
        cell2.append(largo);
        var cell3 = $(row.insertCell(-1));
        cell3.append(alto);
        var cell4 = $(row.insertCell(-1));
        cell4.append(ancho);
        var cell5 = $(row.insertCell(-1));
        cell5.append(precio);
        var cell6 = $(row.insertCell(-1));
        var btnEdit = $("<button type='button' class='btn btn-warning' title='Editar' style='font-size: 10px'><i class='fa fa-pencil'></i></button>");
        var btnRemove = $("<button type='button' class='btn btn-danger pull-right' title='Eliminar' style='font-size: 10px'><i class=' fa fa-close'></button>");
        var btnConfirm = $("<button type='button' class='btn btn-success hidden' title='Confirmar' style='font-size: 10px'><i class='fa fa-check'></button>");
        btnEdit.on("click", function () {
            $("#Price").val(parseFloat(($("#Price").val()) - parseFloat(cell5.html())).toFixed(2));
            cell0.html("<input name='cell0' class='form-control' value='" + cell0.html() + "'/>");
            cell1.html("<input type='number' name='cell1' step='0.01' class='form-control' value='" + cell1.html() + "'/>");
            cell2.html("<input type='number' name='cell2' step='0.01' class='form-control' value='" + cell2.html() + "'/>");
            cell3.html("<input type='number' name='cell3' step='0.01' class='form-control' value='" + cell3.html() + "'/>");
            cell4.html("<input type='number' name='cell4' step='0.01' class='form-control' value='" + cell4.html() + "'/>");
            cell5.html("<input type='number' readonly name='cell5' class='form-control' value='" + cell5.html() + "'/>");
            $("[name='cell1']").on('change', function () {
                var peso = $("[name='cell1']").val();
                var pesoVolum = $("[name='cell2']").val() * $("[name='cell3']").val() * $("[name='cell4']").val() / 360;
                peso = peso > pesoVolum ? peso : pesoVolum;
                var precioVenta = 0;
                if (peso <= 3.3)
                    precioVenta = precioCB + precio1;
                else
                    precioVenta = precioCB - 1.88 + precio2 * peso;
                $("[name='cell5']").val(precioVenta.toFixed(2));
            });
            btnConfirm.removeClass("hidden");
            btnEdit.addClass("hidden");
            btnRemove.addClass("hidden");
            calcMoney();
        });
        btnRemove.on("click", function () {
            $("#Price").val(parseFloat(($("#Price").val()) - parseFloat(cell5.html())).toFixed(2));
            row.remove();
            calcMoney();
        });
        btnConfirm.on("click", function () {
            if (validateEditProduct()) {
                cell0.html($("[name='cell0']").val());
                cell1.html($("[name='cell1']").val());
                cell2.html($("[name='cell2']").val());
                cell3.html($("[name='cell3']").val());
                cell4.html($("[name='cell4']").val());
                cell5.html($("[name='cell5']").val());
                $("#Price").val((parseFloat($("#Price").val()) + parseFloat(cell5.html())).toFixed(2));
                btnConfirm.addClass("hidden");
                btnEdit.removeClass("hidden");
                btnRemove.removeClass("hidden");
                calcMoney();
            }
        });
        cell6.append(btnEdit.add(btnRemove).add(btnConfirm));

        var currentPrice = parseFloat($("#Price").val());
        currentPrice += parseFloat(precio);
        $("#Price").val(currentPrice.toFixed(2));
    };

    $('#btnAdd').on('click', function () {
        if (validateAddProduct()) {
            calc();
            addProductToTable(
                $("#newPesoLb").val(),
                $("#newLargo").val(),
                $("#newAlto").val(),
                $("#newAncho").val(),
                $("#newPrecio").val(),
                $("#newDescrip").val()
            );
            $("#newPesoLb").val("0");
            $("#newPesoKg").val("0");
            $("#newLargo").val("0");
            $("#newAlto").val("0");
            $("#newAncho").val("0");
            $("#newPrecio").val("0");
            $("#newDescrip").val("");
            calcMoney();
            $("#newDescrip").focus();
        }
    });

    $("#newPesoLb").on('keypress', function (a) {
        if (a.key == 'Enter' && validateAddProduct()) {
            calc();
            addProductToTable(
                $("#newPesoLb").val(),
                $("#newLargo").val(),
                $("#newAlto").val(),
                $("#newAncho").val(),
                $("#newPrecio").val(),
                $("#newDescrip").val()
            );
            $("#newPesoLb").val("0");
            $("#newPesoKg").val("0");
            $("#newLargo").val("0");
            $("#newAlto").val("0");
            $("#newAncho").val("0");
            $("#newPrecio").val("0");
            $("#newDescrip").val("");
            calcMoney();
            $("#newDescrip").focus();
        }
    });

    $('#newDescrip').on('keypress', function (e) {
        if (e.key == 'Enter') {
            $('#newPesoLb').focus();
        }
    });
    $('#newPesoLb').focus(function () { this.select(); });

    setNoOrden();
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
            url: "/OrderNew/GetClient",
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
                if (data.getCredito && data.getCredito != 0)
                    $("#div_credito").removeAttr('hidden');

                $('#remitente').html(data.name + " " + data.lastName);
                //showContactsOfAClient();
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            }
        });
    }

    // Para cargar el valor del precio por libra

    function getprecioxlibra() {
        var idcontacto = $('[name = "selectContact"]').val();
        var idmayorista = $('#selectMayorista').val();
        $.ajax({
            type: "POST",
            url: "/OrderCubiq/getPrecioxLibra",
            async: false,
            data: {
                idContacto: idcontacto
            },
            beforeSend: function () {
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
            success: function (data) {
                precioCB = data.cb;
                precio1 = data.precio1;
                precio2 = data.precio2;
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
        $.unblockUI();
    }
});