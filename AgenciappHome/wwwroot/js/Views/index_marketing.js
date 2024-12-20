let isMms = false;
let marketing = null;

$(document).ready(function () {
    $('#select-services').select2({
        placeholder: 'Seleccione los servicios a filtrar',
    })

    $('#select-provinces').select2({
        placeholder: 'Seleccione las provincias a filtrar',
    })

    $('#btn-search').on('click', getData);

    $('#text-message').on('keypress', function (event) {
        // Obtener el código de la tecla pulsada
        var keyCode = event.keyCode;

        if (isMms) return true;

        // Permitir letras y números (códigos de teclas entre 48 y 90) permitir punto, coma y espacio
        if ((keyCode >= 48 && keyCode <= 57) || // Números
            (keyCode >= 65 && keyCode <= 90) || // Letras mayúsculas
            (keyCode >= 97 && keyCode <= 122) || // Letras minúsculas
            (keyCode == 32 || keyCode == 46 || keyCode == 44)) { // tecla espacio
            return true; // Permitir la pulsación de tecla
        } else {
            // Si se pulsa otra tecla, cancelar la pulsación de tecla
            event.preventDefault();
            return false;
        }
    });

    $('#text-message').on('input', function (event) {
        const length = $('#text-message').val().length;
        $('#qty-characters').html(`${length}`)
    });

    $('#check-mms').on('change', function () {
        isMms = $(this).is(':checked');
        $('#text-message').attr('maxlength', isMms ? 1000 : 160);
        if (isMms) {
            $('#input-file-image').show()
        }
        else {
            $('#input-file-image').hide()
        }

        $('#text-message').val('').trigger('input');
    })

    $('#btn-send').on('click', sendCampaing);

    $('#select-services').val(['all']).trigger('change');

    $('#btn-configuration').on('click', function () {
        $.ajax({
            async: true,
            type: "Get",
            url: "/Marketing/GetMarketing",
            data: {},
            success: function (response) {
                if (response.success) {
                    marketing = response.data;
                    $('#input-secretkey').val(response.data.stripeSecretkey);
                    $('#input-customerId').val(response.data.stripeCustomerId);
                    $('#input-numberFrom').val(response.data.numberFrom);
                    $('#input-accountSid').val(response.data.accountSid);
                    $('#input-authToken').val(response.data.authToken);
                    $('#input-priceSMS').val(response.data.priceSms);
                    $('#input-priceMMS').val(response.data.priceMms);

                    $('#modal-config').modal('show');
                }
                else {
                    toastr.error(response.message);
                }
            },
            error: function (e) {
                console.log(e);
                toastr.error("No se ha podido obtener la configuracion");
            }
        });
    });

    $('#btn-save-configuration').on('click', function () {
        marketing.stripeSecretkey = $('#input-secretkey').val();
        marketing.stripeCustomerId = $('#input-customerId').val();
        marketing.numberFrom = $('#input-numberFrom').val();
        marketing.accountSid = $('#input-accountSid').val();
        marketing.authToken = $('#input-authToken').val();
        marketing.priceSms = $('#input-priceSMS').val();
        marketing.priceMms = $('#input-priceMMS').val();

        const jsonData = JSON.stringify(marketing);
        $.ajax({
            url: '/marketing/UpdateMarketing',
            type: 'POST',
            data: jsonData,
            contentType: 'application/json',
            success: function (response) {
                if (response.success) {
                    marketing = response.data;
                    toastr.success("Se ha actualizado con exito");
                }
                else {
                    toastr.error(response.message);
                }
            },
            error: function () {
                console.log("Ha ocurrido un error.");
            }
        });
    })

    // set range date min value
    const dateSplit = $('#daterange').val().split(' - ');
    dateSplit[0] = `${new Date(0).toLocaleDateString('en-US')} 12:00 AM`;
    x = $('#daterange').val(`${dateSplit[0]} - ${dateSplit[1]}`).trigger('change');

    getData();
    getRegisters();
});

