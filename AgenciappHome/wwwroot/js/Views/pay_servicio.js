$(document).ready(function () {

    $(".hide-search-pago").select2({
        minimumResultsForSearch: Infinity,
        placeholder: "Tipo de Pago",
    });

    $("#guardar").click(function () {
        if (validatePay()) {
            var okConfirm = function () {
                var datos = [
                    $("#orderNumber").html(),
                    $(".hide-search-pago").val(),
                    $("#cantPay").val().replace(",", "."),
                ];

                $.ajax({
                    type: "POST",
                    url: "/Servicios/PayServicio",
                    data: JSON.stringify(datos),
                    dataType: 'json',
                    contentType: 'application/json',
                    async: false,
                    success: function (data) {
                        window.location = "/Servicios/Index?msg=successPago&noPago=" + data.number;
                    }
                });
            };
            confirmationMsg("¿Está seguro que desea registrar este pago?", "¡Esta acción no se puede reestablecer!", okConfirm);
        }
    });

    var validatePay = function () {
        if ($("#cantPay").val() == "") {
            showWarningMessage("Atención", "El campo Cant. a pagar o esta vacio o debe ser expresado con punto flotante y no con coma.");
            return false;
        }
        else if ($("#cantPay").val() <= 0) {
            showWarningMessage("Atención", "El campo Cant. a pagar debe ser mayor que 0.");
            return false;
        } else if (parseFloat($("#cantPay").val()) > parseFloat($("#porPagar").html())) {
            showWarningMessage("Atención", "El campo Cant. a pagar debe ser menor o igual que la cantidad que queda por pagar.");
            return false;
        }

        return true;
    };
});