var table = new Tabulator("#tableEventos", {
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
        abrirDialogPagos(row);
    },
    columns: [
        { title: "id", field: "contrato_id", visible: false },
        { title: "tipoCambio", field: "cotizacion_tipocambio_precioventa", visible: false },
        { title: "Codigo", field: "contrato_numeracion", headerFilter: "input", headerFilterPlaceholder: "Contrato" },
        { title: "Proyecto", field: "proyecto_nombrecorto", headerFilter: "input", headerFilterPlaceholder: "Proyecto" },
        { title: "Lote", field: "lote_nombre", headerFilter: "input", headerFilterPlaceholder: "Lote" },
        { title: "Tip. Doc.", field: "proformauif_tipodocumentoidentidad_descripcion" },
        { title: "Documento", field: "proformauif_cliente_nrodocumento", headerFilter: "input", headerFilterPlaceholder: "Documento" },
        { title: "Cliente", field: "proformauif_cliente_razonsocial", headerFilter: "input", headerFilterPlaceholder: "Cliente" },
        { title: "F. Ven", field: "cuota_fechavencimiento" },
        { title: "M. Cuota", field: "montocuota", formatter: "money" },
        { title: "C. Faltantes", field: "nrocuotas" },
        { title: "Dias Vencimiento", field: "diasvencimiento" }
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

$(document).ready(() => {
    $("#formEventos").validate({
        rules: {
            inputFecha: {
                required: true
            },
            inputEvento: {
                required: true
            }

        },
        messages: {
            inputFecha: {
                required: 'Campo Obligatorio',
            },
            inputEvento: {
                required: 'Campo Obligatorio',
            }
        },
        ignore: [".dataOculta"]
    });
});

document.getElementById('inputEvento').addEventListener("keyup", () => {
    document.querySelector("#inputEvento").value = document.querySelector("#inputEvento").value.toUpperCase();
});

document.getElementById('formEventos').addEventListener('submit', (e) => {
    e.preventDefault();
    document.getElementById('buttonAgregarEvento').disable = true;
    var valid = false
    if ($('#selectMetodoContacto').select2('data').length > 0) {
        $("#selectMetodoContacto").addClass("is-valid").removeClass('is-invalid');
        valid = true;
    } else {
        $("#selectMetodoContacto").addClass("is-invalid").removeClass('is-valid');
        valid = false;
    }
    if ($("#formEventos").valid() && valid) {
        guardarEvento();
    }
});

const abrirDialogPagos = (row) => {
    limpiarModal(row);
    document.querySelector("#inputContratoId").value = row._row.data.contrato_id;
    $("#modalEvento").modal({
        keyboard: false,
        backdrop: false
    });
}

const limpiarModal = (row) => {
    document.querySelector("#inputEvento").value = "";
    document.querySelector("#divHistoria").innerHTML = '';
    document.querySelector("#inputFecha").value = null;
    document.querySelector("#inputMonto").value = "";
    document.querySelector("#inputContratoId").value = "";
    cargarData(row);
}

const cargarData = async (row) => {
    var response = await axios.post('cargarTiposContacto');
    $('#selectMetodoContacto').html('').select2({
        theme: 'bootstrap4',
        data: response.data
    });
    response = await axios.post('cargarEventos?contratoId=' + row._row.data.contrato_id);    
    var timeline = '<ul class="timeline">';
    if (response.data.flag == 1) {
        response.data.data.forEach((item) => {
            var css = '';
            if (item.evento_estadoevento_id == 2) {
                css = 'style="background-color: #f51d0a;"';
            } else if (item.evento_estadoevento_id == 3) {
                css = 'style="background-color: #1cb305;"';
            } else if (item.evento_estadoevento_id == 4) {
                css = 'style="background-color: #ced909;"';
            }
            timeline += '<li ' + css + '>' +
                '<a href ="#">Fecha Evento:</a>' +
                '<a href ="#" class="float-right">' + item.evento_fechacreacion + '</a>' +
                '<p class="mb-0"> Fecha propuesta: ' + item.evento_fechapropuesta + '</p>' +
                '<p class="mb-0"> Monto: ' + (item.evento_montopropuesto == null ? 0.00 : item.evento_montopropuesto) + '</p>' +
                '<p class="mb-0"> Descripcion: ' + item.evento_descripcion + '</p>' +
                '</li>';

        });
        timeline += '</ul>'
        $("#divHistoria").append(timeline);
    } else {
        toastr["error"]("Ocurrio un error en el servidor, comuniquese con el area de sistemas");
    }
}

const guardarEvento = async () => {
    var data = new Object();
    data.eventoContratoId = document.querySelector("#inputContratoId").value;
    data.eventoDescripcion = document.querySelector("#inputEvento").value;
    data.eventoFecha = moment(document.querySelector('#inputFecha').value, 'YYYY-MM-DD').format("DD/MM/YYYY");
    data.eventoMonto = document.querySelector("#inputMonto").value;
    var opcionesContacto = [];
    $('#selectMetodoContacto').select2('data').forEach((item) => {
        var opcionContacto = new Object();
        opcionContacto.contactoId = item.id;
        opcionesContacto.push(opcionContacto);
    });
    data.eventoMetodosContacto = opcionesContacto;
    const response = await axios.post('GuardarEvento', JSON.stringify(data), {
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (response.data.flag == 1) {
        toastr.options = {
            "showDuration": "300",
            "hideDuration": "500",
            "timeOut": "500",
            "extendedTimeOut": "500",
            "onHidden": () => { location.reload(); }
        }
        toastr["success"]("Evento guardado correctamente");
        location.reload();
    } else if (response.data.flag == 2) {
        toastr.options = {
            "showDuration": "300",
            "hideDuration": "500",
            "timeOut": "500",
            "extendedTimeOut": "500",
            "onHidden": () => { location.reload(); }
        }
        toastr["warning"]("El evento activo a sido reemplazado correctamente");
    } else if (response.data.flag == 3) {
        toastr["error"]("Ocurrio un error en el servidor, comuniquese con su area de sistemas");
    }
}