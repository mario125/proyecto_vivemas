var table = new Tabulator("#tableSeparaciones", {
    //ajaxURL: "cargarSeparaciones",
    //ajaxConfig: "POST",
    layout: "fitColumns",
    placeholder: "Registros Vacios",
    height: "400px",
    pagination: "local",
    paginationSize: 20,
    paginationSizeSelector: [3, 6, 8, 10, 20, 50],
    columns: [
        { title: "id", field: "separacion_id", visible: false },
        { title: "idcliente", field: "cliente_id", visible: false },
        { title: "idlote", field: "lote_id", visible: false },
        { title: "Nro Documento", field: "cliente_nrodocumento", headerFilter: "input", headerFilterPlaceholder: "dni/ruc", width: 80 },
        { title: "Nombre/Razon Social", field: "cliente_razonsocial", headerFilter: "input", headerFilterPlaceholder: "nombre/razon social", width: 300 },
        { title: "Proyecto", field: "proyecto_nombrecorto", headerFilter: "input", headerFilterPlaceholder: "proyecto" },
        { title: "Lote", field: "lote_nombre", headerFilter: "input", headerFilterPlaceholder: "lote", width: 60 },
        { title: "Fecha Inicio", field: "separacion_fechainicio", headerFilter: "input", headerFilterPlaceholder: "nro contacto" },
        { title: "Fecha Fin", field: "separacion_fechafin", headerFilter: "input", headerFilterPlaceholder: "nro auxiliar" },
        { title: "Moneda", field: "moneda_descripcion" },
        { title: "Monto", field: "separacion_monto", width: 60 },
        {
            title: "Editar", field: "separacion_id", formatter: function (cell, formatterParams, onRendered) {
                return '<button class=" btn btn-success btn-sm" onclick="CargarDataSeparacionActualizar(' + cell.getValue() + ')">Cambiar Lote</button>'
            },
        }/*,
        {
            title: "extender", field: "separacion_id", formatter: function (cell, formatterParams, onRendered) {
                return '<button class=" btn btn-warning btn-sm" onclick="CargarDataSeparacionActualizar(' + cell.getValue() + ')">Cambiar Lote</button>'
            },
        }*/
    ]
});

$("#inputNroDocumento").autocomplete({
    delay: 300,
    minLength: 3,
    source: '../Clientes/buscarClientesPorDocumento',
    select: (event, ui) => {
        event.preventDefault();
        document.querySelector('#inputRazonSocial').value = ui.item.nombre;
        document.querySelector('#inputNroContacto').value = ui.item.nroContacto.trim();
        document.querySelector('#inputNroDocumento').value = ui.item.nroDocumento;
        document.querySelector('#inputIdCliente').value = ui.item.id;
        $('#selectPromotor').val(ui.item.promotorId);
        $('#selectPromotor').trigger('change');
        $('#selectTipoPersona').val(ui.item.tipoPersona);
        $('#selectTipoPersona').trigger('change');
        $('#selectTipoDocumento').val(ui.item.tipoDocumento);
        $('#selectTipoDocumento').trigger('change');
    }
});

$(document).ready(function () {
    cargarDataSeparaciones();
    $("#formSeparaciones").validate({
        rules: {
            inputNroDocumento: {
                required: true,
                digits: true
            },
            inputNroContacto: {
                required: true,
                digits: true
            },
            inputRazonSocial: {
                required: true
            },
            inputFechaVencimiento: {
                required: true,
                digits: true
            },
            inputNroSeparacion: {
                required: true,
                digits: true
            },
            inputMonto: {
                required: true
            }
        },
        messages: {
            inputRazonSocial: {
                required: 'Campo Obligatorio'
            },
            inputNroDocumento: {
                required: 'Campo Obligatorio',
                digits: 'Solo se pueden ingresar numeros'
            },
            inputNroContacto: {
                required: 'Campo Obligatorio',
                digits: 'Solo se pueden ingresar numeros'
            },
            inputNroAuxiliarContacto: {
                digits: 'Solo se pueden ingresar numeros'
            },
            inputFechaVencimiento: {
                required: 'Campo Obligatorio',
                digits: 'Solo puede ingresar un numero de dias'
            },
            inputNroSeparacion: {
                required: 'Campo Obligatorio',
                digits: 'Solo se pueden ingresar numeros'
            },
            inputMonto: {
                required: 'Campo Obligatorio'
            }
        },
        ignore: [".dataOculta"]
    });    
});

