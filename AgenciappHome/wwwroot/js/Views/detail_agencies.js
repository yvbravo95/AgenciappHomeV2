$(document).ready(function () {

    $(".deleteAgencies").click(function () {
        var agencyId = $(this).attr("agencyId");
        var nombre = $(this).attr("name");
        var okConfirm = function () {
            var urlDelete = "/Agencies/Delete/" + agencyId;
            $.ajax({
                type: "GET",
                url: urlDelete,
                async: false,
                success: function () {
                    document.location = "/Agencies?msg=successDelete&nombre=" + nombre;
                }
            });
        };
        getDelConfirmation(okConfirm);
    });

});