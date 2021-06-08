/** */
var table = new Tabulator("#tableTransacciones", {
    index: "tipotransaccion_id",
    layout: "fitDataStretch",
    ajaxURL: "CargarTiposTransaccion",
    ajaxConfig: "POST",
    placeholder: "Cargando...",
    height: "900px",
    pagination: "local",
    paginationSize: 20,
    paginationSizeSelector: [3, 6, 8, 10, 20, 50],
    renderComplete: () => {
        abrirTransaccionesDetalle(4);
    },
    columns: [
        { title: "id", field: "tipotransaccion_id", visible: false },
        {
            title: "", field: "tipotransaccion_id", formatter: function (cell, formatterParams, onRendered) {
                var row = cell.getRow();
                return '<button class=" btn btn-success btn-sm" onclick="abrirTransaccionesDetalle(' + cell.getRow().getIndex() + ')">Mostrar detalle</button>'
            }
        },
        { title: "tipo transaccion", field: "tipotransaccion_descripcion" }
    ]
});
var subTable;
const hoy = new Date();
/** */
$(document).ready(() => {
    cargarDataPagos();
    //abrirTransaccionesDetalle(4)
});

$('#selectBancoDestino').on('select2:select', (e) => {
    cargarCuentasDestino();
});

document.getElementById('buttonPagar').addEventListener("click", () => {
    actualizarTransaccion();
});

document.getElementById('buttonCerrar').addEventListener("click", () => {
    $('#formPagos').trigger("reset");
    cargarDataPagos();
});

document.getElementById('buttonAnularTransaccion').addEventListener("click", () => {
    anulacionTransaccion(document.querySelector("#inputTransaccionId").value, document.querySelector("#inputTipoTransaccion").value);
});

document.getElementById('formObservaciones').addEventListener("submit", (e) => {
    actualizarObservacion();
});

document.getElementById('formFechaEmision').addEventListener("submit", (e) => {
    e.preventDefault();
    emitirDocumentoElectronicoFechaEmision();
});

document.getElementById('buttonEmitirDocumento').addEventListener("click", () => {
    if (document.querySelector("#inputAccionHidden").value == 1) {
        document.getElementById('buttonEmitirDocumento').disabled = true;
        window.open('../Reportes/EmitirDocumentoElectronico?transaccionId=' + document.querySelector("#inputTransaccionIdHiddenConfirmacion").value, '_blank');
        setTimeout(() => { location.reload(); }, 3000);
    } else if (document.querySelector("#inputAccionHidden").value == 2) {
        document.getElementById('buttonEmitirDocumento').disabled = true;
        window.open('../Reportes/EmitirDocumentoElectronicoFechaEmision?transaccionId=' + document.querySelector("#inputTransaccionIdHiddenConfirmacion").value + '&fechaEmisionDet=' + document.querySelector("#fechaEmision").value, '_blank');
        setTimeout(() => { location.reload(); }, 3000);
    }
        
});

