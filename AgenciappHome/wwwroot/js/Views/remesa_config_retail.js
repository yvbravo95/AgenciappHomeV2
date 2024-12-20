const content_header_html =
    `<h6 class="text-primary font-weight-bold">Mayorista Cubatex Multiservices</h6>
    <ul class="list-group list-group-flush">
        <li>- Les informamos que este servicio es para que ud pueda enviar sus remesas a cuba con la ayuda de un Mayorista.</li>
        <li>- Cubatex Multiservices , es una agencia especiada en Remesas a Cuba. Esta agencia esta validada y aprobada por la administración de Agenciapp.</li>
        <li>- La tabla que aparece abajo, es la información de los costos del mayorista.</li>
        <li>- Agenciapp por este servicio va cobrar 0.90 por cada Orden realizada. Este fee sera sumado al costo del mayorista.</li>
        <li>Importante: Toda la información que usted ingrese de sus clientes estará protegida con este servicio. Cubatex Multiservices, NO tendrá acceso a teléfono, email y dirección de sus Clientes.</li>
    </ul>`;

$('#content_header').html(content_header_html);

$('#btnGuardar').on('click', function () {
    var id = $('#ref_valortramite_id').val();
    //ajax
    $.ajax({
        url: '/remesas/ConfigRetail',
        type: 'POST',
        data: {
            idRef: id
        },
        beforeSend: function () {
            $.blockUI();
        },
        success: function (data) {
            $.unblockUI();
            if (data.success) {
                window.location.href = `/remesas/createremesa?idWholesaler=${data.idWholesaler}`;
            } else {
                toastr.error(data.msg, 'Error');
            }
        },
        error: function (error) {
            toastr.error('Error al guardar la información', 'Error');
            $.unblockUI();
        }
    });
});