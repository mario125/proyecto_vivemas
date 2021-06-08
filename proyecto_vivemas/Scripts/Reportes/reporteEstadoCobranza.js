$(document).ready(() => {
    cargarProyectos();
});

document.getElementById('formReporteEstadoCobranza').addEventListener('submit', (event) => {
    event.preventDefault();
    obtenerReporte();
});


document.getElementById('buttonEmitirReportePDF').addEventListener("click", (e) => {
    const doc = new jsPDF({
        orientation: "landscape",
        unit: "in",
        format: [1700, 792]
    });
    doc.autoTable({
        html: '#tableReporte',
        showHead: 'firstPage',
        showFoot: 'never',
        styles: {
            halign: 'center',
            valign: 'bottom'
        }
    });
    doc.save('reporteEstadoCobranza.pdf');   
});

document.getElementById('buttonEmitirReporteXLS').addEventListener("click", (e) => {
    TableToExcel.convert(document.getElementById("tableReporte"), {
        name: "reporteEstadoCobranza.xlsx",
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
    const response = await axios.post('obtenerReporteEstadoCobranza?idProyecto=' + $('#selectProyecto').select2('data')[0].id)
    if (response.data.flag == 1) {
        console.log(response.data);
        document.getElementById('buttonEmitirReportePDF').disabled = false;
        document.getElementById('buttonEmitirReporteXLS').disabled = false;

        var reporteCabecera = '';
        var reporteCuerpo = '';
        var reportePie = '';

        reporteCabecera += '<tr> <th colspan="17" class="text-center table-info" data-a-h="center" data-a-v="bottom" data-fill-color="3399ff" data-f-color="ebedef">REPORTE DE ESTADO DE COBRANZA</th></tr>'
            + '<tr>'
            + '<th rowspan="3" class="text-center table-info" style="min-width:150px;" data-a-v="bottom" data-fill-color="3399ff" data-f-color="ebedef">PROYECTO</th>'
            + '<th rowspan="3" class="text-center table-info" style="min-width:100px;" data-a-v="bottom" data-fill-color="3399ff" data-f-color="ebedef">LOTE</th>'
            + '<th rowspan="3" class="text-center table-info" data-a-v="bottom" data-fill-color="3399ff" data-f-color="ebedef">NRO DOCUMENTO</th>'
            + '<th rowspan="3" class="text-center table-info" style="min-width:350px;" data-a-v="bottom" data-fill-color="3399ff" data-f-color="ebedef">CLIENTE</th>'
            + '<th rowspan="3" class="text-center table-info" style="min-width:350px;" data-a-v="bottom" data-fill-color="3399ff" data-f-color="ebedef">VENDEDOR</th>'
            + '<th rowspan="3" class="text-center table-info" data-a-v="bottom" data-fill-color="3399ff" data-f-color="ebedef">MONEDA</th>'
            + '<th rowspan="3" class="text-center table-info" data-a-v="bottom" data-fill-color="3399ff" data-f-color="ebedef">FECHA DE CONTRATO</th>'
            + '<th rowspan="3" class="text-center table-info" data-a-v="bottom" data-fill-color="3399ff" data-f-color="ebedef"> TOTAL CONTRATO</th>'           
            + '<th colspan="9" class="text-center table-info" data-a-v="bottom" data-fill-color="3399ff" data-f-color="ebedef">ESTADO</th>'
            + '</tr>'
            + '<tr>'
            + '<th colspan="4" class="text-center table-success" data-a-v="bottom" data-fill-color="2eb85c" data-f-color="ebedef">PAGADO</th>'
            + '<th rowspan="2" class="text-center table-success" data-a-v="bottom" data-fill-color="2eb85c" data-f-color="ebedef">TOTAL</th>'
            + '<th colspan="2" class="text-center table-danger" data-a-v="bottom" data-fill-color="e55353" data-f-color="ebedef">NO PAGADO</th>'
            + '<th colspan="2" class="text-center table-warning" data-a-v="bottom" data-fill-color="f9b115" data-f-color="ebedef">ANALISIS</th>'
            + '</tr>'
            + '<tr>'
            + '<th class="text-center table-success" data-a-v="bottom" data-fill-color="2eb85c" data-f-color="ebedef">CASH</th>'
            + '<th class="text-center table-success" data-a-v="bottom" data-fill-color="2eb85c" data-f-color="ebedef">MONTO</th>'
            + '<th class="text-center table-success" data-a-v="bottom" data-fill-color="2eb85c" data-f-color="ebedef">NRO LETRAS</th>'
            + '<th class="text-center table-success" data-a-v="bottom" data-fill-color="2eb85c" data-f-color="ebedef">ULTIMO PAGO</th>'
            + '<th class="text-center table-danger" data-a-v="bottom" data-fill-color="e55353" data-f-color="ebedef">MONTO</th>'
            + '<th class="text-center table-danger" data-a-v="bottom" data-fill-color="e55353" data-f-color="ebedef">NRO LETRAS</th>'
            + '<th class="text-center table-warning" data-a-v="bottom" data-fill-color="f9b115" data-f-color="ebedef">% PAGADO</th>'
            + '<th class="text-center table-warning" data-a-v="bottom" data-fill-color="f9b115" data-f-color="ebedef">DEVOLUCION</th>'
            + '</tr>';
        $("#theadReporte").html('').append(reporteCabecera);
        response.data.reporte.forEach((item) => {
            reporteCuerpo += '<tr>' +
                '<td>' + item.proyecto_nombrecorto + '</td>' +
                '<td>' + item.lote_nombre + '</td>' +
                '<td>' + item.proformauif_cliente_nrodocumento + '</td>' +
                '<td>' + item.proformauif_cliente_razonsocial + '</td>' +
                '<td>' + item.cotizacion_promotor_nombre + '</td>' +
                '<td>' + item.moneda_descripcioncorta + '</td>' +
                '<td>' + item.anexocontratocotizacion_fechapagoefectivo + '</td>' +
                '<td>' + formatearMonto(item.cotizacion_preciototal, item.moneda_descripcioncorta) + '</td>' +
                '<td>' + formatearMonto(item.cotizacion_montoinicial, item.moneda_descripcioncorta)  + '</td>' +
                '<td>' + formatearMonto(item.cuota_montopagado, item.moneda_descripcioncorta)  + '</td>' +
                '<td>' + item.cuota_cuotaspagadas + '</td>' +
                '<td>' + item.cuota_fechaultimopago + '</td>' +
                '<td>' + formatearMonto(item.cuota_montopagadototal, item.moneda_descripcioncorta) + '</td>' +
                '<td>' + formatearMonto(item.cuota_montopendiente, item.moneda_descripcioncorta)  + '</td>' +
                '<td>' + item.cuota_letrapendiente + '</td>' +
                '<td>' + item.cuota_porcentajepagado + '%' + '</td>' +
                '<td>' + formatearMonto(item.cuota_devolucion, item.moneda_descripcioncorta)  + '</td>' +
                '</tr>';

        });
        $("#tbodyReporte").html('').append(reporteCuerpo);
    }
}

const formatearMonto = (monto, moneda) => {
    if (monto == null) {
        return moneda == 'PEN' ? ('S/.' + parseFloat(0).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,')) : ('$' + parseFloat(0).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,'));
    } else {
        return moneda == 'PEN' ? ('S/.' + parseFloat(monto).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,')) : ('$' + parseFloat(monto).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,'));
    }
}
