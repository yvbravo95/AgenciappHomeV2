$(document).ready(function () {

    selectedIds = new Array;

    $(".order-select").on("change", function () {
        if ($(this)[0].checked) {
            selectedIds.push($(this).val());
        } else {
            selectedIds.splice($.inArray($(this).val(), selectedIds), 1);
        }

        if (selectedIds.length == 0) {
            $("#gen_report").addClass("hidden");
        } else {
            $("#gen_report").removeClass("hidden");
        }
    });

    $("#gen_report").click(function () {
        window.location = "/Orders/Report?" + $.param({ ids: selectedIds });
    });

    $('#searchNumber').on('change', function () {
        var search = $("#searchNumber");
        $.ajax({
            type: "POST",
            url: "/Orders/SearchNumber",
            data: JSON.stringify(search.val()),
            dataType: 'json',
            contentType: 'application/json',
            success: function (r) {
                alert(r + " record(s) inserted.");
            }

        });
    });

    $('#searchClient').on('change', function () {
        var search = $("#searchClient");
        $.ajax({
            type: "POST",
            url: "/Orders/SearchClient",
            data: JSON.stringify(search.val()),
            dataType: 'json',
            contentType: 'application/json',
            success: function (r) {
                alert(r + " record(s) inserted.");
            }

        });
    });

    $('#searchStatus').on('change', function () {
        var search = $("#searchStatus");
        $.ajax({
            type: "POST",
            url: "/Orders/SearchStatus",
            data: JSON.stringify(search.val()),
            dataType: 'json',
            contentType: 'application/json',
            success: function (r) {
                alert(r + " record(s) inserted.");
            }

        });
    });

    $("#searchBtn").on("change", function () {
        var searchVal = $("#searchBtn").val();

        $("#tableOrders tr").removeClass("hidden");

        var tBody = $("#tableOrders > tbody")[0];

        for (var i = 0; i < tBody.rows.length; i++) {
            var fila = tBody.rows[i];
            if (!$(fila.children[0]).html().includes(searchVal) && !$(fila.children[4]).html().includes(searchVal))
                $(fila).addClass("hidden");
        }
    });

});