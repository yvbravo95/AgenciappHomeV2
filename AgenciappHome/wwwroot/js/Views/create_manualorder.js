$(".select2-placeholder-selectContact").select2({
    placeholder: "Buscar contacto por telefono, nombre o apellido",
    text: " ",
    ajax: {
        type: "POST",
        dataType: "json",
        delay: 500,
        url: "/Contacts/findContacts",
        data: function (params) {
            var query = {
                search: params.term,
                idClient: $(".select2-placeholder-selectClient").val(),
            };

            // Query parameters will be ?search=[term]&type=public
            return query;
        },
        processResults: function (data) {
            // Transforms the top-level key of the response object from 'items' to 'results'
            return {
                results: $.map(data, function (obj) {
                    return {
                        id: obj.contactId,
                        text: obj.phone1 + "-" + obj.name + " " + obj.lastName,
                    };
                }),
            };
        },
    },
});

$("#inputContactPhoneHome").mask("(000)-000-0000", {
    placeholder: "(000)-000-0000",
});

$("#inputContactPhoneMovil").mask("(000)-000-0000", {
    placeholder: "(000)-000-0000",
});

var selectMunicipios = function () {
    var provincia = $("#provincia").val();
    if (!provincia) return;

    $.ajax({
        url: "/Provincias/Municipios?nombre=" + provincia,
        type: "POST",
        dataType: "json",
        success: function (response) {
            var municipios = $("#municipio");
            municipios.empty();
            municipios.append(new Option());
            for (var i in response) {
                var m = response[i];
                municipios.append(new Option(m, m));
            }
            municipios.val(selected_municipio).trigger("change");
        },
    });
};

var showAllContact = function () {
    $.ajax({
        type: "POST",
        url: "/OrderNew/GetAllContacts",
        dataType: "json",
        contentType: "application/json",
        async: false,
        success: function (data) {
            // Contactos del cliente
            $("[name='selectContact']").empty();
            $("[name='selectContact']").append(new Option());

            var contactData = "";
            for (var i = 0; i < data.length; i++) {
                if (data[i].phones1 != "")
                    contactData =
                        data[i].phone1 + " - " + data[i].name + " " + data[i].lastName;
                else
                    contactData =
                        data[i].phone2 + " - " + data[i].name + " " + data[i].lastName;
                $("[name='selectContact']").append(
                    new Option(contactData, data[i].contactId)
                );
            }
        },
        failure: function (response) {
            showErrorMessage("ERROR", response.responseText);
        },
        error: function (response) {
            showErrorMessage("ERROR", response.responseText);
        },
    });
};

var showContact = function () {
    var idContacto = $("#ContactId").val();
    console.log(idContacto);
    if (idContacto != null && idContacto != "") {
        $.ajax({
            type: "POST",
            url: "/OrderNew/GetContact",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(idContacto),
            async: false,
            success: function (data) {
                $("#inputContactName").val(data.name);
                $("#inputContactLastName").val(data.lastName);
                $("#inputContactPhoneMovil").val(data.movilPhone);
                $("#inputContactPhoneHome").val(data.casaPhone);
                $("#contactDireccion").val(data.direccion);
                $("#provincia").val(data.city).trigger("change");
                $("#municipio").val(data.municipio);
                $("#reparto").val(data.reparto);
                $("#contactCI").val(data.ci);
                selected_municipio = data.municipio;

                $("#destinatario").html(data.name + " " + data.lastName);

                $("a[href='#next']").removeClass("hidden");
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.statusText);
            },
        });
    }
};

var validateEditarContacto = function () {
    if ($("#inputContactName").val() == "") {
        showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
        return false;
    } else if ($("#inputContactLastName").val() == "") {
        showWarningMessage(
            "Atención",
            "El campo Apellidos no puede estar vacío."
        );
        return false;
    } else if (
        $("#inputContactPhoneMovil").val() == "" &&
        $("#inputContactPhoneHome").val() == ""
    ) {
        showWarningMessage(
            "Atención",
            "Debe introducir al menos un teléfono de contacto."
        );
        return false;
    } else if ($("#contactCI").val().length > 0) {
        if ($("#contactCI").val().length != 11) {
            showWarningMessage(
                "Atención",
                "El carnet de identidad debe tener 11 dígitos"
            );
            return false;
        }
    } else if ($("#contactDireccion").val() == "") {
        showWarningMessage("Atención", "El campo Dirección no puede estar vacío");
        return false;
    } else if ($("#provincia").val() == "") {
        showWarningMessage(
            "Atención",
            "El campo Provincia no puede estar vacío."
        );
        return false;
    } else if ($("#municipio").val() == "") {
        showWarningMessage(
            "Atención",
            "El campo Municipio no puede estar vacío."
        );
        return false;
    }
    return true;
};

