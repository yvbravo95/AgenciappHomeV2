$(document).ready(function () {

    $(".select2-placeholder-agencyName").select2({
        placeholder: "Nombre",
        val: null
    });

    $("#btnGuardar").click(function () {

        var agencyId = $(".select2-placeholder-agencyName").val();
        if (agencyId != "") {

            var datos = [
                $("#orderNumber").html(),
                agencyId
            ];

            $.ajax({
                type: "POST",
                url: "/Orders/ChangeAgency",
                data: JSON.stringify(datos),
                dataType: 'json',
                contentType: 'application/json',
                async: false,
                success: function (data) {
                    document.location = "/Orders";
                }
            });
            document.location = "/Orders";
        }
    });

});