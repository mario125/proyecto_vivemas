using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proyecto_vivemas.ViewModels
{
    public class DataEncuesta
    {
        public string clienteNombre { get; set; }
        public string observaciones { get; set; }
        public List<DataPreguntas> encuesta { get;set;}
    }

    public class DataPreguntas
    {
        public long preguntaId { get; set; }
        public long respuestaId { get; set; }
    }
}