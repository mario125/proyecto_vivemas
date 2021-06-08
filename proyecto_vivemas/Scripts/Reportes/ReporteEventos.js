$(document).ready(() => {
    cargarProyectos();
});

document.getElementById('formReporteEventos').addEventListener('submit', (event) => {
    event.preventDefault();
    obtenerReporte();
});

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

const cargarProyectos = async () => {
    var response = await axios.post('../Proyectos/cargarProyectos');
    $('#selectProyecto').select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
}

const obtenerReporte = async () => {

    var data = new Object();    
    data.idProyecto = $('#selectProyecto').select2('data')[0].id;        
    const response = await axios.post('obtenerReporteEventos', JSON.stringify(data), {
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
            '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">NRO DOCUMENTO</th>' +
            '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">RAZON SOCIAL</th>' +
            '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">PROYECTO</th>' +
            '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">LOTE</th>' +
            '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">LLAMADA</th>' +
            '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">WHATSAPP</th>' +
            '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">CORREO</th>' +
            '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">ENTREVISTA</th>' +
            '</tr>';
        $("#theadReporte").html('').append(reporteCabecera);
        response.data.dataReporte.forEach((item) => {
            reporteCuerpo += '<tr>' +
                '<td>' + item.proformauif_cliente_nrodocumento + '</td>' +
                '<td>' + item.proformauif_cliente_razonsocial + '</td>' +
                '<td>' + item.proyecto_nombrecorto + '</td>' +
                '<td>' + item.cotizacion_lote_nombre + '</td>' +
                '<td>' + item.llamada + '</td>' +
                '<td>' + item.whatsapp + '</td>' +
                '<td>' + item.correo + '</td>' +
                '<td>' + item.entrevista + '</td>' +
                '</tr>'
        });
        $("#tbodyReporte").html('').append(reporteCuerpo);
    } else {
        toastr["error"]("ocurrio un error en el sistema, comuniquese con el area de sistemas");
    } 
}