//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace proyecto_vivemas.Models
{
    using System;
    
    public partial class sp_obtenerReportePagosClienteDetalle_Result
    {
        public string cuota_numeracion { get; set; }
        public string cuota_fechavencimiento { get; set; }
        public Nullable<decimal> cuota_monto { get; set; }
        public Nullable<decimal> transaccion_monto { get; set; }
        public Nullable<decimal> pago_montodescuento { get; set; }
        public string fecha_pago { get; set; }
        public string cuota_estado { get; set; }
    }
}
