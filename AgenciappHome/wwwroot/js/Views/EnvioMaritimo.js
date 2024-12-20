$(document).ready(function () {
    $('.dropdown-toggle').dropdown()

    //********** Preview Attachment ***************
    function filePreview(input, name) {
        if (input.files && input.files[0]) {
            for (var i = 0; i < input.files.length; i++) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    if (name == "otrosImg") {//Duplico el input
                        var fieldInput = $('input[name="otrosImg"]');
                        element = fieldInput.clone();
                        time = jQuery.now();
                        element.attr('data-element', '' + time);
                        element.attr('name', name + '_' + time);
                        element.attr('id', '' + time);
                        element.attr('data-aux', 'update');
                        element.appendTo('#input_attachments');
                        //$('#attachments + img').remove();
                        $('#attachments').prepend(
                            '<div style="width: auto;" data-aux="update" name="component_' + name + '" class="col-md-1">' +
                            '<div class="container-enlarge" style="width:82px;border-color:green;border-width:1px;border-style:dashed;padding-left:2px;">' +
                            '<button data-other="true" data-element = "' + time + '" name="remove_' + name + '" style="margin:0px;padding:0px;display:inline-block;float:right;margin-right:2px;" type="button" class="btn btn-icon btn-pure danger"><i style="font-size:15px;margin:0px;padding:0px;" class="fa fa-times"></i></button>' +
                            '<button  data-toggle="modal" data-target="#preview"  data-element = "' + time + '" name="view_' + name + '" style="margin:0px;padding:0px;display:inline-block;float:right;margin-right:2px;" type="button" class="btn btn-icon btn-pure blue"><i style="font-size:15px;margin:0px;padding:0px;" class="fa fa-eye"></i></button>' +
                            '<img style="margin:5px; margin:0px;padding:0px;z-index:0;" name="' + name + '" src="' + e.target.result + '" width="75" height="75" />' +
                            '<label style="font-size:12px;"><b>Otro</b></label>' +
                            '</div>' +
                            '</div>'
                        );
                        $('input[name="otrosImg"]').val("");
                    }
                    else {
                        if (name == "IdImg") {
                            $('#attachments').prepend(
                                '<div style="width: auto;" data-aux="update" name="component_' + name + '" class="col-md-1">' +
                                '<div class="container-enlarge" style="width:82px;border-color:green;border-width:1px;border-style:dashed;padding-left:2px;">' +
                                '<button name="remove_' + name + '" style="margin:0px;padding:0px;display:inline-block;float:right;margin-right:2px;" type="button" class="btn btn-icon btn-pure danger"><i style="font-size:15px;margin:0px;padding:0px;" class="fa fa-times"></i></button>' +
                                '<button  data-toggle="modal" data-target="#preview" name="view_' + name + '" style="margin:0px;padding:0px;display:inline-block;float:right;margin-right:2px;" type="button" class="btn btn-icon btn-pure blue"><i style="font-size:15px;margin:0px;padding:0px;" class="fa fa-eye"></i></button>' +
                                '<img style="margin:5px; margin:0px;padding:0px;" name="' + name + '" src="' + e.target.result + '" width="75" height="75" />' +
                                '<label style="font-size:12px;"><b>ID</b></label>' +
                                '</div>' +
                                '</div>'
                            );
                            $('#atachmentId').prepend(
                                '<div style="width: auto;" data-aux="update" name="component_' + name + '" class="col-md-1">' +
                                '<div class="container-enlarge" style="width:255px;border-color:green;border-width:1px;border-style:dashed;padding-left:2px;">' +
                                '<img style="margin:0px;padding:0px;" name="' + name + '" src="' + e.target.result + '" width="250" height="250" />' +
                                '<label id="labelID" style="font-size:12px;"><b>No. ID: </b>' + $('#TextoIDImg').val() + '</label>' +
                                '</div>' +
                                '</div>'
                            );

                        }
                        if (name == "pasaporteImg") {
                            $('#attachments').prepend(
                                '<div style="width: auto;" data-aux="update" name="component_' + name + '" class="col-md-1">' +
                                '<div class="container-enlarge" style="width:82px;border-color:green;border-width:1px;border-style:dashed;padding-left:2px;">' +
                                '<button name="remove_' + name + '" style="margin:0px;padding:0px;display:inline-block;float:right;margin-right:2px;" type="button" class="btn btn-icon btn-pure danger"><i style="font-size:15px;margin:0px;padding:0px;" class="fa fa-times"></i></button>' +
                                '<button  data-toggle="modal" data-target="#preview" name="view_' + name + '" style="margin:0px;padding:0px;display:inline-block;float:right;margin-right:2px;" type="button" class="btn btn-icon btn-pure blue"><i style="font-size:15px;margin:0px;padding:0px;" class="fa fa-eye"></i></button>' +
                                '<img style="margin:5px; margin:0px;padding:0px;" name="' + name + '" src="' + e.target.result + '" width="75" height="75" />' +
                                '<label style="font-size:12px;"><b>Pasaporte</b></label>' +
                                '</div>' +
                                '</div>'
                            );
                        }
                        if (name == "residenciaImg") {
                            $('#attachments').prepend(
                                '<div style="width: auto;" data-aux="update" name="component_' + name + '" class="col-md-1">' +
                                '<div class="container-enlarge" style="width:82px;border-color:green;border-width:1px;border-style:dashed;padding-left:2px;">' +
                                '<button name="remove_' + name + '" style="margin:0px;padding:0px;display:inline-block;float:right;margin-right:2px;" type="button" class="btn btn-icon btn-pure danger"><i style="font-size:15px;margin:0px;padding:0px;" class="fa fa-times"></i></button>' +
                                '<button  data-toggle="modal" data-target="#preview" name="view_' + name + '" style="margin:0px;padding:0px;display:inline-block;float:right;margin-right:2px;" type="button" class="btn btn-icon btn-pure blue"><i style="font-size:15px;margin:0px;padding:0px;" class="fa fa-eye"></i></button>' +
                                '<img style="margin:5px; margin:0px;padding:0px;" name="' + name + '" src="' + e.target.result + '" width="75" height="75" />' +
                                '<label style="font-size:12px;"><b>Residencia</b></label>' +
                                '</div>' +
                                '</div>'
                            );
                        }
                        if (name == "document") {
                            $('#attachments').prepend(
                                '<div style="width: auto;margin-botton:0px;" data-aux="update" name="component_' + name + '" class="col-md-1">' +
                                '<div style="width:82px;border-color:green;border-width:1px;border-style:dashed;padding-left:2px;">' +
                                '<button name="remove_' + name + '" style="margin:0px;padding:0px;display:inline-block;float:right;margin-right:2px;" type="button" class="btn btn-icon btn-pure danger"><i style="font-size:15px;margin:0px;padding:0px;" class="fa fa-times"></i></button>' +
                                '<i style="font-size:70px;margin:5px; margin-left:8px;padding:0px;margin-bottom:0px;" name="' + name + '" class="fa fa-file-text-o" width="75" height="75" />' +
                                '<label style="font-size:12px;"><b>Document</b></label>' +
                                '</div>' +
                                '</div>'
                            );
                        }

                    }
                }
                reader.readAsDataURL(input.files[i]);
            }
            if (name != "otrosImg") {
                $('label[name="' + name + '"]').hide();
            }
        }
    }

    $('input[type="file"]').on('change', function () { //Si un campo file obtiene un valor entonces se hace un preview
        filePreview(this, $(this).attr("name"));
        if ($(this).attr("name") == "IdImg") {
            $('#IdModal').modal({ show: true });
        }
        else if ($(this).attr("name") == "pasaporteImg") {
            $('#PasaporteModal').modal({ show: true });
        }
        else {


        }

    });

    $('#confirmId').on('click', function () {
        $("#IdModal").modal('hide')
    });
    $('#confirmPasaporte').on('click', function () {
        $("#PasaporteModal").modal('hide')
    });


    $(document).on('click', 'button[name="remove_IdImg"]', function () {
        var elem = $(this);
        swal({
            title: "Está seguro que desea eliminar el fichero?",
            text: "Esta acción no podrá desacerse!",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DA4453",
            confirmButtonText: "Sí, eliminar!",
            cancelButtonText: "No, cancelar!",
            closeOnConfirm: false,
            closeOnCancel: false
        }, function (isConfirm) {
            if (isConfirm) {
                if (elem.attr("id") != null) {
                    var key = elem.attr("id");
                    removeImage(key);

                }
                $('div[name="component_IdImg"]').remove();
                $('label[name="IdImg"]').show();
                $('input[name="IdImg"]').val("");
                swal("Eliminado!", "El adjunto ha sido eliminado.", "success");
            } else {
                swal("Cancelado! ", "La eliminación ha sido cancelada :)", "error");
            }
        });
    });
    $(document).on('click', 'button[name="remove_pasaporteImg"]', function () {
        var elem = $(this);
        swal({
            title: "Está seguro que desea eliminar el fichero?",
            text: "Esta acción no podrá desacerse!",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DA4453",
            confirmButtonText: "Sí, eliminar!",
            cancelButtonText: "No, cancelar!",
            closeOnConfirm: false,
            closeOnCancel: false
        }, function (isConfirm) {
            if (isConfirm) {
                if (elem.attr("id") != null) {
                    var key = elem.attr("id");
                    removeImage(key);

                }
                $('div[name="component_pasaporteImg"]').remove();
                $('label[name="pasaporteImg"]').show();
                $('input[name="pasaporteImg"]').val("");
                swal("Eliminado!", "El adjunto ha sido eliminado.", "success");
            } else {
                swal("Cancelado! ", "La eliminación ha sido cancelada :)", "error");
            }
        });



    });
    $(document).on('click', 'button[name="remove_residenciaImg"]', function () {
        var elem = $(this);
        swal({
            title: "Está seguro que desea eliminar el fichero?",
            text: "Esta acción no podrá desacerse!",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DA4453",
            confirmButtonText: "Sí, eliminar!",
            cancelButtonText: "No, cancelar!",
            closeOnConfirm: false,
            closeOnCancel: false
        }, function (isConfirm) {
            if (isConfirm) {
                if (elem.attr("id") != null) {
                    var key = elem.attr("id");
                    var response = removeImage(key);
                }
                $('div[name="component_residenciaImg"]').remove();
                $('label[name="residenciaImg"]').show();
                $('input[name="residenciaImg"]').val("");
                swal("Eliminado!", "El adjunto ha sido eliminado.", "success");
            } else {
                swal("Cancelado! ", "La eliminación ha sido cancelada :)", "error");
            }
        });


    });
    $(document).on('click', 'button[name="remove_document"]', function () {
        var elem = $(this);
        swal({
            title: "Está seguro que desea eliminar el fichero?",
            text: "Esta acción no podrá desacerse!",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DA4453",
            confirmButtonText: "Sí, eliminar!",
            cancelButtonText: "No, cancelar!",
            closeOnConfirm: false,
            closeOnCancel: false
        }, function (isConfirm) {
            if (isConfirm) {
                if (elem.attr("id") != null) {
                    var key = elem.attr("id");
                    var response = removeImageTicket(key);
                }
                $('div[name="component_document"]').remove();
                $('label[name="document"]').show();
                $('input[name="document"]').val("");
                swal("Eliminado!", "El adjunto ha sido eliminado.", "success");
            } else {
                swal("Cancelado! ", "La eliminación ha sido cancelada :)", "error");
            }
        });


    });
    $(document).on('click', 'button[name="remove_otrosImg"]', function () {
        var elem = $(this);
        swal({
            title: "Está seguro que desea eliminar el fichero?",
            text: "Esta acción no podrá desacerse!",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DA4453",
            confirmButtonText: "Sí, eliminar!",
            cancelButtonText: "No, cancelar!",
            closeOnConfirm: false,
            closeOnCancel: false
        }, function (isConfirm) {
            if (isConfirm) {
                if (elem.attr("id") != null) {
                    var key = elem.attr("id");
                    removeImageTicket(key);
                }
                var name = elem.attr('data-element');
                elem.parent('div').parent('div').remove();
                $('input[data-element="' + name + '"]').remove();
                swal("Eliminado!", "El adjunto ha sido eliminado.", "success");
            } else {
                swal("Cancelado! ", "La eliminación ha sido cancelada :)", "error");
            }
        });


    });

    //Eliminar imagenes guardadas en la bd
    function removeImage(key) {
        $.ajax({
            async: true,
            type: "POST",
            dataType: "json",
            contentType: "application/x-www-form-urlencoded",
            url: "/ticket/RemoveImage",
            data: {
                id: key,
            },
            beforeSend: function () {
            },
            success: function (response) {
                if (response != "success") {
                    alert(response);
                    return false;
                }
                else {
                    return true;

                }


            },
            error: function () {
                alert("Error al intentar eliminar un adjunto")
            },
            timeout: 4000,
        });

    }

    function removeImageTicket(key) {
        $.ajax({
            async: true,
            type: "POST",
            dataType: "json",
            contentType: "application/x-www-form-urlencoded",
            url: "/ticket/RemoveImageTicket",
            data: {
                id: key,
            },
            beforeSend: function () {
            },
            success: function (response) {
                if (response != "success") {
                    alert(response);
                    return false;
                }
                else {
                    return true;

                }


            },
            error: function () {
                alert("Error al intentar eliminar un adjunto")
            },
            timeout: 4000,
        });

    }

    // Visualizar imagenes
    $(document).on('click', 'button[name="view_residenciaImg"]', function () {
        var elem = $(this);
        var img = elem.parent().find('img');
        $('#imgModal').attr("src", img.attr("src"));
        $('#labelIDModal').html("<b>Residencia</b>");
    });
    $(document).on('click', 'button[name="view_IdImg"]', function () {
        $('#labelIDModal').html($('#labelID').html());

        var elem = $(this);
        var img = elem.parent().find('img');
        $('#imgModal').attr("src", img.attr("src"));

    });
    $(document).on('click', 'button[name="view_pasaporteImg"]', function () {
        var elem = $(this);
        var img = elem.parent().find('img');
        $('#imgModal').attr("src", img.attr("src"));
        $('#labelIDModal').html("<b>Fecha Pasaporte: </b> " + $('#FechaPasaporteImg').val());

    });
    $(document).on('click', 'button[name="view_otrosImg"]', function () {
        var elem = $(this);
        var img = elem.parent().find('img');
        $('#imgModal').attr("src", img.attr("src"));
        $('#labelIDModal').html("<b>Otro</b>");

    });

    //*********** Get Document of Client Imagen ********************
    $("[name = 'selectClient']").change(function () {
        var $this = $(this);
        var clientId = $this.val();
        if (clientId != "") {
            $.ajax({
                async: true,
                type: "POST",
                dataType: "json",
                contentType: "application/x-www-form-urlencoded",
                url: "/ticket/LoadImages",
                data: {
                    clientId: clientId,
                },
                beforeSend: function () {
                    $('[data-aux="update"]').remove();
                    $('file').val("");
                    $('[name="TextoIDImg"]').val("");
                    $('[name="FechaPasaporteImg"]').val("");
                    $('label[name="IdImg"]').show();
                    $('label[name="pasaporteImg"]').show();
                    $('label[name="residenciaImg"]').show();
                    $('label[name="document"]').show();
                },
                success: function (response) {
                    if (response.length > 0)
                        AddAttachment(response);
                },
                error: function () {
                    alert("Error")
                },
                timeout: 4000,
            });
        }
    })

    //*************
    function AddAttachment(response) {
        for (var i = 0; i < response.length; i++) {
            var id = response[i]["ID"];
            var type = response[i]["type"];
            if (type == "IdImg" || type == "pasaporteImg" || type == "residenciaImg") {
                if (type == "IdImg") {
                    $('#attachments').prepend(
                        '<div style="width: auto;" data-aux="update" name="component_' + type + '" class="col-md-1">' +
                        '<div class="container-enlarge" style="width:82px;border-color:green;border-width:1px;border-style:dashed;padding-left:2px;">' +
                        '<button id="' + id + '" name="remove_' + type + '" style="margin:0px;padding:0px;display:inline-block;float:right;margin-right:2px;" type="button" class="btn btn-icon btn-pure danger"><i style="font-size:15px;margin:0px;padding:0px;" class="fa fa-times"></i></button>' +
                        '<button  data-toggle="modal" data-target="#preview" data-id="' + id + '" name="view_' + type + '" style="margin:0px;padding:0px;display:inline-block;float:right;margin-right:2px;" type="button" class="btn btn-icon btn-pure blue"><i style="font-size:15px;margin:0px;padding:0px;" class="fa fa-eye"></i></button>' +
                        '<img style="margin:5px; margin:0px;padding:0px;" name="' + type + '" src="/Ticket/getImage/' + id + '" width="75" height="75" />' +
                        '<label  style="font-size:12px;"><b>ID</b></label>' +
                        '</div>' +
                        '</div>'
                    );
                    $('#atachmentId').prepend(
                        '<div style="width: auto;" data-aux="update" name="component_' + type + '" class="col-md-1">' +
                        '<div class="container-enlarge" style="width:255px;border-color:green;border-width:1px;border-style:dashed;padding-left:2px;">' +
                        '<img style="margin:5px; margin:0px;padding:0px;" name="' + type + '" src="/Ticket/getImage/' + id + '" width="250" height="250" />' +
                        '<label id="labelID" style="font-size:12px;"><b>No. ID:</b> ' + response[i]["description"] + '</label>' +
                        '</div>' +
                        '</div>'
                    );

                }
                if (type == "pasaporteImg") {
                    $('#attachments').prepend(
                        '<div style="width: auto;" data-aux="update" name="component_' + type + '" class="col-md-1">' +
                        '<div class="container-enlarge" style="width:82px;border-color:green;border-width:1px;border-style:dashed;padding-left:2px;">' +
                        '<button id="' + id + '" name="remove_' + type + '" style="margin:0px;padding:0px;display:inline-block;float:right;margin-right:2px;" type="button" class="btn btn-icon btn-pure danger"><i style="font-size:15px;margin:0px;padding:0px;" class="fa fa-times"></i></button>' +
                        '<button  data-toggle="modal" data-target="#preview" data-id="' + id + '" name="view_' + type + '" style="margin:0px;padding:0px;display:inline-block;float:right;margin-right:2px;" type="button" class="btn btn-icon btn-pure blue"><i style="font-size:15px;margin:0px;padding:0px;" class="fa fa-eye"></i></button>' +
                        '<img style="margin:5px; margin:0px;padding:0px;" name="' + type + '" src="/Ticket/getImage/' + id + '" width="75" height="75" />' +
                        '<label style="font-size:12px;"><b>Pasaporte</b></label>' +
                        '</div>' +
                        '</div>'
                    );
                    var date = new Date(response[i]["ExpirationDate"]);
                    var options = { year: "numeric", month: "long", day: "numeric" };
                    $('#FechaPasaporteImg').val(date.toLocaleDateString("en-En", options));

                }



                $('label[name="' + type + '"]').hide();
            }
            /*else {
                var fieldInput = $('input[name="otrosImg"]');
                element = fieldInput.clone();
                time = jQuery.now();
                element.attr('data-element', '' + time);
                element.attr('name', type + '_' + time);
                element.attr('id', '' + time);
                element.attr('data-aux', 'update');
                element.appendTo('#input_attachments');
                $('#attachments').prepend(
                    '<div style="width: auto;" data-aux="update" name="component_' + type + '" class="col-md-1">' +
                    '<div style="width:82px;border-color:green;border-width:1px;border-style:dashed;padding-left:2px;">' +
                    '<button data-other="true" id="' + id + '" data-element = "' + time + '" name="remove_' + type + '" style="margin:0px;padding:0px;margin-left:60px" type="button" class="btn btn-icon btn-pure danger"><i style="font-size:15px;margin:0px;padding:0px;" class="fa fa-times"></i></button>' +
                    '<img style="margin:5px; margin:0px;padding:0px;float:left;" name="' + type + '" src="/Ticket/getImage/' + id + '"  width="75" height="75" />' +
                    '<label style="font-size:12px;"><b>Otro</b></label>' +
                    '</div>' +
                    '</div>'
                );
            }*/
        }
    }


    $('#TextoIDImg').on('change', function () {
        $("#labelID").html("<b>No. ID:</b> " + $('#TextoIDImg').val());
    });
});