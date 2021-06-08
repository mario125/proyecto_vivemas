var preguntas;

$(document).ready(() => {
    cargarEncuesta();
    //abrirTransaccionesDetalle(4)
});

document.getElementById('formEncuesta').addEventListener("submit", (e) => {
    e.preventDefault();
    document.querySelector("#inputCliente").value == '' ? $("#inputCliente").addClass('is-invalid').removeClass('is-valid') : $("#inputCliente").addClass('is-valid').removeClass('is-invalid');
    var flag = true;
    preguntas.forEach((item) => {
        if ($('input:radio[name="radiosPreguntas' + item.pregunta.pregunta_id + '"]:checked').val() == null) {
            console.log("entro");
            flag = false;
        }        
    });
    if (flag == false) {
        toastr["error"]("faltan llenar preguntas");
    }
    if (flag == true && $(".is-invalid").length == 0) {
        subirEncuesta();
    }    
});

const cargarEncuesta = async () => {
    const response = await axios.post('cargarEncuestaSatisfaccionCliente');
    //console.log(response.data);
    if (response.data.flag == 1) {
        var tablaEncuesta = '';
        preguntas = response.data.data;
        response.data.data.forEach((item) => {
            tablaEncuesta += '<tr>' +
                '<td class="text-justify">' + item.pregunta.pregunta_descripcion + '</td>';            
            item.respuesta.forEach((itemRespuesta) => {                
                tablaEncuesta += '<td><div class="text-center"><input type="radio" name="radiosPreguntas' + itemRespuesta.respuesta_pregunta_id+'" id="radioPregunta' + itemRespuesta.respuesta_id + '" value="' + itemRespuesta.respuesta_id + '"></div></td>';
            });
            tablaEncuesta += '</tr>';
        });
        $("#tbodyEncuesta").html('').append(tablaEncuesta);
    } else {
        toastr["error"]("Ocurrio un problema en el servidor comuniquese con el area de sistemas");
    }
}

const subirEncuesta = async () => {
    var data = new Object();
    data.clienteNombre = document.querySelector("#inputCliente").value.toUpperCase();
    data.observaciones = document.querySelector("#inputObservaciones").value.toUpperCase();
    var encuesta = [];
    preguntas.forEach((item) => {
        var pregunta = new Object();
        pregunta.preguntaId = item.pregunta.pregunta_id;
        pregunta.respuestaId = $('input:radio[name="radiosPreguntas' + item.pregunta.pregunta_id + '"]:checked').val();
        encuesta.push(pregunta);
    });
    data.encuesta = encuesta;
    console.log(data);
    const response = await axios.post('procesarEncuesta', JSON.stringify(data), {
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
        toastr["success"]("Encuesta llenada Correctamente");
           
    } else {
        toastr["error"]("Ocurrio un problema comuniquese con el area de sistemas");
    }
}