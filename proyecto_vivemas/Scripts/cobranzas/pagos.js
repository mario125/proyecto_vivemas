/* Inicializacion de tabla */
var table = new Tabulator("#tablePagos", {
    ajaxURL: "CargarPagos",
    ajaxConfig: "POST",
    selectable: 1,
    placeholder: "Cargando...",
    height: "400px",
    pagination: "local",
    paginationSize: 20,
    paginationSizeSelector: [3, 6, 8, 10, 20, 50],
    rowSelected: (row) => {        
        row.toggleSelect();
        if (row._row.data.contrato_estadocontrato_id == 3)//ESTA ACTIVO??
        {
            abrirDialogPagos(row);
        }        
    },
    columns: [
        { title: "id", field: "contrato_id", visible: false },
        { title: "tipoCambio", field: "cotizacion_tipocambio_precioventa", visible: false },
        { title: "Codigo", field: "contrato_numeracion" },
        { title: "Estado", field: "estadocontrato_descripcion" },
        { title: "Proyecto", field: "proyecto_nombrecorto" },
        { title: "Lote", field: "lote_nombre" },
        { title: "Tip. Doc.", field: "proformauif_tipodocumentoidentidad_descripcion" },
        { title: "Documento", field: "proformauif_cliente_nrodocumento" },
        { title: "Cliente", field: "proformauif_cliente_razonsocial" },
        { title: "Moneda", field: "moneda_descripcioncorta" },
        { title: "F. Ven", field: "cuota_fechavencimiento" },
        { title: "M. Cuota", field: "montocuota", formatter: "money" },
        { title: "M. Total", field: "cotizacion_preciototal", formatter: "money" },
        { title: "M. Pendiente", field: "montopendiente", formatter: "money" },
        { title: "C. Faltantes", field: "nrocuotas" }
    ],
    rowFormatter: (row) => {
        var data = row.getData();
        //console.log(row.getData());
        if (data.estadovencimiento == 1) {
            if (data.diasvencimiento <= 30) {
                row.getElement().style.backgroundColor = "#047a12";
            } else if (data.diasvencimiento > 30 && data.diasvencimiento <= 60) {
                row.getElement().style.backgroundColor = "#cccf3e";
            } else if (data.diasvencimiento > 60) {
                row.getElement().style.backgroundColor = "#c71a1a";
            }
        }
    }
});
/* Fin inicializacion de tabla*/
/*document ready*/
var tipoCambioInterno;
$(document).ready(function () {
    cargarDataPagos();
    cargarTipoCambio();
    $("#formPagos").validate({
        rules: {
            inputMonto: {
                required: true,
                max: () => {
                    return parseFloat(document.querySelector("#pMontoTotal").textContent);
                }
            },
            inputBancoOrigen: {
                required: () => {
                    var idMetodoPago = $('#selectMetodoPago').select2('data')[0].id;
                    if (idMetodoPago == 1 || idMetodoPago == 3 || idMetodoPago == 4) {
                        return false;
                    }
                    if (idMetodoPago == 2) {
                        return false;
                    }
                    if (idMetodoPago == 5) {
                        return true;
                    }
                }
            },
            inputNroOperacion: {
                required: () => {
                    var idMetodoPago = $('#selectMetodoPago').select2('data')[0].id;
                    if (idMetodoPago == 1 || idMetodoPago == 3 || idMetodoPago == 4) {
                        return false;
                    }
                    if (idMetodoPago == 2) {
                        return true;
                    }
                    if (idMetodoPago == 5) {
                        return true;
                    }
                }
            }
        },
        messages: {
            inputMonto: {
                required: 'Campo Obligatorio',
                max: 'El monto no puede ser mayor al total'
            },
            inputBancoOrigen: {
                required: 'Campo Obligatorio'
            },
            inputNroOperacion: {
                required: 'Campo Obligatorio'
            }
        },
        ignore: [".dataOculta"]
    });
    //prueba();
    /** Carga el datepicker */   
    /** fin cargado de fechas */
});
/*const prueba = async () => {
    const response = await axios.post('CargarPagos');
    console.log(response.data);
}*/
/*fin document ready*/
/*Eventos*/
document.getElementById('inputDocumento').addEventListener("keyup", event => {
    table.setFilter("proformauif_cliente_nrodocumento", 'like', document.querySelector("#inputDocumento").value);
});

