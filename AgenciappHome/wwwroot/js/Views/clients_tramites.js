$(document).ready(function () {

    $('#parentModal').select2({
        containerCssClass: 'select-sm',
        dropdownParent: $('#modalAddTask')
    });
    $('#selectTemplate').select2({
        containerCssClass: 'select-sm',
        dropdownParent: $('#sendEmail')
    });

    $('#addTask').on('click', function () {
        $('#modalAddTask').modal('show');
    });
    $('#btnAddNota').on('click', function(){
        clientId = $(this).attr('data-id');
        $('#modalNota').modal('hide');
        var note = $('#nota').val();
        if(clientId == null || nota == null){
            toastr.error('No se ha podido crear la nota');
        }
        
        $.ajax({
            async: true,
            type: "Post",
            url: "/Clients/AddNote",
            data: {
                clientId: clientId,
                note: note
            },
            success: function(response){
                if(response.success){
                    location.href="/Clients/tramites/"+clientId+"?msg=La nota ha sido creada";
                }
                else{
                    toastr.error(response.msg);
                }
            },
            error: function(){
                toastr.error("No se ha podido crear la nota");
            }
        })
    })

    $('#btnSaveTask').on('click', function () {
        var task = new Object();
        task.ClientId = $('#ClientId').val();
        task.SubjectId = $('#SubjectId').val();
        task.EmployeeId = $('#EmployeeId').val();
        task.DueDate = $('#DueDate').val();
        task.Priority = $('#Priority').val();
        task.Nota = $('#Nota').val();

        if (validateTask(task)) {
            $.ajax({
                async: true,
                type: "POST",
                url: "/Tasks/Create/",
                data: {
                    model: task
                },
                beforeSend: function () {
                    $.blockUI();
                },
                success: function (response) {
                    if (response.success) {
                        $('#modalAddTask').modal('hide');
                        toastr.success(response.msg, "Success");
                    }
                    else {
                        toastr.error(response.msg, "Error")
                    }
                    $.unblockUI();
                },
                error: function (response) {
                    toastr.error("No se ha podido crear la tarea", "Error");
                    $.unblockUI();

                }
            })
        }
       
    });

    $('#checkTemplate').on('change', function () {
        var isChecked = $(this).is(':checked');
        if (isChecked) {
            $('#containerTemplates').show();
            $('#containerText').hide();
        }
        else {
            $('#containerTemplates').hide();
            $('#containerText').show();
        }
    })

    function validateTask(task) {
        var isValid = true;
        if (task.SubjectId == null) {
            isValid = false;
            toastr.warning("Debe seleccionar un asunto para la tarea", "Warning")
        }
        if (task.EmployeeId == null) {
            isValid = false;
            toastr.warning("Debe seleccionar un empleado para asignar la tarea", "Warning")
        }
        if (task.DueDate == null) {
            isValid = false;
            toastr.warning("Debe elegir una fecha de vencimiento para la tarea", "Warning")
        }
        if (task.Priority == null) {
            isValid = false;
            toastr.warning("Debe elegir una prioridad para la tarea", "Warning")
        }

        return isValid;
    }
})