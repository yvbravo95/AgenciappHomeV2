
$(document).ready(function () {
    loadStates();

    var phoneElements = document.getElementsByClassName('phone');
    for (var i = 0; i < phoneElements.length; i++) {
        phoneElements[i].addEventListener('input', function (e) {
            this.value = this.value.replace(/[^0-9]/g, '');
        });
    }

    $('#Clientstate').on('change', function () {
        loadCiudades($(this), $('#Clientcity'));
    })

    $('#nuevoContactProvincia').on('change', function () {
        loadMunicipios($(this), $("#nuevoContactMunicipio"));
    })

    $('#btnSaveClient').on('click', save);
});

function loadStates() {
    $("select[id=Clientstate]").empty()
    $("select[id=Clientstate]").append(new Option())
    $("select[id=Clientstate]").append(new Option("Alabama", "Alabama"))
    $("select[id=Clientstate]").append(new Option("Alaska", "Alaska"))
    $("select[id=Clientstate]").append(new Option("American Samoa", "American Samoa"))
    $("select[id=Clientstate]").append(new Option("Arizona", "Arizona"))
    $("select[id=Clientstate]").append(new Option("Arkansas", "Arkansas"))
    $("select[id=Clientstate]").append(new Option("Armed Forces Americas", "Armed Forces Americas"))
    $("select[id=Clientstate]").append(new Option("Armed Forces Europe, Canada, Africa and Middle East", "Armed Forces Europe, Canada, Africa and Middle East"))
    $("select[id=Clientstate]").append(new Option("Armed Forces Pacific", "Armed Forces Pacific"))
    $("select[id=Clientstate]").append(new Option("California", "California"))
    $("select[id=Clientstate]").append(new Option("Colorado", "Colorado"))
    $("select[id=Clientstate]").append(new Option("Connecticut", "Connecticut"))
    $("select[id=Clientstate]").append(new Option("Delaware", "Delaware"))
    $("select[id=Clientstate]").append(new Option("District of Columbia", "District of Columbia"))
    $("select[id=Clientstate]").append(new Option("Florida", "Florida"))
    $("select[id=Clientstate]").append(new Option("Georgia", "Georgia"))
    $("select[id=Clientstate]").append(new Option("Guam", "Guam"))
    $("select[id=Clientstate]").append(new Option("Hawaii", "Hawaii"))
    $("select[id=Clientstate]").append(new Option("Idaho", "Idaho"))
    $("select[id=Clientstate]").append(new Option("Illinois", "Illinois"))
    $("select[id=Clientstate]").append(new Option("Indiana", "Indiana"))
    $("select[id=Clientstate]").append(new Option("Iowa", "Iowa"))
    $("select[id=Clientstate]").append(new Option("Kansas", "Kansas"))
    $("select[id=Clientstate]").append(new Option("Kentucky", "Kentucky"))
    $("select[id=Clientstate]").append(new Option("Luisiana", "Luisiana"))
    $("select[id=Clientstate]").append(new Option("Maine", "Maine"))
    $("select[id=Clientstate]").append(new Option("Marshall Islands", "Marshall Islands"))
    $("select[id=Clientstate]").append(new Option("Maryland", "Maryland"))
    $("select[id=Clientstate]").append(new Option("Massachusetts", "Massachusetts"))
    $("select[id=Clientstate]").append(new Option("Michigan", "Michigan"))
    $("select[id=Clientstate]").append(new Option("Micronesia", "Micronesia"))
    $("select[id=Clientstate]").append(new Option("Minnesota", "Minnesota"))
    $("select[id=Clientstate]").append(new Option("Mississippi", "Mississippi"))
    $("select[id=Clientstate]").append(new Option("Missouri", "Missouri"))
    $("select[id=Clientstate]").append(new Option("Montana", "Montana"))
    $("select[id=Clientstate]").append(new Option("Nebraska", "Nebraska"))
    $("select[id=Clientstate]").append(new Option("Nevada", "Nevada"))
    $("select[id=Clientstate]").append(new Option("New Hampshire", "New Hampshire"))
    $("select[id=Clientstate]").append(new Option("New Jersey", "New Jersey"))
    $("select[id=Clientstate]").append(new Option("New Mexico", "New Mexico"))
    $("select[id=Clientstate]").append(new Option("New York", "New York"))
    $("select[id=Clientstate]").append(new Option("North Carolina", "North Carolina"))
    $("select[id=Clientstate]").append(new Option("North Dakota", "North Dakota"))
    $("select[id=Clientstate]").append(new Option("Northern Mariana Islands", "Northern Mariana Islands"))
    $("select[id=Clientstate]").append(new Option("Ohio", "Ohio"))
    $("select[id=Clientstate]").append(new Option("Oklahoma", "Oklahoma"))
    $("select[id=Clientstate]").append(new Option("Oregon", "Oregon"))
    $("select[id=Clientstate]").append(new Option("Palau", "Palau"))
    $("select[id=Clientstate]").append(new Option("Pennsylvania", "Pennsylvania"))
    $("select[id=Clientstate]").append(new Option("Puerto Rico", "Puerto Rico"))
    $("select[id=Clientstate]").append(new Option("Rhode Island", "Rhode Island"))
    $("select[id=Clientstate]").append(new Option("South Carolina", "South Carolina"))
    $("select[id=Clientstate]").append(new Option("South Dakota", "South Dakota"))
    $("select[id=Clientstate]").append(new Option("Tennessee", "Tennessee"))
    $("select[id=Clientstate]").append(new Option("Texas", "Texas"))
    $("select[id=Clientstate]").append(new Option("Utah", "Utah"))
    $("select[id=Clientstate]").append(new Option("Vermont", "Vermont"))
    $("select[id=Clientstate]").append(new Option("Virgin Islands", "Virgin Islands"))
    $("select[id=Clientstate]").append(new Option("Virginia", "Virginia"))
    $("select[id=Clientstate]").append(new Option("Washington", "Washington"))
    $("select[id=Clientstate]").append(new Option("West Virginia", "West Virginia"))
    $("select[id=Clientstate]").append(new Option("Wisconsin", "Wisconsin"))
    $("select[id=Clientstate]").append(new Option("Wyoming", "Wyoming"))
}

