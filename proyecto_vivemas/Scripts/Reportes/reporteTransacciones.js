var table;
$(document).ready(function () {
    cargarDataReporte();
});

$('#selectBancoDestino').on('select2:select', (e) => {
    cargarCuentasDestino();
});

document.getElementById('formReporteTransacciones').addEventListener("submit", (e) => {
    e.preventDefault();
    obtenerReporte();
});

document.getElementById('buttonEmitirReportePDF').addEventListener("click", (e) => {
    table.download("pdf", "reporte_transacciones.pdf", {
        orientation: "landscape", //set page orientation to portrait
        jsPDF: {
            unit: "in", //set units to inches
            format: [1700, 792]
        },
        title: "Reporte Transacciones", //add title to report
    });
});

document.getElementById('buttonEmitirReporteXLS').addEventListener("click", (e) => {
    table.download("xlsx", "reporte_transacciones.xlsx", { sheetName: "reporte" });
});

const cargarDataReporte = async () => {
    var opcion = {
        id: 0,
        text: 'TODOS'
    };
    var nuevaOpcionProyecto = new Option(opcion.text, opcion.id, false, false);
    $('#selectProyecto').append(nuevaOpcionProyecto).trigger('change');
    response = await axios.post('../Transacciones/CargarProyectos');
    $('#selectProyecto').select2({
        theme: 'bootstrap4',
        data: response.data
    });
    var nuevaOpcionTipoTransaccion = new Option(opcion.text, opcion.id, false, false);
    $('#selectTipoTransaccion').append(nuevaOpcionTipoTransaccion).trigger('change');
    response = await axios.post('../Transacciones/CargarTiposTransaccionSelect');
    $('#selectTipoTransaccion').select2({
        theme: 'bootstrap4',
        data: response.data
    });
    var nuevaOpcionMetodoPago = new Option(opcion.text, opcion.id, false, false);
    $('#selectMetodoPago').append(nuevaOpcionMetodoPago).trigger('change');
    response = await axios.post('../Transacciones/cargarMetodosPago');
    $('#selectMetodoPago').select2({
        theme: 'bootstrap4',
        data: response.data
    });
    var nuevaOpcionBanco = new Option(opcion.text, opcion.id, false, false);
    $('#selectBancoDestino').append(nuevaOpcionBanco).trigger('change');
    response = await axios.post('../Transacciones/cargarBancos');
    $('#selectBancoDestino').select2({
        theme: 'bootstrap4',
        data: response.data
    });
    cargarCuentasDestino();
}

const cargarCuentasDestino = async () => {
    var opcion = {
        id: 0,
        text: 'TODOS'
    };
    var nuevaOpcionCuenta = new Option(opcion.text, opcion.id, false, false);

    var idBanco = $('#selectBancoDestino').select2('data')[0].id;
    const response = await axios.post('../Transacciones/cargarCuentasBanco?idBanco=' + idBanco);
    $('#selectCtaDestino').html('');
    $('#selectCtaDestino').append(nuevaOpcionCuenta).trigger('change');
    $('#selectCtaDestino').select2({
        theme: 'bootstrap4',
        data: response.data
    });
}

const obtenerReporte = async () => {
    var data = new Object();
    data.fechaDesde = document.querySelector('#inputFechaDesde').value == '' ? '' : moment(document.querySelector('#inputFechaDesde').value, 'YYYY-MM-DD').format("DD/MM/YYYY");
    data.fechaHasta = document.querySelector('#inputFechaHasta').value == '' ? '' : moment(document.querySelector('#inputFechaHasta').value, 'YYYY-MM-DD').format("DD/MM/YYYY");
    data.idMetodoPago = $('#selectMetodoPago').select2('data')[0].id;

    data.idBancoDestino = $('#selectBancoDestino').select2('data')[0].id;
    data.idNroCuentaBancoDestino = $('#selectCtaDestino').select2('data')[0].id;
    data.idProyecto = $('#selectProyecto').select2('data')[0].id;
    data.idTipoTransaccion = $('#selectTipoTransaccion').select2('data')[0].id;

    const response = await axios.post('obtenerReporteTransacciones', JSON.stringify(data), {
        headers: {
            'Content-Type': 'application/json'
        }
    });
    if (response.data.flag == 1) {
        document.getElementById('buttonEmitirReportePDF').disabled = false;
        document.getElementById('buttonEmitirReporteXLS').disabled = false;
        table = new Tabulator("#reporte", {
            data: response.data.data,
            pagination: "local",
            paginationSize: 20,
            footerElement: '<p>TOTAL:' + response.data.montoTotal + '</p>',
            columns: [
                { title: "NRO TRANSACCION", field: "transaccion_numeracion" },
                { title: "NRO DOCUMENTO", field: "proformauif_cliente_nrodocumento" },
                { title: "RAZON SOCIAL", field: "proformauif_cliente_razonsocial" },
                { title: "PROYECTO", field: "proyecto_nombrecorto" },
                { title: "LOTE", field: "lote_nombre" },
                { title: "TIPO TRANSACCION", field: "tipotransaccion_descripcion" },
                { title: "METODO PAGO", field: "tipometodopago_descripcion" },
                { title: "FECHA DEPOSITO", field: "transaccion_fechadeposito" },
                { title: "BANCO", field: "banco_descripcion" },
                { title: "CUENTA", field: "cuentabanco_cuenta" },
                { title: "MONEDA", field: "moneda_descripcioncorta" },
                { title: "MONTO TOTAL", field: "transaccion_monto" },
                { title: "MONTO NETO", field: "transaccion_montonetototal" },
                { title: "DESCUENTO", field: "transaccion_montototaldescuento" },
            ]
        });
        /*var reporte = '<table id="tableReporte" style="border-collapse:collapse; width:100%; margin-bottom:1rem; color:#4f5d73; table-layout:fixed; width: 100%">' +
            '<thead class="">' +
            '<tr>' +
            '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid; width:120px">NRO TRANSACCION</th>' +
            '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid; width:120px">NRO DOCUMENTO</th>' +
            '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid; width:220px">RAZON SOCIAL</th>' +
            '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid; width:120px">TIPO TRANSACCION</th>' +
            '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid; width:120px">METODO PAGO</th>' +
            '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid; width:120px">FECHA DEPOSITO</th>' +
            '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid; width:120px">BANCO</th>' +
            '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid; width:150px">CUENTA</th>' +
            '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid; width:120px">MONEDA</th>' +
            '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid; width:120px">MONTO TOTAL</th>' +
            '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid; width:120px">MONTO NETO</th>' +
            '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid; width:120px">DESCUENTO</th>' +
            '</tr>' +
            '</thead><tbody>';
        response.data.data.forEach(item => {
            reporte += '<tr>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + item.transaccion_numeracion + '</td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + item.proformauif_cliente_nrodocumento + '</td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + item.proformauif_cliente_razonsocial + '</td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + item.tipotransaccion_descripcion + '</td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + item.tipometodopago_descripcion + '</td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + item.transaccion_fechadeposito + '</td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + item.banco_descripcion + '</td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + item.cuentabanco_cuenta + '</td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + item.moneda_descripcioncorta + '</td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + item.transaccion_monto + '</td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + item.transaccion_montonetototal + '</td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + item.transaccion_montototaldescuento + '</td>' +
                '</tr>';
        });
        reporte += '</tbody>' + '</table>';
        $("#reporte").html('');
        $("#reporte").html(reporte);*/
    } else {
        toastr["error"]("Ocurrio un error, comuniquese con el area de sistemas")
    }

};