document.getElementById('inputRazonSocial').addEventListener("keyup", () => {
    document.getElementById('inputRazonSocial').value = document.getElementById('inputRazonSocial').value.toUpperCase();
});

document.getElementById('inputBancoOrigen').addEventListener("keyup", () => {
    document.getElementById('inputBancoOrigen').value = document.getElementById('inputBancoOrigen').value.toUpperCase();
});

document.getElementById('buttonAbrirModalSeparaciones').addEventListener("click", (event) => {
    abrirDialogCreacionSeparacion();
});

document.getElementById('buttonMostrarDetalle').addEventListener("click", (event) => {
    abrirDialogDetalleSeparacion();
});

document.getElementById('buttonCrearSeparacion').addEventListener("click", (event) => {
    crearSeparacion();
});

document.getElementById('buttonActualizarLote').addEventListener("click", (event) => {
    var idLoteSeparacion = $('#selectLoteActualizacion').select2('data');
    if (idLoteSeparacion.length >= 1) {
        $('#selectLoteActualizacion').addClass('is-valid').removeClass('is-invalid');
        actualizarLote();
    } else {
        $('#selectLoteActualizacion').addClass('is-invalid').removeClass('is-valid');
    }
    
});

document.getElementById('buttonCerrar').addEventListener("click", (event) => {
    limpiarFormulario();
});

$('#selectProyecto').on('select2:select', (e) => {
    cargarLotes(e.params.data.id)
});

$('#selectProyectoActualizacion').on('select2:select', (e) => {
    cargarLotesActualizacion(e.params.data.id)
});

$('#selectBancoDestino').on('select2:select', (e) => {
    cargarCuentasBanco();
});

$('#selectMoneda').on('select2:select', (e) => {
    cargarCuentasBanco();
});

$('#selectLote').on('select2:select', async (e) => {
    var response = await axios.post('cargarDatosLote?idLote=' + e.params.data.id);
    document.querySelector('#inputLoteArea').value = response.data.lote_areatotal;
    document.querySelector('#inputLotePrecioMetro').value = response.data.lote_preciometro;
    document.querySelector('#inputLotePrecio').value = response.data.lote_precio;
});

$('#selectLoteActualizacion').on('select2:select', async (e) => {
    var response = await axios.post('cargarDatosLote?idLote=' + e.params.data.id);
    document.querySelector('#inputLoteAreaActualizacion').value = response.data.lote_areatotal;
    document.querySelector('#inputLotePrecioMetroActualizacion').value = response.data.lote_preciometro;
    document.querySelector('#inputLotePrecioActualizacion').value = response.data.lote_precio;
});

$('#selectMetodoPago').on('select2:select', (e) => {
    mostrarDatosPorMetodoPago();
});

$('#selectEstadoSeparacion').on('select2:select', (e) => {    
    table.setData('cargarSeparaciones?estadoSeparacionId=' + e.params.data.id, {}, 'POST');
});