document.getElementById('inputNombreCliente').addEventListener("keyup", event => {
    table.setFilter("proformauif_cliente_razonsocial", 'like', document.querySelector("#inputNombreCliente").value);
});

document.getElementById('inputCodigo').addEventListener("keyup", event => {
    table.setFilter("contrato_numeracion", 'like', document.querySelector("#inputCodigo").value);
});

document.getElementById('inputLote').addEventListener("keyup", event => {
    table.setFilter("lote_nombre", 'like', document.querySelector("#inputLote").value);
});

$('#selectBancoDestino').on('select2:select', (e) => {
    cargarCuentasDestino();
});

$('#selectMetodoPago').on('select2:select', (e) => {
    mostrarDatosPorMetodoPago();
});

document.getElementById('buttonAgregarPago').addEventListener("click", (event) => {    
    if ($("#formPagos").valid() == true) {       
        var idMetodoPago = $('#selectMetodoPago').select2('data')[0].id;        
        if (idMetodoPago == 2 || idMetodoPago == 5) {
            if ($('#selectBancoDestino').select2('data')[0] != null) {                
                $("#selectBancoDestino").addClass('is-valid').removeClass('is-invalid');
                calcularPago();
                document.querySelector("#pMontoPagado").textContent = parseFloat(document.querySelector("#inputMonto").value).toFixed(2);
                document.querySelector("#pMontoPendiente").textContent = parseFloat(parseFloat(document.querySelector("#pMontoTotal").textContent) - parseFloat(document.querySelector("#inputMonto").value).toFixed(2)).toFixed(2);
                $('.pagosContenedor').find('input ').each(function () {
                    $(this).attr('disabled', true);
                });
                $('.pagosContenedor').find('select').each(function () {
                    $(this).attr('disabled', true);
                });
                $('.pagosContenedor').find('button').each(function () {
                    $(this).attr('disabled', true);
                });
            }
            else {                
                $("#selectBancoDestino").addClass('is-invalid').removeClass('is-valid');
            }
        } else {
            calcularPago();
            document.querySelector("#pMontoPagado").textContent = parseFloat(document.querySelector("#inputMonto").value).toFixed(2);
            document.querySelector("#pMontoPendiente").textContent = parseFloat(parseFloat(document.querySelector("#pMontoTotal").textContent) - parseFloat(document.querySelector("#inputMonto").value).toFixed(2)).toFixed(2);
            $('.pagosContenedor').find('input ').each(function () {
                $(this).attr('disabled', true);
            });
            $('.pagosContenedor').find('select').each(function () {
                $(this).attr('disabled', true);
            });
            $('.pagosContenedor').find('button').each(function () {
                $(this).attr('disabled', true);
            });
        }
    }    
});

document.getElementById('buttonCerrar').addEventListener("click", (event) => {
    limpiarForm();
});

document.getElementById('buttonPagar').addEventListener("click", (event) => {

    if (document.querySelector("#tbodyPagoDetalle").rows.length === 0) {
        toastr["warning"]("Debe agregar una transaccion para poder procesar pagos")
    } else {
        confirmarPago();
    }
});

document.getElementById('buttonCrearSeparacion').addEventListener("click", (event) => {
    document.getElementById('buttonCrearSeparacion').disabled = true;
    realizarPago();
});

document.getElementById('inputNroOperacion').addEventListener("focusout", async (e) => {
    var nroOperacion = document.querySelector("#inputNroOperacion").value;
    response = await axios.post('validarNroOperacion?nroOperacion=' + nroOperacion);
    if (response.data.flag == 2) {
        $("#modalDuplicado").modal({
            keyboard: false,
            backdrop: false
        });
    } else if (response.data.flag == 0) {
        toastr["error"]("No se pudo realizar la validacion contacte con el area de sistemas");
    }
});

document.getElementById('buttonRechazarDuplicado').addEventListener("click", (e) => {
    document.querySelector("#inputNroOperacion").value = '';
});
/* Fin eventos */
/*Funciones*/

const cargarTipoCambio = async () => {
    const response = await axios.post('cargarTipoCambio');
    tipoCambioInterno = response.data.tipoCambioInterno;
}

