using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proyecto_vivemas.ViewModels
{
    public class DataTransacciones
    {
        public long transaccionId { get; set; }
        public long? transaccionMetodoPago { get; set; }
        public long? transaccionMoneda { get; set; }
        public long? transaccionBancoDestino { get; set; }
        public long? transaccionCuentaDestino { get; set; }
        public string transaccionNroSeparacion { get; set; }
        public string transaccionNroOperacion { get; set; }
        public string transaccionBancoOrigen { get; set; }
        public string transaccionFechaDeposito { get; set; }
        public decimal transaccionMonto { get; set; }
        public string transaccionObservaciones { get; set; }
    }

    public class DataTransaccionesProcesado
    {
        public List<DataTransacciones> listaTransacciones { get; set; }
    }
}