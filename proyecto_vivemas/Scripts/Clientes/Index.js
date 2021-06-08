var editarCliente = (cell, formatterParams) => {   
    return '<button class=" btn btn-success btn-sm" onclick="' + CargarDataClientesActualizar(cell.row.data.cliente_id)+'">Editar cliente</button>';
}

var table = new Tabulator("#tableClientes", {
    ajaxURL: "CargarClientes",
    ajaxConfig: "POST",
    layout: "fitColumns",    
    placeholder: "Registros Vacios",
    height: "400px",
    pagination: "local",
    paginationSize: 20,
    paginationSizeSelector: [3, 6, 8, 10, 20, 50],
    columns: [
        { title: "id", field: "cliente_id", visible: false },
        { title: "Nro Documento", field: "cliente_nrodocumento", headerFilter: "input", headerFilterPlaceholder: "dni/ruc" },
        { title: "Nombre/Razon Social", field: "cliente_razonsocial", headerFilter: "input", headerFilterPlaceholder: "nombre/razon social" },
        { title: "Nro Contacto", field: "cliente_nrocontacto", headerFilter: "input", headerFilterPlaceholder: "nro contacto" },
        { title: "Nro Auxiliar", field: "cliente_nrocontactoauxiliar", headerFilter: "input", headerFilterPlaceholder: "nro auxiliar" },
        { title: "Email", field: "cliente_email", headerFilter: "input", headerFilterPlaceholder: "correo electronico" },
       
        {
            title: "Editar", field: "cliente_id", formatter: function (cell, formatterParams, onRendered) {
                return '<button class=" btn btn-success btn-sm" onclick="CargarDataClientesActualizar('+cell.getValue()+')">Editar cliente</button>'
            },
        }
    ]
});

$(document).ready(function () {
    cargarDataClientes();
    $("#formClientes").validate({
        rules: {
            inputNroDocumento: {
                digits: true
            },
            inputNroContacto: {
                required: true,
                digits: true
            },
            inputNroAuxiliarContacto: {
                digits: true
            }
        },
        messages: {
            inputRazonSocial: {
                required: 'Campo Obligatorio'
            },
            inputNroDocumento: {
                digits: 'Solo se pueden ingresar numeros'
            },
            inputNroContacto: {
                required: 'Campo Obligatorio',
                digits: 'Solo se pueden ingresar numeros'
            },
            inputNroAuxiliarContacto: {
                digits: 'Solo se pueden ingresar numeros'
            }
        }
    });
});

document.getElementById('buttonAbrirModalClientes').addEventListener("click", (event) => {
    abrirDialogCreacionCliente()
});

document.getElementById('inputRazonSocial').addEventListener("keyup", () => {
    document.getElementById('inputRazonSocial').value = document.getElementById('inputRazonSocial').value.toUpperCase();
});

/*document.getElementById('buttonBuscarNroContacto').addEventListener("click", (event) => {
    buscarNroContacto();
});*/

document.getElementById('inputNroContacto').addEventListener('focusout', (event) => {
    buscarNroContacto();
});

document.getElementById('buttonLimpiar').addEventListener("click", (event) => {
    limpiarForm();
});

document.getElementById('buttonCerrar').addEventListener("click", (event) => {
    limpiarForm();
    document.getElementById('buttonLimpiar').disabled = false;
});

document.getElementById('buttonCrearCliente').addEventListener("click", (event) => {
    if (document.querySelector('#inputIdCliente').value === "") {
        crearCliente();
    } else {        
        actualizarCliente();
    }
    
});

$('#selectDepartamento').on('select2:select', function (e) {
    var data = e.params.data;
    actualizarProvincias(data.id)
});

$('#selectProvincia').on('select2:select', function (e) {
    var data = e.params.data;
    actualizarDistritos(data.id)
});

$('#selectMedioContacto').on('select2:select', function (e) {
    var data = e.params.data;
    actualizarCanales(data.id)
});

const abrirDialogCreacionCliente = () => {
    //cargarCuentasDestino();
    $("#modalClientes").modal({
        keyboard: false,
        backdrop: false
    });
}