const mostrarDatosPorMetodoPago = () => {
    var idMetodoPago = $('#selectMetodoPago').select2('data')[0].id;
    if (idMetodoPago == 1 || idMetodoPago == 3 || idMetodoPago == 4) {
        document.querySelector('#inputBancoOrigen').parentElement.parentElement.style.visibility = 'hidden';
        document.querySelector('#selectBancoDestino').parentElement.parentElement.style.visibility = 'hidden';
        document.querySelector('#inputNroOperacion').parentElement.parentElement.style.visibility = 'hidden';
        document.querySelector('#selectCtaDestino').parentElement.parentElement.style.visibility = 'hidden';
        $("#inputBancoOrigen").addClass('dataOculta');
        $("#selectBancoDestino").addClass('dataOculta');
        $("#inputNroOperacion").addClass('dataOculta');
        $("#selectCtaDestino").addClass('dataOculta');
    }
    if (idMetodoPago == 2 /*|| idMetodoPago == 1 || idMetodoPago == 3 || idMetodoPago == 4*/) {
        document.querySelector('#inputBancoOrigen').parentElement.parentElement.style.visibility = 'hidden';
        document.querySelector('#selectBancoDestino').parentElement.parentElement.style.visibility = 'visible';
        document.querySelector('#inputNroOperacion').parentElement.parentElement.style.visibility = 'visible';
        document.querySelector('#selectCtaDestino').parentElement.parentElement.style.visibility = 'visible';
        $("#inputBancoOrigen").addClass('dataOculta');
        $("#selectBancoDestino").removeClass('dataOculta');
        $("#inputNroOperacion").removeClass('dataOculta');
        $("#selectCtaDestino").removeClass('dataOculta');
    } if (idMetodoPago == 5) {
        document.querySelector('#inputBancoOrigen').parentElement.parentElement.style.visibility = 'visible';
        document.querySelector('#selectBancoDestino').parentElement.parentElement.style.visibility = 'visible';
        document.querySelector('#inputNroOperacion').parentElement.parentElement.style.visibility = 'visible';
        document.querySelector('#selectCtaDestino').parentElement.parentElement.style.visibility = 'visible';
        $("#inputBancoOrigen").removeClass('dataOculta');
        $("#selectBancoDestino").removeClass('dataOculta');
        $("#inputNroOperacion").removeClass('dataOculta');
        $("#selectCtaDestino").removeClass('dataOculta');
    }
}

const abrirDialogPagos = async (row) => {    
    //cargarCuentasDestino();
    const response = await axios.post('cargarEvento?contratoId=' + row._row.data.contrato_id);
    if (response.data.flag == 1) {
        if (response.data.data.idEvento != null) {
            document.querySelector("#inputEventoId").value = response.data.data.idEvento;
        } 
        /*if (response.data.data.monto != null) {
            document.querySelector("#inputMonto").value = response.data.data.monto;
        }*/
    }
    tipoCambioInterno = response.data.tipoCambioInterno;
    document.querySelector("#hTituloPagos").innerHTML = 'Nombre: ' + row._row.data.proformauif_cliente_razonsocial + ' - T. Cambio: ' + tipoCambioInterno;
    document.querySelector("#inputContratoId").value = row._row.data.contrato_id;
    document.querySelector("#pMontoTotal").innerHTML = parseFloat(row._row.data.montopendiente).toFixed(2);
    mostrarDatosPorMetodoPago();
    $("#modalPagos").modal({
        keyboard: false,
        backdrop: false
    });
    if (row._row.data.estadovencimiento == 1) {
        toastr["warning"]("Cuota vencida verificar la mora");
    }
}

const cargarDataPagos = async () => {
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
    //cargarCuentasDestino();
    $('#selectBancoDestino').val(null).trigger('change');
    document.querySelector("#fechaDeposito").value = moment(cargarFechaHoy(), "DD/MM/YYYY").format('YYYY-MM-DD');
}

const cargarCuentasDestino = async () => {
    var idBanco = $('#selectBancoDestino').select2('data')[0].id;
    //var idMoneda = $('#selectMoneda').select2('data')[0].id;
    const response = await axios.post('../Transacciones/cargarCuentasBanco?idBanco=' + idBanco);
    $('#selectCtaDestino').html('').select2({
        theme: 'bootstrap4',
        data: response.data
    });
}

const cargarFechaHoy = () => {
    var date = new Date();
    var fecha = '';
    if (date.getDate() < 10) {
        fecha = fecha + "0" + date.getDate() + "/";
    } else {
        fecha = fecha + date.getDate() + "/";
    }
    if (date.getMonth() < 9) {
        fecha = fecha + "0" + (date.getMonth() + 1) + "/";
    } else {
        fecha = fecha + (date.getMonth() + 1) + "/";
    }
    fecha = fecha + date.getFullYear();
    return fecha;
}

