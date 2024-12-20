

var showClient = function () {
    selectedClient = $("#ClientId").val();
    $.ajax({
        type: "POST",
        url: "/Clients/GetClient",
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(selectedClient),
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
            $('#inputClientName2').val(data.name2);
            $('#inputClientLastName2').val(data.lastName2);
            $('#inputClientMovil').val(data.movil);
            $('#inputClientEmail').val(data.email);
            $('#inputClientAddress').val(data.calle);
            $('#inputClientCity').data("value", data.city);
            $('#inputClientZip').val(data.zip);
            $('#inputClientState').val(data.state).trigger("change");
            $('#inputClientFNaci').val(parseDate(data.fechaNac.slice(0, 10)));
            $('#inputClientID').val(data.id);

            if (data.getCredito && data.getCredito != 0) {
                $("#div_credito").removeAttr('hidden');
                $("#credito").html(data.getCredito);
            }
            else {
                $("#div_credito").attr('hidden', "hidden");
            }


            $("#PaisResidencia").val(data.country);
            $("#PaisActual").val(data.country);
            $("#Estado").val(data.state).trigger("change");
            $("#PrimerNombre").val(data.name);
            $("#PrimerApellidos").val(data.lastName);
            $("#SegundoNombre").val(data.name2);
            $("#SegundoApellidos").val(data.lastName2);
            $("#DireccionActual").val(data.calle);
            $("#ProvinciaActual").data("value", data.city);;
            $("#EstadoActual").val(data.state).trigger("change");
            $("#CodigoPostalActual").val(data.zip);
            $("#Telefono").val(data.movil);
            $("#EmailActual").val(data.email);
            if (data.fechaNac.slice(0, 10) != "0001-01-01") {
                $("#DateBirth").val(data.fechaNac.slice(0, 10));
                $("div[data-input='DateBirth']").find("input:first").val(parseDate(data.fechaNac.slice(0, 10)))
            }
            if (data.conflictivo) {
                $("#conflictivo").removeAttr("hidden");
            }
            else {
                $("#conflictivo").attr("hidden", "hidden");
            }

            //Datos pasaporte
            //$("#PassaportNumber").val(data.passportNumber);
            //$("#ExpirePassport1").val(data.passportExpireDate)
        },
        failure: function (response) {
            showErrorMessage("ERROR", response.statusText);
        },
        error: function (response) {
            showErrorMessage("ERROR", response.statusText);
        }
    });
}

var desactClientForm = function () {
    $('#nuevoCliente').removeAttr("disabled");
    $(".select2-placeholder-selectClient").removeAttr("disabled");

    $('#inputClientName').attr("disabled", "disabled");
    $('#inputClientLastName').attr("disabled", "disabled");
    $('#inputClientName2').attr("disabled", "disabled");
    $('#inputClientLastName2').attr("disabled", "disabled");
    $('#inputClientMovil').attr("disabled", "disabled");
    $('#inputClientEmail').attr("disabled", "disabled");
    $('#inputClientAddress').attr("disabled", "disabled");
    $('#inputClientCity').attr("disabled", "disabled");
    $('#inputClientState').attr("disabled", "disabled");
    $('#inputClientZip').attr("disabled", "disabled");
    $('#inputClientFNaci').attr("disabled", "disabled");
    $('#inputClientID').attr("disabled", "disabled");

    $('#editarCliente').removeClass("hidden");
    $("#cancelarCliente").addClass("hidden");
    $("#guardarCliente").addClass("hidden");

    $("a[href='#next']").removeClass("hidden");
}

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

    if ($("#inputClientEmail").val() != "") {
        var regexEmail = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
        if (!regexEmail.test($("#inputClientEmail").val())) {
            showWarningMessage("Atención", "El campo Email no tiene el formato correcto.");
            return false;
        }
    }

    return true;
}

var parseDate = function (value) {
    if (value == "0001-01-01") {
        return "";
    }
    var m = value.match(/^(\d{4})(\/|-)?(\d{1,2})(\/|-)?(\d{1,2})$/);
    if (m)
        value = ("00" + m[3]).slice(-2) + '/' + ("00" + m[5]).slice(-2) + '/' + m[1];

    return value;
}