// Array de datos
var datos = []; // Tu array de datos

// Configuración del DataTable
var tabla = $('#tblClients').DataTable({
    serverSide: true,
    processing: true,
    searching: false,
    lengthChange: false,
    ajax: function (data, callback, settings) {
        // Calcula el índice del primer registro de la página actual
        var startIndex = data.start;
        // Calcula el índice del último registro de la página actual
        var endIndex = data.start + data.length;

        // Filtra los datos para obtener solo los de la página actual
        var pageData = datos.slice(startIndex, endIndex);

        // Llama al callback con los datos de la página actual
        callback({
            draw: data.draw,
            recordsTotal: datos.length, // Total de registros en el arreglo completo
            recordsFiltered: datos.length, // Total de registros después de aplicar los filtros
            data: pageData // Datos de la página actual
        });
    },
    columns: [
        { data: 'phone', title: 'Telefono' },
        { data: 'name', title: 'Nombre' }
        // Agrega más columnas si es necesario
    ]
});

var registros = [];

var columns = [
    { data: 'createdAt', title: 'Fecha', render: function (data) { return formatDate(new Date(data)) } },
    { data: 'totalSend', title: 'Total' },
    { data: 'isMms', title: 'MMS' },
    { data: 'amount', title: 'Monto' },
    { data: 'message', title: 'Mensaje' }
    // Agrega más columnas si es necesario
]

if (username == '000') {
    columns = [
        { data: 'createdAt', title: 'Fecha', render: function (data) { return formatDate(new Date(data)) } },
        { data: 'totalSend', title: 'Total' },
        { data: 'failSend', title: 'Fallidos' },
        { data: 'isMms', title: 'MMS' },
        { data: 'amount', title: 'Monto' },
        { data: 'message', title: 'Mensaje' }
        // Agrega más columnas si es necesario
    ]
}

var tablaRegistros = $('#tblRegisters').DataTable({
serverSide: true,
    processing: true,
    searching: false,
    lengthChange: false,
    ajax: function (data, callback, settings) {
        // Calcula el índice del primer registro de la página actual
        var startIndex = data.start;
        // Calcula el índice del último registro de la página actual
        var endIndex = data.start + data.length;

        // Filtra los datos para obtener solo los de la página actual
        var pageData = registros.slice(startIndex, endIndex);

        // Llama al callback con los datos de la página actual
        callback({
            draw: data.draw,
            recordsTotal: registros.length, // Total de registros en el arreglo completo
            recordsFiltered: registros.length, // Total de registros después de aplicar los filtros
            data: pageData // Datos de la página actual
        });
    },
    columns: columns
});

function formatDate(date) {
    // Extract date components
    var month = '' + (date.getMonth() + 1);
    var day = '' + date.getDate();
    var year = '' + date.getFullYear();
    var hours = '' + date.getHours();
    var minutes = '' + date.getMinutes();
    var seconds = '' + date.getSeconds();

    // Pad single digits with leading zeros
    if (month.length < 2) month = '0' + month;
    if (day.length < 2) day = '0' + day;
    if (hours.length < 2) hours = '0' + hours;
    if (minutes.length < 2) minutes = '0' + minutes;
    if (seconds.length < 2) seconds = '0' + seconds;

    // Return the formatted date string
    return [month, day, year.slice(2)].join('/') + ' ' + [hours, minutes, seconds].join(':');
}

