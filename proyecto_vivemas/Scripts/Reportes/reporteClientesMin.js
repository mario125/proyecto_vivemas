
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

document.getElementById('formReporteClientesMin').addEventListener('submit', (event) => {
    event.preventDefault();
    obtenerReporte();
});

document.getElementById('buttonEmitirReportePDF').addEventListener("click", (e) => {
    const doc = new jsPDF();
    doc.autoTable({
        html: '#tableReporte',
        showHead: 'firstPage',
        showFoot: 'lastPage'
    });
    doc.save('reporteCliente.pdf');    
});

document.getElementById('buttonEmitirReporteXLS').addEventListener("click", (e) => {
    TableToExcel.convert(document.getElementById("tableReporte"), {
        name: "reporteCliente.xlsx",
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
    if (document.querySelector("#inputIdContrato").value == "") {
        toastr["warning"]("Debe buscar un cliente primero");
    } else {
        const response = await axios.post('obtenerReportePagosCliente?idContrato=' + document.querySelector("#inputIdContrato").value);
        if (response.data.flag == 1) {            
            document.getElementById('buttonEmitirReportePDF').disabled = false;
            document.getElementById('buttonEmitirReporteXLS').disabled = false;

            var reporteCabecera = '';
            var reporteCuerpo = '';
            var reportePie = '';

            reporteCabecera += '<tr class="row bg-info text-light">' +
                '<th class="col-3" data-fill-color="3399ff" data-f-color="ebedef">Codigo</th>' +
                '<td class="col-9" colspan="3" data-fill-color="3399ff" data-f-color="ebedef">' + response.data.cabecera.codigo + '</td>' +
                '</tr>' +
                '<tr class="row bg-info text-light">' +
                '<th class="col-3" data-fill-color="3399ff" data-f-color="ebedef">Contrato</th>' +
                '<td class="col-9" colspan="3" data-fill-color="3399ff" data-f-color="ebedef">' + response.data.cabecera.contrato_numeracion + '</td>' +
                '</tr>' +
                '<tr class="row bg-info text-light">' +
                '<th class="col-3" data-fill-color="3399ff" data-f-color="ebedef">Proyecto</th>' +
                '<td class="col-9" colspan="3" data-fill-color="3399ff" data-f-color="ebedef">' + response.data.cabecera.proyecto_nombre + '</td>' +
                '</tr>' +
                '<tr class="row bg-info text-light">' +
                '<th class="col-3" data-fill-color="3399ff" data-f-color="ebedef">Lote</th>' +
                '<td class="col-9" colspan="3" data-fill-color="3399ff" data-f-color="ebedef">' + response.data.cabecera.lote_nombre + '</td>' +
                '</tr>' +
                '<tr class="row bg-info text-light">' +
                '<th class="col-3" data-fill-color="3399ff" data-f-color="ebedef">Nro de Documento</th>' +
                '<td class="col-9" colspan="3" data-fill-color="3399ff" data-f-color="ebedef">' + response.data.cabecera.proformauif_cliente_nrodocumento + '</td>' +
                '</tr>' +
                '<tr class="row bg-info text-light">' +
                '<th class="col-3" data-fill-color="3399ff" data-f-color="ebedef">Cliente</th>' +
                '<td class="col-9" colspan="3" data-fill-color="3399ff" data-f-color="ebedef">' + response.data.cabecera.proformauif_cliente_razonsocial + '</td>' +
                '</tr>' +
                '<tr class="row bg-info text-light">' +
                '<th class="col-3" data-fill-color="3399ff" data-f-color="ebedef">Promotor</th>' +
                '<td class="col-9" colspan="3" data-fill-color="3399ff" data-f-color="ebedef">' + response.data.cabecera.datosusuario_razonsocial + '</td>' +
                '</tr>' +
                '<tr class="row bg-info text-light">' +
                '<th class="col-3" data-fill-color="3399ff" data-f-color="ebedef">Cuota</th>' +
                '<th class="col-3" data-fill-color="3399ff" data-f-color="ebedef">Fecha Vencimiento</th>' +
                '<th class="col-3" data-fill-color="3399ff" data-f-color="ebedef">Monto</th>' +
                '<th class="col-3" data-fill-color="3399ff" data-f-color="ebedef">Estado</th>' +
                '</tr>';
            $("#theadReporte").html('').append(reporteCabecera);

            response.data.detalle.forEach((item) => {
                reporteCuerpo += '<tr class="row">' +
                    '<td class="col-3">' + item.cuota_numeracion +'</td>' +
                    '<td class="col-3">' + (item.fecha_pago == null ? '' : item.fecha_pago) + '</td>' +
                    '<td class="col-3">' + (item.transaccion_monto == null ? '0.00' : item.transaccion_monto) + '</td>' +
                    '<td class="col-3">' + item.cuota_estado + '</td>' +
                    '</tr>';

            });
            $("#tbodyReporte").html('').append(reporteCuerpo);

            reportePie += '<tr class="row bg-info text-light">' +
                '<th class="col-3" data-fill-color="3399ff" data-f-color="ebedef">Monto Cancelado</th>' +
                '<td class="col-9" colspan="3" data-fill-color="3399ff" data-f-color="ebedef">' + response.data.cabecera.montocancelado + '</td>' +
                '</tr>' +
                '<tr class="row bg-info text-light">' +
                '<th class="col-3" data-fill-color="3399ff" data-f-color="ebedef">Monto Pendiente</th>' +
                '<td class="col-9" colspan="3" data-fill-color="3399ff" data-f-color="ebedef">' + response.data.cabecera.pendientehoy + '</td>' +
                '</tr>' +
                '<tr class="row bg-info text-light">' +
                '<th class="col-3" data-fill-color="3399ff" data-f-color="ebedef">Porcentaje</th>' +
                '<td class="col-9" colspan="3" data-fill-color="3399ff" data-f-color="ebedef">' + response.data.cabecera.porcentaje_pagado + '</td>' +
                '</tr>';
            $("#tfootReporte").html('').append(reportePie);


            /*table = $("#gridContainer").dxDataGrid({
                dataSource: response.data.detalle,
                showBorders: true,
                export: {
                    enabled: true,
                    allowExportSelectedData: true
                },
                onExporting: function (e) {
                    var workbook = new ExcelJS.Workbook();
                    var worksheet = workbook.addWorksheet('Employees');

                    DevExpress.excelExporter.exportDataGrid({
                        component: e.component,
                        worksheet: worksheet,
                        autoFilterEnabled: true
                    }).then(function () {
                        workbook.xlsx.writeBuffer().then(function (buffer) {
                            saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'Employees.xlsx');
                        });
                    });
                    e.cancel = true;
                },
                columns: [
                    { dataField: "cuota_numeracion", caption: "Numeracion" },
                    { dataField: "cuota_fechavencimiento", caption: "Fecha" },
                    { dataField: "cuota_estado", caption: "estado"}
                ]
            }).dxDataGrid('instance');;*/
        }
    }
}