const abrirTransaccionesDetalle = (rowIndex) => {
    table.getData().forEach((item) => {
        var roww = table.getRow(item.tipotransaccion_id);
        if ($('#contenedor' + roww.getData().tipotransaccion_id).length !== 0) {
            $('#contenedor' + roww.getData().tipotransaccion_id).remove();
        }
    });
    var row = table.getRow(rowIndex);
    if ($('#contenedor' + row.getData().tipotransaccion_id).length !== 0) {

        $('#contenedor' + row.getData().tipotransaccion_id).remove();
    } else {
        var holderEl = document.createElement("div");
        holderEl.setAttribute("id", "contenedor" + row.getData().tipotransaccion_id);
        var tableEl = document.createElement("div");

        holderEl.style.boxSizing = "border-box";
        holderEl.style.padding = "10px 30px 10px 10px";
        holderEl.style.borderTop = "1px solid #333";
        holderEl.style.borderBotom = "1px solid #333";
        holderEl.style.background = "#ddd";

        tableEl.style.border = "1px solid #333";

        holderEl.appendChild(tableEl);

        row.getElement().appendChild(holderEl);

        var headerMenu = function () {
            var menu = [];
            var columns = subTable.getColumns();

            for (let column of columns) {

                //create checkbox element using font awesome icons
                let icon = document.createElement("i");
                icon.classList.add(column.isVisible() ? "cil-minus" : "cil-plus");

                //build label
                let label = document.createElement("span");
                let title = document.createElement("span");

                title.textContent = " " + column.getDefinition().title;

                label.appendChild(icon);
                label.appendChild(title);

                //create menu item
                menu.push({
                    label: label,
                    action: function (e) {
                        console.log("entro");
                        //prevent menu closing
                        e.stopPropagation();
                        e.preventDefault();

                        //toggle current column visibility
                        column.toggle();

                        //change menu item icon
                        if (column.isVisible()) {
                            icon.classList.remove("cil-minus");
                            icon.classList.add("cil-plus");
                        } else {
                            icon.classList.remove("cil-plus");
                            icon.classList.add("cil-minus");
                        }
                    }
                });
            }
            return menu;
        };

        subTable = new Tabulator(tableEl, {
            index: "transaccion_id",
            ajaxURL: "CargarTransaccionesPorTipo?tipoTransaccion=" + row.getData().tipotransaccion_id,
            ajaxConfig: "POST",
            placeholder: "Registros Vacios",
            pagination: "local",
            paginationSize: 10,
            columns: [
                /*{ title: "id", field: "transaccion_id", visible: false },*/
                {
                    title: '<button class="btn btn-primary" onclick="procesarSeleccionados()">Procesar seleccionados</button>',
                    columns: [
                        {
                            title: '<input type="checkbox" class="select-all-row" id="checktodos" onclick="seleccionarTodos()" />',
                            formatter: function (cell, formatterParams, onRendered) {
                                return '<input type="checkbox" class="select-row" onclick="seleccionarFila(this)"/>';
                            },
                            headerSort: false,
                            cssClass: 'text-center',
                            frozen: true,
                            cellClick: function (e, cell) {
                                var $element = $(cell.getElement());
                                var $chkbox = $element.find('.select-row');
                                if ($chkbox.prop('checked')) {
                                    $chkbox.prop('checked', false);
                                    cell.getRow().deselect();
                                } else {
                                    $chkbox.prop('checked', true);
                                    cell.getRow().select();
                                }
                            }
                        },
                        { title: "Numero", field: "transaccion_numeracion", visible: false, headerFilter: "input", headerFilterPlaceholder: "Numero", headerMenu: headerMenu },
                        { title: "Documento", field: "cliente_nrodocumento", visible: false, headerFilter: "input", headerFilterPlaceholder: "Documento", headerMenu: headerMenu },
                        { title: "Nombre", field: "cliente_razonsocial", headerFilter: "input", headerFilterPlaceholder: "Nombre", headerMenu: headerMenu },
                        { title: "Lote", field: "lote_nombre", headerFilter: "input", headerFilterPlaceholder: "Lote", headerMenu: headerMenu },
                        { title: "Proyecto", field: "proyecto_nombrecorto", visible: false, headerFilter: "select", headerFilterParams: { values: { "CASUARINAS": "CASUARINAS", "PUNTA I": "PUNTA I", "PUNTA II": "PUNTA II", "BOULEVARD": "BOULEVARD", "SUNSET BAY": "SUNSET BAY" } }, headerFilterPlaceholder: "Proyecto", headerMenu: headerMenu },
                        { title: "M. Pago", field: "tipometodopago_descripcion", headerFilter: "input", headerFilterPlaceholder: "M. Pago", headerMenu: headerMenu },
                        { title: "Nro Operacion", field: "transaccion_nrooperacion", visible: false, headerFilter: "input", headerFilterPlaceholder: "Lote", headerMenu: headerMenu },
                        { title: "F. Transaccion", field: "fechaTransaccion", width: 280, visible: false, hozAlign: "center", sorter: "date", headerFilter: rangoFechasEditor, headerFilterFunc: rangoFechasFuncion, headerFilterLiveFilter: false, headerMenu: headerMenu },
                        { title: "F. Deposito", field: "fechaDeposito", width: 280, hozAlign: "center", sorter: "date", headerFilter: rangoFechasEditor, headerFilterFunc: rangoFechasFuncion, headerFilterLiveFilter: false, headerMenu: headerMenu },
                        { title: "Banco", field: "banco_descripcioncorta", headerFilter: "input", headerFilterPlaceholder: "Banco", headerMenu: headerMenu },
                        { title: "Cuenta", field: "cuentabanco_cuenta", visible: false, headerMenu: headerMenu },
                        { title: "Monto", field: "transaccion_monto", headerMenu: headerMenu }
                    ],
                },
                {
                    title: "Editar    ", field: "transaccion_id", formatter: function (cell, formatterParams, onRendered) {
                        return '<button class=" btn btn-primary btn-sm" onclick="abrirModalEdicionTransaccion(' + cell.getValue() + ',' + rowIndex + ')">Editar</button>'
                    }
                },
                {
                    title: "Anular   ", field: "transaccion_id", formatter: function (cell, formatterParams, onRendered) {
                        if (!cell.getRow()._row.data.transaccion_estadoemision) {
                            console.log(cell.getRow()._row.data.transaccion_estadoemision);
                            return '<button class=" btn btn-danger btn-sm" onclick="anularTransaccion(' + cell.getValue() + ',' + row.getData().tipotransaccion_id + ',' + rowIndex + ')">Anular</button>'
                        } else {
                            console.log(cell.getRow()._row.data.transaccion_estadoemision);
                            return '';
                        }

                    }
                },
                {
                    title: "Emitir", field: "transaccion_id", formatter: function (cell, formatterParams, onRendered) {
                        if (!cell.getRow()._row.data.transaccion_estadoemision) {
                            return '<button class=" btn btn-success btn-sm" onclick="emitirDocumentoElectronico(' + cell.getValue() + ',' + rowIndex + ')">Emitir</button>'
                        } else {
                            return '';
                        }

                    }
                },
                {
                    title: "Emitir Fecha Cambiada", field: "transaccion_id", formatter: function (cell, formatterParams, onRendered) {
                        if (!cell.getRow()._row.data.transaccion_estadoemision) {
                            return '<button class=" btn btn-success btn-sm" onclick="abrirModalFechaEmision(' + cell.getValue() + ',' + rowIndex + ')">Emitir Fecha Cambiada</button>'
                        } else {
                            return '';
                        }

                    }
                },
                {
                    title: "Observaciones", field: "transaccion_id", formatter: function (cell, formatterParams, onRendered) {
                        return '<button class=" btn btn-warning btn-sm" onclick="abrirModalObservacionTransaccion(' + cell.getValue() + ',' + rowIndex + ')">Observacion</button>';
                    }
                }
            ],
        });
    }
}

