$(document).ready(function () {
    $(".select2-placeholder-selectClient").select2({
        placeholder: "Seleccione el clientes a exportar sus datos",
    });

    var ClientId = ""

    $(".select2-placeholder-selectClient").change(function () {
        $("#exportarCliente").removeAttr("disabled")
        ClientId = $(this).val()
        $.ajax({
            type: "POST",
            url: "/Clients/getDataClient/" + ClientId,
            dataType: 'json',
            contentType: 'application/json',
            async: true,
            success: function (response) {
                //response = jQuery.parseJSON(response);
                var html = "<tr><td>" + response["firstname"] + "</td><td>" + response["lastname"] + "</td><td>" + response["phone"] + "</td><td>" + response["email"] + "</td><td>" + response["address"] + "</td><td>" + response["city"] + "</td><td>" + response["state"] + "</td><td>" + response["zip"] +"</td></tr>"
                $("#previewData").html(html)
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    })
});