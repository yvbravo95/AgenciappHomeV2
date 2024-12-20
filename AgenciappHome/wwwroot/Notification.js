var notificationContainer = $('#notificationContainer'),
    cantNewNotif = $('[name="cantNotifications"]');
var items = [];

function appendNotifications(data) {
    notificationContainer.html("");
    for (var i = 0; i < data.length; i++) {
        items.push(data[i].id);
        var icon = "ft-plus-square";
        var color = "bg-cyan";
        const type = data[i].type;
        if (type == 0) { //Information
            icon = "ft-info";
            color = "bg-cyan";
        }
        else if (type == 1) { //Alert

        }
        else if (type == 2) { //Warning
            icon = "ft-alert-triangle";
            color = "bg-yellow";
        }
        else if (type == 3) {//Danger
            icon = "ft-minus-circle";
            color = "bg-red";
        }
        var item = '<a href="javascript:void(0)" class="list-group-item">' +
            '<div class="media">' +
            '<div class="media-left valign-middle"><i class="' + icon + ' icon-bg-circle ' + color + '"></i></div>' +
            '<div class="media-body">' +
            '<h6 class="media-heading">' + data[i].title + '</h6>' +
            '<p class="notification-text font-small-3 text-muted">' + data[i].description + '</p><small>' +
            '<time datetime="' + data[i].createdAt + '" class="media-meta text-muted">' + data[i].getTimeRemaining + '</time>' +
            '</small>' +
            '</div>' +
            '</div >' +
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