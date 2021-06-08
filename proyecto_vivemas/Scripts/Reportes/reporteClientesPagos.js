$(document).ready(() => {
    cargarProyectos();
    //console.log($('#selectProyecto').select2('data')[0].id);
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
$('#selectProyecto').on('select2:select', (e) => {
    limpiarBusqueda();
});
document.getElementById('formReportePagosCliente').addEventListener("submit", (e) => {
    e.preventDefault();
    obtenerReporte();
});
document.getElementById('buttonEmitirReportePDF').addEventListener("click", (e) => {
    var element = document.getElementById('reporte');
    html2pdf(element);
});
document.getElementById('buttonEmitirReporteXLS').addEventListener("click", (e) => {
    tableToExcel('tableReporte', 'Prueba Reporte');
});

const cargarProyectos = async () => {
    var response = await axios.post('../Proyectos/cargarProyectos');
    $('#selectProyecto').select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
}

const limpiarBusqueda = () => {
    document.querySelector("#inputDocumentoCliente").value = "";
    document.querySelector("#inputNroContrato").value = "";
    document.querySelector("#inputIdContrato").value = "";
    document.querySelector("#inputLote").value = "";
}

const obtenerReporte = async () => {
    if (document.querySelector("#inputIdContrato").value == "") {
        toastr["warning"]("Debe buscar un cliente primero");
    } else {
        var fechaInicio = document.querySelector("#inputFechaDesde").value == "" ? "" : moment(document.querySelector('#inputFechaDesde').value, 'YYYY-MM-DD').format("DD/MM/YYYY");
        var fechaFin = document.querySelector("#inputFechaHasta").value == "" ? "" : moment(document.querySelector('#inputFechaHasta').value, 'YYYY-MM-DD').format("DD/MM/YYYY");
        const response = await axios.post('obtenerReportePagosCliente?idContrato=' + document.querySelector("#inputIdContrato").value + "&fechaInicio=" + fechaInicio + "&fechaFin=" + fechaFin);
        if (response.data.flag == 1) {
            document.getElementById('buttonEmitirReportePDF').disabled = false;
            document.getElementById('buttonEmitirReporteXLS').disabled = false;           
            var reporte = '<table id="tableReporte" style="border-collapse:collapse; width:100%; margin-bottom:1rem; color:#4f5d73;">' +
                '<thead class="">' +
                '<tr><th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid; text-align: center;" colspan="7">REPORTE DE PAGOS POR CLIENTES</th></tr>' +
                '<tr><th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;" colspan="2">PF:</th><td style="border-color:#758297; border-top: 1px solid #dee2e6;" colspan="5">' + response.data.cabecera.contrato_numeracion + '</td></tr>' +
                '<tr><th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;" colspan="2">PROYECTO:</th><td style="border-color:#758297; border-top: 1px solid #dee2e6;" colspan="5">' + response.data.cabecera.proyecto_nombre + '</td></tr>' +
                '<tr><th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;" colspan="2">LOTE:</th><td style="border-color:#758297; border-top: 1px solid #dee2e6;" colspan="5">' + response.data.cabecera.lote_nombre + '</td></tr>' +
                '<tr><th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;" colspan="2">CLIENTE:</th><td style="border-color:#758297; border-top: 1px solid #dee2e6;" colspan="5">' + response.data.cabecera.proformauif_cliente_razonsocial + '</td></tr>' +
                '<tr><th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;" colspan="2">DNI:</th><td style="border-color:#758297; border-top: 1px solid #dee2e6;" colspan="5">' + response.data.cabecera.proformauif_cliente_nrodocumento + '</td></tr>' +
                '<tr><th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;" colspan="2">CELULAR:</th><td style="border-color:#758297; border-top: 1px solid #dee2e6;"colspan="5">' + response.data.cabecera.proformauif_cliente_nrocontacto + '</td></tr>' +
                '<tr><th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;" colspan="2">CODIGO:</th><td style="border-color:#758297; border-top: 1px solid #dee2e6;" colspan="5">' + response.data.cabecera.codigo + '</td></tr>' +
                '<tr><th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;" colspan="2">MONEDA:</th><td style="border-color:#758297; border-top: 1px solid #dee2e6;" colspan="5">' + response.data.cabecera.moneda_descripcioncorta + '</td></tr>' +
                '<tr><th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;" colspan="2">VENDEDOR:</th><td style="border-color:#758297; border-top: 1px solid #dee2e6;" colspan="5">' + response.data.cabecera.datosusuario_razonsocial + '</td></tr>' +
                '<tr>' +
                '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;">DOC</th>' +
                '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;">FECHA VCTO</th>' +
                '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;">MONTO CUOTA</th>' +
                '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;">MONTO PAGO</th>' +
                '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;">MONTO DESC.</th>' +
                '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;">SALDO</th>' +
                '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;">F.PAGO</th>' +
                '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;">ESTADO</th>' +
                '</tr>' +
                '</thead>' +
                '<tbody>';
            var flagCuota = '';
            var montoCuota = 0;
            response.data.detalle.forEach(item => {

                reporte = reporte + '<tr>' +
                    '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + item.cuota_numeracion + '</td>' +
                    '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + item.cuota_fechavencimiento + '</td>' +
                    '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + item.cuota_monto + '</td>';

                if (item.transaccion_monto == null) {
                    reporte = reporte + '<td style="border-color:#758297; border-top: 1px solid #dee2e6;"></td>';
                } else {
                    reporte = reporte + '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + item.transaccion_monto + '</td>';
                }
                if (item.transaccion_monto == null) {
                    reporte = reporte + '<td style="border-color:#758297; border-top: 1px solid #dee2e6;"></td>';
                } else {
                    reporte = reporte + '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + item.pago_montodescuento + '</td>';
                }
                
                if (flagCuota != item.cuota_numeracion) {
                    flagCuota = item.cuota_numeracion;
                    montoCuota = parseFloat(item.cuota_monto - item.transaccion_monto - item.pago_montodescuento).toFixed(2);
                    if (item.transaccion_monto == null) {
                        reporte = reporte + '<td style="border-color:#758297; border-top: 1px solid #dee2e6;"></td>';
                    } else {
                        reporte = reporte + '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + montoCuota + '</td>';
                    }

                } else {
                    console.log(montoCuota);
                    montoCuota = parseFloat(montoCuota - item.transaccion_monto - item.pago_montodescuento).toFixed(2);
                    if (item.transaccion_monto == null) {
                        reporte = reporte + '<td style="border-color:#758297; border-top: 1px solid #dee2e6;"></td>';
                    } else {
                        reporte = reporte + '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + montoCuota + '</td>';
                    }
                }

                if (item.fecha_pago == null) {
                    reporte = reporte + '<td style="border-color:#758297; border-top: 1px solid #dee2e6;"></td>';
                } else {
                    reporte = reporte + '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + item.fecha_pago + '</td>';
                }
                reporte = reporte + '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + item.cuota_estado + '</td>' +
                    '</tr>';
            });
            reporte = reporte + '</tbody>' +
                '<tfoot>' +
                '<tr>' +
                '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;" colspan="2">TOTAL CANCELADO: </th>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + response.data.cabecera.montocancelado + '</td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;"></td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;"></td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;"></td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;"></td>' +
                '</tr>' +
                '<tr>' +
                '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;" colspan="2">TOTAL POR CANCELAR: </th>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + response.data.cabecera.montopendiente + '</td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;"></td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;"></td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;"></td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;"></td>' +
                '</tr>' +
                '<tr>' +
                '<th style="color:#fff; background-color:#636f83; border-color:#758297; border-top: 1px solid;" colspan="2">PORCENTAJE CANCELADO: </th>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">' + response.data.cabecera.porcentaje_pagado + '</td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;"></td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;"></td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;"></td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;"></td>' +
                '</tr>' +
                '<tr>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;" colspan="6">* ULTIMA FECHA DE PAGO:' + response.data.cabecera.fecha_ultimacuota + ', CANTIDAD DE CUOTAS: ' + response.data.cabecera.nro_cuotas + '</td>' +
                '<td style="border-color:#758297; border-top: 1px solid #dee2e6;">F. IMPRESION: ' + response.data.cabecera.fecha_impresion + '</td>' +
                '</tr>' +
                '</tfoot>' +
                '</table>';
            $("#reporte").html('');
            $("#reporte").html(reporte);
        } else {
            toastr["error"]("Ocurrio un error en el servidor contactese con el area de sistemas");
        }
    }

}