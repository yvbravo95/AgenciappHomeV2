$(document).ready(function () {
    //code here
    function block() {
        $.blockUI({
            message: '<div class="ft-refresh-cw icon-spin font-medium-2"></div>',
            overlayCSS: {
                backgroundColor: '#FFF',
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


    $('[name="visible"]').on('change', function () {
        var idwholesaler = $(this).attr('data-id');
        var checked = $(this)[0].checked;
        $.ajax({
            type: "POST",
            url: "/Wholesalers/changeStatusAsync",
            data: {
                idwholesaler: idwholesaler,
                check: checked
            },
            async: false,
            beforeSend: function () {
                block()
            },
            success: function (data) {
                var aux = data.split('-');
                if (aux[0] == "success") {
                    toastr.success(aux[1]);
                }
                else {
                    toastr.error(aux[1]);
                }
                $.unblockUI();
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
                $.unblockUI();

            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
                $.unblockUI();

            }
        });
    });

    $('[name="default"]').on('change', function () {
        var idwholesaler = $(this).attr('data-id');
        var value = $(this)[0].checked;
        $.ajax({
            type: "POST",
            url: "/Wholesalers/SetIsDefault",
            data: {
                idWholesaler: idwholesaler,
                isDefault: value
            },
            async: false,
            beforeSend: function () {
                block()
            },
            success: function (data) {
                var aux = data.split('-');
                if (aux[0] == "success") {
                    toastr.success(aux[1]);
                }
                else {
                    toastr.error(aux[1]);
                }
                $.unblockUI();
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
                $.unblockUI();

            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
                $.unblockUI();

            }
        });
    });
});