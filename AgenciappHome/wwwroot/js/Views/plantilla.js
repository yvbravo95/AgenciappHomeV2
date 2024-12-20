$.ajax({
    async: true,
    type: "POST",
    dataType: "json",
    contentType: "application/x-www-form-urlencoded",
    url: "/Home/getmenus",
    beforeSend: function () {

    },
    success: function (response) {
        var menu = $('#menu');
        for (var i = 0; i < response.length; i++) {
            var url = ("/" + response[i].controller + "/" + response[i].action).toLowerCase();
            if (response[i].controller.toLowerCase() == "home" && response[i].action.toLowerCase() == "index") {
                url = "/"
            }
            var elem = menu.find('a[href = "' + url + '"]');
            if (elem.length != 0) {
                var elem = elem[0];
                // Obtengo el parent ul 
                var ul = elem.parentNode.parentNode;
                elem.parentNode.remove();
                auxMenus(ul);
            }
            else {
                // Verifico si el action es Index
                url = ("/" + response[i].controller + "/").toLowerCase();
                elem = menu.find('a[href = "' + url + '"]');
                if (elem.length != 0) {
                    var elem = elem[0];
                    // Obtengo el parent ul 
                    var ul = elem.parentNode.parentNode;
                    elem.parentNode.remove();
                    auxMenus(ul);
                }
            }
        }
        UserViewAdmin();
        isMinoristaCubiq();
        HideMenusMCOMutiservices();
        $('#menu').show();
    },
    error: function () {
        // alert("Error al crear menús")
    },
    timeout: 120000,
}); //Para establecer los menus del usuario

function auxMenus(ul) {
    //Si el id de ul es menu termino de eliminar (No existen elementos en el menu)
    if (ul.id == "menu") {
        return;
    }
    else {
        if (ul.children.length == 0) {
            var aux = ul.parentNode.parentNode;
            ul.parentNode.remove();
            auxMenus(aux);
        }
        else {
            return;
        }
    }

} 

var UserAuth = null;
function UserViewAdmin() {
    $.ajax({
        async: true,
        type: "POST",
        dataType: "json",
        contentType: "application/x-www-form-urlencoded",
        url: "/Users/getUserAuth",
        beforeSend: function () {
            
        },
        success: function (response) {
            UserAuth = response;
            if (!UserAuth.viewAdministracion) {
                $('#menuAdministracion').remove();
            }
            if (!UserAuth.viewindexadmin) {
                if (UserAuth.type != "Agencia")
                $('#indexAdmin').remove();
            }
            if (!UserAuth.viewcuentas) {
                if (UserAuth.type != "Agencia") {
                    $('#menuCuentas').remove();
                }
            }
            else {
                if (UserAuth.userId.toUpperCase() == "B318E8D8-D050-43EC-9E67-C08A04CB9F28") {
                    $('#menuCuentasxCobrar').remove();
                }
            }

            if (UserAuth.agencyRefId != null && UserAuth.agencyRefId != UserAuth.agencyId) {
                $('#btnReturnAgency').show();
            }
            else{
                $('#btnReturnAgency').hide();
            }
        },
        error: function () {
            alert("El menú administración no ha podido ser cargado")
        },
        timeout: 120000,
    });
}

function isMinoristaCubiq() {
    $.ajax({
        async: true,
        type: "POST",
        dataType: "json",
        contentType: "application/x-www-form-urlencoded",
        url: "/Account/isMinoristaCubiq",
        beforeSend: function () {

        },
        success: function (response) {
            if (!response) {
                $("#menu-cubiq").remove();
            }
        },
        error: function (response) {
            console.log(response)
        },
        timeout: 120000,
    });

    

    $('#btnReturnAgency').on('click', function () {
        $.ajax({
            async: true,
            type: "post",
            url: "/Users/visitReturnAgency",
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                if (response.success) {
                    location.href = "/Minorista/Visit"
                }
                else {
                    toastr.error(response.msg);
                }
                $.unblockUI();
            },
            error: function (response) {
                toastr.error("No se ha podido visitar la agencia")
                $.unblockUI();
            }
        })
    })



}

function HideMenusMCOMutiservices() {
    var id = (getcookie("AgencyId")).toLowerCase();
    var compare = "4CD32E7D-45DE-46E7-9944-9F44B03E5BE4".toLowerCase()
    if (id == compare) {
        $('[data-menu="Recarga"]').hide();
        $('[data-menu="Remesas"]').hide();
        $('[data-menu="Reservas"]').hide();
        $('[data-menu="Maritimo"]').hide();
        $('[data-menu="Envios"]').hide();
        $('[data-menu="Cubiq"]').hide();
        $('[data-menu="CubiqGuia"]').hide();
        $('[data-menu="Mercado"]').hide();
        $('[data-menu="Equipaje"]').hide();
        $('[data-menu="Reclamo"]').hide();
        $('[data-menu="ECOrdenesEnviadas"]').hide();
        $('[data-menu="ECOrdenesRevisadas"]').hide();
        $('[data-menu="ECOrdenesEntregadas"]').hide();
        $('[data-menu="ECOCombos"]').hide();
        $('[data-menu="ECEquipajes"]').hide();
    }
}

//get cookies
function getcookie(name) {
    misCookies = document.cookie;
    listaCookies = misCookies.split(";");
    for (i in listaCookies) {
        busca = listaCookies[i].search(name);
        if (busca > -1) {
            micookie = listaCookies[i];
            igual = micookie.indexOf("=");
            valor = micookie.substring(igual + 1);
            var re = /%20/g;
            valor = valor.replace(re, " ");
            return valor
        }

    }
    return "";
}




