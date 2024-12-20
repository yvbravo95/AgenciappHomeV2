var notificationContainer = $('#notificationContainer'),
    cantNewNotif = $('[name="cantNotifications"]');
var items = [];

function appendNotifications(data) {
    notificationContainer.html("");
    for (var i = 0; i < data.length; i++) {
        items.push(data[i].id);
        var icon = "icon-plus-square";
        var color = "bg-cyan";
        const type = data[i].type;
        if (type == 0) { //Information
            icon = "icon-info";
            color = "bg-cyan";
        }
        else if (type == 1) { //Alert

        }
        else if (type == 2) { //Warning
            icon = "icon-alert-triangle";
            color = "bg-yellow";
        }
        else if (type == 3) {//Danger
            icon = "icon-minus-circle";
            color = "bg-red";
        }

        var item = '<a href="javascript:void(0)">' +
            '<div class="media">'+
            ' <div class="media-left align-self-center"><i class="feather ' + icon + ' icon-bg-circle ' + color + ' bg-darken-1"></i></div>'+
                                            '<div class="media-body">'+
            '<h6 class="media-heading red darken-1">' + data[i].title + '</h6>'+
            '<p class="notification-text font-small-3 text-muted">' + data[i].description + '</p><small>'+
            '<time class="media-meta text-muted" datetime="' + data[i].createdAt + '">' + data[i].getTimeRemaining + '</time></small>'+
                                            '</div>'+
                                        '</div >'+
                                    '</a >';
        notificationContainer.append(item);
    }

    cantNewNotif.html(data.length);
}

function getNewNotification() {
    $.ajax({
        async: true,
        type: "GET",
        url: "/Notification/GetNewNotification",
        beforeSend: function () {
            items = [];
        },
        success: function (response) {
            if (response.success) {
                appendNotifications(response.data);
            }
            else {
                console.log(response);
            }
            setTimeout(getNewNotification, 10000);
        }
    })
};

getNewNotification();

$('#readAllNotify').on('click', function () {
    var ids = items;
    $.ajax({
        async: true,
        type: "POST",
        url: "/Notification/ReadNotification",
        data: {
            ids: ids
        },
        beforeSend: function () {
            items = [];
            $.blockUI();
        },
        success: function (response) {

            if (response.success) {
                appendNotifications(response.data);
            }
            else {
                console.log(response);
            }
            setTimeout(getNewNotification, 10000);
            $.unblockUI();
        }
    })
})