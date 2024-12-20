$(document).ready(function () {

    var url = decodeURIComponent(window.location);
    var params = String.split(url, "?")[1];
    if (params != null) {
        var params = String.split(params, "&");
        var msg = String.split(params[0], "=")[1];
        if (msg == "success") {
            var nombre = String.split(params[1], "=")[1];
            showOKMessage("Nuevo Valor Aduanal", "Valor Aduanal " + nombre + " se ha creado con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successEdit") {
            var nombre = String.split(params[1], "=")[1];
            showOKMessage("Editar Valor Aduanal", "Valor Aduanal " + nombre + " ha sido editado con éxito", { "timeOut": 0, "closeButton": true });
        } else if (msg == "successDelete") {
            var nombre = String.split(params[1], "=")[1];
            showOKMessage("Eliminar Valor Aduanal", "Valor Aduanal " + nombre + " ha sido eliminado con éxito", { "timeOut": 0, "closeButton": true });
        }
    }

    var searchAction = function () {
        var searchVal = $("#searchField").val().toLowerCase();

        $("#tableVA tr").removeClass("hidden");

        var tBody = $("#tableVA > tbody")[0];

        for (var i = 0; i < tBody.rows.length; i++) {
            var fila = tBody.rows[i];
            if (!$(fila.children[0]).html().toLowerCase().includes(searchVal) && !$(fila.children[1]).html().toLowerCase().includes(searchVal)) {
                $(fila).addClass("hidden");
            }
        }

        var cantHide = $("#tableVA tr.hidden").length;
        if (tBody.rows.length == cantHide)
            $("#no_result").removeClass("hidden");
        else
            $("#no_result").addClass("hidden");
    };

    $("#btnSearch").click(searchAction);
    $("#searchField").on("keyup", searchAction);

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