const cargarDataClientes = async () => {
    var response = await axios.post('cargarTiposPersona');
    $('#selectTipoPersona').select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
    response = await axios.post('cargarTiposDocumento');
    $('#selectTipoDocumento').select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
    response = await axios.post('cargarDepartamentos');
    $('#selectDepartamento').select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
    var idDepartamento = response.data.results[0].id
    response = await axios.post('cargarProvincias?idDepartamento=' + idDepartamento);
    $('#selectProvincia').select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
    var idProvincia = response.data.results[0].id
    response = await axios.post('cargarDistritos?idProvincia=' + idProvincia);
    $('#selectDistrito').select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
    response = await axios.post('cargarPromotores');
    $('#selectPromotor').select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
    response = await axios.post('cargarProyectos');
    $("#selectInteresProyecto").select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
    response = await axios.post('cargarMedios');
    $("#selectMedioContacto").select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
    var idMedio = response.data.results[0].id
    response = await axios.post('cargarCanales?idMedio=' + idMedio);
    $("#selectCanalContacto").select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
    $('#selectMedioContacto').val(null).trigger('change');
    $('#selectCanalContacto').val(null).trigger('change');
}

const CargarDataClientesActualizar = async (cliente_id) => {
    const response = await axios.post('buscarCliente?idCliente=' + cliente_id);
    if (response.data.flag == 1) {
        document.querySelector('#inputIdCliente').value = cliente_id;
        if (response.data.dataClientes.clienteMedioContacto !== null) {
            $('#selectMedioContacto').val(response.data.dataClientes.clienteMedioContacto);
            $('#selectMedioContacto').trigger('change');
            actualizarCanales(response.data.dataClientes.clienteMedioContacto);
        }
        if (response.data.dataClientes.clienteCanalContacto !== null) {
            $('#selectCanalContacto').val(response.data.dataClientes.clienteCanalContacto);
            $('#selectCanalContacto').trigger('change');
        }
        if (response.data.dataClientes.clienteTipoPersona !== null) {
            $('#selectTipoPersona').val(response.data.dataClientes.clienteTipoPersona);
            $('#selectTipoPersona').trigger('change');
        }
        if (response.data.dataClientes.clienteTipoDocumento !== null) {
            $('#selectTipoDocumento').val(response.data.dataClientes.clienteTipoDocumento);
            $('#selectTipoDocumento').trigger('change');
        }
        $('#selectPromotor').val(response.data.dataClientes.clientePromotor);
        $('#selectPromotor').trigger('change');
        document.querySelector("#inputNroDocumento").value = response.data.dataClientes.clienteNroDocumento;
        document.querySelector("#inputRazonSocial").value = response.data.dataClientes.clienteRazonSocial;
        document.querySelector("#inputNroContacto").value = response.data.dataClientes.clienteNroContacto;
        document.querySelector("#inputNroAuxiliarContacto").value = response.data.dataClientes.clienteNroContactoAuxiliar;
        document.querySelector("#inputEmail").value = response.data.dataClientes.clienteCorreoElectronico;

        if (response.data.dataClientes.clienteDepartamento !== null) {
            $('#selectDepartamento').val(response.data.dataClientes.clienteDepartamento);
            $('#selectDepartamento').trigger('change');
            actualizarProvincias(response.data.dataClientes.clienteDepartamento);
        }
        if (response.data.dataClientes.clienteProvincia !== null) {
            $('#selectProvincia').val(response.data.dataClientes.clienteProvincia);
            $('#selectProvincia').trigger('change');
        }
        if (response.data.dataClientes.clienteDistrito !== null) {
            $('#selectDistrito').val(response.data.dataClientes.clienteDistrito);
            $('#selectDistrito').trigger('change');
        }
        document.querySelector("#inputDireccion").value = response.data.dataClientes.clienteCorreoElectronico;
        var intereses = [];
        response.data.dataClientes.clienteInteresesProyecto.forEach(element => {
            intereses.push(element.idInteresProyecto);
        });
        if (intereses.length !== 0) {
            $('#selectInteresProyecto').val(intereses);
            $('#selectInteresProyecto').trigger('change');
        }
        document.getElementById("buttonLimpiar").disabled = true;
        $("#modalClientes").modal({
            keyboard: false,
            backdrop: false
        });
    } else {
        toastr["error"]("Ocurrio un error del lado del servidor comuniquese con su area de sistemas");
    }
    
}

