using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace proyecto_vivemas.ViewModels
{
    public class DataClientes
    {
        public long? clienteId { get; set; }
        public long? clienteMedioContacto { get; set; }
        public long? clienteCanalContacto { get; set; }
        public long? clienteTipoPersona { get; set; }
        public long? clienteTipoDocumento { get; set; }
        public long? clientePromotor { get; set; }
        public string clienteNroDocumento { get; set; }
        public string clienteRazonSocial { get; set; }
        public string clienteNroContacto { get; set; }
        public string clienteNroContactoAuxiliar { get; set; }
        public string clienteCorreoElectronico { get; set; }
        public long? clienteDepartamento { get; set; }
        public long? clienteProvincia { get; set; }
        public long? clienteDistrito { get; set; }
        public string clienteDireccion { get; set; }
        public List<DataInteresesProyecto> clienteInteresesProyecto { get; set; }
    }

    public class DataInteresesProyecto
    {
        public long? idInteresProyecto { get; set; }
    }
}