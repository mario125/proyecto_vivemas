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
    
    public partial class proyectos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public proyectos()
        {
            this.lotes = new HashSet<lotes>();
            this.interesesclienteproyecto = new HashSet<interesesclienteproyecto>();
            this.cotizaciones = new HashSet<cotizaciones>();
        }
    
        public long proyecto_id { get; set; }
        public Nullable<long> proyecto_empresa_id { get; set; }
        public string proyecto_nombre { get; set; }
        public string proyecto_nombrecorto { get; set; }
        public string proyecto_imagemuestra { get; set; }
        public Nullable<System.DateTime> proyecto_fechacreacion { get; set; }
        public Nullable<long> proyecto_usuariocreacion { get; set; }
        public Nullable<System.DateTime> proyecto_fechamodificacion { get; set; }
        public Nullable<long> proyecto_usuariomodificacion { get; set; }
        public Nullable<bool> proyecto_estado { get; set; }
        public Nullable<long> proyecto_codigoanterior { get; set; }
        public Nullable<int> proyecto_totallotes { get; set; }
        public Nullable<decimal> proyecto_precioprom { get; set; }
        public Nullable<int> proyecto_zona_id { get; set; }
        public Nullable<System.DateTime> proyecto_fechaentrega { get; set; }
        public string proyecto_ubicontrato { get; set; }
        public string proyecto_abreviatura { get; set; }
        public Nullable<int> proyecto_anexo { get; set; }
        public string proyecto_moneda_id { get; set; }
    
        public virtual empresas empresas { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<lotes> lotes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<interesesclienteproyecto> interesesclienteproyecto { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cotizaciones> cotizaciones { get; set; }
    }
}