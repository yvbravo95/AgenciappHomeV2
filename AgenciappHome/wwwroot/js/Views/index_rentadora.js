$(document).on('ready', function () {
    $('.table-group').DataTable({
        "lengthChange": false,
        "dom": 't',
        "order": [[3, "asc"]],
        'language': {
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
        },
        "columnDefs": [
            {
                targets: 3,
                visible: false
            }
        ],
        "scrollY": "400px",
        "scrollCollapse": true,
        "paging": false,
        "drawCallback": function (settings) {
            var api = this.api();
            var rows = api.rows({ page: 'current' }).nodes();
            var last = null;
            api.column(3, { page: 'current' }).data().each(function (group, i) {
                if (last != group) {
                    if (group == "Alta") {
                        $(rows).eq(i).before(
                            '<tr class="group"><td colspan=7>' + group + ' (1/1-30/6) (1/9-30/11)</td></tr>'
                        );
                    }
                    else if (group == "Media Alta") {
                        $(rows).eq(i).before(
                            '<tr class="group"><td colspan=7>' + group + '</td></tr>'
                        );
                    }
                    else if (group == "Media ALta II") {
                        $(rows).eq(i).before(
                            '<tr class="group"><td colspan=7>' + group + '</td></tr>'
                        );
                    }
                    else {
                        $(rows).eq(i).before(
                            '<tr class="group"><td colspan=7>' + group + ' (1/7-31/8) (1/12-31/12)</td></tr>'
                        );
                    }

                    last = group;
                }
            })
        }
    });

    $('#table_precios').DataTable({
        "lengthChange": false,
        "dom": 't',
        order: -1
    });

    $("#Rentadoraid").select2({
        width: "100%"
    });

    $("#Rentadoraid").on("change", () => {
        var rentadoraId = $("#Rentadoraid").val();
        if (rentadoraId) {
            $.ajax({
                url: "/Ticket/GetConfigPreciosAuto",
                method: "POST",
                data: {
                    rentadoraId: rentadoraId
                },
                success: function (response) {
                    if (response.success) {
                        if (isMayorista) {
                            $("#div_precio").html(`
                            <table class="table table-striped table-bordered data-multiple-buttongroups" id="table_precios">
                            <thead>
                                <tr>
                                    <th>Categoría</th>
                                    <th>Margen Alta</th>
                                    <th>Margen Ext</th>
                                    <th>Combust + Imp</th>
                                    <th>Minorista 1</th>
                                    <th>Minorista 2</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td>
                                        Economico Manual
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="Eco" data-type="number">
                                            ${response.data.precios1}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="Eco" data-type="number">
                                            ${response.data.seguro1}                                            
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editFijo" href="#" data-pk="Eco" data-type="number">
                                            ${response.data.feeFijo1}                                                                                        
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista1" href="#" data-pk="Eco" data-type="number">
                                            ${response.data.minorista11}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista2" href="#" data-pk="Eco" data-type="number">
                                            ${response.data.minorista21}
                                        </a>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        Economico Automatico
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="Eco-Autom" data-type="number">
                                            ${response.data.precios11}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="Eco-Autom" data-type="number">
                                            ${response.data.seguro11}                                            
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editFijo" href="#" data-pk="Eco-Autom" data-type="number">
                                            ${response.data.feeFijo11}                                                                                        
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista1" href="#" data-pk="Eco-Autom" data-type="number">
                                            ${response.data.minorista111}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista2" href="#" data-pk="Eco-Autom" data-type="number">
                                            ${response.data.minorista211}
                                        </a>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        Medio Manual
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="MedioAut" data-type="number">
                                            ${response.data.precios3}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="MedioAut" data-type="number">
                                            ${response.data.seguro3}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editFijo" href="#" data-pk="MedioAut" data-type="number">
                                            ${response.data.feeFijo3}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista1" href="#" data-pk="MedioAut" data-type="number">
                                            ${response.data.minorista13}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista2" href="#" data-pk="MedioAut" data-type="number">
                                            ${response.data.minorista23}
                                        </a>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        Medio Automatico
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="MedioAut-Autom" data-type="number">
                                            ${response.data.precios12}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="MedioAut-Autom" data-type="number">
                                            ${response.data.seguro12}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editFijo" href="#" data-pk="MedioAut-Autom" data-type="number">
                                            ${response.data.feeFijo12}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista1" href="#" data-pk="MedioAut-Autom" data-type="number">
                                            ${response.data.minorista112}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista2" href="#" data-pk="MedioAut-Autom" data-type="number">
                                            ${response.data.minorista212}
                                        </a>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        Estandar
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="Estandar" data-type="number">
                                            ${response.data.precios2}                                            
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="Estandar" data-type="number">
                                            ${response.data.seguro2}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editFijo" href="#" data-pk="Estandar" data-type="number">
                                            ${response.data.feeFijo2}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista1" href="#" data-pk="Estandar" data-type="number">
                                            ${response.data.minorista12}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista2" href="#" data-pk="Estandar" data-type="number">
                                            ${response.data.minorista22}
                                        </a>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        Premium
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="Premiun" data-type="number">
                                            ${response.data.precios4}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="Premiun" data-type="number">
                                            ${response.data.seguro4}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editFijo" href="#" data-pk="Premiun" data-type="number">
                                            ${response.data.feeFijo4}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista1" href="#" data-pk="Premiun" data-type="number">
                                            ${response.data.minorista14}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista2" href="#" data-pk="Premiun" data-type="number">
                                            ${response.data.minorista24}
                                        </a>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        Premium Plus
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="PremiunPlus" data-type="number">
                                            ${response.data.precios5}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="PremiunPlus" data-type="number">
                                            ${response.data.seguro5}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editFijo" href="#" data-pk="PremiunPlus" data-type="number">
                                            ${response.data.feeFijo5}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista1" href="#" data-pk="PremiunPlus" data-type="number">
                                            ${response.data.minorista15}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista2" href="#" data-pk="PremiunPlus" data-type="number">
                                            ${response.data.minorista25}
                                        </a>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        Jeep
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="Jeep" data-type="number">
                                            ${response.data.precios6}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="Jeep" data-type="number">
                                            ${response.data.seguro6}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editFijo" href="#" data-pk="Jeep" data-type="number">
                                            ${response.data.feeFijo6}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista1" href="#" data-pk="Jeep" data-type="number">
                                            ${response.data.minorista16}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista2" href="#" data-pk="Jeep" data-type="number">
                                            ${response.data.minorista26}
                                        </a>
                                    </td>
                                </tr>
                                 <tr>
                                    <td>
                                        Jeep Alto Estandar
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="JeepAE" data-type="number">
                                            ${response.data.precios7}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="JeepAE" data-type="number">
                                            ${response.data.seguro7}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editFijo" href="#" data-pk="JeepAE" data-type="number">
                                            ${response.data.feeFijo7}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista1" href="#" data-pk="JeepAE" data-type="number">
                                            ${response.data.minorista17}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista2" href="#" data-pk="JeepAE" data-type="number">
                                            ${response.data.minorista27}
                                        </a>
                                    </td>
                                </tr>
                                 <tr>
                                    <td>
                                        Camper
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="Camper" data-type="number">
                                            ${response.data.precios8}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="Camper" data-type="number">
                                            ${response.data.seguro8}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editFijo" href="#" data-pk="Camper" data-type="number">
                                            ${response.data.feeFijo8}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista1" href="#" data-pk="Camper" data-type="number">
                                            ${response.data.minorista18}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista2" href="#" data-pk="Camper" data-type="number">
                                            ${response.data.minorista28}
                                        </a>
                                    </td>
                                </tr>
                                 <tr>
                                    <td>
                                        Motos
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="Motos" data-type="number">
                                            ${response.data.precios9}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="Motos" data-type="number">
                                            ${response.data.seguro9}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editFijo" href="#" data-pk="Motos" data-type="number">
                                            ${response.data.feeFijo9}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista1" href="#" data-pk="Motos" data-type="number">
                                            ${response.data.minorista19}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista2" href="#" data-pk="Motos" data-type="number">
                                            ${response.data.minorista29}
                                        </a>
                                    </td>
                                </tr>
                                 <tr>
                                    <td>
                                        Van
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="Van" data-type="number">
                                            ${response.data.precios10}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="Van" data-type="number">
                                            ${response.data.seguro10}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editFijo" href="#" data-pk="Van" data-type="number">
                                            ${response.data.feeFijo10}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista1" href="#" data-pk="Van" data-type="number">
                                            ${response.data.minorista110}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista2" href="#" data-pk="Van" data-type="number">
                                            ${response.data.minorista210}
                                        </a>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        SUV
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="SUV" data-type="number">
                                            ${response.data.precios13 ?? 0}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="SUV" data-type="number">
                                            ${response.data.seguro13 ?? 0}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editFijo" href="#" data-pk="SUV" data-type="number">
                                            ${response.data.feeFijo13 ?? 0}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista1" href="#" data-pk="SUV" data-type="number">
                                            ${response.data.minorista113 ?? 0}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editMinorista2" href="#" data-pk="SUV" data-type="number">
                                            ${response.data.minorista213 ?? 0}
                                        </a>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                        `);
                        }
                        else {
                            $("#div_precio").html(`
                            <table class="table table-striped table-bordered data-multiple-buttongroups" id="table_precios">
                            <thead>
                                <tr>
                                    <th>Categoría</th>
                                    <th>Margen Alta</th>
                                    <th>Margen Ext</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td>
                                        Economico
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="Eco" data-type="number">
                                            ${response.data.precios1}
                                        </a>
                                    </td>   
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="Eco" data-type="number">
                                            ${response.data.seguro1}                                            
                                        </a>
                                    </td>
                                </tr>
                                 <tr>
                                    <td>
                                        Medio
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="MedioAut" data-type="number">
                                            ${response.data.precios3}
                                        </a>
                                    </td> 
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="MedioAut" data-type="number">
                                            ${response.data.seguro3}                                            
                                        </a>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        Estandar
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="Estandar" data-type="number">
                                            ${response.data.precios2}                                            
                                        </a>
                                    </td>  
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="Estandar" data-type="number">
                                            ${response.data.seguro2}                                            
                                        </a>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        Premium
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="Premiun" data-type="number">
                                            ${response.data.precios4}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="Premiun" data-type="number">
                                            ${response.data.seguro4}                                            
                                        </a>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        Premium Plus
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="PremiunPlus" data-type="number">
                                            ${response.data.precios5}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="PremiunPlus" data-type="number">
                                            ${response.data.seguro5}                                            
                                        </a>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        Jeep
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="Jeep" data-type="number">
                                            ${response.data.precios6}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="Jeep" data-type="number">
                                            ${response.data.seguro6}                                            
                                        </a>
                                    </td>
                                </tr>
                                 <tr>
                                    <td>
                                        Jeep Alto Estandar
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="JeepAE" data-type="number">
                                            ${response.data.precios7}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="JeepAE" data-type="number">
                                            ${response.data.seguro7}                                            
                                        </a>
                                    </td>
                                </tr>
                                 <tr>
                                    <td>
                                        Camper
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="Camper" data-type="number">
                                            ${response.data.precios8}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="Camper" data-type="number">
                                            ${response.data.seguro8}                                            
                                        </a>
                                    </td>
                                </tr>
                                 <tr>
                                    <td>
                                        Motos
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="Motos" data-type="number">
                                            ${response.data.precios9}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="Motos" data-type="number">
                                            ${response.data.seguro9}                                            
                                        </a>
                                    </td>
                                </tr>
                                 <tr>
                                    <td>
                                        Van
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="Van" data-type="number">
                                            ${response.data.precios10}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="Van" data-type="number">
                                            ${response.data.seguro10}                                            
                                        </a>
                                    </td>
                                </tr>
                                 <tr>
                                    <td>
                                        SUV
                                    </td>
                                    <td>
                                        <a class="editPrecio" href="#" data-pk="SUV" data-type="number">
                                            ${response.data.precios13 ?? 0}
                                        </a>
                                    </td>
                                    <td>
                                        <a class="editSeguro" href="#" data-pk="SUV" data-type="number">
                                            ${response.data.seguro13 ?? 0}                                            
                                        </a>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                        `);
                        }
                        $('.editPrecio').editable({
                            url: `/Ticket/EditPreciosAutos?id=${rentadoraId}`,
                            success: function (response) {
                                if (response.success) {
                                    toastr.success("Se editó el precio correctamente.");
                                }
                            },
                            onblur: 'ignore'
                        });

                        $('.editSeguro').editable({
                            url: `/Ticket/EditSeguroAutos?id=${rentadoraId}`,
                            success: function (response) {
                                if (response.success) {
                                    toastr.success("Se editó el precio correctamente.");
                                }
                            },
                            onblur: 'ignore'
                        });

                        $('.editFijo').editable({
                            url: `/Ticket/EditFeeAutos?id=${rentadoraId}`,
                            success: function (response) {
                                if (response.success) {
                                    toastr.success("Se editó el precio correctamente.");
                                }
                            },
                            onblur: 'ignore'
                        });

                        $('.editMinorista1').editable({
                            url: `/Ticket/EditMinorista1Autos?id=${rentadoraId}`,
                            success: function (response) {
                                if (response.success) {
                                    toastr.success("Se editó el precio correctamente.");
                                }
                            },
                            onblur: 'ignore'
                        });

                        $('.editMinorista2').editable({
                            url: `/Ticket/EditMinorista2Autos?id=${rentadoraId}`,
                            success: function (response) {
                                if (response.success) {
                                    toastr.success("Se editó el precio correctamente.");
                                }
                            },
                            onblur: 'ignore'
                        });
                    }
                    else
                        showErrorMessage("Atención", "Error al cargar el precio y el costo");
                }
            })
        }
    })

    $.fn.editable.defaults.mode = 'inline';

    $('.edit').editable({
        url: "/Ticket/EditRentadora",
        success: function (response) {
            if (response.reload) {
                window.location = "/Ticket/Rentadoras?rentadoraId=" + response.rentadoraId;
            }
            if (response.success) {
                toastr.success("Se editó la rentadora correctamente.");
            }
        },
        onblur: "ignore"
    });
   
    $('[name="visible"]').on('change', function () {
        var idRentadora = $(this).attr('data-id');
        var checked = $(this)[0].checked;
        $.ajax({
            type: "POST",
            url: "/Ticket/changeStatusRentadora",
            data: {
                idRentadora: idRentadora,
                check: checked
            },
            success: function (data) {
                if (!data.success) {
                    toastr.error("Ocurrió un error al cambiar la visibilidad");
                }
            },
            failure: function (response) {
                showErrorMessage("ERROR", response.responseText);
            },
            error: function (response) {
                showErrorMessage("ERROR", response.responseText);
            }
        });
    });

    $(".select2-multiple").select2({
        width: "100%"
    });

    $('[name="tab"]').on('click', function () {
        $('[name="tab"]').removeClass('active');
        $(this).addClass('active');

    });

    $("#save_minoristas").on("click", () => {
        var minoristas1 = $("#agenciasprecio1").val()
        var minoristas2 = $("#agenciasprecio2").val()
        $.post( "/Ticket/SaveMinorista", {
                minoristas1: minoristas1,
                minoristas2: minoristas2
            },
            function (response) {
                if (response.success) {
                    toastr.success("Se guardo la configuracion de los minoristas");
                }
                else {
                    toastr.error("No se pudo guardar la configuracion de los minoristas");
                }
            })
    })
})