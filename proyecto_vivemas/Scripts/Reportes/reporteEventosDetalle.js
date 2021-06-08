$(document).ready(() => {

    cargarProyectos();
    $("#inputLote").autocomplete({
        source: function (request, response) {
            $.getJSON("../Lotes/buscarLotesAutocomplete",
                {
                    term: request.term,
                    param1: $('#selectProyecto').select2('data')[0].id
                },
                response
            );
        },
        select: function (event, ui) {
           
           
            document.getElementById("idlote").value = ui.item.loteid;
      
        },
        delay: 300
    });
    
});


const cargarProyectos = async () => {
    var response = await axios.post('../Proyectos/cargarProyectos');
    $('#selectProyecto').select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
}








document.getElementById('formReporteEventosDetalle').addEventListener('submit', (event) => {
    event.preventDefault();
    obtenerReporte();

    
});









const obtenerReporte = async () => {  


    if (document.querySelector("#inputDocumentoCliente").value == "---") {
        toastr["warning"]("Debe buscar un cliente primero");
    } else {

        var miCadena = document.querySelector("#inputDocumentoCliente").value;
      
        var divisiones = miCadena.split("-");

       

        //alert(divisiones[0]);
        //alert(document.querySelector("#selectProyecto").value);
        //alert(document.querySelector("#idlote").value);


        var data = new Object();

        data.documento = divisiones[0];
        data.proyecto = document.querySelector("#selectProyecto").value ;
        data.lote = document.querySelector("#idlote").value ;

        const response = await axios.post('obtenerReporteEventosDetalle', JSON.stringify(data), {
            headers: {
                'Content-Type': 'application/json'
            }
        }); 


        if (response.data.flag == 1) {
            document.getElementById('buttonEmitirReportePDF').disabled = false;
            document.getElementById('buttonEmitirReporteXLS').disabled = false;
            var reporteCabecera = '';
            var reporteCuerpo = '';
            reporteCabecera += '<tr>' +
                '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">PROYECTO</th>' +
                '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">LOTE</th>' +
                '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">N° DOCUMENTO</th>' +
                '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">CLIENTE</th>' +
                '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">F. CREACION</th>' +
                '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">F. PROPUESTA</th>' +
                '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">MONTO PROPUESTO</th>' +
                '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">DESCRIPCION</th>' +
                '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">ESTADO</th>' +
                '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">TIPO CONTACTO</th>' +
                '</tr>';
            $("#theadReporte").html('').append(reporteCabecera);
            response.data.dataReporte.forEach((item) => {
                reporteCuerpo += '<tr>' +
                    '<td>' + item.proyecto_nombrecorto + '</td>' +
                    '<td>' + item.cotizacion_lote_nombre + '</td>' +
                    '<td>' + item.proformauif_cliente_nrodocumento + '</td>' +
                    '<td>' + item.proformauif_cliente_razonsocial + '</td>' +
                    '<td>' + item.evento_fechacreacion + '</td>' +
                    '<td>' + item.evento_fechapropuesta + '</td>' +
                    '<td>' + item.evento_montopropuesto + '</td>' +
                    '<td>' + item.evento_descripcion + '</td>' +
                    '<td>' + item.estadoevento_descripcion + '</td>' +
                    '<td>' + item.tipocontacto_descripcion + '</td>' +
                    '</tr>'
            });
            $("#tbodyReporte").html('').append(reporteCuerpo);
        } else {
            toastr["error"]("ocurrio un error en el sistema, comuniquese con el area de sistemas");
        }

        
    }
}


document.getElementById('buttonEmitirReportePDF').addEventListener("click", (e) => {
    const doc = new jsPDF({
        orientation: "landscape",
        unit: "in",
        format: [1000, 792]
    });
    doc.autoTable({
        html: '#tableReporte',
        showHead: 'firstPage',
        showFoot: 'lastPage'
    });
    doc.save('reporteEventos.pdf');
});

document.getElementById('buttonEmitirReporteXLS').addEventListener("click", (e) => {
    TableToExcel.convert(document.getElementById("tableReporte"), {
        name: "reporteEventos.xlsx",
        sheet: {
            name: "Reporte"
        }
    });
});