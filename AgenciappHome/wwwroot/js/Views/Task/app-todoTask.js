/*=========================================================================================
    File Name: app-todo.js
    Description: app-todo
    ----------------------------------------------------------------------------------------
    Item Name: Modern Admin - Clean Bootstrap 4 Dashboard HTML Template
    Author: PIXINVENT
    Author URL: http://www.themeforest.net/user/pixinvent
==========================================================================================*/

// Todo App variables
var todoNewTasksidebar = $(".todo-new-task-sidebar"),
  appContentOverlay = $(".app-content-overlay"),
  sideBarLeft = $(".sidebar-left"),
  todoTaskListWrapper = $(".todo-task-list-wrapper"),
  todoItem = $(".todo-item"),
  updateTodo = $(".update-todo"),
  addTodo = $(".add-todo"),
  markCompleteBtn = $(".mark-complete-btn"),
  newTaskTitle = $(".new-task-title"),
  taskTitle = $(".task-title"),
  noResults = $(".no-results"),
  assignedAvatarContent = $(".assigned .avatar .avatar-content"),
  todoAppMenu = $(".todo-app-menu"),
  taskNumber = $(".taskNumber");

//FormElements
var subject = $('#SubjectId'),
    employee = $('#EmployeeId'),
    dueDate = $('#DueDate'),
    state = $('#StatusForm'),
    priority = $('#PriorityForm'),
    client = $('#ClientId'),
    nota = $('#Nota');


// badge colors object define here for badge color
var badgeColors = {
  "Frontend": "badge-primary",
  "Backend": "badge-success",
  "Issue": "badge-danger",
  "Design": "badge-warning",
  "Wireframe": "badge-info",
}


