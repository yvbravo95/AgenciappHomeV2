const selectAgency = $('#select-agency').select2();
const selectMinorista = $('#select-minorista').select2();
const selectProvince = $('#select-province').select2();
const selectMunicipality = $('#select-municipality').select2();

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
    const i = $(e.target).data('column');
    table.column(i).search($(e.target).val()).draw();
}, 350);

const t = $('#tbl');
const table = t.DataTable({
    "searching": true,
    "lengthChange": false,
    "order": [[0, "desc"]],
    language: {
        "decimal": "",
        "emptyTable": "No hay información",
        "info": "Mostrando _START_ a _END_ de _TOTAL_ Entradas",
        "infoEmpty": "Mostrando 0 a 0 de 0 Entradas",
        "infoFiltered": "(Filtrado de _MAX_ total entradas)",
        "infoPostFix": "",
        "thousands": ",",
        "lengthMenu": "Mostrar _MENU_ Entradas",
        "loadingRecords": "Cargando...",
        "processing": "Procesando...",
        "search": "Buscar:",
        "zeroRecords": "Sin resultados encontrados",
        "paginate": {
            "first": "Primero",
            "last": "Ultimo",
            "next": "Siguiente",
            "previous": "Anterior"
        }
    }
});

table.column(0).visible(false);
table.column(1).visible(false);
table.column(2).visible(false);

$.each($(".searchColumn", t), (i, e) => {
    $(e).on('change', (e) => searchColumn(e, table));
})

$('#btn-confirm').on('click', function () {
    const rows = table.rows({ filter: 'applied' }).data();
    const price = $('#input-price').val();

    let data = [];
    rows.each(item => {
      
        const agencyAux = agencies.find(x => x.name == item[3])
        const retailAux = retails.find(x => x.name == item[4])
        const provAux = provinces.find(x => x.name == item[5])
        const municAux = municipalities.find(x => x.name == item[6])
        data.push({
            type: item[0],
            id: item[1],
            agencyId: item[2],
            retailAgencyId: agencyAux?.id,
            retailId: retailAux?.id,
            provinceId: provAux.id,
            municipalityId: municAux.id,
            price: parseFloat(price)
        })
    })

    $.ajax({
        async: true,
        type: "POST",
        contentType: "application/x-www-form-urlencoded",
        url: "/setting/UpdatePriceByProvice",
        data: {
            model: data
        },
        beforeSend: function () {
            $.blockUI();
        },
        success: function (response) {
            if (response.success)
                location.reload();

            $.unblockUI();
        },
        error: function () {
            toastr.error("No se ha podido exportar", "Error");
            $.unblockUI();
        },
    });
})