$(document).on("ready", function () {
    $("#wz").steps({
        headerTag: "h6",
        bodyTag: "fieldset",
        transitionEffect: "fade",
        enableCancelButton: true,
        titleTemplate: '<span class="step">#index#</span> #title#',
        labels: {
            previous: "Anterior",
            next: "Siguiente",
            finish: 'Crear',
            cancel: "Cancelar"
        },
        onStepChanging: function (event, currentIndex, newIndex) {
            // Allways allow previous action even if the current form is not valid!
            if (currentIndex > newIndex) {
                return true;
            }
            //----------------Step1
            var error = false
            if (newIndex == 1) {
                if ($("#ClientId").val() == "") {
                    showWarningMessage("Atención", "El campo de cliente es obligatorio.");
                    error = true
                }
            }
            return !error;
        },
        onFinishing: function (event, currentIndex) {
            var error = false
            if ($("#OrderNumber").val() == "") {
                error = true;
                showWarningMessage("Atención", "El campo número de orden es obligatorio.");
            }
            if ($("#Type").val() == "None") {
                error = true;
                showWarningMessage("Atención", "El campo tipo es obligatorio.");
            }
            if ($("#ReclamoTickets_0__Nota").val() == "") {
                error = true;
                showWarningMessage("Atención", "El campo Nota es obligatorio.");
            }
            if (!error) {
                var form = $('#wz');
                form.submit()
            }
            return !error;
        },
        onFinished: function (event, currentIndex) {

        },
        onCanceled: function () {
            window.location = "/Reclamo"
        }
    });
    $("[href='#cancel']").addClass('btn-danger');


    // cargar estado
    $("select[id=state]").empty()
    $("select[id=state]").append(new Option("Alabama", "Alabama"))
    $("select[id=state]").append(new Option("Alaska", "Alaska"))
    $("select[id=state]").append(new Option("American Samoa", "American Samoa"))
    $("select[id=state]").append(new Option("Arizona", "Arizona"))
    $("select[id=state]").append(new Option("Arkansas", "Arkansas"))
    $("select[id=state]").append(new Option("Armed Forces Americas", "Armed Forces Americas"))
    $("select[id=state]").append(new Option("Armed Forces Europe, Canada, Africa and Middle East", "Armed Forces Europe, Canada, Africa and Middle East"))
    $("select[id=state]").append(new Option("Armed Forces Pacific", "Armed Forces Pacific"))
    $("select[id=state]").append(new Option("California", "California"))
    $("select[id=state]").append(new Option("Colorado", "Colorado"))
    $("select[id=state]").append(new Option("Connecticut", "Connecticut"))
    $("select[id=state]").append(new Option("Delaware", "Delaware"))
    $("select[id=state]").append(new Option("District of Columbia", "District of Columbia"))
    $("select[id=state]").append(new Option("Florida", "Florida"))
    $("select[id=state]").append(new Option("Georgia", "Georgia"))
    $("select[id=state]").append(new Option("Guam", "Guam"))
    $("select[id=state]").append(new Option("Hawaii", "Hawaii"))
    $("select[id=state]").append(new Option("Idaho", "Idaho"))
    $("select[id=state]").append(new Option("Illinois", "Illinois"))
    $("select[id=state]").append(new Option("Indiana", "Indiana"))
    $("select[id=state]").append(new Option("Iowa", "Iowa"))
    $("select[id=state]").append(new Option("Kansas", "Kansas"))
    $("select[id=state]").append(new Option("Kentucky", "Kentucky"))
    $("select[id=state]").append(new Option("Louisiana", "Louisiana"))
    $("select[id=state]").append(new Option("Maine", "Maine"))
    $("select[id=state]").append(new Option("Marshall Islands", "Marshall Islands"))
    $("select[id=state]").append(new Option("Maryland", "Maryland"))
    $("select[id=state]").append(new Option("Massachusetts", "Massachusetts"))
    $("select[id=state]").append(new Option("Michigan", "Michigan"))
    $("select[id=state]").append(new Option("Micronesia", "Micronesia"))
    $("select[id=state]").append(new Option("Minnesota", "Minnesota"))
    $("select[id=state]").append(new Option("Mississippi", "Mississippi"))
    $("select[id=state]").append(new Option("Missouri", "Missouri"))
    $("select[id=state]").append(new Option("Montana", "Montana"))
    $("select[id=state]").append(new Option("Nebraska", "Nebraska"))
    $("select[id=state]").append(new Option("Nevada", "Nevada"))
    $("select[id=state]").append(new Option("New Hampshire", "New Hampshire"))
    $("select[id=state]").append(new Option("New Jersey", "New Jersey"))
    $("select[id=state]").append(new Option("New Mexico", "New Mexico"))
    $("select[id=state]").append(new Option("New York", "New York"))
    $("select[id=state]").append(new Option("North Carolina", "North Carolina"))
    $("select[id=state]").append(new Option("North Dakota", "North Dakota"))
    $("select[id=state]").append(new Option("Northern Mariana Islands", "Northern Mariana Islands"))
    $("select[id=state]").append(new Option("Ohio", "Ohio"))
    $("select[id=state]").append(new Option("Oklahoma", "Oklahoma"))
    $("select[id=state]").append(new Option("Oregon", "Oregon"))
    $("select[id=state]").append(new Option("Palau", "Palau"))
    $("select[id=state]").append(new Option("Pennsylvania", "Pennsylvania"))
    $("select[id=state]").append(new Option("Puerto Rico", "Puerto Rico"))
    $("select[id=state]").append(new Option("Rhode Island", "Rhode Island"))
    $("select[id=state]").append(new Option("South Carolina", "South Carolina"))
    $("select[id=state]").append(new Option("South Dakota", "South Dakota"))
    $("select[id=state]").append(new Option("Tennessee", "Tennessee"))
    $("select[id=state]").append(new Option("Texas", "Texas"))
    $("select[id=state]").append(new Option("Utah", "Utah"))
    $("select[id=state]").append(new Option("Vermont", "Vermont"))
    $("select[id=state]").append(new Option("Virgin Islands", "Virgin Islands"))
    $("select[id=state]").append(new Option("Virginia", "Virginia"))
    $("select[id=state]").append(new Option("Washington", "Washington"))
    $("select[id=state]").append(new Option("West Virginia", "West Virginia"))
    $("select[id=state]").append(new Option("Wisconsin", "Wisconsin"))
    $("select[id=state]").append(new Option("Wyoming", "Wyoming"))

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

    if ($("#ClientId").val() != "") {        
        clientId = $("#ClientId").val();
        $.ajax({
            async: true,
            type: "POST",
            contentType: "application/x-www-form-urlencoded",
            url: "/ticket/loadclient",
            data: {
                clientId: clientId,
            },
            beforeSend: function () {
                block($("#ClientId"))
            },
            success: function (response) {
                $("#inputClientName").data("pasaporteid", response["passportid"]);
                $("#inputClientName").val(response["name"]);
                $("#inputClientLastName").val(response["lastname"]);
                $("#inputClientMovil").val(response["movile"]);
                $("#inputClientEmail").val(response["email"]);
                $("#inputClientAddress").val(response["address"]);
                $("#state").val(response["state"]);
                $("#inputClientCity").val(response["city"]);
                $("#inputClientZip").val(response["zip"]);
                $("#inputClientTelCuba").val(response["phoneCuba"]);
                $("#inputClientIdentidad").val(response["id"]);
                $("#inputClientFecha").pickadate('picker').set('select', new Date(response["fechaNac"]));

                $('[name = "clienteInfo"]').html(response["name"] + " " + response["lastname"]);
                $('[name = "telefonoInfo"]').html(response["movile"]);


                //Datos del Cliente en Authorization Card
                $('#nameClientCard').html('<strong>Nombre: </strong>' + response["name"] + ' ' + response["lastname"]);
                $('#phoneClientCard').html('<strong>Teléfono: </strong>' + response["movile"]);
                $('#emailClientCard').html('<strong>Email: </strong>' + response["email"]);
                $('#stateClientCard').html('<strong>Estado: </strong>' + response["state"]);
                $('#cityClientCard').html('<strong>Ciudad: </strong>' + response["city"]);
                $('#addressClientCard').html('<strong>Dirección: </strong>' + response["address"]);

                $('#AuthaddressOfSend').val(response["address"]);
                $('#Authemail').val(response["email"]);
                $('#Authphone').val(response["movile"]);

                //Valor del ID en la imagen adjunta
                $('#TextoIDImg').val(response["id"]);

                $("#Pasajeros_0__Name").val(response["name"]);
                $("#Pasajeros_0__LastName").val(response["lastname"]);
                $("#Pasajeros_0__Phone").val(response["movile"]);

                if (response.getCredito && response.getCredito != 0) {
                    $("#div_credito").removeAttr('hidden');
                    $("#credito").html(response.getCredito);
                }
                else {
                    $("#div_credito").attr('hidden', "hidden");
                }

                $('.actions').show();
                unblock($("#ClientId"))
            },
            error: function () {
                $('.actions').hide();
                alert("Error");
                unblock($("#ClientId"));
            },
            timeout: 4000,
        });
    }

    $("#inputClientFecha").pickadate({
        labelMonthNext: 'Next month',
        labelMonthPrev: 'Previous month',
        labelMonthSelect: 'Pick a Month',
        selectMonths: true,
        selectYears: 200,
        format: 'dd mmmm yyyy',
        formatSubmit: 'yyyy/dd/mm',
    });

    $(".selectdos").select2({ width: "100%" })

    // para autocomletar cliente
    $(".Sel").select2({
        placeholder: "Buscar cliente por teléfono, nombre o apellido",
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

                        return { id: obj.clientId, text: obj.fullData };
                    })
                };
            }
        }
    });

    // Seleccion de cliente
    $("#ClientId").change(function () {
        $("#editarCliente").show();
        var $this = $(this);
        var clientId = $this.val();
        if (clientId != "") {
            $.ajax({
                async: true,
                type: "POST",
                contentType: "application/x-www-form-urlencoded",
                url: "/ticket/loadclient",
                data: {
                    clientId: clientId,
                },
                beforeSend: function () {
                    block($this)
                },
                success: function (response) {
                    $("#inputClientName").data("pasaporteid", response["passportid"]);
                    $("#inputClientName").val(response["name"]);
                    $("#inputClientLastName").val(response["lastname"]);
                    $("#inputClientMovil").val(response["movile"]);
                    $("#inputClientEmail").val(response["email"]);
                    $("#inputClientAddress").val(response["address"]);
                    $("#state").val(response["state"]);
                    $("#inputClientCity").val(response["city"]);
                    $("#inputClientZip").val(response["zip"]);
                    $("#inputClientTelCuba").val(response["phoneCuba"]);
                    $("#inputClientIdentidad").val(response["id"]);
                    $("#inputClientFecha").pickadate('picker').set('select', new Date(response["fechaNac"]));

                    $('[name = "clienteInfo"]').html(response["name"] + " " + response["lastname"]);
                    $('[name = "telefonoInfo"]').html(response["movile"]);


                    //Datos del Cliente en Authorization Card
                    $('#nameClientCard').html('<strong>Nombre: </strong>' + response["name"] + ' ' + response["lastname"]);
                    $('#phoneClientCard').html('<strong>Teléfono: </strong>' + response["movile"]);
                    $('#emailClientCard').html('<strong>Email: </strong>' + response["email"]);
                    $('#stateClientCard').html('<strong>Estado: </strong>' + response["state"]);
                    $('#cityClientCard').html('<strong>Ciudad: </strong>' + response["city"]);
                    $('#addressClientCard').html('<strong>Dirección: </strong>' + response["address"]);

                    $('#AuthaddressOfSend').val(response["address"]);
                    $('#Authemail').val(response["email"]);
                    $('#Authphone').val(response["movile"]);

                    //Valor del ID en la imagen adjunta
                    $('#TextoIDImg').val(response["id"]);

                    $("#Pasajeros_0__Name").val(response["name"]);
                    $("#Pasajeros_0__LastName").val(response["lastname"]);
                    $("#Pasajeros_0__Phone").val(response["movile"]);

                    if (response.getCredito && response.getCredito != 0) {
                        $("#div_credito").removeAttr('hidden');
                        $("#credito").html(response.getCredito);
                    }
                    else {
                        $("#div_credito").attr('hidden', "hidden");
                    }

                    $('.actions').show();
                    unblock($this)
                },
                error: function () {
                    $('.actions').hide();
                    alert("Error");
                    unblock($this);
                },
                timeout: 4000,
            });
        }
        else {
        }
    });

    // editar cliente
    $("#editarCliente").click(function () {
        showEdit()
    })

    $("#cancelarCliente").click(function () {
        hideEdit()
    })

    function showEdit() {
        $("#editarCliente").hide()
        $("#guardarCliente").show()
        $("#cancelarCliente").show()
        $("#ClientId").attr("disabled", "disabled")

        $("#inputClientName").removeAttr("disabled")
        $("#inputClientLastName").removeAttr("disabled")
        $("#inputClientMovil").removeAttr("disabled")
        $("#inputClientEmail").removeAttr("disabled")
        $("#inputClientAddress").removeAttr("disabled")
        $("#state").removeAttr("disabled")
        $("#inputClientCity").removeAttr("disabled")
        $("#inputClientZip").removeAttr("disabled")
        $("#inputClientTelCuba").removeAttr("disabled")
        $("#inputClientIdentidad").removeAttr("disabled")
        $("#inputClientFecha").removeAttr("disabled")
    }

    function hideEdit() {
        $("#editarCliente").show()
        $("#guardarCliente").hide()
        $("#cancelarCliente").hide()
        $("#ClientId").removeAttr("disabled")

        $("#inputClientName").attr("disabled", "disabled")
        $("#inputClientLastName").attr("disabled", "disabled")
        $("#inputClientMovil").attr("disabled", "disabled")
        $("#inputClientEmail").attr("disabled", "disabled")
        $("#inputClientAddress").attr("disabled", "disabled")
        $("#state").attr("disabled", "disabled")
        $("#inputClientCity").attr("disabled", "disabled")
        $("#inputClientZip").attr("disabled", "disabled")
        $("#inputClientTelCuba").attr("disabled", "disabled")
        $("#inputClientIdentidad").attr("disabled", "disabled")
        $("#inputClientFecha").attr("disabled", "disabled")
    }

    $("#guardarCliente").click(function () {
        var source = [
            $("#ClientId").val(), // 0
            $('#inputClientName').val(), //1
            $('#inputClientLastName').val(), //2
            $('#inputClientEmail').val(),  //3
            $('#inputClientMovil').val(), //4
            $('#inputClientAddress').val(), //5
            $('#inputClientCity').val(), // 6
            $('#state').val(),  // 7
            $('#inputClientZip').val(), //8
            $('#inputClientTelCuba').val(), //9
            "", //10
            $('#inputClientIdentidad').val(), //11
            "", //12
            "", //13
            $("#inputClientFecha").val() // 14
        ];

        $.ajax({
            type: "POST",
            url: "/clients/EditClient",
            data: JSON.stringify(source),
            dataType: 'json',
            contentType: 'application/json',
            async: false,
            success: function (response) {
                hideEdit()
                if (response.success) {
                    toastr.success(response.msg);
                }
                else {
                    toastr.error(response.msg);
                }
                //Datos del Cliente en Authorization Card
                $('#nameClientCard').html('<strong>Nombre: </strong>' + $('#inputClientName').val() + ' ' + $('#inputClientLastName').val());
                $('#phoneClientCard').html('<strong>Teléfono: </strong>' + $('#inputClientMovil').val());
                $('#emailClientCard').html('<strong>Email: </strong>' + $('#inputClientEmail').val());
                $('#stateClientCard').html('<strong>Estado: </strong>' + $('#state').val());
                $('#cityClientCard').html('<strong>Ciudad: </strong>' + $('#inputClientCity').val());
                $('#addressClientCard').html('<strong>Dirección: </strong>' + $('#inputClientAddress').val());

                $('#AuthaddressOfSend').val($('#inputClientAddress').val());
                $('#Authemail').val($('#inputClientEmail').val());
                $('#Authphone').val($('#inputClientMovil').val());

                //Valor del ID en la imagen adjunta
                $('#TextoIDImg').val($('#inputClientIdentidad').val());
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    });
});