function getData() {
    const services = ($('#select-services').val() ?? []).join('-') ?? "";
    const provinces = ($('#select-provinces').val() ?? []).join('-') ?? "";
    const date_range = $('#daterange').val();
    let dateinit = null;
    let dateend = null;
    if (date_range) {
        dateinit = new Date(date_range.split(' - ')[0]).toISOString();
        dateend = new Date(date_range.split(' - ')[1]).toISOString();

        console.log(dateinit, dateend)
    }

    if (!services) return;
    $.ajax({
        async: true,
        type: "Get",
        url: "/Marketing/GetData",
        data: {
            services: services,
            provinces: provinces,
            init: dateinit,
            end: dateend
        },
        success: function (response) {
            if (response.success) {
                datos = response.data;
                tabla.ajax.reload();
                tabla.search('').draw();

                $('#span-total').html(response.data.length);
                $('#span-price-sms').html(`$${(response.data.length * response.prices.sms).toFixed(2)}`);
                $('#span-price-mms').html(`$${(response.data.length * response.prices.mms).toFixed(2)}`);

            }
            else {
                toastr.error(response.message);
            }
        },
        error: function (e) {
            console.log(e);
            toastr.error("No se ha podido obtener la informacion");
        }
    });
}

function sendCampaing() {
    if (validateTextMessage() == true) {
        const message = $('#text-message').val();
        const phoneNumbers = datos.map(x => x.phone);
        const formData = new FormData();
        var files = $('#input-file-image')[0].files;
        for (var i = 0; i < files.length; i++) {
            formData.append('file', files[i]);
        }

        if (isMms) {
            if (files.length == 0) {
                toastr.error("Debe añadir un adjunto para enviar un mensaje multimedia");
                return;
            }
            if (files.length > 1) {
                toastr.error("El maximo de adjuntos a eviar es 1");
                return;
            }
        }

        if (files.length > 0 && !isMms) {
            toastr.error("No se puede enviar un mensaje multimedia sin marcar la casilla correspondiente");
            return;
        }

        if (!message) {
            toastr.error("El mensaje no puede estar vacio");
            return;
        }

        if (phoneNumbers.length == 0) {
            toastr.error("No hay numeros de telefono para enviar");
            return;
        }

        formData.append('phoneNumbers', phoneNumbers);
        formData.append('message', message);

        $.ajax({
            async: true,
            type: "Post",

            url: "/Marketing/SendCampaing",
            data: formData,
            processData: false,  // tell jQuery not to process the data
            contentType: false,  // tell jQuery not to set contentType
            beforeSend: function () {
                $('#btn-send').hide();
                $('#btn-loading').show();
                $('#div-message').block({ message: '<h1> Enviando...</h1>' });
            },
            success: function (response) {
                if (response.success) {
                    toastr.success("La campanna ha sido enviada");
                    getRegisters();
                    getData();
                }
                else {
                    toastr.error(response.message);
                }
                $('#btn-send').show();
                $('#btn-loading').hide();
                $('#div-message').unblock();
            },
            error: function () {
                //$('#btn-send').show();
                //$('#btn-loading').hide();
                //$('#div-message').unblock();
                toastr.info("La campanna se esta enviando. Esto puede tomar algo de tiempo!")
                // timeout de 2 seg y redireccionar a la pagina de inicio
                setTimeout(function () {
                    window.location.href = "/Home/Index";
                }, 2000);
            },

        })
    }
}

function getRegisters() {
// Función para obtener los registros
    $.ajax({
        async: true,
        type: "Get",
        url: "/Marketing/GetRegisters",
        data: {},
        success: function (response) {
            if (response.success) {
                registros = response.data;
                tablaRegistros.ajax.reload();
                tablaRegistros.search('').draw();
            }
            else {
                toastr.error(response.message);
            }
        },
        error: function (e) {
            console.log(e);
            toastr.error("No se ha podido obtener la informacion");
        }
    });
}

function validateTextMessage() {
    let isValid = true;
    const isMms = $('#check-mms').is(':checked');
    if (!isMms) {
        const message = $('#text-message').val();
        // validar que no contengan caracteres especiales
        const regex = /^[a-zA-Z0-9\s.,?!'":\n$]+$/;
        if (!regex.test(message)) {
            toastr.error("El mensaje sms no puede contener caracteres especiales ejemplo: tilde, ñ");
            isValid = false;
        }
        if(message.length > 160) {
            toastr.error("El mensaje sms no puede contener mas de 160 caracteres.");
            isValid = false;
        }
    }

    return isValid;
}