const cargarDataSeparaciones = async () => {
    var response = await axios.post('../Clientes/cargarPromotores');
    $('#selectPromotor').select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
    response = await axios.post('../Clientes/cargarTiposPersona');
    $('#selectTipoPersona').select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
    response = await axios.post('cargarEstadosSeparacion');
    $('#selectEstadoSeparacion').select2({
        theme: 'bootstrap4',
        data: response.data
    });
    response = await axios.post('../Clientes/cargarTiposDocumento');
    $('#selectTipoDocumento').select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
    cargarProyectos();
    response = await axios.post('../Transacciones/cargarMetodosPago');
    $('#selectMetodoPago').select2({
        theme: 'bootstrap4',
        data: response.data
    });
    response = await axios.post('../Transacciones/cargarBancos');
    $('#selectBancoDestino').select2({
        theme: 'bootstrap4',
        data: response.data
    });
    response = await axios.post('../Transacciones/cargarMonedas');
    $('#selectMoneda').select2({
        theme: 'bootstrap4',
        data: response.data
    });
    cargarCuentasBanco();
    mostrarDatosPorMetodoPago();    
    var estadoSeparacionId = $('#selectEstadoSeparacion').select2('data')[0].id;
    table.setData('cargarSeparaciones?estadoSeparacionId=' + estadoSeparacionId, {}, 'POST');
}

const cargarProyectos = async () => {
    var response = await axios.post('cargarProyectos');
    $('#selectProyecto').select2({
        theme: 'bootstrap4',
        data: response.data
    });
    var idProyecto = response.data[0].id;
    cargarLotes(idProyecto);
}

const cargarProyectosActualizacion = async () => {
    var response = await axios.post('cargarProyectos');
    $('#selectProyectoActualizacion').html('').select2({
        theme: 'bootstrap4',
        data: response.data
    });
    var idProyecto = response.data[0].id;
    cargarLotesActualizacion(idProyecto);
}

const cargarLotes = async (idProyecto) => {
    const response = await axios.post('cargarLotes?idProyecto=' + idProyecto);
    $('#selectLote').html('').select2({
        theme: 'bootstrap4',
        data: response.data
    });
    $('#selectLote').val(null).trigger('change');
    document.querySelector('#inputLoteArea').value = '';
    document.querySelector('#inputLotePrecioMetro').value = '';
    document.querySelector('#inputLotePrecio').value = '';
}

const cargarLotesActualizacion = async (idProyecto) => {
    const response = await axios.post('cargarLotes?idProyecto=' + idProyecto);
    $('#selectLoteActualizacion').html('').select2({
        theme: 'bootstrap4',
        data: response.data
    });
    $('#selectLoteActualizacion').val(null).trigger('change');
    document.querySelector('#inputLoteAreaActualizacion').value = '';
    document.querySelector('#inputLotePrecioMetroActualizacion').value = '';
    document.querySelector('#inputLotePrecioActualizacion').value = '';
}

const abrirDialogCreacionSeparacion = () => {
    //cargarCuentasDestino();
    $("#modalSeparaciones").modal({
        keyboard: false,
        backdrop: false
    });
}

