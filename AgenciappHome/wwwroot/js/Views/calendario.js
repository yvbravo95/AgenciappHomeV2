$(document).on('ready', () => {
    $(".date").datetimepicker({
        format: 'MM/DD/YYYY',
        widgetPositioning: {
            horizontal: 'auto',
            vertical: 'bottom'
        },
    })

    $(".time").datetimepicker({
        format: 'HH:mm',
        widgetPositioning: {
            horizontal: 'auto',
            vertical: 'bottom'
        },
    })

    $(".date-input").mask("99/99/9999")
    $(".time-input").mask("99:99")

    $("#eventClient").select2({
        width: "100%",
        placeholder: "Cliente",
        text: " ",
        ajax: {
            type: 'POST',
            url: '/Clients/findClient',
            data: function (params) {
                var query = {
                    search: params.term,
                }
                return query;
            },
            processResults: function (data) {
                return {
                    results: $.map(data, function (obj) {

                        return { id: obj.clientId, text: obj.fullData, conflictivo: obj.conflictivo };
                    })
                };
            }
        }
    });

    $("#eventUser").select2({
        width: "100%",
        placeholder: "Usuario"
    })

})

document.addEventListener('DOMContentLoaded', function () {
    var calendarEl = document.getElementById('calendar');

    var calendar = new FullCalendar.Calendar(calendarEl, {
        locale: 'es-us',
        displayEventTime: false,
        initialDate: new Date(),
        headerToolbar: {
            left: 'prev,next today addEventButton',
            center: 'title',
            right: 'dayGridMonth,listYear'
        },
        customButtons: {
            addEventButton: {
                text: '+',
                click: () => $("#addEventModal").modal("show")
            }
        },
        eventClick: function (info) {
            var eventObj = info.event._def.extendedProps;
            $("#detailUser").html(eventObj.user);
            $("#detailClient").html(eventObj.client);
            $("#detailTitle").html(eventObj.title);
            $("#detailFecha").html(eventObj.fecha);
            $("#detailDescrition").html(eventObj.description);

            $("#detailsEventModal").modal("show");
        },
        loading: function (bool) {
            var block_ele = $("#calendario");
            if (bool) {
                $(block_ele).block({
                    message: '<span class="semibold"> Loading...</span>',
                    overlayCSS: {
                        backgroundColor: '#fff',
                        opacity: 0.8,
                        cursor: 'wait'
                    },
                    css: {
                        border: 0,
                        padding: 0,
                        backgroundColor: 'transparent'
                    }
                });
            }
            else {
                $(block_ele).unblock()
            }
        },
        events: "/Clients/GetEvents"

    });

    var addEvent = () => {
        $.ajax({
            method: "POST",
            url: "/Clients/AddEvents",
            data: {
                title: $("#eventTitle").val(),
                description: $("#eventDescription").val(),
                date: $("#eventStartDate").val(),
                time: $("#eventStartTime").val(),
                clientId: $("#eventClient").val(),
                userId: $("#eventUser").val()
            },
            success: (result) => {
                if (result) {
                    calendar.addEvent(result);
                }
                $("#addEventModal").modal("hide")
            },
            error: () => {
                showErrorMessage("ERROR", "No se pudo crear el evento");
            }
        })
    }

    $("#ConfirmAddEvent").on("click", () => {
        if ($("#eventTitle").val() && $("#eventStartDate").val() && $("#eventClient").val()) {
            addEvent();
        }
        else {
            showWarningMessage("Atención", "El evento no es válido");
        }
    })

    calendar.render();
});