var showClient = function () {
    selectedClient = $("#ClientId").val();
    $.ajax({
        type: "POST",
        url: "/Clients/GetClient",
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(selectedClient),
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
            $('#inputClientName2').val(data.name2);
            $('#inputClientLastName2').val(data.lastName2);
            $('#inputClientMovil').val(data.movil);
            $('#inputClientEmail').val(data.email);
            $('#inputClientAddress').val(data.calle);
            $('#inputClientCity').data("value", data.city);
            $('#inputClientZip').val(data.zip);
            $('#inputClientState').val(data.state).trigger("change");
            $('#inputClientFNaci').val(parseDate(data.fechaNac.slice(0, 10)));
            $('#inputClientID').val(data.id);

            if (data.getCredito && data.getCredito != 0) {
                $("#div_credito").removeAttr('hidden');
                $("#credito").html(data.getCredito);
            }
            else {
                $("#div_credito").attr('hidden', "hidden");
            }


            $("#PaisResidencia").val(data.country);
            $("#PaisActual").val(data.country);
            $("#Estado").val(data.state).trigger("change");
            $("#PrimerNombre").val(data.name);
            $("#PrimerApellidos").val(data.lastName);
            $("#SegundoNombre").val(data.name2);
            $("#SegundoApellidos").val(data.lastName2);
            $("#DireccionActual").val(data.calle);
            $("#ProvinciaActual").data("value", data.city);;
            $("#EstadoActual").val(data.state).trigger("change");
            $("#CodigoPostalActual").val(data.zip);
            $("#Telefono").val(data.movil);
            $("#EmailActual").val(data.email);
            if (data.fechaNac.slice(0, 10) != "0001-01-01") {
                $("#DateBirth").val(data.fechaNac.slice(0, 10));
                $("div[data-input='DateBirth']").find("input:first").val(parseDate(data.fechaNac.slice(0, 10)))
            }
            if (data.conflictivo) {
                $("#conflictivo").removeAttr("hidden");
            }
            else {
                $("#conflictivo").attr("hidden", "hidden");
            }

            //Datos pasaporte
            //$("#PassaportNumber").val(data.passportNumber);
            //$("#ExpirePassport1").val(data.passportExpireDate)
        },
        failure: function (response) {
            showErrorMessage("ERROR", response.statusText);
        },
        error: function (response) {
            showErrorMessage("ERROR", response.statusText);
        }
    });
}

var loadMunicipios = function (provincia, municipios) {
    if (!provincia)
        return;

    $.ajax({
        url: "/Provincias/Municipios?nombre=" + provincia.val(),
        type: "POST",
        dataType: "json",
        success: function (response) {
            municipios.empty();
            municipios.append(new Option())
            for (var i in response) {
                var m = response[i];
                municipios.append(new Option(m, m))
            }
        }
    })
}

var loadCiudad = function (estado, ciudad) {
    if (!estado)
        return;

    $.ajax({
        url: "/Provincias/Ciudades?nombre=" + estado.val(),
        type: "POST",
        dataType: "json",
        success: function (response) {
            ciudad.empty();
            ciudad.append(new Option())
            for (var i in response) {
                var m = response[i];
                ciudad.append(new Option(m, m))
                if ($(ciudad).data("value") != "" && $(ciudad).data("value") != null) {
                    $(ciudad).val($(ciudad).data("value")).trigger("change")
                }
            }
        }
    })
}

var validateForm = function () {
    let validate = true;
    if (!$('#MaritalStatus').val()) {
        validate = false;
        toastr.warning("Estado conyugal es requerido")
    }
    if (!$('#MigratoryCategory').val()) {
        validate = false;
        toastr.warning("Categoria migratoria es requerido")
    }
    return validate;
}

var save = function () {
    const dataForm = $('form').serializeArray();
    var data = {};
    dataForm.map(item => {
        data[item.name] = item.value
    });

    $.ajax({
        async: true,
        type: "POST",
        url: "/MinorAuthorization/Create",
        data: data,
        contentType: "application/x-www-form-urlencoded",
        beforeSend: function () {
            $.blockUI();
        },
        success: function (data) {
            if (data.success) {
                window.location = "/MinorAuthorization/Index/?msg=" + data.msg;
            } else {
                console.log(data);
                toastr.error("Ha ocurrido un error", "ERROR");
                $.unblockUI();
            }
        },
        failure: function (response) {
            showErrorMessage("FAILURE", "No se han podido guardar los datos");
            $.unblockUI();
        },
        error: function (response) {
            showErrorMessage("ERROR", "No se han podido guardar los datos");
            $.unblockUI();
        },
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

var send = false;

$("#zc").steps({
    headerTag: "h6",
    bodyTag: "fieldset",
    transitionEffect: "fade",
    titleTemplate: '<span class="step">#index#</span> #title#',
    labels: {
        previous: "Anterior",
        next: "Siguiente",
        finish: 'Crear'
    },
    onStepChanging: function (event, currentIndex, newIndex) {
        // Allways allow previous action even if the current form is not valid!
        if (currentIndex > newIndex) {
            if (newIndex == 1)
                $("a[href=#previous]").hide();
            return true;
        }

        if (newIndex == 1) {
            if ($("#ClientId").val() == "") {
                showWarningMessage("Atención", "El campo cliente es obligatorio.");
                return false;
            }
            if (new Date($("#DateBirth").val()) > new Date()) {
                showWarningMessage("Atención", "La fecha de nacimiento no es válida.");
                return false;
            }
        }

        return true;
    },
    onFinishing: function (event, currentIndex) {
        if (!validateForm()) {
            return false;
        }
        swal({
            title: "¿Está seguro?",
            text: "Verifique que todos los datos ingresados se encuentren correctamente y presione 'ok'.",
            type: "info",
            showCancelButton: true,
            closeOnConfirm: false,
            showLoaderOnConfirm: true,
        }, function (isConfirm) {
            if (isConfirm) {
                save();
            } else {
                return false;
            }

        });
    },
    onFinished: function (event, currentIndex) {
    }
});


$("#ClientId").select2({
    placeholder: "Buscar cliente por teléfono, nombre o apellido",
    width: "100%",
    val: null,
    ajax: {
        type: 'POST',
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

                    return { id: obj.clientId, text: obj.fullData, conflictivo: obj.conflictivo };
                })
            };
        }
    }
});

