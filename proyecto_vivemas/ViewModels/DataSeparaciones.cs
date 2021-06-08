using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proyecto_vivemas.ViewModels
{
    public class DataSeparaciones
    {
        public DataClientes dataCliente { get; set; }
        public DataTransacciones dataTransaccion { get; set; }
        public long? separacionId { get; set; }
        public long? separacionProyectoId { get; set; }
        public long? separacionLoteId { get; set; }      
        public int separacionTiempo { get; set; }
    }
}