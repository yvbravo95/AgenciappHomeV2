$(document).ready(function () {
    var checked = null;
    var date = new Date();
    $('.btnModalZelle').on('click', function () {
        //Abrir Modal Zelle
        $('#zelleModal').modal('show');
        if($(this).data('date'))
        {
            date = new Date($(this).data('date'));
        }
    });
    function getZelle() {
        $.ajax({
            async: true,
            type: "POST",
            data: {
                date: date.toISOString()
            },
            url: "/ZellePayment/VerifyNotAssociated",
            success: function (response) {
                if (response.success) {
                    var data = response.data;
                    for (var i = 0; i < data.length; i++) {
                        var zelle = data[i];

                        if ($('#tblZelle > tbody > tr[data-id="'+zelle.zellItemId+'"]').length == 0) {
                            var row = '<tr data-id="' + zelle.zellItemId + '" >' +
                                '<td style="padding:5px;"> <input type="checkbox" name="checkzelle" data-code="' + zelle.code +'" /></td>' +
                                '<td>' + zelle.date + '</td>' +
                                '<td>' + zelle.code + '</td>' +
                                '<td>' + zelle.client + '</td>' +
                                '<td>' + zelle.amount + '</td>' +
                                '</tr >';
                            $('#tblZelle > tbody').append(row);
                        }
                    }
                }
                else {
                    toastr.error(response.msg);
                }
            },
            error: function (response) {

            }
        });
    }
    setInterval(getZelle, 5000);

    $('#btnConfirmZelle').click(function () {
        $('.inputZelle').val(checked)
        $('.inputZelle').prop('readonly', true)
    });

    $('#btnCancellZelle').click(function () {
        $('.inputZelle').val("")
        $('.inputZelle').prop('readonly', false)
        $('[name="checkzelle"]').prop('checked', false);
        checked = null;
    });

    $('.tipoPago').on('change', function () {
        var id = $(this).val();
        if (id == "Zelle" || id == "709cb429-cc75-4722-a8c9-616b07545738") {
            $('#btnModalZelle').show();
        }
        else {
            $('#btnModalZelle').hide();
        }
    })

    $(document).on('click', '[name="checkzelle"]', function () {
        $('[name="checkzelle"]').prop('checked', false);
        $(this).prop('checked', true);
        checked = $(this).attr('data-code');
    })
})