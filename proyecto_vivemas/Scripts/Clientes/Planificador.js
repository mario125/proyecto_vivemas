var calendarDiv = document.getElementById('calendar');
var calendar;
$(document).ready(() => {
    var fechaInicioCalendario = new Date();
    var stringFechaInicio = fechaInicioCalendario.getFullYear() + "-" + String(fechaInicioCalendario.getMonth() + 1).padStart(2, '0') + "-" + String(fechaInicioCalendario.getDate()).padStart(2, '0');    
    cargarEventos(fechaInicioCalendario.getMonth()+1);
    calendar = new FullCalendar.Calendar(calendarDiv, {
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay,listMonth'
        },
        initialDate: stringFechaInicio,
        locale: "es",
        navLinks: true, 
        selectable: true,
        selectMirror: true,
        select: function (arg) {
            //var title = prompt('Event Title:');           
            document.querySelector("#inputFechaInicio").value = arg.start.getFullYear() + "-" + String(arg.start.getMonth() + 1).padStart(2, '0') + "-" + String(arg.start.getDate()).padStart(2, '0');
            document.querySelector("#inputFechaFin").value = arg.end.getFullYear() + "-" + String(arg.end.getMonth() + 1).padStart(2, '0') + "-" + String(arg.end.getDate()).padStart(2, '0');
            $("#modalEventos").modal({
                keyboard: false,
                backdrop: false
            });
            /*if ("prueba") {
                calendar.addEvent({
                    title: "prueba",
                    start: arg.start,
                    end: arg.end,
                    allDay: arg.allDay
                })
            }*/
            calendar.unselect()
        },
        eventClick: function (arg) {
            if (confirm('Are you sure you want to delete this event?')) {
                arg.event.remove()
            }
        },
        editable: true,
        dayMaxEvents: true, // allow "more" link when too many events
        events: [
            {
                title: 'All Day Event',
                start: '2020-09-01'
            },
            {
                title: 'Long Event',
                start: '2020-09-07',
                end: '2020-09-10'
            },
            {
                groupId: 999,
                title: 'Repeating Event',
                start: '2020-09-09T16:00:00'
            },
            {
                groupId: 999,
                title: 'Repeating Event',
                start: '2020-09-16T16:00:00'
            },
            {
                title: 'Conference',
                start: '2020-09-11',
                end: '2020-09-13'
            },
            {
                title: 'Meeting',
                start: '2020-09-12T10:30:00',
                end: '2020-09-12T12:30:00'
            },
            {
                title: 'Lunch',
                start: '2020-09-12T12:00:00'
            },
            {
                title: 'Meeting',
                start: '2020-09-12T14:30:00'
            },
            {
                title: 'Happy Hour',
                start: '2020-09-12T17:30:00'
            },
            {
                title: 'Dinner',
                start: '2020-09-12T20:00:00'
            },
            {
                title: 'Birthday Party',
                start: '2020-09-13T07:00:00'
            },
            {
                title: 'Click for Google',
                url: 'http://google.com/',
                start: '2020-09-28'
            }
        ]
    });
    calendar.render();
    $("#formEventos").validate({
        rules: {
            inputDescripcion: {
                required: true
            }            
        },
        messages: {
            inputDescripcion: {
                required: 'Campo Obligatorio'
            },
            
        }
    });
});

document.getElementById('buttonCrearEvento').addEventListener("click", (event) => {
    crearEvento();
});

const cargarEventos = async (mes) => {    
    const response = await axios.post('cargarEventos?mes=' + mes);    
    if (response.data.flag === 0) {
        toastr["error"]("Ocurrio un error del lado del servidor comuniquese con su area de sistemas");
    } else if (response.data.flag === 1) {
        //toastr["success"]("Evento creado correctamente");
        if (response.dataEventos.length > 0) {
            limpiarEventos();
            calendar.addEventSource(response.dataEventos);
        }
    }
}

const crearEvento = async () => {
    var data = new Object();    
    if ($("#formEventos").valid()) {
        data.eventoFechaInicio = document.querySelector("#inputFechaInicio").value;
        data.eventoFechaFin = document.querySelector("#inputFechaFin").value;
        data.eventoDescripcion = document.querySelector("#inputDescripcion").value;        
        const response = await axios.post('crearEvento', JSON.stringify(data), {
            headers: {
                'Content-Type': 'application/json'
            }
        });
        if (response.data.flag === 0) {
            toastr["error"]("Ocurrio un error del lado del servidor comuniquese con su area de sistemas");
        } else if (response.data.flag === 1) {
            toastr["success"]("Evento creado correctamente");
            var fecha = new Date(Date.parse(data.eventoFechaInicio));
            var fechaInicio = new Date(fecha.getFullYear(), fecha.getMonth + 1, fecha.getDate() + 1);
            fecha = new Date(Date.parse(data.eventoFechaFin));
            var fechaFin = new Date(fecha.getFullYear(), fecha.getMonth + 1, fecha.getDate() + 1);
            calendar.addEvent({
                title: data.eventoDescripcion,
                start: data.eventoFechaInicio,
                end: data.eventoFechaFin
            });
            calendar.unselect();
            //calendar.render();
            //location.reload();
        }
    }
}

const limpiarEventos = () => {
    calendar.getEventSources()[0].remove();
}