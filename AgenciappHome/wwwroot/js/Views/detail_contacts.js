$(document).ready(function () {
    $(".deleteContacts").click(function () {
        var contactId = $(this).attr("contactId");
        var nombre = $(this).attr("name");
        var okConfirm = function () {
            var urlDelete = "/Contacts/Delete/" + contactId;
            $.ajax({
                type: "GET",
                url: urlDelete,
                async: false,
                success: function () {
                    document.location = "/Contacts?msg=successDelete&nombre=" + nombre;
                }
            });
        };
        getDelConfirmation(okConfirm);
    });

});