// Build Table
var tableIniciada;
const formatDate = "MM/DD/YY";

const blockOptions = {
    message: '<span class="semibold"> Loading...</span>',
    overlayCSS: {
        backgroundColor: '#fff',
        opacity: 0.8,
        cursor: 'wait'
    },
    css: {
        border: 0,
        padding: 0,
        backgroundColor: 'transparent'
    }
};

const tableLanguage = {
    "decimal": "",
    "emptyTable": "No hay información",
    "info": "Mostrando _START_ a _END_ de _TOTAL_ Entradas",
    "infoEmpty": "Mostrando 0 a 0 de 0 Entradas",
    "infoFiltered": "(Filtrado de _MAX_ total entradas)",
    "infoPostFix": "",
    "thousands": ",",
    "lengthMenu": "Mostrar _MENU_ Entradas",
    "loadingRecords": "Cargando...",
    "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw"></i>Procesando...',
    "search": "Buscar:",
    "zeroRecords": "Sin resultados encontrados",
    "paginate": {
        "first": "Primero",
        "last": "Ultimo",
        "next": "Siguiente",
        "previous": "Anterior"
    }
};
$.fn.dataTable.ext.errMode = 'none'; //No mostrar warning de alerta de dataTable

$.fn.editable.defaults.mode = 'inline';

const debounce = (func, wait) => {
    let timeout;

    return function executedFunction(...args) {
        const later = () => {
            timeout = null;
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
};

var searchColumn = debounce(function (e, table) {
    var i = $(e.target).data('column');
    table.column(i).search($(e.target).val()).draw();
}, 350);

$(window).on("load", () => {

    var divContainer = $('#tblContainer');
    $(divContainer).block(blockOptions);

    var t = $('#tbl');
    tableIniciada = t.DataTable({
        searching: true,
        dom: "ltip",
        lengthChange: false,
        order: [[1, "desc"]],
        serverSide: true,
        processing: false,
        language: tableLanguage,
        ajax: {
            "url": `/empleadocubamaritimo/GetData?status=Iniciadas`,
            "type": 'POST',
            "dataType": "json",
            "dataFilter": function (data) {
                var json = jQuery.parseJSON(data);
                return JSON.stringify(json); // return JSON string
            },
            "statusCode": {
                401: function () {
                    location.href = "/Account/Login"
                }
            }
        },
        columnDefs: [
            {
                targets: [0],
                data: "id",
                render: function (data, type, item) {
                    var html = '<label class="custom-control custom-checkbox">' +
                        `<input type="checkbox" class="custom-control-input order-select" value="${data}" />` +
                        '<span class="custom-control-indicator"></span>' +
                        '<span class="custom-control-description"></span>' +
                        '</label>';
                    return html;
                }
            },
            {
                targets: [1],
                data: "date",
                render: function (data, type, item) {
                    const init = new moment(item.createdAt);
                    const end = new moment();
                    var duration = moment.duration(end.diff(init));
                    var html = [];
                    html.push(moment(data).format(formatDate));
                    html.push(`<div><span class="tag tag-default tag-warning">${duration.days()}d ${duration.hours()}h ${duration.minutes()}m</span></div>`);
                    return html.join("");
                }
            },
            {
                targets: [2],
                data: "number",
                name: "number",
                render: function (data, type, item) {
                    var html = [];
                    html.push(`<a href="/enviomaritimo/detailbag/${item.id}">${data}</a>`);
                    if (item.type == "Tienda" && item.noOrden) {
                        html.push(`<p style="color:red">${item.noOrden}</p>`)
                    }
                    if (item.express) {
                        html.push('<div class="tag tag-danger">Express</div>');
                    }
                    if (item.problem) {
                        html.push('<div class="tag tag-danger">Problema</div>');
                    }
                    return html.join("");
                },
            },
            {
                targets: [3],
                data: "maritimeNumber",
                name: "maritimeNumber"
            },
            {
                targets: [4],
                data: "quantity",
                name: "quantity",
            },
            {
                targets: [5],
                data: "weight",
                name: "weight"
            },
            {
                targets: [6],
                data: "province",
                name: "province"
            },
            {
                targets: [7],
                data: "description",
                name: "description"
            },
            {
                targets: [8],
                data: "id",
                render: function (data, type, item) {
                    var html = [];
                    html.push('<div class="dropdown">');
                    html.push('<i class="fa fa-ellipsis-v dropdown-toggle" data-toggle="dropdown" style="cursor: pointer"></i>');
                    html.push(`<div class="dropdown-menu" aria-label="dropdown${item.id}">`);
                    html.push(`<a class="dropdown-item" href="/enviomaritimo/detailbag?id=${data}"><i class="ft-info"></i>Detalles</a>`);
                    html.push('</div>');
                    html.push('</div>');
                    return html.join("");
                },
            },
        ],
        createdRow: function (row, data, rowIndex) {
            InitRow(row);
        },
        initComplete: function (settings, json) {
            $(divContainer).unblock();
        }
    });

    $.each($(".searchColumn", t), (i, e) => {
        $(e).on('keyup', (e) => searchColumn(e, tableIniciada));
    })

    $(t).on('processing.dt', function (e, settings, processing) {
        if (processing) {
            $(divContainer).block(blockOptions);
        }
        else {
            $(divContainer).unblock();
        }
    })

    LoadEvents();
})


function InitRow(row) {
    $.each($('.dropdown-toggle', row), (i, e) => { $(e).dropdown(); });
}

function LoadEvents() {
    var cantSelect = 0;
    var selectedIds = new Array;
    $(document).on("change", ".order-select", function () {
        if ($(this)[0].checked) {
            cantSelect++;
            selectedIds.push($(this).val());
        } else {
            cantSelect--;
            selectedIds.splice($.inArray($(this).val(), selectedIds), 1);
        }

        if (cantSelect > 0) {
            $('#btn-nacionalizar').removeClass('hidden');
        }
        else {
            $('#btn-nacionalizar').addClass('hidden');
        }
    });

    $('#btn-nacionalizar').on('click', function () {
        // swal confirm
        swal({
            title: "¿Está seguro?",
            text: "¿Desea marcar como nacionalizadas las órdenes seleccionadas?",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Si",
            cancelButtonText: "No",
            closeOnConfirm: false
        }, function (isConfirm) {
            if (isConfirm) {
                $.ajax({
                    url: '/empleadocubamaritimo/ChangeStatusBag',
                    type: 'POST',
                    data: {
                        status: 'Nacionalizando',
                        ids: selectedIds
                    },
                    success: function (data) {
                        if (data.success) {
                            swal("¡Correcto!", "Las órdenes seleccionadas se han marcado como nacionalizadas", "success");
                            tableIniciada.ajax.reload();
                            cantSelect = 0;
                            selectedIds = new Array;
                            $('#btn-nacionalizar').addClass('hidden');
                        }
                        else {
                            console.log(data);
                            swal("¡Error!", "Ha ocurrido un error al intentar marcar las órdenes como nacionalizadas", "error");
                        }
                    },
                    error: function (data) {
                        console.log(data);
                        swal("¡Error!", "Ha ocurrido un error al intentar marcar las órdenes como nacionalizadas", "error");
                    }
                });
            }
            else return false;
        });
    })
}
