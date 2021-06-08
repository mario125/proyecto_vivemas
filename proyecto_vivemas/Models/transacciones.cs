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
    
    public partial class transacciones
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public transacciones()
        {
            this.pagos = new HashSet<pagos>();
            this.transaccionesseparacion = new HashSet<transaccionesseparacion>();
            this.documentosventa = new HashSet<documentosventa>();
            this.proformasuif = new HashSet<proformasuif>();
        }
    
        public long transaccion_id { get; set; }
        public Nullable<long> transaccion_empresa_id { get; set; }
        public Nullable<long> transaccion_tipotransaccion_id { get; set; }
        public Nullable<long> transaccion_tipomovimiento_id { get; set; }
        public Nullable<long> transaccion_estadotransaccion_id { get; set; }
        public Nullable<long> transaccion_tipometodopago_id { get; set; }
        public Nullable<long> transaccion_banco_id { get; set; }
        public Nullable<long> transaccion_cuentabanco_id { get; set; }
        public string transaccion_codigodocumento { get; set; }
        public string transaccion_nrooperacion { get; set; }
        public string transaccion_bancoorigen { get; set; }
        public Nullable<decimal> transaccion_monto { get; set; }
        public Nullable<System.DateTime> transaccion_fechacreacion { get; set; }
        public Nullable<long> transaccion_usuariocreacion { get; set; }
        public Nullable<System.DateTime> transaccion_fechamodificacion { get; set; }
        public Nullable<long> transaccion_usuariomodificacion { get; set; }
        public Nullable<long> transaccion_moneda_id { get; set; }
        public Nullable<long> transaccion_tipocambio_id { get; set; }
        public Nullable<System.DateTime> transaccion_tipocambio_fecha { get; set; }
        public Nullable<long> transaccion_codigotransaccion_id { get; set; }
        public Nullable<long> transaccion_codigoinicial_id { get; set; }
        public Nullable<System.DateTime> transaccion_fecha { get; set; }
        public Nullable<decimal> transaccion_tipocambio_venta { get; set; }
        public Nullable<long> transaccion_idproformacomodin { get; set; }
        public Nullable<System.DateTime> transaccion_fechadeposito { get; set; }
        public string transaccion_numeracion { get; set; }
        public Nullable<decimal> transaccion_montototaldescuento { get; set; }
        public Nullable<decimal> transaccion_montonetototal { get; set; }
        public Nullable<decimal> transaccion_porcentajedescuento { get; set; }
        public Nullable<bool> transaccion_estadoemision { get; set; }
        public Nullable<long> transaccion_evento_id { get; set; }
        public string transaccion_observaciones { get; set; }
    
        public virtual cuentasbanco cuentasbanco { get; set; }
        public virtual empresas empresas { get; set; }
        public virtual estadostransaccion estadostransaccion { get; set; }
        public virtual tiposmetodopago tiposmetodopago { get; set; }
        public virtual tiposmovimiento tiposmovimiento { get; set; }
        public virtual tipostransaccion tipostransaccion { get; set; }
        public virtual monedas monedas { get; set; }
        public virtual tiposcambio tiposcambio { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<pagos> pagos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transaccionesseparacion> transaccionesseparacion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<documentosventa> documentosventa { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<proformasuif> proformasuif { get; set; }
        public virtual eventos eventos { get; set; }
    }
}