const actualizarProvincias = async (idDepartamento) => {
    response = await axios.post('cargarProvincias?idDepartamento=' + idDepartamento);
    $("#selectProvincia").html('').select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
    response.data.results[0].id
    actualizarDistritos(response.data.results[0].id)
}

const actualizarDistritos = async (idProvincia) => {
    response = await axios.post('cargarDistritos?idProvincia=' + idProvincia);
    $('#selectDistrito').html('').select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
}

const actualizarCanales = async (idMedio) => {
    response = await axios.post('cargarCanales?idMedio=' + idMedio);
    $('#selectCanalContacto').html('').select2({
        theme: 'bootstrap4',
        data: response.data.results
    });
    $('#selectCanalContacto').val(null).trigger('change');
}

const buscarNroContacto = async () => {
    var nroContacto = document.querySelector("#inputNroContacto").value;
    response = await axios.post('validarNroContacto?nroContacto=' + nroContacto);
    if (response.data.flag === 0) {
        toastr["error"]("Ocurrio un error del lado del servidor comuniquese con su area de sistemas");
    } else if (response.data.flag === 1) {
        toastr["success"]("Nro de contacto disponible");
    } else if (response.data.flag === 2) {
        toastr["warning"]("Nro de contacto previamente registrado para el promotor " + response.data.promotor);
    } else if (response.data.flag === 3) {
        toastr["warning"]("Ningun numero ha sido ingresado");
    }
}

const limpiarForm = () => {
    document.querySelector('#inputIdCliente').value = "";
    $('#selectInteresProyecto').val(null).trigger('change');
    $('#selectMedioContacto').val(1).trigger('change');
    actualizarCanales(1)
    $('#selectTipoPersona').val(1).trigger('change');
    $('#selectTipoDocumento').val(1).trigger('change');
    $('#selectDepartamento').val(1).trigger('change');
    actualizarProvincias(1);
    $('#selectPromotor').val(5).trigger('change');   
    $('#formClientes').trigger("reset");
    $('#selectMedioContacto').val(null).trigger('change');
    $('#selectCanalContacto').val(null).trigger('change');
    $(".is-invalid").removeClass("is-invalid");
    $(".is-valid").removeClass("is-valid");
}

