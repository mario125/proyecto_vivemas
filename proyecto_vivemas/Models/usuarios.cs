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
    
    public partial class usuarios
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public usuarios()
        {
            this.clientes = new HashSet<clientes>();
            this.cotizaciones = new HashSet<cotizaciones>();
            this.eventos = new HashSet<eventos>();
        }
    
        public long usuario_id { get; set; }
        public Nullable<long> usuario_rol_id { get; set; }
        public Nullable<long> usuario_datosusuario_id { get; set; }
        public string usuario_usuario { get; set; }
        public string usuario_pass { get; set; }
        public Nullable<System.DateTime> usuario_fechacreacion { get; set; }
        public Nullable<System.DateTime> usuario_fechamodificacion { get; set; }
        public Nullable<bool> usuario_estado { get; set; }
        public Nullable<long> usuario_idanterior { get; set; }
    
        public virtual datosusuarios datosusuarios { get; set; }
        public virtual roles roles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<clientes> clientes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cotizaciones> cotizaciones { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<eventos> eventos { get; set; }
    }
}