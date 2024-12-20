$(document).ready(function () {
    var auxSubmit = false;
    function verifyUser(e) {
        if (!auxSubmit) {
            e.preventDefault();
            var code = $('#code').val();
            $.ajax({
                type: "POST",
                url: "/Ticket/VerifyUserCode",
                data: {
                    code: code
                },
                async: true,
                beforeSend: function () {
                    //Bloqueo la pantalla del usuario
                    $.blockUI({
                        message: '<div class="ft-refresh-cw icon-spin font-medium-2"></div>',
                        timeout: 60000,
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
                },
                success: function (response) {
                    if (response.success) {
                        successtrue = true;
                        if (response.verify) {
                            auxSubmit = true;
                            $('#form').submit();
                        }
                        else {
                            swal({
                                title: "Está usted seguro?",
                                text: response.msg,
                                type: "warning",
                                showCancelButton: true,
                                confirmButtonColor: "#d43f3a",
                                confirmButtonText: "Sí, Crear!",
                                cancelButtonText: "No, Cancelar!",
                                closeOnConfirm: false,
                                closeOnCancel: false
                            }, function (isConfirm) {
                                if (isConfirm) {
                                    auxSubmit = true;
                                    $('#form').submit();
                                } else {
                                    location.href = "/Ticket/Index";
                                }
                            });
                        }
                    }
                    else {
                        toastr.error(response.msg);
                        $.unblockUI();
                    }
                },
                failure: function (response) {
                    $.unblockUI();
                    showErrorMessage("ERROR", response.responseText);
                },
                error: function (response) {
                    $.unblockUI();
                    showErrorMessage("ERROR", response.responseText);
                }
            });
        }
        
    }

    $('#form').on('submit', verifyUser);
});