var desactContactForm = function () {
    $("#nuevoCliente").removeAttr("disabled");
    $(".select2-placeholder-selectClient").removeAttr("disabled");
    $("#nuevoContacto").removeAttr("disabled");
    $(".select2-placeholder-selectContact").removeAttr("disabled");
    $("#showAllContacts").removeAttr("disabled");
    $("#editarCliente").removeAttr("disabled");

    $("#inputContactName").attr("disabled", "disabled");
    $("#inputContactLastName").attr("disabled", "disabled");
    $("#inputContactPhoneMovil").attr("disabled", "disabled");
    $("#inputContactPhoneHome").attr("disabled", "disabled");
    $("#contactDireccion").attr("disabled", "disabled");
    $("#provincia").attr("disabled", "disabled");
    $("#municipio").attr("disabled", "disabled");
    $("#reparto").attr("disabled", "disabled");
    $("#contactCI").attr("disabled", "disabled");

    $("#editarContacto").removeClass("hidden");
    $("#cancelarContacto").addClass("hidden");
    $("#guardarContacto").addClass("hidden");

    $("a[href='#next']").removeClass("hidden");
    $("#showAllContacts").attr("disabled", "disabled");
};

var cancelarContactForm = function () {
    $("#inputContactName").val($("#inputContactName").data("prevVal"));
    $("#inputContactLastName").val($("#inputContactLastName").data("prevVal"));
    $("#inputContactPhoneMovil").val(
        $("#inputContactPhoneMovil").data("prevVal")
    );
    $("#inputContactPhoneHome").val(
        $("#inputContactPhoneHome").data("prevVal")
    );
    $("#contactDireccion").val($("#contactDireccion").data("prevVal"));
    $("#provincia").val($("#provincia").data("prevVal")).trigger("change");
    $("#reparto").val($("#reparto").data("prevVal"));
    $("#municipio").val($("#municipio").data("prevVal"));
    $("#contactCI").val($("#contactCI").data("prevVal"));

    desactContactForm();
};

var mostrarEstados = function () {
    $("#inputClientState").empty();
    $("#inputClientState").append(new Option());
    $("#inputClientState").append(new Option("Alabama", "Alabama"));
    $("#inputClientState").append(new Option("Alaska", "Alaska"));
    $("#inputClientState").append(
        new Option("American Samoa", "American Samoa")
    );
    $("#inputClientState").append(new Option("Arizona", "Arizona"));
    $("#inputClientState").append(new Option("Arkansas", "Arkansas"));
    $("#inputClientState").append(
        new Option("Armed Forces Americas", "Armed Forces Americas")
    );
    $("#inputClientState").append(
        new Option(
            "Armed Forces Europe, Canada, Africa and Middle East",
            "Armed Forces Europe, Canada, Africa and Middle East"
        )
    );
    $("#inputClientState").append(
        new Option("Armed Forces Pacific", "Armed Forces Pacific")
    );
    $("#inputClientState").append(new Option("California", "California"));
    $("#inputClientState").append(new Option("Colorado", "Colorado"));
    $("#inputClientState").append(new Option("Connecticut", "Connecticut"));
    $("#inputClientState").append(new Option("Delaware", "Delaware"));
    $("#inputClientState").append(
        new Option("District of Columbia", "District of Columbia")
    );
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
    $("#inputClientState").append(
        new Option("Marshall Islands", "Marshall Islands")
    );
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
    $("#inputClientState").append(
        new Option("North Carolina", "North Carolina")
    );
    $("#inputClientState").append(new Option("North Dakota", "North Dakota"));
    $("#inputClientState").append(
        new Option("Northern Mariana Islands", "Northern Mariana Islands")
    );
    $("#inputClientState").append(new Option("Ohio", "Ohio"));
    $("#inputClientState").append(new Option("Oklahoma", "Oklahoma"));
    $("#inputClientState").append(new Option("Oregon", "Oregon"));
    $("#inputClientState").append(new Option("Palau", "Palau"));
    $("#inputClientState").append(new Option("Pennsylvania", "Pennsylvania"));
    $("#inputClientState").append(new Option("Puerto Rico", "Puerto Rico"));
    $("#inputClientState").append(new Option("Rhode Island", "Rhode Island"));
    $("#inputClientState").append(
        new Option("South Carolina", "South Carolina")
    );
    $("#inputClientState").append(new Option("South Dakota", "South Dakota"));
    $("#inputClientState").append(new Option("Tennessee", "Tennessee"));
    $("#inputClientState").append(new Option("Texas", "Texas"));
    $("#inputClientState").append(new Option("Utah", "Utah"));
    $("#inputClientState").append(new Option("Vermont", "Vermont"));
    $("#inputClientState").append(
        new Option("Virgin Islands", "Virgin Islands")
    );
    $("#inputClientState").append(new Option("Virginia", "Virginia"));
    $("#inputClientState").append(new Option("Washington", "Washington"));
    $("#inputClientState").append(new Option("West Virginia", "West Virginia"));
    $("#inputClientState").append(new Option("Wisconsin", "Wisconsin"));
    $("#inputClientState").append(new Option("Wyoming", "Wyoming"));
};

mostrarEstados();