const abrirDocumentosDetalle = (transaccionId) => {

}

const abrirModalFechaEmision = (transaccion_id) => {
    document.querySelector("#inputTransaccionIdHidden").value = transaccion_id;
    
    document.querySelector("#fechaEmision").value = establecerFechaHoy(); 
    $("#modalEdicionFechaEmision").modal({
        keyboard: false,
        backdrop: false
    });
}

const seleccionarTodos = () => {

    if (subTable.getSelectedData().length == 0) {
        subTable.selectRow();
        $(".select-row").prop("checked", true);
    } else {
        subTable.deselectRow();
        $(".select-row").prop("checked", false);
    }
}

const seleccionarFila = (elem) => {
    if ($(elem).prop("checked")) {
        $(elem).prop("checked", false);
    } else {
        $(elem).prop("checked", true);
    }
};

const procesarSeleccionados = () => {
    if (subTable.getSelectedData().length != 0) {
        asyncProcesarSeleccionados();
    }
}

var rangoFechasEditor = (cell, onRendered, success, cancel, editorParams) => {

    var end;
    //var cellValue = moment(cell.getValue(), "DD/MM/YYYY").format("YYYY-MM-DD"),
    var container = document.createElement("span");

    //create and style inputs
    var start = document.createElement("input");
    start.setAttribute("type", "date");
    start.setAttribute("placeholder", "inicio");
    start.style.padding = "4px";
    start.style.width = "50%";
    start.style.boxSizing = "border-box";

    start.value = cell.getValue();

    function buildValues() {
        success({
            start: start.value,
            end: end.value,
        });
    }

    function keypress(e) {
        if (e.keyCode == 13) {
            buildValues();
        }

        if (e.keyCode == 27) {
            cancel();
        }
    }

    end = start.cloneNode();
    end.setAttribute("placeholder", "Fin");

    start.addEventListener("change", buildValues);
    start.addEventListener("blur", buildValues);
    start.addEventListener("keydown", keypress);

    end.addEventListener("change", buildValues);
    end.addEventListener("blur", buildValues);
    end.addEventListener("keydown", keypress);


    container.appendChild(start);
    container.appendChild(end);

    return container;
}