const abrirDialogDetalleSeparacion = () => {
    var idLoteSeparacion = $('#selectLote').select2('data');
    var idMetodoPago = $('#selectMetodoPago').select2('data')[0].id;
    var idPromotor = $('#selectPromotor').select2('data');
    if (idMetodoPago == 1 || idMetodoPago == 3 || idMetodoPago == 4) {
        if ($("#formSeparaciones").valid() == true && idLoteSeparacion.length >= 1 && idPromotor.length >= 1) {
            $("#selectLote").addClass('is-valid').removeClass('is-invalid');
            $("#selectPromotor").addClass('is-valid').removeClass('is-invalid');
            cargarDataModalDetalle();
            $("#modalDetalle").modal({
                keyboard: false,
                backdrop: false
            });
        } else {
            if (idLoteSeparacion.length >= 1) {
                $("#selectLote").addClass('is-valid').removeClass('is-invalid');
            } else {
                $("#selectLote").addClass('is-invalid').removeClass('is-valid');
            }
            if (idPromotor.length >= 1) {
                $("#selectPromotor").addClass('is-valid').removeClass('is-invalid');
            } else {
                $("#selectPromotor").addClass('is-invalid').removeClass('is-valid');
            }

        }
    }
    if (idMetodoPago == 2) {
        if ($("#formSeparaciones").valid() == true && idLoteSeparacion.length >= 1 && document.querySelector('#inputNroOperacion').value !== '' && idPromotor.length >= 1) {
            $("#selectLote").addClass('is-valid').removeClass('is-invalid');
            $("#inputNroOperacion").addClass('is-valid').removeClass('is-invalid');
            $("#selectPromotor").addClass('is-valid').removeClass('is-invalid');
            cargarDataModalDetalle();
            $("#modalDetalle").modal({
                keyboard: false,
                backdrop: false
            });
        } else {
            if (idLoteSeparacion.length >= 1) {
                $("#selectLote").addClass('is-valid').removeClass('is-invalid');
            } else {
                $("#selectLote").addClass('is-invalid').removeClass('is-valid');
            } if (idPromotor.length >= 1) {
                $("#selectPromotor").addClass('is-valid').removeClass('is-invalid');
            } else {
                $("#selectPromotor").addClass('is-invalid').removeClass('is-valid');
            } if (document.querySelector('#inputNroOperacion').value !== '') {
                $("#inputNroOperacion").addClass('is-valid').removeClass('is-invalid');
            } else {
                $("#inputNroOperacion").addClass('is-invalid').removeClass('is-valid');
            }
        }
    } if (idMetodoPago == 5) {
        if ($("#formSeparaciones").valid() == true && idLoteSeparacion.length >= 1 && document.querySelector('#inputNroOperacion').value !== '' && document.querySelector('#inputBancoOrigen').value !== '' && idPromotor.length >= 1) {
            $("#selectLote").addClass('is-valid').removeClass('is-invalid');
            $("#inputNroOperacion").addClass('is-valid').removeClass('is-invalid');
            $("#inputBancoOrigen").addClass('is-valid').removeClass('is-invalid');
            $("#selectPromotor").addClass('is-valid').removeClass('is-invalid');
            cargarDataModalDetalle();
            $("#modalDetalle").modal({
                keyboard: false,
                backdrop: false
            });
        } else {
            if (idLoteSeparacion.length >= 1) {
                $("#selectLote").addClass('is-valid').removeClass('is-invalid');
            } else {
                $("#selectLote").addClass('is-invalid').removeClass('is-valid');
            } if (idPromotor.length >= 1) {
                $("#selectPromotor").addClass('is-valid').removeClass('is-invalid');
            } else {
                $("#selectPromotor").addClass('is-invalid').removeClass('is-valid');
            } if (document.querySelector('#inputNroOperacion').value !== '') {
                $("#inputNroOperacion").addClass('is-valid').removeClass('is-invalid');
            } else {
                $("#inputNroOperacion").addClass('is-invalid').removeClass('is-valid');
            } if (document.querySelector('#inputBancoOrigen').value !== '') {
                $("#inputBancoOrigen").addClass('is-valid').removeClass('is-invalid');
            } else {
                $("#inputBancoOrigen").addClass('is-invalid').removeClass('is-valid');
            }
        }
    }

    //cargarCuentasDestino();

}

const cargarCuentasBanco = async () => {
    var idBanco = $('#selectBancoDestino').select2('data')[0].id;
    var idMoneda = $('#selectMoneda').select2('data')[0].id;
    const response = await axios.post('../Transacciones/cargarCuentas?idBanco=' + idBanco + '&idMoneda=' + idMoneda);
    $('#selectCtaDestino').html('').select2({
        theme: 'bootstrap4',
        data: response.data
    });
}

