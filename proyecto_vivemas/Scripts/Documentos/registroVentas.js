var table = new Tabulator("#tableDocumentos", {
    layout: "fitDataStretch",
    ajaxURL: "CargarRegistroVentas",
    ajaxConfig: "POST",
    placeholder: "Cargando...",
    height: "800px",
    pagination: "local",
    paginationSize: 20,
    paginationSizeSelector: [3, 6, 8, 10, 20, 50, 100],
    columns: [
        { title: "id", field: "documentoventa_id", visible: false },
        { title: "idNota", field: "notaId", visible: false },
        { title: "Tipo", field: "tipodocumentoventa_descripcion" },
        { title: "Documento", field: "serie", headerFilter: "input", headerFilterPlaceholder: "Documento" },
        { title: "DNI/RUC", field: "documentoventa_cliente_nrodocumento", headerFilter: "input", headerFilterPlaceholder: "DNI/RUC" },
        { title: "Cliente", field: "documentoventa_cliente_nombre", headerFilter: "input", headerFilterPlaceholder: "Cliente" },
        { title: "F. Emision", field: "documentoventa_fechaemision", headerFilter: "input", headerFilterPlaceholder: "F. Emision" },
        { title: "Estado", field: "estadodocumento_descripcion", headerFilter: "input", headerFilterPlaceholder: "Estado" },
        { title: "Estado Sunat", field: "documentoventa_estadosunat" },
        { title: "Serie Referencia", field: "serieReferencia", headerFilter: "input", headerFilterPlaceholder: "Serie Referencia" },
        { title: "Total", field: "documentoventa_total" },
        {
            title: " Reimpresion", field: "documentoventa_id", formatter: function (cell, formatterParams, onRendered) {
                var row = cell.getRow();
                return '<button class=" btn btn-primary btn-sm" onclick="reimprimirDocumento(' + cell.getValue() + ')">Reimprimir</button>'
            }
        },
        {
            title: " Reimpresion Nota", field: "documentoventa_id", formatter: function (cell, formatterParams, onRendered) {
                var row = cell.getRow();
                if (row._row.data.estadodocumento_descripcion != 'ANULADO') {
                    return '';
                } else {
                    return '<button class=" btn btn-danger btn-sm" onclick="reimprimirNota(' + row._row.data.notaId + ')">Reimprimir Nota</button>'
                }
            }
        },
        {
            title: "Reenvio", field: "documentoventa_id", formatter: function (cell, formatterParams, onRendered) {
                var row = cell.getRow();
                if (row._row.data.documentoventa_estadosunat == '0' || row._row.data.estadodocumento_descripcion == 'ANULADO') {
                    return '';
                } else {
                    return '<button class=" btn btn-success btn-sm" onclick="reenviarSunat(' + cell.getValue() + ')">Reenviar sunat</button>'
                }
            }
        },
        {
            title: "Anulacion", field: "documentoventa_id", formatter: function (cell, formatterParams, onRendered) {
                var row = cell.getRow();
                if (row._row.data.estadodocumento_descripcion == 'ANULADO') {
                    return '';
                } else {                    
                    return '<button class=" btn btn-danger btn-sm" onclick="anularDocumento(' + cell.getValue() + ', \'' + row._row.data.serie + '\')">Anular</button>'
                }

            }
        }
    ],
    rowFormatter: (row) => {
        var data = row.getData();
        //console.log(row.getData());

        if (data.estadodocumento_descripcion == 'CON ERRORES') {
            row.getElement().style.backgroundColor = "#c71a1a";
        }

    }
});

document.getElementById('buttonConfirmarAnulacion').addEventListener("click", (e) => {
    if (document.querySelector("#inputMotivo").value == '') {
        $("#inputMotivo").addClass("is-invalid").removeClass("is-valid");
    } else {
        $("#inputMotivo").addClass("is-valid").removeClass("is-invalid");
        cargarModelo();
    }
});

document.getElementById('buttonAnularDocumento').addEventListener("click", (e) => {
    document.querySelector("#buttonAnularDocumento").disabled = true;
    realizarAnulacion();
});