function loadCiudades (estado, ciudad) {
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
            }
        }
    })
}

function loadMunicipios(selectorProvincia, selectorMunicipio) {
    $.ajax({
        url: "/Provincias/Municipios?nombre=" + selectorProvincia.val(),
        type: "POST",
        dataType: "json",
        success: function (response) {
            selectorMunicipio.empty();
            selectorMunicipio.append(new Option())
            for (var i in response) {
                var m = response[i];
                selectorMunicipio.append(new Option(m, m))
            }
        }
    })
}

async function save() {
    if (!validateClient() || !validateContact()) return;

    $.blockUI();
    const clientResult = await saveClient();
    if (!clientResult.success) {
        toastr.error(clientResult.msg, "Error");
        $.unblockUI();
        return;
    }

    const contactResult = await saveContact(clientResult.data.idClient);
    if (!contactResult.success) {
        toastr.error(contactResult.msg, "Error");
        $.unblockUI();
        return;
    }

    $.unblockUI();
    $('#div_create').hide();
    $('#div_success').show();
}

async function saveClient() {
    const response = await $.ajax({
        async: true,
        type: "POST",
        contentType: "application/x-www-form-urlencoded",
        url: "/Clients/AddClientNew",
        data: {
            name: $("#Name").val(),
            lastname: $("#Lastname").val(),
            phone: $("#Phone").val(),
            email: $("#Email").val(),
            id: $("#ID").val(),
            address: $("#Address").val(),
            state: $("#Clientstate").val(),
            city: $("#Clientcity").val(),
            zip: $("#Zip").val(),
            check: $('#acepto').is(':checked'),
            FechaNac: $("#fechaNac").val(),
            name2: $("#Name2").val(),
            lastname2: $("#Lastname2").val(),
            agencyId: agencyId
        },
        success: function (response) {
            return response;
        },
        error: function () {
            toastr.error("No se ha podido crear el usuario, vuelva a intentar", "Error");
            return null;
        },

    });

    return response;
}

async function saveContact(selectedClient) {
    var source = [
        selectedClient,    // id del cliente al cual se asocia hay q declararla en donde se use este script
        $('#nuevoContactName').val(),
        $('#nuevoContactLastName').val(),
        $('#nuevoContactPhoneMovil').val(),
        $('#nuevoContactPhoneHome').val(),
        $('#nuevoContactDir').val(),
        $('#nuevoContactProvincia').val(),
        $('#nuevoContactMunicipio').val(),
        $('#nuevoContactReparto').val(),
        $('#nuevoContactCI').val(),
        2,
    ]

    const result = await $.ajax({
        type: "POST",
        url: "/Contacts/AddContact?agencyId=" + agencyId,
        data: JSON.stringify(source),
        dataType: 'json',
        contentType: 'application/json',
        async: false,
        success: function (response) {
            return response;
        },
        failure: function (response) {
            showErrorMessage("ERROR", response.responseText);
            return null;
        },
        error: function (response) {
            showErrorMessage("ERROR", response.responseText);
            return null;
        }
    });

    return result;
}

function validateClient() {
    if ($("#Name").val() == "") {
        showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
        return false;
    }
    if ($("#Lastname").val() == "") {
        showWarningMessage("Atención", "El campo Apellidos no puede estar vacío.");
        return false;
    }
    if ($("#Phone").val() == "") {
        showWarningMessage("Atención", "El campo Teléfono Móvil no puede estar vacío.");
        return false;
    }

    var p = $("#Phone").val();
    if (p.length != 10) {
        showWarningMessage("Atención", "El campo Teléfono Móvil debe tener 10 dígitos");
        return false;
    }
    return true;
}

function validateContact() {
    if ($("#nuevoContactName").val() == "") {
        showWarningMessage("Atención", "El campo Nombre no puede estar vacío.");
        return false;
    } else if ($("#nuevoContactLastName").val() == "") {
        showWarningMessage("Atención", "El campo Apellidos no puede estar vacío.");
        return false;
    } else if ($("#nuevoContactPhoneMovil").val() == "" && $("#nuevoContactPhoneHome").val() == "") {
        showWarningMessage("Atención", "Debe introducir al menos un teléfono de contacto.");
        return false;
    }
    else if ($("#nuevoContactCI").val().length > 0 && $("#nuevoContactCI").val().length != 11) {
        showWarningMessage("Atención", "El campo Carnet de Identidad debe poseer 11 dígitos.");
        return false;
    }
    else if ($("#nuevoContactDir").val() == "") {
        showWarningMessage("Atención", "El campo Dirección no puede estar vacío");
        return false;
    } else if ($("#nuevoContactProvincia").val() == "") {
        showWarningMessage("Atención", "El campo Provincia no puede estar vacío.");
        return false;
    } else if ($("#nuevoContactMunicipio").val() == "") {
        showWarningMessage("Atención", "El campo Municipio no puede estar vacío.");
        return false;
    }
    const splitLasName = $("#nuevoContactLastName").val().trim().split(' ');
    if (splitLasName.length <= 1) {
        showWarningMessage(
            "Atención",
            "El campo Apellidos debe contener el primer y segundo apellido."
        );
        return false;
    }

    return true;
}