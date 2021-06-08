using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proyecto_vivemas.ViewModels
{
    public class ClientesProforma
    {
        public int codigo { get; set; }
        public string proyecto { get; set; }
        public string lote { get; set; }
        public string tipDoc { get; set; }
        public string nroDocumento { get; set; }
        public string cliente { get; set; }       
        public string moneda { get; set; }
        public double montoTotal { get; set; }
        public double montoPendiente { get; set; }
        public int cuotasFaltantes { get; set; }
        public string fechaVencimiento { get; set; }
        public double montoCuota { get; set; }
    }

    public class ListClientesProforma
    {
        public List<ClientesProforma> clientesProformas { get; set; }
    }
}