$("#ClientId").on("select2:select", function (a, b) {
    $('#editarCliente').removeClass("hidden");
    showClient();
});

$("#inputClientState").select2({
    placeholder: "Estado",
    width: "100%"
});

$("#inputClientCity").select2({
    placeholder: "Ciudad",
    width: "100%"
});

$("#inputClientState").on("change", function () {
    loadCiudad($("#inputClientState"), $("#inputClientCity"));
})

$('#editarCliente').on('click', function () {
    // para que no pueda crear nuevo cliente mientras edita cliente
    $('#nuevoCliente').attr("disabled", "disabled");

    // para que no pueda cambiar de cliente mientras edita cliente
    $('.select2-placeholder-selectClient').attr("disabled", "disabled");

    $("a[href='#next']").addClass("hidden");

    $('#inputClientName').removeAttr("disabled").data("prevVal", $('#inputClientName').val());
    $('#inputClientLastName').removeAttr("disabled").data("prevVal", $('#inputClientLastName').val());
    $('#inputClientName2').removeAttr("disabled").data("prevVal", $('#inputClientName2').val());
    $('#inputClientLastName2').removeAttr("disabled").data("prevVal", $('#inputClientLastName2').val());
    $('#inputClientMovil').removeAttr("disabled").data("prevVal", $('#inputClientMovil').val());
    $('#inputClientEmail').removeAttr("disabled").data("prevVal", $('#inputClientEmail').val());
    $('#inputClientAddress').removeAttr("disabled").data("prevVal", $('#inputClientAddress').val());
    $('#inputClientCity').removeAttr("disabled").data("prevVal", $('inputClientCity').val());
    $('#inputClientState').removeAttr("disabled").data("prevVal", $('#inputClientState').val());
    $('#inputClientZip').removeAttr("disabled").data("prevVal", $('#inputClientZip').val());
    $('#inputClientFNaci').removeAttr("disabled").data("prevVal", $('#inputClientFNaci').val());
    $('#inputClientID').removeAttr("disabled").data("prevVal", $('#inputClientID').val());

    $('#editarCliente').addClass("hidden");
    $("#cancelarCliente").removeClass("hidden");
    $("#guardarCliente").removeClass("hidden");
});

$('#guardarCliente').on('click', function () {
    if (validateEditarCliente()) {
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            data: {
                clientId: $("#ClientId").val(),
                name: $('#inputClientName').val(),
                name2: $('#inputClientName2').val(),
                lastname: $('#inputClientLastName').val(),
                lastname2: $('#inputClientLastName2').val(),
                email: $('#inputClientEmail').val(),
                phone: $('#inputClientMovil').val(),
                address: $('#inputClientAddress').val(),
                city: $('#inputClientCity').val(),
                state: $('#inputClientState').val(),
                zip: $('#inputClientZip').val(),
                fechaNac: $("#inputClientFNaci").val(),
                id: $("#inputClientID").val()
            },
            url: "/Clients/EditClientNew",
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
            $("#ClientId").append(newOption);
            $("#ClientId").val(selectedClient).trigger("change").trigger("select2:select");

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
            $('#inputClientName2').val(data.name2);
            $('#inputClientLastName2').val(data.lastName2);
            $('#inputClientMovil').val(data.movil);
            $('#inputClientEmail').val(data.email);
            $('#inputClientAddress').val(data.calle);
            $('#inputClientCity').data("value", data.city);
            $('#inputClientZip').val(data.zip);
            $('#inputClientState').val(data.state).trigger("change");
            $('#inputClientFNaci').val(parseDate(data.fechaNac.slice(0, 10)));
            $('#inputClientID').val(data.id);

            if (data.getCredito && data.getCredito != 0) {
                $("#div_credito").removeAttr('hidden');
                $("#credito").html(data.getCredito);
            }
            else {
                $("#div_credito").attr('hidden', "hidden");
            }


            $("#PaisResidencia").val(data.country);
            $("#PaisActual").val(data.country);
            $("#Estado").val(data.state).trigger("change");
            $("#PrimerNombre").val(data.name);
            $("#PrimerApellidos").val(data.lastName);
            $("#SegundoNombre").val(data.name2);
            $("#SegundoApellidos").val(data.lastName2);
            $("#DireccionActual").val(data.calle);
            $("#ProvinciaActual").data("value", data.city);;
            $("#EstadoActual").val(data.state).trigger("change");
            $("#CodigoPostalActual").val(data.zip);
            $("#Telefono").val(data.movil);
            $("#EmailActual").val(data.email);
            if (data.fechaNac.slice(0, 10) != "0001-01-01") {
                $("#DateBirth").val(data.fechaNac.slice(0, 10));
                $("div[data-input='DateBirth']").find("input:first").val(parseDate(data.fechaNac.slice(0, 10)))
            }
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


