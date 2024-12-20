$(document).ready(function(){
    var idGuia = null;
    $('.permiso').on('click', async function(){
        idGuia = $(this).attr('data-guiaId');
        var datos = await $.get("/OrderCubiq/GetAccessAgenciesGuia?id=" + idGuia);
        if(!datos.success){
            toastr.error(datos.msg);
            return false;
        }
        
        var tblBody = $('#bodyTblMinorista');
        $(tblBody).html("");
        if(datos.minoristas.length > 0){
            for (let i = 0; i < datos.minoristas.length; i++) {
                const element = datos.minoristas[i];
                var row = "";
                var verify = datos.agenciesAccess.find(x => x.id == element.id);

                if(verify != null){
                    row = '<tr>'+
                    '<td>'+
                        '<label class="custom-control custom-checkbox">'+
                            '<input type="checkbox" class="custom-control-input" name="Minoristas" value="'+ element.id +'" checked />'+
                            '<span class="custom-control-indicator"></span>'+
                            '<span class="custom-control-description"></span>'+
                        '</label>'+
                    '</td>'+
                    '<td>'+element.name+'</td>'+
                '</tr>';
                }
                else{
                    row = '<tr>'+
                    '<td>'+
                        '<label class="custom-control custom-checkbox">'+
                            '<input type="checkbox" class="custom-control-input" name="Minoristas" value="'+ element.id +'" />'+
                            '<span class="custom-control-indicator"></span>'+
                            '<span class="custom-control-description"></span>'+
                        '</label>'+
                    '</td>'+
                    '<td>'+element.name+'</td>'+
                '</tr>';
                }
                $(tblBody).append(row);
            }
            $('#modalAgenciesAccess').modal('show');
       
        }
        else{
            toastr.info("No existen minoristas");
            return false;
        }

    })

    $('#btnAcceptModalAccess').on('click', function(){
        if(idGuia == null){
            toastr.info("No existe una guía aérea");
            return false;
        }
        var rows = $(document).find('[name="Minoristas"]');
        var minoristas = [];
        for (let index = 0; index < rows.length; index++) {
            const elem = rows[index];
            if($(elem).is(':checked')){
                var id = $(elem).val();
                minoristas.push(id);
            }
        }
        $.ajax({
            async: true,
            type: "POST",
            url: "/OrderCubiq/AddAccessAgenciesGuia",
            data: {
                id: idGuia,
                minoristas: minoristas
            },
            beforeSend: function(){
                $.blockUI();
            },
            success: function(response){
                if(response.success){
                    toastr.success("Los cambios han sido guardados");
                }
                else{
                    toastr.error(response.msg);
                }
                $.unblockUI();
            },
            error: function(e){
                toastr.error("Ha ocurrido un error");
                $.unblockUI();
            }
        })
    })

    var idGuia = null;
    $('.editcosto').on('click', function () {
        idGuia = $(this).attr('data-guiaId');
        $('#modalEditCost').modal('show');
    })

    $('#btnSaveCost').on('click', function () {
        var cost = $('#cost_input').val();
        $.ajax({
            type:"POST",
            url:"/OrderCubiq/EditCostGuia",
            data: {
                guiaId: idGuia,
                cost: cost
            },
            success: function (response) {
                if (response.success) {
                    location.reload();
                }
                else {
                    toastr.error(response.error);
                }
            },
            error: function (error) {
                toastr.error("No se ha podido actualizar el costo de la guia");
            }
        })
    })
})