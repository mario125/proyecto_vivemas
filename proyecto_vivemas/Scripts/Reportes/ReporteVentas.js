var table;

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
            document.querySelector("#inputNroContrato").value = ui.item.numeracion;
            document.querySelector("#inputIdContrato").value = ui.item.id;
            document.querySelector("#inputDocumentoCliente").value = ui.item.nombre;
            //$("#inputProductoId").val(ui.item.producto_id);
        },
        delay: 300
    });
    $("#inputDocumentoCliente").autocomplete({
        source: function (request, response) {
            $.getJSON("../Clientes/buscarClientesContrato",
                {
                    term: request.term,
                    param1: $('#selectProyecto').select2('data')[0].id
                },
                response
            );
        },
        delay: 300,
        select: (event, ui) => {
            document.querySelector("#inputNroContrato").value = ui.item.numeracion;
            document.querySelector("#inputIdContrato").value = ui.item.id;
            document.querySelector("#inputLote").value = ui.item.lote;
        }
    });
    $("#inputNroContrato").autocomplete({
        source: function (request, response) {
            $.getJSON("../Clientes/buscarClientesContrato2",
                {
                    term: request.term,
                    param1: $('#selectProyecto').select2('data')[0].id
                },
                response
            );
        },
        delay: 300,
        select: (event, ui) => {
            event.preventDefault();
            document.querySelector("#inputNroContrato").value = ui.item.numeracion;
            document.querySelector("#inputIdContrato").value = ui.item.id;
            document.querySelector("#inputDocumentoCliente").value = ui.item.cliente;
            document.querySelector("#inputLote").value = ui.item.lote;
        }
    });
});

document.getElementById('inputDocumentoCliente').addEventListener("keyup", (e) => {
    if (document.querySelector("#inputDocumentoCliente").value == "") {
        limpiarBusqueda();
    }
});

document.getElementById('inputNroContrato').addEventListener("keyup", (e) => {
    if (document.querySelector("#inputNroContrato").value == "") {
        limpiarBusqueda();
    }
});

document.getElementById('inputLote').addEventListener("keyup", (e) => {
    if (document.querySelector("#inputLote").value == "") {
        limpiarBusqueda();
    }
    //limpiarClienteCambioLote();
});

document.getElementById('formReporteVentas').addEventListener('submit', (event) => {
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
    doc.save('reporteVentas.pdf');
});

document.getElementById('buttonEmitirReporteXLS').addEventListener("click", (e) => {
    TableToExcel.convert(document.getElementById("tableReporte"), {
        name: "reporteVentas.xlsx",
        sheet: {
            name: "Reporte"
        }
    });
});

$('#selectProyecto').on('select2:select', (e) => {
    limpiarBusqueda();
});

const limpiarBusqueda = () => {
    document.querySelector("#inputDocumentoCliente").value = "";
    document.querySelector("#inputNroContrato").value = "";
    document.querySelector("#inputIdContrato").value = "";
    document.querySelector("#inputLote").value = "";
}

const cargarProyectos = async () => {
    var response = await axios.post('../Proyectos/cargarProyectos');
    $('#selectProyecto').select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
}

const obtenerReporte = async () => {

    var data = new Object();
    data.idContrato = document.querySelector("#inputIdContrato").value;
    data.idProyecto = $('#selectProyecto').select2('data')[0].id;
    data.fechaDesde = document.querySelector("#inputFechaDesde").value == '' ? document.querySelector("#inputFechaDesde").value : moment(document.querySelector("#inputFechaDesde").value, 'YYYY-MM-DD').format("DD/MM/YYYY");
    data.fechaHasta = document.querySelector("#inputFechaHasta").value == '' ? document.querySelector("#inputFechaHasta").value : moment(document.querySelector("#inputFechaHasta").value, 'YYYY-MM-DD').format("DD/MM/YYYY");
       
    const response = await axios.post('obtenerReporteVentas', JSON.stringify(data), {
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
            '<th data-fill-color="3399ff" data-f-color="ebedef" style="width:115px;" class="bg-info">DOCUMENTO</th>' +
            '<th data-fill-color="3399ff" data-f-color="ebedef" style="width:115px;" class="bg-info">NOTA CREDITO</th>' +
            '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">NRO DOCUMENTO</th>' +
            '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">RAZON SOCIAL</th>' +
            '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">TOTAL</th>' +
            '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">MONEDA</th>' +
            '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">FECHA EMISION</th>' +
            '<th data-fill-color="3399ff" data-f-color="ebedef" class="bg-info">FECHA NOTA DE CREDITO</th>' +
            '</tr>';
        $("#theadReporte").html('').append(reporteCabecera);
        response.data.dataReporte.forEach((item) => {
            var fecha = item.notacredito_fechaemision == null ? "" : item.notacredito_fechaemision;
            reporteCuerpo += '<tr>' +
                '<td>' + item.documentoEmision + '</td>' +
                '<td>' + (item.notacreditoanulacion == null ? '' : item.notacreditoanulacion) + '</td>' +
                '<td>' + (item.documentoventa_cliente_nrodocumento == null ? '' : item.documentoventa_cliente_nrodocumento) + '</td>' +
                '<td>' + (item.documentoventa_cliente_nombre == null ? '' : item.documentoventa_cliente_nombre) + '</td>' +
                '<td>' + item.documentoventa_total + '</td>' +
                '<td>' + item.documentoventa_moneda_codigo + '</td>' +
                '<td>' + item.documentoventa_fechaemision + '</td>' +
                '<td>' + fecha + '</td>' +
                '</tr>'
        });
        $("#tbodyReporte").html('').append(reporteCuerpo);
    } else {
        toastr["error"]("ocurrio un error en el sistema, comuniquese con el area de sistemas");
    }   
}