const calcularPago = async () => {
    var data = new Object();
    data.montoPago = document.querySelector("#inputMonto").value;
    data.idContrato = document.querySelector("#inputContratoId").value;
    data.montoMora = document.querySelector("#inputMora").value;
    data.montoDescuento = document.querySelector("#inputDescuento").value;
    const response = await axios.post('CalcularPagos', JSON.stringify(data), {
        headers: {
            'Content-Type': 'application/json'
        }
    });
    if (response.data.flag == 1) {

        response.data.datosPagoCalculo.cuotasPagadas.forEach((item, index) => {
            console.log(item);
            var tr = document.createElement('tr');
            var td = document.createElement('td');

            var inputDescripcion = document.createElement('textarea');
            inputDescripcion.classList.add('form-control');
            inputDescripcion.classList.add('textAreaDescripcion')
            inputDescripcion.setAttribute('rows', '6')
            inputDescripcion.value = item.descripcionPago

            td.appendChild(document.createTextNode(item.nroCuota));
            td.setAttribute('class', 'idCuota');
            tr.appendChild(td);

            td = document.createElement('td');
            td.appendChild(document.createTextNode(item.fechaVencimientoCuota));
            tr.appendChild(td);

            td = document.createElement('td');
            td.setAttribute('class', 'tdDescripcion');
            td.appendChild(inputDescripcion);
            tr.appendChild(td);

            td = document.createElement('td');
            td.appendChild(document.createTextNode(parseFloat(item.montoPagado).toFixed(2)));
            tr.appendChild(td);

            td = document.createElement('td');
            td.appendChild(document.createTextNode(parseFloat(item.montoDescuento).toFixed(2)));
            tr.appendChild(td);

            td = document.createElement('td');
            td.appendChild(document.createTextNode(parseFloat(item.montoPagadoDescuento).toFixed(2)));
            tr.appendChild(td);

            td = document.createElement('td');
            td.appendChild(document.createTextNode(parseFloat(item.montoPendiente).toFixed(2)));
            tr.appendChild(td);

            document.getElementById('tbodyPagoDetalle').appendChild(tr);
        });
    } else if (response.data.flag == 0) {
        toastr["error"]("Ocurrio un error del lado del servidor comuniquese con su area de sistemas");
    }
}

const limpiarForm = () => {
    $(".is-valid").removeClass("is-valid");
    $(".is-invalid").removeClass("is-invalid");
    $('#formPagos').trigger("reset");
    $("#tbodyPagoDetalle tr").remove();
    document.querySelector("#inputContratoId").value = '';
    document.querySelector("#inputEventoId").value = '';
    document.querySelector("#pMontoPagado").textContent = '';
    document.querySelector("#pMontoPendiente").textContent = '';    
    document.querySelector("#fechaDeposito").value = moment(cargarFechaHoy(), "DD/MM/YYYY").format('YYYY-MM-DD');
    //console.log(cargarFechaHoy);
    //document.querySelector("#fechaDeposito").value = cargarFechaHoy();
    $('.pagosContenedor').find('input ').each(function () {
        $(this).attr('disabled', false);
    });
    $('.pagosContenedor').find('select').each(function () {
        $(this).attr('disabled', false);
    });
    $('.pagosContenedor').find('button').each(function () {
        $(this).attr('disabled', false);
    });
    $("#selectCtaDestino").html('');
    cargarDataPagos();
    mostrarDatosPorMetodoPago();
}

const confirmarPago = () => {
    if ($("#formPagos").valid()) {
        var idMetodoPago = $('#selectMetodoPago').select2('data')[0].id;
        if (idMetodoPago == 2 || idMetodoPago == 5) {
            if ($('#selectBancoDestino').select2('data')[0] != null) {
                $('#selectBancoDestino').addClass("is-valid").removeClass("is-invalid");
                cargarDataModalDetalle();
                $("#modalDetalle").modal({
                    keyboard: false,
                    backdrop: false
                });
            } else {
                $("#selectBancoDestino").addClass('is-invalid').removeClass('is-valid');
            }
        }
        else {
            cargarDataModalDetalle();
            $("#modalDetalle").modal({
                keyboard: false,
                backdrop: false
            });
        }
    }
}

