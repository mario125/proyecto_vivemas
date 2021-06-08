var calendarDiv = document.getElementById('calendar');
var calendar;

$(document).ready(() => {
    //var eventos = cargarEventos();
    var fechaInicioCalendario = new Date();
    var stringFechaInicio = fechaInicioCalendario.getFullYear() + "-" + String(fechaInicioCalendario.getMonth() + 1).padStart(2, '0') + "-" + String(fechaInicioCalendario.getDate()).padStart(2, '0');
    //cargarEventos(fechaInicioCalendario.getMonth() + 1);
    calendar = new FullCalendar.Calendar(calendarDiv, {        
        themeSystem: 'bootstrap',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay,listMonth'
        },
        initialDate: stringFechaInicio,
        locale: "es",
        navLinks: true,
        selectable: false,
        selectMirror: true,       
        editable: false,
        dayMaxEvents: true, // allow "more" link when too many events
        eventClick: function (info) {           
            console.log(info.event._def.extendedProps.evento_id);
        }
    });
    cargarEventos();
});

const cargarEventos = async (mes) => {
    const response = await axios.post('cargarEventos');
    if (response.data.flag === 0) {
        toastr["error"]("Ocurrio un error del lado del servidor comuniquese con su area de sistemas");
    } else if (response.data.flag === 1) {
        calendar.render();    
        calendar.addEventSource(response.data.data);        
    }
}
