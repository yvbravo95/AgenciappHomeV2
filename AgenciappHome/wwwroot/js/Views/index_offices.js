$(document).ready(function () {

    var url = decodeURIComponent(window.location);
    var params = url.split("?")[1];
    if (params != null) {
        var params = params.split("&");
        var msg = params[0].split("=")[1];
        if (msg == "success") {
            var nombre = params[1].split("=")[1];
            showOKMessage("Nueva Oficina", "Oficina " + nombre + " se ha creado con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successEdit") {
            var nombre = params[1].split("=")[1];
            showOKMessage("Editar Oficina", "Oficina " + nombre + " editada con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successDelete") {
            var nombre = params[1].split("=")[1];
            showOKMessage("Eliminar Oficina", "Oficina " + nombre + " eliminada con éxito", { "timeOut": 0, "closeButton": true });
        }
    }

    var searchAction = function () {
        var searchVal = $("#searchField").val().toLowerCase();

        $("#tableOffices tr").removeClass("hidden");

        var tBody = $("#tableOffices > tbody")[0];

        for (var i = 0; i < tBody.rows.length; i++) {
            var fila = tBody.rows[i];
            if (!$(fila.children[0]).html().toLowerCase().includes(searchVal) && !$(fila.children[1]).html().toLowerCase().includes(searchVal) && !$(fila.children[2]).html().toLowerCase().includes(searchVal)) {
                $(fila).addClass("hidden");
            }
        }

        var cantHide = $("#tableOffices tr.hidden").length;
        if (tBody.rows.length == cantHide)
            $("#no_result").removeClass("hidden");
        else
            $("#no_result").addClass("hidden");
    };

    $("#btnSearch").click(searchAction);
    $("#searchField").on("keyup", searchAction);

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