const cargarDataModalDetalle = () => {
    var detalleSeparacion = '';
    detalleSeparacion = '<dt class="col-4">Metodo de Pago</dt>' + '<dd class="col-sm-8">' + $('#selectMetodoPago').select2('data')[0].text + '</dd>';
    var idMetodoPago = $('#selectMetodoPago').select2('data')[0].id;
    if (idMetodoPago == 2 || idMetodoPago == 5) {//monto mas banco y cuenta
        detalleSeparacion = detalleSeparacion
            + '<dt class="col-4">Fecha deposito</dt>' + '<dd class="col-sm-8">' + moment(document.querySelector('#fechaDeposito').value, 'YYYY-MM-DD').format("DD/MM/YYYY") + '</dd>'
            + '<dt class="col-4">Banco destino</dt>' + '<dd class="col-sm-8">' + $('#selectBancoDestino').select2('data')[0].text + '</dd>'
            + '<dt class="col-4">Cuenta destino</dt>' + '<dd class="col-sm-8">' + $('#selectCtaDestino').select2('data')[0].text + '</dd>';
        if (idMetodoPago == 5) {//monto mas banco destino origen mas cuenta
            detalleSeparacion = detalleSeparacion
                + '<dt class="col-4">Banco origen</dt>' + '<dd class="col-sm-8">' + document.querySelector('#inputBancoOrigen').value + '</dd>';
        }
    }
    if (document.querySelector('#inputMora').value != "") {
        detalleSeparacion = detalleSeparacion
            + '<dt class="col-4">Mora</dt>' + '<dd class="col-sm-8">' + document.querySelector('#inputMora').value + '</dd>';
    }
    if (document.querySelector('#inputDescuento').value != "") {
        detalleSeparacion = detalleSeparacion
            + '<dt class="col-4">Descuento</dt>' + '<dd class="col-sm-8">' + document.querySelector('#inputDescuento').value + '</dd>';
    }
    detalleSeparacion = detalleSeparacion + '<dt class="col-4">Monto</dt>' + '<dd class="col-sm-8">' + document.querySelector('#inputMonto').value + '</dd>';
    document.querySelector('#dlDetallePago').innerHTML = '';
    document.querySelector('#dlDetallePago').innerHTML = detalleSeparacion;
}

const realizarPago = async () => {
    var data = new Object();
    var idMetodoPago = $('#selectMetodoPago').select2('data')[0].id;
    data.montoPago = document.querySelector("#inputMonto").value;
    data.idContrato = document.querySelector("#inputContratoId").value;
    data.idMetodoPago = $('#selectMetodoPago').select2('data')[0].id;
    data.idEvento = document.querySelector("#inputEventoId").value;
    data.fechaDeposito = moment(document.querySelector('#fechaDeposito').value, 'YYYY-MM-DD').format("DD/MM/YYYY");
    data.montoDescuento = document.querySelector("#inputDescuento").value;
    if (idMetodoPago == 2) {
        data.nroOperacion = document.querySelector("#inputNroOperacion").value;
        data.bancoDestino = $('#selectBancoDestino').select2('data')[0].id;
        data.nroCuentaBancoDestino = $('#selectCtaDestino').select2('data')[0].id;
    }
    if (idMetodoPago == 5) {
        data.bancoOrigen = document.querySelector('#inputBancoOrigen').value;
    }
    if (document.querySelector("#inputMora").value !== "") {
        data.montoMora = document.querySelector("#inputMora").value;
    }    


    data.detallePagos = [];

    $('#tablePagoDetalle > tbody > tr').each(function () {
        var detalleItem = {};
        detalleItem.idPago = $(this).find(".idCuota").text();
        detalleItem.descripcion = $(this).find(".textAreaDescripcion").val();
        data.detallePagos.push(detalleItem);
    });

    //console.log(data.detalleItems);

   



    const response = await axios.post('RealizarPago', JSON.stringify(data), {
        headers: {
            'Content-Type': 'application/json'
        }
    });
    if (response.data.flag === 1) {
        toastr.options = {
            "showDuration": "300",
            "hideDuration": "1000",
            "timeOut": "1000",
            "extendedTimeOut": "1000",
            "onHidden": () => { location.reload(); }
        }
        toastr["success"]("Pago realizado correctamente");

    } else if (response.data.flag === 0) {
        toastr.options = {
            "showDuration": "2000",
            "hideDuration": "1000",
            "timeOut": "1000",
            "extendedTimeOut": "1000"            
        }
        toastr["error"]("ocurrio un error por favor comuniquese con el area de sistemas");
    }

   


}
/*Fin funciones*/