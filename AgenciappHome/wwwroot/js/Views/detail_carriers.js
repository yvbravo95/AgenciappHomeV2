$(document).ready(function () {
    $(".deleteCarriers").click(function () {
        var carrierId = $(this).attr("carrierId");
        var nombre = $(this).attr("name");
        var okConfirm = function () {
            var urlDelete = "/Carriers/Delete/" + carrierId;
            $.ajax({
                type: "GET",
                url: urlDelete,
                async: false,
                success: function () {
                    document.location = "/Carriers?msg=successDelete&nombre=" + nombre;
                }
            });
        };
        getDelConfirmation(okConfirm);
    });

});