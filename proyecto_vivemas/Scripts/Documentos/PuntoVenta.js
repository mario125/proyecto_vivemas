$(document).ready(() => {

	cargarData();

	document.querySelector("#inputFechaEmision").value = establecerFechaHoy(); 

	$("#inputDocumentoCliente").autocomplete({
		source: function (request, response) {
			$.getJSON("../Clientes/buscarClientesAutocompleteCasuarinas",
				{
					term: request.term,
					param1: $('#selectTipoDocumento').select2('data')[0].id
				},
				response
			);
		},
		delay: 300,
		select: (event, ui) => {
			document.querySelector("#inputIdCliente").value = ui.item.id;
		}
	});
	$("#ventaForm").validate({
		errorPlacement: function (error, element) { },
		rules: {
			inputDocumentoCliente: {
				required: true
			},
			inputFechaDeposito: {
				required: true
			},
			inputFechaEmision: {
				required: true
			}
		},
		messages: {
			inputDocumentoCliente: {
				required: 'Cliente Obligatorio'
			},
			inputFechaDeposito: {
				required: 'Campo Obligatorio'
			},
			inputFechaEmision: {
				required: 'Campo Obligatorio'
			}
		}
	});
});

document.getElementById('inputDocumentoCliente').addEventListener('keyup', (e) => { limpiarClienteCambio(); });

document.getElementById('inputDocumentoCliente').addEventListener('focusout', (e) => { limpiarClienteCambio(); });

document.getElementById('buttonAgregarFila').addEventListener('click', (e) => { agregarFilaItem(); });

document.getElementById('ventaForm').addEventListener('submit', (event) => {
	event.preventDefault();
	$(".inputPrecio").rules("add", {
		required: true
	});
	if ($("#ventaForm").valid()) {
		var flagValid = true;
		if ($('#tableItems > tbody > tr').length == 0) {
			toastr["error"]("Tiene que agregar algun detalle a la venta");
			flagValid = false;
		}
		$('#tableItems > tbody > tr').each(function () {
			if ($(this).find(".inputItemDescripcion").val() == "") {
				$(this).find(".inputItemDescripcion").addClass("is-invalid").removeClass("is-valid");
				flagValid = false;
			} else {
				$(this).find(".inputItemDescripcion").addClass("is-valid").removeClass("is-invalid");
			}
			if ($(this).find(".inputItemCodigo").val() == "") {
				$(this).find(".inputItemCodigo").addClass("is-invalid").removeClass("is-valid");
				flagValid = false
			} else {
				$(this).find(".inputItemCodigo").addClass("is-valid").removeClass("is-invalid");
			}
			if ($(this).find(".inputPrecio").val() == "") {
				$(this).find(".inputPrecio").addClass("is-invalid").removeClass("is-valid");
				flagValid = false;
			} else {
				$(this).find(".inputPrecio").addClass("is-valid").removeClass("is-invalid");
			}
		});
		if (flagValid) {
			procesarDocumento();
		}
	}
});

$('#selectTipoDocumento').on('select2:select', (e) => {
	$("#inputIdCliente").val("");
	$("#inputDocumentoCliente").val("");
});

$('#selectBancoDestino').on('select2:select', (e) => {
	cargarCuentasDestino();
});

const hoy = new Date();

const establecerFechaHoy = () => {

	var mes = (hoy.getMonth() + 1);
	return hoy.getFullYear() + '-' + mes.toString().padStart(2, '0') + '-' + hoy.getDate();
}

