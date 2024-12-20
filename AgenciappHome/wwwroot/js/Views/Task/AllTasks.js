$(document).ready(function () {
    var showAll = false;
    $('#Status').select2({
        containerCssClass: 'select-sm',
        placeholder: "Estado"
    });
    $('#Priority').select2({
        containerCssClass: 'select-sm',
        placeholder: "Prioridad"
    });
    
    $('#EmployeeFilter').select2({
        containerCssClass: 'select-sm',
        placeholder: "Empleado"
    });

    var loadPage = function () {
        var url = "/Tasks/AllTasks";
        var status = $('#Status').val();
        var priority = $('#Priority').val();
        var subject = $('#Subject').val();
        var employee = $('#EmployeeFilter').val();
        if (status != "" || priority != "" || subject != "" || employee != "") {
            url += "?";
            var init = false;
            if (status != "") {
                url += "status=" + status;
                init = true;
            }
            if (priority != "") {
                if (init) {
                    url += "&priority=" + priority;
                }
                else {
                    url += "priority=" + priority;
                }
                init = true;
            }
            if (subject != "") {
                if (init) {
                    url += "&subject=" + subject;
                }
                else {
                    url += "subject=" + subject;
                }
                init = true;
            }
            if (employee != "") {
                if (init) {
                    url += "&employee=" + employee;
                }
                else {
                    url += "employee=" + employee;
                }
                init = true;
            }
            if (showAll) {
                url += "&showAll=true";
            }
        }
        else {
            if (showAll) {
                url += "?showAll=true";
            }
        }
        location.href = url;
    };

    $('#Status').on('change', loadPage);
    $('#Priority').on('change', loadPage);
    $('#Subject').on('change', loadPage);
    $('#EmployeeFilter').on('change', loadPage);

    $('#btnShowAll').on('click', function () {
        showAll = true;
        loadPage();
    })

    $('#removeFilters').on('click', function () {
        location.href = "/Tasks/AllTasks";
    })
});