$(function () {
  "use strict";

  // dragable list
  dragula([document.getElementById("todo-task-list-drag")], {
    moves: function (el, container, handle) {
      return handle.classList.contains("handle");
    }
  });

  // select client
    $(client).select2({
    dropdownAutoWidth: true,
        width: '100%',
        placeholder: "Seleccione un Cliente",
        val: null,
        ajax: {
            type: 'POST',
            url: '/Clients/findClient',
            data: function (params) {
                var query = {
                    search: params.term,
                }

                // Query parameters will be ?search=[term]&type=public
                return query;
            },
            processResults: function (data) {

                // Transforms the top-level key of the response object from 'items' to 'results'
                return {
                    results: $.map(data, function (obj) {

                        return { id: obj.clientId, text: obj.fullData, conflictivo: obj.conflictivo };
                    })
                };
            }
        }
    });

    // select subject
    $('.parentModal').select2({
    dropdownAutoWidth: true,
    width: '100%'
    });

    
    $('#EmployeeId').select2({
    dropdownAutoWidth: true,
        width: '100%',
        placeholder: "Empleado"
    });
    $('#PriorityForm').select2({
    dropdownAutoWidth: true,
        width: '100%',
        placeholder: "Prioridad"
  });


  //perfect scrollbar
  if (!$.app.menu.is_touch_device()) {

    // Sidebar scrollbar
    if ($('.todo-application .sidebar-menu-list').length > 0) {
      new PerfectScrollbar('.sidebar-menu-list', {
        theme: "dark",
        wheelPropagation: false
      });
    }

    //  New task scrollbar
    if (todoNewTasksidebar.length > 0) {
      new PerfectScrollbar('.todo-new-task-sidebar', {
        theme: "dark",
        wheelPropagation: false
      });
    }

    // Task list scrollbar
    if ($('.todo-application .todo-task-list').length > 0) {
      new PerfectScrollbar('.todo-task-list', {
        theme: "dark",
        wheelPropagation: false
      });
    }
  } else {
    $('.sidebar-menu-list').css('overflow', 'scroll');
    $('.todo-new-task-sidebar').css('overflow', 'scroll');
    $('.todo-task-list').css('overflow', 'scroll');
  }

  // New compose message compose field
  var composeEditor = new Quill('.snow-container .compose-editor', {
    modules: {
      toolbar: '.compose-quill-toolbar'
    },
    placeholder: 'Add Description..... ',
    theme: 'snow'
  });

  //Assigner Comment Quill editor
  var commentEditor = new Quill('.snow-container .comment-editor', {
    modules: {
      toolbar: '.comment-quill-toolbar'
    },
    placeholder: 'Write a Comment...',
    theme: 'snow'
  });

  // **************Sidebar Left**************//
  // -----------------------------------------

  // Main menu toggle should hide app menu
  $('.menu-toggle').on('click', function () {
    sideBarLeft.removeClass('show');
    appContentOverlay.removeClass('show');
    todoNewTasksidebar.removeClass('show');
  });

  //on click of app overlay removeclass show from sidebar left and overlay
  appContentOverlay.on('click', function () {
    sideBarLeft.removeClass('show');
    appContentOverlay.removeClass('show');
  });

  // Add class active on click of sidebar menu's filters
  todoAppMenu.find(".list-group a").on('click', function () {
    var $this = $(this);
    todoAppMenu.find(".active").removeClass('active');
    $this.addClass("active")
  });

  //On compose btn click of compose mail visible and sidebar left hide
  $('.add-task-btn').on('click', function () {
    //show class add on new task sidebar,overlay
    todoNewTasksidebar.addClass('show');
    appContentOverlay.addClass('show');
    sideBarLeft.removeClass('show');
    //d-none add on avatar and remove from avatar-content
    assignedAvatarContent.removeClass("d-none");
    //select2 value null assign
    //update button has add class d-none remove from add TODO
    updateTodo.addClass("d-none");
    addTodo.removeClass("d-none");
    //mark complete btn should hide & new task title will visible
    markCompleteBtn.addClass("d-none");
    taskNumber.addClass("d-none");
    newTaskTitle.removeClass("d-none");
    //Input field Value empty
      taskTitle.val("");
      $('#containerStatus').hide();
      $('#containerClient').attr('class', 'col-md-12');
  });

  // On sidebar close click hide sidebarleft and overlay
  $(".todo-application .sidebar-close-icon").on('click', function () {
    sideBarLeft.removeClass('show');
    appContentOverlay.removeClass('show');
  });

  // **************New Task sidebar**************//
  // ---------------------------------------------

    var validateTask = function (task, method) {
        var isValid = true;
        if (task.SubjectId == null || task.SubjectId == "") {
            isValid = false;
            toastr.warning("Debe seleccionar un asunto para la tarea", "Warning")
        }
        if (task.EmployeeId == null || task.EmployeeId=="") {
            isValid = false;
            toastr.warning("Debe seleccionar un empleado para asignar la tarea", "Warning")
        }
        if (task.DueDate == null) {
            isValid = false;
            toastr.warning("Debe elegir una fecha de vencimiento para la tarea", "Warning")
        }
        if (task.Status == "undefined" && method == "Create") {
            isValid = false;
            toastr.warning("Debe elegir un estado para la tarea", "Warning")
        }
        if (task.Priority == null || task.Priority == "") {
            isValid = false;
            toastr.warning("Debe elegir una prioridad para la tarea", "Warning")
        }
        if (task.ClientId == null) {
            isValid = false;
            toastr.warning("Debe elegir una cliente para la tarea", "Warning")
        }

        return isValid;
    }

  // add new task
    addTodo.on("click", function () {

        //Save Task
        var task = new Object();
        task.ClientId = $(client).val();
        task.SubjectId = $(subject).val();
        task.EmployeeId = $(employee).val();
        task.DueDate = $(dueDate).val();
        task.Priority = $(priority).val();
        task.Nota = $(nota).val();

        if (validateTask(task, "Create")) {
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
                        toastr.success(response.msg, "Success");

                        var component =  // append a new task in task list
                            '<li class="todo-item" data-id="' + response.data.id + '">' +
                            '<div class="todo-title-wrapper d-flex justify-content-between align-items-center">' +
                            '<div class="todo-title-area d-flex">' +
                            '<i class="feather icon-more-vertical handle"></i>' +
                            '<div class="custom-control custom-checkbox">' +
                            '<input type="checkbox" class="custom-control-input" data-id="' + response.data.id + '" id="checkbox_' + response.data.id + '">' +
                            '<label class="custom-control-label" for="checkbox_' + response.data.id + '"></label>' + '</div>' +
                            '<p class="todo-title mx-50 m-0 truncate" name="subjectItem">' + response.data.client_FullName + " " + response.data.client_PhoneNumber + " - " + response.data.subject + '</p>' +
                            '</div>' +
                            '<div class="todo-item-action d-flex align-items-center">' +
                            '<div class="todo-badge-wrapper d-flex">';
                        if (response.data.isFinished) {
                            component += '<span class="badge badge-danger badge-pill" name="timeRemainingItem">' + response.data.dateRange + '</span>';

                        }
                        else {
                            component += '<span class="badge badge-primary badge-pill" name="timeRemainingItem">' + response.data.dateRange + '</span>';

                        }
                        component += '</div>' +
                            '<div class="avatar ml-1">';
                        if (response.data.employe_ImageProfile == null) {
                            component += '<img name="imageItem" title="' + response.data.employee_FullName + '" src="/AdminTemplate4/app-assets/images/portrait/small/avatarUser.png" alt="avatar" height="30" width="30">';
                        }
                        else {
                            component += '<img name="imageItem" title="' + response.data.employee_FullName + '" src="/images/EmployeeImages/' + response.data.employe_ImageProfile +'" alt="avatar" height="30" width="30">';

                        }
                            
                           component +=  '</div>' +
                            '<a class="todo-item-delete ml-75" data-id="' + response.data.id + '"><i class="fa fa-times red"></i></a>' +
                            '</div></div></li>';
                        todoTaskListWrapper.append( component);
                        // new task sidebar, overlay hide
                        todoNewTasksidebar.removeClass('show');
                        appContentOverlay.removeClass('show');
                    }
                    else {
                        toastr.error(response.msg, "Error")
                        // new task sidebar, overlay hide
                        todoNewTasksidebar.removeClass('show');
                        appContentOverlay.removeClass('show');
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

  // On Click of Close Icon btn, cancel btn and overlay remove show class from new task sidebar and overlay
  // and reset all form fields
  $(".close-icon, .cancel-btn, .app-content-overlay, .mark-complete-btn").on('click', function () {
    todoNewTasksidebar.removeClass('show');
    appContentOverlay.removeClass('show');
    setTimeout(function () {
      todoNewTasksidebar.find('textarea').val("");
    }, 100)
  });

  // Update Task
    updateTodo.on("click", function () {
        var task = new Object();
        task.ClientId = $(client).val();
        task.SubjectId = $(subject).val();
        task.EmployeeId = $(employee).val();
        task.DueDate = $(dueDate).val();
        task.Status = $(state).val();
        task.Priority = $(priority).val();
        task.Nota = $(nota).val();
        task.Id = $('#TaskId').val();

        if(validateTask(task, "Edit")){
            $.ajax({
                async: true,
                type: "POST",
                url: "/tasks/update/",
                data: {
                    model: task
                },
                beforeSend: function () {

                },
                success: function (response) {
                    if (!response.success) {
                        toastr.error(response.msg, "Error");
                    }
                    else {
                        var data = response.data;

                        if (data.status == 2) {
                            elemItemEdit.addClass('completed');
                            elemItemEdit.find('input[type="checkbox"]').prop('checked', true)
                        }
                        else {
                            elemItemEdit.removeClass('completed');
                            elemItemEdit.find('input[type="checkbox"]').prop('checked', false)
                        }
                        elemItemEdit.find('[name="subjectItem"]').html(data.client_FullName + " " + data.client_PhoneNumber + " - " + data.subject)
                        var elemTime = elemItemEdit.find('[name="timeRemainingItem"]').html(data.dateRange)
                        elemTime.html(data.dateRange);
                        if (data.isFinished) {
                            elemTime.removeClass("badge-primary");
                            elemTime.addClass("badge-danger");
                        }
                        else{
                            elemTime.addClass("badge-primary");
                            elemTime.removeClass("badge-danger");
                        }
                        elemItemEdit.find('[name="imageItem"]').attr('title', data.employee_FullName);
                        if (data.employe_ImageProfile == null) {
                            elemItemEdit.find('[name="imageItem"]').attr('src', "/AdminTemplate4/app-assets/images/portrait/small/avatarUser.png");
                        }
                        else {
                            elemItemEdit.find('[name="imageItem"]').attr('src', "/images/EmployeeImages/" + data.employe_ImageProfile);
                        }
                        todoNewTasksidebar.removeClass('show');
                        appContentOverlay.removeClass('show');
                    }
                },
                error: function (e) {
                    toastr.error("No se ha podido actualizar la tarea")
                }
            })
            
        }
        
  });

  // ************Rightside content************//
  // -----------------------------------------

  // Search filter for task list
  $(document).on("keyup", ".todo-search", function () {
    todoItem = $(".todo-item");
    $('.todo-item').css('animation', 'none')
    var value = $(this).val().toLowerCase();
    if (value != "") {
      todoItem.filter(function () {
        $(this).toggle($(this).text().toLowerCase().includes(value));
      });
      var tbl_row = $(".todo-item:visible").length; //here tbl_test is table name

      //Check if table has row or not
      if (tbl_row == 0) {
        if (!noResults.hasClass('show')) {
          noResults.addClass('show');
        }
      } else {
        noResults.removeClass('show');
      }
    } else {
      // If filter box is empty
      todoItem.show();
      if (noResults.hasClass('show')) {
        noResults.removeClass('show');
      }
    }
  });

  // on Todo Item click show data in sidebar
    var elemItemEdit = null;
  var globalThis = ""; // Global variable use in multiple function
    todoTaskListWrapper.on('click', '.todo-item', function (e) {

        $('#containerStatus').show();
        $('#containerClient').attr('class','col-md-6');

        elemItemEdit = $(this);
        var id = $(this).attr('data-id');
        var $this = $(this);
        globalThis = $this;
        $.ajax({
            async: true,
            type: "POST",
            url: "/tasks/details/",
            data: {
                id:id
            },
            beforeSend: function () {
                $.blockUI();
            },
            success: function (response) {
                if (!response.success) {
                    toastr.error(response.msg, "Error");
                }
                else {
                    var task = response.data;
                    subject.val(task.subject).trigger('change');
                    var date = task.dueDate.split("T");
                    dueDate.val(date[0]).trigger('change');
                    employee.val(task.employee.id).trigger('change');

                    if ($('#ClientId').find("option[value='" + task.client.id + "']").length > 0) {
                        $('#ClientId').val(task.client.id).trigger('change');
                    } else {
                        // Create a DOM Option and pre-select by default
                        var newOption = new Option(task.client.fullName, task.client.id, true, true);
                        // Append it to the select
                        $('#ClientId').append(newOption).trigger('change');
                    } 

                    state.val(task.status).trigger('change');
                    priority.val(task.priority).trigger('change');
                    nota.val(task.nota);
                    $('#TaskId').val(response.data.id);

                    taskNumber.html("#" + response.data.number);

                    todoNewTasksidebar.addClass('show');
                    appContentOverlay.addClass('show');

                    var todoTitle = $this.find(".todo-title").text();
                    taskTitle.val(todoTitle);
                    // if avatar is available
                    if ($this.find(".avatar img").length) {
                        assignedAvatarContent.addClass("d-none");
                    } else {
                        avatarUserImage.addClass("d-none");
                        assignedAvatarContent.removeClass("d-none");
                    }

                    // badge selected value check
                    if ($(this).find('.badge').length) {
                        //if badge available in current task
                        var badgevalAll = [];
                        var selected = $(this).find('.badge');

                        selected.each(function () {
                            var badgeVal = $(this).text();
                            badgevalAll.push(badgeVal);
                        });
                    }

                    // update button has remove class d-none & add class d-none in add todo button
                    updateTodo.removeClass("d-none");
                    addTodo.addClass("d-none");

                    markCompleteBtn.addClass("d-none"); //Oculto para no usar esta funcionalidad
                    newTaskTitle.addClass("d-none");
                    taskNumber.removeClass("d-none");

                }
                $.unblockUI();
            },
            error: function (e) {
                toastr.error("No se ha podido obtener la informacion de la tarea");
                $.unblockUI();
            }
        })
    })
    .on('click', '.todo-item-favorite', function (e) {
        e.stopPropagation();
        $(this).toggleClass("warning");
        $(this).find("i").toggleClass("bxs-star");
    }).on('click', '.todo-item-delete', function (e) {
        var id = $(this).attr('data-id');
        $.ajax({
            async: true,
            type: "Post",
            url: "/tasks/changestatus",
            data: {
                id: id,
                status: 3
            },
            success: function (response) {
                if (!response.success) {
                    toastr.error(response.msg);
                }
            },
            error: function (e) {
                toastr.error("No se ha podido completar la tarea");
            }
        })
        e.stopPropagation();
        $(this).closest('.todo-item').remove();
    }).on('click', '.custom-checkbox', function (e) {
        e.stopPropagation();
    })
        .on('click', '.todo-item-view', function (e) {
            e.stopPropagation();
            var id = $(this).attr('data-id');
            $.ajax({
                type: "GET",
                url: "/Clients/ClientDetails?id=" + id,
                async: false,
                success: function (response) {
                    if (response.success) {
                        $('[name="ModalClientNumber"]').html(response.data.number);
                        $('[name="ModalClientName"]').html(response.data.name);
                        $('[name="ModalClientLastName"]').html(response.data.lastName);
                        $('[name="ModalClientPhone"]').html(response.data.phone);
                        $('[name="ModalClientEmail"]').html(response.data.email);
                        $('[name="ModalClientID"]').html(response.data.id);
                        $('[name="ModalClientPais"]').html(response.data.country);
                        $('[name="ModalClientCiudad"]').html(response.data.city);
                        $('[name="ModalClientAddress"]').html(response.data.address);
                        $('[name="ModalClientZip"]').html(response.data.zip);
                        $('[name="ModalClientPhoneCuba"]').html(response.data.phoneCuba);
                        $('[name="btnView"]').attr("href","/Clients/Tramites/" + response.data.clientId);
                        $('#viewClient').modal('show')
                    }
                    else {
                        toastr.error(response.msg);
                    }
                    
                },
                failure: function (response) {
                    toastr.error("No se ha podido obtener el cliente")
                },
                error: function (response) {
                    toastr.error("No se ha podido obtener el cliente")

                }
            });
        });
    

  // Complete task strike through
    todoTaskListWrapper.on('click', ".todo-item .custom-control-input", function (e) {
      //Completar tarea
        var component = $(this);
      var id = $(this).attr('data-id');
        var status = 1;
        var textMsg = "Esta seguro de completar la tarea?"
        if ($(this).is(':checked')) {
            status = 2;
        }
        else {
            textMsg = "Esta seguro de pasar a estado En Curso la tarea?"
        }
        
        Swal.fire({
            title: textMsg,
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Si!",
            cancelButtonText: "Cancelar",
            confirmButtonClass: "btn btn-primary",
            cancelButtonClass: "btn btn-danger ml-1",
            buttonsStyling: false
        }).then(function (result) {
            if (result.value) {
                $.ajax({
                    async: true,
                    type: "Post",
                    url: "/tasks/changestatus",
                    data: {
                        id: id,
                        status: status
                    },
                    success: function (response) {
                        if (!response.success) {
                            toastr.error(response.msg);
                        }
                    },
                    error: function (e) {
                        toastr.error("No se ha podido completar la tarea");
                    }
                })
                $(component).closest('.todo-item').toggleClass("completed");

                Swal.fire({
                    type: "success",
                    title: "Completado!",
                    text: "Se ha cambiado el estado de la tarea.",
                    confirmButtonClass: "btn btn-success"
                });
            }
            else {
                if ($(component).is(':checked')) {
                    $(component).prop('checked', false);
                }
                else {
                    $(component).prop('checked', true);

                }
            }
        });
        
  });

  // Complete button click action
  markCompleteBtn.on("click", function () {
    globalThis.addClass("completed");
    globalThis.find(".custom-control-input").prop("checked", true);
  });

  // Todo sidebar toggle
  $('.sidebar-toggle').on('click', function (e) {
    e.stopPropagation();
    sideBarLeft.toggleClass('show');
    appContentOverlay.addClass('show');
  });

  // sorting task list item
  $(".ascending").on("click", function () {
    todoItem = $(".todo-item");
    $('.todo-item').css('animation', 'none')
    todoItem.sort(sort_li).appendTo(todoTaskListWrapper);

    function sort_li(a, b) {
      return ($(b).find('.todo-title').text()) < ($(a).find('.todo-title').text()) ? 1 : -1;
    }
  });

  // descending sorting
  $(".descending").on("click", function () {
    todoItem = $(".todo-item");
    $('.todo-item').css('animation', 'none')
    todoItem.sort(sort_li).appendTo(todoTaskListWrapper);

    function sort_li(a, b) {
      return ($(b).find('.todo-title').text()) > ($(a).find('.todo-title').text()) ? 1 : -1;
    }
  });
});

$(window).on("resize", function () {
  // remove show classes from sidebar and overlay if size is > 992
  if ($(window).width() > 992) {
    if (appContentOverlay.hasClass('show')) {
      sideBarLeft.removeClass('show');
      appContentOverlay.removeClass('show');
      todoNewTasksidebar.removeClass("show");
    }
  }
});

$(document).ready(function () {

    $('.todo-search').trigger("keyup");
})