const cargarModelo = async () => {
    var documentoId = document.querySelector("#inputDocumentoId").value;
    var tipoanulacion;
    if (document.querySelector("#checktipo").checked) {
        tipoanulacion = 1;
    } else {
        tipoanulacion = 2;
    }
    const response = await axios.post('../Reportes/cargarModeloNotaCredito?documentoId=' + documentoId + '&tipoAnulacion=' + tipoanulacion + '&motivo=' + document.querySelector("#inputMotivo").value)
    if (response.data.flag == 1) {
        console.log(response.data);
        document.querySelector("#documento_empresa_nombre").innerHTML = response.data.data.documento_empresa_nombre;
        document.querySelector("#documento_empresa_direccion").innerHTML = "<b>Domicilio Fiscal:</b>" + response.data.data.documento_empresa_direccion;
        document.querySelector("#documento_empresa_numeroContacto").innerHTML = "<b>Telefono:</b>" + response.data.data.documento_empresa_numeroContacto;
        document.querySelector("#documento_empresa_correo").innerHTML = "<b>Correo:</b>" + response.data.data.documento_empresa_correo;
        document.querySelector("#documento_empresa_documento").innerHTML = "RUC: " + response.data.data.documento_empresa_documento;
        document.querySelector("#documento_descripcion").innerHTML = response.data.data.documento_descripcion;
        document.querySelector("#documento_serie").innerHTML = response.data.data.documento_serie;
        document.querySelector("#documento_cliente_nombre").innerHTML = ": " + response.data.data.documento_cliente_nombre;
        document.querySelector("#documento_cliente_nroDocumento").innerHTML = ": " + response.data.data.documento_cliente_nroDocumento;
        document.querySelector("#documento_cliente_direccion").innerHTML = ": " + response.data.data.documento_cliente_direccion;
        document.querySelector("#documento_fechaEmision").innerHTML = ": " + response.data.data.documento_fechaEmision;
        document.querySelector("#documento_moneda_descripcion").innerHTML = ": " + response.data.data.documento_moneda_descripcion;
        document.querySelector("#documento_montoletras").innerHTML = "SON: " + response.data.data.documento_montoletras;
        document.querySelector("#documento_fechaemisionreferencia").innerHTML = ": " + response.data.data.documento_fechaemisionreferencia;
        document.querySelector("#documento_documentoreferencia").innerHTML = ": " + response.data.data.documento_documentoreferencia;
        document.querySelector("#documento_motivo").innerHTML = ": " + response.data.data.documento_motivo;
        document.querySelector("#documento_tiponota").innerHTML = "<b>" + response.data.data.documento_tiponota + "</b>";
        crearQrr(response.data.data.documento_qrcodeValue);
        $(".monedaSimbolo").append(response.data.data.moneda_simbolo);
        document.querySelector("#documento_subtotal").innerHTML = response.data.data.documento_subtotal;
        document.querySelector("#documento_igv").innerHTML = response.data.data.documento_igv;
        $(".documentoTotal").append(response.data.data.documento_total);
        document.querySelector("#transaccion_fechadeposito").innerHTML = response.data.data.transaccion_fechadeposito;
        document.querySelector("#transaccion_nrooperacion").innerHTML = response.data.data.transaccion_nrooperacion;
        document.querySelector("#transaccion_banco").innerHTML = response.data.data.transaccion_banco;
        document.querySelector("#transaccion_cuenta").innerHTML = response.data.data.transaccion_cuenta;
        document.querySelector("#documento_digestValue").innerHTML = response.data.data.documento_digestValue;
        var detalle = "";
        response.data.data.detalleVenta.forEach((item) => {
            detalle += ' <tr>' +
                '<td class="text-right">' + item.documentoDetalle_cantidad + '</td>' +
                '<td class="text-center">' + item.documentoDetalle_codigo + '</td>' +
                '<td class="text-left">' + item.documentoDetalle_descripcion + '</td>' +
                '<td class="text-center">' + item.documentoDetalle_unidad + '</td>' +
                '<td class="text-right">' + item.documentoDetalle_valorUnitario + '</td>' +
                '<td class="text-right">' + item.documentoDetalle_total + '</td>' +
                '</tr>';
        });
        $("#tbodyDetalles").append(detalle);
        $("#modalConfirmacionAnulacion").modal({
            keyboard: false,
            backdrop: false
        });
    } else {
        toastr["error"]("Ocurrio un problema, por favor comuniquese con el area de sistemas")
    }
}

const crearQrr = (qrValue) => {
    qr = new QRious({
        element: document.getElementById('qr-code'),
        size: 100,
        value: qrValue
    });
}

const reimprimirDocumento = (documentoId) => {
    window.open('../Reportes/ReimprimirDocumentoElectronico?documentoId=' + documentoId, '_blank');
}

const reimprimirNota = (documentoId) => {
    window.open('../Reportes/imprimirNotaCredito?documentoId=' + documentoId, '_blank');
}

const reenviarSunat = async (documentoId) => {
    const response = await axios.post('reenviarSunat?documentoId=' + documentoId);
    if (response.data.flag) {
        location.reload();
    } else {
        location.reload();
    }
}

const anularDocumento = (documentoId, serie) => {
    document.querySelector("#inputDocumentoId").value = documentoId;
    document.querySelector("#serieDocumento").value = serie;
    document.querySelector("#inputMotivo").value = "";
    document.querySelector("#checktipo").checked = false;
    $("#inputMotivo").removeClass("is-invalid").removeClass("is-valid");
    $("#modalAnulacionDocumento").modal({
        keyboard: false,
        backdrop: false
    });
}

const realizarAnulacion = async () => {
    var documentoId = document.querySelector("#inputDocumentoId").value;
    var tipoanulacion;
    if (document.querySelector("#checktipo").checked) {
        tipoanulacion = 1;
    } else {
        tipoanulacion = 2;
    }
    const response = await axios.post('anularDocumento?documentoId=' + documentoId + '&tipoAnulacion=' + tipoanulacion + '&motivo=' + document.querySelector("#inputMotivo").value);
    if (response.data.flag != 0) {
        window.open('../Reportes/imprimirNotaCredito?documentoId=' + response.data.notaCreditoId, '_blank');
        location.reload();
    } else {
        toastr["error"]("hubo un error en la emision del documento comuniquese con el area de sistemas");
    }
}