/**
 * 
 * @param {any} headerValue
 * @param {any} rowValue
 * @param {any} rowData
 * @param {any} filterParams
 */
function rangoFechasFuncion(headerValue, rowValue, rowData, filterParams) {
    //headerValue - the value of the header filter element
    //rowValue - the value of the column in this row
    //rowData - the data for the row being filtered
    //filterParams - params object passed to the headerFilterFuncParams property

    if (rowValue) {
        var valueRow = moment(rowValue, "DD/MM/YYYY");
        if (headerValue.start != "") {
            var start = moment(headerValue.start, "YYYY-MM-DD");
            if (headerValue.end != "") {
                var end = moment(headerValue.end, "YYYY-MM-DD");
                //return rowValue >= headerValue.start && rowValue <= headerValue.end;
                return valueRow >= start && valueRow <= end;
            } else {
                //return rowValue >= headerValue.start;
                return valueRow >= start;
            }
        } else {
            if (headerValue.end != "") {
                var end = moment(headerValue.end, "YYYY-MM-DD");
                //return rowValue <= headerValue.end;
                return valueRow >= end;
            }
        }
    }
    return true; //must return a boolean, true if it passes the filter.
}

const cargarDataPagos = async () => {
    response = await axios.post('../Transacciones/cargarBancos');
    $('#selectBancoDestino').select2({
        theme: 'bootstrap4',
        data: response.data
    });
    cargarCuentasDestino();
}

const cargarCuentasDestino = async () => {
    var idBanco = $('#selectBancoDestino').select2('data')[0].id;
    const response = await axios.post('../Transacciones/cargarCuentasBanco?idBanco=' + idBanco);
    $('#selectCtaDestino').html('').select2({
        theme: 'bootstrap4',
        data: response.data
    });
}

const cargarCuentasDestino2 = async (cuentaId) => {
    var idBanco = $('#selectBancoDestino').select2('data')[0].id;
    const response = await axios.post('../Transacciones/cargarCuentasBanco?idBanco=' + idBanco);
    $('#selectCtaDestino').html('').select2({
        theme: 'bootstrap4',
        data: response.data
    });
    $("#selectCtaDestino").val(cuentaId).trigger("change");
}

const abrirModalEdicionTransaccion = (transaccionId, rowIndex) => {
    cargarDatosTransaccion(transaccionId);
    $("#modalEdicionTransaccion").modal({
        keyboard: false,
        backdrop: false
    });
}

const abrirModalObservacionTransaccion = (transaccionId, rowIndex) => {
    cargarObservacionTransaccion(transaccionId);
    $("#modalObservacionesTransaccion").modal({
        keyboard: false,
        backdrop: false
    });
}

const cargarDatosTransaccion = async (transaccionId) => {
    const response = await axios.post('CargarTransaccionPorId?idTransaccion=' + transaccionId);
    if (response.data.flag === 1) {
        if (response.data.transaccionRespuesta.transaccion_banco_id != null) {
            $("#selectBancoDestino").val(response.data.transaccionRespuesta.transaccion_banco_id).trigger("change");
            cargarCuentasDestino2(response.data.transaccionRespuesta.transaccion_cuentabanco_id);
        }
        if (response.data.transaccionRespuesta.transaccion_fechadeposito != null) {
            document.querySelector("#fechaDeposito").value = moment(response.data.transaccionRespuesta.transaccion_fechadeposito, "DD/MM/YYYY").format("YYYY-MM-DD");
        }
        if (response.data.transaccionRespuesta.transaccion_nrooperacion != null) {
            document.querySelector("#inputNroOperacion").value = response.data.transaccionRespuesta.transaccion_nrooperacion;
        }
        document.querySelector("#inputTransaccionId").value = transaccionId;
    } else if (response.data.flag == 0) {
        toastr["error"]("ocurrio un error en el servidor comuniquese con su area de sistemas");
    }
}

