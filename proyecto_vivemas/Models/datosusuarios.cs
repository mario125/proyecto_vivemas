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
    
    public partial class datosusuarios
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public datosusuarios()
        {
            this.usuarios = new HashSet<usuarios>();
        }
    
        public long datosusuario_id { get; set; }
        public string datosusuario_razonsocial { get; set; }
        public string datosusuario_nrodocumento { get; set; }
        public string datosusuario_direccion { get; set; }
        public Nullable<System.DateTime> datosusuario_fechacreacion { get; set; }
        public Nullable<System.DateTime> datosusuario_fechamodificacion { get; set; }
        public Nullable<long> datosusuario_idanterior { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<usuarios> usuarios { get; set; }
    }
}