const mostrarDatosPorMetodoPago = () => {
    var idMetodoPago = $('#selectMetodoPago').select2('data')[0].id;
    if (idMetodoPago == 1 || idMetodoPago == 3 || idMetodoPago == 4) {
        document.querySelector('#inputBancoOrigen').parentElement.parentElement.style.visibility = 'hidden';
        document.querySelector('#selectBancoDestino').parentElement.parentElement.style.visibility = 'hidden';
        document.querySelector('#inputNroOperacion').parentElement.parentElement.style.visibility = 'hidden';
        document.querySelector('#selectCtaDestino').parentElement.parentElement.style.visibility = 'hidden';
    }
    if (idMetodoPago == 2) {
        document.querySelector('#inputBancoOrigen').parentElement.parentElement.style.visibility = 'hidden';
        document.querySelector('#selectBancoDestino').parentElement.parentElement.style.visibility = 'visible';
        document.querySelector('#inputNroOperacion').parentElement.parentElement.style.visibility = 'visible';
        document.querySelector('#selectCtaDestino').parentElement.parentElement.style.visibility = 'visible';
    } if (idMetodoPago == 5) {
        document.querySelector('#inputBancoOrigen').parentElement.parentElement.style.visibility = 'visible';
        document.querySelector('#selectBancoDestino').parentElement.parentElement.style.visibility = 'visible';
        document.querySelector('#inputNroOperacion').parentElement.parentElement.style.visibility = 'visible';
        document.querySelector('#selectCtaDestino').parentElement.parentElement.style.visibility = 'visible';
    }
}

const cargarDataModalDetalle = () => {
    var detalleSeparacion = '';
    detalleSeparacion = '<dt class="col-4">Nro Separacion</dt>' + '<dd class="col-sm-8">' + document.querySelector('#inputNroSeparacion').value + '</dd>'
        + '<dt class="col-4">Nro de Documento</dt>' + '<dd class="col-sm-8">' + document.querySelector('#inputNroDocumento').value + '</dd>'
        + '<dt class="col-4">Nombre</dt>' + '<dd class="col-sm-8">' + document.querySelector('#inputRazonSocial').value + '</dd>'
        + '<dt class="col-4">Nro de contacto</dt>' + '<dd class="col-sm-8">' + document.querySelector('#inputNroContacto').value + '</dd>'
        + '<dt class="col-4">Promotor</dt>' + '<dd class="col-sm-8">' + $('#selectPromotor').select2('data')[0].text + '</dd>'
        + '<dt class="col-4">Proyecto</dt>' + '<dd class="col-sm-8">' + $('#selectProyecto').select2('data')[0].text + '</dd>'
        + '<dt class="col-4">Lote</dt>' + '<dd class="col-sm-8">' + $('#selectLote').select2('data')[0].text + '</dd>'
        + '<dt class="col-4">Area</dt>' + '<dd class="col-sm-8">' + document.querySelector('#inputLoteArea').value + '</dd>'
        + '<dt class="col-4">Precio</dt>' + '<dd class="col-sm-8">' + document.querySelector('#inputLotePrecio').value + '</dd>'
        + '<dt class="col-4">Dias de separacion</dt>' + '<dd class="col-sm-8">' + document.querySelector('#inputFechaVencimiento').value + '</dd>'
        + '<dt class="col-4">Metodo de Pago</dt>' + '<dd class="col-sm-8">' + $('#selectMetodoPago').select2('data')[0].text + '</dd>'
        + '<dt class="col-4">Moneda</dt>' + '<dd class="col-sm-8">' + $('#selectMoneda').select2('data')[0].text + '</dd>'
        + '<dt class="col-4">Monto</dt>' + '<dd class="col-sm-8">' + document.querySelector('#inputMonto').value + '</dd>';
    document.querySelector('#dlDetalleSeparacion').innerHTML = '';
    document.querySelector('#dlDetalleSeparacion').innerHTML = detalleSeparacion;
}

