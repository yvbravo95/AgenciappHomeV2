$(document).ready(function () {

    $("#btnLogin").click(function (e) {
        
        var datos = [
            $("#username").val(),
            $("#password").val(),
        ];

        $.ajax({
            type: "POST",
            url: "/Home/Auth",
            data: JSON.stringify(datos),
            dataType: 'json',
            contentType: 'application/json',
            async: false,
            success: function (data) {
                if (data.msg == "success")
                    window.location = "/";
                else
                    showErrorMessage("ERROR", "Usuario o contraseña incorrectos.");
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    });
});