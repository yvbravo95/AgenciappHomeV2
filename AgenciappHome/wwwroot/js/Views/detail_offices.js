$(document).ready(function () {
$(".deleteOffices").click(function () {
        var officeId = $(this).attr("officeId");
        var nombre = $(this).attr("name");
        var okConfirm = function () {
            var urlDelete = "/Offices/Delete/" + officeId;
            $.ajax({
                type: "GET",
                url: urlDelete,
                async: false,
                success: function () {
                    document.location = "/Offices?msg=successDelete&nombre=" + nombre;
                }
            });
        };
        getDelConfirmation(okConfirm);
    });

});