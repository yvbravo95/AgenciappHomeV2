$(document).ready(function () {
    $(".deleteUsers").click(function () {
        var userId = $(this).attr("userId");
        var nombre = $(this).attr("name");
        var okConfirm = function () {
            var urlDelete = "/Users/Delete/" + userId;
            $.ajax({
                type: "GET",
                url: urlDelete,
                async: false,
                success: function () {
                    document.location = "/Users?msg=successDelete&nombre=" + nombre;
                }
            });
        };
        getDelConfirmation(okConfirm);
    });

});