const cargarObservacionTransaccion = async (transaccionId) => {
    const response = await axios.post('CargarObservacionTransaccion?idTransaccion=' + transaccionId);
    if (response.data.flag === 1) {
        console.log(response);
        document.querySelector("#transaccionObservacion").value = response.data.transaccion_observaciones == null ? '' : response.data.transaccion_observaciones;
        document.querySelector("#inputTransaccionIdObservacion").value = transaccionId;

    } else if (response.data.flag == 0) {
        toastr["error"]("ocurrio un error en el servidor comuniquese con su area de sistemas");
    }
}

const actualizarTransaccion = async () => {
    var data = new Object();
    data.transaccionId = document.querySelector("#inputTransaccionId").value;
    data.transaccionBancoDestino = $('#selectBancoDestino').select2('data')[0].id;
    data.transaccionCuentaDestino = $("#selectCtaDestino").select2('data')[0].id;
    data.transaccionFechaDeposito = moment(document.querySelector('#fechaDeposito').value, 'YYYY-MM-DD').format("DD/MM/YYYY");
    data.transaccionNroOperacion = document.querySelector("#inputNroOperacion").value;
    const response = await axios.post('actualizarTransaccion', JSON.stringify(data), {
        headers: {
            'Content-Type': 'application/json'
        }
    });
    if (response.data.flag == 1) {
        toastr["success"]("Transaccion modificada correctamente");
        location.reload();
    } else if (response.data.flag == 0) {
        toastr["error"]("ocurrio un error en el servidor comuniquese con su area de sistemas");
    }
}

/**  */
const actualizarObservacion = async () => {
    var data = new Object();
    data.transaccionId = document.querySelector("#inputTransaccionIdObservacion").value;
    data.transaccionObservaciones = document.querySelector("#transaccionObservacion").value;
    const response = await axios.post('actualizarObservacion', JSON.stringify(data), {
        headers: {
            'Content-Type': 'application/json'
        }
    });
    if (response.data.flag == 1) {
        toastr.options.onHidden = () => {
            location.reload();
        };
        toastr.options.timeOut = 1000;
        toastr["success"]("Se proceso correctamente las transacciones");
    } else {
        toastr["error"]("Ocurrio un error comuniquese con el area de sistemas");
    }
}

const anularTransaccion = (transaccion_id, tipotransaccion_id) => {
    document.querySelector("#inputTransaccionId").value = transaccion_id;
    document.querySelector("#inputTipoTransaccion").value = tipotransaccion_id;
    $("#modalConfirmacionAnulacion").modal({
        keyboard: false,
        backdrop: false
    });
}

const anulacionTransaccion = async (transaccion_id, tipotransaccion_id) => {
    const response = await axios.post('anularTransaccion?transaccionId=' + transaccion_id + '&tipoTransaccion=' + tipotransaccion_id);
    if (response.data.flag == 1) {
        toastr["success"]("Transaccion anulada correctamente");
        location.reload();
    } else {
        toastr["error"]("ocurrio un error en el servidor comuniquese con su area de sistemas");
    }
}