const cargarData = async () => {
	var opcion = {
		id: 0,
		text: '-'
	};
	let response = await axios.post('cargarTiposDocumentoVenta');
	if (response.data == "") {
		toastr["error"]("ocurrio un error comuniquese con el area de sistemas");
	} else {
		$('#selectTipoDocumento').select2({
			theme: 'bootstrap4',
			data: response.data
		});
	}
	response = await axios.post('../Transacciones/cargarMonedas');
	if (response.data == "") {
		toastr["error"]("ocurrio un error comuniquese con el area de sistemas");
	} else {
		$('#selectMoneda').select2({
			theme: 'bootstrap4',
			data: response.data
		});
	}
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
		text: '-'
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

const limpiarClienteCambio = () => {
	if ($("#inputDocumentoCliente").val() === "") {
		$("#inputIdCliente").val("");
	}
}

const agregarFilaItem = () => {
	str = '<tr class="d-flex">' +
		'<td class="itemDescripcion col-4"><textarea type="text" rows="5" class="form-control inputItemDescripcion" id="inputPrueba"></textarea></td>' +
		'<td class="itemCodigo col-2"><input type="text" class="form-control inputItemCodigo"></td>' +
		'<td class="itemCantidad col-2">1</td>' +
		'<td class="itemPrecio col-2"><input type="number" class="form-control inputPrecio"></td>' +
		'<td class="col-2">' + ' <button type="button" class="btn btn-danger btn-xs btn-flat deleteItem">eliminar</button>' + '</td>'
	'</tr>';
	$('#tableItems tbody').append(str);

	$(".deleteItem").on('click', function (event) {
		$(this).parent().parent().remove();
		recalcularValoresVenta();
	});

	var inputsItemDescripcion = document.getElementsByClassName("inputItemDescripcion");
	var toUpper = () => {
		this.value = this.value.toUpperCase();
	};
	for (var i = 0; i < inputsItemDescripcion.length; i++) {
		inputsItemDescripcion[i].addEventListener("keyup", toUpper, false);
	}

	var inputsItemCodigo = document.getElementsByClassName("inputItemCodigo");
	for (var i = 0; i < inputsItemCodigo.length; i++) {
		inputsItemCodigo[i].addEventListener("keyup", toUpper, false);
	}

	var inputsPrecio = document.getElementsByClassName('inputPrecio');
	for (var i = 0; i < inputsPrecio.length; i++) {
		inputsPrecio[i].addEventListener("keyup", recalcularValoresVenta, false);
	}

}

const recalcularValoresVenta = () => {
	var total = 0.00;
	var subtotal = 0.00;
	var igv = 0.00;
	$('#tableItems > tbody > tr').each(function () {
		total = parseFloat(total) + parseFloat($(this).find(".inputPrecio").val());
		subtotal = parseFloat(subtotal) + parseFloat($(this).find(".inputPrecio").val());
	});
	$("#inputSubtotal").val(subtotal.toFixed(2));
	$("#inputImpuesto").val(igv.toFixed(2));
	$("#inputTotal").val(total.toFixed(2));
}

const procesarDocumento = async () => {
	var data = {};
	data.documento_tipodocumentoventa_id = $('#selectTipoDocumento').select2('data')[0].id;
	data.documento_tipometodopago_id = $("#selectMetodoPago").select2('data')[0].id;
	data.documento_nrooperacion = document.querySelector("#inputNroOperacion").value;
	data.documento_fechadeposito = moment(document.querySelector('#inputFechaDeposito').value, 'YYYY-MM-DD').format("DD/MM/YYYY");
	data.documento_moneda_id = $('#selectMoneda').select2('data')[0].id;
	data.documento_cliente_id = document.querySelector("#inputIdCliente").value;
	data.documento_total = parseFloat(document.querySelector("#inputTotal").value);
	data.documento_subtotal = parseFloat(document.querySelector("#inputSubtotal").value);
	data.documento_igv = parseFloat(document.querySelector("#inputImpuesto").value);
	data.documento_banco_id = $('#selectBancoDestino').select2('data')[0].id;
	data.documento_cuenta_id = $('#selectCtaDestino').select2('data')[0].id;
	data.documento_fecha_emision = $("#inputFechaEmision").val();
	data.detalleventa = [];
	$('#tableItems > tbody > tr').each(function () {
		var detalleItem = {};
		detalleItem.documentoDetalle_codigo = $(this).find(".inputItemCodigo").val();
		detalleItem.documentoDetalle_descripcion = $(this).find(".inputItemDescripcion").val();
		detalleItem.documentoDetalle_total = $(this).find(".inputPrecio").val();
		data.detalleventa.push(detalleItem);
	});
	console.log(data);
	const response = await axios.post('emitirDocumentoPuntoVenta', JSON.stringify(data), {
		headers: {
			'Content-Type': 'application/json'
		}
	});

	if (response.data.flag == 1) {
		//console.log(response.data.itemEnviado);
		toastr.options = {
			"showDuration": "500",
			"hideDuration": "500",
			"timeOut": "1000"
		}
		toastr.options.onShown = () => {
			window.open('../Reportes/ReimprimirDocumentoElectronico?documentoId=' + response.data.ventaId, '_blank');
		};
		toastr.options.onHidden = () => {
			location.reload()
		};

		toastr["success"]("Documento emitido correctamente");


	} else if (response.data.flag == 0) {
		toastr["error"]("Ocurrio un error durante la emision del documento contacte con su area de sistemas");
	}


}



const validarFechaEmision = () => {
	let fechaLimite = establecerFechaLimite();
	compararFechas(fechaLimite);
	const splitFecha = document.querySelector("#inputFechaEmision").value.split('-');
	const fechaInput = new Date(splitFecha[0], splitFecha[1] - 1, splitFecha[2]);

}

const establecerFechaLimite = () => {
	let fechalimite = new Date(hoy);
	fechalimite.setDate(fechalimite.getDate() - 7);
	return moment(fechalimite).format('YYYY-MM-DD');
}

const compararFechas = (fechaLimite) => {
	if (!moment(fechaLimite).isBefore(document.querySelector('#inputFechaEmision').value)) {
		toastr["error"]("La fecha esta fuera del rango de 7 dias");
		document.querySelector('#inputFechaEmision').value = establecerFechaHoy();
	}
	const fechaHoy = moment(hoy).format('YYYY-MM-DD');
	if (moment(document.querySelector('#inputFechaEmision').value).isAfter(fechaHoy)) {
		toastr["error"]("la fecha de emision no puede ser superior a la fecha de hoy");
		document.querySelector('#inputFechaEmision').value = establecerFechaHoy();
	}
}