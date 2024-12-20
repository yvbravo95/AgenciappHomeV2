$(document).ready(function () {

    var url = decodeURIComponent(window.location);
    var params = String.split(url, "?")[1];
    if (params != null) {
        var params = String.split(params, "&");
        var msg = String.split(params[0], "=")[1];
        if (msg == "success") {
            var nombre = String.split(params[1], "=")[1];
            showOKMessage("Nueva Agencia", "Agencia " + nombre + " se ha creado con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successEdit") {
            var nombre = String.split(params[1], "=")[1];
            showOKMessage("Editar Agencia", "Agencia " + nombre + " editada con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successDelete") {
            var nombre = String.split(params[1], "=")[1];
            showOKMessage("Eliminar Agencia", "Agencia " + nombre + " eliminada con éxito", { "timeOut": 0, "closeButton": true });
        }
    }

    var searchAction = function () {
        var searchVal = $("#searchField").val().toLowerCase();

        $("#tableAgencies tr").removeClass("hidden");

        var tBody = $("#tableAgencies > tbody")[0];

        for (var i = 0; i < tBody.rows.length; i++) {
            var fila = tBody.rows[i];
            if (!$(fila.children[0]).html().toLowerCase().includes(searchVal) && !$(fila.children[1]).html().toLowerCase().includes(searchVal) && !$(fila.children[3]).html().toLowerCase().includes(searchVal)) {
                $(fila).addClass("hidden");
            }
        }

        var cantHide = $("#tableAgencies tr.hidden").length;
        if (tBody.rows.length == cantHide)
            $("#no_result").removeClass("hidden");
        else
            $("#no_result").addClass("hidden");
    };

    $("#btnSearch").click(searchAction);
    $("#searchField").on("keyup", searchAction);

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