const crearCliente = async () => {
    if ($("#formClientes").valid()) {
        var interesesProyecto = $('#selectInteresProyecto').select2('data');
        var medioContacto = $('#selectMedioContacto').select2('data');
        var canalContacto = $('#selectCanalContacto').select2('data');       
        if (interesesProyecto.length < 1) {
            $("#selectInteresProyecto").addClass('is-invalid').removeClass('is-valid');
        } else if (medioContacto.length < 1) {
            $("#selectMedioContacto").addClass('is-invalid').removeClass('is-valid');
        } else if (canalContacto.length < 1) {
            $("#selectCanalContacto").addClass('is-invalid').removeClass('is-valid');
        } else {
            $("#selectInteresProyecto").addClass('is-valid').removeClass('is-invalid');
            $("#selectMedioContacto").addClass('is-valid').removeClass('is-invalid');
            $("#selectCanalContacto").addClass('is-valid').removeClass('is-invalid');
            var data = new Object();
            var clienteInteresesProyecto = [];
            data.clienteMedioContacto = $("#selectMedioContacto").select2('data')[0].id;
            data.clienteCanalContacto = $('#selectCanalContacto').select2('data')[0].id;
            data.clientePromotor = $('#selectPromotor').select2('data')[0].id;
            data.clienteTipoPersona = $('#selectTipoPersona').select2('data')[0].id;
            data.clienteTipoDocumento = $('#selectTipoDocumento').select2('data')[0].id;
            data.clienteNroDocumento = document.querySelector('#inputNroDocumento').value;
            data.clienteRazonSocial = document.querySelector('#inputRazonSocial').value;
            data.clienteNroContacto = document.querySelector('#inputNroContacto').value;
            data.clienteNroContactoAuxiliar = document.querySelector('#inputNroAuxiliarContacto').value;
            data.clienteCorreoElectronico = document.querySelector('#inputEmail').value;
            data.clienteDepartamento = $('#selectDepartamento').select2('data')[0].id;
            data.clienteProvincia = $('#selectProvincia').select2('data')[0].id;
            data.clienteDistrito = $('#selectDistrito').select2('data')[0].id;
            data.clienteDireccion = document.querySelector("#inputDireccion").value;
            interesesProyecto.forEach(element => {
                var itemInteresProyecto = new Object();
                itemInteresProyecto.idInteresProyecto = element.id
                clienteInteresesProyecto.push(itemInteresProyecto)
            });
            data.clienteInteresesProyecto = clienteInteresesProyecto;
            const response = await axios.post('crearCliente', JSON.stringify(data), {
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            if (response.data.flag === 0) {
                toastr["error"]("Ocurrio un error del lado del servidor comuniquese con su area de sistemas");
            } else if (response.data.flag === 1) {
                toastr["success"]("Cliente Creado correctamente");
                location.reload();
            } else if (response.data.flag === 2) {
                toastr["error"]("Nro de contacto ya registrado");
            }
        }
    }
}

const actualizarCliente = async () => {
    if($("#formClientes").valid()) {
        var interesesProyecto = $('#selectInteresProyecto').select2('data');
        if (interesesProyecto.length < 1) {
            $("#selectInteresProyecto").addClass('is-invalid').removeClass('is-valid');
        } else {
            $("#selectInteresProyecto").addClass('is-valid').removeClass('is-invalid');
            var data = new Object();
            var clienteInteresesProyecto = [];
            data.clienteId = document.querySelector('#inputIdCliente').value;
            data.clienteMedioContacto = $("#selectMedioContacto").select2('data')[0].id;
            data.clienteCanalContacto = $('#selectCanalContacto').select2('data')[0].id;
            data.clienteTipoPersona = $('#selectTipoPersona').select2('data')[0].id;
            data.clienteTipoDocumento = $('#selectTipoDocumento').select2('data')[0].id;
            data.clientePromotor = $('#selectPromotor').select2('data')[0].id;
            data.clienteNroDocumento = document.querySelector('#inputNroDocumento').value;
            data.clienteRazonSocial = document.querySelector('#inputRazonSocial').value;
            data.clienteNroContacto = document.querySelector('#inputNroContacto').value;
            data.clienteNroContactoAuxiliar = document.querySelector('#inputNroAuxiliarContacto').value;
            data.clienteCorreoElectronico = document.querySelector('#inputEmail').value;
            data.clienteDepartamento = $('#selectDepartamento').select2('data')[0].id;
            data.clienteProvincia = $('#selectProvincia').select2('data')[0].id;
            data.clienteDistrito = $('#selectDistrito').select2('data')[0].id;
            data.clienteDireccion = document.querySelector("#inputDireccion").value;
            interesesProyecto.forEach(element => {
                var itemInteresProyecto = new Object();
                itemInteresProyecto.idInteresProyecto = element.id
                clienteInteresesProyecto.push(itemInteresProyecto)
            });
            data.clienteInteresesProyecto = clienteInteresesProyecto;
            const response = await axios.post('actualizarCliente', JSON.stringify(data), {
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            if (response.data.flag === 0) {
                toastr["error"]("Ocurrio un error del lado del servidor comuniquese con su area de sistemas");
            } else if (response.data.flag === 1) {
                toastr["success"]("Cliente Creado correctamente");
                location.reload();
            } else if (response.data.flag === 2) {
                toastr["error"]("Nro de contacto ya registrado");
            }
        }
    }
}