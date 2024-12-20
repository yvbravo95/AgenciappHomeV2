$(document).ready(function () {
    $(".deleteShipping").click(function () {
        var packingId = $(this).attr("packingId");
        var okConfirm = function () {
            window.location = "/Shippings/Delete/" + packingId;
        };
        getDelConfirmation(okConfirm);
    });

});