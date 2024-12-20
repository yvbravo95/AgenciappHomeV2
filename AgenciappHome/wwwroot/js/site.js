$(document).ready(function () {
	$(".dropdown-toggle").dropdown();

    showOKMessage = function (title, msg) {
        toastr.success(msg, title);
    };

    showOKMessage = function (title, msg, param) {
        toastr.success(msg, title, param);
    };

    showWarningMessage = function (title, msg) {
        toastr.warning(msg, title);
    };

    showErrorMessage = function (title, msg) {
        toastr.error(msg, title);
    };

    confirmationMsg = function (title, text, confirmFunction) {
        swal({
            title: title,
            text: text,
            type: "warning",
            showCancelButton: true,
            /*confirmButtonColor: "#F6BB42",*/
            confirmButtonText: "Si",
            cancelButtonText: "No",
            /*closeOnConfirm: false,
            closeOnCancel: false*/
        }, function (isConfirm) {
            if (isConfirm) {
                confirmFunction();
            }
        });
    };

    getDelConfirmation = function (confirmFunction) {
        confirmationMsg("¿Estás seguro que desea eliminar este elemento?", "¡Esta acción no se puede reestablecer!", confirmFunction);
    };

    getCancelConfirmation = function (confirmFunction) {
        confirmationMsg("¿Estás seguro que desea cancelar este elemento?", "¡Esta acción no se puede reestablecer!", confirmFunction);
    };

    getUrlBookingTourAdvisor = async function () {
        const response = await fetch('/account/GetUrlTourAdvisor');

        fetch('/account/GetUrlTourAdvisor')
            .then(response => {
                // Verificando si la respuesta es exitosa (código de estado 200)
                if (!response.ok) {
                    throw new Error('La solicitud no fue exitosa');
                }
                // Parseando la respuesta JSON
                return response.json();
            })
            .then(data => {
                // Manejando los datos recibidos
                $('a.menu-hotel').prop('href', data.url);

                // Aquí puedes realizar operaciones adicionales con los datos recibidos
            })
            .catch(error => {
                // Manejando errores en caso de que la solicitud falle
                console.error('Error en la solicitud:', error);
            });

    };

    getUrlBookingTourAdvisor();
});