const crearSeparacion = async () => {
    var data = new Object();
    var dataCliente = new Object();
    var dataTransaccion = new Object();
    dataCliente.clienteId = document.querySelector('#inputIdCliente').value;
    dataCliente.clienteTipoPersona = $('#selectTipoPersona').select2('data')[0].id;
    dataCliente.clienteTipoDocumento = $('#selectTipoDocumento').select2('data')[0].id;
    dataCliente.clienteNroDocumento = document.querySelector('#inputNroDocumento').value;
    dataCliente.clienteRazonSocial = document.querySelector('#inputRazonSocial').value;
    dataCliente.clienteNroContacto = document.querySelector('#inputNroContacto').value;
    dataCliente.clientePromotor = $('#selectPromotor').select2('data')[0].id;
    data.separacionProyectoId = $('#selectProyecto').select2('data')[0].id;
    data.separacionLoteId = $('#selectLote').select2('data')[0].id;
    data.separacionTiempo = document.querySelector('#inputFechaVencimiento').value;
    dataTransaccion.transaccionMetodoPago = $('#selectMetodoPago').select2('data')[0].id;
    var idMetodoPago = $('#selectMetodoPago').select2('data')[0].id;
    dataTransaccion.transaccionNroSeparacion = document.querySelector('#inputNroSeparacion').value;
    dataTransaccion.transaccionMoneda = $('#selectMoneda').select2('data')[0].id;
    dataTransaccion.transaccionMonto = document.querySelector('#inputMonto').value;
    if (idMetodoPago == 2 || idMetodoPago == 5) {
        dataTransaccion.transaccionNroOperacion = document.querySelector('#inputNroOperacion').value;
        dataTransaccion.transaccionBancoDestino = $('#selectBancoDestino').select2('data')[0].id;
        dataTransaccion.transaccionCuentaDestino = $('#selectCtaDestino').select2('data')[0].id;
        if (idMetodoPago == 5) {
            dataTransaccion.transaccionBancoOrigen = document.querySelector('#inputBancoOrigen').value;
        }
    }
    data.dataCliente = dataCliente;
    data.dataTransaccion = dataTransaccion;
    //console.log(data);
    const response = await axios.post('crearSeparacion', JSON.stringify(data), {
        headers: {
            'Content-Type': 'application/json'
        }
    });
    if (response.data.flag == 1) {
        toastr["success"]("Separacion Creada correctamente");
        location.reload();
    } else if (response.data.flag == 0) {
        toastr["error"]("Ocurrio un error del lado del servidor comuniquese con su area de sistemas");
    }
}

const CargarDataSeparacionActualizar = async (separacionId) => {
    document.querySelector("#inputSeparacionId").value = separacionId
    cargarProyectosActualizacion();
    $("#modalActualizarLote").modal({
        keyboard: false,
        backdrop: false
    });
}

const actualizarLote = async () => {
    var data = new Object();
    data.separacionId = document.querySelector('#inputSeparacionId').value;
    data.separacionLoteId = $('#selectLoteActualizacion').select2('data')[0].id;
    data.separacionProyectoId = $('#selectProyecto').select2('data')[0].id;
    const response = await axios.post('actualizarLote', JSON.stringify(data), {
        headers: {
            'Content-Type': 'application/json'
        }
    });
    if (response.data.flag == 1) {
        toastr["success"]("Lote cambiado correctamente");
        location.reload();
    } else if (response.data.flag == 0) {
        toastr["error"]("Ocurrio un error del lado del servidor comuniquese con su area de sistemas");
    }
}

const limpiarFormulario = async () => {
    $('#formSeparaciones').trigger("reset");
    $(".is-valid").removeClass("is-valid");
    $(".is-invalid").removeClass("is-invalid");
    var response = await axios.post('../Clientes/cargarPromotores');
    $('#selectPromotor').html('').select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
    response = await axios.post('../Clientes/cargarTiposPersona');
    $('#selectTipoPersona').html('').select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
    response = await axios.post('../Clientes/cargarTiposDocumento');
    $('#selectTipoDocumento').html('').select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
    cargarProyectos();
    response = await axios.post('../Transacciones/cargarMetodosPago');
    $('#selectMetodoPago').html('').select2({
        theme: 'bootstrap4',
        data: response.data
    });
    response = await axios.post('../Transacciones/cargarMonedas');
    $('#selectMoneda').html('').select2({
        theme: 'bootstrap4',
        data: response.data
    });
    response = await axios.post('../Transacciones/cargarBancos');
    $('#selectBancoDestino').html('').select2({
        theme: 'bootstrap4',
        data: response.data
    });
    cargarCuentasBanco();
    $("#collapseOne").addClass('show')
    $("#collapseThree").removeClass('show');
}

