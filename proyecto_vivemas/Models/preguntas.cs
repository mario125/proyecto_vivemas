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
    
    public partial class preguntas
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public preguntas()
        {
            this.encuestas = new HashSet<encuestas>();
            this.respuestas = new HashSet<respuestas>();
        }
    
        public long pregunta_id { get; set; }
        public Nullable<long> pregunta_tipoencuesta_id { get; set; }
        public string pregunta_descripcion { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<encuestas> encuestas { get; set; }
        public virtual tiposencuesta tiposencuesta { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<respuestas> respuestas { get; set; }
    }
}
