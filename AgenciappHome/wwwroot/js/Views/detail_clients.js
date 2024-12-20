$(document).ready(function () {
    $(".deleteClients").click(function () {
        var clientId = $(this).attr("clientId");
        var nombre = $(this).attr("name");
        var okConfirm = function () {
            var urlDelete = "/Clients/Delete/" + clientId;
            $.ajax({
                type: "GET",
                url: urlDelete,
                async: false,
                success: function () {
                    document.location = "/Clients?msg=successDelete&nombre=" + nombre;
                }
            });
        };
        getDelConfirmation(okConfirm);
    });

});