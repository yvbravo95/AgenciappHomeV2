$(document).ready(function () {
    var url = decodeURIComponent(window.location);
    var params = url.split("?")[1];
    if (params != null) {
        var params = params.split("&");
        var msg = params[0].split("=")[1];
        if (msg == "success") {
            var nombre = params[1].split("=")[1];
            showOKMessage("Nuevo Contacto", "Contacto " + nombre + " se ha adicionado con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successEdit") {
            var nombre = params[1].split("=")[1];
            showOKMessage("Editar Contacto", "Contacto " + nombre + " editado con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successDelete") {
            var nombre = params[1].split("=")[1];
            showOKMessage("Eliminar Contacto", "Contacto " + nombre + " eliminado con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successImport") {
            var cantContactsImport = params[1].split("=")[1];
            if (cantContactsImport == 1)
                showOKMessage("Importar Contactos", "1 contacto importado con éxito", { "timeOut": 0, "closeButton": true });
            else
                showOKMessage("Importar Contactos", cantContactsImport + " contactos importados con éxito", { "timeOut": 0, "closeButton": true });
        }
    }

    var searchAction = function () {
        var searchVal = $("#searchField").val().toLowerCase();

        $("#tableContacts tr").removeClass("hidden");

        var tBody = $("#tableContacts > tbody")[0];

        for (var i = 0; i < tBody.rows.length; i++) {
            var fila = tBody.rows[i];
            if (!$(fila.children[0]).html().toLowerCase().includes(searchVal) && !$(fila.children[1]).html().toLowerCase().includes(searchVal) && !$(fila.children[2]).html().toLowerCase().includes(searchVal) && !$(fila.children[3]).html().toLowerCase().includes(searchVal) && !$(fila.children[4]).html().toLowerCase().includes(searchVal)) {
                $(fila).addClass("hidden");
            }
        }

        var cantHide = $("#tableContacts tr.hidden").length;
        if (tBody.rows.length == cantHide)
            $("#no_result").removeClass("hidden");
        else
            $("#no_result").addClass("hidden");
    };

    $("#btnSearch").click(searchAction);
    $("#searchField").on("keyup", searchAction);

    $(".deleteContacts").click(function () {
        var contactId = $(this).attr("contactId");
        var nombre = $(this).attr("name");
        var okConfirm = function () {
            var urlDelete = "/Contacts/Delete/" + contactId;
            $.ajax({
                type: "GET",
                url: urlDelete,
                async: false,
                success: function (response) {
                    if (response["status"] == "ok")
                        document.location = "/Contacts?msg=successDelete&nombre=" + nombre;
                    else if (response["status"] == "inorder")
                        showWarningMessage("Atención", "Este contacto no puede eliminarse porque pertenece a una orden. ");
                }
            });
        };
        getDelConfirmation(okConfirm);
    });

});