const emitirDocumentoElectronico = async (transaccionId) => {
    document.querySelector("#inputAccionHidden").value = 1;
    document.querySelector("#inputTransaccionIdHiddenConfirmacion").value = transaccionId;
    const response = await axios.post('../Reportes/cargarModelo?transaccionId=' + transaccionId);
    console.log("-------------------------");
    console.log(response);
    console.log("-------------------------");
    if (response.data.flag == 1) {
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
        crearQrr(response.data.data.documento_qrcodeValue);
        $(".monedaSimbolo").empty();
        $(".monedaSimbolo").append(response.data.data.moneda_simbolo);
        document.querySelector("#documento_subtotal").innerHTML = response.data.data.documento_subtotal;
        document.querySelector("#documento_igv").innerHTML = response.data.data.documento_igv;
        $(".documentoTotal").empty();
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
        $("#tbodyDetalles tr").remove();
        $("#tbodyDetalles").append(detalle);
    }

    $("#modalConfirmacionEmision").modal({
        keyboard: false,
        backdrop: false
    });
    /*document.getElementById('buttonEmitirDocumento').addEventListener("click", () => {
        document.getElementById('buttonEmitirDocumento').disabled = true;
        window.open('../Reportes/EmitirDocumentoElectronico?transaccionId=' + transaccionId, '_blank');
        setTimeout(() => { location.reload(); }, 3000);
    });*/

    //const response = await axios.post('../Reportes/EmitirDocumentoElectronico?transaccionId=' + transaccion_id);   
}

const emitirDocumentoElectronicoFechaEmision = async () => {
    document.querySelector("#inputAccionHidden").value = 2;
    document.querySelector("#inputTransaccionIdHiddenConfirmacion").value = document.querySelector("#inputTransaccionIdHidden").value
    const response = await axios.post('../Reportes/cargarModeloFechaEmision?transaccionId=' + document.querySelector("#inputTransaccionIdHidden").value + '&fechaEmision=' + document.querySelector("#fechaEmision").value);
    if (response.data.flag == 1) {
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
    }

    $("#modalConfirmacionEmision").modal({
        keyboard: false,
        backdrop: false
    });  

    //const response = await axios.post('../Reportes/EmitirDocumentoElectronico?transaccionId=' + transaccion_id);   
}

const crearQrr = (qrValue) => {
    qr = new QRious({
        element: document.getElementById('qr-code'),
        size: 100,
        value: qrValue
    });
}

/** */
const asyncProcesarSeleccionados = async () => {
    console.log(subTable.getSelectedData());
    var transaccionesSeleccionadas = [];
    subTable.getSelectedData().forEach((item) => {
        var transaccionSeleccionada = new Object();
        transaccionSeleccionada.transaccionId = item.transaccion_id;
        transaccionesSeleccionadas.push(transaccionSeleccionada);
    });
    console.log(transaccionesSeleccionadas);
    var data = new Object();
    data.listaTransacciones = transaccionesSeleccionadas;
    const response = await axios.post('procesarTransacciones', JSON.stringify(data), {
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (response.data.flag == 1) {
        toastr.options.onHidden = () => {
            location.reload();
        };
        toastr.options.timeOut = 1000;
        toastr["success"]("Se proceso correctamente las transacciones");
    } else {
        toastr["error"]("Ocurrio un error comuniquese con el area de sistemas");
    }
}

const validarFechaEmision = () => {
    let fechaLimite = establecerFechaLimite();
    compararFechas(fechaLimite);
    const splitFecha = document.querySelector("#fechaEmision").value.split('-');
    const fechaInput = new Date(splitFecha[0], splitFecha[1] - 1, splitFecha[2]);

}

const establecerFechaLimite = () => {
    let fechalimite = new Date(hoy);
    fechalimite.setDate(fechalimite.getDate() - 7);
    return moment(fechalimite).format('YYYY-MM-DD');
}

const establecerFechaHoy = () => {
    var mes = (hoy.getMonth() + 1);
    return hoy.getFullYear() + '-' + mes.toString().padStart(2, '0') + '-' + hoy.getDate();
}

const compararFechas = (fechaLimite) => {
    if (!moment(fechaLimite).isBefore(document.querySelector('#fechaEmision').value)) {
        toastr["error"]("La fecha esta fuera del rango de 7 dias");
        document.querySelector('#fechaEmision').value = establecerFechaHoy();
    }
    const fechaHoy = moment(hoy).format('YYYY-MM-DD');
    if (moment(document.querySelector('#fechaEmision').value).isAfter(fechaHoy)) {
        toastr["error"]("la fecha de emision no puede ser superior a la fecha de hoy");
        document.querySelector('#fechaEmision').value = establecerFechaHoy();
    }
}