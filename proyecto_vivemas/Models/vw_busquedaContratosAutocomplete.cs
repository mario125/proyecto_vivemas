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
    using System.Collections.Generic;
    
    public partial class vw_busquedaContratosAutocomplete
    {
        public long contrato_id { get; set; }
        public string contrato_numeracion { get; set; }
        public string cliente_nrodocumento { get; set; }
        public string cliente_razonsocial { get; set; }
        public string lote_nombre { get; set; }
        public Nullable<long> lote_proyecto_id { get; set; }
        public string proyecto_nombrecorto { get; set; }
        public long lote_id { get; set; }
    }
}
