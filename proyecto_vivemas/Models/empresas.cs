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
    
    public partial class empresas
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public empresas()
        {
            this.proyectos = new HashSet<proyectos>();
            this.roles = new HashSet<roles>();
            this.titulares = new HashSet<titulares>();
            this.clientes = new HashSet<clientes>();
            this.transacciones = new HashSet<transacciones>();
            this.configuraciones = new HashSet<configuraciones>();
            this.series = new HashSet<series>();
            this.documentosventa = new HashSet<documentosventa>();
            this.proformasuif = new HashSet<proformasuif>();
            this.notascredito = new HashSet<notascredito>();
            this.tiposencuesta = new HashSet<tiposencuesta>();
        }
    
        public long empresa_id { get; set; }
        public string empresa_nombre { get; set; }
        public string empresa_nombrecomercial { get; set; }
        public string empresa_documento { get; set; }
        public Nullable<long> empresa_tipodocumentoidentidad_id { get; set; }
        public string empresa_ubigeo { get; set; }
        public string empresa_direccion { get; set; }
        public string empresa_zona { get; set; }
        public string empresa_distrito { get; set; }
        public string empresa_provincia { get; set; }
        public string empresa_departamento { get; set; }
        public string empresa_codigopais { get; set; }
        public string empresa_nombrecarpeta { get; set; }
        public string empresa_correo { get; set; }
        public string empresa_nrocontacto { get; set; }
    
        public virtual tiposdocumentoidentidad tiposdocumentoidentidad { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<proyectos> proyectos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<roles> roles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<titulares> titulares { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<clientes> clientes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transacciones> transacciones { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<configuraciones> configuraciones { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<series> series { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<documentosventa> documentosventa { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<proformasuif> proformasuif { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<notascredito> notascredito { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tiposencuesta> tiposencuesta { get; set; }
    }
}
