$(document).ready(function () {

    $(".deleteVA").click(function () {
        var vaId = $(this).attr("vaId");
        var nombre = $(this).attr("name");
        var okConfirm = function () {
            var urlDelete = "/ValorAduanals/Delete/" + vaId;
            $.ajax({
                type: "GET",
                url: urlDelete,
                async: false,
                success: function () {
                    document.location = "/ValorAduanals?msg=successDelete&nombre=" + nombre;
                }
            });
        };
        getDelConfirmation(okConfirm);
    });

});