function save() {
    if (validate()) {
        const source = {
            contactId: $('#ContactId').val(),
            retailId: $('#RetailId').val(),
            number: $('#Number').val(),
            clientName: $('#ClientName').val(),
            cantLb: parseFloat($('#CantLb').val())
        }

        $.ajax({
            type: "POST",
            url: "/AirShipping/CreateManualOrder",
            data: JSON.stringify(source),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: true,
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                $.unblockUI();

                if (response.success) {
                    location.href = "/airshipping/createmanualorder?msg=Orden guardada con exito"; 
                }
                else {
                    showErrorMessage("ERROR", response.message);
                }
            },
            failure: function (response) {
                $.unblockUI();
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                $.unblockUI();
                showErrorMessage("ERROR", response.responseText);
            },
        });
    }
}

function validate() {

    if (!$('#ContactId').val()) {
        showErrorMessage("ERROR", "El contacto es obligatorio");
        return false;
    }
    if (!$('#RetailId').val()) {
        showErrorMessage("ERROR", "El minorista es obligatorio");
        return false;
    }
    if (!$('#Number').val()) {
        showErrorMessage("ERROR", "El numero es obligatorio");
        return false;
    }
    if (!$('#ClientName').val()) {
        showErrorMessage("ERROR", "El cliente es obligatorio");
        return false;
    }
    if (parseFloat($('#CantLb').val()) <= 0) {
        showErrorMessage("ERROR", "La cantidad de libras debe ser mayor que 0");
        return false;
    }
    return true;
}

$(document).ready(function () {
    $('#ContactId').on("change", function () {
        showContact()
    })

    $("#provincia").on("change", selectMunicipios);
    $(".select2-placeholder-selectContact").on("select2:select", function () {
        $("#editarContacto").removeClass("hidden hide-search-contactCity");
        const id = $(this).val();
        $('#ContactId').val(id).trigger("change")
    });

    if (
        selectedContact != null &&
        selectedContact != "00000000-0000-0000-0000-000000000000"
    ) {
        $('#ContactId').val(selectedContact).trigger("change");
        selectedContact = "00000000-0000-0000-0000-000000000000";
    }

    $("#editarContacto").on("click", function () {
        // para que no pueda crear nuevo cliente mientras edita contacto
        $("#nuevoCliente").attr("disabled", "disabled");

        // para que no pueda cambiar de cliente mientras edita contacto
        $(".select2-placeholder-selectClient").attr("disabled", "disabled");

        // para que no pueda crear nuevo contacto mientras edita contacto
        $("#nuevoContacto").attr("disabled", "disabled");

        // para que no pueda cambiar de contacto mientras edita contacto
        $(".select2-placeholder-selectContact").attr("disabled", "disabled");

        // para que no pueda cambiar de contacto mientras edita contacto
        $("#showAllContacts").attr("disabled", "disabled");

        // para que no pueda editar cliente mientras edita contacto
        $("#editarCliente").attr("disabled", "disabled");

        // para que no pueda avanzar a la otra parte del formulario
        $("a[href='#next']").addClass("hidden");

        $("#inputContactName")
            .removeAttr("disabled")
            .data("prevVal", $("#inputContactName").val());
        $("#inputContactLastName")
            .removeAttr("disabled")
            .data("prevVal", $("#inputContactLastName").val());
        $("#inputContactPhoneMovil")
            .removeAttr("disabled")
            .data("prevVal", $("#inputContactPhoneMovil").val());
        $("#inputContactPhoneHome")
            .removeAttr("disabled")
            .data("prevVal", $("#inputContactPhoneHome").val());
        $("#contactDireccion")
            .removeAttr("disabled")
            .data("prevVal", $("#contactDireccion").val());
        $("#provincia")
            .removeAttr("disabled")
            .data("prevVal", $("#provincia").val());
        $("#municipio")
            .removeAttr("disabled")
            .data("prevVal", $("#municipio").val());
        $("#reparto").removeAttr("disabled").data("prevVal", $("#reparto").val());
        $("#contactCI")
            .removeAttr("disabled")
            .data("prevVal", $("#contactCI").val());

        $("#editarContacto").addClass("hidden");
        $("#cancelarContacto").removeClass("hidden");
        $("#guardarContacto").removeClass("hidden");
    });

    $("#guardarContacto").on("click", function () {
        selectedContact = $(".select2-placeholder-selectContact").val();
        if (validateEditarContacto()) {
            var source = [
                $(".select2-placeholder-selectContact").val(),
                $("#inputContactName").val(),
                $("#inputContactLastName").val(),
                $("#inputContactPhoneMovil").val(),
                $("#inputContactPhoneHome").val(),
                $("#contactDireccion").val(),
                $("#provincia").val(),
                $("#municipio").val(),
                $("#reparto").val(),
                $("#contactCI").val(),
                $(".select2-placeholder-selectClient").val(),
            ];

            $.ajax({
                type: "POST",
                url: "/Contacts/EditContact",
                data: JSON.stringify(source),
                dataType: "json",
                contentType: "application/json",
                async: false,
                success: function () {
                    showOKMessage("Editar Contacto", "Contacto editado con éxito");

                    showContactsOfAClient();
                },
                failure: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                },
                error: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                },
            });

            desactContactForm();
        }
    });

    $("#cancelarContacto").click(cancelarContactForm);

    $('#save').on('click', save)
})



